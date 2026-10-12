using Runner.Core.Containers;

namespace Runner.Core.Jobs;

public interface IStepRunnerService
{
  Task RunAsync(Step step, Container container, string jobIdentifier, CancellationToken cancellationToken = default);
}
