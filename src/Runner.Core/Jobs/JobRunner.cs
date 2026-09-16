namespace Runner.Core.Jobs;

public class JobRunner : IJobRunner
{
  public void Run(Job job)
  {
    if (string.IsNullOrEmpty(job.Name)) throw new ArgumentException("Job name cannot be empty.");

    Console.WriteLine($"Job {job.Name} started.");
    Thread.Sleep(5000);
    Console.WriteLine($"Job {job.Name} ended.");
  }
}
