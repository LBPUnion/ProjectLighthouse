using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Middlewares;
using LBPUnion.ProjectLighthouse.Tests.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Unit.Middlewares;

[Trait("Category", "Unit")]
public class SetLastContactMiddlewareTests
{

    [Fact]
    public async void SetLastContact_ShouldAddLastContact_WhenTokenIsLBP1()
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
        SetLastContactMiddleware middleware = new(httpContext =>
            {
                httpContext.Response.StatusCode = 200;
                httpContext.Response.WriteAsync("");
                return Task.CompletedTask;
            });

        DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        await middleware.InvokeAsync(context, dbMock);

        const int expectedCode = 200;
        const int expectedUserId = 1;
        const GameVersion expectedGameVersion = GameVersion.LittleBigPlanet1;

        Assert.Equal(expectedCode, context.Response.StatusCode);
        LastContactEntity? lastContactEntity = dbMock.LastContacts.FirstOrDefault();
        Assert.NotNull(lastContactEntity);
        Assert.Equal(expectedUserId, lastContactEntity.UserId);
        Assert.Equal(expectedGameVersion, lastContactEntity.GameVersion);
    }

    [Fact]
    public async void SetLastContact_ShouldUpdateLastContact_WhenTokenIsLBP1()
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
        SetLastContactMiddleware middleware = new(httpContext =>
        {
            httpContext.Response.StatusCode = 200;
            httpContext.Response.WriteAsync("");
            return Task.CompletedTask;
        });

        List<LastContactEntity> lastContacts = new()
        {
            new LastContactEntity
            {
                UserId = 1,
                GameVersion = GameVersion.LittleBigPlanet1,
                Timestamp = 0,
                Platform = Platform.UnitTest,
            },
        };

        DatabaseContext dbMock = await MockHelper.GetTestDatabase(new[]{lastContacts,});

        await middleware.InvokeAsync(context, dbMock);

        const int expectedCode = 200;
        const int expectedUserId = 1;
        const int oldTimestamp = 0;
        const GameVersion expectedGameVersion = GameVersion.LittleBigPlanet1;

        Assert.Equal(expectedCode, context.Response.StatusCode);
        LastContactEntity? lastContactEntity = dbMock.LastContacts.FirstOrDefault();
        Assert.NotNull(lastContactEntity);
        Assert.Equal(expectedUserId, lastContactEntity.UserId);
        Assert.Equal(expectedGameVersion, lastContactEntity.GameVersion);
        Assert.NotEqual(oldTimestamp, lastContactEntity.Timestamp);
    }

    [Fact]
    public async void SetLastContact_ShouldNotAddLastContact_WhenTokenIsNotLBP1()
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
        SetLastContactMiddleware middleware = new(httpContext =>
        {
            httpContext.Response.StatusCode = 200;
            httpContext.Response.WriteAsync("");
            return Task.CompletedTask;
        });

        List<GameTokenEntity> tokens = new()
        {
            MockHelper.GetUnitTestToken(),
        };
        tokens[0].GameVersion = GameVersion.LittleBigPlanet2;

        DatabaseContext dbMock = await MockHelper.GetTestDatabase(new[]
        {
            tokens,
        });

        await middleware.InvokeAsync(context, dbMock);

        const int expectedCode = 200;

        Assert.Equal(expectedCode, context.Response.StatusCode);
        LastContactEntity? lastContactEntity = dbMock.LastContacts.FirstOrDefault();
        Assert.Null(lastContactEntity);
    }

}