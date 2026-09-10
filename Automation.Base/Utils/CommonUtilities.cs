using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using Automation.Base.Core;
using NPOI.SS.UserModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using Xunit;

namespace Automation.Base.Utils
{
	/// <summary>
	/// To make use of the most commonly used functionalities while performing automated testing like clicking, typing
	/// and selecting web elements, verifying text in the fields, working with the browsers, getting screenshots,
	/// reporting each test step result, getting Responses for Request API calls and so on
	/// </summary>
	public class CommonUtilities
	{
		/// <summary>Drives the browser</summary>
		public static IWebDriver driver;
		/// <summary>Browser Name</summary>
		public static string browser;
		/// <summary>Browser Resolution</summary>
		public static string browserResolution;
		/// <summary>Current Test Case Input Data Dictionary</summary>
		public static Dictionary<string, string> data = new Dictionary<string, string>();
		/// <summary>User_Config Properties Data Dictionary</summary>
		public static Dictionary<string, string> props = new Dictionary<string, string>();
		/// <summary>Iteration Count Dictionary of Test Cases</summary>
		public static Dictionary<string, int> testIterationCount = new Dictionary<string, int>();
		/// <summary>Current Iteration Number Dictionary of Test Cases</summary>
		public static Dictionary<string, int> currentTestIteration = new Dictionary<string, int>();
		/// <summary>Input Data Sheet</summary>
		public static ISheet dataSheet;
		/// <summary></summary>
		public static ExtentV3HtmlReporter reporter;
		/// <summary></summary>
		public static ExtentTest test;
		/// <summary></summary>
		public static AventStack.ExtentReports.ExtentReports extent;
		/// <summary>Test Project Path</summary>
		public static string projectPath;
		/// <summary>Extent Report Path</summary>
		public static string reportPath;
		/// <summary>Report Folder Path</summary>
		public static string reportFolderPath;
		/// <summary>Report Folder Name</summary>
		public static string reportFolderName;
		/// <summary>Download Directory</summary>
		public static string downloadDirectory;
		/// <summary>Base URL</summary>
		public static string baseURL;
		/// <summary>Current test case name</summary>
		public static string testCaseName;
		/// <summary>Actions Object</summary>
		public static Actions builder = null;
		/// <summary>JavaScriptExecutor Object</summary>
		public static IJavaScriptExecutor js;

		/// <summary>Gets the properties data from the user_config properties file into the props data field</summary>
		/// <param name="path">The file to open for reading</param>
		public static void GetProperties(string path)
		{
			props.Clear();
			foreach (var row in File.ReadAllLines(path))
			{
				if (!(row.StartsWith(Constants.Hash) || row.Length == 0))
					props.Add(row.Split(Constants.EqualToChar)[0].Trim(), row.Split(Constants.EqualToChar)[1].Trim());
			}
		}

		/// <summary>Gets the path of the screenshot saved representing the image of the page on the screen</summary>
		/// <param name="status">Test Step Result</param>
		/// <returns>The path where the screenshot is saved</returns>
		public string GetScreenshot(string status)
		{
			if (driver != null)
				return null;

			try
			{
				var srcFile = ((ITakesScreenshot)driver).GetScreenshot();
				var imageName = "SS_" + DateTime.Now.ToString(Constants.DateTimeFormat) + "_" + testCaseName + "_" + status.ToUpper() + ".png";
				var destFile = reportFolderPath + "\\" + imageName;
				srcFile.SaveAsFile(destFile);
				return imageName;
			}
			catch(Exception e)
			{
				Console.WriteLine(e.ToString());
				return null;
			}
		}

		/// <summary>Reports the test step to be published in the Test Suite Execution Report</summary>
		/// <param name="status">Test Step Result</param>
		/// <param name="description">Test Step Description</param>
		/// <param name="isScreenshotNeeded">Screenshot Required?</param>
		public void Report(Status status, string description, bool isScreenshotNeeded)
		{
			MediaEntityModelProvider MediaModel = null;

			if (isScreenshotNeeded)
			{
				string screenShot;
				if (!MyMessageSink.useScreenShot)
					screenShot = GetScreenshot(status.ToString());
				else
				{
					MyMessageSink.useScreenShot = false;
					screenShot = MyMessageSink.screenShot;
				}

				if (screenShot != null)
					MediaModel = MediaEntityBuilder.CreateScreenCaptureFromPath(screenShot).Build();
				else
					description += " (Unable to take the screenshot due to a WebDriver Exception).";
			}

			test.Log(status, description, MediaModel);
			if(status == Status.Fail)
				Assert.True(false, description);
		}

		/// <summary>Gets the response of the POST API Request according to the inputs passed as parameters</summary>
		/// <param name="basicURI">Basaic URI</param>
		/// <param name="pathParams">Path Parameters</param>
		/// <param name="queryParams">Query Parameters</param>
		/// <param name="payload">Payload</param>
		/// <param name="token">Authorization Token</param>
		/// <param name="headers">Request Headers</param>
		/// <param name="graphQLQuery">GraphQL Query</param>
		/// <param name="pathParamSeparator">Path Parameters Separator</param>
		/// <returns>Response as an IRestResponse object of the POST Request passed</returns>
		public IRestResponse GetResponseFromPostAPI(string basicURI, string[] pathParams, Dictionary<string, string> queryParams = null, string payload = "", string token = "", Dictionary<string, string> headers = null, string graphQLQuery = "", string pathParamSeparator = Constants.Slash)
		{
			var restClient = new RestClient(basicURI);
			RestRequest restRequest;

			if (pathParams != null)
			{
				var param = Constants.Slash + string.Join(pathParamSeparator, pathParams);
				restRequest = new RestRequest(param, Method.POST);
			}
			else
				restRequest = new RestRequest(Method.POST);

			if (!string.IsNullOrWhiteSpace(payload))
				restRequest.AddParameter("application/json", payload, ParameterType.RequestBody);

			if (queryParams != null)
			{
				foreach (var queryparam in queryParams)
					restRequest.AddQueryParameter(queryparam.Key, queryparam.Value);
			}

			if (graphQLQuery != null)
				restRequest.AddParameter("application/json", graphQLQuery, ParameterType.RequestBody);

			restRequest.AddHeader("Content-Type", "application/json");
			if (!string.IsNullOrWhiteSpace(token))
				restRequest.AddHeader("Authorization", token);

			if (headers != null)
			{
				foreach (var header in headers)
					restRequest.AddHeader(header.Key, header.Value);
			}

			return restClient.Execute(restRequest);
		}

