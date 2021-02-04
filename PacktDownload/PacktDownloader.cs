using System;
using System.Linq;
using System.Net;
using System.Threading;
using Delimon.Win32.IO;
using OpenQA.Selenium.Chrome;

// ReSharper disable IdentifierTypo
namespace PacktDownload
{
    public class PacktDownloader
    {
        private readonly ChromeDriver _webDriver;
        private readonly WebDriverHelper _helper;

        public PacktDownloader(ChromeDriver webDriver, WebDriverHelper helper)
        {
            _webDriver = webDriver ?? throw new ArgumentNullException(nameof(webDriver));
            _helper = helper ?? throw new ArgumentNullException(nameof(helper));
        }
        public void Process()
        {
            try
            {
                var url = _helper.PromptUser("Url: ");
                if (string.IsNullOrWhiteSpace(url)) return;
                Console.Write("Enter Folder Name: ");
                var folder = Console.ReadLine() ?? string.Empty;
                folder = Path.Combine(Environment.CurrentDirectory, folder);
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                _helper.Navigate(url);
                _helper.FindAndClickByXPath("//a[@href='/login']");
                var email = _helper.FindById("login-input-email");
                _helper.SendKeys(email, "ghill@pearlgateonline.com");
                var pwd = _helper.FindById("login-input-password");
                _helper.SendKeys(pwd, "roXyaub1");
                _helper.FindAndClickByXPath("//div[@id='login-form']//button[@type='submit']");
                _helper.FindAndClickById("menu-toggle");
                const string linksXpath = "//li[@class='sub-nav ng-scope']/a";
                var links = _helper.FindAllByXPath(linksXpath).Select(c => c.GetAttribute("href")).ToArray();
                var webClient = new WebClient();
                var lockObject = new object();
                webClient.DownloadProgressChanged += (obj, evt) =>
                {
                    lock (lockObject)
                    {
                        Console.SetCursorPosition(0, 1);
                        var pct = Math.Round(evt.BytesReceived / (double)evt.TotalBytesToReceive, 2) *
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
                        _webDriver.Navigate().GoToUrl(link);
                        Thread.Sleep(1500);
                        var title = _helper.FindByXPath("//h2[starts-with(@class,'title')]")
                            .GetAttribute("innerText");
                        while (string.IsNullOrWhiteSpace(title))
                        {
                            Thread.Sleep(2000);
                            title = _helper.FindByXPath("//h2[starts-with(@class,'title')]")
                                .GetAttribute("innerText");
                        }

                        var video = _helper.FindByXPath("//video[@id='video-content_html5_api']");
                        var videoUrl = video.GetAttribute("src");
                        var filename = $"{i.ToString().PadLeft(3, '0')} - {title}.mp4";
                        filename = _helper.RemoveIllegalFilenameChars(filename);
                        Console.Clear();
                        Console.WriteLine($"Downloading {filename}...");
                        filename = $"{folder}\\{filename}";
                        if (File.Exists(filename)) continue;
                        webClient.DownloadFileTaskAsync(videoUrl, filename).ConfigureAwait(true).GetAwaiter().GetResult();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                System.Diagnostics.Debugger.Break();
            }
        }
    }
}
