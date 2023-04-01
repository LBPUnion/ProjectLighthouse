using LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Unit.Controllers;

[Trait("Category", "Unit")]
public class StatusControllerTests
{
    [Fact]
    public void Status_ShouldReturnOk()
    {
        StatusController statusController = new()
        {
            ControllerContext = MockHelper.GetMockControllerContext(),
        };

        const int expectedStatusCode = 200;

        IActionResult result = statusController.GetStatus();
        OkResult? okResult = result as OkResult;
        Assert.NotNull(okResult);
        Assert.Equal(expectedStatusCode, okResult.StatusCode);
    }
}