using System;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Tests;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using Xunit;

namespace ProjectLighthouse.Tests.WebsiteTests;

public class RegisterTests : LighthouseWebTest
{
    [DatabaseFact]
    public async Task ShouldRegister()
    {
        await using Database database = new();

        string username = ("unitTestUser" + new Random().Next()).Substring(0, 16);
        string passwordPlaintext = Convert.ToHexString(HashHelper.GenerateRandomBytes(16).ToArray());

        this.Driver.Navigate().GoToUrl(this.BaseAddress + "/register");

        this.Driver.FindElement(By.Id("text")).SendKeys(username);

        this.Driver.FindElement(By.Id("password")).SendKeys(passwordPlaintext);
        this.Driver.FindElement(By.Id("confirmPassword")).SendKeys(passwordPlaintext);

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
        string passwordPlaintext = Convert.ToHexString(HashHelper.GenerateRandomBytes(16).ToArray());

        this.Driver.Navigate().GoToUrl(this.BaseAddress + "/register");

        this.Driver.FindElement(By.Id("text")).SendKeys(username);

        this.Driver.FindElement(By.Id("password")).SendKeys(passwordPlaintext);
        this.Driver.FindElement(By.Id("confirmPassword")).SendKeys(passwordPlaintext + "a");

        this.Driver.FindElement(By.Id("submit")).Click();

        User? user = await database.Users.FirstOrDefaultAsync(u => u.Username == username);
        Assert.Null(user);
    }

    [DatabaseFact]
    public async Task ShouldNotRegisterWithTakenUsername()
    {
        await using Database database = new();

        string username = ("unitTestUser" + new Random().Next()).Substring(0, 16);
        string passwordPlaintext = Convert.ToHexString(HashHelper.GenerateRandomBytes(16).ToArray());
        string passwordSha256 = HashHelper.Sha256Hash(passwordPlaintext);

        await database.CreateUser(username, HashHelper.BCryptHash(passwordSha256));
        User? user = await database.Users.FirstOrDefaultAsync(u => u.Username == username);
        Assert.NotNull(user);

        this.Driver.Navigate().GoToUrl(this.BaseAddress + "/register");

        this.Driver.FindElement(By.Id("text")).SendKeys(username);

        this.Driver.FindElement(By.Id("password")).SendKeys(passwordPlaintext);
        this.Driver.FindElement(By.Id("confirmPassword")).SendKeys(passwordPlaintext);

        this.Driver.FindElement(By.Id("submit")).Click();

        Assert.Contains("The username you've chosen is already taken.", this.Driver.PageSource);
        await database.RemoveUser(user);
    }
}