using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;

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

    public static async Task<DatabaseContext> GetTestDatabase(IEnumerable<IList> sets, [CallerMemberName] string caller = "", [CallerLineNumber] int lineNum = 0)
    {
        Dictionary<Type, IList> setDict = new();
        foreach (IList list in sets)
        {
            Type? type = list.GetType().GetGenericArguments().ElementAtOrDefault(0);
            if (type == null) continue;
            setDict[type] = list;
        }

        if (!setDict.TryGetValue(typeof(GameTokenEntity), out _))
        {
            setDict[typeof(GameTokenEntity)] = new List<GameTokenEntity>
            {
                GetUnitTestToken(),
            };
        }

        if (!setDict.TryGetValue(typeof(UserEntity), out _))
        {
            setDict[typeof(UserEntity)] = new List<UserEntity>
            {
                GetUnitTestUser(),
            };
        }


        DbContextOptions<DatabaseContext> options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase($"{caller}_{lineNum}")
            .Options;

        await using DatabaseContext context = new(options);
        foreach (IList list in setDict.Select(p => p.Value))
        {
            foreach (object item in list)
            {
                context.Add(item);
            }
        }


        await context.SaveChangesAsync();
        await context.DisposeAsync();
        return new DatabaseContext(options);
    }

    public static async Task<DatabaseContext> GetTestDatabase(List<UserEntity>? users = null, List<GameTokenEntity>? tokens = null,
        [CallerMemberName] string caller = "", [CallerLineNumber] int lineNum = 0
    )
    {
        users ??= new List<UserEntity>
        {
            GetUnitTestUser(),
        };

        tokens ??= new List<GameTokenEntity>
        {
            GetUnitTestToken(),
        };
        DbContextOptions<DatabaseContext> options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase($"{caller}_{lineNum}")
            .Options;
        await using DatabaseContext context = new(options);
        context.Users.AddRange(users);
        context.GameTokens.AddRange(tokens);
        await context.SaveChangesAsync();
        await context.DisposeAsync();
        return new DatabaseContext(options);
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