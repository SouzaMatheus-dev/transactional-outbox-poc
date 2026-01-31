using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using POC.Infra;
using POC.Worker.Dispatcher;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<DispatcherOptions>(builder.Configuration.GetSection(DispatcherOptions.SectionName));
builder.Services.AddInfraDispatcher(builder.Configuration);
builder.Services.AddHostedService<OutboxDispatcherService>();

var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "POC.Worker.Dispatcher")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();
builder.Logging.AddSerilog(logger);

var host = builder.Build();
await host.RunAsync();
