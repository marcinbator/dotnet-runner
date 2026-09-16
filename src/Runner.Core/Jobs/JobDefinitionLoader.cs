using Microsoft.Extensions.Options;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Runner.Core.Jobs;

public class JobDefinitionLoader(IOptions<JobRunnerOptions> options) : IJobDefinitionLoader
{
  private readonly string _definitionsPath = Path.IsPathRooted(options.Value.DefinitionsPath)
    ? options.Value.DefinitionsPath
    : Path.Combine(AppContext.BaseDirectory, options.Value.DefinitionsPath);

  public Job Load(string fileName)
  {
    string path = Path.Combine(_definitionsPath, fileName);
    if (!File.Exists(path)) throw new FileNotFoundException($"Not found: {path}");

    var deserializer = new DeserializerBuilder()
      .WithNamingConvention(UnderscoredNamingConvention.Instance)
      .WithTypeConverter(new QueueYamlTypeConverter())
      .Build();

    string yml = File.ReadAllText(path);
    return deserializer.Deserialize<Job>(yml);
  }
}
