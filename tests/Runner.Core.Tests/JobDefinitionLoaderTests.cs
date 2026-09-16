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
    var loader = new JobDefinitionLoader(Options.Create(new JobRunnerOptions { DefinitionsPath = dir }));

    Job job = loader.Load("job.yml");

    Assert.Equal("test", job.Name);
  }
}
