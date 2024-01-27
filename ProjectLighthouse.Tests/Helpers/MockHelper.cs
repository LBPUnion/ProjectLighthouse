using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LBPUnion.ProjectLighthouse.Tests.Helpers;

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

    public static T2 CastTo<T1, T2>(this IActionResult result) where T1 : ObjectResult
    {
        T1 typedResult = Assert.IsType<T1>(result);
        Assert.NotNull(typedResult);
        Assert.NotNull(typedResult.Value);
        T2 finalResult = Assert.IsType<T2>(typedResult.Value);
        Assert.NotNull(finalResult);
        return finalResult;
    }

    private static async Task<DbContextOptionsBuilder<DatabaseContext>> GetInMemoryDbOptions()
    {
        DbConnection connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        return new DbContextOptionsBuilder<DatabaseContext>().UseSqlite(connection);
    }

    public static async Task<DatabaseContext> GetTestDatabase(params object[] sets)
    {
        await RoomHelper.Rooms.RemoveAllAsync();

        Dictionary<Type, IList> setDict = new();
        foreach (IList list in sets)
        {
            Type? type = list.GetType().GetGenericArguments().ElementAtOrDefault(0);
            if (type == null) continue;
            setDict[type] = list;
        }

        setDict.TryAdd(typeof(GameTokenEntity), new List<GameTokenEntity>());
        setDict.TryAdd(typeof(UserEntity), new List<UserEntity>());

        // add the default user token if another token with id 1 isn't specified
        if (setDict.TryGetValue(typeof(GameTokenEntity), out IList? tokens))
        {
            if (tokens.Cast<GameTokenEntity>().FirstOrDefault(t => t.TokenId == 1) == null)
            {
                setDict[typeof(GameTokenEntity)].Add(GetUnitTestToken());
            }
        }

        // add the default user if another user with id 1 isn't specified
        if (setDict.TryGetValue(typeof(UserEntity), out IList? users))
        {
            if (users.Cast<UserEntity>().FirstOrDefault(u => u.UserId == 1) == null)
            {
                setDict[typeof(UserEntity)].Add(GetUnitTestUser());
            }
        }

        DbContextOptions<DatabaseContext> options = (await GetInMemoryDbOptions()).Options;

        await using DatabaseContext context = new(options);
        await context.Database.EnsureCreatedAsync();

        foreach (IList list in setDict.Select(p => p.Value))
        {
            foreach (object item in list)
            {
                context.Add(item);
            }
        }

        await context.SaveChangesAsync();

        return new DatabaseContext(options);
    }

    public static void SetupTestController(this ControllerBase controllerBase, string? body = null)
    {
        SetupTestController(controllerBase, GetUnitTestToken(), body);
    }

    public static void SetupTestController(this ControllerBase controllerBase, GameTokenEntity token, string? body = null)
    {
        controllerBase.ControllerContext = GetMockControllerContext(body);
        SetupTestGameToken(controllerBase, token);
    }

    public static ControllerContext GetMockControllerContext() =>
        new()
        {
            HttpContext = new DefaultHttpContext(),
        };

    private static ControllerContext GetMockControllerContext(string? body) =>
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
            ActionDescriptor = new ControllerActionDescriptor
            {
                ActionName = "",
            },
        };

    private static void SetupTestGameToken(ControllerBase controller, GameTokenEntity token)
    {
        controller.HttpContext.Items["Token"] = token;
    }
}