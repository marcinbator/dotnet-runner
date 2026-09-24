namespace Runner.Core.Jobs;

public interface IJobRunnerService
{
  Task RunAsync(Job job, CancellationToken cancellationToken = default);
}
