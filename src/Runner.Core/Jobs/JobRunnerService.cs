using Microsoft.Extensions.Logging;
using Runner.Core.Containers;

namespace Runner.Core.Jobs;

public sealed partial class JobRunnerService(
  IStepRunnerService stepRunner,
  IContainerRuntimeService containerRuntime,
  ILogger<JobRunnerService> logger
) : IJobRunnerService
{
  public async Task RunAsync(Job job, CancellationToken cancellationToken = default)
  {
    LogJobStarted(logger, job.Name);

    Container container = await containerRuntime.RunAsync(job.Image, cancellationToken);

    try
    {
      foreach (Step step in job.Steps)
      {
        await stepRunner.RunAsync(step, container, cancellationToken);
      }
    }
    finally
    {
      await containerRuntime.StopAsync(container, CancellationToken.None);
    }

    LogJobEnded(logger, job.Name);
  }

  //

  [LoggerMessage(Level = LogLevel.Information, Message = "Job {JobName} started.")]
  private static partial void LogJobStarted(ILogger logger, string jobName);

  [LoggerMessage(Level = LogLevel.Information, Message = "Job {JobName} ended.")]
  private static partial void LogJobEnded(ILogger logger, string jobName);
}
