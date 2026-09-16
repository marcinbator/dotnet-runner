using Runner.Core.Jobs;
using Xunit;

namespace Runner.Core.Tests;

public class JobRunnerTests
{
  [Fact]
  public void Run_Throws_WhenJobNameIsEmpty()
  {
    var runner = new JobRunner();
    var job = new Job { Name = "" };

    Assert.Throws<ArgumentException>(() => runner.Run(job));
  }
}
