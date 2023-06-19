using System.Net;
using LBPUnion.ProjectLighthouse.Administration.Maintenance;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Mail;
using LBPUnion.ProjectLighthouse.Middlewares;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Middlewares;
using LBPUnion.ProjectLighthouse.Services;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Mail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Startup;

public class GameServerStartup
{
    public GameServerStartup(IConfiguration configuration)
    {
        this.Configuration = configuration;
    }

    private IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = "tokenAuth";
            options.AddScheme<TokenAuthHandler>("tokenAuth", null);
        });

        services.AddAuthorization(o =>
        {
            AuthorizationPolicyBuilder builder = new("tokenAuth");
            builder = builder.RequireClaim("userId");
            o.DefaultPolicy = builder.Build();
        });

        services.AddMvc
        (
            options =>
            {
                options.OutputFormatters.Add(new LbpOutputFormatter());
                options.OutputFormatters.Add(new JsonOutputFormatter());
                options.OutputFormatters.Add(new StringOutputFormatter());
            }
        );

        services.AddDbContext<DatabaseContext>(builder =>
        {
            builder.UseMySql(ServerConfiguration.Instance.DbConnectionString,
                MySqlServerVersion.LatestSupportedServerVersion);
        });

        IMailService mailService = ServerConfiguration.Instance.Mail.MailEnabled
            ? new MailQueueService(new SmtpMailSender())
            : new NullMailService();
        services.AddSingleton(mailService);

        services.AddHostedService(provider => new RepeatingTaskService(provider, MaintenanceHelper.RepeatingTasks));

        services.Configure<ForwardedHeadersOptions>
        (
            options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                foreach (KeyValuePair<string, string?> proxy in this.Configuration.GetSection("KnownProxies").AsEnumerable())
                {
                    if (proxy.Value == null) continue;
                    options.KnownProxies.Add(IPAddress.Parse(proxy.Value));
                }
            }
        );
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        bool computeDigests = true;

        if (string.IsNullOrEmpty(ServerConfiguration.Instance.DigestKey.PrimaryDigestKey))
        {
            Logger.Warn
            (
                "The serverDigestKey configuration option wasn't set, so digest headers won't be set or verified. This will also prevent LBP 1, LBP 2, and LBP Vita from working. " +
                "To increase security, it is recommended that you find and set this variable.",
                LogArea.Startup
            );
            computeDigests = false;
        }

        #if DEBUG
        app.UseDeveloperExceptionPage();
        #endif

        app.UseForwardedHeaders();

        app.UseMiddleware<RequestLogMiddleware>();
        app.UseMiddleware<RateLimitMiddleware>();
        app.UseMiddleware<DigestMiddleware>(computeDigests);
        app.UseMiddleware<SetLastContactMiddleware>();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}
