using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;
using LBPUnion.ProjectLighthouse.Tests.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Unit.Controllers;

[Trait("Category", "Unit")]
public class UserControllerTests
{
    [Fact]
    public async void GetUser_WithValidUser_ShouldReturnUser()
    {
        Mock<DatabaseContext> dbMock = MockHelper.GetDatabaseMock();

        UserController userController = new(dbMock.Object);
        userController.SetupTestController();

        const int expectedId = 1;
        const int expectedStatus = 200;

        IActionResult result = await userController.GetUser("unittest");
        OkObjectResult? okObject = result as OkObjectResult;
        Assert.NotNull(okObject);
        Assert.Equal(expectedStatus, okObject.StatusCode);
        GameUser? gameUser = okObject.Value as GameUser;
        Assert.NotNull(gameUser);
        Assert.Equal(expectedId, gameUser.UserId);
    }

    [Fact]
    public async void GetUser_WithInvalidUser_ShouldReturnNotFound()
    {
        Mock<DatabaseContext> dbMock = MockHelper.GetDatabaseMock();

        UserController userController = new(dbMock.Object);
        userController.SetupTestController();

        const int expectedStatus = 404;

        IActionResult result = await userController.GetUser("notfound");
        NotFoundResult? okObject = result as NotFoundResult;
        Assert.NotNull(okObject);
        Assert.Equal(expectedStatus, okObject.StatusCode);
    }

    [Fact]
    public async void GetUserAlt_WithInvalidUser_ShouldReturnEmptyList()
    {
        Mock<DatabaseContext> dbMock = MockHelper.GetDatabaseMock();

        UserController userController = new(dbMock.Object);
        userController.SetupTestController();

        const int expectedStatus = 200;

        IActionResult result = await userController.GetUserAlt(new[]{"notfound",});
        OkObjectResult? okObject = result as OkObjectResult;
        Assert.NotNull(okObject);
        Assert.Equal(expectedStatus, okObject.StatusCode);
        MinimalUserListResponse? userList = okObject.Value as MinimalUserListResponse? ?? default;
        Assert.NotNull(userList);
        Assert.Empty(userList.Value.Users);
    }

    [Fact]
    public async void GetUserAlt_WithOnlyInvalidUsers_ShouldReturnEmptyList()
    {
        Mock<DatabaseContext> dbMock = MockHelper.GetDatabaseMock();

        UserController userController = new(dbMock.Object);
        userController.SetupTestController();

        const int expectedStatus = 200;

        IActionResult result = await userController.GetUserAlt(new[]
        {
            "notfound", "notfound2", "notfound3",
        });
        OkObjectResult? okObject = result as OkObjectResult;
        Assert.NotNull(okObject);
        Assert.Equal(expectedStatus, okObject.StatusCode);
        MinimalUserListResponse? userList = okObject.Value as MinimalUserListResponse? ?? default;
        Assert.NotNull(userList);
        Assert.Empty(userList.Value.Users);
    }

    [Fact]
    public async void GetUserAlt_WithTwoInvalidUsers_AndOneValidUser_ShouldReturnOne()
    {
        Mock<DatabaseContext> dbMock = MockHelper.GetDatabaseMock();

        UserController userController = new(dbMock.Object);
        userController.SetupTestController();

        const int expectedStatus = 200;
        const int expectedLength = 1;

        IActionResult result = await userController.GetUserAlt(new[]
        {
            "notfound", "unittest", "notfound3",
        });
        OkObjectResult? okObject = result as OkObjectResult;
        Assert.NotNull(okObject);
        Assert.Equal(expectedStatus, okObject.StatusCode);
        MinimalUserListResponse? userList = okObject.Value as MinimalUserListResponse? ?? default;
        Assert.NotNull(userList);
        Assert.Single(userList.Value.Users);
    }

    [Fact]
    public async void GetUserAlt_WithTwoValidUsers_ShouldReturnTwo()
    {
        Mock<DatabaseContext> dbMock = MockHelper.GetDatabaseMock();
        dbMock.Setup(x => x.Users)
            .ReturnsDbSet(new List<UserEntity>
            {
                MockHelper.GetUnitTestUser(),
                new()
                {
                    UserId = 2,
                    Username = "unittest2",
                },
            });

        UserController userController = new(dbMock.Object);
        userController.SetupTestController();

        const int expectedStatus = 200;
        const int expectedLength = 2;

        IActionResult result = await userController.GetUserAlt(new[]
        {
            "unittest2", "unittest",
        });
        OkObjectResult? okObject = result as OkObjectResult;
        Assert.NotNull(okObject);
        Assert.Equal(expectedStatus, okObject.StatusCode);
        MinimalUserListResponse? userList = okObject.Value as MinimalUserListResponse? ?? default;
        Assert.NotNull(userList);
        Assert.Equal(expectedLength, userList.Value.Users.Count);
    }

    [Fact]
    public async void UpdateMyPins_ShouldReturnBadRequest_WhenBodyIsInvalid()
    {
        Mock<DatabaseContext> dbMock = MockHelper.GetDatabaseMock();

        UserController userController = new(dbMock.Object);
        userController.SetupTestController("{}");

        const int expectedStatus = 400;

        IActionResult result = await userController.UpdateMyPins();
        BadRequestResult? okObject = result as BadRequestResult;
        Assert.NotNull(okObject);
        Assert.Equal(expectedStatus, okObject.StatusCode);
    }

    [Fact]
    public async void UpdateMyPins_ShouldUpdatePins()
    {
        Mock<DatabaseContext> dbMock = MockHelper.GetDatabaseMock();

        UserController userController = new(dbMock.Object);
        userController.SetupTestController("{\"profile_pins\": [1234]}");

        const int expectedStatus = 200;
        const string expectedPins = "1234";
        const string expectedResponse = "[{\"StatusCode\":200}]";

        IActionResult result = await userController.UpdateMyPins();
        OkObjectResult? okObject = result as OkObjectResult;
        Assert.NotNull(okObject);
        Assert.Equal(expectedStatus, okObject.StatusCode);
        Assert.Equal(expectedPins, dbMock.Object.Users.First().Pins);
        Assert.Equal(expectedResponse, okObject.Value);
    }

    [Fact]
    public async void UpdateMyPins_ShouldNotSave_WhenPinsAreEqual()
    {
        UserEntity entity = MockHelper.GetUnitTestUser();
        entity.Pins = "1234";
        List<UserEntity> users = new()
        {
            entity
        };
        Mock<DatabaseContext> dbMock = MockHelper.GetDatabaseMock(users);

        UserController userController = new(dbMock.Object);
        userController.SetupTestController("{\"profile_pins\": [1234]}");

        const int expectedStatus = 200;
        const string expectedPins = "1234";
        const string expectedResponse = "[{\"StatusCode\":200}]";

        IActionResult result = await userController.UpdateMyPins();
        OkObjectResult? okObject = result as OkObjectResult;
        Assert.NotNull(okObject);
        Assert.Equal(expectedStatus, okObject.StatusCode);
        Assert.Equal(expectedPins, dbMock.Object.Users.First().Pins);
        Assert.Equal(expectedResponse, okObject.Value);
        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}