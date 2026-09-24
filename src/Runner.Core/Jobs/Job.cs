namespace Runner.Core.Jobs;

public class Job
{
  public string? Name { get; set; }
  public List<Step> Steps { get; set; } = [];
}
