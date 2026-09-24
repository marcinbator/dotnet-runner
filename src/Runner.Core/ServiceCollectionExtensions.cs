using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Runner.Core.Containers;
using Runner.Core.Jobs;

namespace Runner.Core;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddRunnerCore(this IServiceCollection services, IConfiguration configuration)
  {
    services.Configure<RunnerOptions>(configuration.GetSection("Runner"));
    services.AddSingleton<IJobDefinitionLoaderService, JobDefinitionLoaderService>();
    services.AddSingleton<IJobRunnerService, JobRunnerService>();
    services.AddSingleton<IStepRunnerService, StepRunnerService>();
    services.AddSingleton<IContainerRuntimeService, ContainerRuntimeService>();

    return services;
  }
}
