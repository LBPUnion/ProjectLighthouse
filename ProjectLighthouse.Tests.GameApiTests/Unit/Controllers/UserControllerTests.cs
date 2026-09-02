using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;
using LBPUnion.ProjectLighthouse.Tests.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Unit.Controllers;

[Trait("Category", "Unit")]
public class UserControllerTests
{
    private static GameTokenEntity GetLbp2Token()
    {
        GameTokenEntity token = MockHelper.GetUnitTestToken();
        token.GameVersion = GameVersion.LittleBigPlanet2;
        return token;
    }

    private static GameTokenEntity GetLbp3Token()
    {
        GameTokenEntity token = MockHelper.GetUnitTestToken();
        token.GameVersion = GameVersion.LittleBigPlanet3;
        return token;
    }

    private static GameTokenEntity GetVitaToken()
    {
        GameTokenEntity token = MockHelper.GetUnitTestToken();
        token.GameVersion = GameVersion.LittleBigPlanetVita;
        return token;
    }

    [Fact]
    public async Task GetUser_WithValidUser_ShouldReturnUser()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        UserController userController = new(dbMock);
        userController.SetupTestController();

        const int expectedId = 1;

        IActionResult result = await userController.GetUser("unittest");

        GameUser gameUser = result.CastTo<OkObjectResult, GameUser>();
        Assert.Equal(expectedId, gameUser.UserId);
    }

    [Fact]
    public async Task GetUser_WithInvalidUser_ShouldReturnNotFound()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        UserController userController = new(dbMock);
        userController.SetupTestController();

        IActionResult result = await userController.GetUser("notfound");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetUserAlt_WithInvalidUser_ShouldReturnEmptyList()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        UserController userController = new(dbMock);
        userController.SetupTestController();

        IActionResult result = await userController.GetUserAlt(new[]{"notfound",});

        MinimalUserListResponse userList = result.CastTo<OkObjectResult, MinimalUserListResponse>();
        Assert.Empty(userList.Users);
    }

    [Fact]
    public async Task GetUserAlt_WithOnlyInvalidUsers_ShouldReturnEmptyList()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        UserController userController = new(dbMock);
        userController.SetupTestController();

        IActionResult result = await userController.GetUserAlt(new[]
        {
            "notfound", "notfound2", "notfound3",
        });

        MinimalUserListResponse userList = result.CastTo<OkObjectResult, MinimalUserListResponse>();
        Assert.Empty(userList.Users);
    }

    [Fact]
    public async Task GetUserAlt_WithTwoInvalidUsers_AndOneValidUser_ShouldReturnOne()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        UserController userController = new(dbMock);
        userController.SetupTestController();


        IActionResult result = await userController.GetUserAlt(new[]
        {
            "notfound", "unittest", "notfound3",
        });

        MinimalUserListResponse userList = result.CastTo<OkObjectResult, MinimalUserListResponse>();
        Assert.Single(userList.Users);
    }

    [Fact]
    public async Task GetUserAlt_WithTwoValidUsers_ShouldReturnTwo()
    {
        List<UserEntity> users = new()
        {
            MockHelper.GetUnitTestUser(),
            new UserEntity
            {
                UserId = 2,
                Username = "unittest2",
            },
        };

        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase(users);

        UserController userController = new(dbMock);
        userController.SetupTestController();

        const int expectedLength = 2;

        IActionResult result = await userController.GetUserAlt(new[]
        {
            "unittest2", "unittest",
        });

        MinimalUserListResponse userList = result.CastTo<OkObjectResult, MinimalUserListResponse>();
        Assert.Equal(expectedLength, userList.Users.Count);
    }

    [Fact]
    public async Task UpdateMyPins_ShouldReturnBadRequest_WhenBodyIsInvalid()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        UserController userController = new(dbMock);
        userController.SetupTestController(GetLbp2Token(), "{");

        IActionResult result = await userController.UpdateMyPins();

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task UpdateMyPins_ShouldUpdatePins()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        UserController userController = new(dbMock);
        userController.SetupTestController(GetLbp2Token(), "{\"profile_pins\": [1234]}");

        IActionResult result = await userController.UpdateMyPins();

        JsonResult jsonResult = Assert.IsType<JsonResult>(result);

        UserProfilePinsEntity profilePins = dbMock.UserProfilePins.Single();

        Assert.Equal(1, profilePins.UserId);
        Assert.Equal(GameVersion.LittleBigPlanet2, profilePins.GameVersion);
        Assert.Equal("1234", profilePins.Pins);

        Assert.Equal("text/json", jsonResult.ContentType);
    }

    [Fact]
    public async Task UpdateMyPins_ShouldLeaveLegacyPinsUnchanged()
    {
        UserEntity entity = MockHelper.GetUnitTestUser();
        entity.Pins = "1234";
        List<UserEntity> users = new()
        {
            entity,
        };
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase(users);

        UserController userController = new(dbMock);
        userController.SetupTestController(GetLbp2Token(), "{\"profile_pins\": [5678]}");

        IActionResult result = await userController.UpdateMyPins();

        Assert.IsType<JsonResult>(result);

        UserProfilePinsEntity profilePins = dbMock.UserProfilePins.Single();

        Assert.Equal(GameVersion.LittleBigPlanet2, profilePins.GameVersion);
        Assert.Equal("5678", profilePins.Pins);

        Assert.Equal("1234", dbMock.Users.First().Pins);
    }

    [Fact]
    public async Task UpdateMyPins_ShouldRejectDuplicateProfilePins()
    {
        UserEntity entity = MockHelper.GetUnitTestUser();
        entity.Pins = "1234";
        List<UserEntity> users = new()
        {
            entity,
        };
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase(users);

        UserController userController = new(dbMock);
        userController.SetupTestController(GetLbp2Token(), "{\"profile_pins\": [1234, 1234]}");

        IActionResult result = await userController.UpdateMyPins();

        Assert.IsType<BadRequestResult>(result);
        Assert.Empty(dbMock.UserProfilePins);

        Assert.Equal("1234", dbMock.Users.First().Pins);
    }

    [Fact]
    public async Task UpdateMyPins_ShouldNotDowngradeStoredProgress()
    {
        List<UserPinProgressEntity> progress = new()
        {
            new UserPinProgressEntity
            {
                UserId = 1,
                PinSet = PinSet.LittleBigPlanet,
                ProgressType = 1234,
                Value = 10,
            },
        };

        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase(progress);

        UserController userController = new(dbMock);
        userController.SetupTestController(GetLbp2Token(), "{\"progress\": [1234, 5]}");

        IActionResult result = await userController.UpdateMyPins();

        Assert.IsType<JsonResult>(result);

        UserPinProgressEntity storedProgress = dbMock.UserPinProgress.Single();

        Assert.Equal(1234u, storedProgress.ProgressType);
        Assert.Equal(10, storedProgress.Value);
    }

    [Fact]
    public async Task UpdateMyPins_ShouldAcceptLowerValue_WhenLowerProgressIsBetter()
    {
        const uint progressType = 191183438u;

        List<UserPinProgressEntity> progress = new()
        {
            new UserPinProgressEntity
            {
                UserId = 1,
                PinSet = PinSet.LittleBigPlanet,
                ProgressType = progressType,
                Value = 10,
            },
        };

        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase(progress);

        UserController userController = new(dbMock);
        userController.SetupTestController(GetLbp2Token(), "{\"progress\": [191183438, 5]}");

        IActionResult result = await userController.UpdateMyPins();

        Assert.IsType<JsonResult>(result);

        UserPinProgressEntity storedProgress = dbMock.UserPinProgress.Single();

        Assert.Equal(progressType, storedProgress.ProgressType);
        Assert.Equal(5, storedProgress.Value);
    }

    [Fact]
    public async Task UpdateMyPins_ShouldNotReplaceBetterLowerProgressWithWorseValue()
    {
        const uint progressType = 191183438u;

        List<UserPinProgressEntity> progress = new()
        {
            new UserPinProgressEntity
            {
                UserId = 1,
                PinSet = PinSet.LittleBigPlanet,
                ProgressType = progressType,
                Value = 5,
            },
        };

        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase(progress);

        UserController userController = new(dbMock);
        userController.SetupTestController(GetLbp2Token(), "{\"progress\": [191183438, 10]}");

        IActionResult result = await userController.UpdateMyPins();

        Assert.IsType<JsonResult>(result);

        UserPinProgressEntity storedProgress = dbMock.UserPinProgress.Single();

        Assert.Equal(progressType, storedProgress.ProgressType);
        Assert.Equal(5, storedProgress.Value);
    }

    [Fact]
    public async Task UpdateMyPins_ShouldReturnStoredProgress_WhenClientUploadsWorseValue()
    {
        const uint progressType = 1234u;

        List<UserPinProgressEntity> progress = new()
        {
            new UserPinProgressEntity
            {
                UserId = 1,
                PinSet = PinSet.LittleBigPlanet,
                ProgressType = progressType,
                Value = 10,
            },
        };

        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase(progress);

        UserController userController = new(dbMock);
        userController.SetupTestController(GetLbp2Token(), "{\"progress\": [1234, 5]}");

        IActionResult result = await userController.UpdateMyPins();

        JsonResult jsonResult = Assert.IsType<JsonResult>(result);

        string json = JsonSerializer.Serialize(jsonResult.Value);

        using JsonDocument document = JsonDocument.Parse(json);

        JsonElement responseProgress = document.RootElement.GetProperty("progress");

        JsonElement responseAwards = document.RootElement.GetProperty("awards");

        Assert.Equal(2, responseProgress.GetArrayLength());
        Assert.Equal(progressType, responseProgress[0].GetUInt32());
        Assert.Equal(10, responseProgress[1].GetDouble());

        Assert.Equal(2, responseAwards.GetArrayLength());
        Assert.Equal(progressType, responseAwards[0].GetUInt32());
        Assert.Equal(10, responseAwards[1].GetDouble());
    }

    [Fact]
    public async Task UpdateMyPins_ShouldUseHigherValue_WhenProgressAndAwardsOverlap()
    {
        const uint progressType = 1234u;

        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        UserController userController = new(dbMock);
        userController.SetupTestController(GetLbp2Token(), "{\"progress\": [1234, 5], \"awards\": [1234, 9]}");

        IActionResult result = await userController.UpdateMyPins();

        Assert.IsType<JsonResult>(result);

        UserPinProgressEntity storedProgress = dbMock.UserPinProgress.Single();

        Assert.Equal(progressType, storedProgress.ProgressType);
        Assert.Equal(9, storedProgress.Value);
    }

    [Fact]
    public async Task UpdateMyPins_ShouldPersistFractionalProgress()
    {
        const uint progressType = 1234u;
        const double expectedValue = 5.5;

        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        UserController userController = new(dbMock);
        userController.SetupTestController(GetLbp2Token(), "{\"progress\": [1234, 5.5]}");

        IActionResult result = await userController.UpdateMyPins();

        Assert.IsType<JsonResult>(result);

        UserPinProgressEntity storedProgress = dbMock.UserPinProgress.Single();

        Assert.Equal(progressType, storedProgress.ProgressType);
        Assert.Equal(expectedValue, storedProgress.Value);
    }

    [Fact]
    public async Task UpdateMyPins_ShouldReturnBadRequest_WhenProgressArrayHasOddLength()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        UserController userController = new(dbMock);
        userController.SetupTestController(GetLbp2Token(), "{\"progress\": [1234, 5, 5678]}");

        IActionResult result = await userController.UpdateMyPins();

        Assert.IsType<BadRequestResult>(result);
        Assert.Empty(dbMock.UserPinProgress);
    }

    [Theory]
    [InlineData("{\"awards\": [1234]}")]
    [InlineData("{\"progress\": [1234, 1, 1234, 2]}")]
    [InlineData("{\"awards\": [1234, 1, 1234, 2]}")]
    [InlineData("{\"progress\": [-1, 5]}")]
    [InlineData("{\"progress\": [4294967296, 5]}")]
    [InlineData("{\"progress\": [1234, \"5\"]}")]
    [InlineData("{\"awards\": [1234, -1]}")]
    [InlineData("{\"awards\": [1234, 1.5]}")]
    public async Task UpdateMyPins_ShouldReturnBadRequest_WhenPinDataIsMalformed(string body)
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        UserController userController = new(dbMock);
        userController.SetupTestController(GetLbp2Token(), body);

        IActionResult result = await userController.UpdateMyPins();

        Assert.IsType<BadRequestResult>(result);
        Assert.Empty(dbMock.UserPinProgress);
        Assert.Empty(dbMock.UserProfilePins);
    }

    [Fact]
    public async Task UpdateMyPins_ShouldReturnBadRequest_WhenMoreThanThreeProfilePinsAreUploaded()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        UserController userController = new(dbMock);
        userController.SetupTestController(GetLbp2Token(), "{\"profile_pins\": [1, 2, 3, 4]}");

        IActionResult result = await userController.UpdateMyPins();

        Assert.IsType<BadRequestResult>(result);
        Assert.Empty(dbMock.UserProfilePins);
    }

    [Fact]
    public async Task UpdateMyPins_ShouldKeepProfilePins_WhenProfilePinsAreOmitted()
    {
        List<UserProfilePinsEntity> profilePins = new()
        {
            new UserProfilePinsEntity
            {
                UserId = 1, GameVersion = GameVersion.LittleBigPlanet2, Pins = "111,222,333",
            },
        };

        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase(profilePins);

        UserController userController = new(dbMock);
        userController.SetupTestController(GetLbp2Token(), "{\"progress\": [1234, 5]}");

        IActionResult result = await userController.UpdateMyPins();

        Assert.IsType<JsonResult>(result);

        UserProfilePinsEntity storedPins = dbMock.UserProfilePins.Single();

        Assert.Equal("111,222,333", storedPins.Pins);
    }

    [Fact]
    public async Task UpdateMyPins_ShouldClearProfilePins_WhenEmptyArrayIsUploaded()
    {
        List<UserProfilePinsEntity> profilePins = new()
        {
            new UserProfilePinsEntity
            {
                UserId = 1, GameVersion = GameVersion.LittleBigPlanet2, Pins = "111,222,333",
            },
        };

        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase(profilePins);

        UserController userController = new(dbMock);
        userController.SetupTestController(GetLbp2Token(), "{\"profile_pins\": []}");

        IActionResult result = await userController.UpdateMyPins();

        Assert.IsType<JsonResult>(result);

        UserProfilePinsEntity storedPins = dbMock.UserProfilePins.Single();

        Assert.Equal(string.Empty, storedPins.Pins);
    }

    [Fact]
    public async Task UpdateMyPins_ShouldPreserveProgressMissingFromSparseUpload()
    {
        List<UserPinProgressEntity> progress = new()
        {
            new UserPinProgressEntity
            {
                UserId = 1, PinSet = PinSet.LittleBigPlanet, ProgressType = 1234, Value = 10,
            },
            new UserPinProgressEntity
            {
                UserId = 1, PinSet = PinSet.LittleBigPlanet, ProgressType = 5678, Value = 20,
            },
        };

        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase(progress);

        UserController userController = new(dbMock);
        userController.SetupTestController(GetLbp2Token(), "{\"progress\": [1234, 15]}");

        IActionResult result = await userController.UpdateMyPins();

        Assert.IsType<JsonResult>(result);

        List<UserPinProgressEntity> storedProgress = dbMock.UserPinProgress
            .OrderBy(p => p.ProgressType)
            .ToList();

        Assert.Equal(2, storedProgress.Count);

        Assert.Equal(1234u, storedProgress[0].ProgressType);
        Assert.Equal(15, storedProgress[0].Value);

        Assert.Equal(5678u, storedProgress[1].ProgressType);
        Assert.Equal(20, storedProgress[1].Value);
    }

    [Fact]
    public async Task UpdateMyPins_ShouldShareProgressBetweenLbp2AndLbp3()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        UserController lbp2Controller = new(dbMock);
        lbp2Controller.SetupTestController(GetLbp2Token(), "{\"progress\": [1234, 10]}");

        IActionResult lbp2Result = await lbp2Controller.UpdateMyPins();

        Assert.IsType<JsonResult>(lbp2Result);

        UserController lbp3Controller = new(dbMock);
        lbp3Controller.SetupTestController(GetLbp3Token(), "{\"progress\": [1234, 5]}");

        IActionResult lbp3Result = await lbp3Controller.UpdateMyPins();

        Assert.IsType<JsonResult>(lbp3Result);

        List<UserPinProgressEntity> storedProgress = dbMock.UserPinProgress.ToList();

        Assert.Single(storedProgress);
        Assert.Equal(PinSet.LittleBigPlanet, storedProgress[0].PinSet);
        Assert.Equal(1234u, storedProgress[0].ProgressType);
        Assert.Equal(10, storedProgress[0].Value);
    }

    [Fact]
    public async Task UpdateMyPins_ShouldKeepVitaProgressSeparateFromLbp2AndLbp3()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        UserController lbp2Controller = new(dbMock);
        lbp2Controller.SetupTestController(GetLbp2Token(), "{\"progress\": [1234, 10]}");

        IActionResult lbp2Result = await lbp2Controller.UpdateMyPins();

        Assert.IsType<JsonResult>(lbp2Result);

        UserController vitaController = new(dbMock);
        vitaController.SetupTestController(GetVitaToken(), "{\"progress\": [1234, 3]}");

        IActionResult vitaResult = await vitaController.UpdateMyPins();

        Assert.IsType<JsonResult>(vitaResult);

        List<UserPinProgressEntity> storedProgress = dbMock.UserPinProgress.ToList();

        Assert.Equal(2, storedProgress.Count);

        UserPinProgressEntity lbpProgress = storedProgress.Single(p => p.PinSet == PinSet.LittleBigPlanet);

        UserPinProgressEntity vitaProgress = storedProgress.Single(p => p.PinSet == PinSet.Vita);

        Assert.Equal(10, lbpProgress.Value);
        Assert.Equal(3, vitaProgress.Value);
    }

    [Fact]
    public async Task UpdateMyPins_ShouldUseLowerValue_ForCommunityLowerIsBetterProgressType()
    {
        const uint progressType = 2033315234u;

        List<UserPinProgressEntity> progress = new()
        {
            new UserPinProgressEntity
            {
                UserId = 1, PinSet = PinSet.LittleBigPlanet, ProgressType = progressType, Value = 50,
            },
        };

        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase(progress);

        UserController userController = new(dbMock);
        userController.SetupTestController(
            GetLbp2Token(),
            "{\"progress\": [2033315234, 25]}");

        IActionResult result = await userController.UpdateMyPins();

        Assert.IsType<JsonResult>(result);

        UserPinProgressEntity storedProgress = dbMock.UserPinProgress.Single();

        Assert.Equal(progressType, storedProgress.ProgressType);
        Assert.Equal(25, storedProgress.Value);
    }

    [Fact]
    public async Task UpdateMyPins_ShouldStoreProfilePinsSeparatelyForLbp2AndLbp3()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        UserController lbp2Controller = new(dbMock);
        lbp2Controller.SetupTestController(GetLbp2Token(), "{\"profile_pins\": [111]}");

        IActionResult lbp2Result = await lbp2Controller.UpdateMyPins();

        Assert.IsType<JsonResult>(lbp2Result);

        UserController lbp3Controller = new(dbMock);
        lbp3Controller.SetupTestController(GetLbp3Token(), "{\"profile_pins\": [222]}");

        IActionResult lbp3Result = await lbp3Controller.UpdateMyPins();

        Assert.IsType<JsonResult>(lbp3Result);

        List<UserProfilePinsEntity> profilePins = dbMock.UserProfilePins.ToList();

        Assert.Equal(2, profilePins.Count);

        UserProfilePinsEntity lbp2Pins = profilePins.Single(p => p.GameVersion == GameVersion.LittleBigPlanet2);

        UserProfilePinsEntity lbp3Pins = profilePins.Single(p => p.GameVersion == GameVersion.LittleBigPlanet3);

        Assert.Equal("111", lbp2Pins.Pins);
        Assert.Equal("222", lbp3Pins.Pins);
    }

    [Fact]
    public async Task GetUser_ShouldReturnGameSpecificProfilePins()
    {
        UserEntity user = MockHelper.GetUnitTestUser();
        user.Pins = "111";

        List<UserEntity> users = [user];

        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase(users);

        dbMock.UserProfilePins.Add(new UserProfilePinsEntity
        {
            UserId = user.UserId,
            GameVersion = GameVersion.LittleBigPlanet2,
            Pins = "222,333",
        });

        await dbMock.SaveChangesAsync();

        UserController controller = new(dbMock);
        controller.SetupTestController(GetLbp2Token());

        IActionResult result = await controller.GetUser(user.Username);

        GameUser gameUser = result.CastTo<OkObjectResult, GameUser>();

        Assert.Equal("222,333", gameUser.ProfilePins);
    }

    [Fact]
    public async Task GetUser_ShouldFallBackToLegacyProfilePins()
    {
        UserEntity user = MockHelper.GetUnitTestUser();
        user.Pins = "111,222,333";

        List<UserEntity> users = [user];

        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase(users);

        UserController controller = new(dbMock);
        controller.SetupTestController(GetLbp2Token());

        IActionResult result = await controller.GetUser(user.Username);

        GameUser gameUser = result.CastTo<OkObjectResult, GameUser>();

        Assert.Equal("111,222,333", gameUser.ProfilePins);
    }
}
