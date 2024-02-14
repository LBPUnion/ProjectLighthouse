using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers.Slots;
using LBPUnion.ProjectLighthouse.Tests.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Matchmaking.Rooms;
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
            new UserEntity
            {
                Username = "bytest",
                UserId = 2,
            },
            new UserEntity
            {
                Username = "user3",
                UserId = 3,
            },
        };
        DatabaseContext db = await MockHelper.GetTestDatabase(slots, users);
        SlotsController slotsController = new(db);
        slotsController.SetupTestController();

        IActionResult result = await slotsController.SlotsBy("bytest");

        const int expectedElements = 2;
        HashSet<int> expectedSlotIds = new(){1, 2,};

        GenericSlotResponse slotResponse = result.CastTo<OkObjectResult, GenericSlotResponse>();
        Assert.Equal(expectedElements, slotResponse.Slots.Count);
        Assert.Equal(expectedSlotIds, slotResponse.Slots.OfType<GameUserSlot>().Select(s => s.SlotId).ToHashSet());
    }

    [Fact]
    public async Task SlotsBy_ResultsAreOrderedByFirstUploadedTimestampAscending()
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
            new UserEntity
            {
                Username = "bytest",
                UserId = 2,
            },
        };
        DatabaseContext db = await MockHelper.GetTestDatabase(slots, users);
        SlotsController slotsController = new(db);
        slotsController.SetupTestController();

        IActionResult result = await slotsController.SlotsBy("bytest");

        const int expectedElements = 3;
        const int expectedFirstSlotId = 2;
        const int expectedSecondSlotId = 3;
        const int expectedThirdSlotId = 1;

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
                CreatorId = 1,
                SlotId = 2,
            },
        };
        DatabaseContext db = await MockHelper.GetTestDatabase(slots);
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
                CreatorId = 1,
                SlotId = 2,
                GameVersion = GameVersion.LittleBigPlanet2,
            },
        };
        DatabaseContext db = await MockHelper.GetTestDatabase(slots);
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
                CreatorId = 1,
                GameVersion = GameVersion.LittleBigPlanetVita,
            },
        };
        DatabaseContext db = await MockHelper.GetTestDatabase(slots, tokens);
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
                CreatorId = 1,
                GameVersion = GameVersion.LittleBigPlanet1,
            },
        };
        DatabaseContext db = await MockHelper.GetTestDatabase(slots, tokens);
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
                CreatorId = 1,
                SubLevel = false,
            },
        };
        DatabaseContext db = await MockHelper.GetTestDatabase(slots);
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
                CreatorId = 1,
                Hidden = true,
            },
        };
        DatabaseContext db = await MockHelper.GetTestDatabase(slots);
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
                CreatorId = 1,
                Type = SlotType.Developer,
            },
        };
        DatabaseContext db = await MockHelper.GetTestDatabase(slots);
        SlotsController slotsController = new(db);
        slotsController.SetupTestController();

        IActionResult result = await slotsController.UserSlot(27);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UserSlot_ShouldFetch_WhenSlotIsSubLevel()
    {
        List<SlotEntity> slots = new()
        {
            new SlotEntity
            {
                SlotId = 27,
                CreatorId = 1,
                SubLevel = true,
            },
        };
        DatabaseContext db = await MockHelper.GetTestDatabase(slots);
        SlotsController slotsController = new(db);
        slotsController.SetupTestController();

        IActionResult result = await slotsController.UserSlot(27);

        Assert.IsType<OkObjectResult>(result);
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
                CreatorId = 1,
                InternalSlotId = 25,
                Type = SlotType.Developer,
            },
        };
        DatabaseContext db = await MockHelper.GetTestDatabase(slots);
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

    #region BusiestLevels

    // Rather than trying to mock a singleton
    // we just make the unit tests take turns
    private static readonly Mutex roomMutex = new(false);

    private static async Task AddRoom(int slotId, SlotType type, params int[] playerIds)
    {
        await RoomHelper.Rooms.AddAsync(new Room
        {
            PlayerIds = new List<int>(playerIds),
            Slot = new RoomSlot
            {
                SlotId = slotId,
                SlotType = type,
            },
        });
    }

    [Fact]
    public async Task BusiestLevels_ShouldReturnSlots_OrderedByRoomCount()
    {
        roomMutex.WaitOne();
        try
        {
            DatabaseContext db = await MockHelper.GetTestDatabase(new List<SlotEntity>
            {
                new()
                {
                    SlotId = 1,
                    CreatorId = 1,
                    Type = SlotType.User,
                },
                new()
                {
                    SlotId = 2,
                    CreatorId = 1,
                    Type = SlotType.User,
                },
                new()
                {
                    SlotId = 3,
                    CreatorId = 1,
                    Type = SlotType.User,
                },
                new()
                {
                    SlotId = 4,
                    CreatorId = 1,
                    Type = SlotType.Developer,
                    InternalSlotId = 10,
                },
            });
            SlotsController controller = new(db);
            controller.SetupTestController();

            await AddRoom(1, SlotType.User, 1);
            await AddRoom(2, SlotType.User, 2);
            await AddRoom(2, SlotType.User, 3);
            await AddRoom(3, SlotType.User, 4);
            await AddRoom(3, SlotType.User, 5);
            await AddRoom(3, SlotType.User, 6);

            await AddRoom(10, SlotType.Developer, 7);

            IActionResult result = await controller.BusiestLevels();
            GenericSlotResponse slotResponse = result.CastTo<OkObjectResult, GenericSlotResponse>();
            Assert.Equal(3, slotResponse.Slots.Count);
            Assert.IsType<GameUserSlot>(slotResponse.Slots[0]);
            Assert.Equal(3, ((GameUserSlot)slotResponse.Slots[0]).SlotId);
            Assert.IsType<GameUserSlot>(slotResponse.Slots[1]);
            Assert.Equal(2, ((GameUserSlot)slotResponse.Slots[1]).SlotId);
            Assert.IsType<GameUserSlot>(slotResponse.Slots[2]);
            Assert.Equal(1, ((GameUserSlot)slotResponse.Slots[2]).SlotId);
        }
        finally
        {
            roomMutex.ReleaseMutex();
        }
    }

    [Fact]
    public async Task BusiestLevels_ShouldNotIncludeDeveloperSlots()
    {
        roomMutex.WaitOne();
        try
        {
            DatabaseContext db = await MockHelper.GetTestDatabase(new List<SlotEntity>
            {
                new()
                {
                    SlotId = 4,
                    CreatorId = 1,
                    Type = SlotType.Developer,
                    InternalSlotId = 10,
                },
            });
            SlotsController controller = new(db);
            controller.SetupTestController();

            await AddRoom(10, SlotType.Developer, 1);

            IActionResult result = await controller.BusiestLevels();
            GenericSlotResponse slotResponse = result.CastTo<OkObjectResult, GenericSlotResponse>();
            Assert.Empty(slotResponse.Slots);
        }
        finally
        {
            roomMutex.ReleaseMutex();
        }
    }

    [Fact]
    public async Task BusiestLevels_ShouldNotIncludeInvalidSlots()
    {
        roomMutex.WaitOne();
        try
        {
            DatabaseContext db = await MockHelper.GetTestDatabase();
            SlotsController controller = new(db);
            controller.SetupTestController();

            await AddRoom(1, SlotType.User, 1);

            IActionResult result = await controller.BusiestLevels();
            GenericSlotResponse slotResponse = result.CastTo<OkObjectResult, GenericSlotResponse>();
            Assert.Empty(slotResponse.Slots);
        }
        finally
        {
            roomMutex.ReleaseMutex();
        }
    }
    #endregion

    #region Team Picks
    [Fact]
    public async Task TeamPick_ShouldOnlyIncludeTeamPickedLevels()
    {
        DatabaseContext db = await MockHelper.GetTestDatabase(new List<SlotEntity>
        {
            new()
            {
                SlotId = 1,
                CreatorId = 1,
                TeamPickTime = 1,
            },
            new()
            {
                SlotId = 2,
                CreatorId = 1,
                TeamPickTime = 0,
            },
        });
        SlotsController controller = new(db);
        controller.SetupTestController();

        IActionResult result = await controller.TeamPickedSlots();
        GenericSlotResponse slotResponse = result.CastTo<OkObjectResult, GenericSlotResponse>();
        Assert.Single(slotResponse.Slots);
        Assert.Equal(1, slotResponse.Slots.OfType<GameUserSlot>().First().SlotId);
    }

    [Fact]
    public async Task TeamPick_LevelsAreSortedByTimestamp()
    {
        DatabaseContext db = await MockHelper.GetTestDatabase(new List<SlotEntity>
        {
            new()
            {
                SlotId = 1,
                CreatorId = 1,
                TeamPickTime = 1,
            },
            new()
            {
                SlotId = 2,
                CreatorId = 1,
                TeamPickTime = 5,
            },
        });
        SlotsController controller = new(db);
        controller.SetupTestController();

        IActionResult result = await controller.TeamPickedSlots();

        GenericSlotResponse slotResponse = result.CastTo<OkObjectResult, GenericSlotResponse>();
        Assert.Equal(2, slotResponse.Slots.Count);
        Assert.Equal(2, slotResponse.Slots.OfType<GameUserSlot>().First().SlotId);
        Assert.Equal(1, slotResponse.Slots.OfType<GameUserSlot>().Last().SlotId);
    }
    #endregion
}
