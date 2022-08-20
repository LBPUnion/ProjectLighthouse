using LBPUnion.ProjectLighthouse.Middlewares;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Middlewares;

public class HandlePageErrorMiddleware : Middleware
{
    public HandlePageErrorMiddleware(RequestDelegate next) : base(next)
    {}
    
    public override async Task InvokeAsync(HttpContext ctx)
    {
        await this.next(ctx);
//        ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        if (ctx.Response.StatusCode == 404 && !ctx.Request.Path.StartsWithSegments("/gameAssets"))
        {
            try
            {
                ctx.Request.Path = "/404";
            }
            finally
            {
                // not much we can do to save us, carry on anyways
                await next(ctx);
            }
        }
    }
}