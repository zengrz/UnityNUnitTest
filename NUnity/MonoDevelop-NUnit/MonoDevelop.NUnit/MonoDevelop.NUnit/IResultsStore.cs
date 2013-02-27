using System;
namespace MonoDevelop.NUnit
{
	public interface IResultsStore
	{
		void RegisterResult(string configuration, UnitTest test, UnitTestResult result);
		UnitTestResult GetLastResult(string configuration, UnitTest test, System.DateTime date);
		UnitTestResult GetNextResult(string configuration, UnitTest test, System.DateTime date);
		UnitTestResult GetPreviousResult(string configuration, UnitTest test, System.DateTime date);
		UnitTestResult[] GetResults(string configuration, UnitTest test, System.DateTime startDate, System.DateTime endDate);
		UnitTestResult[] GetResultsToDate(string configuration, UnitTest test, System.DateTime endDate, int count);
		void Save();
	}
}
