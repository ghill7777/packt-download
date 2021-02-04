using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

// ReSharper disable IdentifierTypo
namespace PacktDownload
{
    internal class Program
    {
        private const bool Headless = false;

        private static void Main()
        {
            var dependencies = GetDependencies();
            var helper = (WebDriverHelper) dependencies["WebDriverHelper"];
            var doMenu = true;
            while (doMenu)
            {
                var choice = helper.PromptUser(@"
1. Packt Downloader
2. Udemy Downloader
: ");
                switch (choice)
                {
                    case "1":
                        var packDownloader = new PacktDownloader((ChromeDriver)dependencies["ChromeDriver"], 
                            (WebDriverHelper)dependencies["WebDriverHelper"]);
                        packDownloader.Process();
                        break;
                    case "2":
                        var udemyDownloader = new UdemyDownloader((ChromeDriver)dependencies["ChromeDriver"],
                            (WebDriverHelper)dependencies["WebDriverHelper"]);
                        udemyDownloader.Process();
                        Console.ReadLine();
                        break;
                    default:
                        doMenu = false;
                        break;
                }
            }

            Dispose(dependencies);
        }

        private static void Dispose(Dictionary<string, object> dependencies)
        {
            dependencies.Values
                .OfType<IDisposable>()
                .ToList().ForEach(obj => obj.Dispose());
        }

        private static Dictionary<string, object> GetDependencies()
        {
            var dic = new Dictionary<string, object>();
            
            var chromeOptions = new ChromeOptions();
            // ReSharper disable once HeuristicUnreachableCode
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
#pragma warning disable 162
            if (Headless)
            {
                chromeOptions.AddArgument("--headless");
            }
            else
            {
                chromeOptions.AddArgument("start-maximized");
            }

            chromeOptions.SetLoggingPreference(LogType.Browser, LogLevel.Off);

#pragma warning restore 162
            Console.WriteLine("Instantiating Chrome driver...");
            var webDriver = new ChromeDriver(chromeOptions);
            dic.Add("ChromeDriver", webDriver);
            webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            var webDriverHelper = new WebDriverHelper(webDriver);
            dic.Add("WebDriverHelper", webDriverHelper);

            return dic;
        }
    }
}
