# Automation.Base
The project Automation.Base comprises of processes performing all the pre-requisites and post-requisites that need to be perfomed on both the suite execution level and the test execution level.

# Components
---

### OneTimeSetup

The OneTimeSetup class takes care of all the configurations that need to be done on a Suite Execution Level. There are 2 methods being used:

- Constructor Method
- Dispose Method

The Constructor method invokes and sets up the various reporting functionalities, data sheet configuratins and loads the user properties file. This is performed once, at the start of the entire Test Suite Execution.

The Dispose method acts as a flush closing the HTML Extent reporting. This is performed once, at the end of the entire Test Suite Execution.

### TestInit

The TestInit class takes care of all the configurations that need to be done on a Test Execution Level, i.e., this will performed all the pre-requisites and post-requistes for every test method. There are 2 methods being used:

- Constructor Method
- Dispose Method

The Constructor method sets up the input data coming in from the Excel according to the test case currently being executed and the current data iteration and starts the HTML extent report.

The Dispose method clears the test data which was setup in Before method.

### RepeatAttribute

The RepeatAttribute class sets up the number of times a test case needs to be executed, which in turn depends on the no. of data iterations mentioned in the input file.

In order to make use of this feature, every method marked with **[Theory] attribute** needs to be accompanied with a **[Repeat] attribute** as well so that while builing the solution, the test case get updated with the no. of iterations that the test explorer will be running.

### MyRunnerReporter

The MyRunnerReporter class extends the IRunerReporter interface and sets up a runner reporter that runs throughout the test execution to keep observing and listening how the tests behave and execute. This class works in hand with the MyMessageSink class for reporting as explained in the next section.

### MyMessageSink

The MyMessageSink class extends the IMessageSink interface and the sole purpose of this class is to report the result of each test case and report it, i.e., whether the test case passed or failed.

### CommonUtilities

This class houses all the common utility methods such as the Report(), GetResponseFromAPI() and so on.

The report function is called whenever a checkpoint is reached or the result (i.e) PASS, FAIL, ERROR, WARNING or INFO is to be logged. This writes into the HTML extent report.

The GetResponseFromAPI function is called to perform API Test to get the response from different endpoints like POST, GET, PUT, DELET, PATCH with all the required parameters passed to this method.

### IOUtil

This is the main utilities class for the input data functionalities. It has methods to read data from excel spreadsheets.

The GetInputData accepts the String which is the name of the test method and an Integer which indicates the current iteration of the test case as parameters. Returns the Data Dictionary of data read from the Input excel file (the corresponding row whose TC_Name is the same as the test method name and iteration number is same as the current iteration).

The GetIterationCountExcel accepts a string which is the name of the test method as a parameter. Returns the no. of data iterations mentioned for the the corresponding row whose TC_Name is the same as the test method name in the Input excel file.


# Automation.Tests
The project Automation.Base comprises of processes performing all the pre-requisites and post-requisites that need to be perfomed on both the suite execution level and the test execution level.

# Components
---

### Tests

The Tets folder consists of all the Test Classes that we would like to run from our test suite. Currently, we have created a DealingWithAccounts.cs file that comprises of 4 tests:

- CreateAccount
- DepositIntoAccount
- WithdrawFromAccount
- DeleteAccount

These tests will help us in creating and deleting accounts, as well as performing transactions such as Deposit and Withdraw from the account.

### Resources

This folder consists of the input data sheet that we will be using during our execution. Each line corresponds to a test case method, with different combinations of inputs listed out to perform differnt tests on the same test case.

### Configuration

This folder comprises of just a single file CollectionDefinition.cs with the only intent of supporting the test suite framework so as to bring all the test class files under a single suite governed by OneTimeSetup file mentioned in Automation.Base project.