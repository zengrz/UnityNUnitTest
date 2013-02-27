using System;
namespace MonoDevelop.NUnit.External
{
	internal interface IRemoteEventListener
	{
		void TestStarted(string testCase);
		void TestFinished(string test, UnitTestResult result);
		void SuiteStarted(string suite);
		void SuiteFinished(string suite, UnitTestResult result);
	}
}
