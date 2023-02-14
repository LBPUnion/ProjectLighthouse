#nullable enable
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Logging.Loggers.AspNet;
using LBPUnion.ProjectLighthouse.Servers.Website.Startup;
using LBPUnion.ProjectLighthouse.Types.Misc;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LBPUnion.ProjectLighthouse.Servers.Website;

public static class Program
{
    public static void Main(string[] args)
    {
        StartupTasks.Run(args, ServerType.Website);

        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
        => Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults
            (
                webBuilder =>
                {
                    webBuilder.UseStartup<WebsiteStartup>();
                    webBuilder.UseWebRoot("StaticFiles");
                    webBuilder.UseUrls(ServerConfiguration.Instance.WebsiteListenUrl);
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