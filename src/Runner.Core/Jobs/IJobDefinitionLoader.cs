namespace Runner.Core.Jobs;

public interface IJobDefinitionLoader
{
  Job Load(string fileName);
}
