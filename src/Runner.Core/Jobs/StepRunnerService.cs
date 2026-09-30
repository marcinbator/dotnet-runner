using Microsoft.Extensions.Logging;
using Runner.Core.Containers;

namespace Runner.Core.Jobs;

public sealed partial class StepRunnerService(
  IContainerRuntimeService containerRuntime,
  ILogger<StepRunnerService> logger
) : IStepRunnerService
{
  public async Task RunAsync(Step step, Container container, CancellationToken cancellationToken = default)
  {
    LogStepStarted(logger, step.Name, step.Id);

    CommandResult result = await containerRuntime.RunCommandAsync(container, step.Run, cancellationToken);

    if (logger.IsEnabled(LogLevel.Information) && result.Stdout.Length > 0)
      LogStepStdout(logger, step.Id, result.Stdout.TrimEnd());
    if (logger.IsEnabled(LogLevel.Warning) && result.Stderr.Length > 0)
      LogStepStderr(logger, step.Id, result.Stderr.TrimEnd());

    if (result.ExitCode != 0)
      throw new StepFailedException(step.Id, result.ExitCode);
  }

  //

  [LoggerMessage(Level = LogLevel.Information, Message = "Step {StepName} ({StepId}) started.")]
  private static partial void LogStepStarted(ILogger logger, string? stepName, string? stepId);

  [LoggerMessage(Level = LogLevel.Information, Message = "Step {StepId} stdout: {StepOutput}")]
  private static partial void LogStepStdout(ILogger logger, string? stepId, string stepOutput);

  [LoggerMessage(Level = LogLevel.Warning, Message = "Step {StepId} stderr: {StepOutput}")]
  private static partial void LogStepStderr(ILogger logger, string? stepId, string stepOutput);
}
