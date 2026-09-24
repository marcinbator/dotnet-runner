using Microsoft.Extensions.Options;
using Runner.Core.Jobs;
using Xunit;

namespace Runner.Core.Tests;

public class JobDefinitionLoaderTests
{
  [Fact]
  public void Load_ReturnsJob_WithNameFromYaml()
  {
    string dir = Directory.CreateTempSubdirectory().FullName;
    File.WriteAllText(Path.Combine(dir, "job.yml"), "name: test");
    var loader = new JobDefinitionLoaderService(Options.Create(new RunnerOptions { DefinitionsPath = dir }));

    Job job = loader.Load("job.yml");

    Assert.Equal("test", job.Name);
  }

  [Fact]
  public void Load_ReturnsJob_WithStepsFromYaml()
  {
    string dir = Directory.CreateTempSubdirectory().FullName;
    File.WriteAllText(Path.Combine(dir, "job.yml"), """
      name: test
      steps:
        - id: s1
          name: step1
        - id: s2
          name: step2
      """);
    var loader = new JobDefinitionLoaderService(Options.Create(new RunnerOptions { DefinitionsPath = dir }));

    Job job = loader.Load("job.yml");

    Assert.Equal(["s1", "s2"], job.Steps.Select(s => s.Id));
  }
}
