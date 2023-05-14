using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Startup;
using LBPUnion.ProjectLighthouse.Tests.Helpers;
using LBPUnion.ProjectLighthouse.Tests.Integration;
using LBPUnion.ProjectLighthouse.Types.Users;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Integration;

[Trait("Category", "Integration")]
public class MatchTests : LighthouseServerTest<GameServerTestStartup>
{
    [Fact]
    public async Task Match_ShouldRejectEmptyData()
    {
        await IntegrationHelper.GetIntegrationDatabase();

        LoginResult loginResult = await this.Authenticate();

        HttpResponseMessage result = await this.AuthenticatedUploadDataRequest("/LITTLEBIGPLANETPS3_XML/match", Array.Empty<byte>(), loginResult.AuthTicket);

        const HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest;

        Assert.Equal(expectedStatusCode, result.StatusCode);
    }

    [Fact]
    public async Task Match_ShouldReturnOk_WithGoodRequest()
    {
        await IntegrationHelper.GetIntegrationDatabase();

        LoginResult loginResult = await this.Authenticate();

        HttpResponseMessage result = await this.AuthenticatedUploadDataRequest
            ("/LITTLEBIGPLANETPS3_XML/match", "[UpdateMyPlayerData,[\"Player\":\"1984\"]]"u8.ToArray(), loginResult.AuthTicket);

        const HttpStatusCode expectedStatusCode = HttpStatusCode.OK;

        Assert.Equal(expectedStatusCode, result.StatusCode);
    }

    [Fact]
    public async Task Match_ShouldIncrementPlayerCount()
    {
        await IntegrationHelper.GetIntegrationDatabase();

        LoginResult loginResult = await this.Authenticate();

        await using DatabaseContext database = DatabaseContext.CreateNewInstance();

        int oldPlayerCount = await StatisticsHelper.RecentMatches(database);

        HttpResponseMessage result = await this.AuthenticatedUploadDataRequest
            ("/LITTLEBIGPLANETPS3_XML/match", "[UpdateMyPlayerData,[\"Player\":\"1984\"]]"u8.ToArray(), loginResult.AuthTicket);

        const HttpStatusCode expectedStatusCode = HttpStatusCode.OK;

        Assert.Equal(expectedStatusCode, result.StatusCode);

        int playerCount = await StatisticsHelper.RecentMatches(database);

        Assert.Equal(oldPlayerCount + 1, playerCount);
    }
}