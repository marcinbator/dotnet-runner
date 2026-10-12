using Microsoft.Extensions.Options;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Runner.Core.Jobs;

public sealed class JobDefinitionLoaderService(IOptions<RunnerOptions> options) : IJobDefinitionLoaderService
{
  private readonly string _definitionsPath = Path.TrimEndingDirectorySeparator(
    Path.GetFullPath(options.Value.DefinitionsPath, AppContext.BaseDirectory)
  );

  private readonly IDeserializer _deserializer = new DeserializerBuilder()
    .WithNamingConvention(HyphenatedNamingConvention.Instance)
    .Build();

  public Job Load(string fileName)
  {
    string path = Path.GetFullPath(fileName, _definitionsPath);
    if (!path.StartsWith(_definitionsPath + Path.DirectorySeparatorChar, StringComparison.Ordinal))
      throw new ArgumentException($"Job definition '{fileName}' is outside of '{_definitionsPath}'.", nameof(fileName));

    Job job = _deserializer.Deserialize<Job>(File.ReadAllText(path));

    if (string.IsNullOrWhiteSpace(job.Name))
      throw new InvalidDataException($"Job definition '{fileName}': name is required.");
    if (string.IsNullOrWhiteSpace(job.Image))
      throw new InvalidDataException($"Job definition '{fileName}': image is required.");

    int stepIndex = 0;
    foreach (Step step in job.Steps)
    {
      step.Number = stepIndex;
      stepIndex++;

      if (string.IsNullOrWhiteSpace(step.Run))
        throw new InvalidDataException($"Job definition '{fileName}': step '{step.Id}' has no run command.");
    }

    return job;
  }
}
