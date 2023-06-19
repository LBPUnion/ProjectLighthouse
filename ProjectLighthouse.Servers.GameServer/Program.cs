using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Logging.Loggers.AspNet;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Startup;
using LBPUnion.ProjectLighthouse.Types.Misc;

await StartupTasks.Run(ServerType.GameServer);

IHostBuilder builder = Host.CreateDefaultBuilder();
builder.ConfigureWebHostDefaults(webBuilder =>
{
    webBuilder.UseStartup<GameServerStartup>();
    webBuilder.UseUrls(ServerConfiguration.Instance.GameApiListenUrl);
});

builder.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddProvider(new AspNetToLighthouseLoggerProvider());
});

await builder.Build().RunAsync();