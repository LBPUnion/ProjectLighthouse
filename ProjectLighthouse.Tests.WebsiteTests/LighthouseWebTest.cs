using System;
using System.Linq;
using LBPUnion.ProjectLighthouse.Startup;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Xunit;

namespace ProjectLighthouse.Tests.WebsiteTests;

[Collection(nameof(LighthouseWebTest))]
public class LighthouseWebTest : IDisposable
{
    public readonly string BaseAddress;

    public readonly IWebDriver Driver;
    public readonly IWebHost WebHost = new WebHostBuilder().UseKestrel().UseStartup<TestStartup>().UseWebRoot("StaticFiles").Build();

    public LighthouseWebTest()
    {
        this.WebHost.Start();

        IServerAddressesFeature? serverAddressesFeature = this.WebHost.ServerFeatures.Get<IServerAddressesFeature>();
        if (serverAddressesFeature == null) throw new ArgumentNullException();

        this.BaseAddress = serverAddressesFeature.Addresses.First();

        ChromeOptions chromeOptions = new();
        if (Convert.ToBoolean(Environment.GetEnvironmentVariable("CI") ?? "false"))
        {
            chromeOptions.AddArgument("headless");
            chromeOptions.AddArgument("no-sandbox");
            chromeOptions.AddArgument("disable-dev-shm-usage");
            Console.WriteLine("We are in a CI environment, so chrome headless mode has been enabled.");
        }

        this.Driver = new ChromeDriver(chromeOptions);
    }

    public void Dispose()
    {
        this.Driver.Close();
        this.Driver.Dispose();
        this.WebHost.Dispose();

        GC.SuppressFinalize(this);
    }
}