using Docker.DotNet;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Runner.Core.Containers;

namespace Runner.Core.Tests;

public sealed class ContainerRuntimeServiceTests : IDisposable
{
  private const string Image = "alpine:3.20";

  private readonly ContainerRuntimeService _runtime = Create(new RunnerOptions());

  [DockerFact]
  public async Task RunAsync_ReturnsContainer_WithIdAndName()
  {
    Container container = await _runtime.RunAsync(Image);

    try
    {
      Assert.False(string.IsNullOrEmpty(container.Id));
      Assert.StartsWith("runner-", container.Name);
    }
    finally
    {
      await _runtime.StopAsync(container);
    }
  }

  [DockerFact]
  public async Task StopAsync_ReturnsTrue_WhenContainerIsRunning()
  {
    Container container = await _runtime.RunAsync(Image);

    Assert.True(await _runtime.StopAsync(container));
  }

  [DockerFact]
  public async Task StopAsync_DoesNotThrow_WhenContainerWasAlreadyStopped()
  {
    Container container = await _runtime.RunAsync(Image);
    await _runtime.StopAsync(container);

    Exception? exception = await Record.ExceptionAsync(() => _runtime.StopAsync(container));

    Assert.Null(exception);
  }

  [DockerFact]
  public async Task StopAsync_ReturnsFalse_WhenContainerDoesNotExist()
  {
    var container = new Container(Guid.NewGuid().ToString("N"), "missing");

    Assert.False(await _runtime.StopAsync(container));
  }

  [DockerFact]
  public async Task RunAsync_Throws_WhenImageCannotBePulled()
  {
    await Assert.ThrowsAnyAsync<DockerApiException>(() =>
      _runtime.RunAsync($"runner-tests/missing-{Guid.NewGuid():N}:1")
    );
  }

  [Fact]
  public async Task RunAsync_Throws_WhenDockerSocketIsUnreachable()
  {
    string socket = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.sock");
    using ContainerRuntimeService unreachable = Create(new RunnerOptions { DockerSocket = socket });

    await Assert.ThrowsAsync<HttpRequestException>(() => unreachable.RunAsync(Image));
  }

  public void Dispose() => _runtime.Dispose();

  //

  private static ContainerRuntimeService Create(RunnerOptions options) =>
    new(Options.Create(options), NullLogger<ContainerRuntimeService>.Instance);
}
