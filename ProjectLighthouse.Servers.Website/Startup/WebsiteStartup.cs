using System.Globalization;
using System.Net;
using LBPUnion.ProjectLighthouse.Administration.Maintenance;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Configuration.ConfigurationCategories;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Localization;
using LBPUnion.ProjectLighthouse.Mail;
using LBPUnion.ProjectLighthouse.Middlewares;
using LBPUnion.ProjectLighthouse.Servers.Website.Captcha;
using LBPUnion.ProjectLighthouse.Servers.Website.Middlewares;
using LBPUnion.ProjectLighthouse.Services;
using LBPUnion.ProjectLighthouse.Types.Mail;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
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

    private IConfiguration Configuration { get; }

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

        services.AddHttpClient<ICaptchaService, CaptchaService>("CaptchaAPI",
            client =>
            {
                Uri captchaUri = ServerConfiguration.Instance.Captcha.Type switch
                {
                    CaptchaType.HCaptcha => new Uri("https://hcaptcha.com"),
                    CaptchaType.ReCaptcha => new Uri("https://www.google.com/recaptcha/api/"),
                    _ => throw new ArgumentOutOfRangeException(nameof(client)),
                };
                client.BaseAddress = captchaUri;
                client.Timeout = TimeSpan.FromSeconds(5);
                client.DefaultRequestHeaders.Add("User-Agent", "Project Lighthouse");
            });

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

        app.UseStatusCodePagesWithReExecute("/404");

        app.UseForwardedHeaders();

        app.UseMiddleware<RequestLogMiddleware>();
        app.UseMiddleware<RateLimitMiddleware>();
        app.UseMiddleware<UserRequiredRedirectMiddleware>();

        app.UseRouting();

        app.UseStaticFiles(new StaticFileOptions
        {
            ServeUnknownFileTypes = true,
        });

        app.UseRequestLocalization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapRazorPages();
        });
    }
}
