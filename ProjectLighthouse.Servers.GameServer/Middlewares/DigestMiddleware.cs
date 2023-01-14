using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.PlayerData;
using System.IO.Compression;
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

    #if !DEBUG
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
    #endif

    public override async Task InvokeAsync(HttpContext context)
    {
        // Client digest check.
        if (!context.Request.Cookies.TryGetValue("MM_AUTH", out string? authCookie))
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
             if (context.Request.Headers.TryGetValue("User-Agent", out StringValues useragent))
             {
                if (useragent == "MM CHTTPClient $Id: HTTPClient.cpp 36247 2009-11-24 16:17:36Z paul $" || "MM CHTTPClient LBP2 01.00" || "MM CHTTPClient LBP2 01.02" || "MM CHTTPClient LBP2 01.03" || "MM CHTTPClient LBP2 01.04" || "MM CHTTPClient LBP2 01.05" || "MM CHTTPClient LBP2 01.06" || "MM CHTTPClient LBP2 01.07" || "MM CHTTPClient LBP2 01.08" || "MM CHTTPClient LBP2 01.09" || "MM CHTTPClient LBP2 01.10" || "MM CHTTPClient LBP2 01.11" || "MM CHTTPClient LBP2 01.12" || "MM CHTTPClient LBP2 01.13" || "MM CHTTPClient LBP2 01.14" || "MM CHTTPClient LBP2 01.15" || "MM CHTTPClient LBP2 01.16" || "MM CHTTPClient LBP2 01.17" || "MM CHTTPClient LBP2 01.18" || "MM CHTTPClient LBP2 01.19" || "MM CHTTPClient LBP2 01.20" || "MM CHTTPClient LBP2 01.21" || "MM CHTTPClient LBP2 01.22" || "MM CHTTPClient LBP2 01.23" || "MM CHTTPClient LBP2 01.24" || "MM CHTTPClient LBP2 01.25" || "MM CHTTPClient LBP2 01.27" || "MM CHTTPClient LBP2 01.28" || "MM CHTTPClient LBP2 01.29" || "MM CHTTPClient LBP2 01.30" || "MM CHTTPClient LBP2 01.31" || "MM CHTTPClient LBP2 01.32") {
                    context.Response.StatusCode = 403;
                    return;
                }
             }


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

        if (responseBuffer.Length > 1000 && context.Request.Headers.AcceptEncoding.Contains("deflate") && (context.Response.ContentType ?? string.Empty).Contains("text/xml"))
        {
            context.Response.Headers.Add("X-Original-Content-Length", responseBuffer.Length.ToString());
            context.Response.Headers.Add("Vary", "Accept-Encoding");
            MemoryStream resultStream = new();
            await using ZLibStream stream = new(resultStream, CompressionMode.Compress, true);
            await stream.WriteAsync(responseBuffer.ToArray());
            stream.Close();
        
            resultStream.Position = 0;
            context.Response.Headers.Add("Content-Length", resultStream.Length.ToString());
            context.Response.Headers.Add("Content-Encoding", "deflate");
            responseBuffer.SetLength(0);
            await resultStream.CopyToAsync(responseBuffer);
        }
        else
        {
            string headerName = !context.Response.Headers.ContentLength.HasValue
                ? "Content-Length"
                : "X-Original-Content-Length";
            context.Response.Headers.Add(headerName, responseBuffer.Length.ToString());
        }

        // Copy the buffered response to the actual response stream.
        responseBuffer.Position = 0;
        await responseBuffer.CopyToAsync(oldResponseStream);
        context.Response.Body = oldResponseStream;
    }
}