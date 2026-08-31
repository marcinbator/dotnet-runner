using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace runner.src.models;

public class Job
{
  public string? Name { get; set; }

  public Job()
  {
  }

  public Job(string path)
  {
    if (!File.Exists(path)) throw new FileNotFoundException($"Not found: {path}");

    var deserializer = new DeserializerBuilder()
      .WithNamingConvention(UnderscoredNamingConvention.Instance)
      .Build();

    string yml = File.ReadAllText(path);
    var job = deserializer.Deserialize<Job>(yml);

    Name = job.Name;
  }
}
