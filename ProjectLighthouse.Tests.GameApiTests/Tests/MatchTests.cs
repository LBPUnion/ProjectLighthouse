using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Startup;
using LBPUnion.ProjectLighthouse.Tests;
using LBPUnion.ProjectLighthouse.Types.Users;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Tests;

public class MatchTests : LighthouseServerTest<GameServerTestStartup>
{
    private static readonly SemaphoreSlim semaphore = new(1, 1);

    [DatabaseFact]
    public async Task ShouldRejectEmptyData()
    {
        LoginResult loginResult = await this.Authenticate();
        await semaphore.WaitAsync();

        HttpResponseMessage result = await this.AuthenticatedUploadDataRequest("LITTLEBIGPLANETPS3_XML/match", Array.Empty<byte>(), loginResult.AuthTicket);

        semaphore.Release();
        Assert.False(result.IsSuccessStatusCode);
    }

    [DatabaseFact]
    public async Task ShouldReturnOk()
    {
        LoginResult loginResult = await this.Authenticate();
        await semaphore.WaitAsync();

        HttpResponseMessage result = await this.AuthenticatedUploadDataRequest
            ("LITTLEBIGPLANETPS3_XML/match", "[UpdateMyPlayerData,[\"Player\":\"1984\"]]"u8.ToArray(), loginResult.AuthTicket);

        semaphore.Release();
        Assert.True(result.IsSuccessStatusCode);
    }

    [DatabaseFact]
    public async Task ShouldIncrementPlayerCount()
    {
        LoginResult loginResult = await this.Authenticate(new Random().Next());

        await semaphore.WaitAsync();

        await using DatabaseContext database = new();

        int oldPlayerCount = await StatisticsHelper.RecentMatches(database);

        HttpResponseMessage result = await this.AuthenticatedUploadDataRequest
            ("LITTLEBIGPLANETPS3_XML/match", "[UpdateMyPlayerData,[\"Player\":\"1984\"]]"u8.ToArray(), loginResult.AuthTicket);

        Assert.True(result.IsSuccessStatusCode);

        int playerCount = await StatisticsHelper.RecentMatches(database);

        semaphore.Release();
        Assert.Equal(oldPlayerCount + 1, playerCount);
    }
}