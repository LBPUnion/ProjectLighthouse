using LBPUnion.ProjectLighthouse.Helpers.Middlewares;

namespace LBPUnion.ProjectLighthouse.Startup;

public class TestGameApiStartup : GameApiStartup
{
    public TestGameApiStartup(IConfiguration configuration) : base(configuration)
    {}

    public override void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseMiddleware<FakeRemoteIPAddressMiddleware>();
        base.Configure(app, env);
    }
}