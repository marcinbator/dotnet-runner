namespace Runner.Core.Jobs;

public sealed class Step
{
  public string? Id { get; set; }
  public string? Name { get; set; }
  public int Number { get; set; } = 0;
  public string Run { get; set; } = "";
}
