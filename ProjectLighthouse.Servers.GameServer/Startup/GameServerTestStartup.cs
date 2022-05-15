using LBPUnion.ProjectLighthouse.Helpers.Middlewares;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Startup;

public class GameServerTestStartup : GameServerStartup
{
    public GameServerTestStartup(IConfiguration configuration) : base(configuration)
    {}

    public override void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseMiddleware<FakeRemoteIPAddressMiddleware>();
        base.Configure(app, env);
    }
}