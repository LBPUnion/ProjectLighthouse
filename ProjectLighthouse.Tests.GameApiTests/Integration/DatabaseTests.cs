using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Startup;
using LBPUnion.ProjectLighthouse.Tests.Integration;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Integration;

[Trait("Category", "Integration")]
public class DatabaseTests : LighthouseServerTest<GameServerTestStartup>
{
    [DatabaseFact]
    public async Task CanCreateUserTwice()
    {
        await using DatabaseContext database = new();
        int rand = new Random().Next();

        UserEntity userA = await database.CreateUser("unitTestUser" + rand, CryptoHelper.GenerateAuthToken());
        UserEntity userB = await database.CreateUser("unitTestUser" + rand, CryptoHelper.GenerateAuthToken());

        Assert.NotNull(userA);
        Assert.NotNull(userB);

        await database.RemoveUser(userA); // Only remove userA since userA and userB are the same user
    }
}