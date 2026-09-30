using Runner.Core.Containers;

namespace Runner.Core.Tests;

internal sealed class RecordingContainerRuntime(List<string> events) : IContainerRuntimeService
{
  public CommandResult CommandResult { get; set; } = new(0, "", "");

  public Task<Container> RunAsync(string image, CancellationToken cancellationToken = default)
  {
    events.Add($"run {image}");
    return Task.FromResult(new Container("ct1", "ct1"));
  }

  public Task<bool> StopAsync(Container container, CancellationToken cancellationToken = default)
  {
    events.Add($"stop {container.Id}");
    return Task.FromResult(true);
  }

  public Task<CommandResult> RunCommandAsync(
    Container container,
    string command,
    CancellationToken cancellationToken = default
  )
  {
    events.Add($"exec {command} in {container.Id}");
    return Task.FromResult(CommandResult);
  }
}
