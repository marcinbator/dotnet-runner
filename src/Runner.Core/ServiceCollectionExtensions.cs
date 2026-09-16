using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Runner.Core.Jobs;

namespace Runner.Core;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddRunnerCore(this IServiceCollection services, IConfiguration configuration)
  {
    services.Configure<JobRunnerOptions>(configuration.GetSection("Runner"));
    services.AddSingleton<IJobDefinitionLoader, JobDefinitionLoader>();
    services.AddSingleton<IJobRunner, JobRunner>();
    services.AddSingleton<IStepRunner, StepRunner>();

    return services;
  }
}
