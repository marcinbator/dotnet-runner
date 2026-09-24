namespace Runner.Core.Jobs;

public interface IStepRunnerService
{
  Task RunAsync(Step step, CancellationToken cancellationToken = default);
}
