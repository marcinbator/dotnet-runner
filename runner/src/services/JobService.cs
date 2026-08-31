using runner.src.models;

namespace runner.src.services;

public class JobService(Job job)
{
  private Job Job = job;

  public void Run()
  {
    if (Job.Name == null || Job.Name.Length == 0) throw new ArgumentException("Job name cannot be empty.");
    Console.WriteLine($"Job {Job.Name} started.");
    Thread.Sleep(5000);
    Console.WriteLine($"Job {Job.Name} ended.");
  }
}
