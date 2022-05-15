using LBPUnion.ProjectLighthouse.Logging.Loggers.AspNet;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Startup;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Settings;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer;

public static class Program
{
    public static void Main(string[] args)
    {
        StartupTasks.Run(args, ServerType.GameApi);

        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
        => Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults
            (
                webBuilder =>
                {
                    webBuilder.UseStartup<GameServerStartup>();
                    webBuilder.UseUrls(ServerConfiguration.Instance.GameApiListenUrl);
                }
            )
            .ConfigureLogging
            (
                logging =>
                {
                    logging.ClearProviders();
                    logging.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, AspNetToLighthouseLoggerProvider>());
                }
            );
}