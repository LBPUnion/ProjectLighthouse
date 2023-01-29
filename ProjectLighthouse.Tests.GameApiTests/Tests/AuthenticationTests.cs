using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Startup;
using LBPUnion.ProjectLighthouse.Tests;
using LBPUnion.ProjectLighthouse.Users;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Tests;

public class AuthenticationTests : LighthouseServerTest<GameServerTestStartup>
{
    [Fact]
    public async Task ShouldReturnErrorOnNoPostData()
    {
        HttpResponseMessage response = await this.Client.PostAsync("/LITTLEBIGPLANETPS3_XML/login", null!);
        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
    }

    [DatabaseFact]
    public async Task ShouldReturnWithValidData()
    {
        HttpResponseMessage response = await this.AuthenticateResponse();
        Assert.True(response.IsSuccessStatusCode);
        string responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("MM_AUTH=", responseContent);
        Assert.Contains(VersionHelper.EnvVer, responseContent);
    }

    [DatabaseFact]
    public async Task CanSerializeBack()
    {
        LoginResult loginResult = await this.Authenticate();

        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult.AuthTicket);
        Assert.NotNull(loginResult.ServerBrand);

        Assert.Contains("MM_AUTH=", loginResult.AuthTicket);
        Assert.Equal(VersionHelper.EnvVer, loginResult.ServerBrand);
    }

    [DatabaseFact]
    public async Task CanUseToken()
    {
        LoginResult loginResult = await this.Authenticate();

        HttpResponseMessage response = await this.AuthenticatedRequest("/LITTLEBIGPLANETPS3_XML/enterLevel/420", loginResult.AuthTicket);
        await response.Content.ReadAsStringAsync();

        Assert.False(response.StatusCode == HttpStatusCode.Forbidden);
    }

    [DatabaseFact]
    public async Task ShouldReturnForbiddenWhenNotAuthenticated()
    {
        HttpResponseMessage response = await this.Client.GetAsync("/LITTLEBIGPLANETPS3_XML/announce");
        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.Forbidden);
    }
}