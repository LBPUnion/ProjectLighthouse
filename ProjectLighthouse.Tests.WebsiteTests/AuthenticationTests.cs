using System;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Tests;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Xunit;

namespace ProjectLighthouse.Tests.WebsiteTests
{
    public class AuthenticationTests : IDisposable
    {
        public readonly IWebHost WebHost = new WebHostBuilder().UseKestrel().UseStartup<TestStartup>().UseWebRoot("StaticFiles").Build();
        public readonly string BaseAddress;

        public readonly IWebDriver Driver = new ChromeDriver();

        public AuthenticationTests()
        {
            this.WebHost.Start();

            IServerAddressesFeature? serverAddressesFeature = WebHost.ServerFeatures.Get<IServerAddressesFeature>();
            if (serverAddressesFeature == null) throw new ArgumentNullException();

            this.BaseAddress = serverAddressesFeature.Addresses.First();
        }

        [DatabaseFact]
        public async Task ShouldLoginWithPassword()
        {
            await using Database database = new();
            Random random = new();

            string password = HashHelper.Sha256Hash(HashHelper.GenerateRandomBytes(64).ToArray());
            User user = await database.CreateUser($"unitTestUser{random.Next()}", HashHelper.BCryptHash(HashHelper.Sha256Hash(password)));

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
            User user = await database.CreateUser($"unitTestUser{random.Next()}", HashHelper.BCryptHash("just like the hindenberg,"));

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
            User user = await database.CreateUser($"unitTestUser{random.Next()}", HashHelper.BCryptHash("i'm an engineering failure"));

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
            const string loggedInAsUsernameTextXPath = "/html/body/div/div/div/p[1]/b";

            await using Database database = new();
            Random random = new();
            User user = await database.CreateUser($"unitTestUser{random.Next()}", HashHelper.BCryptHash("i'm an engineering failure"));

            WebToken webToken = new()
            {
                UserId = user.UserId,
                UserToken = HashHelper.GenerateAuthToken(),
            };

            database.WebTokens.Add(webToken);
            await database.SaveChangesAsync();

            INavigation navigation = this.Driver.Navigate();

            navigation.GoToUrl(this.BaseAddress + "/");
            this.Driver.Manage().Cookies.AddCookie(new Cookie("LighthouseToken", webToken.UserToken));
            Assert.Throws<NoSuchElementException>(() => this.Driver.FindElement(By.XPath(loggedInAsUsernameTextXPath)));
            navigation.Refresh();
            Assert.True(this.Driver.FindElement(By.XPath(loggedInAsUsernameTextXPath)).Text == user.Username);

            await database.RemoveUser(user);
        }

        public void Dispose()
        {
            this.Driver.Close();
            this.Driver.Dispose();
            this.WebHost.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}