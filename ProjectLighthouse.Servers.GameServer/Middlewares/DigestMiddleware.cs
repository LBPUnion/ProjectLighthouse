using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Middlewares;
using Microsoft.Extensions.Primitives;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Middlewares;

public class DigestMiddleware : Middleware
{
    private readonly bool computeDigests;

    public DigestMiddleware(RequestDelegate next, bool computeDigests) : base(next)
    {
        this.computeDigests = computeDigests;
    }

    private static readonly HashSet<string> exemptPathList = new()
    {
        "/login",
        "/eula",
        "/announce",
        "/status",
        "/farc_hashes",
        "/t_conf",
        "/network_settings.nws",
        "/ChallengeConfig.xml",
    };

    public override async Task InvokeAsync(HttpContext context)
    {
        // Client digest check.
        if (!context.Request.Cookies.TryGetValue("MM_AUTH", out string? authCookie) || authCookie == null)
            authCookie = string.Empty;
        string digestPath = context.Request.Path;
        #if !DEBUG
        const string url = "/LITTLEBIGPLANETPS3_XML";
        string strippedPath = digestPath.Contains(url) ? digestPath[url.Length..] : "";
        #endif
        Stream body = context.Request.Body;

        bool usedAlternateDigestKey = false;

        if (this.computeDigests && digestPath.StartsWith("/LITTLEBIGPLANETPS3_XML"))
        {
            // The game sets X-Digest-B on a resource upload instead of X-Digest-A
            string digestHeaderKey = "X-Digest-A";
            bool excludeBodyFromDigest = false;
            if (digestPath.Contains("/upload/"))
            {
                digestHeaderKey = "X-Digest-B";
                excludeBodyFromDigest = true;
            }

            string clientRequestDigest = await CryptoHelper.ComputeDigest
                (digestPath, authCookie, body, ServerConfiguration.Instance.DigestKey.PrimaryDigestKey, excludeBodyFromDigest);

            // Check the digest we've just calculated against the digest header if the game set the header. They should match.
            if (context.Request.Headers.TryGetValue(digestHeaderKey, out StringValues sentDigest))
            {
                if (clientRequestDigest != sentDigest)
                {
                    // If we got here, the normal ServerDigestKey failed to validate. Lets try again with the alternate digest key.
                    usedAlternateDigestKey = true;

                    // Reset the body stream
                    body.Position = 0;

                    clientRequestDigest = await CryptoHelper.ComputeDigest
                        (digestPath, authCookie, body, ServerConfiguration.Instance.DigestKey.AlternateDigestKey, excludeBodyFromDigest);
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
                        return;
                    }
                }
            }
            #if !DEBUG
            // The game doesn't start sending digests until after the announcement so if it's not one of those requests
            // and it doesn't include a digest we need to reject the request 
            else if (!ServerStatics.IsUnitTesting && !exemptPathList.Contains(strippedPath))
            {
                context.Response.StatusCode = 403;
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

        await this.next(context); // Handle the request so we can get the server digest hash
        responseBuffer.Position = 0;

        // Compute the server digest hash.
        if (this.computeDigests)
        {
            responseBuffer.Position = 0;

            string digestKey = usedAlternateDigestKey
                ? ServerConfiguration.Instance.DigestKey.AlternateDigestKey
                : ServerConfiguration.Instance.DigestKey.PrimaryDigestKey;

            // Compute the digest for the response.
            string serverDigest = await CryptoHelper.ComputeDigest(context.Request.Path, authCookie, responseBuffer, digestKey);
            context.Response.Headers.Add("X-Digest-A", serverDigest);
        }

        // Add a content-length header if it isn't present to disable response chunking
        if (!context.Response.Headers.ContainsKey("Content-Length"))
            context.Response.Headers.Add("Content-Length", responseBuffer.Length.ToString());

        // Copy the buffered response to the actual response stream.
        responseBuffer.Position = 0;
        await responseBuffer.CopyToAsync(oldResponseStream);
        context.Response.Body = oldResponseStream;
    }
}