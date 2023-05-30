using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;
using LBPUnion.ProjectLighthouse.Tests.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Unit.Controllers;

[Trait("Category", "Unit")]
public class UserControllerTests
{
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
        userController.SetupTestController("{}");


        IActionResult result = await userController.UpdateMyPins();

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task UpdateMyPins_ShouldUpdatePins()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        UserController userController = new(dbMock);
        userController.SetupTestController("{\"profile_pins\": [1234]}");

        const string expectedPins = "1234";
        const string expectedResponse = "[{\"StatusCode\":200}]";

        IActionResult result = await userController.UpdateMyPins();

        string pinsResponse = result.CastTo<OkObjectResult, string>();
        Assert.Equal(expectedPins, dbMock.Users.First().Pins);
        Assert.Equal(expectedResponse, pinsResponse);
    }

    [Fact]
    public async Task UpdateMyPins_ShouldNotSave_WhenPinsAreEqual()
    {
        UserEntity entity = MockHelper.GetUnitTestUser();
        entity.Pins = "1234";
        List<UserEntity> users = new()
        {
            entity,
        };
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase(users);

        UserController userController = new(dbMock);
        userController.SetupTestController("{\"profile_pins\": [1234]}");

        const string expectedPins = "1234";
        const string expectedResponse = "[{\"StatusCode\":200}]";

        IActionResult result = await userController.UpdateMyPins();

        string pinsResponse = result.CastTo<OkObjectResult, string>();

        Assert.Equal(expectedPins, dbMock.Users.First().Pins);
        Assert.Equal(expectedResponse, pinsResponse);
    }
}