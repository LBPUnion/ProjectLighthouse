using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Middlewares;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Middlewares;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Startup;

public class GameServerStartup
{
    public GameServerStartup(IConfiguration configuration)
    {
        this.Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

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
                options.OutputFormatters.Add(new XmlOutputFormatter());
                options.OutputFormatters.Add(new JsonOutputFormatter());
            }
        );

        services.AddDbContext<Database>();

        services.Configure<ForwardedHeadersOptions>
        (
            options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
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
        app.UseEndpoints(endpoints => endpoints.MapRazorPages());
    }
}
