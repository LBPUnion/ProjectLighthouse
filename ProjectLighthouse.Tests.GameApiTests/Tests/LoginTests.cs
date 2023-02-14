using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Startup;
using LBPUnion.ProjectLighthouse.Tests;
using LBPUnion.ProjectLighthouse.Tickets;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Tests;

public class LoginTests : LighthouseServerTest<GameServerTestStartup>
{
    [Fact]
    public async Task ShouldLoginWithGoodTicket()
    {
        string username = await this.CreateRandomUser();
        ulong userId = (ulong)Convert.ToInt32(username["unitTestUser".Length..]);
        byte[] ticketData = new TicketBuilder()
            .SetUsername(username)
            .SetUserId(userId)
            .Build();
        HttpResponseMessage response = await this.Client.PostAsync("/LITTLEBIGPLANETPS3_XML/login", new ByteArrayContent(ticketData));
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task ShouldNotLoginWithExpiredTicket()
    {
        string username = await this.CreateRandomUser();
        ulong userId = (ulong)Convert.ToInt32(username["unitTestUser".Length..]);
        byte[] ticketData = new TicketBuilder()
            .SetUsername(username)
            .SetUserId(userId)
            .setExpirationTime((ulong)TimeHelper.TimestampMillis - 1000 * 60)
            .Build();
        HttpResponseMessage response = await this.Client.PostAsync("/LITTLEBIGPLANETPS3_XML/login", new ByteArrayContent(ticketData));
        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task ShouldNotLoginWithBadTitleId()
    {
        string username = await this.CreateRandomUser();
        ulong userId = (ulong)Convert.ToInt32(username["unitTestUser".Length..]);
        byte[] ticketData = new TicketBuilder()
            .SetUsername(username)
            .SetUserId(userId)
            .SetTitleId("UP9000-BLUS30079_00")
            .Build();
        HttpResponseMessage response = await this.Client.PostAsync("/LITTLEBIGPLANETPS3_XML/login", new ByteArrayContent(ticketData));
        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldNotLoginWithBadSignature()
    {
        string username = await this.CreateRandomUser();
        ulong userId = (ulong)Convert.ToInt32(username["unitTestUser".Length..]);
        byte[] ticketData = new TicketBuilder()
            .SetUsername(username)
            .SetUserId(userId)
            .Build();
        // Create second ticket and replace the first tickets signature with the first.
        byte[] ticketData2 = new TicketBuilder()
            .SetUsername(username)
            .SetUserId(userId)
            .Build();

        Array.Copy(ticketData2, ticketData2.Length - 0x38, ticketData, ticketData.Length - 0x38, 0x38);
        
        HttpResponseMessage response = await this.Client.PostAsync("/LITTLEBIGPLANETPS3_XML/login", new ByteArrayContent(ticketData));
        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldNotLoginIfBanned()
    {
        string username = await this.CreateRandomUser();
        ulong userId = (ulong)Convert.ToInt32(username["unitTestUser".Length..]);
        await using DatabaseContext database = new();
        User user = await database.Users.FirstAsync(u => u.Username == username);
        user.PermissionLevel = PermissionLevel.Banned;
        await database.SaveChangesAsync();

        byte[] ticketData = new TicketBuilder()
            .SetUsername(username)
            .SetUserId(userId)
            .Build();
        HttpResponseMessage response =
            await this.Client.PostAsync("/LITTLEBIGPLANETPS3_XML/login", new ByteArrayContent(ticketData));
        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.Forbidden);
    }

}