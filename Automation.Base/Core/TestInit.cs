using CommonSpirit.Automation.Base.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;

namespace CommonSpirit.Automation.Base.Core
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
			var desc = string.Format("{0}", data.GetValueOrDefault("Description", ""));
			test = extent.CreateTest(name, desc);
			if (currentIteration != iterationCount)
				currentTestIteration[testCaseName] = currentIteration + 1;
		}

        /// <summary>Performs all the tear down operation, since Reporting is done at the end of every test method</summary>
        public void Dispose() => data.Clear();
    }

	/// <summary>To decide the no. of iterations of execution for each test case, build the solution before each run</summary>
	public class RepeatAttribute : Xunit.Sdk.DataAttribute
    {
		/// <summary>Gets the data sheet from Excel input file</summary>
		public RepeatAttribute()
		{
			CommonUtilities.GetProperties("..\\..\\..\\user_config.properties");
            IOUtil.iterationDataPath = "..\\..\\..\\Resources\\" + CommonUtilities.props["datasheet"];
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