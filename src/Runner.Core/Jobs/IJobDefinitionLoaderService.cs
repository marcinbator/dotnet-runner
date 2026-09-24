namespace Runner.Core.Jobs;

public interface IJobDefinitionLoaderService
{
  Job Load(string fileName);
}
