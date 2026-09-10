using AventStack.ExtentReports;
using Automation.Base.Utils;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;

namespace Automation.Base.Core
{
    /// <summary>To initialize the driver, input data and test case report for each test case iteration</summary>
    public class TestInit : CommonUtilities, IDisposable
    {
        /// <summary>Represents the current test case being executed</summary>
		public ITest testCase;
		/// <summary>An object of ITestOutputHelper</summary>
		public ITestOutputHelper output;
		/// <summary>Determines the current iteration number of the test case being executed</summary>
		public int currentIteration;
		/// <summary>Determines the iteration count of the test case being executed</summary>
		public int iterationCount;

		/// <summary>
		/// Performs all the pre-requisites for each test case method to be executed
		/// like creating an Extent Test, launching the browser, fetching test data, etc.
		/// </summary>
		/// <param name="output">ITestOutputHelper object</param>
		public TestInit(ITestOutputHelper output)
        {
			//Wait until Test Runner reports the previous Test Case result
			//Once found, make it false again for the next Test Case
			while(!MyMessageSink.reportStatus) { }
			MyMessageSink.reportStatus = false;

			//Get the test case context
			this.output = output;
            var type = output.GetType();
            var testMember = type.GetField("test", BindingFlags.Instance | BindingFlags.NonPublic);
            testCase = (ITest)testMember.GetValue(output);
            testCaseName = testCase.TestCase.TestMethod.Method.Name;
			iterationCount = testIterationCount.GetValueOrDefault(testCaseName, 1);
			currentIteration = currentTestIteration.GetValueOrDefault(testCaseName, 1);

			// setup test data
			data = IOUtil.GetInputData(dataSheet, testCaseName, currentIteration);
			
			//Start the extent test
			var name = string.Format("{0} : Iteration {1}", testCaseName, currentIteration);
			var desc = string.Format("{0} [Browser : {1}]", data.GetValueOrDefault("Description", ""), browser);
			test = extent.CreateTest(name, desc);
			if (currentIteration != iterationCount)
				currentTestIteration[testCaseName] = currentIteration + 1;

			//Veriy to whether open the browser
			if(data.GetValueOrDefault("Open Browser", "Y").ToUpper().Contains("Y")) { LaunchBrowser(); }
		}

		/// <summary>Launches the browser with the Base URL mentioned in Common Parameters in the Excel input data</summary>
		public void LaunchBrowser()
		{
			//Navigate to URL
			var url = baseURL + data.GetValueOrDefault("Trailing URL", "");
			CloseBrowserIfOpen();

			//Setting up Chrome Driver options
			var option = new ChromeOptions();
			option.AddUserProfilePreference("download.default_directory", downloadDirectory);
			option.AddUserProfilePreference("download.prompt_for_download", false);
			option.AddUserProfilePreference("intl.accept_languages", "nl");
			option.AddUserProfilePreference("disable-popup-blocking", "true");
			option.AddArgument("--window-size=" + browserResolution);
			if (browser.ToUpper().Contains("HEADLESS"))
			{
				option.AddArgument("--headless");
			}

			driver = (browser.ToUpper()) switch
			{
				"CHROME" => new ChromeDriver(option),
				"IE" => new InternetExplorerDriver(),
				"INTERNET EXPLORER" => new InternetExplorerDriver(),
				"FF" => new FirefoxDriver(),
				"FIREFOX" => new FirefoxDriver(),
				"HEADLESS" => new ChromeDriver(option),
				"HEADLESS CHROME" => new ChromeDriver(option),
				_ => new ChromeDriver(option),
			};

			driver.Manage().Window.Maximize();
			driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(double.Parse(props.GetValueOrDefault("timeout", "30")));
			js = (IJavaScriptExecutor)driver;

			try
			{
				if (!url.Equals(""))
				{
					driver.Navigate().GoToUrl(url);
				}
			}
			catch(Exception)
			{
				Report(Status.Fail, "Unable to hit the URL: " + url, true);
			}
		}

        /// <summary>Performs all the tear down operation, since Reporting is done at the end of every test method</summary>
        public void Dispose()
		{
			// clean-up code
			MyMessageSink.screenShot = GetScreenshot("Fail");
			CloseBrowserIfOpen();
			data.Clear();
			js = null;
        }
    }

	/// <summary>To decide the no. of iterations of execution for each test case, build the solution before each run</summary>
	public class RepeatAttribute : Xunit.Sdk.DataAttribute
    {
		/// <summary>Gets the data sheet from Excel input file</summary>
		public RepeatAttribute()
		{
			IOUtil.iterationDataPath = "..\\..\\..\\Resources\\Input.xlsx";
		}

		/// <summary>Sets the execution iteration count for each test case</summary>
		/// <param name="testMethod"></param>
		public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            foreach (var iterationNumber in Enumerable.Range(start: 1, count: IOUtil.GetIterationCountExcel(testMethod.Name)))
            {
				yield return new object[] { iterationNumber };
            }
		}
    }
}
