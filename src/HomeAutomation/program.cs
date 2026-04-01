using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Extensions.Logging;
using NetDaemon.Extensions.Scheduler;
using NetDaemon.Extensions.Tts;
using NetDaemon.Runtime;

var builder = WebApplication.CreateBuilder(args);

builder
    .Host.UseNetDaemonAppSettings()
    .UseNetDaemonDefaultLogging()
    .UseNetDaemonRuntime()
    .UseNetDaemonTextToSpeech();

builder
    .Services.AddAppsFromAssembly(Assembly.GetExecutingAssembly())
    .AddNetDaemonStateManager()
    .AddNetDaemonScheduler()
    .AddHomeAssistantGenerated()
    .AddHomeEntitiesAndServices()
    .AddControllers();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

await app.RunAsync().ConfigureAwait(false);
