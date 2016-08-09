namespace Microsoft.Edge.A11y
{
    using System;
    using System.IO;
    using System.Threading;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Edge;
    using OpenQA.Selenium.Remote;
    using OpenQA.Selenium.Support.UI;

    /// <summary>
    /// This is another wrapper around WebDriver. The reason this is used is to maintain
    /// compatibility with Edge internal testing tools.
    /// </summary>
    public class DriverManager
    {
        /// <summary>
        /// The WebDriver session this manager will use
        /// </summary>
        private RemoteWebDriver driver;

        /// <summary>
        /// How long to wait when searching for an element
        /// </summary>
        private TimeSpan searchTimeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="DriverManager" /> class
        /// </summary>
        /// <param name="searchTimeout">How long to search for elements</param>
        public DriverManager(TimeSpan searchTimeout)
        {
            try
            {
                this.driver = new EdgeDriver(EdgeDriverService.CreateDefaultService(DriverExecutablePath, "MicrosoftWebDriver.exe", 17556));
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Unable to start a WebDriver session. Ensure that the previous server window is closed.");
                Environment.Exit(1);
            }

            this.searchTimeout = searchTimeout;
        }

        /// <summary>
        /// Gets the root directory of the A11y project
        /// </summary>
        public static string ProjectRootFolder
        {
            get
            {
                return new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
            }
        }

        /// <summary>
        /// Gets the path to the WebDriver executables for Edge
        /// </summary>
        private static string DriverExecutablePath
        {
            get
            {
                var architecture = 
                    Environment.Is64BitOperatingSystem
                        ? "DriversExecutables\\AMD64"
                        : "DriversExecutables\\X86";

                // TODO support for linux/mac paths
                return Path.Combine(ProjectRootFolder, architecture);
            }
        }

        /// <summary>
        /// Navigate to url
        /// </summary>
        /// <param name="url">The url to which to navigate</param>
        public void NavigateToUrl(string url)
        {
            this.driver.Navigate().GoToUrl(url);
        }

        /// <summary>
        /// Execute a script on the current page
        /// </summary>
        /// <param name="script">The script</param>
        /// <param name="timeout">The timeout in seconds</param>
        /// <param name="additionalSleep">How long to wait before executing the script in milliseconds</param>
        /// <param name="args">Parameters to pass to the script</param>
        /// <returns>An object which contains the result of the script execution</returns>
        public object ExecuteScript(string script, int timeout, int additionalSleep = 0, params object[] args)
        {
            Thread.Sleep(additionalSleep);

            var wait = new WebDriverWait(this.driver, new TimeSpan(0, 0, 0, timeout));
            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

            var js = (IJavaScriptExecutor)this.driver;
            return js.ExecuteScript(script, args);
        }

        /// <summary>
        /// Send the given keys to the element with the given id
        /// </summary>
        /// <param name="elementId">The element's id</param>
        /// <param name="keys">The keys to send</param>
        public void SendKeys(string elementId, string keys)
        {
            this.driver.FindElement(By.Id(elementId)).SendKeys(keys);
        }

        /// <summary>
        /// Take a screenshot of the browser
        /// </summary>
        /// <returns>A screenshot of the browser</returns>
        public Screenshot GetScreenshot()
        {
            return this.driver.GetScreenshot();
        }

        /// <summary>
        /// Close the driver
        /// </summary>
        internal void Close()
        {
            if (null != this.driver)
            {
                try
                {
                    this.driver.Quit();
                    this.driver.Dispose();
                }
                catch (Exception)
                {
                    // Don't throw here
                }

                this.driver = null;
            }
        }
    }
}