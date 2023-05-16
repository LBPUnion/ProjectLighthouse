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

        const HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest;

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldReturnWithValidData()
    {
        await IntegrationHelper.GetIntegrationDatabase();

        HttpResponseMessage response = await this.AuthenticateResponse();

        const HttpStatusCode expectedStatusCode = HttpStatusCode.OK;

        Assert.Equal(expectedStatusCode, response.StatusCode);
        string responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("MM_AUTH=", responseContent);
        Assert.Contains(VersionHelper.EnvVer, responseContent);
    }

    [Fact]
    public async Task Login_CanSerializeBack()
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
    public async Task Login_CanUseToken()
    {
        await IntegrationHelper.GetIntegrationDatabase();

        LoginResult loginResult = await this.Authenticate();

        HttpResponseMessage response = await this.AuthenticatedRequest("/LITTLEBIGPLANETPS3_XML/enterLevel/420", loginResult.AuthTicket);
        await response.Content.ReadAsStringAsync();

        const HttpStatusCode expectedStatusCode = HttpStatusCode.NotFound;

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldReturnForbiddenWhenNotAuthenticated()
    {
        await IntegrationHelper.GetIntegrationDatabase();

        HttpResponseMessage response = await this.Client.GetAsync("/LITTLEBIGPLANETPS3_XML/announce");

        const HttpStatusCode expectedStatusCode = HttpStatusCode.Forbidden;

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }
}