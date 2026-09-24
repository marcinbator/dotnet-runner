namespace Runner.Core;

public class RunnerOptions
{
  public string DefinitionsPath { get; set; } = "definitions";
  public string DockerSocket { get; set; } = "/var/run/docker.sock";
}