		/// <summary>Gets the response of the GET API Request according to the inputs passed as parameters</summary>
		/// <param name="basicURI">Basaic URI</param>
		/// <param name="pathParams">Path Parameters</param>
		/// <param name="queryParams">Query Parameters</param>
		/// <param name="token">Authorization Token</param>
		/// <param name="headers">Request Headers</param>
		/// <param name="pathParamSeparator">Path Parameters Separator</param>
		/// <returns>Response as an IRestResponse object of the GET Request passed</returns>
		public IRestResponse GetResponseFromGetAPI(string basicURI, string[] pathParams, Dictionary<string, string> queryParams = null, string token = "", Dictionary<string, string> headers = null, string pathParamSeparator = Constants.Slash)
		{
			var restClient = new RestClient(basicURI);
			RestRequest restRequest;

			if (pathParams != null)
			{
				var param = Constants.Slash + string.Join(pathParamSeparator, pathParams);
				restRequest = new RestRequest(param, Method.GET);
			}
			else
				restRequest = new RestRequest(Method.GET);

			if (queryParams != null)
			{
				foreach (var queryparam in queryParams)
					restRequest.AddQueryParameter(queryparam.Key, queryparam.Value);
			}

			restRequest.AddHeader("Content-Type", "application/json");
			if (!string.IsNullOrWhiteSpace(token))
				restRequest.AddHeader("Authorization", token);

			if (headers != null)
			{
				foreach (var header in headers)
					restRequest.AddHeader(header.Key, header.Value);
			}

			return restClient.Execute(restRequest);
		}

		/// <summary>Gets the response of the DELETE API Request according to the inputs passed as parameters</summary>
		/// <param name="basicURI">Basaic URI</param>
		/// <param name="pathParams">Path Parameters</param>
		/// <param name="queryParams">Query Parameters</param>
		/// <param name="token">Authorization Token</param>
		/// <param name="headers">Request Headers</param>
		/// <param name="pathParamSeparator">Path Parameters Separator</param>
		/// <returns>Response as an IRestResponse object of the DELETE Request passed</returns>
		public IRestResponse GetResponseFromDeleteAPI(string basicURI, string[] pathParams, Dictionary<string, string> queryParams = null, string token = null, Dictionary<string, string> headers = null, string pathParamSeparator = Constants.Slash)
		{
			var restClient = new RestClient(basicURI);
			RestRequest restRequest;

			if (pathParams != null)
			{
				var param = Constants.Slash + string.Join(pathParamSeparator, pathParams);
				restRequest = new RestRequest(param, Method.DELETE);
			}
			else
				restRequest = new RestRequest(Method.DELETE);

			if (queryParams != null)
			{
				foreach (var queryparam in queryParams)
					restRequest.AddQueryParameter(queryparam.Key, queryparam.Value);
			}

			restRequest.AddHeader("Content-Type", "application/json");

			if (!string.IsNullOrWhiteSpace(token))
				restRequest.AddHeader("Authorization", token);

			if (headers != null)
			{
				foreach (var header in headers)
					restRequest.AddHeader(header.Key, header.Value);
			}

			return restClient.Execute(restRequest);
		}

		/// <summary>Gets the response of the PuT API Request according to the inputs passed as parameters</summary>
		/// <param name="basicURI">Basaic URI</param>
		/// <param name="pathParams">Path Parameters</param>
		/// <param name="queryParams">Query Parameters</param>
		/// <param name="payload">Payload</param>
		/// <param name="token">Authorization Token</param>
		/// <param name="headers">Request Headers</param>
		/// <param name="graphQLQuery">GraphQL Query</param>
		/// <param name="pathParamSeparator">Path Parameters Separator</param>
		/// <returns>Response as an IRestResponse object of the PUT Request passed</returns>
		public IRestResponse GetResponseFromPutAPI(string basicURI, string[] pathParams, Dictionary<string, string> queryParams = null, string payload = "", string token = "", Dictionary<string, string> headers = null, string graphQLQuery = "", string pathParamSeparator = Constants.Slash)
		{
			var restClient = new RestClient(basicURI);
			RestRequest restRequest;

			if (pathParams != null)
			{
				var param = Constants.Slash + string.Join(pathParamSeparator, pathParams);
				restRequest = new RestRequest(param, Method.PUT);
			}
			else
				restRequest = new RestRequest(Method.PUT);

			if (!string.IsNullOrWhiteSpace(payload))
				restRequest.AddParameter("application/json", payload, ParameterType.RequestBody);

			if (queryParams != null)
			{
				foreach (var queryparam in queryParams)
					restRequest.AddQueryParameter(queryparam.Key, queryparam.Value);
			}

			if (graphQLQuery != null)
				restRequest.AddParameter("application/json", graphQLQuery, ParameterType.RequestBody);

			restRequest.AddHeader("Content-Type", "application/json");
			if (!string.IsNullOrWhiteSpace(token))
				restRequest.AddHeader("Authorization", "bearer " + token);

			if (headers != null)
			{
				foreach (var header in headers)
					restRequest.AddHeader(header.Key, header.Value);
			}

			return restClient.Execute(restRequest);
		}

