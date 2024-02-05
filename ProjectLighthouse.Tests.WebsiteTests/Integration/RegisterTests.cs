using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Localization.StringLists;
using LBPUnion.ProjectLighthouse.Tests.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using ProjectLighthouse.Tests.WebsiteTests.Extensions;
using Xunit;

namespace ProjectLighthouse.Tests.WebsiteTests.Integration;

[Trait("Category", "Integration")]
public class RegisterTests : LighthouseWebTest
{
    [Fact]
    public async Task ShouldRegister()
    {
        await using DatabaseContext database = await IntegrationHelper.GetIntegrationDatabase();
        this.Driver.Manage().Cookies.DeleteAllCookies();

        ServerConfiguration.Instance.Authentication.RegistrationEnabled = true;

        string username = ("unitTestUser" + CryptoHelper.GenerateRandomInt32(0, int.MaxValue))[..16];
        string password = CryptoHelper.Sha256Hash(CryptoHelper.GenerateRandomBytes(64).ToArray());

        this.Driver.Navigate().GoToUrl(this.BaseAddress + "/register");

        this.Driver.FindElement(By.Id("text")).SendKeys(username);

        this.Driver.FindElement(By.Id("password")).SendKeys(password);
        this.Driver.FindElement(By.Id("confirmPassword")).SendKeys(password);

        this.Driver.FindElement(By.Id("age-checkbox")).Click();

        this.Driver.FindElement(By.Id("submit")).Click();

        UserEntity? user = await database.Users.FirstOrDefaultAsync(u => u.Username == username);
        Assert.NotNull(user);

        Assert.Equal("/", this.Driver.GetPath());

        ServerConfiguration.Instance.Authentication.RegistrationEnabled = false;
    }

    [Fact]
    public async Task ShouldNotRegisterWithMismatchingPasswords()
    {
        await using DatabaseContext database = await IntegrationHelper.GetIntegrationDatabase();
        this.Driver.Manage().Cookies.DeleteAllCookies();

        ServerConfiguration.Instance.Authentication.RegistrationEnabled = true;

        string username = ("unitTestUser" + CryptoHelper.GenerateRandomInt32(0, int.MaxValue))[..16];
        string password = CryptoHelper.Sha256Hash(CryptoHelper.GenerateRandomBytes(64).ToArray());

        this.Driver.Navigate().GoToUrl(this.BaseAddress + "/register");

        this.Driver.FindElement(By.Id("text")).SendKeys(username);

        this.Driver.FindElement(By.Id("password")).SendKeys(password);
        this.Driver.FindElement(By.Id("confirmPassword")).SendKeys(password + "a");

        this.Driver.FindElement(By.Id("age-checkbox")).Click();

        this.Driver.FindElement(By.Id("submit")).Click();

        UserEntity? user = await database.Users.FirstOrDefaultAsync(u => u.Username == username);
        Assert.Null(user);

        Assert.Equal("/register", this.Driver.GetPath());
        Assert.Equal(Translate(ErrorStrings.PasswordDoesntMatch), this.Driver.GetErrorMessage());

        ServerConfiguration.Instance.Authentication.RegistrationEnabled = false;
    }

    [Fact]
    public async Task ShouldNotRegisterWithTakenUsername()
    {
        await using DatabaseContext database = await IntegrationHelper.GetIntegrationDatabase();
        this.Driver.Manage().Cookies.DeleteAllCookies();

        ServerConfiguration.Instance.Authentication.RegistrationEnabled = true;

        string username = ("unitTestUser" + CryptoHelper.GenerateRandomInt32(0, int.MaxValue))[..16];
        string password = CryptoHelper.Sha256Hash(CryptoHelper.GenerateRandomBytes(64).ToArray());

        await database.CreateUser(username, CryptoHelper.BCryptHash(password));
        UserEntity? user = await database.Users.FirstOrDefaultAsync(u => u.Username == username);
        Assert.NotNull(user);

        this.Driver.Navigate().GoToUrl(this.BaseAddress + "/register");

        this.Driver.FindElement(By.Id("text")).SendKeys(username);

        this.Driver.FindElement(By.Id("password")).SendKeys(password);
        this.Driver.FindElement(By.Id("confirmPassword")).SendKeys(password);

        this.Driver.FindElement(By.Id("age-checkbox")).Click();

        this.Driver.FindElement(By.Id("submit")).Click();

        Assert.Equal("/register", this.Driver.GetPath());

        Assert.Equal(Translate(ErrorStrings.UsernameTaken), this.Driver.GetErrorMessage());

        ServerConfiguration.Instance.Authentication.RegistrationEnabled = false;
    }
}