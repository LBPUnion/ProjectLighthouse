using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Startup;
using LBPUnion.ProjectLighthouse.Tests.Helpers;
using LBPUnion.ProjectLighthouse.Tests.Integration;
using LBPUnion.ProjectLighthouse.Types.Users;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Integration;

[Trait("Category", "Integration")]
public class UploadTests : LighthouseServerTest<GameServerTestStartup>
{
    public UploadTests()
    {
        string assetsDirectory = Path.Combine(Environment.CurrentDirectory, "r");
        if (Directory.Exists(assetsDirectory)) Directory.Delete(assetsDirectory, true);
    }

    [Fact]
    public async Task ShouldNotAcceptScript()
    {
        await IntegrationHelper.GetIntegrationDatabase();

        LoginResult loginResult = await this.Authenticate();

        HttpResponseMessage response = await this.AuthenticatedUploadFileEndpointRequest("ExampleFiles/TestScript.ff", loginResult.AuthTicket);

        const HttpStatusCode expectedStatusCode = HttpStatusCode.Conflict;

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Fact]
    public async Task ShouldNotAcceptFarc()
    {
        await IntegrationHelper.GetIntegrationDatabase();

        LoginResult loginResult = await this.Authenticate();

        HttpResponseMessage response = await this.AuthenticatedUploadFileEndpointRequest("ExampleFiles/TestFarc.farc", loginResult.AuthTicket);

        const HttpStatusCode expectedStatusCode = HttpStatusCode.Conflict;

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Fact]
    public async Task ShouldNotAcceptGarbage()
    {
        await IntegrationHelper.GetIntegrationDatabase();

        LoginResult loginResult = await this.Authenticate();

        HttpResponseMessage response = await this.AuthenticatedUploadFileEndpointRequest("ExampleFiles/TestGarbage.bin", loginResult.AuthTicket);

        const HttpStatusCode expectedStatusCode = HttpStatusCode.Conflict;

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Fact]
    public async Task ShouldAcceptTexture()
    {
        await IntegrationHelper.GetIntegrationDatabase();

        LoginResult loginResult = await this.Authenticate();

        HttpResponseMessage response = await this.AuthenticatedUploadFileEndpointRequest("ExampleFiles/TestTexture.tex", loginResult.AuthTicket);

        const HttpStatusCode expectedStatusCode = HttpStatusCode.OK;

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Fact]
    public async Task ShouldAcceptLevel()
    {
        await IntegrationHelper.GetIntegrationDatabase();

        LoginResult loginResult = await this.Authenticate();

        HttpResponseMessage response = await this.AuthenticatedUploadFileEndpointRequest("ExampleFiles/TestLevel.lvl", loginResult.AuthTicket);

        const HttpStatusCode expectedStatusCode = HttpStatusCode.OK;

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }
}