using System.Collections.Generic;
using System.Linq;
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
    public async void GetUser_WithValidUser_ShouldReturnUser()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        UserController userController = new(dbMock);
        userController.SetupTestController();

        const int expectedId = 1;

        IActionResult result = await userController.GetUser("unittest");

        Assert.IsType<OkObjectResult>(result);
        OkObjectResult? okObject = result as OkObjectResult;
        Assert.NotNull(okObject);
        GameUser? gameUser = okObject.Value as GameUser;
        Assert.NotNull(gameUser);
        Assert.Equal(expectedId, gameUser.UserId);
    }

    [Fact]
    public async void GetUser_WithInvalidUser_ShouldReturnNotFound()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        UserController userController = new(dbMock);
        userController.SetupTestController();

        IActionResult result = await userController.GetUser("notfound");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async void GetUserAlt_WithInvalidUser_ShouldReturnEmptyList()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        UserController userController = new(dbMock);
        userController.SetupTestController();

        IActionResult result = await userController.GetUserAlt(new[]{"notfound",});

        Assert.IsType<OkObjectResult>(result);
        OkObjectResult? okObject = result as OkObjectResult;
        Assert.NotNull(okObject);
        MinimalUserListResponse? userList = okObject.Value as MinimalUserListResponse? ?? default;
        Assert.NotNull(userList);
        Assert.Empty(userList.Value.Users);
    }

    [Fact]
    public async void GetUserAlt_WithOnlyInvalidUsers_ShouldReturnEmptyList()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        UserController userController = new(dbMock);
        userController.SetupTestController();

        IActionResult result = await userController.GetUserAlt(new[]
        {
            "notfound", "notfound2", "notfound3",
        });

        Assert.IsType<OkObjectResult>(result);
        OkObjectResult? okObject = result as OkObjectResult;
        Assert.NotNull(okObject);
        MinimalUserListResponse? userList = okObject.Value as MinimalUserListResponse? ?? default;
        Assert.NotNull(userList);
        Assert.Empty(userList.Value.Users);
    }

    [Fact]
    public async void GetUserAlt_WithTwoInvalidUsers_AndOneValidUser_ShouldReturnOne()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        UserController userController = new(dbMock);
        userController.SetupTestController();


        IActionResult result = await userController.GetUserAlt(new[]
        {
            "notfound", "unittest", "notfound3",
        });

        Assert.IsType<OkObjectResult>(result);
        OkObjectResult? okObject = result as OkObjectResult;
        Assert.NotNull(okObject);
        MinimalUserListResponse? userList = okObject.Value as MinimalUserListResponse? ?? default;
        Assert.NotNull(userList);
        Assert.Single(userList.Value.Users);
    }

    [Fact]
    public async void GetUserAlt_WithTwoValidUsers_ShouldReturnTwo()
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

        Assert.IsType<OkObjectResult>(result);
        OkObjectResult? okObject = result as OkObjectResult;
        Assert.NotNull(okObject);
        MinimalUserListResponse? userList = okObject.Value as MinimalUserListResponse? ?? default;
        Assert.NotNull(userList);
        Assert.Equal(expectedLength, userList.Value.Users.Count);
    }

    [Fact]
    public async void UpdateMyPins_ShouldReturnBadRequest_WhenBodyIsInvalid()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        UserController userController = new(dbMock);
        userController.SetupTestController("{}");


        IActionResult result = await userController.UpdateMyPins();

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async void UpdateMyPins_ShouldUpdatePins()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        UserController userController = new(dbMock);
        userController.SetupTestController("{\"profile_pins\": [1234]}");

        const string expectedPins = "1234";
        const string expectedResponse = "[{\"StatusCode\":200}]";

        IActionResult result = await userController.UpdateMyPins();

        Assert.IsType<OkObjectResult>(result);
        OkObjectResult? okObject = result as OkObjectResult;
        Assert.NotNull(okObject);
        Assert.Equal(expectedPins, dbMock.Users.First().Pins);
        Assert.Equal(expectedResponse, okObject.Value);
    }

    [Fact]
    public async void UpdateMyPins_ShouldNotSave_WhenPinsAreEqual()
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

        Assert.IsType<OkObjectResult>(result);
        OkObjectResult? okObject = result as OkObjectResult;
        Assert.NotNull(okObject);
        Assert.Equal(expectedPins, dbMock.Users.First().Pins);
        Assert.Equal(expectedResponse, okObject.Value);
    }
}