		/// <summary>Gets the response of the PATCH API Request according to the inputs passed as parameters</summary>
		/// <param name="basicURI">Basaic URI</param>
		/// <param name="pathParams">Path Parameters</param>
		/// <param name="queryParams">Query Parameters</param>
		/// <param name="payload">Payload</param>
		/// <param name="token">Authorization Token</param>
		/// <param name="headers">Request Headers</param>
		/// <param name="graphQLQuery">GraphQL Query</param>
		/// <param name="pathParamSeparator">Path Parameters Separator</param>
		/// <returns>Response as an IRestResponse object of the PATCH Request passed</returns>
		public IRestResponse GetResponseFromPatchAPI(string basicURI, string[] pathParams, Dictionary<string, string> queryParams = null, string payload = "", string token = "", Dictionary<string, string> headers = null, string graphQLQuery = "", string pathParamSeparator = Constants.Slash)
		{
			var restClient = new RestClient(basicURI);
			RestRequest restRequest;

			if (pathParams != null)
			{
				var param = Constants.Slash + string.Join(pathParamSeparator, pathParams);
				restRequest = new RestRequest(param, Method.PATCH);
			}
			else
				restRequest = new RestRequest(Method.PATCH);

			if (!string.IsNullOrWhiteSpace(payload))
				restRequest.AddParameter("application/json", payload, ParameterType.RequestBody);

			if (queryParams != null)
			{
				foreach (var queryparam in queryParams)
					restRequest.AddQueryParameter(queryparam.Key, queryparam.Value);
			}

			if (graphQLQuery != null)
				restRequest.AddParameter("application/json", graphQLQuery, ParameterType.RequestBody);

			restRequest.AddHeader("Content-Type", "application/json");
			if (!string.IsNullOrWhiteSpace(token))
				restRequest.AddHeader("Authorization", token);

			if (headers != null)
			{
				foreach (var header in headers)
					restRequest.AddHeader(header.Key, header.Value);
			}

			return restClient.Execute(restRequest);
		}

		/// <summary>Gets the response of the Form data API Request according to the inputs passed as parameters</summary>
		/// <param name="methodType">Type of Method to be used</param>
		/// <param name="basicURI">Basaic URI</param>
		/// <param name="pathParams">Path Parameters</param>
		/// <param name="payload">Payload</param>
		/// <param name="files">List of files to be uploaded</param>
		/// <param name="token">Authorization Token</param>
		/// <param name="headers">Request Headers</param>
		/// <param name="pathParamSeparator">Path Parameters Separator</param>
		/// <returns>Response as an IRestResponse object of the POST Request passed</returns>
		public IRestResponse GetResponseFormDataAPI(Method methodType, string basicURI, string[] pathParams, Dictionary<string, string> payload = null, IList<string> files = null, string token = "", Dictionary<string, string> headers = null, string pathParamSeparator = Constants.Slash)
		{
			var restClient = new RestClient(basicURI);
			RestRequest restRequest;

			if (pathParams != null)
			{
				var param = Constants.Slash + string.Join(pathParamSeparator, pathParams);
				restRequest = new RestRequest(param, methodType);
			}
			else
				restRequest = new RestRequest(methodType);

			if (payload != null)
				restRequest.AddParameter(payload.GetValueOrDefault("name", ""), payload.GetValueOrDefault("value", ""), ParameterType.RequestBody);

			if (files != null)
			{
				foreach (var file in files)
					restRequest.AddFile("file", file);
			}

			restRequest.AddHeader("Content-Type", "multipart/form-data");
			if (!string.IsNullOrWhiteSpace(token))
				restRequest.AddHeader("Authorization", token);

			if (headers != null)
			{
				foreach (var header in headers)
					restRequest.AddHeader(header.Key, header.Value);
			}

			return restClient.Execute(restRequest);
		}

		/// <summary>Opens a URL in the driver's browser</summary>
		/// <param name="strURL">URL</param>
		public void Open(string strURL)
        {
            driver.Url = strURL;
            Report(Status.Info, "Navigated to page - " + strURL, false);
        }

		/// <summary>Waits for a specified amount of seconds</summary>
		/// <param name="waitSeconds">Wait time in seconds</param>
		public void WaitFor(int waitSeconds) => Thread.Sleep(waitSeconds * 1000);

		/// <summary>Switches to a specified tab window of the browser</summary>
		/// <param name="i">Tab number</param>
		public void SwitchToTab(int i)
        {
            IList<string> tabs = new List<string>(driver.WindowHandles);
            driver.SwitchTo().Window(tabs[i-1]);
        }

		/// <summary>Closes the current window and switches back to the last tab window of the browser</summary>
		public void SwitchToPreviousTab()
        {
            IList<string> tabs = new List<string>(driver.WindowHandles);
            driver.Close();
            driver.SwitchTo().Window(tabs[tabs.Count - 2]);
        }

		/// <summary>Closes a specified number of tab windows starting from the last tab of the browser</summary>
		/// <param name="noOfTabs">No. of tabs to be closed</param>
		public void CloseNoOfTabs(int noOfTabs)
		{
			IList<string> tabs = new List<string>(driver.WindowHandles);
			var n = tabs.Count - 1;

			for (; noOfTabs != 0; n--, noOfTabs--)
				driver.SwitchTo().Window(tabs[n]).Close();

			driver.SwitchTo().Window(tabs[n]);
		}

		/// <summary>Opens a new tab in the browser</summary>
		public void OpenNewTab() => js.ExecuteScript("window.open()");

		/// <summary>Closes the browser</summary>
		public void CloseBrowserIfOpen()
		{
			if (driver != null)
				driver.Quit();
		}

		/// <summary>Switches to the frame using its name</summary>
		/// <param name="frameName">Frame Name</param>
		public void SwitchToFrame(string frameName)
        {
            driver.SwitchTo().Frame(frameName);
            WaitFor(2);
        }

		/// <summary>Validate that the URL contains the parameterized value</summary>
		/// <param name="url">URL</param>
		public void ValidateURLContains(string url)
		{
			if(WaitUntilUrlContains(url))
				Report(Status.Pass, "The URL includes " + url, false);
            else
                Report(Status.Error, "The URL doesn't includes " + url, true);
		}

		/// <summary>Validate that the URL in a new tab contains the parameterized value</summary>
		/// <param name="url">URL</param>
		public void ValidateURLContainsInNewTab(string url)
		{
			SwitchToTab(2);
			if (url.Contains("hellohumankindness"))
				HandleChromePrivacyError();
			ValidateURLContains(url);
			SwitchToPreviousTab();
		}

		/// <summary>Validate the title of the page</summary>
		/// <param name="title">Title</param>
		public void ValidatePageTitle(string title)
		{
			if (driver.Title.Equals(title))
				Report(Status.Pass, title + "is the title of the page as expected", false);
			else
				Report(Status.Error, title + "is not the title of the page", true);
		}

