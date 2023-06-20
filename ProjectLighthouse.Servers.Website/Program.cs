using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Logging.Loggers.AspNet;
using LBPUnion.ProjectLighthouse.Servers.Website.Startup;
using LBPUnion.ProjectLighthouse.Types.Misc;

await StartupTasks.Run(ServerType.Website);

IHostBuilder builder = Host.CreateDefaultBuilder();
builder.ConfigureWebHostDefaults(webBuilder =>
{
    webBuilder.UseStartup<WebsiteStartup>();
    webBuilder.UseUrls(ServerConfiguration.Instance.WebsiteListenUrl);
    webBuilder.UseWebRoot("StaticFiles");
});

builder.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddProvider(new AspNetToLighthouseLoggerProvider());
});
await builder.Build().RunAsync();