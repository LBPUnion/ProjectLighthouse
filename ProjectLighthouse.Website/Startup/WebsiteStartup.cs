using LBPUnion.ProjectLighthouse.Startup;
using LBPUnion.ProjectLighthouse.Startup.Middlewares;
using Microsoft.AspNetCore.HttpOverrides;

namespace LBPUnion.ProjectLighthouse.Website.Startup;

public sealed class WebsiteStartup
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
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        #if DEBUG
        app.UseDeveloperExceptionPage();
        #endif

        app.UseForwardedHeaders();

        app.UseMiddleware<RequestLogMiddleware>();

        app.UseRouting();

        app.UseStaticFiles();

        app.UseEndpoints(endpoints => endpoints.MapControllers());
        app.UseEndpoints(endpoints => endpoints.MapRazorPages());
    }
}