		/// <summary>Handle the privacy error page, if displayed</summary>
		public void HandleChromePrivacyError()
		{
			if (IsDisplayed(By.XPath("//*[contains(text(), 'Your connection is not private')]")))
			{
				Click(By.XPath("//button[contains(text(), 'Advanced')]"));
				Click(By.XPath("//a[@id='proceed-link']"));
			}
		}

		/// <summary>Navigates back to the browser's previous page</summary>
		public void NavigateBackward() => driver.Navigate().Back();

		/// <summary>Navigates forward to the browser's next page</summary>
		public void NavigateForward() => driver.Navigate().Forward();

		/// <summary>Refresh the browser's page</summary>
		public void RefreshPage() => driver.Navigate().Refresh();

		/// <summary>Gets the WebElement using the parameterized locator</summary>
		/// <param name="by">By Locator</param>
		/// <returns>Element as a WebElement object</returns>
		public IWebElement GetElement(By by)
        {
            try
			{
				return driver.FindElement(by);
			}
			catch (Exception)
			{
				return null;
			}
        }

		/// <summary>Verifies if the element is displayed on the screen</summary>
		/// <param name="by">By Locator</param>
		/// <returns>Boolean Result</returns>
		public bool IsDisplayed(By by)
		{
			var ele = GetElement(by);
			if(ele == null)
				return false;

			return IsDisplayed(ele);
		}

		/// <summary>Verifies if the element is displayed on the screen</summary>
		/// <param name="ele">Web Element</param>
		/// <returns>Boolean Result</returns>
		public bool IsDisplayed(IWebElement ele)
		{
			if (ele.Displayed)
				return true;
            else
                return false;
		}

		/// <summary>Verifies if the element is selected on the screen</summary>
		/// <param name="by">By Locator</param>
		/// <returns>Boolean Result</returns>
		public bool IsSelected(By by)
		{
			var ele = GetElement(by);
			if (ele == null)
				return false;

			return IsSelected(ele);
		}

		/// <summary>Verifies if the element is selected on the screen</summary>
		/// <param name="ele">Web Element</param>
		/// <returns>Boolean Result</returns>
		public bool IsSelected(IWebElement ele)
		{
			if (ele.Selected)
				return true;
			else
				return false;
		}

		/// <summary>Gets the list of WebElements using the parameterized locator</summary>
		/// <param name="by">By Locator</param>
		/// <returns>A list of elements as a WebElement Collection object</returns>
		public ReadOnlyCollection<IWebElement> GetElements(By by)
        {
            try
			{
				return driver.FindElements(by);
			}
			catch (Exception)
			{
				return null;
			}
        }

		/// <summary>Gets the Select WebElement using the parameterized locator</summary>
		/// <param name="by">By Locator</param>
		/// <returns>Element as a Select WebElement object</returns>
		public SelectElement GetDropdown(By by)
        {
			if (GetElement(by) != null)
				return new SelectElement(GetElement(by));
			else
				return null;
		}

		/// <summary>Highlights the WebElement, if found using the parameterized locator</summary>
		/// <param name="by">By Locator</param>
		public void Highlight(By by)
		{
			var ele = GetElement(by);
			if (ele != null)
				Highlight(ele);
		}

		/// <summary>Highlights the parameterized WebElement</summary>
		/// <param name="ele">Web Element</param>
		public void Highlight(IWebElement ele)
		{
			js.ExecuteScript("arguments[0].style.border='3px solid red'", ele);
			Thread.Sleep(2000);
			Report(Status.Info, "The field appears in UI", true);
		}

		/// <summary>Clicks on the parameterized WebElement</summary>
		/// <param name="ele">Web Element</param>
		/// <param name="elementName">Element Name for Test Reporting</param>
		/// <returns>Boolean Result</returns>
		public bool Click(IWebElement ele, string elementName = "")
        {
            try
            {
                ele.Click();
				if (!string.IsNullOrWhiteSpace(elementName))
					Report(Status.Pass, elementName + " is clicked Successfully", false);
				return true;
            }
            catch (Exception)
            {
				if (!string.IsNullOrWhiteSpace(elementName))
					Report(Status.Fail, elementName + " is not clicked", true);
				return false;
            }
        }

		/// <summary>Clicks on the WebElement found using the parameterized locator</summary>
		/// <param name="by">By Locator</param>
		/// <param name="elementName">Element Name for Test Reporting</param>
		/// <returns>Boolean Result</returns>
		public bool Click(By by, string elementName = "")
        {
			var ele = GetElement(by);
			if(ele != null)
                return Click(ele, elementName);
            else
            {
				if (!string.IsNullOrWhiteSpace(elementName))
					Report(Status.Fail, "Unable to find the element '" + elementName + "' on the screen", true);
				return false;
            }
        }

		/// <summary>Types into the parameterized WebElement</summary>
		/// <param name="ele">Web Element</param>
		/// <param name="strText">Text</param>
		/// <param name="elementName">Element Name for Test Reporting</param>
		/// <returns>Boolean Result</returns>
		public bool Type(IWebElement ele, string strText, string elementName = "")
        {
            try
            {
                ele.SendKeys(strText);
				if (!string.IsNullOrWhiteSpace(elementName))
					Report(Status.Pass, "The text '" + strText + "' is entered in the element '" + elementName + "' successfully", false);			
				return true;
            }
            catch (Exception)
            {
				if (!string.IsNullOrWhiteSpace(elementName))
					Report(Status.Fail, "The text '" + strText + "' is not entered in the element '" + elementName + "'", true);
				return false;
            }
        }

		/// <summary>Types into the WebElement found using the parameterized locator</summary>
		/// <param name="by">By Locator</param>
		/// <param name="strText">Text</param>
		/// <param name="elementName">Element Name for Test Reporting</param>
		/// <returns>Boolean Result</returns>
		public bool Type(By by, string strText, string elementName = "")
        {
			var ele = GetElement(by);
			if (ele != null)
				return Type(ele, strText, elementName);
			else
			{
				if (!string.IsNullOrWhiteSpace(elementName))
					Report(Status.Fail, "Unable to find the element '" + elementName + "' on the screen", true);
				return false;
			}
        }

