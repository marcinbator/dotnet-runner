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

    if (logger.IsEnabled(LogLevel.Information))
    {
      foreach (string line in SplitLines(result.Stdout))
        LogStepStdout(logger, step.Id, line);
    }
    if (logger.IsEnabled(LogLevel.Warning))
    {
      foreach (string line in SplitLines(result.Stderr))
        LogStepStderr(logger, step.Id, line);
    }

    if (result.ExitCode != 0)
      throw new StepFailedException(step.Id, result.ExitCode);
  }

  //

  private static string[] SplitLines(string output)
  {
    string trimmed = output.TrimEnd();
    return trimmed.Length == 0 ? [] : trimmed.Split('\n', StringSplitOptions.TrimEntries);
  }

  //

  [LoggerMessage(Level = LogLevel.Information, Message = "Step {StepName} ({StepId}) started.")]
  private static partial void LogStepStarted(ILogger logger, string? stepName, string? stepId);

  [LoggerMessage(Level = LogLevel.Information, Message = "[{StepId}] {StepOutput}")]
  private static partial void LogStepStdout(ILogger logger, string? stepId, string stepOutput);

  [LoggerMessage(Level = LogLevel.Warning, Message = "[{StepId}] {StepOutput}")]
  private static partial void LogStepStderr(ILogger logger, string? stepId, string stepOutput);
}
