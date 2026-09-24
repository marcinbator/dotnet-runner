namespace Runner.Core.Containers;

public interface IContainerRuntimeService
{
  public Task<Container> RunAsync(string image, CancellationToken cancellationToken = default);
  public Task<bool> StopAsync(Container container, CancellationToken cancellationToken = default);
}
