using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Models;
#if RELEASE
using Microsoft.Extensions.Hosting.Internal;
#endif

namespace LBPUnion.ProjectLighthouse.Startup;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        this.Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        #if DEBUG
        services.AddRazorPages().WithRazorPagesAtContentRoot().AddRazorRuntimeCompilation();
        #else
        services.AddRazorPages().WithRazorPagesAtContentRoot();
        #endif

        services.AddMvc
        (
            options =>
            {
                options.OutputFormatters.Add(new XmlOutputFormatter());
                options.OutputFormatters.Add(new JsonOutputFormatter());
            }
        );

        services.AddDbContext<Database>();

        services.Configure<ForwardedHeadersOptions>
        (
            options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            }
        );

        services.AddSwaggerGen
        (
            c =>
            {
                // Give swagger the name and version of our project
                c.SwaggerDoc
                (
                    "v1",
                    new OpenApiInfo
                    {
                        Title = "Project Lighthouse API",
                        Version = "v1",
                    }
                );

                // Filter out endpoints not in /api/v1
                c.DocumentFilter<SwaggerFilter>();

                // Add XMLDoc to swagger
                string xmlDocs = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlDocs));
            }
        );

        #if DEBUG
        services.AddSingleton<IHostLifetime, DebugWarmupLifetime>();
        #else
        services.AddSingleton<IHostLifetime, ConsoleLifetime>();
        #endif
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        bool computeDigests = true;

        if (string.IsNullOrEmpty(ServerConfiguration.Instance.DigestKey.PrimaryDigestKey))
        {
            Logger.LogWarn
            (
                "The serverDigestKey configuration option wasn't set, so digest headers won't be set or verified. This will also prevent LBP 1, LBP 2, and LBP Vita from working. " +
                "To increase security, it is recommended that you find and set this variable.",
                LogArea.Startup
            );
            computeDigests = false;
        }

        #if DEBUG
        app.UseDeveloperExceptionPage();
        #endif

        app.UseForwardedHeaders();

        app.UseSwagger();
        app.UseSwaggerUI
        (
            c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "Project Lighthouse API");
            }
        );

        // Logs every request and the response to it
        // Example: "200, 13ms: GET /LITTLEBIGPLANETPS3_XML/news"
        // Example: "404, 127ms: GET /asdasd?query=osucookiezi727ppbluezenithtopplayhdhr"
        app.Use
        (
            async (context, next) =>
            {
                Stopwatch requestStopwatch = new();
                requestStopwatch.Start();

                context.Request.EnableBuffering(); // Allows us to reset the position of Request.Body for later logging

                // Log all headers.
//                    foreach (KeyValuePair<string, StringValues> header in context.Request.Headers) Logger.Log($"{header.Key}: {header.Value}");

                await next(context); // Handle the request so we can get the status code from it

                requestStopwatch.Stop();

                Logger.LogInfo
                (
                    $"{context.Response.StatusCode}, {requestStopwatch.ElapsedMilliseconds}ms: {context.Request.Method} {context.Request.Path}{context.Request.QueryString}",
                    LogArea.HTTP
                );

                #if DEBUG
                // Log post body
                if (context.Request.Method == "POST")
                {
                    context.Request.Body.Position = 0;
                    Logger.LogDebug(await new StreamReader(context.Request.Body).ReadToEndAsync(), LogArea.HTTP);
                }
                #endif
            }
        );

        // Digest check
        app.Use
        (
            async (context, next) =>
            {
                // Client digest check.
                if (!context.Request.Cookies.TryGetValue("MM_AUTH", out string authCookie)) authCookie = string.Empty;
                string digestPath = context.Request.Path;
                Stream body = context.Request.Body;

                bool usedAlternateDigestKey = false;

                if (computeDigests && digestPath.StartsWith("/LITTLEBIGPLANETPS3_XML"))
                {
                    string clientRequestDigest = await CryptoHelper.ComputeDigest
                        (digestPath, authCookie, body, ServerConfiguration.Instance.DigestKey.PrimaryDigestKey);

                    // Check the digest we've just calculated against the X-Digest-A header if the game set the header. They should match.
                    if (context.Request.Headers.TryGetValue("X-Digest-A", out StringValues sentDigest))
                    {
                        if (clientRequestDigest != sentDigest)
                        {
                            // If we got here, the normal ServerDigestKey failed to validate. Lets try again with the alternate digest key.
                            usedAlternateDigestKey = true;

                            // Reset the body stream
                            body.Position = 0;

                            clientRequestDigest = await CryptoHelper.ComputeDigest
                                (digestPath, authCookie, body, ServerConfiguration.Instance.DigestKey.AlternateDigestKey);
                            if (clientRequestDigest != sentDigest)
                            {
                                #if DEBUG
                                Console.WriteLine("Digest failed");
                                Console.WriteLine("digestKey: " + ServerConfiguration.Instance.DigestKey.PrimaryDigestKey);
                                Console.WriteLine("altDigestKey: " + ServerConfiguration.Instance.DigestKey.AlternateDigestKey);
                                Console.WriteLine("computed digest: " + clientRequestDigest);
                                #endif
                                // We still failed to validate. Abort the request.
                                context.Response.StatusCode = 403;
                                context.Abort();
                                return;
                            }
                        }
                    }

                    context.Response.Headers.Add("X-Digest-B", clientRequestDigest);
                    context.Request.Body.Position = 0;
                }

                // This does the same as above, but for the response stream.
                await using MemoryStream responseBuffer = new();
                Stream oldResponseStream = context.Response.Body;
                context.Response.Body = responseBuffer;

                await next(context); // Handle the request so we can get the server digest hash

                // Compute the server digest hash.
                if (computeDigests)
                {
                    responseBuffer.Position = 0;

                    string digestKey = usedAlternateDigestKey
                        ? ServerConfiguration.Instance.DigestKey.AlternateDigestKey
                        : ServerConfiguration.Instance.DigestKey.PrimaryDigestKey;

                    // Compute the digest for the response.
                    string serverDigest = await CryptoHelper.ComputeDigest(context.Request.Path, authCookie, responseBuffer, digestKey);
                    context.Response.Headers.Add("X-Digest-A", serverDigest);
                }

                // Set the X-Original-Content-Length header to the length of the response buffer.
                context.Response.Headers.Add("X-Original-Content-Length", responseBuffer.Length.ToString());

                // Copy the buffered response to the actual respose stream.
                responseBuffer.Position = 0;
                await responseBuffer.CopyToAsync(oldResponseStream);
                context.Response.Body = oldResponseStream;
            }
        );

        app.Use
        (
            async (context, next) =>
            {
                #nullable enable
                // Log LastContact for LBP1. This is done on LBP2/3/V on a Match request.
                if (context.Request.Path.ToString().StartsWith("/LITTLEBIGPLANETPS3_XML"))
                {
                    // We begin by grabbing a token from the request, if this is a LBPPS3_XML request of course.
                    await using Database database = new(); // Gets nuked at the end of the scope
                    GameToken? gameToken = await database.GameTokenFromRequest(context.Request);

                    if (gameToken != null && gameToken.GameVersion == GameVersion.LittleBigPlanet1)
                        // Ignore UserFromGameToken null because user must exist for a token to exist
                        await LastContactHelper.SetLastContact
                            ((await database.UserFromGameToken(gameToken))!, GameVersion.LittleBigPlanet1, gameToken.Platform);
                }
                #nullable disable

                await next(context);
            }
        );

        app.UseRouting();

        app.UseStaticFiles();

        app.UseEndpoints(endpoints => endpoints.MapControllers());
        app.UseEndpoints(endpoints => endpoints.MapRazorPages());
    }
}