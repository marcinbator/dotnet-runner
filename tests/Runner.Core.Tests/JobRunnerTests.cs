using Runner.Core.Jobs;
using Xunit;

namespace Runner.Core.Tests;

public class JobRunnerTests
{
  [Fact]
  public void Run_Throws_WhenJobNameIsEmpty()
  {
    var runner = new JobRunner(new StepRunner());
    var job = new Job { Name = "" };

    Assert.Throws<ArgumentException>(() => runner.Run(job));
  }

  [Fact]
  public void Run_RunsEachStep_InOrder()
  {
    var stepRunner = new RecordingStepRunner();
    var runner = new JobRunner(stepRunner);
    var job = new Job
    {
      Name = "test",
      Steps = [new Step { Id = "s1" }, new Step { Id = "s2" }]
    };

    runner.Run(job);

    Assert.Equal(["s1", "s2"], stepRunner.RunIds);
  }

  private class RecordingStepRunner : IStepRunner
  {
    public List<string?> RunIds { get; } = [];

    public void Run(Step step) => RunIds.Add(step.Id);
  }
}
