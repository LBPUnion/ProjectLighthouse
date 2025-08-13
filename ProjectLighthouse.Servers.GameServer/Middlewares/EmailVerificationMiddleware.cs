using System.Diagnostics;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Middlewares;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Middlewares;

public class EmailVerificationMiddleware: MiddlewareDBContext
{
    private readonly bool requireVerification;

    private static readonly HashSet<string> verifyPathList =
    [
        "uploadPhoto",
        "deletePhoto",
        "upload",
        "publish",
        "rateUserComment",
        "rateComment",
        "postUserComment",
        "postComment",
        "deleteUserComment",
        "deleteComment",
        "npdata",
        "grief",
        "updateUser",
        "update_my_pins",
        "match",
        "play",
        "enterLevel",
        "startPublish",
    ];
    
    public EmailVerificationMiddleware(RequestDelegate next, bool requireVerification) : base(next)
    {
        this.requireVerification = requireVerification;
    }

    public override async Task InvokeAsync(HttpContext context, DatabaseContext database)
    {
        if (requireVerification)
        {
            if (context.Request.Path.Value == null)
            {
                await this.next(context);
                return;
            }

            const string url = "/LITTLEBIGPLANETPS3_XML";
            string verifyPath = context.Request.Path;
            string strippedPath = verifyPath.Contains(url) ? verifyPath[url.Length..].Split("/")[0] : "";
            
            if (verifyPathList.Contains(strippedPath))
            {
                GameTokenEntity? gameToken = await database.GameTokenFromRequest(context.Request);
                if (gameToken == null)
                {
                    context.Response.StatusCode = 403;
                    context.Abort();
                    return;
                }
                UserEntity? user = await database.UserFromGameToken(gameToken);
                if (!user!.EmailAddressVerified)
                {
                    context.Response.StatusCode = 403;
                    context.Abort();
                    return;
                }
            }
        }   
        await this.next(context);
    }
}