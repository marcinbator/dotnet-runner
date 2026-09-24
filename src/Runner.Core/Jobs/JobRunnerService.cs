using Microsoft.Extensions.Logging;
using Runner.Core.Containers;

namespace Runner.Core.Jobs;

public partial class JobRunnerService(IStepRunnerService runner, IContainerRuntimeService containerRuntime, ILogger<JobRunnerService> logger) : IJobRunnerService
{
  public async Task Run(Job job)
  {
    if (string.IsNullOrEmpty(job.Name)) throw new ArgumentException("Job name cannot be empty.");

    LogJobStarted(logger, job.Name);

    Container ct = await containerRuntime.RunAsync("hello-world:latest");

    foreach (Step step in job.Steps)
    {
      runner.Run(step);
    }

    await containerRuntime.StopAsync(ct);

    LogJobEnded(logger, job.Name);
  }

  //

  [LoggerMessage(Level = LogLevel.Information, Message = "Job {JobName} started.")]
  private static partial void LogJobStarted(ILogger logger, string jobName);

  [LoggerMessage(Level = LogLevel.Information, Message = "Job {JobName} ended.")]
  private static partial void LogJobEnded(ILogger logger, string jobName);
}
