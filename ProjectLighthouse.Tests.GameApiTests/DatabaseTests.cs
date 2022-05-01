using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Tests;
using LBPUnion.ProjectLighthouse.Types;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests;

public class DatabaseTests : LighthouseServerTest
{
    [DatabaseFact]
    public async Task CanCreateUserTwice()
    {
        await using Database database = new();
        int rand = new Random().Next();

        User userA = await database.CreateUser("unitTestUser" + rand, CryptoHelper.GenerateAuthToken());
        User userB = await database.CreateUser("unitTestUser" + rand, CryptoHelper.GenerateAuthToken());

        Assert.NotNull(userA);
        Assert.NotNull(userB);

        await database.RemoveUser(userA); // Only remove userA since userA and userB are the same user
    }
}