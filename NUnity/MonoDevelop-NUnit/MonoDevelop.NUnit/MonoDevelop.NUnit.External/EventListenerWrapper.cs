using MonoDevelop.Core;
using NUnit.Core;
using System;
using System.Collections.Generic;
using System.Text;
namespace MonoDevelop.NUnit.External
{
	internal class EventListenerWrapper : System.MarshalByRefObject, EventListener
	{
		private IRemoteEventListener wrapped;
		private System.Text.StringBuilder consoleOutput;
		private System.Text.StringBuilder consoleError;
		private Stack<string> testSuites = new Stack<string>();
		public EventListenerWrapper(IRemoteEventListener wrapped)
		{
			this.wrapped = wrapped;
		}
		public void RunFinished(System.Exception exception)
		{
		}
		public void RunFinished(TestResult results)
		{
		}
		public void RunStarted(string name, int testCount)
		{
		}
		public void SuiteFinished(TestSuiteResult result)
		{
			this.testSuites.Pop();
			this.wrapped.SuiteFinished(this.GetTestName(result.Test), this.GetLocalTestResult(result));
		}
		public void SuiteStarted(TestName suite)
		{
			System.Console.WriteLine(string.Concat(new object[]
			{
				"start:", 
				suite.Name, 
				"/", 
				suite.GetType()
			}));
			this.testSuites.Push(suite.FullName);
			this.wrapped.SuiteStarted(this.GetTestName(suite));
		}
		public void TestFinished(TestCaseResult result)
		{
			this.wrapped.TestFinished(this.GetTestName(result.Test), this.GetLocalTestResult(result));
		}
		public void TestOutput(TestOutput testOutput)
		{
			if (this.consoleOutput == null)
			{
				System.Console.WriteLine(testOutput.Text);
			}
			else
			{
				if (testOutput.Type == TestOutputType.Out)
				{
					this.consoleOutput.Append(testOutput.Text);
				}
				else
				{
					this.consoleError.Append(testOutput.Text);
				}
			}
		}
		public void TestStarted(TestName testCase)
		{
			this.wrapped.TestStarted(this.GetTestName(testCase));
			this.consoleOutput = new System.Text.StringBuilder();
			this.consoleError = new System.Text.StringBuilder();
		}
		public override object InitializeLifetimeService()
		{
			return null;
		}
		private string GetTestName(ITest t)
		{
			string result;
			if (t == null)
			{
				result = null;
			}
			else
			{
				if (t.TestType != "Test Case" || this.testSuites.Count == 0)
				{
					result = t.TestName.FullName;
				}
				else
				{
					string name = t.TestName.Name;
					int idx = name.LastIndexOf('.');
					if (idx >= 0)
					{
						name = name.Substring(idx + 1);
					}
					result = this.testSuites.Peek() + "." + name;
				}
			}
			return result;
		}
		public string GetTestName(TestName t)
		{
			string result;
			if (t == null)
			{
				result = null;
			}
			else
			{
				result = t.FullName;
			}
			return result;
		}
		public UnitTestResult GetLocalTestResult(TestResult t)
		{
			UnitTestResult res = new UnitTestResult();
			res.Message = t.Message;
			if (t is TestSuiteResult)
			{
				int s = 0;
				int f = 0;
				int i = 0;
				this.CountResults((TestSuiteResult)t, ref s, ref f, ref i);
				res.TotalFailures = f;
				res.TotalSuccess = s;
				res.TotalIgnored = i;
				if (f > 0)
				{
					res.Status |= ResultStatus.Failure;
				}
				if (s > 0)
				{
					res.Status |= ResultStatus.Success;
				}
				if (i > 0)
				{
					res.Status |= ResultStatus.Ignored;
				}
			}
			else
			{
				if (t.IsFailure)
				{
					res.Status = ResultStatus.Failure;
					res.TotalFailures = 1;
				}
				else
				{
					if (!t.Executed)
					{
						res.Status = ResultStatus.Ignored;
						res.TotalIgnored = 1;
					}
					else
					{
						res.Status = ResultStatus.Success;
						res.TotalSuccess = 1;
					}
				}
				if (string.IsNullOrEmpty(res.Message))
				{
					if (t.IsFailure)
					{
						res.Message = GettextCatalog.GetString("Test failed");
					}
					else
					{
						if (!t.Executed)
						{
							res.Message = GettextCatalog.GetString("Test ignored");
						}
						else
						{
							res.Message = GettextCatalog.GetString("Test successful") + "\n\n";
							UnitTestResult expr_181 = res;
							expr_181.Message += GettextCatalog.GetString("Execution time: {0:0.00}ms", t.Time);
						}
					}
				}
			}
			res.StackTrace = t.StackTrace;
			res.Time = System.TimeSpan.FromSeconds(t.Time);
			if (this.consoleOutput != null)
			{
				res.ConsoleOutput = this.consoleOutput.ToString();
				res.ConsoleError = this.consoleError.ToString();
				this.consoleOutput = null;
				this.consoleError = null;
			}
			return res;
		}
		private void CountResults(TestSuiteResult ts, ref int s, ref int f, ref int i)
		{
			if (ts.Results != null)
			{
				foreach (TestResult t in ts.Results)
				{
					if (t is TestCaseResult)
					{
						if (t.IsFailure)
						{
							f++;
						}
						else
						{
							if (!t.Executed)
							{
								i++;
							}
							else
							{
								s++;
							}
						}
					}
					else
					{
						if (t is TestSuiteResult)
						{
							this.CountResults((TestSuiteResult)t, ref s, ref f, ref i);
						}
					}
				}
			}
		}
		public void UnhandledException(System.Exception exception)
		{
		}
	}
}
