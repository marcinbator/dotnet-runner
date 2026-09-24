using Microsoft.Extensions.Options;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Runner.Core.Jobs;

public class JobDefinitionLoaderService(IOptions<RunnerOptions> options) : IJobDefinitionLoaderService
{
  private readonly string _definitionsPath = Path.IsPathRooted(options.Value.DefinitionsPath)
    ? options.Value.DefinitionsPath
    : Path.Combine(AppContext.BaseDirectory, options.Value.DefinitionsPath);

  private readonly IDeserializer _deserializer = new DeserializerBuilder()
     .WithNamingConvention(UnderscoredNamingConvention.Instance)
     .Build();

  public Job Load(string fileName)
  {
    string path = Path.Combine(_definitionsPath, fileName);
    if (!File.Exists(path)) throw new FileNotFoundException($"Not found: {path}");

    string yml = File.ReadAllText(path);
    var job = _deserializer.Deserialize<Job>(yml);

    if (string.IsNullOrEmpty(job.Name)) throw new ArgumentException("Job name cannot be empty.");

    return job;
  }
}
