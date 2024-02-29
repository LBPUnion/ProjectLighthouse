using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Middlewares;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Types;
using Microsoft.Extensions.Primitives;
using Org.BouncyCastle.Utilities.Zlib;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Middlewares;

public class DigestMiddleware : Middleware
{
    private readonly bool computeDigests;

    public DigestMiddleware(RequestDelegate next, bool computeDigests) : base(next)
    {
        this.computeDigests = computeDigests;
    }

    private readonly List<string> digestKeys;

    public DigestMiddleware(RequestDelegate next, List<string> digestKeys) : base(next)
    {
        this.digestKeys = digestKeys;
    }

    private static async Task HandleResponseCompression(HttpContext context, MemoryStream responseBuffer)
    {
        const int minCompressionLen = 1000;
        if (responseBuffer.Length > minCompressionLen &&
            context.Request.Headers.AcceptEncoding.Contains("deflate") &&
            (context.Response.ContentType ?? string.Empty).Contains("text/xml"))
        {
            context.Response.Headers.Append("X-Original-Content-Length", responseBuffer.Length.ToString());
            context.Response.Headers.Append("Vary", "Accept-Encoding");
            MemoryStream resultStream = new();
            const int defaultCompressionLevel = 6;
            await using ZOutputStreamLeaveOpen stream = new(resultStream, defaultCompressionLevel);
            await stream.WriteAsync(responseBuffer.ToArray());
            stream.Finish();

            resultStream.Position = 0;
            context.Response.Headers.Append("Content-Length", resultStream.Length.ToString());
            context.Response.Headers.Append("Content-Encoding", "deflate");
            responseBuffer.SetLength(0);
            await resultStream.CopyToAsync(responseBuffer);
        }
        else
        {
            string headerName = !context.Response.Headers.ContentLength.HasValue ? "Content-Length" : "X-Original-Content-Length";
            context.Response.Headers.Append(headerName, responseBuffer.Length.ToString());
        }
    }

    public override async Task InvokeAsync(HttpContext context)
    {
        UseDigestAttribute? digestAttribute = context.GetEndpoint()?.Metadata.OfType<UseDigestAttribute>().FirstOrDefault();
        if (digestAttribute == null)
        {
            await this.next(context);
            return;
        }

        if (!context.Request.Cookies.TryGetValue("MM_AUTH", out string? authCookie))
        {
            context.Response.StatusCode = 403;
            return;
        }

        string digestPath = context.Request.Path;

        byte[] bodyBytes = await context.Request.BodyReader.ReadAllAsync();

        if (!context.Request.Headers.TryGetValue(digestAttribute.DigestHeaderName, out StringValues digestHeaders) ||
            digestHeaders.Count != 1 && digestAttribute.EnforceDigest)
        {
            context.Response.StatusCode = 403;
            return;
        }

        string? clientDigest = digestHeaders[0];

        string? matchingDigestKey = null;
        string? calculatedRequestDigest = null;

        foreach (string digestKey in this.digestKeys)
        {
            string calculatedDigest = CryptoHelper.ComputeDigest(digestPath,
                authCookie,
                bodyBytes,
                digestKey,
                digestAttribute.ExcludeBodyFromDigest);
            if (calculatedDigest != clientDigest) continue;

            matchingDigestKey = digestKey;
            calculatedRequestDigest = calculatedDigest;
        }

        matchingDigestKey ??= this.digestKeys.First();

        switch (matchingDigestKey)
        {
            case null when digestAttribute.EnforceDigest:
                context.Response.StatusCode = 403;
                return;
            case null:
                calculatedRequestDigest = CryptoHelper.ComputeDigest(digestPath,
                    authCookie,
                    bodyBytes,
                    matchingDigestKey,
                    digestAttribute.ExcludeBodyFromDigest);
                break;
        }

        context.Response.Headers.Append("X-Digest-B", calculatedRequestDigest);
        // context.Request.Body.Position = 0;

        // Let endpoint generate response so we can calculate the digest for it
        Stream originalBody = context.Response.Body;
        await using MemoryStream responseBuffer = new();
        context.Response.Body = responseBuffer;

        await this.next(context);

        await HandleResponseCompression(context, responseBuffer);

        string responseDigest = CryptoHelper.ComputeDigest(digestPath,
            authCookie,
            responseBuffer.ToArray(),
            matchingDigestKey,
            digestAttribute.ExcludeBodyFromDigest);

        context.Response.Headers.Append("X-Digest-A", responseDigest);

        responseBuffer.Position = 0;
        await responseBuffer.CopyToAsync(originalBody);
        context.Response.Body = originalBody;
    }

}