using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Runner.Core.Containers;

public sealed partial class ContainerRuntimeService(IOptions<RunnerOptions> options, ILogger<ContainerRuntimeService> logger) : IContainerRuntimeService, IDisposable
{
  private readonly HttpClient client = new(new SocketsHttpHandler
  {
    ConnectCallback = async (_, cancellationToken) =>
    {
      var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
      await socket.ConnectAsync(new UnixDomainSocketEndPoint(options.Value.DockerSocket), cancellationToken);
      return new NetworkStream(socket, ownsSocket: true);
    },
  })
  {
    BaseAddress = new Uri("http://docker/"),
  };

  public async Task<Container> RunAsync(string image, CancellationToken cancellationToken = default)
  {
    string name = $"runner-{Guid.NewGuid():N}";

    using HttpResponseMessage created = await CreateAsync(image, name, cancellationToken);
    await ThrowIfFailedAsync(created, cancellationToken);
    DockerMessage? container = await created.Content.ReadFromJsonAsync<DockerMessage>(cancellationToken);

    using HttpResponseMessage started = await client.PostAsync($"containers/{name}/start", null, cancellationToken);
    await ThrowIfFailedAsync(started, cancellationToken);

    LogContainerStarted(logger, name);

    return new Container { Id = container!.Id!, Name = name };
  }

  public async Task<bool> StopAsync(Container container, CancellationToken cancellationToken = default)
  {
    using HttpResponseMessage response = await client.PostAsync($"containers/{container.Id}/stop", null, cancellationToken);

    if (response.StatusCode == HttpStatusCode.NotFound) return false;
    if (response.StatusCode != HttpStatusCode.NotModified) await ThrowIfFailedAsync(response, cancellationToken);

    LogContainerStopped(logger, container.Name);

    return true;
  }

  public void Dispose() => client.Dispose();

  //

  private async Task<HttpResponseMessage> CreateAsync(string image, string name, CancellationToken cancellationToken)
  {
    string uri = $"containers/create?name={name}";
    var body = new { Image = image, Entrypoint = new[] { "tail", "-f", "/dev/null" }, HostConfig = new { AutoRemove = true, Init = true } };

    HttpResponseMessage response = await client.PostAsJsonAsync(uri, body, cancellationToken);
    if (response.StatusCode != HttpStatusCode.NotFound) return response;

    response.Dispose();
    await PullAsync(image, cancellationToken);
    return await client.PostAsJsonAsync(uri, body, cancellationToken);
  }

  private async Task PullAsync(string image, CancellationToken cancellationToken)
  {
    bool hasTag = image.Contains('@') || image[(image.LastIndexOf('/') + 1)..].Contains(':');
    string uri = $"images/create?fromImage={Uri.EscapeDataString(image)}" + (hasTag ? "" : "&tag=latest");

    using HttpResponseMessage response = await client.PostAsync(uri, null, cancellationToken);
    await ThrowIfFailedAsync(response, cancellationToken);

    string lastLine = (await response.Content.ReadAsStringAsync(cancellationToken)).TrimEnd().Split('\n')[^1];
    if (JsonSerializer.Deserialize<DockerMessage>(lastLine, JsonSerializerOptions.Web)?.Error is { } error)
      throw new InvalidOperationException($"Failed to pull image '{image}': {error}");
  }

  private static async Task ThrowIfFailedAsync(HttpResponseMessage response, CancellationToken cancellationToken)
  {
    if (response.IsSuccessStatusCode) return;

    DockerMessage? error = await response.Content.ReadFromJsonAsync<DockerMessage>(cancellationToken);
    throw new HttpRequestException($"Docker API error ({(int)response.StatusCode}): {error?.Message}", null, response.StatusCode);
  }

  //

  [LoggerMessage(Level = LogLevel.Information, Message = "Container {ctName} started.")]
  private static partial void LogContainerStarted(ILogger logger, string ctName);

  [LoggerMessage(Level = LogLevel.Information, Message = "Container {ctName} stopped.")]
  private static partial void LogContainerStopped(ILogger logger, string ctName);

  //

  private sealed record DockerMessage(string? Id, string? Message, string? Error);
}
