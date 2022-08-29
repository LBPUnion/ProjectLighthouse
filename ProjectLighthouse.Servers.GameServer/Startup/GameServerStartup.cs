using System.IO.Compression;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Middlewares;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.Serialization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Primitives;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Startup;

public class GameServerStartup
{
    public GameServerStartup(IConfiguration configuration)
    {
        this.Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

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
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        bool computeDigests = true;

        if (string.IsNullOrEmpty(ServerConfiguration.Instance.DigestKey.PrimaryDigestKey))
        {
            Logger.Warn
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

        app.UseMiddleware<RequestLogMiddleware>();

        // Digest check
        app.Use
        (
            async (context, next) =>
            {
                // Client digest check.
                if (!context.Request.Cookies.TryGetValue("MM_AUTH", out string? authCookie) || authCookie == null) authCookie = string.Empty;
                string digestPath = context.Request.Path;
                #if !DEBUG
                const string url = "/LITTLEBIGPLANETPS3_XML";
                string strippedPath = digestPath.Contains(url) ? digestPath[url.Length..] : "";
                #endif
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
                    #if !DEBUG
                    // The game doesn't start sending digests until after the announcement so if it's not one of those requests
                    // and it doesn't include a digest we need to reject the request 
                    else if (!ServerStatics.IsUnitTesting && (!strippedPath.Equals("/login") && !strippedPath.Equals("/eula") && !strippedPath.Equals("/announce")))
                    {
                        context.Response.StatusCode = 403;
                        context.Abort();
                        return;
                    }
                    #endif

                    context.Response.Headers.Add("X-Digest-B", clientRequestDigest);
                    context.Request.Body.Position = 0;
                }

                // This does the same as above, but for the response stream.
                await using MemoryStream responseBuffer = new();
                Stream oldResponseStream = context.Response.Body;
                context.Response.Body = responseBuffer;

                await next(context); // Handle the request so we can get the server digest hash
                responseBuffer.Position = 0;

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

                // Copy the buffered response to the actual response stream.
                context.Response.Headers.Add("Content-Length", responseBuffer.Length.ToString());
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
                            (database, (await database.UserFromGameToken(gameToken))!, GameVersion.LittleBigPlanet1, gameToken.Platform);
                }
                #nullable disable

                await next(context);
            }
        );

        app.UseRouting();

        app.UseEndpoints(endpoints => endpoints.MapControllers());
        app.UseEndpoints(endpoints => endpoints.MapRazorPages());
    }
}
