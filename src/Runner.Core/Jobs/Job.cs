namespace Runner.Core.Jobs;

public sealed class Job
{
  public string Name { get; set; } = "";
  public string Image { get; set; } = "";
  public List<Step> Steps { get; set; } = [];
}
