using runner.src.models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

Job j1 = new();
Console.WriteLine(j1.Name);

var deserializer = new DeserializerBuilder()
    .WithNamingConvention(UnderscoredNamingConvention.Instance) 
    .Build();

string fileName = Path.Combine(AppContext.BaseDirectory, "definitions", "def1.yml");

if (!File.Exists(fileName))
{
    throw new FileNotFoundException($"Nie znaleziono pliku: {fileName}");
}

string yml = File.ReadAllText(fileName);

var p = deserializer.Deserialize<Job>(yml);

Console.WriteLine(p.Name);