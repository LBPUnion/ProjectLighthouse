using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Startup;
using LBPUnion.ProjectLighthouse.Tests.Helpers;
using LBPUnion.ProjectLighthouse.Tests.Integration;
using LBPUnion.ProjectLighthouse.Tickets;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Users;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Integration;

[Trait("Category", "Integration")]
public class LoginTests : LighthouseServerTest<GameServerTestStartup>
{
    [Fact]
    public async Task ShouldLoginWithGoodTicket()
    {
        await IntegrationHelper.GetIntegrationDatabase();

        UserEntity user = await this.CreateRandomUser();
        byte[] ticketData = new TicketBuilder()
            .SetUsername(user.Username)
            .SetUserId((ulong)user.UserId)
            .Build();
        HttpResponseMessage response = await this.Client.PostAsync("/LITTLEBIGPLANETPS3_XML/login", new ByteArrayContent(ticketData));

        const HttpStatusCode expectedStatusCode = HttpStatusCode.OK;

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Fact]
    public async Task ShouldNotLoginWithExpiredTicket()
    {
        await IntegrationHelper.GetIntegrationDatabase();

        UserEntity user = await this.CreateRandomUser();
        byte[] ticketData = new TicketBuilder()
            .SetUsername(user.Username)
            .SetUserId((ulong)user.UserId)
            .setExpirationTime((ulong)TimeHelper.TimestampMillis - 1000 * 60)
            .Build();
        HttpResponseMessage response = await this.Client.PostAsync("/LITTLEBIGPLANETPS3_XML/login", new ByteArrayContent(ticketData));

        const HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest;

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldNotLoginWithBadTitleId()
    {
        await IntegrationHelper.GetIntegrationDatabase();

        UserEntity user = await this.CreateRandomUser();
        byte[] ticketData = new TicketBuilder()
            .SetUsername(user.Username)
            .SetUserId((ulong)user.UserId)
            .SetTitleId("UP9000-BLUS30079_00")
            .Build();
        HttpResponseMessage response = await this.Client.PostAsync("/LITTLEBIGPLANETPS3_XML/login", new ByteArrayContent(ticketData));

        const HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest;

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Fact]
    public async Task ShouldNotLoginWithBadSignature()
    {
        await IntegrationHelper.GetIntegrationDatabase();

        UserEntity user = await this.CreateRandomUser();
        byte[] ticketData = new TicketBuilder()
            .SetUsername(user.Username)
            .SetUserId((ulong)user.UserId)
            .Build();
        // Create second ticket and replace the first tickets signature with the first.
        byte[] ticketData2 = new TicketBuilder()
            .SetUsername(user.Username)
            .SetUserId((ulong)user.UserId)
            .Build();

        Array.Copy(ticketData2, ticketData2.Length - 0x38, ticketData, ticketData.Length - 0x38, 0x38);
        
        HttpResponseMessage response = await this.Client.PostAsync("/LITTLEBIGPLANETPS3_XML/login", new ByteArrayContent(ticketData));

        const HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest;

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Fact]
    public async Task ShouldNotLoginIfBanned()
    {
        DatabaseContext database = await IntegrationHelper.GetIntegrationDatabase();

        UserEntity user = await this.CreateRandomUser();

        user.PermissionLevel = PermissionLevel.Banned;

        database.Users.Update(user);
        await database.SaveChangesAsync();

        byte[] ticketData = new TicketBuilder()
            .SetUsername(user.Username)
            .SetUserId((ulong)user.UserId)
            .Build();
        HttpResponseMessage response =
            await this.Client.PostAsync("/LITTLEBIGPLANETPS3_XML/login", new ByteArrayContent(ticketData));

        const HttpStatusCode expectedStatusCode = HttpStatusCode.Forbidden;

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

}
