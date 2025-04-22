using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Middlewares;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Middlewares;

public class EmailEnforcementMiddleware : MiddlewareDBContext
{
    private static readonly HashSet<string> enforcedPaths = ServerConfiguration.Instance.EmailEnforcement.BlockedEndpoints;

    public EmailEnforcementMiddleware(RequestDelegate next) : base(next)
    { }

    public override async Task InvokeAsync(HttpContext context, DatabaseContext database)
    {
        if (ServerConfiguration.Instance.EmailEnforcement.EnableEmailEnforcement)
        { 
            // Split path into segments
            string[] pathSegments = context.Request.Path.ToString().Split("/", StringSplitOptions.RemoveEmptyEntries);

            if (pathSegments[0] == "LITTLEBIGPLANETPS3_XML")
            {
                // Get user via GameToken
                GameTokenEntity? token = await database.GameTokenFromRequest(context.Request);
                UserEntity? user = await database.UserFromGameToken(token);

                // Check second part of path to see if client is within an enforced path
                // This could probably be reworked, seeing as you may want to check for a deeper sub-path
                // But it should be perfectly fine for now
                if (enforcedPaths.Contains(pathSegments[1]))
                {
                    // Check if user is valid, don't want any exceptions
                    if (user == null)
                    {
                        // Send bad request status
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsync("Not a valid user");

                        // Don't go to next in pipeline
                        return;
                    }

                    // Check if email is there and verified
                    if (!user.EmailAddressVerified || user.EmailAddress == null)
                    {
                        // Send bad request status
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsync("Invalid user email address");

                        // Don't go to next in pipeline
                        return;
                    }
                }
            }
        }

        // Go to next in pipeline
        await this.next(context);
    }
}