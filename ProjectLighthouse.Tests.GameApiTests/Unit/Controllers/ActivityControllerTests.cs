using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;
using LBPUnion.ProjectLighthouse.Tests.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Serialization.Activity;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Unit.Controllers;

[Trait("Category", "Unit")]
public class ActivityControllerTests
{
    private static void SetupToken(ControllerBase controller, GameVersion version)
    {
        GameTokenEntity token = MockHelper.GetUnitTestToken();
        token.GameVersion = version;
        controller.SetupTestController(token);
    }

    [Fact]
    public async Task LBP2GlobalActivity_ShouldReturnNothing_WhenEmpty()
    {
        DatabaseContext database = await MockHelper.GetTestDatabase();
        ActivityController activityController = new(database);
        SetupToken(activityController, GameVersion.LittleBigPlanet2);

        long timestamp = TimeHelper.TimestampMillis;

        IActionResult response = await activityController.GlobalActivity(timestamp, 0, false, false, false, false, false);
        GameStream stream = response.CastTo<OkObjectResult, GameStream>();
        Assert.Null(stream.Groups);
        Assert.Equal(timestamp, stream.StartTimestamp);
    }
    //TODO write activity controller tests
}