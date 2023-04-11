using System;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Tests.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using Xunit;

namespace ProjectLighthouse.Tests.WebsiteTests.Integration;

[Trait("Category", "Integration")]
public class AuthenticationTests : LighthouseWebTest
{
    [Fact]
    public async Task ShouldLoginWithPassword()
    {
        await using DatabaseContext database = await IntegrationHelper.GetIntegrationDatabase();
        Random random = new();

        string password = CryptoHelper.Sha256Hash(CryptoHelper.GenerateRandomBytes(64).ToArray());
        UserEntity user = await database.CreateUser($"unitTestUser{random.Next()}", CryptoHelper.BCryptHash(CryptoHelper.Sha256Hash(password)));

        this.Driver.Navigate().GoToUrl(this.BaseAddress + "/login");

        this.Driver.FindElement(By.Id("text")).SendKeys(user.Username);
        this.Driver.FindElement(By.Id("password")).SendKeys(password);

        this.Driver.FindElement(By.Id("submit")).Click();

        WebTokenEntity? webToken = await database.WebTokens.FirstOrDefaultAsync(t => t.UserId == user.UserId);
        Assert.NotNull(webToken);

        await database.RemoveUser(user);
    }

    [Fact]
    public async Task ShouldNotLoginWithNoPassword()
    {
        await using DatabaseContext database = await IntegrationHelper.GetIntegrationDatabase();
        Random random = new();
        UserEntity user = await database.CreateUser($"unitTestUser{random.Next()}", CryptoHelper.BCryptHash("just like the hindenberg,"));

        this.Driver.Navigate().GoToUrl(this.BaseAddress + "/login");

        this.Driver.FindElement(By.Id("text")).SendKeys(user.Username);

        this.Driver.FindElement(By.Id("submit")).Click();

        WebTokenEntity? webToken = await database.WebTokens.FirstOrDefaultAsync(t => t.UserId == user.UserId);
        Assert.Null(webToken);

        await database.RemoveUser(user);
    }

    [Fact]
    public async Task ShouldNotLoginWithWrongPassword()
    {
        await using DatabaseContext database = await IntegrationHelper.GetIntegrationDatabase();
        Random random = new();
        UserEntity user = await database.CreateUser($"unitTestUser{random.Next()}", CryptoHelper.BCryptHash("i'm an engineering failure"));

        this.Driver.Navigate().GoToUrl(this.BaseAddress + "/login");

        this.Driver.FindElement(By.Id("text")).SendKeys(user.Username);
        this.Driver.FindElement(By.Id("password")).SendKeys("nah man");

        this.Driver.FindElement(By.Id("submit")).Click();

        WebTokenEntity? webToken = await database.WebTokens.FirstOrDefaultAsync(t => t.UserId == user.UserId);
        Assert.Null(webToken);

        await database.RemoveUser(user);
    }

    [Fact]
    public async Task ShouldLoginWithInjectedCookie()
    {
        const string loggedInAsUsernameTextXPath = "/html/body/div/div/div/div/p[1]";

        await using DatabaseContext database = await IntegrationHelper.GetIntegrationDatabase();
        Random random = new();
        UserEntity user = await database.CreateUser($"unitTestUser{random.Next()}", CryptoHelper.BCryptHash("i'm an engineering failure"));

        WebTokenEntity webToken = new()
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