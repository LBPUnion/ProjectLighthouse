using LBPUnion.ProjectLighthouse.Middlewares;
using Microsoft.AspNetCore.HttpOverrides;

#if !DEBUG
using Microsoft.Extensions.Hosting.Internal;
#else
using LBPUnion.ProjectLighthouse.Startup;
#endif

namespace LBPUnion.ProjectLighthouse.Servers.Website.Startup;

public class WebsiteStartup
{
    public WebsiteStartup(IConfiguration configuration)
    {
        this.Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        #if DEBUG
        services.AddRazorPages().WithRazorPagesAtContentRoot().AddRazorRuntimeCompilation();
        #else
        services.AddRazorPages().WithRazorPagesAtContentRoot();
        #endif

        services.AddDbContext<Database>();

        services.Configure<ForwardedHeadersOptions>
        (
            options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            }
        );

        #if DEBUG
        services.AddSingleton<IHostLifetime, DebugWarmupLifetime>();
        #else
        services.AddSingleton<IHostLifetime, ConsoleLifetime>();
        #endif
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        #if DEBUG
        app.UseDeveloperExceptionPage();
        #endif

        app.UseForwardedHeaders();

        app.UseMiddleware<RequestLogMiddleware>();

        #if !DEBUG
        app.UseHttpsRedirection();
        #endif

        app.UseRouting();
        app.UseStaticFiles();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
        app.UseEndpoints(endpoints => endpoints.MapRazorPages());
    }
}