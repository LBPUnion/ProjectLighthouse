using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Startup;
using LBPUnion.ProjectLighthouse.Tests.Helpers;
using LBPUnion.ProjectLighthouse.Tests.Integration;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Integration;

[Trait("Category", "Integration")]
public class DatabaseTests : LighthouseServerTest<GameServerTestStartup>
{
    [Fact]
    public async Task CanCreateUserTwice()
    {
        await using DatabaseContext database = await IntegrationHelper.GetIntegrationDatabase();

        int rand = new Random().Next();

        UserEntity userA = await database.CreateUser("unitTestUser" + rand, CryptoHelper.BCryptHash(CryptoHelper.GenerateAuthToken()));
        UserEntity userB = await database.CreateUser("unitTestUser" + rand, CryptoHelper.BCryptHash(CryptoHelper.GenerateAuthToken()));

        Assert.NotNull(userA);
        Assert.NotNull(userB);
    }
}