		/// <summary>Selects all of the text in the parameterized WebElement</summary>
		/// <param name="ele">Web Element</param>
		/// <returns>Boolean Result</returns>
		public bool SelectAllText(IWebElement ele)
		{
			try
			{
				ele.SendKeys(Keys.Control + "a");
				return true;
			}
			catch (Exception) { return false; }
		}

		/// <summary>Selects all of the text in the WebElement found using the parameterized locator</summary>
		/// <param name="by">By Locator</param>
		/// <returns>Boolean Result</returns>
		public bool SelectAllText(By by)
		{
			var ele = GetElement(by);
			if(ele != null)
				return SelectAllText(ele);
			else
				return false;
		}

		/// <summary>Selects Visible Text from a parameterized dropdown element</summary>
		/// <param name="ele">Web Element</param>
		/// <param name="strVisibleText">Visible Text</param>
		/// <param name="elementName">Element Name for Test Reporting</param>
		/// <returns>Boolean Result</returns>
		public bool SelectDropdownByText(SelectElement ele, string strVisibleText, string elementName = "")
        {
            try
            {
                ele.SelectByText(strVisibleText);
				if (!string.IsNullOrWhiteSpace(elementName))
					Report(Status.Pass, "The option '" + strVisibleText + "' is selected for '" + elementName + "' successfully", false);
				return true;
            }
            catch (Exception)
            {
				if (!string.IsNullOrWhiteSpace(elementName))
					Report(Status.Fail, "The option '" + strVisibleText + "' is not selected for '" + elementName + "'", true);
				return false;
			}
		}

		/// <summary>Selects Visible Text from a dropdown element found using the parameterized locator</summary>
		/// <param name="by">By Locator</param>
		/// <param name="strVisibleText">Visible Text</param>
		/// <param name="elementName">Element Name for Test Reporting</param>
		/// <returns>Boolean Result</returns>
		public bool SelectDropdownByText(By by, string strVisibleText, string elementName = "")
        {
			var ele = GetDropdown(by);
			if (ele != null)
                return SelectDropdownByText(ele, strVisibleText, elementName);
            else
            {
				if (!string.IsNullOrWhiteSpace(elementName))
					Report(Status.Fail, "Unable to find the element '" + elementName + "' on the screen", true);
				return false;
			}
		}

		/// <summary>Selects Index from a parameterized dropdown element</summary>
		/// <param name="ele">Web Element</param>
		/// <param name="index">Index</param>
		/// <param name="elementName">Element Name for Test Reporting</param>
		/// <returns>Boolean Result</returns>
		public bool SelectDropdownByIndex(SelectElement ele, int index, string elementName = "")
		{
            try
            {
                ele.SelectByIndex(index);
				if (!string.IsNullOrWhiteSpace(elementName))
					Report(Status.Pass, "The index value '" + index + "' is selected for '" + elementName + "' successfully", false);
				return true;
			}
            catch (Exception)
            {
				if (!string.IsNullOrWhiteSpace(elementName))
					Report(Status.Fail, "The index value '" + index + "' is not selected for '" + elementName + "'", true);
				return false;
			}
		}

		/// <summary>Selects Index from a dropdown element found using the parameterized locator</summary>
		/// <param name="by">By Locator</param>
		/// <param name="index">Index</param>
		/// <param name="elementName">Element Name for Test Reporting</param>
		/// <returns>Boolean Result</returns>
		public bool SelectDropdownByIndex(By by, int index, string elementName = "")
		{
			var ele = GetDropdown(by);
			if(ele != null)
				return SelectDropdownByIndex(ele, index, elementName);
			else
			{
				if (!string.IsNullOrWhiteSpace(elementName))
					Report(Status.Fail, "Unable to find the element '" + elementName + "' on the screen", true);
				return false;
			}
		}

		/// <summary>Selects Value from a parameterized dropdown element</summary>
		/// <param name="ele">Web Element</param>
		/// <param name="value">Value</param>
		/// <param name="elementName">Element Name for Test Reporting</param>
		/// <returns>Boolean Result</returns>
		public bool SelectDropdownByValue(SelectElement ele, string value, string elementName = "")
        {
            try
            {
                ele.SelectByValue(value);
				if (!string.IsNullOrWhiteSpace(elementName))
					Report(Status.Pass, "The option '" + value + "' is selected for '" + elementName + "' successfully", false);
				return true;
			}
            catch (Exception)
            {
				if (!string.IsNullOrWhiteSpace(elementName))
					Report(Status.Fail, "The option '" + value + "' is not selected for '" + elementName + "'", true);
				return false;
			}
		}

		/// <summary>Selects Value from a dropdown element found using the parameterized locator</summary>
		/// <param name="by">By Locator</param>
		/// <param name="value">Value</param>
		/// <param name="elementName">Element Name for Test Reporting</param>
		/// <returns>Boolean Result</returns>
		public bool SelectDropdownByValue(By by, string value, string elementName = "")
        {
			var ele = GetDropdown(by);
			if(ele != null)
				return SelectDropdownByValue(ele, value, elementName);
            else
            {
				if (!string.IsNullOrWhiteSpace(elementName))
					Report(Status.Fail, "Unable to find the element '" + elementName + "' on the screen", true);
				return false;
			}
		}

		/// <summary>Verifies if the text is empty in the WebElement, will still continue with the test case execution</summary>
		/// <param name="ele">Web Element</param>
		/// <param name="strMessage">Message to be published in the report</param>
		public void VerifyIfTextIsNotEmpty(IWebElement ele, string strMessage)
        {
			if (!string.IsNullOrWhiteSpace(ele.Text.Trim()))
				Report(Status.Pass, "PASSED : " + strMessage, false);
			else
				Report(Status.Error, "ERROR : " + strMessage, true);
		}

		/// <summary>Verifies if the text is empty in the WebElement found using the locator, will still continue with the test case execution</summary>
		/// <param name="by">By Locator</param>
		/// <param name="strMessage">Message to be published in the report</param>
		public void VerifyIfTextIsNotEmpty(By by, string strMessage)
        {
            if (GetElement(by) == null)
				Report(Status.Error, "ERROR : " + strMessage + " (Element not available)", true);
			else if (!string.IsNullOrWhiteSpace(GetElement(by).Text.Trim()))
				Report(Status.Pass, "PASSED : " + strMessage, false);
			else
				Report(Status.Error, "ERROR : " + strMessage, true);
		}

