using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers.Extensions;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LBPUnion.ProjectLighthouse.Startup;

public class DebugWarmupLifetime : IHostLifetime
{
    public IHostApplicationLifetime ApplicationLifetime { get; }

    private CancellationTokenRegistration applicationStartedRegistration;

    private readonly ConsoleLifetime consoleLifetime;

    public DebugWarmupLifetime
    (
        IOptions<ConsoleLifetimeOptions> options,
        IHostEnvironment environment,
        IHostApplicationLifetime applicationLifetime,
        IOptions<HostOptions> hostOptions,
        ILoggerFactory loggerFactory
    )
    {
        this.consoleLifetime = new ConsoleLifetime(options, environment, applicationLifetime, hostOptions, loggerFactory);
        this.ApplicationLifetime = applicationLifetime;
    }

    public static void OnApplicationStarted()
    {
        using HttpClient client = new();

        string url = ServerConfiguration.Instance.ListenUrl;
        url = url.Replace("0.0.0.0", "127.0.0.1");

        Logger.LogDebug("Warming up Hot Reload...", LogArea.Startup);
        try
        {
            client.GetAsync(url).Wait();
        }
        catch(Exception e)
        {
            Logger.LogDebug("An error occurred while attempting to warm up hot reload. Initial page load will be delayed.", LogArea.Startup);
            Logger.LogDebug(e.ToDetailedException(), LogArea.Startup);
            return;
        }
        Logger.LogSuccess("Hot Reload is ready to go!", LogArea.Startup);
    }

    public Task StopAsync(CancellationToken cancellationToken) => this.consoleLifetime.StopAsync(cancellationToken);

    public Task WaitForStartAsync(CancellationToken cancellationToken)
    {
        this.applicationStartedRegistration = this.ApplicationLifetime.ApplicationStarted.Register((Action<object>)(_ => OnApplicationStarted()), (object)this);

        return this.consoleLifetime.WaitForStartAsync(cancellationToken);
    }
}