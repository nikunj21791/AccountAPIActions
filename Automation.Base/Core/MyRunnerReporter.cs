using Xunit;
using Xunit.Abstractions;

namespace CommonSpirit.Automation.Base.Core
{
	/// <summary>Implements the IRunnerReporter interface to continuously keep a look on each step of the test suite execution</summary>
	public class MyRunnerReporter : IRunnerReporter
	{
		/// <summary>Gets the description of the reporter.</summary>
		public string Description => "My custom runner reporter";

		/// <summary>Returns a 'true' value which indicates that the reporter should be environmentally enabled.</summary>
		public bool IsEnvironmentallyEnabled => true;

		/// <summary>Gets a value which indicates a runner switch which can be used to explicitly enable the runner.</summary>
		public string RunnerSwitch => "mycustomrunnerreporter";

		/// <summary>Creates a message handler (returns an object of MyMessageSink class) that reports messages for the given test assembly.</summary>
		/// <param name="logger">The logger used to send result messages to</param>
		/// <returns>An object of MyMessageSink class</returns>
		public IMessageSink CreateMessageHandler(IRunnerLogger logger) => new MyMessageSink();
	}
}