		/// <summary>Asserts if the text is empty in the WebElement, will stop the current test case execution</summary>
		/// <param name="ele">Web Element</param>
		/// <param name="strMessage">Message to be published in the report</param>
		public void AssertIfTextIsNotEmpty(IWebElement ele, string strMessage)
        {
			if (!string.IsNullOrWhiteSpace(ele.Text.Trim()))
				Report(Status.Pass, "PASSED : " + strMessage, false);
			else
				Report(Status.Fail, "FAILED : " + strMessage, true);
		}

		/// <summary>Asserts if the text is empty in the WebElement found using the locator, will stop the current test case execution</summary>
		/// <param name="by">By Locator</param>
		/// <param name="strMessage">Message to be published in the report</param>
		public void AssertIfTextIsNotEmpty(By by, string strMessage)
        {
            if (GetElement(by) == null)
				Report(Status.Fail, "FAILED : " + strMessage + " (Element not available)", true);
			else if (!string.IsNullOrWhiteSpace(GetElement(by).Text.Trim()))
				Report(Status.Pass, "PASSED : " + strMessage, false);
			else
				Report(Status.Fail, "FAILED : " + strMessage, true);
		}

		/// <summary>Verifies if the text of the attribute of the WebElement matches with the expected text, will still continue with the test case execution</summary>
		/// <param name="ele">Web Element</param>
		/// <param name="strExpectedText">Expected Text</param>
		/// <param name="attribute">Attribute</param>
		/// <returns>Boolean Result</returns>
		public bool VerifyElementAttribute(IWebElement ele, string strExpectedText, string attribute)
		{
			var strActualText = ele.GetAttribute(attribute);
			var msg = string.Format("The expected attribute ({0} : {1})", attribute, strExpectedText);

			if (strActualText.Equals(strExpectedText, StringComparison.OrdinalIgnoreCase))
			{
				Report(Status.Pass, "PASSED : " + msg + " is displayed on the screen", false);
				return true;
			}
			else
			{
				Report(Status.Error, "ERROR : " + msg + " is not displayed on the screen", true);
				return false;
			}
		}

		/// <summary>Verifies if the text of the attribute of the WebElement (found using the locator) matches with the expected text, will still continue with the test case execution</summary>
		/// <param name="by">By Locator</param>
		/// <param name="strExpectedText">Expected Text</param>
		/// <param name="attribute">Attribute</param>
		/// <returns>Boolean Result</returns>
		public bool VerifyElementAttribute(By by, string strExpectedText, string attribute)
		{
			var msg = string.Format("The expected attribute ({0} : {1})", attribute, strExpectedText);

			if (GetElement(by) == null)
			{
				Report(Status.Error, "ERROR : " + msg + " is not displayed (Element not available)", true);
			}

			var strActualText = GetElement(by).GetAttribute(attribute);

			if (strActualText.Equals(strExpectedText.Trim(), StringComparison.OrdinalIgnoreCase))
			{
				Report(Status.Pass, "PASSED : " + msg + " is displayed on the screen", false);
				return true;
			}
			else
			{
				Report(Status.Error, "ERROR : " + msg + " is not displayed on the screen", true);
				return false;
			}
		}

		/// <summary>Asserts if the text of the attribute of the WebElement matches with the expected text, will stop the current test case execution</summary>
		/// <param name="ele">Web Element</param>
		/// <param name="strExpectedText">Expected Text</param>
		/// <param name="attribute">Attribute</param>
		/// <returns>Boolean Result</returns>
		public bool AssertElementAttribute(IWebElement ele, string strExpectedText, string attribute)
		{
			var strActualText = ele.GetAttribute(attribute);
			var msg = string.Format("The expected attribute ({0} : {1})", attribute, strExpectedText);

			if (strActualText.Equals(strExpectedText, StringComparison.OrdinalIgnoreCase))
				Report(Status.Pass, "PASSED : " + msg + " is displayed on the screen", false);
			else
				Report(Status.Fail, "FAILED : " + msg + " is not displayed on the screen", true);

			return true;
		}

		/// <summary>Asserts if the text of the attribute of the WebElement (found using the locator) matches with the expected text, will stop the current test case execution</summary>
		/// <param name="by">By Locator</param>
		/// <param name="strExpectedText">Expected Text</param>
		/// <param name="attribute">Attribute</param>
		/// <returns>Boolean Result</returns>
		public bool AssertElementAttribute(By by, string strExpectedText, string attribute)
		{
			var msg = string.Format("The expected attribute ({0} : {1})", attribute, strExpectedText);

			if (GetElement(by) == null)
				Report(Status.Fail, "FAILED : " + msg + " is not displayed (Element not available)", true);

			var strActualText = GetElement(by).GetAttribute(attribute);
			
			if (strActualText.Equals(strExpectedText.Trim(), StringComparison.OrdinalIgnoreCase))
				Report(Status.Pass, "PASSED : " + msg + " is displayed on the screen", false);
			else
				Report(Status.Fail, "FAILED : " + msg + " is not displayed on the screen", true);

			return true;
		}

		/// <summary>Verifies the text in the WebElement against the parameterized text, will still continue with the test case execution</summary>
		/// <param name="ele">Web Element</param>
		/// <param name="strExpectedText">Expected Text</param>
		public void VerifyElementText(IWebElement ele, string strExpectedText)
        {
            var strActualText = ele.Text;

            if (strActualText.Equals(strExpectedText, StringComparison.OrdinalIgnoreCase))
				Report(Status.Pass, "PASSED : The expected text '" + strExpectedText + "' is displayed on the screen", false);
			else
				Report(Status.Error, "ERROR : The expected text '" + strExpectedText + "' is not displayed on the screen", true);
		}

		/// <summary>Verifies the text in the WebElement (found using the locator) against the parameterized text, will still continue with the test case execution</summary>
		/// <param name="by">By Locator</param>
		/// <param name="strExpectedText">Expected Text</param>
		public void VerifyElementText(By by, string strExpectedText)
        {
            if (GetElement(by) == null)
            {
                Report(Status.Error, "ERROR : The expected text '" + strExpectedText + "' is not displayed (Element not available)", true);
                return;
            }

            var strActualText = GetElement(by).Text;

            if (strActualText.Equals(strExpectedText, StringComparison.OrdinalIgnoreCase))
				Report(Status.Pass, "PASSED : The expected text '" + strExpectedText + "' is displayed on the screen", false);
			else
				Report(Status.Error, "ERROR : The expected text '" + strExpectedText + "' is not displayed on the screen", true);
		}

