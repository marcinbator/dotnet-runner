using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Runner.Core.Containers;

public sealed partial class ContainerRuntimeService(
  IOptions<RunnerOptions> options,
  ILogger<ContainerRuntimeService> logger
) : IContainerRuntimeService, IDisposable
{
  private readonly DockerClient _client = new DockerClientBuilder()
    .WithEndpoint(new Uri($"unix://{options.Value.DockerSocket}"))
    .Build();

  public async Task<Container> RunAsync(string image, CancellationToken cancellationToken = default)
  {
    await EnsureImageAsync(image, cancellationToken);

    string name = $"runner-{Guid.NewGuid():N}";
    CreateContainerResponse created = await _client.Containers.CreateContainerAsync(
      new CreateContainerParameters
      {
        Image = image,
        Name = name,
        Entrypoint = ["tail", "-f", "/dev/null"],
        HostConfig = new HostConfig { AutoRemove = true, Init = true },
      },
      cancellationToken
    );

    try
    {
      await _client.Containers.StartContainerAsync(created.ID, new ContainerStartParameters(), cancellationToken);
    }
    catch
    {
      await _client.Containers.RemoveContainerAsync(
        created.ID,
        new ContainerRemoveParameters { Force = true },
        CancellationToken.None
      );
      throw;
    }

    LogContainerStarted(logger, name);

    return new Container(created.ID, name);
  }

  public async Task<bool> StopAsync(Container container, CancellationToken cancellationToken = default)
  {
    try
    {
      await _client.Containers.StopContainerAsync(container.Id, new ContainerStopParameters(), cancellationToken);
    }
    catch (DockerContainerNotFoundException)
    {
      LogContainerNotFound(logger, container.Name);
      return false;
    }

    LogContainerStopped(logger, container.Name);

    return true;
  }

  public void Dispose() => _client.Dispose();

  //

  private async Task EnsureImageAsync(string image, CancellationToken cancellationToken)
  {
    try
    {
      await _client.Images.InspectImageAsync(image, cancellationToken);
    }
    catch (DockerImageNotFoundException)
    {
      bool hasTag = image.Contains('@') || image[(image.LastIndexOf('/') + 1)..].Contains(':');
      await _client.Images.CreateImageAsync(
        new ImagesCreateParameters { FromImage = image, Tag = hasTag ? null : "latest" },
        null,
        new Progress<JSONMessage>(),
        cancellationToken
      );
    }
  }

  //

  [LoggerMessage(Level = LogLevel.Information, Message = "Container {ContainerName} started.")]
  private static partial void LogContainerStarted(ILogger logger, string containerName);

  [LoggerMessage(Level = LogLevel.Information, Message = "Container {ContainerName} stopped.")]
  private static partial void LogContainerStopped(ILogger logger, string containerName);

  [LoggerMessage(Level = LogLevel.Warning, Message = "Container {ContainerName} was already gone when stopping.")]
  private static partial void LogContainerNotFound(ILogger logger, string containerName);
}
