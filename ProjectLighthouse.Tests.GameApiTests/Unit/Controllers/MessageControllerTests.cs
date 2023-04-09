using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Mail;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Unit.Controllers;

[Trait("Category", "Unit")]
public class MessageControllerTests
{

    [Fact]
    public void Eula_ShouldReturnLicense_WhenConfigEmpty()
    {
        MessageController messageController = new(null!, null!);
        messageController.SetupTestController();

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
along with this program.  If not, see <https://www.gnu.org/licenses/>." + "\n";

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
        MessageController messageController = new(null!, null!);
        messageController.SetupTestController();

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
along with this program.  If not, see <https://www.gnu.org/licenses/>." + "\nunit test eula text";

        IActionResult result = messageController.Eula();
        OkObjectResult? okObjectResult = result as OkObjectResult;
        Assert.NotNull(okObjectResult);
        Assert.Equal(expectedStatus, okObjectResult.StatusCode);
        Assert.NotNull(okObjectResult.Value);
        Assert.Equal(expected, (string)okObjectResult.Value);
    }

    [Fact]
    public async Task Announcement_WithVariables_ShouldBeResolved()
    {
        Mock<DatabaseContext> dbMock = MockHelper.GetDatabaseMock();
        MessageController messageController = new(dbMock.Object, null!);
        messageController.SetupTestController();

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
    public async Task Announcement_WithEmptyString_ShouldBeEmpty()
    {
        await using DatabaseContext db = await MockHelper.GetTestDatabase();
        MessageController messageController = new(db, null!);
        messageController.SetupTestController();

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
    public async Task Notification_ShouldReturn_Empty()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();
        MessageController messageController = new(dbMock, null!);
        messageController.SetupTestController();

        const int expected = 200;

        IActionResult result = messageController.Notification();
        OkResult? okObjectResult = result as OkResult;
        Assert.NotNull(okObjectResult);
        Assert.Equal(expected, okObjectResult.StatusCode);
    }

    [Fact]
    public async Task Filter_ShouldNotCensor_WhenCensorDisabled()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        const string request = "unit test message";
        MessageController messageController = new(dbMock, null!);
        messageController.SetupTestController(request);

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
    public async Task Filter_ShouldCensor_WhenCensorEnabled()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        const string request = "unit test message bruh";
        MessageController messageController = new(dbMock, null!);
        messageController.SetupTestController(request);

        CensorConfiguration.Instance.UserInputFilterMode = FilterMode.Asterisks;
        CensorConfiguration.Instance.FilteredWordList = new List<string>
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

    private static Mock<IMailService> getMailServiceMock()
    {
        Mock<IMailService> mailMock = new();
        mailMock.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.FromResult(true));
        return mailMock;
    }

    [Fact]
    public async void Filter_ShouldNotSendEmail_WhenMailDisabled()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();
        Mock<IMailService> mailMock = getMailServiceMock();
        const string request = "/setemail unittest@unittest.com";
        MessageController messageController = new(dbMock, mailMock.Object);
        messageController.SetupTestController(request);

        ServerConfiguration.Instance.Mail.MailEnabled = false;
        CensorConfiguration.Instance.FilteredWordList = new List<string>();

        const int expectedStatus = 200;
        const string expected = "/setemail unittest@unittest.com";

        IActionResult result = await messageController.Filter();
        OkObjectResult? okObjectResult = result as OkObjectResult;
        Assert.NotNull(okObjectResult);
        Assert.Equal(expectedStatus, okObjectResult.StatusCode);
        Assert.Equal(expected, okObjectResult.Value);
    }

    [Fact]
    public async void Filter_ShouldSendEmail_WhenMailEnabled_AndEmailNotTaken()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();
        
        Mock<IMailService> mailMock = getMailServiceMock();

        const string request = "/setemail unittest@unittest.com";

        MessageController messageController = new(dbMock, mailMock.Object);
        messageController.SetupTestController(request);

        ServerConfiguration.Instance.Mail.MailEnabled = true;

        const int expectedStatus = 200;
        const string expectedEmail = "unittest@unittest.com";

        IActionResult result = await messageController.Filter();
        OkResult? okResult = result as OkResult;
        Assert.NotNull(okResult);
        Assert.Equal(expectedStatus, okResult.StatusCode);
        Assert.Equal(expectedEmail, dbMock.Users.First().EmailAddress);
        mailMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async void Filter_ShouldNotSendEmail_WhenMailEnabled_AndEmailTaken()
    {
        List<UserEntity> users = new()
        {
            MockHelper.GetUnitTestUser(),
            new UserEntity
            {
                UserId = 2,
                EmailAddress = "unittest@unittest.com",
                EmailAddressVerified = false,
            },
        };
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase(users);
        Mock<IMailService> mailMock = getMailServiceMock();

        const string request = "/setemail unittest@unittest.com";

        MessageController messageController = new(dbMock, mailMock.Object);
        messageController.SetupTestController(request);

        ServerConfiguration.Instance.Mail.MailEnabled = true;

        const int expectedStatus = 200;

        IActionResult result = await messageController.Filter();
        OkResult? okResult = result as OkResult;
        Assert.NotNull(okResult);
        Assert.Equal(expectedStatus, okResult.StatusCode);
        mailMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async void Filter_ShouldNotSendEmail_WhenMailEnabled_AndEmailAlreadyVerified()
    {
        UserEntity unitTestUser = MockHelper.GetUnitTestUser();
        unitTestUser.EmailAddressVerified = true;
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase(new List<UserEntity>
        {
            unitTestUser,
        });

        Mock<IMailService> mailMock = getMailServiceMock();

        const string request = "/setemail unittest@unittest.com";

        MessageController messageController = new(dbMock, mailMock.Object);
        messageController.SetupTestController(request);

        ServerConfiguration.Instance.Mail.MailEnabled = true;

        const int expectedStatus = 200;

        IActionResult result = await messageController.Filter();
        OkResult? okResult = result as OkResult;
        Assert.NotNull(okResult);
        Assert.Equal(expectedStatus, okResult.StatusCode);
        mailMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async void Filter_ShouldNotSendEmail_WhenMailEnabled_AndEmailFormatInvalid()
    {
        UserEntity unitTestUser = MockHelper.GetUnitTestUser();
        unitTestUser.EmailAddressVerified = true;
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase(new List<UserEntity>
        {
            unitTestUser,
        });

        Mock<IMailService> mailMock = getMailServiceMock();

        const string request = "/setemail unittestinvalidemail@@@";

        MessageController messageController = new(dbMock, mailMock.Object);
        messageController.SetupTestController(request);

        ServerConfiguration.Instance.Mail.MailEnabled = true;

        const int expectedStatus = 200;

        IActionResult result = await messageController.Filter();
        OkResult? okResult = result as OkResult;
        Assert.NotNull(okResult);
        Assert.Equal(expectedStatus, okResult.StatusCode);
        mailMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
    
}