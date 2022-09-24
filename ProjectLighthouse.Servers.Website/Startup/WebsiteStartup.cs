using System.Globalization;
using System.Reflection;
using LBPUnion.ProjectLighthouse.Localization;
using LBPUnion.ProjectLighthouse.Middlewares;
using LBPUnion.ProjectLighthouse.Servers.Website.Middlewares;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Extensions.FileProviders;

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
        services.AddRazorPages().WithRazorPagesAtContentRoot().AddRazorRuntimeCompilation((options) =>
        {
            // jank but works
            string projectDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
            
            options.FileProviders.Clear();
            options.FileProviders.Add(new PhysicalFileProvider(projectDir));
        });
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

        app.UseMiddleware<HandlePageErrorMiddleware>();
        app.UseMiddleware<RequestLogMiddleware>();
        app.UseMiddleware<UserRequiredRedirectMiddleware>();
        app.UseMiddleware<RateLimitMiddleware>();

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
