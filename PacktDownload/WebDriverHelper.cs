using System;
using System.Collections.Generic;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace PacktDownload
{
    public class WebDriverHelper
    {
        private readonly ChromeDriver _webDriver;

        public WebDriverHelper(ChromeDriver webDriver)
        {
            _webDriver = webDriver ?? throw new ArgumentNullException(nameof(webDriver));
        }
        public void SendKeys(IWebElement element, string value)
        {
            Console.WriteLine($"Sending \"{value}\"");
            foreach (var c in value)
            {
                element.SendKeys(c.ToString());
                System.Threading.Thread.Sleep(300);
            }
        }

        public IWebElement FindByXPath(string xpath)
        {
            Console.WriteLine($"Finding element: {xpath}");
            return _webDriver.FindElementByXPath(xpath);
        }

        public IEnumerable<IWebElement> FindAllByXPath(string xpath)
        {
            Console.WriteLine($"Finding elements: {xpath}");
            return _webDriver.FindElementsByXPath(xpath);
        }

        public IWebElement FindById(string id)
        {
            Console.WriteLine($"Finding element id: {id}");
            return _webDriver.FindElementById(id);
        }

        public string RemoveIllegalFilenameChars(string filename)
        {
            foreach (var c in @"`=[]\;',/~!@#$%^&*()+{}|:""<>?'")
            {
                filename = filename.Replace(c.ToString(), string.Empty);
            }

            return filename;
        }

        public string PromptUser(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine();
        }

        public void Navigate(string url)
        {
            Console.WriteLine($"Navigating to url {url}");
            _webDriver.Navigate().GoToUrl(url);
        }

        public void FindAndClickByXPath(string xpath)
        {
            var element = FindByXPath(xpath);
            Click(element);
        }

        public void FindAndClickById(string id)
        {
            var element = FindById(id);
            Click(element);
        }

        public void Click(IWebElement webElement)
        {
            Console.WriteLine("Clicking element...");
            webElement.Click();
        }

        public void DelayedClick(IWebElement webElement)
        {
            Thread.Sleep(1000);
            webElement.Click();
        }
    }
}
