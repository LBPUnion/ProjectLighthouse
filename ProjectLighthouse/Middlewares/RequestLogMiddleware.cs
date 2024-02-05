using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Logging;
using Microsoft.AspNetCore.Http;

namespace LBPUnion.ProjectLighthouse.Middlewares;

public class RequestLogMiddleware : Middleware
{
    public RequestLogMiddleware(RequestDelegate next) : base(next)
    {}

    // Logs every request and the response to it
    // Example: "200, 13ms: GET /LITTLEBIGPLANETPS3_XML/news"
    // Example: "404, 127ms: GET /asdasd?query=osucookiezi727ppbluezenithtopplayhdhr"
    public override async Task InvokeAsync(HttpContext ctx)
    {
        Stopwatch requestStopwatch = new();
        requestStopwatch.Start();

        ctx.Request.EnableBuffering(); // Allows us to reset the position of Request.Body for later logging

        // Log all headers.
        // foreach (KeyValuePair<string, StringValues> header in ctx.Request.Headers) Logger.Debug($"{header.Key}: {header.Value}", LogArea.HTTP);

        await this.next(ctx); // Handle the request so we can get the status code from it

        requestStopwatch.Stop();

        Logger.Info
        (
            $"{ctx.Response.StatusCode}, {requestStopwatch.ElapsedMilliseconds}ms: {ctx.Request.Method} {ctx.Request.Path}{ctx.Request.QueryString}",
            LogArea.HTTP
        );

        #if DEBUG
        // Log post body
        if (ctx.Request.Method == "POST")
        {
            string body = Encoding.ASCII.GetString(await ctx.Request.BodyReader.ReadAllAsync());
            Logger.Debug(body, LogArea.HTTP);
        }
        #endif
    }
}