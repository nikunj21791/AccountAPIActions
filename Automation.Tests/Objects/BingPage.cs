using Automation.Base.Utils;
using OpenQA.Selenium;
using AventStack.ExtentReports;

namespace Automation.Tests.Objects
{
	class BingPage : CommonUtilities
	{
		public static By searchBox = By.Id("sb_form_q");
		public static By searchButton = By.XPath("//label[@class='search icon tooltip']");
		public static By resultsCount = By.Id("b_results");
		public static By englishLanguage = By.XPath("//a[text()='English']");

		public void EnterSearchText(string searchText)
		{
			//Verify if search box exists
			VerifyPresence(searchBox, "Displaying Bing search box");
			if(GetElement(englishLanguage) != null)
				Click(englishLanguage);
		
			//Verify if the text was entered
			if(Type(searchBox, searchText))
				Report(Status.Pass, "The search text is entered into the Bing search box", false);
			else
				Report(Status.Fail, "Unable to enter the search text into the Bing search box", true);

			WaitUntilElementPresent(searchButton);
		}

		//Verify if search button was clicked
		public void PerformSearch()
		{
			if(Click(searchButton))
				Report(Status.Pass, "The Bing search button is clicked", false);
			else
				Report(Status.Fail, "The Bing search button is not clicked", true);
		}

		//Verify if search results are displayed
		public void VerifySearchResults() => VerifyPresence(resultsCount, "Displaying Bing search results on the screen");
	}
}