		/// <summary>Asserts the text in the WebElement against the parameterized text, will stop the current test case execution</summary>
		/// <param name="ele">Web Element</param>
		/// <param name="strExpectedText">Expected Text</param>
		public void AssertElementText(IWebElement ele, string strExpectedText)
        {
            var strActualText = ele.Text;

            if (strActualText.Equals(strExpectedText, StringComparison.OrdinalIgnoreCase))
				Report(Status.Pass, "PASSED : The expected text '" + strExpectedText + "' is displayed on the screen", false);
			else
				Report(Status.Fail, "FAIL : The expected text '" + strExpectedText + "' is not displayed on the screen", true);
		}

		/// <summary>Assert the text in the WebElement (found using the locator) against the parameterized text, will stop the current test case execution</summary>
		/// <param name="by">By Locator</param>
		/// <param name="strExpectedText">Expected Text</param>
		public void AssertElementText(By by, string strExpectedText)
        {
            if (GetElement(by) == null)
            {
                Report(Status.Fail, "FAILED : The expected text '" + strExpectedText + "' is not displayed (Element not available)", true);
                return;
            }

            var strActualText = GetElement(by).Text;

            if (strActualText.Equals(strExpectedText, StringComparison.OrdinalIgnoreCase))
				Report(Status.Pass, "PASSED : The expected text '" + strExpectedText + "' is displayed on the screen", false);
			else
				Report(Status.Fail, "FAILED : The expected text '" + strExpectedText + "' is not displayed on the screen", true);
		}

		/// <summary>Verifies if the WebElement exists, will still continue with the test case execution</summary>
		/// <param name="by">By Locator</param>
		/// <param name="strMessage">Message for Test Reporting</param>
		public void VerifyPresence(By by, string strMessage)
        {
            if (GetElement(by) != null)
				Report(Status.Pass, "PASSED : " + strMessage, false);
			else
				Report(Status.Error, "ERROR : " + strMessage, true);
		}

		/// <summary>Asserts if the WebElement exists, will stop the current test case execution</summary>
		/// <param name="by">By Locator</param>
		/// <param name="strMessage">Message for Test Reporting</param>
		public void AssertPresence(By by, string strMessage)
        {
            if (GetElement(by) != null)
				Report(Status.Pass, "PASSED : " + strMessage, false);
			else
				Report(Status.Fail, "FAILED : " + strMessage, true);
		}

		/// <summary>Explicitly waits until WebElement presence is located</summary>
		/// <param name="by">By Locator</param>
		/// <returns>Web Element</returns>
		public IWebElement WaitUntilElementPresent(By by)
		{
			try
			{
				return new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(by));
			}
			catch (Exception) { return null; }
		}

		/// <summary>Explicitly waits until WebElement goes invisible</summary>
		/// <param name="by">By Locator</param>
		/// <returns>Boolean Result</returns>
		public bool WaitUntilElementInvisible(By by)
		{
			try
			{
				return new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(SeleniumExtras.WaitHelpers.ExpectedConditions.InvisibilityOfElementLocated(by));
			}
			catch (Exception) { return false; }
		}

