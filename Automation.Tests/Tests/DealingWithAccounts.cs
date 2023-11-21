using AventStack.ExtentReports;
using CommonSpirit.Automation.Base.Core;
using RestSharp;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using Xunit.Priority;

namespace CommonSpirit.Automation.PCDE.Tests
{
	[Collection("XUnit Collection")]
	[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
	[DefaultPriority(0)]
	public class DealingWithAccounts : TestInit
	{
		public DealingWithAccounts(ITestOutputHelper output) : base(output) { }

		[Theory, Priority(1)]
		[Repeat]
		public void CreateAccount(int iterationNumber)
		{
			//Arrange
			Report(Status.Info, "Iteration Number: " + iterationNumber);
			var basicUri = "https://www.localhost:8080/api/account/create";
			var payload = "{\r\nInitialBalance: " + data["Amount"] + ",\r\nAccountName: “" + data["Name"] + "”,\r\nAddress: “" + data["Address"] + "”\r\n}";
			var headers = new Dictionary<string, string>
			{
                { "Content-Type", "application/json" }
            };

            //Act and Assert
            var response = GetResponseFromAPI(basicUri, Method.Post, null, null, payload, "", headers);
            if ((int)response.StatusCode == 200)
			{
				Report(Status.Pass, "Request sent to server is: " + response.ResponseUri);
				Report(Status.Pass, "Create API executed successfully and the response is " + response.StatusCode);
				
				var accountID = response.Content.Split("AccountNumber: “")[1].Split("”")[0];
                Report(Status.Pass, "Account ID: " + accountID);
				accounts.Add("Account " + iterationNumber, accountID);
                accounts.Add("Account " + iterationNumber + " Balance", data["Amount"]);

                if (response.Content.Contains("Account " + accountID + "created successfully"))
                    Report(Status.Pass, "Account is created successfully");
                else
                    Report(Status.Fail, "Account is not created");
            }
            else
				Report(Status.Fail, "Create API failed and the response is " + response.Content);
		}

		[Theory, Priority(2)]
		[Repeat]
		public void DepositIntoAccount(int iterationNumber)
		{
			//Arrange
			Report(Status.Info, "Iteration Number: " + iterationNumber);
			var basicUri = "https://www.localhost:8080/api/account/deposit";
			var payload = "{\r\nAccountNumber: “" + accounts[data["Account Number"]] + "”,\r\nAmount: " + data["Amount"] + "\r\n}";
            var headers = new Dictionary<string, string>
			{
                { "Content-Type", "application/json" }
            };
            var expectedAmount = int.Parse(accounts[data["Account Number"] + " Balance"]) + int.Parse(data["Amount"]);

            //Act and Assert
            var response = GetResponseFromAPI(basicUri, Method.Put, null, null, payload, "", headers);
			if ((int)response.StatusCode == 200)
			{
				Report(Status.Pass, "Request send to server is: " + response.ResponseUri);
				Report(Status.Pass, "Deposit API executed successfully and the response is " + response.StatusCode);

                var actualAmount = int.Parse(response.Content.Split("NewBalance: ")[1].Split("\r\n}")[0]);
                accounts[data["Account Number"] + " Balance"] = actualAmount.ToString();
                if (actualAmount == expectedAmount)
                    Report(Status.Pass, "Amount is deposited successfully");
                else
                    Report(Status.Fail, "Amount is not deposited");
            }
			else
				Report(Status.Fail, "Deposit API failed and the response is " + response.Content);
		}

		[Theory, Priority(3)]
		[Repeat]
		public void WithdrawFromAccount(int iterationNumber)
		{
			//Arrange
			Report(Status.Info, "Iteration Number: " + iterationNumber);
			var basicUri = "https://www.localhost:8080/api/account/withdraw";
            var payload = "{\r\nAccountNumber: “" + accounts[data["Account Number"]] + "”,\r\nAmount: " + data["Amount"] + "\r\n}";
			var headers = new Dictionary<string, string>
			{
                { "Content-Type", "application/json" }
            };
            var expectedAmount = int.Parse(accounts[data["Account Number"] + " Balance"]) - int.Parse(data["Amount"]);

            //Act and Assert
            var response = GetResponseFromAPI(basicUri, Method.Put, null, null, payload, "", headers);
			if(expectedAmount < 0)
				if ((int)response.StatusCode == 200)
                    Report(Status.Fail, "Amount is withdrawn even though the balance is not sufficient, hence test failed");

            if ((int)response.StatusCode == 200)
			{
				Report(Status.Pass, "Request send to server is: " + response.ResponseUri);
				Report(Status.Pass, "Withddraw API executed successfully and the response is " + response.StatusCode);

                var actualAmount = int.Parse(response.Content.Split("NewBalance: ")[1].Split("\r\n}")[0]);
                accounts[data["Account Number"] + " Balance"] = actualAmount.ToString();
                if (actualAmount == expectedAmount)
                    Report(Status.Pass, "Amount is withdrawn successfully");
                else
                    Report(Status.Fail, "Amount is not withdrawn");
            }
			else
				Report(Status.Fail, "Withddraw API failed and the response is " + response.Content);
		}

		[Theory, Priority(4)]
		[Repeat]
		public void DeleteAccount(int iterationNumber)
		{
			//Arrange
			Report(Status.Info, "Iteration Number: " + iterationNumber);
			var basicUri = "https://www.localhost:8080/api/account/delete/" + accounts[data["Account Number"]];
			var headers = new Dictionary<string, string>
			{
                { "Content-Type", "application/json" }
            };

            //Act and Assert
            var response = GetResponseFromAPI(basicUri, Method.Delete, null, null, "", "", headers);
			
			if ((int)response.StatusCode == 200)
			{
				Report(Status.Pass, "Request send to server is: " + response.ResponseUri);
				Report(Status.Pass, " Delete API executed successfully and the response is " + response.StatusCode);
			}
			else
				Report(Status.Fail, "Delete API failed and the response is " + response.Content);
		}
	}
}