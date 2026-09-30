namespace Runner.Core.Jobs;

public sealed class StepFailedException(string? stepId, long exitCode)
  : Exception($"Step {stepId} failed with exit code {exitCode}.")
{
  public string? StepId { get; } = stepId;
  public long ExitCode { get; } = exitCode;
}
