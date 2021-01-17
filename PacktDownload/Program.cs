using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace PacktDownload
{
    class Program
    {
        private static ChromeDriver cd;

        static void Main(string[] args)
        {
            IWebDriver o = null;
            Console.Write("Enter URL of Video: ");
            var url = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(url)) return;
            Console.Write("Enter Folder Name: ");
            var folder = Console.ReadLine() ?? string.Empty;
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            try
            {
                var chromeOptions = new ChromeOptions();
                chromeOptions.AddArgument("--headless");
                Console.WriteLine("Instantiating Chrome driver...");
                cd = new ChromeDriver(chromeOptions);
                o = cd;
                Console.WriteLine("Navigating to video url...");
                cd.Navigate().GoToUrl(url);
                cd.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                var loginBtn = FindByXPath("//a[@href='/login']");
                loginBtn.Click();
                var email = FindById("login-input-email");
                SendKeys(email, "ghill@pearlgateonline.com");
                var pwd = FindById("login-input-password");
                SendKeys(pwd, "roXyaub1");
                var submitLogin = FindByXPath("//div[@id='login-form']//button[@type='submit']");
                Console.WriteLine("Clicking submit button...");
                submitLogin.Click();
                var menuToggle = FindById("menu-toggle");
                Console.WriteLine("Clicking menu toggle...");
                menuToggle.Click();
                const string linksXpath = "//li[@class='sub-nav ng-scope']/a";
                var links = FindAllByXPath(linksXpath).Select(c => c.GetAttribute("href")).ToArray();
                var webClient = new WebClient();
                var lockObject = new object();
                webClient.DownloadProgressChanged += (obj, evt) =>
                {
                    lock (lockObject)
                    {
                        Console.SetCursorPosition(0, 1);
                        var pct = Math.Round((double)evt.BytesReceived / (double)evt.TotalBytesToReceive, 2) *
                                  100;
                        Console.Write($"{pct}% done...".PadRight(10));
                    }
                };
                var i = 0;
                foreach (var link in links)
                {
                    if (string.IsNullOrWhiteSpace(link)) continue;
                    i++;
                    try
                    {
                        cd.Navigate().GoToUrl(link);
                        Thread.Sleep(1500);
                        var title = FindByXPath("//h2[starts-with(@class,'title')]")
                            .GetAttribute("innerText");
                        while (string.IsNullOrWhiteSpace(title))
                        {
                            Thread.Sleep(2000);
                            title = FindByXPath("//h2[starts-with(@class,'title')]")
                                .GetAttribute("innerText");
                        }

                        var video = FindByXPath("//video[@id='video-content_html5_api']");
                        var videoUrl = video.GetAttribute("src");
                        var filename = $"{i.ToString().PadLeft(3, '0')} - {title}.mp4";
                        filename = RemoveIllegalFilenameChars(filename);
                        Console.Clear();
                        Console.WriteLine($"Downloading {filename}...");
                        filename = $"{folder}\\{filename}";
                        if (File.Exists(filename)) continue;
                        webClient.DownloadFileTaskAsync(videoUrl, filename).ConfigureAwait(true).GetAwaiter().GetResult();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        continue;
                    }
                }

                cd.Close();
                cd.Dispose();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                System.Diagnostics.Debugger.Break();
            }
            finally
            {
                o?.Dispose();
            }
        }

        private static void SendKeys(IWebElement element, string value)
        {
            Console.WriteLine($"Sending \"{value}\"");
            element.SendKeys(value);
        }

        private static IWebElement FindByXPath(string xpath)
        {
            Console.WriteLine($"Finding element: {xpath}");
            return cd.FindElementByXPath(xpath);
        }

        private static IEnumerable<IWebElement> FindAllByXPath(string xpath)
        {
            Console.WriteLine($"Finding elements: {xpath}");
            return cd.FindElementsByXPath(xpath);
        }

        private static IWebElement FindById(string id)
        {
            Console.WriteLine($"Finding element id: {id}");
            return cd.FindElementById(id);
        }

        private static string RemoveIllegalFilenameChars(string filename)
        {
            foreach (var c in @"`=[]\;',/~!@#$%^&*()+{}|:""<>?'")
            {
                filename = filename.Replace(c.ToString(), string.Empty);
            }

            return filename;
        }

        static void ShowLinks(IEnumerable<string> links)
        {
            foreach (var link in links)
            {
                Console.WriteLine(link);
            }
        }
    }
}
