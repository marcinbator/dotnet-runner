using Microsoft.Extensions.Options;
using Runner.Core.Jobs;

namespace Runner.Core.Tests;

public sealed class JobDefinitionLoaderServiceTests : IDisposable
{
  private const string FileName = "job.yml";

  private readonly DirectoryInfo _dir = Directory.CreateTempSubdirectory();
  private readonly JobDefinitionLoaderService _loader;

  public JobDefinitionLoaderServiceTests()
  {
    _loader = new JobDefinitionLoaderService(Options.Create(new RunnerOptions { DefinitionsPath = _dir.FullName }));
  }

  [Fact]
  public void Load_ReturnsJob_WithNameAndImageFromYaml()
  {
    Job job = Load(
      """
      name: test
      image: alpine:3.20
      """
    );

    Assert.Equal("test", job.Name);
    Assert.Equal("alpine:3.20", job.Image);
  }

  [Fact]
  public void Load_ReturnsJob_WithStepsFromYaml()
  {
    Job job = Load(
      """
      name: test
      image: alpine:3.20
      steps:
        - id: s1
          name: step1
          run: echo one
        - id: s2
          name: step2
          run: echo two
      """
    );

    Assert.Equal(["s1", "s2"], job.Steps.Select(s => s.Id));
    Assert.Equal(["echo one", "echo two"], job.Steps.Select(s => s.Run));
  }

  [Fact]
  public void Load_Throws_WhenNameIsMissing()
  {
    Assert.Throws<InvalidDataException>(() => Load("image: alpine:3.20"));
  }

  [Fact]
  public void Load_Throws_WhenImageIsMissing()
  {
    Assert.Throws<InvalidDataException>(() => Load("name: test"));
  }

  [Fact]
  public void Load_Throws_WhenStepHasNoRunCommand()
  {
    Assert.Throws<InvalidDataException>(() =>
      Load(
        """
        name: test
        image: alpine:3.20
        steps:
          - id: s1
        """
      )
    );
  }

  [Fact]
  public void Load_Throws_WhenFileDoesNotExist()
  {
    Assert.Throws<FileNotFoundException>(() => _loader.Load("missing.yml"));
  }

  [Fact]
  public void Load_Throws_WhenFileIsOutsideDefinitionsPath()
  {
    Assert.Throws<ArgumentException>(() => _loader.Load("../job.yml"));
  }

  public void Dispose() => _dir.Delete(recursive: true);

  //

  private Job Load(string yaml)
  {
    File.WriteAllText(Path.Combine(_dir.FullName, FileName), yaml);
    return _loader.Load(FileName);
  }
}
