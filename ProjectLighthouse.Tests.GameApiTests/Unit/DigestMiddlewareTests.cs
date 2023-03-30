using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Unit;

[Trait("Category", "Unit")]
public class DigestMiddlewareTests
{

    [Fact]
    public async void DigestMiddleware_ShouldNotComputeDigests_WhenDigestsDisabled()
    {
        DefaultHttpContext context = new()
        {
            Request =
            {
                Body = new MemoryStream(),
                Path = "/LITTLEBIGPLANETPS3_XML/notification",
                Headers = { KeyValuePair.Create<string, StringValues>("Cookie", "MM_AUTH=unittest"), },
            },
        };
        DigestMiddleware middleware = new(httpContext =>
        {
            httpContext.Response.StatusCode = 200;
            httpContext.Response.WriteAsync("");
            return Task.CompletedTask;
        }, false);

        await middleware.InvokeAsync(context);

        const int expectedCode = 200;

        Assert.Equal(expectedCode, context.Response.StatusCode);
        Assert.Empty(context.Response.Headers["X-Digest-A"]);
        Assert.Empty(context.Response.Headers["X-Digest-B"]);
    }

    [Fact]
    public async void DigestMiddleware_ShouldNotReject_WhenRequestingAnnounce()
    {
        DefaultHttpContext context = new()
        {
            Request =
            {
                Body = new MemoryStream(),
                Path = "/LITTLEBIGPLANETPS3_XML/announce",
                Headers =
                {
                    KeyValuePair.Create<string, StringValues>("Cookie", "MM_AUTH=unittest"),
                },
            },
        };
        ServerConfiguration.Instance.DigestKey.PrimaryDigestKey = "bruh";
        DigestMiddleware middleware = new(httpContext =>
            {
                httpContext.Response.StatusCode = 200;
                httpContext.Response.WriteAsync("");
                return Task.CompletedTask;
            },
            true);

        await middleware.InvokeAsync(context);

        const int expectedCode = 200;
        const string expectedClientDigest = "9243acecfa83ac25bdfefe97f5681b439c003f1e";
        const string expectedServerDigest = "9243acecfa83ac25bdfefe97f5681b439c003f1e";

        Assert.Equal(expectedCode, context.Response.StatusCode);
        Assert.NotEmpty(context.Response.Headers["X-Digest-A"]);
        Assert.NotEmpty(context.Response.Headers["X-Digest-B"]);
        Assert.Equal(expectedClientDigest, context.Response.Headers["X-Digest-A"]);
        Assert.Equal(expectedServerDigest, context.Response.Headers["X-Digest-B"]);
    }

    [Fact]
    public async void DigestMiddleware_ShouldReject_WhenRequestDigestInvalid()
    {
        DefaultHttpContext context = new()
        {
            Request =
            {
                Body = new MemoryStream(),
                Path = "/LITTLEBIGPLANETPS3_XML/notification",
                Headers =
                {
                    KeyValuePair.Create<string, StringValues>("Cookie", "MM_AUTH=unittest"),
                    KeyValuePair.Create<string, StringValues>("X-Digest-A", "df619790a2579a077eae4a6b6864966ff4768724"),
                },
            },
        };
        ServerConfiguration.Instance.DigestKey.PrimaryDigestKey = "bruh";
        DigestMiddleware middleware = new(httpContext =>
            {
                httpContext.Response.StatusCode = 200;
                httpContext.Response.WriteAsync("");
                return Task.CompletedTask;
            },
            true);

        await middleware.InvokeAsync(context);

        const int expectedCode = 403;

        Assert.Equal(expectedCode, context.Response.StatusCode);
        Assert.Empty(context.Response.Headers["X-Digest-A"]);
        Assert.Empty(context.Response.Headers["X-Digest-B"]);
    }

    [Fact]
    public async void DigestMiddleware_ShouldComputeDigestsWithNoBody_WhenDigestsEnabled()
    {
        DefaultHttpContext context = new()
        {
            Request =
            {
                Body = new MemoryStream(),
                Path = "/LITTLEBIGPLANETPS3_XML/notification",
                Headers =
                {
                    KeyValuePair.Create<string, StringValues>("Cookie", "MM_AUTH=unittest"),
                },
            },
        };
        ServerConfiguration.Instance.DigestKey.PrimaryDigestKey = "bruh";
        DigestMiddleware middleware = new(httpContext =>
            {
                httpContext.Response.StatusCode = 200;
                httpContext.Response.WriteAsync("");
                return Task.CompletedTask;
            },
            true);

        await middleware.InvokeAsync(context);

        const int expectedCode = 200;
        const string expectedClientDigest = "df619790a2579a077eae4a6b6864966ff4768723";
        const string expectedServerDigest = "df619790a2579a077eae4a6b6864966ff4768723";

        Assert.Equal(expectedCode, context.Response.StatusCode);
        Assert.NotEmpty(context.Response.Headers["X-Digest-A"]);
        Assert.NotEmpty(context.Response.Headers["X-Digest-B"]);
        Assert.Equal(expectedClientDigest, context.Response.Headers["X-Digest-A"][0]);
        Assert.Equal(expectedServerDigest, context.Response.Headers["X-Digest-B"][0]);
    }
    
}