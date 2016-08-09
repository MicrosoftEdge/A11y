namespace Microsoft.Edge.A11y
{
    using OpenQA.Selenium;
    using OpenQA.Selenium.Edge;
    using OpenQA.Selenium.Remote;
    using OpenQA.Selenium.Support.UI;
    using System;
    using System.IO;
    using System.Threading;

    /// <summary>
    /// This is another wrapper around WebDriver. The reason this is used is to maintain
    /// compatibility with Edge internal testing tools.
    /// </summary>
    public class DriverManager
    {
        private RemoteWebDriver _driver;
        private TimeSpan _searchTimeout;

        /// <summary>
        /// Only ctor
        /// </summary>
        /// <param name="searchTimeout">How long to search for elements</param>
        public DriverManager(TimeSpan searchTimeout)
        {
            try
            {
                _driver = new EdgeDriver(EdgeDriverService.CreateDefaultService(DriverExecutablePath, "MicrosoftWebDriver.exe", 17556));
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Unable to start a WebDriver session. Ensure that the previous server window is closed.");
                Environment.Exit(1);
            }
            _searchTimeout = searchTimeout;
        }

        /// <summary>
        /// The root directory of the A11y project
        /// </summary>
        public static string ProjectRootFolder
        {
            get
            {
                return new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
            }
        }

        /// <summary>
        /// The path to the WebDriver executables for Edge
        /// </summary>
        private static string DriverExecutablePath
        {
            get
            {
                //TODO support for linux/mac paths
                return Path.Combine(ProjectRootFolder, Environment.Is64BitOperatingSystem
                    ? "DriversExecutables\\AMD64"
                    : "DriversExecutables\\X86");
            }
        }

        /// <summary>
        /// Navigate to url
        /// </summary>
        /// <param name="url"></param>
        public void NavigateToUrl(string url)
        {
            _driver.Navigate().GoToUrl(url);
        }

        /// <summary>
        /// Execute a script on the current page
        /// </summary>
        /// <param name="script">The script</param>
        /// <param name="timeout">The timeout in seconds</param>
        /// <param name="additionalSleep">How long to wait before executing the script in milliseconds</param>
        /// <param name="args">Parameters to pass to the script</param>
        /// <returns></returns>
        public object ExecuteScript(string script, int timeout, int additionalSleep = 0, params object[] args)
        {
            Thread.Sleep(additionalSleep);

            var wait = new WebDriverWait(_driver, new TimeSpan(0, 0, 0, timeout));
            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

            var js = (IJavaScriptExecutor)_driver;
            return js.ExecuteScript(script, args);
        }

        /// <summary>
        /// Send the given keys to the element with the given id
        /// </summary>
        /// <param name="elementId">The element's id</param>
        /// <param name="keys">The keys to send</param>
        public void SendKeys(string elementId, string keys)
        {
            _driver.FindElement(By.Id(elementId)).SendKeys(keys);
        }

        public Screenshot GetScreenshot()
        {
            return _driver.GetScreenshot();
        }

        /// <summary>
        /// Close the driver
        /// </summary>
        internal void Close()
        {
            if (null != _driver)
            {
                try
                {
                    _driver.Quit();
                    _driver.Dispose();
                }
                catch (Exception)
                {
                    // Don't throw here
                }
                _driver = null;
            }
        }
    }
}