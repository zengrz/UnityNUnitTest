using System;
namespace MonoDevelop.NUnit
{
	public interface ITestProgressMonitor
	{
		event TestHandler CancelRequested;
		bool IsCancelRequested
		{
			get;
		}
		void BeginTest(UnitTest test);
		void EndTest(UnitTest test, UnitTestResult result);
		void ReportRuntimeError(string message, System.Exception exception);
	}
}
