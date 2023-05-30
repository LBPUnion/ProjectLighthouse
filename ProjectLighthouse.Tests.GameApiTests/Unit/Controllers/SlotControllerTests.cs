using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers.Slots;
using LBPUnion.ProjectLighthouse.Tests.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Unit.Controllers;

[Trait("Category", "Unit")]
public class SlotControllerTests
{
    #region SlotsBy
    [Fact]
    public async Task SlotsBy_ShouldReturnNotFound_WhenUserInvalid()
    {
        DatabaseContext db = await MockHelper.GetTestDatabase();
        SlotsController slotsController = new(db);
        slotsController.SetupTestController();

        IActionResult result = await slotsController.SlotsBy("bytest");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task SlotsBy_ShouldFetchLevelsByUser()
    {
        List<SlotEntity> slots = new()
        {
            new SlotEntity
            {
                SlotId = 1,
                CreatorId = 2,
            },
            new SlotEntity
            {
                SlotId = 2,
                CreatorId = 2,
            },
            new SlotEntity
            {
                SlotId = 3,
                CreatorId = 3,
            },
        };
        List<UserEntity> users = new()
        {
            MockHelper.GetUnitTestUser(),
            new UserEntity
            {
                Username = "bytest",
                UserId = 2,
            },
        };
        DatabaseContext db = await MockHelper.GetTestDatabase(new IList[]
        {
            slots, users,
        });
        SlotsController slotsController = new(db);
        slotsController.SetupTestController();

        IActionResult result = await slotsController.SlotsBy("bytest");

        const int expectedElements = 2;

        GenericSlotResponse slotResponse = result.CastTo<OkObjectResult, GenericSlotResponse>();
        Assert.Equal(expectedElements, slotResponse.Slots.Count);
    }

    [Fact]
    public async Task SlotsBy_ResultsAreOrderedByFirstUploadedTimestampDescending()
    {
        List<SlotEntity> slots = new()
        {
            new SlotEntity
            {
                SlotId = 1,
                CreatorId = 2,
                FirstUploaded = 3,
            },
            new SlotEntity
            {
                SlotId = 2,
                CreatorId = 2,
                FirstUploaded = 1,
            },
            new SlotEntity
            {
                SlotId = 3,
                CreatorId = 2,
                FirstUploaded = 2,
            },
        };
        List<UserEntity> users = new()
        {
            MockHelper.GetUnitTestUser(),
            new UserEntity
            {
                Username = "bytest",
                UserId = 2,
            },
        };
        DatabaseContext db = await MockHelper.GetTestDatabase(new IList[]
        {
            slots, users,
        });
        SlotsController slotsController = new(db);
        slotsController.SetupTestController();

        IActionResult result = await slotsController.SlotsBy("bytest");

        const int expectedElements = 3;
        const int expectedFirstSlotId = 1;
        const int expectedSecondSlotId = 3;
        const int expectedThirdSlotId = 2;

        GenericSlotResponse slotResponse = result.CastTo<OkObjectResult, GenericSlotResponse>();
        Assert.Equal(expectedElements, slotResponse.Slots.Count);

        Assert.Equal(expectedFirstSlotId, ((GameUserSlot)slotResponse.Slots[0]).SlotId);
        Assert.Equal(expectedSecondSlotId, ((GameUserSlot)slotResponse.Slots[1]).SlotId);
        Assert.Equal(expectedThirdSlotId, ((GameUserSlot)slotResponse.Slots[2]).SlotId);
    }
    #endregion

    #region UserSlot
    [Fact]
    public async Task UserSlot_ShouldFetch_WhenSlotIsValid()
    {
        List<SlotEntity> slots = new()
        {
            new SlotEntity
            {
                SlotId = 2,
            },
        };
        DatabaseContext db = await MockHelper.GetTestDatabase(new[]
        {
            slots,
        });
        SlotsController slotsController = new(db);
        slotsController.SetupTestController();

        IActionResult result = await slotsController.UserSlot(2);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UserSlot_ShouldNotFetch_WhenGameVersionMismatch()
    {
        List<SlotEntity> slots = new()
        {
            new SlotEntity
            {
                SlotId = 2,
                GameVersion = GameVersion.LittleBigPlanet2,
            },
        };
        DatabaseContext db = await MockHelper.GetTestDatabase(new[]
        {
            slots,
        });
        SlotsController slotsController = new(db);
        slotsController.SetupTestController();

        IActionResult result = await slotsController.UserSlot(2);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UserSlot_ShouldFetch_WhenGameVersionEqual()
    {
        GameTokenEntity token = MockHelper.GetUnitTestToken();
        token.GameVersion = GameVersion.LittleBigPlanetVita;
        List<GameTokenEntity> tokens = new()
        {
            token,
        };
        List<SlotEntity> slots = new()
        {
            new SlotEntity
            {
                SlotId = 2,
                GameVersion = GameVersion.LittleBigPlanetVita,
            },
        };
        DatabaseContext db = await MockHelper.GetTestDatabase(new IList[]
        {
            slots, tokens,
        });
        SlotsController slotsController = new(db);
        slotsController.SetupTestController(token);

        IActionResult result = await slotsController.UserSlot(2);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UserSlot_ShouldFetch_WhenGameVersionIsGreater()
    {
        GameTokenEntity token = MockHelper.GetUnitTestToken();
        token.GameVersion = GameVersion.LittleBigPlanet3;
        List<GameTokenEntity> tokens = new()
        {
            token,
        };
        List<SlotEntity> slots = new()
        {
            new SlotEntity
            {
                SlotId = 2,
                GameVersion = GameVersion.LittleBigPlanet1,
            },
        };
        DatabaseContext db = await MockHelper.GetTestDatabase(new IList[]
        {
            slots, tokens,
        });
        SlotsController slotsController = new(db);
        slotsController.SetupTestController(token);

        IActionResult result = await slotsController.UserSlot(2);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UserSlot_ShouldReturnNotFound_WhenSlotDoesNotExist()
    {
        DatabaseContext db = await MockHelper.GetTestDatabase();
        SlotsController slotsController = new(db);
        slotsController.SetupTestController();

        IActionResult result = await slotsController.UserSlot(20);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UserSlot_ShouldFetch_WhenSlotIsNotSubLevel()
    {
        List<SlotEntity> slots = new()
        {
            new SlotEntity
            {
                SlotId = 27,
                CreatorId = 4,
                SubLevel = false,
            },
        };
        DatabaseContext db = await MockHelper.GetTestDatabase(new[]
        {
            slots,
        });
        SlotsController slotsController = new(db);
        slotsController.SetupTestController();

        IActionResult result = await slotsController.UserSlot(27);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UserSlot_ShouldNotFetch_WhenSlotIsHidden()
    {
        List<SlotEntity> slots = new()
        {
            new SlotEntity
            {
                SlotId = 27,
                CreatorId = 4,
                Hidden = true,
            },
        };
        DatabaseContext db = await MockHelper.GetTestDatabase(new[]
        {
            slots,
        });
        SlotsController slotsController = new(db);
        slotsController.SetupTestController();

        IActionResult result = await slotsController.UserSlot(27);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UserSlot_ShouldNotFetch_WhenSlotIsWrongType()
    {
        List<SlotEntity> slots = new()
        {
            new SlotEntity
            {
                SlotId = 27,
                Type = SlotType.Developer,
            },
        };
        DatabaseContext db = await MockHelper.GetTestDatabase(new[]
        {
            slots,
        });
        SlotsController slotsController = new(db);
        slotsController.SetupTestController();

        IActionResult result = await slotsController.UserSlot(27);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UserSlot_ShouldNotFetch_WhenSlotIsSubLevel()
    {
        List<SlotEntity> slots = new()
        {
            new SlotEntity
            {
                SlotId = 27,
                CreatorId = 4,
                SubLevel = true,
            },
        };
        DatabaseContext db = await MockHelper.GetTestDatabase(new []{slots,});
        SlotsController slotsController = new(db);
        slotsController.SetupTestController();

        IActionResult result = await slotsController.UserSlot(27);

        Assert.IsType<NotFoundResult>(result);
    }
    #endregion

    #region DeveloperSlot
    [Fact]
    public async Task DeveloperSlot_ShouldFetch_WhenSlotIdIsValid()
    {
        List<SlotEntity> slots = new()
        {
            new SlotEntity
            {
                SlotId = 1,
                InternalSlotId = 25,
                Type = SlotType.Developer,
            },
        };
        DatabaseContext db = await MockHelper.GetTestDatabase(new[]
        {
            slots,
        });
        SlotsController controller = new(db);
        controller.SetupTestController();

        IActionResult result = await controller.DeveloperSlot(25);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task DeveloperSlot_ShouldFetch_WhenSlotIdIsInvalid()
    {
        DatabaseContext db = await MockHelper.GetTestDatabase();
        SlotsController controller = new(db);
        controller.SetupTestController();

        IActionResult result = await controller.DeveloperSlot(26);
        Assert.IsType<OkObjectResult>(result);
    }
    #endregion

}