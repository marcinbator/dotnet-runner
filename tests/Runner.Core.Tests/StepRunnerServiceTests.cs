using Microsoft.Extensions.Logging.Abstractions;
using Runner.Core.Containers;
using Runner.Core.Jobs;

namespace Runner.Core.Tests;

public sealed class StepRunnerServiceTests
{
  private readonly List<string> _events = [];
  private readonly RecordingContainerRuntime _runtime;
  private readonly StepRunnerService _runner;

  public StepRunnerServiceTests()
  {
    _runtime = new RecordingContainerRuntime(_events);
    _runner = new StepRunnerService(_runtime, NullLogger<StepRunnerService>.Instance);
  }

  [Fact]
  public async Task RunAsync_RunsCommand_InContainer()
  {
    await _runner.RunAsync(new Step { Id = "s1", Run = "echo hi" }, new Container("ct1", "ct1"));

    Assert.Equal(["exec echo hi in ct1"], _events);
  }

  [Fact]
  public async Task RunAsync_Throws_WhenCommandFails()
  {
    _runtime.CommandResult = new CommandResult(3, "", "boom");

    StepFailedException exception = await Assert.ThrowsAsync<StepFailedException>(() =>
      _runner.RunAsync(new Step { Id = "s1", Run = "exit 3" }, new Container("ct1", "ct1"))
    );

    Assert.Equal("s1", exception.StepId);
    Assert.Equal(3, exception.ExitCode);
  }
}
