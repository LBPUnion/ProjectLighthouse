using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Unit;

[Trait("Category", "Unit")]
public class MessageControllerTests
{

    private static UserEntity getUnitTestUser() =>
        new()
        {
            Username = "unittest",
            UserId = 1,
        };

    private static GameTokenEntity getUnitTestToken() =>
        new()
        {
            Platform = Platform.UnitTest,
            UserId = 1,
            ExpiresAt = DateTime.MaxValue,
            TokenId = 1,
            UserLocation = "127.0.0.1",
            UserToken = "unittest",
        };

    private static Mock<DatabaseContext> getDatabaseMock()
    {
        List<UserEntity> users = new()
        {
            getUnitTestUser(),
        };
        Mock<DatabaseContext> mock = new();
        mock.SetupGet(x => x.Users).ReturnsDbSet(users);
        return mock;
    }

    [Fact]
    public void Eula_ShouldReturnLicense_WhenConfigEmpty()
    {
        Mock<DatabaseContext> dbMock = getDatabaseMock();
        MessageController messageController = new(dbMock.Object)
        {
            ControllerContext = MockHelper.GetMockControllerContext(),
        };
        MockHelper.SetupTestGameToken(messageController, getUnitTestToken());

        ServerConfiguration.Instance.EulaText = "";

        const int expectedStatus = 200;
        const string expected = @"
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation, either version 3 of the
License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
";

        IActionResult result = messageController.Eula();
        OkObjectResult? okObjectResult = result as OkObjectResult;
        Assert.NotNull(okObjectResult);
        Assert.Equal(expectedStatus, okObjectResult.StatusCode);
        Assert.NotNull(okObjectResult.Value);
        Assert.Equal(expected, (string)okObjectResult.Value);
    }

    [Fact]
    public void Eula_ShouldReturnLicenseAndConfigString_WhenConfigNotEmpty()
    {
        Mock<DatabaseContext> dbMock = getDatabaseMock();
        MessageController messageController = new(dbMock.Object)
        {
            ControllerContext = MockHelper.GetMockControllerContext(),
        };
        MockHelper.SetupTestGameToken(messageController, getUnitTestToken());

        ServerConfiguration.Instance.EulaText = "unit test eula text";

        const int expectedStatus = 200;
        const string expected = @"
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation, either version 3 of the
License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
unit test eula text";

        IActionResult result = messageController.Eula();
        OkObjectResult? okObjectResult = result as OkObjectResult;
        Assert.NotNull(okObjectResult);
        Assert.Equal(expectedStatus, okObjectResult.StatusCode);
        Assert.NotNull(okObjectResult.Value);
        Assert.Equal(expected, (string)okObjectResult.Value);
    }

    [Fact]
    public async void Announcement_WithVariables_ShouldBeResolved()
    {
        Mock<DatabaseContext> dbMock = getDatabaseMock();
        MessageController messageController = new(dbMock.Object)
        {
            ControllerContext = MockHelper.GetMockControllerContext(),
        };
        MockHelper.SetupTestGameToken(messageController, getUnitTestToken());

        ServerConfiguration.Instance.AnnounceText = "you are now logged in as %user (id: %id)";

        const int expectedStatus = 200;
        const string expected = "you are now logged in as unittest (id: 1)\n";

        IActionResult result = await messageController.Announce();
        OkObjectResult? okObjectResult = result as OkObjectResult;
        Assert.NotNull(okObjectResult);
        Assert.Equal(expectedStatus, okObjectResult.StatusCode);
        Assert.NotNull(okObjectResult.Value);
        Assert.Equal(expected, (string)okObjectResult.Value);
    }

    [Fact]
    public async void Announcement_WithEmptyString_ShouldBeEmpty()
    {
        Mock<DatabaseContext> dbMock = getDatabaseMock();
        MessageController messageController = new(dbMock.Object)
        {
            ControllerContext = MockHelper.GetMockControllerContext(),
        };
        MockHelper.SetupTestGameToken(messageController, getUnitTestToken());

        ServerConfiguration.Instance.AnnounceText = "";

        const int expectedStatus = 200;
        const string expected = "";

        IActionResult result = await messageController.Announce();
        OkObjectResult? okObjectResult = result as OkObjectResult;
        Assert.NotNull(okObjectResult);
        Assert.Equal(expectedStatus, okObjectResult.StatusCode);
        Assert.NotNull(okObjectResult.Value);
        Assert.Equal(expected, (string)okObjectResult.Value);
    }

