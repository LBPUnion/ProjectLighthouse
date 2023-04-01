using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.EntityFrameworkCore;

namespace ProjectLighthouse.Tests.GameApiTests.Unit;

public static class MockHelper
{

    public static UserEntity GetUnitTestUser() =>
        new()
        {
            Username = "unittest",
            UserId = 1,
        };

    public static GameTokenEntity GetUnitTestToken() =>
        new()
        {
            Platform = Platform.UnitTest,
            UserId = 1,
            ExpiresAt = DateTime.MaxValue,
            TokenId = 1,
            UserLocation = "127.0.0.1",
            UserToken = "unittest",
        };

    public static Mock<DatabaseContext> GetDatabaseMock(List<UserEntity>? users = null, List<GameTokenEntity>? tokens = null)
    {
        users ??= new List<UserEntity>
        {
            GetUnitTestUser(),
        };

        tokens ??= new List<GameTokenEntity>
        {
            GetUnitTestToken(),
        };
        Mock<DatabaseContext> mock = new();
        mock.SetupGet(x => x.Users).ReturnsDbSet(users);
        mock.SetupGet(x => x.GameTokens).ReturnsDbSet(tokens);
        mock.Setup(x => x.Users.FindAsync(It.IsAny<object[]>()))
            .Returns<object[]>(async objects =>
                await Task.FromResult(users.FirstOrDefault(u => u.UserId == (int)objects[0])));
        return mock;
    }

    public static void SetupTestController(this ControllerBase controllerBase, string? body = null)
    {
        controllerBase.ControllerContext = GetMockControllerContext(body);
        SetupTestGameToken(controllerBase, GetUnitTestToken());
    }

    public static ControllerContext GetMockControllerContext() =>
        new()
        {
            HttpContext = new DefaultHttpContext(),
        };

    public static ControllerContext GetMockControllerContext(string? body) =>
        new()
        {
            HttpContext = new DefaultHttpContext
            {
                Request =
                {
                    ContentLength = body?.Length ?? 0,
                    Body = new MemoryStream(Encoding.ASCII.GetBytes(body ?? string.Empty)),
                },
            },
        };

    public static void SetupTestGameToken(ControllerBase controller, GameTokenEntity token)
    {
        controller.HttpContext.Items["Token"] = token;
    }
}