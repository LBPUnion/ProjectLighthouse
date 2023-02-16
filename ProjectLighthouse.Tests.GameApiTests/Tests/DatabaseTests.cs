using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Startup;
using LBPUnion.ProjectLighthouse.Tests;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Tests;

public class DatabaseTests : LighthouseServerTest<GameServerTestStartup>
{
    [DatabaseFact]
    public async Task CanCreateUserTwice()
    {
        await using DatabaseContext database = new();
        int rand = new Random().Next();

        User userA = await database.CreateUser("unitTestUser" + rand, CryptoHelper.GenerateAuthToken());
        User userB = await database.CreateUser("unitTestUser" + rand, CryptoHelper.GenerateAuthToken());

        Assert.NotNull(userA);
        Assert.NotNull(userB);

        await database.RemoveUser(userA); // Only remove userA since userA and userB are the same user
    }
}