    [Fact]
    public void Notification_ShouldReturn_Empty()
    {
        Mock<DatabaseContext> dbMock = getDatabaseMock();
        MessageController messageController = new(dbMock.Object)
        {
            ControllerContext = MockHelper.GetMockControllerContext(),
        };
        MockHelper.SetupTestGameToken(messageController, getUnitTestToken());

        const int expected = 200;

        IActionResult result = messageController.Notification();
        OkResult? okObjectResult = result as OkResult;
        Assert.NotNull(okObjectResult);
        Assert.Equal(expected, okObjectResult.StatusCode);
    }

    [Fact]
    public async void Filter_ShouldNotCensor_WhenCensorDisabled()
    {
        Mock<DatabaseContext> dbMock = getDatabaseMock();
        const string request = "unit test message";
        byte[] requestBytes = Encoding.ASCII.GetBytes(request);
        MessageController messageController = new(dbMock.Object)
        {
            ControllerContext = MockHelper.GetMockControllerContext(),
            Request = { Body = new MemoryStream(requestBytes), ContentLength = requestBytes.Length,},
        };
        MockHelper.SetupTestGameToken(messageController, getUnitTestToken());

        CensorConfiguration.Instance.UserInputFilterMode = FilterMode.None;

        const int expectedStatus = 200;
        const string expectedBody = "unit test message";

        IActionResult result = await messageController.Filter();
        OkObjectResult? okObjectResult = result as OkObjectResult;
        Assert.NotNull(okObjectResult);
        Assert.Equal(expectedStatus, okObjectResult.StatusCode);
        Assert.NotNull(okObjectResult.Value);
        Assert.Equal(expectedBody, (string)okObjectResult.Value);
    }

    [Fact]
    public async void Filter_ShouldCensor_WhenEnabled()
    {
        Mock<DatabaseContext> dbMock = getDatabaseMock();
        const string request = "unit test message bruh";
        byte[] requestBytes = Encoding.ASCII.GetBytes(request);
        MessageController messageController = new(dbMock.Object)
        {
            ControllerContext = MockHelper.GetMockControllerContext(),
            Request =
            {
                Body = new MemoryStream(requestBytes),
                ContentLength = requestBytes.Length,
            },
        };
        MockHelper.SetupTestGameToken(messageController, getUnitTestToken());

        CensorConfiguration.Instance.UserInputFilterMode = FilterMode.Asterisks;
        CensorConfiguration.Instance.FilteredWordList = new List<string>()
        {
            "bruh",
        };

        const int expectedStatus = 200;
        const string expectedBody = "unit test message ****";

        IActionResult result = await messageController.Filter();
        OkObjectResult? okObjectResult = result as OkObjectResult;
        Assert.NotNull(okObjectResult);
        Assert.Equal(expectedStatus, okObjectResult.StatusCode);
        Assert.NotNull(okObjectResult.Value);
        Assert.Equal(expectedBody, (string)okObjectResult.Value);
    }

    [Fact]
    public async void Filter_ShouldNotSendEmail_WhenMailDisabled()
    {
        Mock<DatabaseContext> dbMock = getDatabaseMock();
        const string request = "/setemail unittest@unittest.com";
        byte[] requestBytes = Encoding.ASCII.GetBytes(request);
        MessageController messageController = new(dbMock.Object)
        {
            ControllerContext = MockHelper.GetMockControllerContext(),
            Request =
            {
                Body = new MemoryStream(requestBytes),
                ContentLength = requestBytes.Length,
            },
        };
        MockHelper.SetupTestGameToken(messageController, getUnitTestToken());

        //TODO: mock smtphelper

        ServerConfiguration.Instance.Mail.MailEnabled = false;

        const int expectedStatus = 200;

        IActionResult result = await messageController.Filter();
        OkResult? okObjectResult = result as OkResult;
        Assert.NotNull(okObjectResult);
        Assert.Equal(expectedStatus, okObjectResult.StatusCode);
    }
    
}