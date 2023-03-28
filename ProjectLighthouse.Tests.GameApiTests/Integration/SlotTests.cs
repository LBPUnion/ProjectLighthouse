using System;
using System.Net.Http;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Startup;
using LBPUnion.ProjectLighthouse.Tests.Integration;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Users;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Integration;

[Trait("Category", "Integration")]
public class SlotTests : LighthouseServerTest<GameServerTestStartup>
{
    [DatabaseFact]
    public async Task ShouldOnlyShowUsersLevels()
    {
        await using DatabaseContext database = new();

        Random r = new();

        UserEntity userA = await database.CreateUser($"unitTestUser{r.Next()}", CryptoHelper.GenerateAuthToken());
        UserEntity userB = await database.CreateUser($"unitTestUser{r.Next()}", CryptoHelper.GenerateAuthToken());

        SlotEntity slotA = new()
        {
            Creator = userA,
            CreatorId = userA.UserId,
            Name = "slotA",
            ResourceCollection = "",
        };

        SlotEntity slotB = new()
        {
            Creator = userB,
            CreatorId = userB.UserId,
            Name = "slotB",
            ResourceCollection = "",
        };

        database.Slots.Add(slotA);
        database.Slots.Add(slotB);

        await database.SaveChangesAsync();

        LoginResult loginResult = await this.Authenticate();

        HttpResponseMessage respMessageA = await this.AuthenticatedRequest
            ($"LITTLEBIGPLANETPS3_XML/slots/by?u={userA.Username}&pageStart=1&pageSize=1", loginResult.AuthTicket);
        HttpResponseMessage respMessageB = await this.AuthenticatedRequest
            ($"LITTLEBIGPLANETPS3_XML/slots/by?u={userB.Username}&pageStart=1&pageSize=1", loginResult.AuthTicket);

        Assert.True(respMessageA.IsSuccessStatusCode);
        Assert.True(respMessageB.IsSuccessStatusCode);

        string respA = await respMessageA.Content.ReadAsStringAsync();
        string respB = await respMessageB.Content.ReadAsStringAsync();

        Assert.False(string.IsNullOrEmpty(respA));
        Assert.False(string.IsNullOrEmpty(respB));

        Assert.NotEqual(respA, respB);
        Assert.DoesNotContain(respA, "slotB");
        Assert.DoesNotContain(respB, "slotA");

        // Cleanup

        database.Slots.Remove(slotA);
        database.Slots.Remove(slotB);

        await database.RemoveUser(userA);
        await database.RemoveUser(userB);

        await database.SaveChangesAsync();
    }
}