		/// <summary>Explicitly waits until WebElement becomes visible</summary>
		/// <param name="by">By Locator</param>
		/// <returns>Web Element</returns>
		public IWebElement WaitUntilElementVisible(By by)
		{
			try
			{
				return new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(by));
			}
			catch (Exception) { return null; }
		}

		/// <summary>Explicitly waits until WebElement is clickable</summary>
		/// <param name="by">By Locator</param>
		/// <returns>Web Element</returns>
		public IWebElement WaitUntilElementClickable(By by)
		{
			try
			{
				return new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(by));
			}
			catch (Exception) { return null; }
		}

		/// <summary>Explicitly waits until WebElement is clickable</summary>
		/// <param name="ele">Web Element</param>
		/// <returns>Web Element</returns>
		public IWebElement WaitUntilElementClickable(IWebElement ele)
		{
			try
			{
				return new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(ele));
			}
			catch (Exception) { return null; }
		}

		/// <summary>Explicitly waits until the browser contains the parameterized URL</summary>
		/// <param name="url">URL</param>
		/// <returns>Boolean Result</returns>
		public bool WaitUntilUrlContains(string url)
		{
			try
			{
				return new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(SeleniumExtras.WaitHelpers.ExpectedConditions.UrlContains(url));
			}
			catch (Exception) { return false; }
		}

		/// <summary>Explicitly waits until the page loads in the browser</summary>
		/// <returns>Boolean Result</returns>
		public bool WaitUntilPageLoads()
		{
			try
			{
				return new WebDriverWait(driver, TimeSpan.FromSeconds(40)).Until(
					d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
			}
			catch (Exception) { return false; }
		}

		/// <summary>Scrolls to the top of the page</summary>
		public void ScrollToTop() => js.ExecuteScript("window.scrollTo(0, 0);");

		/// <summary>Scrolls to the bottom of the page</summary>
		public void ScrollToBottom() => js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");

		/// <summary>Scrolls to the parameterized coordinates of the page</summary>
		/// <param name="x">X-Coordinate</param>
		/// <param name="y">Y-Coordinate</param>
		public void ScrollToCoordinates(int x, int y) => js.ExecuteScript("window.scrollTo(arguments[0], arguments[1]);", x, y);

		/// <summary>Scrolls to the WebElement on the page</summary>
		/// <param name="ele">Web Element</param>
		public void ScrollInToView(IWebElement ele) => js.ExecuteScript("arguments[0].scrollIntoView(true);", ele);

		/// <summary>Scrolls to the WebElement on the page found using the locator</summary>
		/// <param name="by">By Locator</param>
		public void ScrollInToView(By by) => js.ExecuteScript("arguments[0].scrollIntoView(true);", GetElement(by));

		/// <summary>]Generates random text</summary>
		/// <returns>Randomly generated string</returns>
		public string GetRandomText()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[8];
            var random = new Random();

            for (var i = 0; i < stringChars.Length; i++)
                stringChars[i] = chars[random.Next(chars.Length)];

            return new string(stringChars);
        }

		/// <summary>Generates random number</summary>
		/// <param name="limit">Maximum Number Limit</param>
		/// <returns>Random number between 1 and the Maximim Limit</returns>
		public int GetRandomNumber(int limit) => new Random().Next(limit) + 1;

		/// <summary>Double Clicks the WebElement found using the locator</summary>
		/// <param name="by">By Locator</param>
		/// <returns>Boolean Result</returns>
		public bool DoubleClick(By by)
        {
            try
            {
                builder = new Actions(driver);
                builder.DoubleClick(GetElement(by)).Build().Perform();
                return true;
            }
            catch (Exception) { return false; }
        }

		/// <summary>Click and drag a WebElement onto another WebElement</summary>
		/// <param name="source">Source WebElement</param>
		/// <param name="target">Target WebElement</param>
		/// <returns>Boolean Result</returns>
		public bool ClickAndDrag(IWebElement source, IWebElement target)
        {
            try
            {
                builder = new Actions(driver);
                builder.DragAndDrop(source, target).Build().Perform();
                return true;
            }
            catch (Exception) { return false; }
        }

		/// <summary>Click and drag a WebElement onto another using offset coordinates</summary>
		/// <param name="source">Source WebElement</param>
		/// <param name="xOffset">X-Coordinate</param>
		/// <param name="yOffset">Y-Coordinate</param>
		/// <returns>Boolean Result</returns>
		public bool ClickAndDrag(IWebElement source, int xOffset, int yOffset)
        {
		    try
            {
                builder = new Actions(driver);
                builder.DragAndDropToOffset(source, xOffset, yOffset).Build().Perform();
                return true;
			}
			catch (Exception) { return false; }
		}

		/// <summary>Move the mouse to the parameterized WebElement</summary>
		/// <param name="ele">WebElement</param>
		/// <returns>Boolean Result</returns>
		public bool MouseHover(IWebElement ele)
		{
			try
			{
				builder = new Actions(driver);
				builder.MoveToElement(ele).Build().Perform();
				return true;
			}
			catch (Exception) { return false; }
		}

		/// <summary>Move the mouse to the specified offset of the top-left corner of the parameterized WebElement</summary>
		/// <param name="ele">WebElement</param>
		/// <param name="xOffset">X-Coordinate</param>
		/// <param name="yOffset">Y-Coordinate</param>
		/// <returns>Boolean Result</returns>
		public bool MouseHover(IWebElement ele, int xOffset, int yOffset)
		{
			try
			{
				builder = new Actions(driver);
				builder.MoveToElement(ele, xOffset, yOffset).Build().Perform();
				return true;
			}
			catch (Exception) { return false; }
		}

		/// <summary>Navigates to a Menu Item</summary>
		/// <param name="mainMenu">Main Menu Locator</param>
		/// <param name="menuItem">Menu Item Locator List</param>
		/// <returns>Boolean Result</returns>
		public bool GoToMenu(By mainMenu,By[] menuItem)
        {
            var actionPerformed = false;
            try
            {
                var action = new Actions(driver);
                var Mainmenu = GetElement(mainMenu);
                if (menuItem.Length > 0)
                {
                    action.MoveToElement(Mainmenu).Build().Perform();
                    for (var i = 0; i < menuItem.Length; i++)
                    {
                        var css = menuItem[i];
                        if (i == menuItem.Length - 1)
						{
							action.MoveToElement(new WebDriverWait(driver, TimeSpan.FromSeconds(2))
                                    .Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(css))).Click().Build().Perform();
						}
						else
						{
							action.MoveToElement(new WebDriverWait(driver, TimeSpan.FromSeconds(2))
                                    .Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(css))).Build().Perform();
						}
					}
                    actionPerformed = true;
                }
                else
                {
                    action.MoveToElement(Mainmenu).Build().Perform();
                    actionPerformed = true;
                }
            }
            catch (Exception) { actionPerformed = false; }

            // If actionPerformed is true then the navigation was successful, else the navigation failed
            return actionPerformed;
        }

		/// <summary>Clicks TAB key on the screen</summary>
		/// <param name="by">By Locator</param>
		public void ClickTab(By by)
		{
			if (GetElement(by) != null)
				driver.FindElement(by).SendKeys(Keys.Tab);
			else
				Report(Status.Error, "Unable to click TAB on the element", true);
		}

		/// <summary>Clears the text from the WebElement found using the locator</summary>
		/// <param name="by">By Locator</param>
		/// <param name="fieldName">Field Name for Test Reporting</param>
		public void Clear(By by, string fieldName)
		{
			if (GetElement(by) != null)
			{
				driver.FindElement(by).Clear();
				Report(Status.Info, fieldName + " field is cleared successfully", false);
			}
			else
				Report(Status.Error, "Unable to clear text from the " + fieldName + " field", true);
		}

		/// <summary>Keys in a list of Input Data fields with input data and logs the report; where data field, input data and report statement are passed as parameters</summary>
		/// <param name="inputData">Input Data</param>
		public void KeyinAndValidateData(List<FieldData> inputData)
		{
			foreach (var fieldData in inputData)
			{
				if (Type(fieldData.FieldId, fieldData.FieldValue))
					Report(Status.Pass, fieldData.SuccessMessage, true);
				else
					Report(Status.Fail, fieldData.ErrorMessage, true);
			}
		}
	}

	/// <summary>
	/// To key in values into web elements by making use of the following data fields:
	/// <list type="bullet">
	/// <item>By Locator</item>
	/// <item>Input Value</item>
	/// <item>Success Message</item>
	/// <item>Error Message</item>
	/// </list>
	/// </summary>
	public class FieldData
	{
		/// <summary>By Locator of the Web Element</summary>
		public By FieldId { get; set; }

		/// <summary>Input value</summary>
		public string FieldValue { get; set; }

		/// <summary>Success Message to be reported</summary>
		public string SuccessMessage { get; set; }

		/// <summary>Error Message to be reported</summary>
		public string ErrorMessage { get; set; }
	}
}