using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Middlewares;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Middlewares;

public class EmailEnforcementMiddleware : MiddlewareDBContext
{
    private static readonly HashSet<string> enforcedPaths = new()
    {
        "rateUserComment",
        "rateComment",
        "comments",
        "userComments",
        "postUserComment",
        "postComment",
        "deleteUserComment",
        "deleteComment",
        "slots",
        "upload",
        "r",
        "uploadPhoto",
        "photos",
        "deletePhoto",
        "match",
        "play",
        "enterLevel",
        "user",
        "users",
        "updateUser",
        "update_my_pins",
        "startPublish",
        "publish",
        "unpublish",
        "playlists",
        "tags",
        "tag",
        "searches",
        "genres",
    };

    public EmailEnforcementMiddleware(RequestDelegate next) : base(next)
    { }

    public override async Task InvokeAsync(HttpContext context, DatabaseContext database)
    {
        // Split path into segments
        string[] pathSegments = context.Request.Path.ToString().Split("/");
        bool emailEnforcementEnabled = EnforceEmailConfiguration.Instance.EnableEmailEnforcement;

        if (pathSegments[0] == "LITTLEBIGPLANETPS3_XML")
        {
            // Get user via GameToken
            GameTokenEntity? token = await database.GameTokenFromRequest(context.Request);
            UserEntity? user = await database.UserFromGameToken(token);

            // Check second part of path to see if client is within an enforced path
            if (enforcedPaths.Contains(pathSegments[1]))
            {
                // Check if user is valid
                if (user == null)
                {
                    // Send bad request status
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Not a valid user");

                    // Don't go to next in pipeline
                    return;
                }

                // Check if email is there and verified
                if (emailEnforcementEnabled && (!user.EmailAddressVerified || user.EmailAddress == null))
                {
                    // Send bad request status
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Invalid user email address");
                    
                    // Don't go to next in pipeline
                    return;
                }
            }
        }

        // Go to next in pipeline
        await this.next(context);
    }
}