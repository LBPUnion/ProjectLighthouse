using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Tests;
using LBPUnion.ProjectLighthouse.Types;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests;

public class UploadTests : LighthouseServerTest
{
    public UploadTests()
    {
        string assetsDirectory = Path.Combine(Environment.CurrentDirectory, "r");
        if (Directory.Exists(assetsDirectory)) Directory.Delete(assetsDirectory, true);
    }

    [Fact]
    public async Task ShouldNotAcceptScript()
    {
        LoginResult loginResult = await this.Authenticate();

        HttpResponseMessage response = await this.AuthenticatedUploadFileEndpointRequest("ExampleFiles/TestScript.ff", loginResult.AuthTicket);
        Assert.False(response.StatusCode == HttpStatusCode.Forbidden);
        Assert.False(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task ShouldNotAcceptFarc()
    {
        LoginResult loginResult = await this.Authenticate();

        HttpResponseMessage response = await this.AuthenticatedUploadFileEndpointRequest("ExampleFiles/TestFarc.farc", loginResult.AuthTicket);
        Assert.False(response.StatusCode == HttpStatusCode.Forbidden);
        Assert.False(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task ShouldNotAcceptGarbage()
    {
        LoginResult loginResult = await this.Authenticate();

        HttpResponseMessage response = await this.AuthenticatedUploadFileEndpointRequest("ExampleFiles/TestGarbage.bin", loginResult.AuthTicket);
        Assert.False(response.StatusCode == HttpStatusCode.Forbidden);
        Assert.False(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task ShouldAcceptTexture()
    {
        LoginResult loginResult = await this.Authenticate();

        HttpResponseMessage response = await this.AuthenticatedUploadFileEndpointRequest("ExampleFiles/TestTexture.tex", loginResult.AuthTicket);
        Assert.False(response.StatusCode == HttpStatusCode.Forbidden);
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task ShouldAcceptLevel()
    {
        LoginResult loginResult = await this.Authenticate();

        HttpResponseMessage response = await this.AuthenticatedUploadFileEndpointRequest("ExampleFiles/TestLevel.lvl", loginResult.AuthTicket);
        Assert.False(response.StatusCode == HttpStatusCode.Forbidden);
        Assert.True(response.IsSuccessStatusCode);
    }
}