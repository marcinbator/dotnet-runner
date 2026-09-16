namespace Runner.Core.Jobs;

public class StepRunner : IStepRunner
{
  public void Run(Step step)
  {
    Console.WriteLine($"-> Step {step.Name} (ID: {step.Id})");
    Thread.Sleep(1000);
  }
}