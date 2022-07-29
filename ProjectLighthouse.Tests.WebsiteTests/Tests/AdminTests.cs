using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Tests;
using OpenQA.Selenium;
using Xunit;

namespace ProjectLighthouse.Tests.WebsiteTests.Tests;

public class AdminTests : LighthouseWebTest
{
    public const string AdminPanelButtonXPath = "/html/body/div/header/div/div/div/a[2]";

    [DatabaseFact]
    public async Task ShouldShowAdminPanelButtonWhenAdmin()
    {
        await using Database database = new();
        Random random = new();
        User user = await database.CreateUser($"unitTestUser{random.Next()}", CryptoHelper.BCryptHash("i'm an engineering failure"));

        WebToken webToken = new()
        {
            UserId = user.UserId,
            UserToken = CryptoHelper.GenerateAuthToken(),
            ExpiresAt = DateTime.Now + TimeSpan.FromHours(1),
        };

        database.WebTokens.Add(webToken);
        user.IsAdmin = true;
        await database.SaveChangesAsync();

        this.Driver.Navigate().GoToUrl(this.BaseAddress + "/");
        this.Driver.Manage().Cookies.AddCookie(new Cookie("LighthouseToken", webToken.UserToken));
        this.Driver.Navigate().Refresh();

        Assert.Contains("Admin Panel", this.Driver.FindElement(By.XPath(AdminPanelButtonXPath)).Text);
    }

    [DatabaseFact]
    public async Task ShouldNotShowAdminPanelButtonWhenNotAdmin()
    {
        await using Database database = new();
        Random random = new();
        User user = await database.CreateUser($"unitTestUser{random.Next()}", CryptoHelper.BCryptHash("i'm an engineering failure"));

        WebToken webToken = new()
        {
            UserId = user.UserId,
            UserToken = CryptoHelper.GenerateAuthToken(),
            ExpiresAt = DateTime.Now + TimeSpan.FromHours(1),
        };

        database.WebTokens.Add(webToken);
        user.IsAdmin = false;
        await database.SaveChangesAsync();

        this.Driver.Navigate().GoToUrl(this.BaseAddress + "/");
        this.Driver.Manage().Cookies.AddCookie(new Cookie("LighthouseToken", webToken.UserToken));
        this.Driver.Navigate().Refresh();

        Assert.DoesNotContain("Admin Panel", this.Driver.FindElement(By.XPath(AdminPanelButtonXPath)).Text);
    }
}