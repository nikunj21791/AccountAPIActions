using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using NPOI.SS.UserModel;
using RestSharp;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace CommonSpirit.Automation.Base.Utils
{
    /// <summary>
    /// To make use of the commonly used functionalities while performing automated testing like
    /// reporting each test step result, getting Responses for Request API calls and so on.
    /// </summary>
    public class CommonUtilities : Xunit.Sdk.LongLivedMarshalByRefObject
	{
        /// <summary>Equal To</summary>
        public const char EqualToChar = '=';
        /// <summary>Hash</summary>
        public const string Hash = "#";
        /// <summary>Slash</summary>
        public const string Slash = "/";

        /// <summary>Current Test Case Input Data Dictionary</summary>
		public static readonly Dictionary<string, string> accounts = new Dictionary<string, string>();
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
		/// <summary>Project Base Path</summary>
		public static string projectPath;
		/// <summary>Extent Report Path</summary>
		public static string reportPath;
		/// <summary>Report Folder Path</summary>
		public static string reportFolderPath;
		/// <summary>Report Folder Name</summary>
		public static string reportFolderName;
		/// <summary>Download Directory</summary>
		public static string downloadDirectory;
		/// <summary>Current test case name</summary>
		public static string testCaseName;
		
		/// <summary>Gets the properties data from the user_config properties file into the props data field</summary>
		/// <param name="path">The file to open for reading</param>
		public static void GetProperties(string path)
		{
			props.Clear();
			foreach (var row in File.ReadAllLines(path))
				if (!(row.StartsWith(Hash) || row.Length == 0))
					props.Add(row.Split(EqualToChar)[0].Trim(), row.Split(EqualToChar)[1].Trim());
		}

		/// <summary>Reports the test step to be published in the Test Suite Execution Report</summary>
		/// <param name="status">Test Step Result</param>
		/// <param name="description">Test Step Description</param>
		public static void Report(Status status, string description)
		{
			test.Log(status, description, null);
			if(status == Status.Fail)
				Assert.Fail(description);
		}

		/// <summary>Gets the response of the Get API Request according to the inputs passed as parameters</summary>
		/// <param name="basicURI">Basic URI</param>
		/// <param name="method">Method</param>
		/// <param name="pathParams">Path Parameters</param>
		/// <param name="queryParams">Query Parameters</param>
		/// <param name="payload">Payload Body</param>
		/// <param name="token">Authorization Token</param>
		/// <param name="headers">Request Headers</param>
		/// <param name="pathParamSeparator">Path Parameters Separator</param>
		/// <returns>Response as an RestResponse object of the API Request passed</returns>
		public static RestResponse GetResponseFromAPI(string basicURI, Method method, string[] pathParams, Dictionary<string, string> queryParams = null, string payload = "", string token = "", Dictionary<string, string> headers = null, string pathParamSeparator = Slash)
		{
			var restClient = new RestClient(basicURI);
			RestRequest restRequest;

            if (pathParams != null)
			{
				var param = Slash + string.Join(pathParamSeparator, pathParams);
				restRequest = new RestRequest(param, method);
			}
			else
				restRequest = new RestRequest("", method);

			if(method == Method.Post || method == Method.Put || method == Method.Patch)
				if (!string.IsNullOrWhiteSpace(payload))
					restRequest.AddParameter("application/json", payload, ParameterType.RequestBody);

            if (queryParams != null)
				foreach (var queryparam in queryParams)
					restRequest.AddQueryParameter(queryparam.Key, queryparam.Value);

			if (!string.IsNullOrWhiteSpace(token))
				restRequest.AddHeader("Authorization", token);

			if (headers != null)
				foreach (var header in headers)
					restRequest.AddHeader(header.Key, header.Value);

			return restClient.Execute(restRequest);
		}
	}
}