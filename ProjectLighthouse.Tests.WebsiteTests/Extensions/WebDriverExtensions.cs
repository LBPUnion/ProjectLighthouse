using System;
using System.Linq;
using OpenQA.Selenium;

namespace ProjectLighthouse.Tests.WebsiteTests.Extensions;

public static class WebDriverExtensions
{
    private static Uri GetUri(this IWebDriver driver) => new(driver.Url);

    public static string GetPath(this IWebDriver driver) => driver.GetUri().AbsolutePath;

    public static string? GetErrorMessage(this IWebDriver driver) => driver.FindElements(By.CssSelector("#error-message > p")).FirstOrDefault()?.Text;
}