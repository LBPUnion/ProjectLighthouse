using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Middlewares;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Middlewares;

public class UserRequiredRedirectMiddleware : MiddlewareDBContext
{
    public UserRequiredRedirectMiddleware(RequestDelegate next) : base(next)
    { }

    public override async Task InvokeAsync(HttpContext ctx, Database database)
    {
        User? user = database.UserFromWebRequest(ctx.Request);
        if (user == null || ctx.Request.Path.StartsWithSegments("/logout"))
        {
            await this.next(ctx);
            return;
        }

        if (user.PasswordResetRequired && !ctx.Request.Path.StartsWithSegments("/passwordResetRequired") &&
            !ctx.Request.Path.StartsWithSegments("/passwordReset"))
        {
            ctx.Response.Redirect("/passwordResetRequired");
            return;
        }

        if (ServerConfiguration.Instance.Mail.MailEnabled)
        {
            // The normal flow is for users to set their email during login so just force them to log out
            if (user.EmailAddress == null)
            {
                ctx.Response.Redirect("/logout");
                return;
            }

            if (!user.EmailAddressVerified &&
                !ctx.Request.Path.StartsWithSegments("/login/sendVerificationEmail") &&
                !ctx.Request.Path.StartsWithSegments("/verifyEmail"))
            {
                ctx.Response.Redirect("/login/sendVerificationEmail");
                return;
            }
        }

        await this.next(ctx);
    }
}