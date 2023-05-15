using LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;
using LBPUnion.ProjectLighthouse.Tests.Helpers;
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

        IActionResult result = statusController.GetStatus();

        Assert.IsType<OkResult>(result);
    }
}