using System.Collections.Generic;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;
using LBPUnion.ProjectLighthouse.Tests.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Unit.Controllers;

[Trait("Category", "Unit")]
public class StatisticsControllerTests
{
    [Fact]
    public async void PlanetStats_ShouldReturnCorrectCounts_WhenEmpty()
    {
        await using DatabaseContext db = await MockHelper.GetTestDatabase();

        StatisticsController statsController = new(db);
        statsController.SetupTestController();

        const int expectedSlots = 0;
        const int expectedTeamPicks = 0;

        IActionResult result = await statsController.PlanetStats();

        Assert.IsType<OkObjectResult>(result);
        OkObjectResult? objectResult = result as OkObjectResult;
        Assert.NotNull(objectResult);
        PlanetStatsResponse? response = objectResult.Value as PlanetStatsResponse;
        Assert.NotNull(response);
        Assert.Equal(expectedSlots, response.TotalSlotCount);
        Assert.Equal(expectedTeamPicks, response.TeamPickCount);
    }

    [Fact]
    public async void PlanetStats_ShouldReturnCorrectCounts_WhenNotEmpty()
    {
        List<SlotEntity> slots = new()
        {
            new SlotEntity
            {
                SlotId = 1,
            },
            new SlotEntity
            {
                SlotId = 2,
            },
            new SlotEntity
            {
                SlotId = 3,
                TeamPick = true,
            },
        };
        await using DatabaseContext db = await MockHelper.GetTestDatabase(new []{slots,});

        StatisticsController statsController = new(db);
        statsController.SetupTestController();

        const int expectedSlots = 3;
        const int expectedTeamPicks = 1;

        IActionResult result = await statsController.PlanetStats();

        Assert.IsType<OkObjectResult>(result);
        OkObjectResult? objectResult = result as OkObjectResult;
        Assert.NotNull(objectResult);
        PlanetStatsResponse? response = objectResult.Value as PlanetStatsResponse;
        Assert.NotNull(response);
        Assert.Equal(expectedSlots, response.TotalSlotCount);
        Assert.Equal(expectedTeamPicks, response.TeamPickCount);
    }

    [Fact]
    public async void PlanetStats_ShouldReturnCorrectCounts_WhenSlotsAreIncompatibleGameVersion()
    {
        List<SlotEntity> slots = new()
        {
            new SlotEntity
            {
                SlotId = 1,
                GameVersion = GameVersion.LittleBigPlanet2,
            },
            new SlotEntity
            {
                SlotId = 2,
                GameVersion = GameVersion.LittleBigPlanet2,
            },
            new SlotEntity
            {
                SlotId = 3,
                TeamPick = true,
                GameVersion = GameVersion.LittleBigPlanet2,
            },
        };
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase(new[]{slots,});

        StatisticsController statsController = new(dbMock);
        statsController.SetupTestController();

        const int expectedSlots = 0;
        const int expectedTeamPicks = 0;

        IActionResult result = await statsController.PlanetStats();

        Assert.IsType<OkObjectResult>(result);
        OkObjectResult? objectResult = result as OkObjectResult;
        Assert.NotNull(objectResult);
        PlanetStatsResponse? response = objectResult.Value as PlanetStatsResponse;
        Assert.NotNull(response);
        Assert.Equal(expectedSlots, response.TotalSlotCount);
        Assert.Equal(expectedTeamPicks, response.TeamPickCount);
    }

    [Fact]
    public async void TotalLevelCount_ShouldReturnCorrectCount_WhenSlotsAreCompatible()
    {
        List<SlotEntity> slots = new()
        {
            new SlotEntity
            {
                SlotId = 1,
                GameVersion = GameVersion.LittleBigPlanet1,
            },
            new SlotEntity
            {
                SlotId = 2,
                GameVersion = GameVersion.LittleBigPlanet1,
            },
            new SlotEntity
            {
                SlotId = 3,
                TeamPick = true,
                GameVersion = GameVersion.LittleBigPlanet1,
            },
        };
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase(new[] {slots,});

        StatisticsController statsController = new(dbMock);
        statsController.SetupTestController();

        const string expectedTotal = "3";

        IActionResult result = await statsController.TotalLevelCount();

        Assert.IsType<OkObjectResult>(result);
        OkObjectResult? objectResult = result as OkObjectResult;
        Assert.NotNull(objectResult);
        Assert.Equal(expectedTotal, objectResult.Value);
    }

    [Fact]
    public async void TotalLevelCount_ShouldReturnCorrectCount_WhenSlotsAreNotCompatible()
    {
        List<SlotEntity> slots = new()
        {
            new SlotEntity
            {
                SlotId = 1,
                GameVersion = GameVersion.LittleBigPlanet2,
            },
            new SlotEntity
            {
                SlotId = 2,
                GameVersion = GameVersion.LittleBigPlanet2,
            },
            new SlotEntity
            {
                SlotId = 3,
                TeamPick = true,
                GameVersion = GameVersion.LittleBigPlanet2,
            },
        }; 
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase(new[] {slots,});

        StatisticsController statsController = new(dbMock);
        statsController.SetupTestController();

        const int expectedStatusCode = 200;
        const string expectedTotal = "0";

        IActionResult result = await statsController.TotalLevelCount();

        Assert.IsType<OkObjectResult>(result);
        OkObjectResult? objectResult = result as OkObjectResult;
        Assert.NotNull(objectResult);
        Assert.Equal(expectedStatusCode, objectResult.StatusCode);
        Assert.Equal(expectedTotal, objectResult.Value);
    }
}