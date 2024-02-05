using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Tests.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Users;
using OpenQA.Selenium;
using Xunit;

namespace ProjectLighthouse.Tests.WebsiteTests.Integration;

[Trait("Category", "Integration")]
public class AdminTests : LighthouseWebTest
{
    private const string adminPanelButtonXPath = "/html/body/div/header/div/div/div/a[2]/span";

    [Fact]
    public async Task ShouldShowAdminPanelButtonWhenAdmin()
    {
        await using DatabaseContext database = await IntegrationHelper.GetIntegrationDatabase();
        this.Driver.Manage().Cookies.DeleteAllCookies();

        UserEntity user = await database.CreateUser($"unitTestUser{CryptoHelper.GenerateRandomInt32()}", CryptoHelper.BCryptHash("i'm an engineering failure"));

        WebTokenEntity webToken = new()
        {
            UserId = user.UserId,
            UserToken = CryptoHelper.GenerateAuthToken(),
            ExpiresAt = DateTime.UtcNow + TimeSpan.FromHours(1),
            Verified = true,
        };

        database.WebTokens.Add(webToken);
        user.PermissionLevel = PermissionLevel.Administrator;
        await database.SaveChangesAsync();

        this.Driver.Navigate().GoToUrl(this.BaseAddress + "/");
        this.Driver.Manage().Cookies.AddCookie(new Cookie("LighthouseToken", webToken.UserToken));
        this.Driver.Navigate().Refresh();

        Assert.Equal("Admin", this.Driver.FindElement(By.XPath(adminPanelButtonXPath)).Text);
    }

    [Fact]
    public async Task ShouldNotShowAdminPanelButtonWhenNotAdmin()
    {
        await using DatabaseContext database = await IntegrationHelper.GetIntegrationDatabase();
        this.Driver.Manage().Cookies.DeleteAllCookies();

        UserEntity user = await database.CreateUser($"unitTestUser{CryptoHelper.GenerateRandomInt32()}", CryptoHelper.BCryptHash("i'm an engineering failure"));

        WebTokenEntity webToken = new()
        {
            UserId = user.UserId,
            UserToken = CryptoHelper.GenerateAuthToken(),
            ExpiresAt = DateTime.UtcNow + TimeSpan.FromHours(1),
            Verified = true,
        };

        database.WebTokens.Add(webToken);
        user.PermissionLevel = PermissionLevel.Default;
        await database.SaveChangesAsync();

        this.Driver.Navigate().GoToUrl(this.BaseAddress + "/");
        this.Driver.Manage().Cookies.AddCookie(new Cookie("LighthouseToken", webToken.UserToken));
        this.Driver.Navigate().Refresh();

        Assert.Empty(this.Driver.FindElements(By.XPath(adminPanelButtonXPath)));
    }
}