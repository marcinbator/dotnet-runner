namespace Runner.Core.Containers;

public interface IContainerRuntimeService
{
  Task<Container> RunAsync(string image, CancellationToken cancellationToken = default);
  Task<bool> StopAsync(Container container, CancellationToken cancellationToken = default);
  Task<CommandResult> RunCommandAsync(
    Container container,
    string command,
    CancellationToken cancellationToken = default
  );
}
