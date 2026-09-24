using Microsoft.Extensions.Logging;

namespace Runner.Core.Jobs;

public partial class StepRunnerService(ILogger<StepRunnerService> logger) : IStepRunnerService
{
  public void Run(Step step)
  {
    LogStepStarted(logger, step.Name, step.Id);
    Thread.Sleep(1000);
  }

  [LoggerMessage(Level = LogLevel.Information, Message = "-> Step {StepName} (ID: {StepId})")]
  private static partial void LogStepStarted(ILogger logger, string? stepName, string? stepId);
}
