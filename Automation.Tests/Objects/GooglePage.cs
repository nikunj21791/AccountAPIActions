using Automation.Base.Utils;
using OpenQA.Selenium;
using AventStack.ExtentReports;

namespace Automation.Tests.Objects
{
	class GooglePage : CommonUtilities
	{
		public static By searchBox = By.XPath("//input[@class='gLFyf gsfi']");
		public static By searchButton = By.XPath("//input[@value='Google Search']");
		public static By resultsCount = By.Id("resultStats");

		public void EnterSearchText(string searchText)
		{
			//Verify if search box exists
			VerifyPresence(searchBox, "Displaying Google search box");
		
			//Verify if the text was entered
			if(Type(searchBox, searchText))
				Report(Status.Pass, "The search text is entered into the Google search box succesfully", false);
			else
				Report(Status.Fail, "Unable to enter the search text into the Google search box", true);

			WaitUntilElementPresent(searchButton);
		}

		public void PerformSearch()
		{
			//Verify if search button was clicked
			if(Click(searchButton))
				Report(Status.Pass, "The Google search button is clicked", false);
			else
				Report(Status.Fail, "The Google search button is not clicked", true);
		}

		//Verify if search results are displayed
		public void VerifySearchResults() => VerifyPresence(resultsCount, "Displaying Google search results on the screen");

	}
}
