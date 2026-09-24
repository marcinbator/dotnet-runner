using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Runner.Core;
using Runner.Core.Jobs;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddRunnerCore(builder.Configuration);

using var host = builder.Build();

var loader = host.Services.GetRequiredService<IJobDefinitionLoaderService>();
var runner = host.Services.GetRequiredService<IJobRunnerService>();

Job job = loader.Load("def1.yml");
await runner.Run(job);
