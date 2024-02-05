using System;
using System.Linq;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Localization;
using LBPUnion.ProjectLighthouse.Servers.Website.Startup;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Xunit;

namespace ProjectLighthouse.Tests.WebsiteTests.Integration;

[Collection(nameof(LighthouseWebTest))]
public class LighthouseWebTest : IDisposable
{
    protected readonly string BaseAddress;

    protected readonly IWebDriver Driver;
    private readonly IWebHost webHost = new WebHostBuilder().UseKestrel().UseStartup<WebsiteTestStartup>().UseWebRoot("StaticFiles").Build();

    protected LighthouseWebTest()
    {
        ServerConfiguration.Instance.DbConnectionString = "server=127.0.0.1;uid=root;pwd=lighthouse_tests;database=lighthouse_tests";
        ServerConfiguration.Instance.TwoFactorConfiguration.TwoFactorEnabled = false;

        this.webHost.Start();

        IServerAddressesFeature? serverAddressesFeature = this.webHost.ServerFeatures.Get<IServerAddressesFeature>();
        if (serverAddressesFeature == null) throw new ArgumentNullException();

        this.BaseAddress = serverAddressesFeature.Addresses.First();

        ChromeOptions chromeOptions = new();
        if (Convert.ToBoolean(Environment.GetEnvironmentVariable("CI") ?? "false"))
        {
            chromeOptions.AddArgument("headless");
            chromeOptions.AddArgument("no-sandbox");
            chromeOptions.AddArgument("disable-dev-shm-usage");
            Console.WriteLine(@"We are in a CI environment, so chrome headless mode has been enabled.");
        }

        this.Driver = new ChromeDriver(chromeOptions);
        this.Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
    }

    protected static string Translate(TranslatableString translatableString) => translatableString.Translate(LocalizationManager.DefaultLang);

    protected static string Translate(TranslatableString translatableString, params object?[] objects) =>
        translatableString.Translate(LocalizationManager.DefaultLang, objects);

    public void Dispose()
    {
        this.Driver.Close();
        this.Driver.Dispose();
        this.webHost.Dispose();

        GC.SuppressFinalize(this);
    }
}