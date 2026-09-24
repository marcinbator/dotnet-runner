namespace Runner.Core.Jobs;

public interface IJobRunnerService
{
  public Task Run(Job job);
}
