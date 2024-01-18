using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Mail;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;
using LBPUnion.ProjectLighthouse.Tests.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Mail;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Unit.Controllers;

[Trait("Category", "Unit")]
public class MessageControllerTests
{
    [Fact]
    public void Eula_ShouldReturnLicense_WhenConfigEmpty()
    {
        MessageController messageController = new(null!);
        messageController.SetupTestController();

        ServerConfiguration.Instance.EulaText = "";

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

        string eulaMsg = result.CastTo<OkObjectResult, string>();
        Assert.Equal(expected, eulaMsg);
    }

    [Fact]
    public void Eula_ShouldReturnLicenseAndConfigString_WhenConfigNotEmpty()
    {
        MessageController messageController = new(null!);
        messageController.SetupTestController();

        ServerConfiguration.Instance.EulaText = "unit test eula text";

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

        string eulaMsg = result.CastTo<OkObjectResult, string>();
        Assert.Equal(expected, eulaMsg);
    }

    [Fact]
    public async Task Announcement_WithVariables_ShouldBeResolved()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();
        MessageController messageController = new(dbMock);
        messageController.SetupTestController();

        ServerConfiguration.Instance.AnnounceText = "you are now logged in as %user (id: %id)";

        const string expected = "you are now logged in as unittest (id: 1)";

        IActionResult result = await messageController.Announce();

        string announceMsg = result.CastTo<OkObjectResult, string>();
        Assert.Equal(expected, announceMsg);
    }

    [Fact]
    public async Task Announcement_WithEmptyString_ShouldBeEmpty()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();
        MessageController messageController = new(dbMock);
        messageController.SetupTestController();

        ServerConfiguration.Instance.AnnounceText = "";

        const string expected = "";

        IActionResult result = await messageController.Announce();

        string announceMsg = result.CastTo<OkObjectResult, string>();
        Assert.Equal(expected, announceMsg);
    }

    [Fact]
    public async Task Filter_ShouldNotCensor_WhenCensorDisabled()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        const string request = "unit test message";
        MessageController messageController = new(dbMock);
        messageController.SetupTestController(request);

        CensorConfiguration.Instance.UserInputFilterMode = FilterMode.None;

        const string expectedBody = "unit test message";

        IActionResult result = await messageController.Filter(new NullMailService());

        string filteredMessage = result.CastTo<OkObjectResult, string>();
        Assert.Equal(expectedBody, filteredMessage);
    }

    [Fact]
    public async Task Filter_ShouldCensor_WhenCensorEnabled()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        const string request = "unit test message bruh";
        MessageController messageController = new(dbMock);
        messageController.SetupTestController(request);

        CensorConfiguration.Instance.UserInputFilterMode = FilterMode.Asterisks;
        CensorConfiguration.Instance.FilteredWordList = new List<string>
        {
            "bruh",
        };

        const string expectedBody = "unit test message ****";

        IActionResult result = await messageController.Filter(new NullMailService());

        string filteredMessage = result.CastTo<OkObjectResult, string>();
        Assert.Equal(expectedBody, filteredMessage);
    }

    private static Mock<IMailService> getMailServiceMock()
    {
        Mock<IMailService> mailMock = new();
        mailMock.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.FromResult(true));
        return mailMock;
    }

    [Fact]
    public async Task Filter_ShouldNotSendEmail_WhenMailDisabled()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();
        Mock<IMailService> mailMock = getMailServiceMock();
        const string request = "/setemail unittest@unittest.com";
        MessageController messageController = new(dbMock);
        messageController.SetupTestController(request);

        ServerConfiguration.Instance.Mail.MailEnabled = false;
        CensorConfiguration.Instance.FilteredWordList = new List<string>();

        const string expected = "/setemail unittest@unittest.com";

        IActionResult result = await messageController.Filter(mailMock.Object);

        string filteredMessage = result.CastTo<OkObjectResult, string>();
        Assert.Equal(expected, filteredMessage);
    }

    [Fact]
    public async Task Filter_ShouldSendEmail_WhenMailEnabled_AndEmailNotTaken()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        Mock<IMailService> mailMock = getMailServiceMock();

        const string request = "/setemail unittest@unittest.com";

        MessageController messageController = new(dbMock);
        messageController.SetupTestController(request);

        ServerConfiguration.Instance.Mail.MailEnabled = true;

        const string expectedEmail = "unittest@unittest.com";

        IActionResult result = await messageController.Filter(mailMock.Object);

        Assert.IsType<OkResult>(result);
        Assert.Equal(expectedEmail, dbMock.Users.First().EmailAddress);
        mailMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Filter_ShouldNotSendEmail_WhenMailEnabled_AndEmailTaken()
    {
        List<UserEntity> users = new()
        {
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

        MessageController messageController = new(dbMock);
        messageController.SetupTestController(request);

        ServerConfiguration.Instance.Mail.MailEnabled = true;

        IActionResult result = await messageController.Filter(mailMock.Object);

        Assert.IsType<OkResult>(result);
        mailMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Filter_ShouldNotSendEmail_WhenMailEnabled_AndEmailAlreadyVerified()
    {
        UserEntity unitTestUser = MockHelper.GetUnitTestUser();
        unitTestUser.EmailAddressVerified = true;
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase(new List<UserEntity>
        {
            unitTestUser,
        });

        Mock<IMailService> mailMock = getMailServiceMock();

        const string request = "/setemail unittest@unittest.com";

        MessageController messageController = new(dbMock);
        messageController.SetupTestController(request);

        ServerConfiguration.Instance.Mail.MailEnabled = true;

        IActionResult result = await messageController.Filter(mailMock.Object);

        Assert.IsType<OkResult>(result);
        mailMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Filter_ShouldNotSendEmail_WhenMailEnabled_AndEmailFormatInvalid()
    {
        UserEntity unitTestUser = MockHelper.GetUnitTestUser();
        unitTestUser.EmailAddressVerified = true;
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        Mock<IMailService> mailMock = getMailServiceMock();

        const string request = "/setemail unittestinvalidemail@@@";

        MessageController messageController = new(dbMock);
        messageController.SetupTestController(request);

        ServerConfiguration.Instance.Mail.MailEnabled = true;

        IActionResult result = await messageController.Filter(mailMock.Object);

        Assert.IsType<OkResult>(result);
        mailMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}