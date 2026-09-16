namespace Runner.Core.Jobs;

public class Job
{
  public string? Name { get; set; }
  public Queue<Step> Steps { get; set; } = new();
}
