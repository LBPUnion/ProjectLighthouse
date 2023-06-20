using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Logging.Loggers.AspNet;
using LBPUnion.ProjectLighthouse.Servers.API.Startup;
using LBPUnion.ProjectLighthouse.Types.Misc;

await StartupTasks.Run(ServerType.Api);

IHostBuilder builder = Host.CreateDefaultBuilder();
builder.ConfigureWebHostDefaults(webBuilder =>
{
    webBuilder.UseStartup<ApiStartup>();
    webBuilder.UseUrls(ServerConfiguration.Instance.ApiListenUrl);
});

builder.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddProvider(new AspNetToLighthouseLoggerProvider());
});

await builder.Build().RunAsync();