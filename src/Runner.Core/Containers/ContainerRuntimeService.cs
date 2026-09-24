using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Runner.Core.Containers;

public sealed partial class ContainerRuntimeService(
  IOptions<RunnerOptions> options,
  ILogger<ContainerRuntimeService> logger
) : IContainerRuntimeService, IDisposable
{
  private readonly HttpClient _client = new(
    new SocketsHttpHandler
    {
      ConnectCallback = async (_, cancellationToken) =>
      {
        var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        await socket.ConnectAsync(new UnixDomainSocketEndPoint(options.Value.DockerSocket), cancellationToken);
        return new NetworkStream(socket, ownsSocket: true);
      },
    }
  )
  {
    BaseAddress = new Uri("http://docker/"),
  };

  public async Task<Container> RunAsync(string image, CancellationToken cancellationToken = default)
  {
    string name = $"runner-{Guid.NewGuid():N}";

    using HttpResponseMessage created = await CreateAsync(image, name, cancellationToken);
    await ThrowIfFailedAsync(created, cancellationToken);
    DockerMessage? message = await created.Content.ReadFromJsonAsync<DockerMessage>(cancellationToken);
    string id = message?.Id ?? throw new ContainerRuntimeException($"Docker returned no id for container '{name}'.");

    using HttpResponseMessage started = await _client.PostAsync($"containers/{id}/start", null, cancellationToken);
    if (!started.IsSuccessStatusCode)
    {
      await RemoveAsync(id);
      await ThrowIfFailedAsync(started, cancellationToken);
    }

    LogContainerStarted(logger, name);

    return new Container(id, name);
  }

  public async Task<bool> StopAsync(Container container, CancellationToken cancellationToken = default)
  {
    using HttpResponseMessage response = await _client.PostAsync(
      $"containers/{container.Id}/stop",
      null,
      cancellationToken
    );

    if (response.StatusCode == HttpStatusCode.NotFound)
    {
      LogContainerNotFound(logger, container.Name);
      return false;
    }

    if (response.StatusCode != HttpStatusCode.NotModified)
      await ThrowIfFailedAsync(response, cancellationToken);

    LogContainerStopped(logger, container.Name);

    return true;
  }

  public void Dispose() => _client.Dispose();

  //

  private async Task<HttpResponseMessage> CreateAsync(string image, string name, CancellationToken cancellationToken)
  {
    string uri = $"containers/create?name={name}";
    var body = new
    {
      Image = image,
      Entrypoint = new[] { "tail", "-f", "/dev/null" },
      HostConfig = new { AutoRemove = true, Init = true },
    };

    HttpResponseMessage response = await _client.PostAsJsonAsync(uri, body, cancellationToken);
    if (response.StatusCode != HttpStatusCode.NotFound)
      return response;

    response.Dispose();
    await PullAsync(image, cancellationToken);
    return await _client.PostAsJsonAsync(uri, body, cancellationToken);
  }

  private async Task PullAsync(string image, CancellationToken cancellationToken)
  {
    bool hasTag = image.Contains('@') || image[(image.LastIndexOf('/') + 1)..].Contains(':');
    string uri = $"images/create?fromImage={Uri.EscapeDataString(image)}" + (hasTag ? "" : "&tag=latest");

    using HttpResponseMessage response = await _client.PostAsync(uri, null, cancellationToken);
    await ThrowIfFailedAsync(response, cancellationToken);

    string lastLine = (await response.Content.ReadAsStringAsync(cancellationToken)).TrimEnd().Split('\n')[^1];
    if (ParseMessage(lastLine)?.Error is { } error)
      throw new ContainerRuntimeException($"Failed to pull image '{image}': {error}");
  }

  private async Task RemoveAsync(string id)
  {
    using HttpResponseMessage _ = await _client.DeleteAsync($"containers/{id}?force=true");
  }

  private static async Task ThrowIfFailedAsync(HttpResponseMessage response, CancellationToken cancellationToken)
  {
    if (response.IsSuccessStatusCode)
      return;

    string body = await response.Content.ReadAsStringAsync(cancellationToken);
    string reason = ParseMessage(body)?.Message ?? body;
    throw new ContainerRuntimeException(
      $"Docker API error ({(int)response.StatusCode}): {reason}",
      response.StatusCode
    );
  }

  private static DockerMessage? ParseMessage(string json)
  {
    try
    {
      return JsonSerializer.Deserialize<DockerMessage>(json, JsonSerializerOptions.Web);
    }
    catch (JsonException)
    {
      return null;
    }
  }

  //

  [LoggerMessage(Level = LogLevel.Information, Message = "Container {ContainerName} started.")]
  private static partial void LogContainerStarted(ILogger logger, string containerName);

  [LoggerMessage(Level = LogLevel.Information, Message = "Container {ContainerName} stopped.")]
  private static partial void LogContainerStopped(ILogger logger, string containerName);

  [LoggerMessage(Level = LogLevel.Warning, Message = "Container {ContainerName} was already gone when stopping.")]
  private static partial void LogContainerNotFound(ILogger logger, string containerName);

  //

  private sealed record DockerMessage(string? Id, string? Message, string? Error);
}
