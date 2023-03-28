using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Middlewares;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Users;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Middlewares;

public class SetLastContactMiddleware : MiddlewareDBContext
{
    public SetLastContactMiddleware(RequestDelegate next) : base(next)
    { }

    public override async Task InvokeAsync(HttpContext context, DatabaseContext database)
    {
        #nullable enable
        // Log LastContact for LBP1. This is done on LBP2/3/V on a Match request.
        if (context.Request.Path.ToString().StartsWith("/LITTLEBIGPLANETPS3_XML"))
        {
            // We begin by grabbing a token from the request, if this is a LBPPS3_XML request of course.
            GameTokenEntity? gameToken = await database.GameTokenFromRequest(context.Request);

            if (gameToken?.GameVersion == GameVersion.LittleBigPlanet1)
                // Ignore UserFromGameToken null because user must exist for a token to exist
                await LastContactHelper.SetLastContact
                    (database, (await database.UserFromGameToken(gameToken))!, GameVersion.LittleBigPlanet1, gameToken.Platform);
        }
        #nullable disable

        await this.next(context);
    }
}