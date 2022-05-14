using LBPUnion.ProjectLighthouse.API.Startup;
using LBPUnion.ProjectLighthouse.Logging.Loggers.AspNet;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Settings;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LBPUnion.ProjectLighthouse.API;

public static class Program
{
    public static void Main(string[] args)
    {
        StartupTasks.Run(args, ServerType.Api);

        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
        => Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults
            (
                webBuilder =>
                {
                    webBuilder.UseStartup<ApiStartup>();
                    webBuilder.UseUrls(ServerConfiguration.Instance.ListenUrl);
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