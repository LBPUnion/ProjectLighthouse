using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers.Slots;
using LBPUnion.ProjectLighthouse.Tests.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Unit.Controllers;

[Trait("Category", "Unit")]
public class ScoreControllerTests
{
    [Fact]
    public async Task SubmitScore_ShouldSubmitValidScore_WhenNoExistingScore()
    {
        DatabaseContext database = await MockHelper.GetTestDatabase();

        SlotEntity slot = new()
        {
            CreatorId = 1,
            SlotId = 1,
        };
        database.Slots.Add(slot);
        await database.SaveChangesAsync();

        ScoreController scoreController = new(database);
        const string xmlBody = """
                         <playRecord>
                           <type>1</type>
                           <score>10</score>
                           <playerIds>unittest</playerIds>
                         </playRecord>
                         """;
        scoreController.SetupTestController(xmlBody);
        IActionResult result = await scoreController.SubmitScore("user", 1, 0);
        Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(database.Scores.FirstOrDefault(s => s.Type == 1 && s.SlotId == 1 && s.UserId == 1));
    }

    [Fact]
    public async Task SubmitScore_ShouldUpdateScore_WhenBetterThanExistingScore()
    {
        DatabaseContext database = await MockHelper.GetTestDatabase();

        SlotEntity slot = new()
        {
            CreatorId = 1,
            SlotId = 1,
        };
        database.Slots.Add(slot);

        ScoreEntity score = new()
        {
            SlotId = 1,
            Type = 1,
            UserId = 1,
            Points = 5,
            Timestamp = 0,
        };
        database.Scores.Add(score);
        await database.SaveChangesAsync();

        ScoreController scoreController = new(database);
        const string xmlBody = """
                         <playRecord>
                           <type>1</type>
                           <score>10</score>
                           <playerIds>unittest</playerIds>
                         </playRecord>
                         """;
        scoreController.SetupTestController(xmlBody);
        IActionResult result = await scoreController.SubmitScore("user", 1, 0);
        Assert.IsType<OkObjectResult>(result);
        ScoreEntity? newScore = database.Scores.FirstOrDefault(s => s.Type == 1 && s.SlotId == 1 && s.UserId == 1);
        Assert.NotNull(newScore);
        Assert.NotEqual(0, newScore.Timestamp);
        Assert.Equal(10, newScore.Points);
    }

    [Fact]
    public async Task SubmitScore_ShouldNotUpdateScore_WhenEqualToExistingScore()
    {
        DatabaseContext database = await MockHelper.GetTestDatabase();

        SlotEntity slot = new()
        {
            CreatorId = 1,
            SlotId = 1,
        };
        database.Slots.Add(slot);

        ScoreEntity score = new()
        {
            SlotId = 1,
            Type = 1,
            UserId = 1,
            Points = 10,
            Timestamp = 0,
        };
        database.Scores.Add(score);
        await database.SaveChangesAsync();

        ScoreController scoreController = new(database);
        const string xmlBody = """
                         <playRecord>
                           <type>1</type>
                           <score>10</score>
                           <playerIds>unittest</playerIds>
                         </playRecord>
                         """;
        scoreController.SetupTestController(xmlBody);
        IActionResult result = await scoreController.SubmitScore("user", 1, 0);
        Assert.IsType<OkObjectResult>(result);
        ScoreEntity? newScore = database.Scores.FirstOrDefault(s => s.Type == 1 && s.SlotId == 1 && s.UserId == 1);
        Assert.NotNull(newScore);
        Assert.Equal(0, newScore.Timestamp);
        Assert.Equal(10, newScore.Points);
    }

    [Fact]
    public async Task SubmitScore_ShouldNotUpdateScore_WhenLessThanExistingScore()
    {
        DatabaseContext database = await MockHelper.GetTestDatabase();

        SlotEntity slot = new()
        {
            CreatorId = 1,
            SlotId = 1,
        };
        database.Slots.Add(slot);

        ScoreEntity score = new()
        {
            SlotId = 1,
            Type = 1,
            UserId = 1,
            Points = 10,
            Timestamp = 0,
        };
        database.Scores.Add(score);
        await database.SaveChangesAsync();

        ScoreController scoreController = new(database);
        const string xmlBody = """
                         <playRecord>
                           <type>1</type>
                           <score>5</score>
                           <playerIds>unittest</playerIds>
                         </playRecord>
                         """;
        scoreController.SetupTestController(xmlBody);
        IActionResult result = await scoreController.SubmitScore("user", 1, 0);
        Assert.IsType<OkObjectResult>(result);
        ScoreEntity? newScore = database.Scores.FirstOrDefault(s => s.Type == 1 && s.SlotId == 1 && s.UserId == 1);
        Assert.NotNull(newScore);
        Assert.Equal(0, newScore.Timestamp);
        Assert.Equal(10, newScore.Points);
    }
}