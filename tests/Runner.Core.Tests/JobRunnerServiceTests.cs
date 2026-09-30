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

    Assert.Equal(["run alpine", "s1 in ct1", "s2 in ct1", "stop ct1"], _events);
  }

  [Fact]
  public async Task RunAsync_StopsContainer_WhenStepThrows()
  {
    JobRunnerService runner = CreateRunner(failingStepId: "s1");

    await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync(CreateJob("s1", "s2")));

    Assert.Equal(["run alpine", "s1 in ct1", "stop ct1"], _events);
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
      Steps = [.. stepIds.Select(id => new Step { Id = id, Run = "true" })],
    };

  private sealed class RecordingStepRunner(List<string> events, string? failingStepId) : IStepRunnerService
  {
    public Task RunAsync(Step step, Container container, CancellationToken cancellationToken = default)
    {
      events.Add($"{step.Id} in {container.Id}");
      return step.Id == failingStepId ? Task.FromException(new InvalidOperationException()) : Task.CompletedTask;
    }
  }
}
