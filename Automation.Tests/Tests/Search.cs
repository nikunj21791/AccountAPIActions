using AventStack.ExtentReports;
using Automation.Base.Core;
using Automation.Tests.Objects;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using Xunit.Priority;

namespace Automation.Tests.Tests
{
	[Collection("XUnit Collection")]
	[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
	[DefaultPriority(0)]
	public class Search : TestInit
	{
		public Search(ITestOutputHelper output) : base(output) { }

		[Theory, Priority(1)]
		[Repeat]
		public void GoogleSearch(int iterationNumber)
		{
			//Arrange
			Report(Status.Info, "Iteration Number: " + iterationNumber, false);
			Open(data.GetValueOrDefault("URL"));
			var obj = new GooglePage();

			//Act
			obj.EnterSearchText(data.GetValueOrDefault("SearchString"));
			obj.PerformSearch();

			//Assert
			obj.VerifySearchResults();
		}

		[Theory, Priority(2)]
		[Repeat]
		public void BingSearch(int iterationNumber)
		{
			//Arrange
			Report(Status.Info, "Iteration Number: " + iterationNumber, false);
			Open(data.GetValueOrDefault("URL"));
			var obj = new BingPage();

			//Act
			obj.EnterSearchText(data.GetValueOrDefault("SearchString"));
			obj.PerformSearch();

			//Assert
			obj.VerifySearchResults();
		}
	}
}