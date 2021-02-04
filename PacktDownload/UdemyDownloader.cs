// ReSharper disable IdentifierTypo

using System;
using System.Collections.Generic;
using System.Linq;
using Delimon.Win32.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace PacktDownload
{
    public class UdemyDownloader
    {
        private readonly ChromeDriver _weDriver;
        private readonly WebDriverHelper _helper;

        public UdemyDownloader(ChromeDriver webDriver,
            WebDriverHelper helper)
        {
            _weDriver = webDriver ?? throw new ArgumentNullException(nameof(webDriver));
            _helper = helper ?? throw new ArgumentNullException(nameof(helper));
        }

        public void Process()
        {
            try
            {
                var urlAndFolder = GetUrlAndFolder();
                if (urlAndFolder == null) return;
                var url = urlAndFolder.Item1;
                var folder = urlAndFolder.Item2;

                Login(url, "greghill974@gmail.com", "roXyuey0");
                var courseTitle = GetCourseTitle();
                var sectionsAndTitles = GetSectionsAndTitles();

                foreach (var sectionsAndTitle in sectionsAndTitles)
                {
                    Console.WriteLine(sectionsAndTitle.Key);
                    Console.WriteLine(string.Join(Environment.NewLine, sectionsAndTitle.Value));
                }

                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
            }
        }

        private Tuple<string, string> GetUrlAndFolder()
        {
            //var url = _helper.PromptUser("Url: ");
            var url =
                "https://www.udemy.com/course/react-and-typescript-build-a-portfolio-project/learn/lecture/24209082?start=90#overview";
            if (string.IsNullOrWhiteSpace(url)) return null;
            //var folder = _helper.PromptUser("Target folder: ");
            var folder = @"C:\Users\rchil\Documents\Downloads";
            if (string.IsNullOrWhiteSpace(folder)) return null;
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            return new Tuple<string, string>(url, folder);
        }

        private Dictionary<string, string[]> GetSectionsAndTitles()
        {
            const string sectionsXpath = @"//div[@data-purpose='curriculum-section-container']/div";
            var sections = _helper.FindAllByXPath(sectionsXpath).ToArray();
            if (sections.Length == 0) return null;
            var dic = new Dictionary<string, string[]>();
            foreach (var section in sections)
            {
                var sectionHeadingElement = section.FindElement(
                    By.XPath(@"./div[@data-purpose='section-heading']"));
                var titleElement = sectionHeadingElement.FindElement(
                    By.XPath(@"./div[@data-purpose='section-label']/span/span/span"));
                var sectionTitle = titleElement.GetAttribute("innerText");
                // is this section expanded?
                var expanded = sectionHeadingElement.GetAttribute("aria-expanded") ?? "false";
                if (!bool.Parse(expanded))
                {
                    _helper.DelayedClick(sectionHeadingElement);
                }

                var listItems = section.FindElements(By.XPath("./ul/li"));
                var videoTitles = new List<string>();
                foreach (var listItem in listItems)
                {
                    var videoTitleElement =
                        listItem.FindElements(
                                By.XPath(".//div[starts-with(@class, 'curriculum-item-link--title')]"))
                            .FirstOrDefault();
                    if (videoTitleElement == null) continue;
                    var videoTitle = videoTitleElement.GetAttribute("innerText");
                    videoTitles.Add(videoTitle);
                }

                dic.Add(sectionTitle, videoTitles.ToArray());
            }

            return dic;
        }

        private IEnumerable<IWebElement> GetSections()
        {
            throw new NotImplementedException();
        }

        private string GetCourseTitle()
        {
            const string titleXpath = @"//a[@data-purpose='course-header-title']";
            var element = _helper.FindByXPath(titleXpath);
            var title = element.GetAttribute("innerText");
            return (title ?? string.Empty).Trim();
        }

        private void Login(string url, string username, string password)
        {
            _helper.Navigate(url);
            const string emailXpath = @"//input[@data-purpose='email']";
            const string pwdXpath = @"//input[@data-purpose='password']";
            const string loginBtnXpath = @"//input[@data-purpose='do-login']";

            _helper.SendKeys(_helper.FindByXPath(emailXpath), username);
            _helper.SendKeys(_helper.FindByXPath(pwdXpath), password);
            System.Threading.Thread.Sleep(1000);
            _helper.FindAndClickByXPath(loginBtnXpath);
        }
    }
}
