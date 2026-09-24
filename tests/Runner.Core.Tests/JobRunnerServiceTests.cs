using Microsoft.Extensions.Logging.Abstractions;
using Runner.Core.Containers;
using Runner.Core.Jobs;

namespace Runner.Core.Tests;

public sealed class JobRunnerServiceTests
{
  private readonly List<string> _events = [];

  [Fact]
  public async Task RunAsync_RunsSteps_InsideContainer_InOrder()
  {
    JobRunnerService runner = CreateRunner();

    await runner.RunAsync(CreateJob("s1", "s2"));

    Assert.Equal(["run alpine", "s1", "s2", "stop ct1"], _events);
  }

  [Fact]
  public async Task RunAsync_StopsContainer_WhenStepThrows()
  {
    JobRunnerService runner = CreateRunner(failingStepId: "s1");

    await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync(CreateJob("s1", "s2")));

    Assert.Equal(["run alpine", "s1", "stop ct1"], _events);
  }

  //

  private JobRunnerService CreateRunner(string? failingStepId = null) =>
    new(
      new RecordingStepRunner(_events, failingStepId),
      new RecordingContainerRuntime(_events),
      NullLogger<JobRunnerService>.Instance
    );

  private static Job CreateJob(params string[] stepIds) =>
    new()
    {
      Name = "test",
      Image = "alpine",
      Steps = [.. stepIds.Select(id => new Step { Id = id })],
    };

  private sealed class RecordingStepRunner(List<string> events, string? failingStepId) : IStepRunnerService
  {
    public Task RunAsync(Step step, CancellationToken cancellationToken = default)
    {
      events.Add(step.Id!);
      return step.Id == failingStepId ? Task.FromException(new InvalidOperationException()) : Task.CompletedTask;
    }
  }

  private sealed class RecordingContainerRuntime(List<string> events) : IContainerRuntimeService
  {
    public Task<Container> RunAsync(string image, CancellationToken cancellationToken = default)
    {
      events.Add($"run {image}");
      return Task.FromResult(new Container("ct1", "ct1"));
    }

    public Task<bool> StopAsync(Container container, CancellationToken cancellationToken = default)
    {
      events.Add($"stop {container.Id}");
      return Task.FromResult(true);
    }
  }
}
