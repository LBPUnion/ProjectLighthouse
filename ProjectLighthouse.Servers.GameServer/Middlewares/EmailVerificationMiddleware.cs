using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Middlewares;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using Microsoft.AspNetCore.Http.Features;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Middlewares;

public class EmailVerificationAttribute : Attribute;

public class EmailVerificationMiddleware: MiddlewareDBContext
{

    public EmailVerificationMiddleware(RequestDelegate next) : base(next)
    {
        
    }

    public override async Task InvokeAsync(HttpContext context, DatabaseContext database)
    {
        if (ServerConfiguration.Instance.Mail.RequireEmailVerification)
        {
            Endpoint? endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
            EmailVerificationAttribute? attribute = endpoint?.Metadata.GetMetadata<EmailVerificationAttribute>();
            
            if (attribute != null)
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
                    context.Response.StatusCode = 401; // 403 will cause a re-auth
                    context.Abort();
                    return;
                }
            }
        }   
        await this.next(context);
    }
}