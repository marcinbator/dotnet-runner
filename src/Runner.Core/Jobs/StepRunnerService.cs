using Microsoft.Extensions.Logging;
using Runner.Core.Containers;

namespace Runner.Core.Jobs;

public sealed partial class StepRunnerService(
  IContainerRuntimeService containerRuntime,
  ILogger<StepRunnerService> logger
) : IStepRunnerService
{
  public async Task RunAsync(
    Step step,
    Container container,
    string jobIdentifier,
    CancellationToken cancellationToken = default
  )
  {
    string stepIdentifier = step.Id ?? step.Name ?? step.Number.ToString();

    LogStepStarted(logger, jobIdentifier, stepIdentifier);

    CommandResult result = await containerRuntime.RunCommandAsync(container, step.Run, cancellationToken);

    if (logger.IsEnabled(LogLevel.Information))
    {
      foreach (string line in SplitLines(result.Stdout))
        LogStepStdout(logger, stepIdentifier, line);
    }
    if (logger.IsEnabled(LogLevel.Warning))
    {
      foreach (string line in SplitLines(result.Stderr))
        LogStepStderr(logger, stepIdentifier, line);
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

  [LoggerMessage(Level = LogLevel.Information, Message = "[Job {JobIdentifier}] Step {StepIdentifier} started.")]
  private static partial void LogStepStarted(ILogger logger, string jobIdentifier, string? stepIdentifier);

  [LoggerMessage(Level = LogLevel.Information, Message = "[Step {StepIdentifier}] {StepOutput}")]
  private static partial void LogStepStdout(ILogger logger, string? stepIdentifier, string stepOutput);

  [LoggerMessage(Level = LogLevel.Warning, Message = "[Step {StepIdentifier}] {StepOutput}")]
  private static partial void LogStepStderr(ILogger logger, string? stepIdentifier, string stepOutput);
}
