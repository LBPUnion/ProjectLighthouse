using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers.Slots;
using LBPUnion.ProjectLighthouse.Tests.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Unit.Controllers;

[Trait("Category", "Unit")]
public class ReviewControllerTests
{
    private static async Task InsertTestData(DatabaseContext database)
    {
        database.Slots.Add(new SlotEntity
        {
            SlotId = 1,
            CreatorId = 1,
            GameVersion = GameVersion.LittleBigPlanet3,
        });

        database.Slots.Add(new SlotEntity
        {
            SlotId = 2,
            CreatorId = 1,
            GameVersion = GameVersion.LittleBigPlanet2,
        });

        database.Reviews.Add(new ReviewEntity
        {
            ReviewId = 1,
            ReviewerId = 1,
            SlotId = 1,
        });

        database.Reviews.Add(new ReviewEntity
        {
            ReviewId = 2,
            ReviewerId = 1,
            SlotId = 2,
        });
        await database.SaveChangesAsync();
    }

    [Theory]
    [InlineData(GameVersion.LittleBigPlanet2, 1)]
    [InlineData(GameVersion.LittleBigPlanet3, 2)]
    public async Task ReviewsBy_ShouldNotList_HigherGameVersions(GameVersion version, int expected)
    {
        GameTokenEntity token = MockHelper.GetUnitTestToken();
        token.GameVersion = version;
        DatabaseContext database = await MockHelper.GetTestDatabase(new List<GameTokenEntity>
        {
            token,
        });

        await InsertTestData(database);

        ReviewController controller = new(database);
        controller.SetupTestController(token);

        IActionResult response = await controller.ReviewsBy("unittest");
        ReviewResponse review = response.CastTo<OkObjectResult, ReviewResponse>();

        Assert.Equal(expected, review.Reviews.Count);
        Assert.True(review.Reviews.All(r => database.Slots.FirstOrDefault(s => s.SlotId == r.Slot.SlotId)?.GameVersion <= version));
    }

    [Theory]
    [InlineData(GameVersion.LittleBigPlanet2, 2, 1)]
    [InlineData(GameVersion.LittleBigPlanet2, 1, 0)]
    [InlineData(GameVersion.LittleBigPlanet3, 2, 1)]
    [InlineData(GameVersion.LittleBigPlanet3, 1, 1)]
    public async Task ReviewsFor_ShouldNotList_HigherGameVersions(GameVersion version, int slotId, int expected)
    {
        GameTokenEntity token = MockHelper.GetUnitTestToken();
        token.GameVersion = version;
        DatabaseContext database = await MockHelper.GetTestDatabase(new List<GameTokenEntity>
        {
            token,
        });

        await InsertTestData(database);

        ReviewController controller = new(database);
        controller.SetupTestController(token);

        IActionResult response = await controller.ReviewsFor(slotId);
        ReviewResponse review = response.CastTo<OkObjectResult, ReviewResponse>();

        Assert.Equal(expected, review.Reviews.Count);
        Assert.True(review.Reviews.All(r => database.Slots.FirstOrDefault(s => s.SlotId == r.Slot.SlotId)?.GameVersion <= version));
    }
}