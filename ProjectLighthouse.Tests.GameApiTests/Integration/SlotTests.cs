using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Startup;
using LBPUnion.ProjectLighthouse.Tests.Helpers;
using LBPUnion.ProjectLighthouse.Tests.Integration;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Users;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Integration;

[Trait("Category", "Integration")]
public class SlotTests : LighthouseServerTest<GameServerTestStartup>
{
    [Fact]
    public async Task ShouldOnlyShowUsersLevels()
    {
        await using DatabaseContext database = await IntegrationHelper.GetIntegrationDatabase();

        UserEntity userA = await this.CreateRandomUser();
        UserEntity userB = await this.CreateRandomUser();

        SlotEntity slotA = new()
        {
            CreatorId = userA.UserId,
            Name = "slotA",
            ResourceCollection = "",
        };

        SlotEntity slotB = new()
        {
            CreatorId = userB.UserId,
            Name = "slotB",
            ResourceCollection = "",
        };

        database.Slots.Add(slotA);
        database.Slots.Add(slotB);

        await database.SaveChangesAsync();

        LoginResult loginResult = await this.Authenticate();

        HttpResponseMessage respMessageA = await this.AuthenticatedRequest
            ($"/LITTLEBIGPLANETPS3_XML/slots/by?u={userA.Username}&pageStart=1&pageSize=1", loginResult.AuthTicket);
        HttpResponseMessage respMessageB = await this.AuthenticatedRequest
            ($"/LITTLEBIGPLANETPS3_XML/slots/by?u={userB.Username}&pageStart=1&pageSize=1", loginResult.AuthTicket);

        const HttpStatusCode expectedStatusCode = HttpStatusCode.OK;

        Assert.Equal(expectedStatusCode, respMessageA.StatusCode);
        Assert.Equal(expectedStatusCode, respMessageB.StatusCode);

        string respA = await respMessageA.Content.ReadAsStringAsync();
        string respB = await respMessageB.Content.ReadAsStringAsync();

        Assert.NotNull(respA);
        Assert.NotNull(respB);

        Assert.NotEqual(respA, respB);
        Assert.DoesNotContain(respA, "slotB");
        Assert.DoesNotContain(respB, "slotA");
    }
}