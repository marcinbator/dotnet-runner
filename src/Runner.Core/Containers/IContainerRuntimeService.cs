namespace Runner.Core.Containers;

public interface IContainerRuntimeService
{
  Task<Container> RunAsync(string image, CancellationToken cancellationToken = default);
  Task<bool> StopAsync(Container container, CancellationToken cancellationToken = default);
}
