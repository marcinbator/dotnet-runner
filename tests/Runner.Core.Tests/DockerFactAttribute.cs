using Xunit;

namespace Runner.Core.Tests;

public sealed class DockerFactAttribute : FactAttribute
{
  public DockerFactAttribute()
  {
    if (!File.Exists(new RunnerOptions().DockerSocket)) Skip = "Docker socket not available.";
  }
}
