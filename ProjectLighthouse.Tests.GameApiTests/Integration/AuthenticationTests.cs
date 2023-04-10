using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Startup;
using LBPUnion.ProjectLighthouse.Tests.Helpers;
using LBPUnion.ProjectLighthouse.Tests.Integration;
using LBPUnion.ProjectLighthouse.Types.Users;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Integration;

[Trait("Category", "Integration")]
public class AuthenticationTests : LighthouseServerTest<GameServerTestStartup>
{
    [Fact]
    public async Task ShouldReturnErrorOnNoPostData()
    {
        await IntegrationHelper.GetIntegrationDatabase();

        HttpResponseMessage response = await this.Client.PostAsync("/LITTLEBIGPLANETPS3_XML/login", null!);
        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldReturnWithValidData()
    {
        await IntegrationHelper.GetIntegrationDatabase();

        HttpResponseMessage response = await this.AuthenticateResponse();
        Assert.True(response.IsSuccessStatusCode);
        string responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("MM_AUTH=", responseContent);
        Assert.Contains(VersionHelper.EnvVer, responseContent);
    }

    [Fact]
    public async Task CanSerializeBack()
    {
        await IntegrationHelper.GetIntegrationDatabase();

        LoginResult loginResult = await this.Authenticate();

        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult.AuthTicket);
        Assert.NotNull(loginResult.ServerBrand);

        Assert.Contains("MM_AUTH=", loginResult.AuthTicket);
        Assert.Equal(VersionHelper.EnvVer, loginResult.ServerBrand);
    }

    [Fact]
    public async Task CanUseToken()
    {
        await IntegrationHelper.GetIntegrationDatabase();

        LoginResult loginResult = await this.Authenticate();

        HttpResponseMessage response = await this.AuthenticatedRequest("/LITTLEBIGPLANETPS3_XML/enterLevel/420", loginResult.AuthTicket);
        await response.Content.ReadAsStringAsync();

        Assert.False(response.StatusCode == HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ShouldReturnForbiddenWhenNotAuthenticated()
    {
        await IntegrationHelper.GetIntegrationDatabase();

        HttpResponseMessage response = await this.Client.GetAsync("/LITTLEBIGPLANETPS3_XML/announce");
        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.Forbidden);
    }
}