using AventStack.ExtentReports;
using CommonSpirit.Automation.Base.Utils;
using Xunit.Abstractions;

namespace CommonSpirit.Automation.Base.Core
{
	/// <summary>
	/// Implements the IMessageSink interface to report the results of the test assembly, i.e., whether the test case
	/// passed or failed with corresponding messages at the end of every test case executed in the test suite
	/// </summary>
	public class MyMessageSink : CommonUtilities, IMessageSink
	{
		/// <summary>Determines whether the final report of test case has been published</summary>
		public static bool reportStatus = false;
		/// <summary>Screenshot in case of test case failure, if required</summary>
		public static string screenShot = null;
		/// <summary>Determines whether to use screenshot while reporting the final report of each test case</summary>
		public static bool useScreenShot = false;

		/// <summary>Reports the presence of a message on the message bus</summary>
		/// <param name="message">Message from the message bus</param>
		/// <returns>Return true to continue running the tests</returns>
		public bool OnMessage(IMessageSinkMessage message)
		{
			if (message is ITestClassConstructionStarting)
			{
				reportStatus = true;
				screenShot = null;
			}
			else if (message is ITestPassed)
			{
				Report(Status.Pass, "Test Case passed successfully");
				reportStatus = true;
				if(screenShot != null) { System.IO.File.Delete(reportFolderPath + "\\" + screenShot); }
			}
			else if (message is ITestFailed testFailed)
			{
				if (!(testFailed.ExceptionTypes[0].Contains("TrueException") && testFailed.StackTraces[0].Contains("CommonUtilities.Report")))
				{
					var msg = string.Format("Test Case failed: {0}. {1}", testFailed.ExceptionTypes[0], testFailed.StackTraces[0]);
					useScreenShot = true;
					Report(Status.Fail, msg);
				}
				else
				{
					if (screenShot != null) { System.IO.File.Delete(reportFolderPath + "\\" + screenShot); }
				}
				reportStatus = true;
			}

			return true;
		}
	}
}