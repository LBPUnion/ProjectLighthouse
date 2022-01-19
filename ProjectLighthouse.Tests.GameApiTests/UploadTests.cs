using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Tests;
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
        HttpResponseMessage response = await this.UploadFileEndpointRequest("ExampleFiles/TestScript.ff");
        Assert.False(response.StatusCode == HttpStatusCode.Forbidden);
        Assert.False(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task ShouldNotAcceptFarc()
    {
        HttpResponseMessage response = await this.UploadFileEndpointRequest("ExampleFiles/TestFarc.farc");
        Assert.False(response.StatusCode == HttpStatusCode.Forbidden);
        Assert.False(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task ShouldNotAcceptGarbage()
    {
        HttpResponseMessage response = await this.UploadFileEndpointRequest("ExampleFiles/TestGarbage.bin");
        Assert.False(response.StatusCode == HttpStatusCode.Forbidden);
        Assert.False(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task ShouldAcceptTexture()
    {
        HttpResponseMessage response = await this.UploadFileEndpointRequest("ExampleFiles/TestTexture.tex");
        Assert.False(response.StatusCode == HttpStatusCode.Forbidden);
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task ShouldAcceptLevel()
    {
        HttpResponseMessage response = await this.UploadFileEndpointRequest("ExampleFiles/TestLevel.lvl");
        Assert.False(response.StatusCode == HttpStatusCode.Forbidden);
        Assert.True(response.IsSuccessStatusCode);
    }
}