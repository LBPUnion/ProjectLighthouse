using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
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
//                    foreach (KeyValuePair<string, StringValues> header in context.Request.Headers) Logger.Log($"{header.Key}: {header.Value}");

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
            ctx.Request.Body.Position = 0;
            Logger.Debug(await new StreamReader(ctx.Request.Body).ReadToEndAsync(), LogArea.HTTP);
        }
        #endif
    }
}