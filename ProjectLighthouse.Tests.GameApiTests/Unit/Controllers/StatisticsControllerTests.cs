using System.Collections.Generic;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;
using LBPUnion.ProjectLighthouse.Tests.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Unit.Controllers;

[Trait("Category", "Unit")]
public class StatisticsControllerTests
{
    [Fact]
    public async void PlanetStats_ShouldReturnCorrectCounts_WhenEmpty()
    {
        Mock<DatabaseContext> dbMock = MockHelper.GetDatabaseMock();
        dbMock.Setup(x => x.Slots).ReturnsDbSet(new List<SlotEntity>());

        StatisticsController statsController = new(dbMock.Object);
        statsController.SetupTestController();

        const int expectedStatusCode = 200;
        const int expectedSlots = 0;
        const int expectedTeamPicks = 0;

        IActionResult result = await statsController.PlanetStats();
        OkObjectResult? objectResult = result as OkObjectResult;
        Assert.NotNull(objectResult);
        Assert.Equal(expectedStatusCode, objectResult.StatusCode);
        PlanetStatsResponse? response = objectResult.Value as PlanetStatsResponse;
        Assert.NotNull(response);
        Assert.Equal(expectedSlots, response.TotalSlotCount);
        Assert.Equal(expectedTeamPicks, response.TeamPickCount);
    }

    [Fact]
    public async void PlanetStats_ShouldReturnCorrectCounts_WhenNotEmpty()
    {
        Mock<DatabaseContext> dbMock = MockHelper.GetDatabaseMock();
        dbMock.Setup(x => x.Slots).ReturnsDbSet(new List<SlotEntity>
        {
            new()
            {
                SlotId = 1,
            },
            new()
            {
                SlotId = 2,
            },
            new()
            {
                SlotId = 3,
                TeamPick = true,
            },
        });

        StatisticsController statsController = new(dbMock.Object);
        statsController.SetupTestController();

        const int expectedStatusCode = 200;
        const int expectedSlots = 3;
        const int expectedTeamPicks = 1;

        IActionResult result = await statsController.PlanetStats();
        OkObjectResult? objectResult = result as OkObjectResult;
        Assert.NotNull(objectResult);
        Assert.Equal(expectedStatusCode, objectResult.StatusCode);
        PlanetStatsResponse? response = objectResult.Value as PlanetStatsResponse;
        Assert.NotNull(response);
        Assert.Equal(expectedSlots, response.TotalSlotCount);
        Assert.Equal(expectedTeamPicks, response.TeamPickCount);
    }

    [Fact]
    public async void PlanetStats_ShouldReturnCorrectCounts_WhenSlotsAreIncompatibleGameVersion()
    {
        Mock<DatabaseContext> dbMock = MockHelper.GetDatabaseMock();
        dbMock.Setup(x => x.Slots)
            .ReturnsDbSet(new List<SlotEntity>
            {
                new()
                {
                    SlotId = 1,
                    GameVersion = GameVersion.LittleBigPlanet2,
                },
                new()
                {
                    SlotId = 2,
                    GameVersion = GameVersion.LittleBigPlanet2,
                },
                new()
                {
                    SlotId = 3,
                    TeamPick = true,
                    GameVersion = GameVersion.LittleBigPlanet2,
                },
            });

        StatisticsController statsController = new(dbMock.Object);
        statsController.SetupTestController();

        const int expectedStatusCode = 200;
        const int expectedSlots = 0;
        const int expectedTeamPicks = 0;

        IActionResult result = await statsController.PlanetStats();
        OkObjectResult? objectResult = result as OkObjectResult;
        Assert.NotNull(objectResult);
        Assert.Equal(expectedStatusCode, objectResult.StatusCode);
        PlanetStatsResponse? response = objectResult.Value as PlanetStatsResponse;
        Assert.NotNull(response);
        Assert.Equal(expectedSlots, response.TotalSlotCount);
        Assert.Equal(expectedTeamPicks, response.TeamPickCount);
    }

    [Fact]
    public async void TotalLevelCount_ShouldReturnCorrectCount_WhenSlotsAreCompatible()
    {
        Mock<DatabaseContext> dbMock = MockHelper.GetDatabaseMock();
        dbMock.Setup(x => x.Slots)
            .ReturnsDbSet(new List<SlotEntity>
            {
                new()
                {
                    SlotId = 1,
                    GameVersion = GameVersion.LittleBigPlanet1,
                },
                new()
                {
                    SlotId = 2,
                    GameVersion = GameVersion.LittleBigPlanet1,
                },
                new()
                {
                    SlotId = 3,
                    TeamPick = true,
                    GameVersion = GameVersion.LittleBigPlanet1,
                },
            });

        StatisticsController statsController = new(dbMock.Object);
        statsController.SetupTestController();

        const int expectedStatusCode = 200;
        const string expectedTotal = "3";

        IActionResult result = await statsController.TotalLevelCount();
        OkObjectResult? objectResult = result as OkObjectResult;
        Assert.NotNull(objectResult);
        Assert.Equal(expectedStatusCode, objectResult.StatusCode);
        Assert.Equal(expectedTotal, objectResult.Value);
    }

    [Fact]
    public async void TotalLevelCount_ShouldReturnCorrectCount_WhenSlotsAreNotCompatible()
    {
        Mock<DatabaseContext> dbMock = MockHelper.GetDatabaseMock();
        dbMock.Setup(x => x.Slots)
            .ReturnsDbSet(new List<SlotEntity>
            {
                new()
                {
                    SlotId = 1,
                    GameVersion = GameVersion.LittleBigPlanet2,
                },
                new()
                {
                    SlotId = 2,
                    GameVersion = GameVersion.LittleBigPlanet2,
                },
                new()
                {
                    SlotId = 3,
                    TeamPick = true,
                    GameVersion = GameVersion.LittleBigPlanet2,
                },
            });

        StatisticsController statsController = new(dbMock.Object);
        statsController.SetupTestController();

        const int expectedStatusCode = 200;
        const string expectedTotal = "0";

        IActionResult result = await statsController.TotalLevelCount();
        OkObjectResult? objectResult = result as OkObjectResult;
        Assert.NotNull(objectResult);
        Assert.Equal(expectedStatusCode, objectResult.StatusCode);
        Assert.Equal(expectedTotal, objectResult.Value);
    }
}