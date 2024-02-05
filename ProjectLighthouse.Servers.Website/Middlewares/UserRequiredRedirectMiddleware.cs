using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Middlewares;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Middlewares;

public class UserRequiredRedirectMiddleware : MiddlewareDBContext
{
    public UserRequiredRedirectMiddleware(RequestDelegate next) : base(next)
    { }

    public override async Task InvokeAsync(HttpContext ctx, DatabaseContext database)
    {
        WebTokenEntity? token = database.WebTokenFromRequest(ctx.Request);
        if (token == null || pathContains(ctx, "/logout"))
        {
            await this.next(ctx);
            return;
        }

        UserEntity? user = await database.Users.FirstOrDefaultAsync(u => u.UserId == token.UserId);
        if (user == null)
        {
            await this.next(ctx);
            return;
        }

        // Request ends with a path (e.g. /css/style.css)
        if (!string.IsNullOrEmpty(Path.GetExtension(ctx.Request.Path)) || pathContains(ctx, "/gameAssets"))
        {
            await this.next(ctx);
            return;
        }

        if (!token.Verified && ServerConfiguration.Instance.TwoFactorConfiguration.TwoFactorEnabled)
        {
            if (!pathContains(ctx, "/2fa"))
            {
                ctx.Response.Redirect("/2fa");
                return;
            }

            await this.next(ctx);
            return;
        }

        if (user.PasswordResetRequired)
        {
            if (!pathContains(ctx, "/passwordResetRequired", "/passwordReset"))
            {
                ctx.Response.Redirect("/passwordResetRequired");
                return;
            }

            await this.next(ctx);
            return;
        }

        if (user.IsBanned)
        {
            if (!pathContains(ctx, "/banned"))
            {
                ctx.Response.Redirect("/banned");
                return;
            }

            await this.next(ctx);
            return;
        }

        if (user.EmailAddress == null && ServerConfiguration.Instance.Mail.MailEnabled)
        {
            if (!pathContains(ctx, "/login/setEmail"))
            {
                ctx.Response.Redirect("/login/setEmail");
                return;
            }
            
            await this.next(ctx);
            return;
        }

        if (!user.EmailAddressVerified && ServerConfiguration.Instance.Mail.MailEnabled)
        {
            if (!pathContains(ctx, "/login/sendVerificationEmail", "/verifyEmail"))
            {
                ctx.Response.Redirect("/login/sendVerificationEmail");
                return;
            }

            await this.next(ctx);
            return;
        }

        if (user.TwoFactorRequired && !user.IsTwoFactorSetup && ServerConfiguration.Instance.TwoFactorConfiguration.TwoFactorEnabled)
        {
            if (!pathContains(ctx, "/setup2fa"))
            {
                ctx.Response.Redirect("/setup2fa");
                return;
            }

            await this.next(ctx);
            return;
        }

        await this.next(ctx);
    }

    private static bool pathContains(HttpContext ctx, params string[] pathList)
    {
        return pathList.Any(path => ctx.Request.Path.StartsWithSegments(path));
    }
}
