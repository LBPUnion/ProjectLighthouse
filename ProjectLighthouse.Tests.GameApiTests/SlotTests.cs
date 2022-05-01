using System;
using System.Net.Http;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Tests;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Profiles;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests;

public class SlotTests : LighthouseServerTest
{
    [DatabaseFact]
    public async Task ShouldOnlyShowUsersLevels()
    {
        await using Database database = new();

        Random r = new();

        User userA = await database.CreateUser($"unitTestUser{r.Next()}", CryptoHelper.GenerateAuthToken());
        User userB = await database.CreateUser($"unitTestUser{r.Next()}", CryptoHelper.GenerateAuthToken());

        Location l = new()
        {
            X = 0,
            Y = 0,
        };
        database.Locations.Add(l);
        await database.SaveChangesAsync();

        Slot slotA = new()
        {
            Creator = userA,
            CreatorId = userA.UserId,
            Name = "slotA",
            Location = l,
            LocationId = l.Id,
            ResourceCollection = "",
        };

        Slot slotB = new()
        {
            Creator = userB,
            CreatorId = userB.UserId,
            Name = "slotB",
            Location = l,
            LocationId = l.Id,
            ResourceCollection = "",
        };

        database.Slots.Add(slotA);
        database.Slots.Add(slotB);

        await database.SaveChangesAsync();

//            XmlSerializer serializer = new(typeof(Slot));
//            Slot slot = (Slot)serializer.Deserialize(new StringReader(bodyString));

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