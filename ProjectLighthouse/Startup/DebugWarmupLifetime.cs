using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kettu;
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

        Logger.Log("Warming up Hot Reload...", LoggerLevelStartup.Instance);
        client.GetAsync(ServerSettings.Instance.ServerListenUrl).Wait();
        Logger.Log("Hot Reload is ready to go!", LoggerLevelStartup.Instance);
    }

    public Task StopAsync(CancellationToken cancellationToken) => this.consoleLifetime.StopAsync(cancellationToken);

    public Task WaitForStartAsync(CancellationToken cancellationToken)
    {
        this.applicationStartedRegistration = this.ApplicationLifetime.ApplicationStarted.Register((Action<object>)(_ => OnApplicationStarted()), (object)this);

        return this.consoleLifetime.WaitForStartAsync(cancellationToken);
    }
}