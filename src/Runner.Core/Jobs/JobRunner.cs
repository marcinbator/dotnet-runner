namespace Runner.Core.Jobs;

public class JobRunner(IStepRunner runner) : IJobRunner
{
  public void Run(Job job)
  {
    if (string.IsNullOrEmpty(job.Name)) throw new ArgumentException("Job name cannot be empty.");

    Console.WriteLine($"Job {job.Name} started.");

    while (job.Steps.Count > 0)
    {
      Step step = job.Steps.Dequeue();
      runner.Run(step);
    }

    Console.WriteLine($"Job {job.Name} ended.");
  }
}
