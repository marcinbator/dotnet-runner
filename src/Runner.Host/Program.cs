using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Runner.Core;
using Runner.Core.Jobs;
using Serilog;

var builder = Host.CreateApplicationBuilder(
  new HostApplicationBuilderSettings { Args = args, ContentRootPath = AppContext.BaseDirectory }
);
builder.Services.AddSerilog(logger => logger.ReadFrom.Configuration(builder.Configuration));
builder.Services.AddRunnerCore(builder.Configuration);

using IHost host = builder.Build();

var loader = host.Services.GetRequiredService<IJobDefinitionLoaderService>();
var runner = host.Services.GetRequiredService<IJobRunnerService>();
var logger = host.Services.GetRequiredService<ILogger<Program>>();

try
{
  Job job = loader.Load("def1.yml");
  await runner.RunAsync(job);
}
catch (StepFailedException ex)
{
  LogJobFailed(logger, ex.Message);
  Environment.ExitCode = 1;
}

internal partial class Program
{
  [LoggerMessage(Level = LogLevel.Error, Message = "{Error}")]
  internal static partial void LogJobFailed(Microsoft.Extensions.Logging.ILogger logger, string error);
}
