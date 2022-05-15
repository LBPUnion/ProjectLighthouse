using LBPUnion.ProjectLighthouse.Helpers.Middlewares;

namespace LBPUnion.ProjectLighthouse.Servers.API.Startup;

public class ApiTestStartup : ApiStartup
{
    public ApiTestStartup(IConfiguration configuration) : base(configuration)
    {}

    public override void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseMiddleware<FakeRemoteIPAddressMiddleware>();
        base.Configure(app, env);
    }
}