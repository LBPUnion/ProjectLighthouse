using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Administration;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Startup;
using LBPUnion.ProjectLighthouse.Tests;
using LBPUnion.ProjectLighthouse.Tickets;
using Microsoft.EntityFrameworkCore;
using Xunit;
using User = LBPUnion.ProjectLighthouse.PlayerData.Profiles.User;

namespace ProjectLighthouse.Tests.GameApiTests.Tests;

public class LoginTests : LighthouseServerTest<GameServerTestStartup>
{
    [Fact]
    public async Task ShouldLoginWithGoodTicket()
    {
        string username = await this.CreateRandomUser();
        ulong userId = (ulong)Convert.ToInt32(username[.."unitTestUser".Length]);
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
        ulong userId = (ulong)Convert.ToInt32(username[.."unitTestUser".Length]);
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
        ulong userId = (ulong)Convert.ToInt32(username[.."unitTestUser".Length]);
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
        ulong userId = (ulong)Convert.ToInt32(username[.."unitTestUser".Length]);
        byte[] ticketData = new TicketBuilder().SetUsername(username)
            .SetUserId(userId)
            .Build();

        ticketData[^2] = 0;
        ticketData[^1] = 0;
        HttpResponseMessage response = await this.Client.PostAsync("/LITTLEBIGPLANETPS3_XML/login", new ByteArrayContent(ticketData));
        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldNotLoginIfBanned()
    {
        string username = await this.CreateRandomUser();
        ulong userId = (ulong)Convert.ToInt32(username[.."unitTestUser".Length]);
        await using Database database = new();
        User user = await database.Users.FirstAsync(u => u.Username == username);
        user.PermissionLevel = PermissionLevel.Banned;
        await database.SaveChangesAsync();

        byte[] ticketData = new TicketBuilder()
            .SetUsername(username)
            .SetUserId(userId).Build();
        HttpResponseMessage response =
            await this.Client.PostAsync("/LITTLEBIGPLANETPS3_XML/login", new ByteArrayContent(ticketData));
        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.Forbidden);
    }

}