using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Runner.Core.Containers;
using Xunit;

namespace Runner.Core.Tests;

public sealed class ContainerRuntimeServiceTests : IDisposable
{
  private const string Image = "alpine:3.20";

  private readonly ContainerRuntimeService runtime = Create(new RunnerOptions());

  [DockerFact]
  public async Task RunAsync_ReturnsContainer_WithIdAndName()
  {
    Container container = await runtime.RunAsync(Image);

    try
    {
      Assert.False(string.IsNullOrEmpty(container.Id));
      Assert.StartsWith("runner-", container.Name);
    }
    finally
    {
      await runtime.StopAsync(container);
    }
  }

  [DockerFact]
  public async Task StopAsync_ReturnsTrue_WhenContainerIsRunning()
  {
    Container container = await runtime.RunAsync(Image);

    Assert.True(await runtime.StopAsync(container));
  }

  [DockerFact]
  public async Task StopAsync_DoesNotThrow_WhenContainerWasAlreadyStopped()
  {
    Container container = await runtime.RunAsync(Image);
    await runtime.StopAsync(container);

    Exception? exception = await Record.ExceptionAsync(() => runtime.StopAsync(container));

    Assert.Null(exception);
  }

  [DockerFact]
  public async Task StopAsync_ReturnsFalse_WhenContainerDoesNotExist()
  {
    var container = new Container { Id = Guid.NewGuid().ToString("N"), Name = "missing" };

    Assert.False(await runtime.StopAsync(container));
  }

  [DockerFact]
  public async Task RunAsync_Throws_WhenImageCannotBePulled()
  {
    await Assert.ThrowsAsync<HttpRequestException>(() => runtime.RunAsync($"runner-tests/missing-{Guid.NewGuid():N}:1"));
  }

  [Fact]
  public async Task RunAsync_Throws_WhenDockerSocketIsUnreachable()
  {
    using ContainerRuntimeService unreachable = Create(new RunnerOptions { DockerSocket = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.sock") });

    await Assert.ThrowsAsync<HttpRequestException>(() => unreachable.RunAsync(Image));
  }

  public void Dispose() => runtime.Dispose();

  private static ContainerRuntimeService Create(RunnerOptions options) => new(Options.Create(options), NullLogger<ContainerRuntimeService>.Instance);
}
