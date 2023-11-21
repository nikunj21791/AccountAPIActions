using AventStack.ExtentReports.Reporter;
using CommonSpirit.Automation.Base.Utils;
using System;
using System.IO;

namespace CommonSpirit.Automation.Base.Core
{
	/// <summary>
	/// Provides methods to perform a one time setup process that runs once at the start of the whole Test Suite execution
	/// and once at the end of the whole Test Suite execution
	/// </summary>
	public class OneTimeSetup : CommonUtilities, IDisposable
    {
		/// <summary>
		/// Runs once at the start of the Test Suite execution, performing a one time process of setting up the following:
		/// <list type="number">
		/// <item>Fetching properties from user_config.properties file</item>
		/// <item>HTML report and screenshot file paths</item>
		/// <item>Database properties</item>
		/// <item>Input Excel sheet file</item>
		/// <item>Base URL according to the run environment</item>
		/// <item>No. of test iterations for each test case</item>
		/// <item>Enable / Disable the Captcha checkboxes in Sitecore based on "DisableSitecoreCaptchaValue" from the "user_config.properties" file</item>
		/// </list>
		/// </summary>
		public OneTimeSetup()
        {
			//Load properties
			GetProperties("..\\..\\..\\user_config.properties");

			//Set report and screenshots path
			var path = System.Reflection.Assembly.GetCallingAssembly().Location;
			var actualPath = path.Substring(0, path.LastIndexOf("bin"));
			projectPath = new Uri(actualPath).LocalPath;
			reportFolderName = "Report_" + DateTime.Now.ToString("yyyy_MM_dd_HHmmss");
			var reportName = reportFolderName + ".html";
			reportFolderPath = projectPath + "Reports\\" + reportFolderName;
			downloadDirectory = projectPath + "Reports\\Downloads";

			if (!Directory.Exists(reportFolderPath)) Directory.CreateDirectory(reportFolderPath);
			if (!Directory.Exists(downloadDirectory)) Directory.CreateDirectory(downloadDirectory);

			reportPath = reportFolderPath + "\\" + reportName;
            reporter = new ExtentV3HtmlReporter(reportPath);
            extent = new AventStack.ExtentReports.ExtentReports();
            reporter.LoadConfig(projectPath + "extentreports_config.xml");
            extent.AttachReporter(reporter);

			//Get the sheet from Excel input file
			dataSheet = IOUtil.GetDataSheet("..\\..\\..\\Resources\\Input.xlsx");
			if (dataSheet == null)
			{
				test = extent.CreateTest("Test Suite not Executed", "Either No DataSheet found or the DataSheet is already open");
				Report(AventStack.ExtentReports.Status.Error, "The specified Data Sheet in the user_config file '" + props["datasheet"] + "' either does not exist or is already open");
				extent.Flush();
				Environment.Exit(0);
			}

			//Set the Iteration List of the Test Cases and Base URL value
			IOUtil.SetIterationList(dataSheet);
			IOUtil.FetchCommonParameters(dataSheet);
		}

        /// <summary>Runs at the end of the Test Suite execution, where the extent/HTML report is flushed and closed</summary>
        public void Dispose() => extent.Flush();
    }
}