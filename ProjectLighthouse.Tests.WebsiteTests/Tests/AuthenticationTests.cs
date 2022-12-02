using System;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Tests;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using Xunit;

namespace ProjectLighthouse.Tests.WebsiteTests.Tests;

public class AuthenticationTests : LighthouseWebTest
{
    [DatabaseFact]
    public async Task ShouldLoginWithPassword()
    {
        await using Database database = new();
        Random random = new();

        string password = CryptoHelper.Sha256Hash(CryptoHelper.GenerateRandomBytes(64).ToArray());
        User user = await database.CreateUser($"unitTestUser{random.Next()}", CryptoHelper.BCryptHash(CryptoHelper.Sha256Hash(password)));

        this.Driver.Navigate().GoToUrl(this.BaseAddress + "/login");

        this.Driver.FindElement(By.Id("text")).SendKeys(user.Username);
        this.Driver.FindElement(By.Id("password")).SendKeys(password);

        this.Driver.FindElement(By.Id("submit")).Click();

        WebToken? webToken = await database.WebTokens.FirstOrDefaultAsync(t => t.UserId == user.UserId);
        Assert.NotNull(webToken);

        await database.RemoveUser(user);
    }

    [DatabaseFact]
    public async Task ShouldNotLoginWithNoPassword()
    {
        await using Database database = new();
        Random random = new();
        User user = await database.CreateUser($"unitTestUser{random.Next()}", CryptoHelper.BCryptHash("just like the hindenberg,"));

        this.Driver.Navigate().GoToUrl(this.BaseAddress + "/login");

        this.Driver.FindElement(By.Id("text")).SendKeys(user.Username);

        this.Driver.FindElement(By.Id("submit")).Click();

        WebToken? webToken = await database.WebTokens.FirstOrDefaultAsync(t => t.UserId == user.UserId);
        Assert.Null(webToken);

        await database.RemoveUser(user);
    }

    [DatabaseFact]
    public async Task ShouldNotLoginWithWrongPassword()
    {
        await using Database database = new();
        Random random = new();
        User user = await database.CreateUser($"unitTestUser{random.Next()}", CryptoHelper.BCryptHash("i'm an engineering failure"));

        this.Driver.Navigate().GoToUrl(this.BaseAddress + "/login");

        this.Driver.FindElement(By.Id("text")).SendKeys(user.Username);
        this.Driver.FindElement(By.Id("password")).SendKeys("nah man");

        this.Driver.FindElement(By.Id("submit")).Click();

        WebToken? webToken = await database.WebTokens.FirstOrDefaultAsync(t => t.UserId == user.UserId);
        Assert.Null(webToken);

        await database.RemoveUser(user);
    }

    [DatabaseFact]
    public async Task ShouldLoginWithInjectedCookie()
    {
        const string loggedInAsUsernameTextXPath = "/html/body/div/div/div/div/p[1]";

        await using Database database = new();
        Random random = new();
        User user = await database.CreateUser($"unitTestUser{random.Next()}", CryptoHelper.BCryptHash("i'm an engineering failure"));

        WebToken webToken = new()
        {
            UserId = user.UserId,
            UserToken = CryptoHelper.GenerateAuthToken(),
            ExpiresAt = DateTime.Now + TimeSpan.FromHours(1),
            Verified = true,
        };

        database.WebTokens.Add(webToken);
        await database.SaveChangesAsync();

        INavigation navigation = this.Driver.Navigate();

        navigation.GoToUrl(this.BaseAddress + "/");
        Assert.DoesNotContain(user.Username, this.Driver.FindElement(By.XPath(loggedInAsUsernameTextXPath)).Text);
        this.Driver.Manage().Cookies.AddCookie(new Cookie("LighthouseToken", webToken.UserToken));
        navigation.Refresh();
        Assert.Contains(user.Username, this.Driver.FindElement(By.XPath(loggedInAsUsernameTextXPath)).Text);

        await database.RemoveUser(user);
    }
}