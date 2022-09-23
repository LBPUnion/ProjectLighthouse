using System;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Tests;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using Xunit;

namespace ProjectLighthouse.Tests.WebsiteTests.Tests;

public class RegisterTests : LighthouseWebTest
{
    [DatabaseFact]
    public async Task ShouldRegister()
    {
        await using Database database = new();

        string username = ("unitTestUser" + new Random().Next()).Substring(0, 16);
        string password = CryptoHelper.Sha256Hash(CryptoHelper.GenerateRandomBytes(64).ToArray());

        this.Driver.Navigate().GoToUrl(this.BaseAddress + "/register");

        this.Driver.FindElement(By.Id("text")).SendKeys(username);

        this.Driver.FindElement(By.Id("password")).SendKeys(password);
        this.Driver.FindElement(By.Id("confirmPassword")).SendKeys(password);

        this.Driver.FindElement(By.Id("age-checkbox")).Click();

        this.Driver.FindElement(By.Id("submit")).Click();

        User? user = await database.Users.FirstOrDefaultAsync(u => u.Username == username);
        Assert.NotNull(user);

        await database.RemoveUser(user);
    }

    [DatabaseFact]
    public async Task ShouldNotRegisterWithMismatchingPasswords()
    {
        await using Database database = new();

        string username = ("unitTestUser" + new Random().Next()).Substring(0, 16);
        string password = CryptoHelper.Sha256Hash(CryptoHelper.GenerateRandomBytes(64).ToArray());

        this.Driver.Navigate().GoToUrl(this.BaseAddress + "/register");

        this.Driver.FindElement(By.Id("text")).SendKeys(username);

        this.Driver.FindElement(By.Id("password")).SendKeys(password);
        this.Driver.FindElement(By.Id("confirmPassword")).SendKeys(password + "a");

        this.Driver.FindElement(By.Id("age-checkbox")).Click();

        this.Driver.FindElement(By.Id("submit")).Click();

        User? user = await database.Users.FirstOrDefaultAsync(u => u.Username == username);
        Assert.Null(user);
    }

    [DatabaseFact]
    public async Task ShouldNotRegisterWithTakenUsername()
    {
        await using Database database = new();

        string username = ("unitTestUser" + new Random().Next()).Substring(0, 16);
        string password = CryptoHelper.Sha256Hash(CryptoHelper.GenerateRandomBytes(64).ToArray());

        await database.CreateUser(username, CryptoHelper.BCryptHash(password));
        User? user = await database.Users.FirstOrDefaultAsync(u => u.Username == username);
        Assert.NotNull(user);

        this.Driver.Navigate().GoToUrl(this.BaseAddress + "/register");

        this.Driver.FindElement(By.Id("text")).SendKeys(username);

        this.Driver.FindElement(By.Id("password")).SendKeys(password);
        this.Driver.FindElement(By.Id("confirmPassword")).SendKeys(password);

        this.Driver.FindElement(By.Id("age-checkbox")).Click();

        this.Driver.FindElement(By.Id("submit")).Click();

        Assert.Contains("The username you've chosen is already taken.", this.Driver.PageSource);
    }
}