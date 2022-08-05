using System.Globalization;
using LBPUnion.ProjectLighthouse.Localization;
using LBPUnion.ProjectLighthouse.Middlewares;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;

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

        services.Configure<RequestLocalizationOptions>(config =>
        {
            List<CultureInfo> languages = LocalizationManager.GetAvailableLanguages().Select(l => new CultureInfo(LocalizationManager.MapLanguage(l))).ToList();

            config.DefaultRequestCulture = new RequestCulture(new CultureInfo("en"));

            config.SupportedCultures = languages;
            config.SupportedUICultures = languages;
        });

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

        app.UseRouting();

        app.UseStaticFiles(new StaticFileOptions
        {
            ServeUnknownFileTypes = true,
        });

        app.UseRequestLocalization();

        app.UseEndpoints(endpoints => endpoints.MapControllers());
        app.UseEndpoints(endpoints => endpoints.MapRazorPages());
    }
}
