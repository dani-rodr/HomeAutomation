using System.Reflection;
using System.IO;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Extensions.Logging;
using NetDaemon.Extensions.Scheduler;
using NetDaemon.Extensions.Tts;
using NetDaemon.Runtime;

var builder = WebApplication.CreateBuilder(args);

var webServerSettings =
    builder.Configuration.GetSection("WebServer").Get<WebServerSettings>()
    ?? new WebServerSettings();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(webServerSettings.HttpPort);
    if (webServerSettings.UseHttps)
    {
        options.ListenAnyIP(webServerSettings.HttpsPort, listenOptions => listenOptions.UseHttps());
    }
});

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
    .AddHomeEntitiesAndServices();

var dataProtectionKeyPath = Path.Combine(
    AppContext.BaseDirectory,
    ".aspnet",
    "DataProtection-Keys"
);
Directory.CreateDirectory(dataProtectionKeyPath);
builder
    .Services.AddDataProtection()
    .SetApplicationName("HomeAutomation")
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeyPath));

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers();

var app = builder.Build();

app.UseStaticFiles();
app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

await app.RunAsync().ConfigureAwait(false);

internal sealed class WebServerSettings
{
    public int HttpPort { get; init; } = 10000;
    public int HttpsPort { get; init; } = 10001;
    public bool UseHttps { get; init; }
}
