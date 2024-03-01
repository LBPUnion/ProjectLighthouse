using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Middlewares;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Types;
using Microsoft.Extensions.Primitives;
using Org.BouncyCastle.Utilities.Zlib;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Middlewares;

public class DigestMiddleware : Middleware
{
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
        // If no digest keys are supplied, then we can't do anything
        if (this.digestKeys.Count == 0)
        {
            await this.next(context);
            return;
        }

        UseDigestAttribute? digestAttribute = context.GetEndpoint()?.Metadata.GetMetadata<UseDigestAttribute>();
        if (digestAttribute == null)
        {
            await this.next(context);
            return;
        }

        if (!context.Request.Cookies.TryGetValue("MM_AUTH", out string? authCookie)) authCookie = string.Empty;

        string digestPath = context.Request.Path;

        byte[] bodyBytes = await context.Request.BodyReader.ReadAllAsync();

        if ((!context.Request.Headers.TryGetValue(digestAttribute.DigestHeaderName, out StringValues digestHeaders) ||
            digestHeaders.Count != 1) && digestAttribute.EnforceDigest)
        {
            context.Response.StatusCode = 403;
            return;
        }

        string? clientDigest = digestHeaders.FirstOrDefault() ?? null;

        string? matchingDigestKey = null;
        string? calculatedRequestDigest = null;

        if (clientDigest != null)
        {
            foreach (string digestKey in this.digestKeys)
            {
                string calculatedDigest = CalculateDigest(digestKey, bodyBytes);
                if (calculatedDigest != clientDigest) continue;

                matchingDigestKey = digestKey;
                calculatedRequestDigest = calculatedDigest;
            }
        }

        matchingDigestKey ??= this.digestKeys.First();

        switch (calculatedRequestDigest)
        {
            case null when digestAttribute.EnforceDigest:
                context.Response.StatusCode = 403;
                return;
            case null:
                calculatedRequestDigest = CalculateDigest(matchingDigestKey, bodyBytes);
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

        string responseDigest = CalculateDigest(matchingDigestKey, responseBuffer.ToArray());

        context.Response.Headers.Append("X-Digest-A", responseDigest);

        responseBuffer.Position = 0;
        await responseBuffer.CopyToAsync(originalBody);
        context.Response.Body = originalBody;
        return;

        string CalculateDigest(string digestKey, byte[] data) =>
            CryptoHelper.ComputeDigest(digestPath,
                authCookie,
                data,
                digestKey,
                digestAttribute.ExcludeBodyFromDigest);
    }

}