using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Unit.Middlewares;

[Trait("Category", "Unit")]
public class DigestMiddlewareTests
{

    [Fact]
    public async Task DigestMiddleware_ShouldNotComputeDigests_WhenDigestsDisabled()
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
        Assert.False(context.Response.Headers.TryGetValue("X-Digest-A", out _));
        Assert.False(context.Response.Headers.TryGetValue("X-Digest-B", out _));
    }

    [Fact]
    public async Task DigestMiddleware_ShouldReject_WhenDigestHeaderIsMissing()
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

        const int expectedCode = 403;

        Assert.True(expectedCode == context.Response.StatusCode,
            "The digest middleware accepted the request when it shouldn't have (are you running this test in Debug mode?)");
        Assert.False(context.Response.Headers.TryGetValue("X-Digest-A", out _));
        Assert.False(context.Response.Headers.TryGetValue("X-Digest-B", out _));
    }

    [Fact]
    public async Task DigestMiddleware_ShouldReject_WhenRequestDigestInvalid()
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
                    KeyValuePair.Create<string, StringValues>("X-Digest-A", "invalid_digest"),
                },
            },
        };
        ServerConfiguration.Instance.DigestKey.PrimaryDigestKey = "bruh";
        ServerConfiguration.Instance.DigestKey.AlternateDigestKey = "test";
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
        Assert.False(context.Response.Headers.TryGetValue("X-Digest-A", out _));
        Assert.False(context.Response.Headers.TryGetValue("X-Digest-B", out _));
    }

    [Fact]
    public async Task DigestMiddleware_ShouldUseAlternateDigest_WhenPrimaryDigestInvalid()
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
                    KeyValuePair.Create<string, StringValues>("X-Digest-A", "df619790a2579a077eae4a6b6864966ff4768723"),
                },
            },
        };
        ServerConfiguration.Instance.DigestKey.PrimaryDigestKey = "test";
        ServerConfiguration.Instance.DigestKey.AlternateDigestKey = "bruh";
        DigestMiddleware middleware = new(httpContext =>
            {
                httpContext.Response.StatusCode = 200;
                httpContext.Response.WriteAsync("");
                return Task.CompletedTask;
            },
            true);

        await middleware.InvokeAsync(context);

        const int expectedCode = 200;
        const string expectedServerDigest = "df619790a2579a077eae4a6b6864966ff4768723";
        const string expectedClientDigest = "df619790a2579a077eae4a6b6864966ff4768723";

        Assert.Equal(expectedCode, context.Response.StatusCode);
        Assert.True(context.Response.Headers.TryGetValue("X-Digest-A", out _));
        Assert.True(context.Response.Headers.TryGetValue("X-Digest-B", out _));
        Assert.Equal(expectedServerDigest, context.Response.Headers["X-Digest-A"][0]);
        Assert.Equal(expectedClientDigest, context.Response.Headers["X-Digest-B"][0]);
    }

    [Fact]
    public async Task DigestMiddleware_ShouldNotReject_WhenRequestingAnnounce()
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
        const string expectedServerDigest = "9243acecfa83ac25bdfefe97f5681b439c003f1e";
        const string expectedClientDigest = "9243acecfa83ac25bdfefe97f5681b439c003f1e";

        Assert.Equal(expectedCode, context.Response.StatusCode);
        Assert.True(context.Response.Headers.TryGetValue("X-Digest-A", out _));
        Assert.True(context.Response.Headers.TryGetValue("X-Digest-B", out _));
        Assert.Equal(expectedServerDigest, context.Response.Headers["X-Digest-A"]);
        Assert.Equal(expectedClientDigest, context.Response.Headers["X-Digest-B"]);
    }

    [Fact]
    public async Task DigestMiddleware_ShouldCalculate_WhenAuthCookieEmpty()
    {
        DefaultHttpContext context = new()
        {
            Request =
            {
                Body = new MemoryStream(),
                Path = "/LITTLEBIGPLANETPS3_XML/notification",
                Headers =
                {
                    KeyValuePair.Create<string, StringValues>("X-Digest-A", "0a06d25662c2d3bab2a767c0c504898df2385e62"),
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
        const string expectedServerDigest = "0a06d25662c2d3bab2a767c0c504898df2385e62";
        const string expectedClientDigest = "0a06d25662c2d3bab2a767c0c504898df2385e62";

        Assert.Equal(expectedCode, context.Response.StatusCode);
        Assert.True(context.Response.Headers.TryGetValue("X-Digest-A", out _));
        Assert.True(context.Response.Headers.TryGetValue("X-Digest-B", out _));
        Assert.Equal(expectedServerDigest, context.Response.Headers["X-Digest-A"][0]);
        Assert.Equal(expectedClientDigest, context.Response.Headers["X-Digest-B"][0]);
    }

    [Fact]
    public async Task DigestMiddleware_ShouldComputeDigestsWithNoBody_WhenDigestsEnabled()
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
                    KeyValuePair.Create<string, StringValues>("X-Digest-A", "df619790a2579a077eae4a6b6864966ff4768723"),
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
        const string expectedServerDigest = "df619790a2579a077eae4a6b6864966ff4768723";
        const string expectedClientDigest = "df619790a2579a077eae4a6b6864966ff4768723";

        Assert.Equal(expectedCode, context.Response.StatusCode);
        Assert.True(context.Response.Headers.TryGetValue("X-Digest-A", out _));
        Assert.True(context.Response.Headers.TryGetValue("X-Digest-B", out _));
        Assert.Equal(expectedServerDigest, context.Response.Headers["X-Digest-A"][0]);
        Assert.Equal(expectedClientDigest, context.Response.Headers["X-Digest-B"][0]);
    }

    [Fact]
    public async Task DigestMiddleware_ShouldComputeDigestsWithBody_WhenDigestsEnabled_AndNoResponseBody()
    {
        DefaultHttpContext context = new()
        {
            Request =
            {
                Body = new MemoryStream("digest test"u8.ToArray()),
                Path = "/LITTLEBIGPLANETPS3_XML/filter",
                Headers =
                {
                    KeyValuePair.Create<string, StringValues>("Cookie", "MM_AUTH=unittest"),
                    KeyValuePair.Create<string, StringValues>("X-Digest-A", "3105059f9283773f7982a4d79455bcc97c330f10"),
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
        const string expectedServerDigest = "c87ef375f095d36369bb6d9689220fd0ce0e0d4b";
        const string expectedClientDigest = "3105059f9283773f7982a4d79455bcc97c330f10";

        Assert.Equal(expectedCode, context.Response.StatusCode);
        Assert.True(context.Response.Headers.TryGetValue("X-Digest-A", out _));
        Assert.True(context.Response.Headers.TryGetValue("X-Digest-B", out _));
        Assert.Equal(expectedServerDigest, context.Response.Headers["X-Digest-A"][0]);
        Assert.Equal(expectedClientDigest, context.Response.Headers["X-Digest-B"][0]);
    }

    [Fact]
    public async Task DigestMiddleware_ShouldComputeDigestsWithBody_WhenDigestsEnabled_AndResponseBody()
    {
        DefaultHttpContext context = new()
        {
            Request =
            {
                Body = new MemoryStream("digest test"u8.ToArray()),
                Path = "/LITTLEBIGPLANETPS3_XML/filter",
                Headers =
                {
                    KeyValuePair.Create<string, StringValues>("Cookie", "MM_AUTH=unittest"),
                    KeyValuePair.Create<string, StringValues>("X-Digest-A", "3105059f9283773f7982a4d79455bcc97c330f10"),
                },
            },
        };
        ServerConfiguration.Instance.DigestKey.PrimaryDigestKey = "bruh";
        DigestMiddleware middleware = new(httpContext =>
            {
                httpContext.Response.StatusCode = 200;
                httpContext.Response.WriteAsync("digest test");
                return Task.CompletedTask;
            },
            true);

        await middleware.InvokeAsync(context);

        const int expectedCode = 200;
        const string expectedServerDigest = "3105059f9283773f7982a4d79455bcc97c330f10";
        const string expectedClientDigest = "3105059f9283773f7982a4d79455bcc97c330f10";

        Assert.Equal(expectedCode, context.Response.StatusCode);
        Assert.True(context.Response.Headers.TryGetValue("X-Digest-A", out _));
        Assert.True(context.Response.Headers.TryGetValue("X-Digest-B", out _));
        Assert.Equal(expectedServerDigest, context.Response.Headers["X-Digest-A"][0]);
        Assert.Equal(expectedClientDigest, context.Response.Headers["X-Digest-B"][0]);
    }

    [Fact]
    public async Task DigestMiddleware_ShouldComputeDigestsWithBody_WhenUploading()
    {
        DefaultHttpContext context = new()
        {
            Request =
            {
                Body = new MemoryStream("digest test"u8.ToArray()),
                Path = "/LITTLEBIGPLANETPS3_XML/upload/unittesthash",
                Headers =
                {
                    KeyValuePair.Create<string, StringValues>("Cookie", "MM_AUTH=unittest"),
                    KeyValuePair.Create<string, StringValues>("X-Digest-B", "2e54cd2bc69ff8c1ff85dd3b4f62e0a0e27d9e23"),
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
        const string expectedServerDigest = "2e54cd2bc69ff8c1ff85dd3b4f62e0a0e27d9e23";
        const string expectedClientDigest = "2e54cd2bc69ff8c1ff85dd3b4f62e0a0e27d9e23";

        Assert.Equal(expectedCode, context.Response.StatusCode);
        Assert.True(context.Response.Headers.TryGetValue("X-Digest-A", out _));
        Assert.True(context.Response.Headers.TryGetValue("X-Digest-B", out _));
        Assert.Equal(expectedServerDigest, context.Response.Headers["X-Digest-A"][0]);
        Assert.Equal(expectedClientDigest, context.Response.Headers["X-Digest-B"][0]);
    }

    [Fact]
    public async Task DigestMiddleware_ShouldCompressResponse_WhenAcceptEncodingHeaderIsPresent()
    {
        DefaultHttpContext context = new()
        {
            Response =
            {
                Body = new MemoryStream(),
            },
            Request =
            {
                Body = new MemoryStream(),
                Path = "/LITTLEBIGPLANETPS3_XML/r/testing",
                Headers =
                {
                    KeyValuePair.Create<string, StringValues>("Cookie", "MM_AUTH=unittest"),
                    KeyValuePair.Create<string, StringValues>("X-Digest-A", "80714c0936408855d86d47a650320f91895812d0"),
                    KeyValuePair.Create<string, StringValues>("Accept-Encoding", "deflate"),
                },
            },
        };
        ServerConfiguration.Instance.DigestKey.PrimaryDigestKey = "bruh";
        DigestMiddleware middleware = new(httpContext =>
            {
                httpContext.Response.StatusCode = 200;
                httpContext.Response.WriteAsync(new string('a', 1000 * 2));
                httpContext.Response.Headers.ContentType = "text/xml";
                return Task.CompletedTask;
            },
            true);

        await middleware.InvokeAsync(context);

        const int expectedCode = 200;
        const string expectedServerDigest = "404e589cafbff7886fe9fc5ee8a5454b57d9cb50";
        const string expectedClientDigest = "80714c0936408855d86d47a650320f91895812d0";
        const string expectedContentLen = "2000";
        const string expectedContentEncoding = "deflate";
        const string expectedCompressedContentLen = "23";
        const string expectedData = "783F4B4C1C053F60143F3F51300A463F500700643F3F3F";

        context.Response.Body.Position = 0;
        string output = await new StreamReader(context.Response.Body).ReadToEndAsync();
        string outputBytes = Convert.ToHexString(Encoding.ASCII.GetBytes(output)); 

        Assert.Equal(expectedCode, context.Response.StatusCode);
        Assert.True(context.Response.Headers.TryGetValue("X-Digest-A", out _));
        Assert.True(context.Response.Headers.TryGetValue("X-Digest-B", out _));
        Assert.True(context.Response.Headers.TryGetValue("X-Original-Content-Length", out _));
        Assert.Equal(expectedContentEncoding, context.Response.Headers.ContentEncoding);
        Assert.Equal(expectedServerDigest, context.Response.Headers["X-Digest-A"][0]);
        Assert.Equal(expectedClientDigest, context.Response.Headers["X-Digest-B"][0]);
        Assert.Equal(expectedContentLen, context.Response.Headers["X-Original-Content-Length"][0]);
        Assert.Equal(expectedCompressedContentLen, context.Response.Headers["Content-Length"][0]);
        Assert.Equal(expectedData, outputBytes);
    }
    
}