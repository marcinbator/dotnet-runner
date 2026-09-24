using Microsoft.Extensions.Logging;

namespace Runner.Core.Jobs;

public sealed partial class StepRunnerService(ILogger<StepRunnerService> logger) : IStepRunnerService
{
  public async Task RunAsync(Step step, CancellationToken cancellationToken = default)
  {
    LogStepStarted(logger, step.Name, step.Id);
    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
  }

  //

  [LoggerMessage(Level = LogLevel.Information, Message = "Step {StepName} ({StepId}) started.")]
  private static partial void LogStepStarted(ILogger logger, string? stepName, string? stepId);
}
