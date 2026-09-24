using Microsoft.Extensions.Logging.Abstractions;
using Runner.Core.Containers;
using Runner.Core.Jobs;
using Xunit;

namespace Runner.Core.Tests;

public class JobRunnerTests
{
  [Fact]
  public async Task Run_Throws_WhenJobNameIsEmpty()
  {
    var runner = new JobRunnerService(new RecordingStepRunner([]), new RecordingContainerRuntime([]), NullLogger<JobRunnerService>.Instance);
    var job = new Job { Name = "" };

    await Assert.ThrowsAsync<ArgumentException>(() => runner.Run(job));
  }

  [Fact]
  public async Task Run_RunsSteps_InsideContainer_InOrder()
  {
    List<string> events = [];
    var runner = new JobRunnerService(new RecordingStepRunner(events), new RecordingContainerRuntime(events), NullLogger<JobRunnerService>.Instance);
    var job = new Job
    {
      Name = "test",
      Steps = [new Step { Id = "s1" }, new Step { Id = "s2" }]
    };

    await runner.Run(job);

    Assert.Equal(["run", "s1", "s2", "stop ct1"], events);
  }

  private class RecordingStepRunner(List<string> events) : IStepRunnerService
  {
    public void Run(Step step) => events.Add(step.Id!);
  }

  private class RecordingContainerRuntime(List<string> events) : IContainerRuntimeService
  {
    public Task<Container> RunAsync(string image, CancellationToken cancellationToken = default)
    {
      events.Add("run");
      return Task.FromResult(new Container { Id = "ct1", Name = "ct1" });
    }

    public Task<bool> StopAsync(Container container, CancellationToken cancellationToken = default)
    {
      events.Add($"stop {container.Id}");
      return Task.FromResult(true);
    }
  }
}
