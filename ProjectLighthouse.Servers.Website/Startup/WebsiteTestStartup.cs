using LBPUnion.ProjectLighthouse.Middlewares;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Startup;

public class WebsiteTestStartup : WebsiteStartup
{
    public WebsiteTestStartup(IConfiguration configuration) : base(configuration)
    {}

    public override void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseMiddleware<FakeRemoteIPAddressMiddleware>();
        base.Configure(app, env);
    }
}