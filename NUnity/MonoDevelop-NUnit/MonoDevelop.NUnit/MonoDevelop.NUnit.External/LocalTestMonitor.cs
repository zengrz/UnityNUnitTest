using System;
namespace MonoDevelop.NUnit.External
{
	internal class LocalTestMonitor : System.MarshalByRefObject, IRemoteEventListener
	{
		private TestContext context;
		private UnitTest rootTest;
		private string rootFullName;
		private UnitTest runningTest;
		private bool singleTestRun;
		private UnitTestResult singleTestResult;
		public bool Canceled;
		public UnitTest RunningTest
		{
			get
			{
				return this.runningTest;
			}
		}
		internal UnitTestResult SingleTestResult
		{
			get
			{
				if (this.singleTestResult == null)
				{
					this.singleTestResult = new UnitTestResult();
				}
				return this.singleTestResult;
			}
			set
			{
				this.singleTestResult = value;
			}
		}
		public LocalTestMonitor(TestContext context, ExternalTestRunner runner, UnitTest rootTest, string rootFullName, bool singleTestRun)
		{
			this.rootFullName = rootFullName;
			this.rootTest = rootTest;
			this.context = context;
			this.singleTestRun = singleTestRun;
		}
		void IRemoteEventListener.TestStarted(string testCase)
		{
			if (!this.singleTestRun && !this.Canceled)
			{
				UnitTest t = this.GetLocalTest(testCase);
				if (t != null)
				{
					this.runningTest = t;
					this.context.Monitor.BeginTest(t);
					t.Status = TestStatus.Running;
				}
			}
		}
		void IRemoteEventListener.TestFinished(string test, UnitTestResult result)
		{
			if (!this.Canceled)
			{
				if (this.singleTestRun)
				{
					this.SingleTestResult = result;
				}
				else
				{
					UnitTest t = this.GetLocalTest(test);
					if (t != null)
					{
						t.RegisterResult(this.context, result);
						this.context.Monitor.EndTest(t, result);
						t.Status = TestStatus.Ready;
						this.runningTest = null;
					}
				}
			}
		}
		void IRemoteEventListener.SuiteStarted(string suite)
		{
			if (!this.singleTestRun && !this.Canceled)
			{
				UnitTest t = this.GetLocalTest(suite);
				if (t != null)
				{
					t.Status = TestStatus.Running;
					this.context.Monitor.BeginTest(t);
				}
			}
		}
		void IRemoteEventListener.SuiteFinished(string suite, UnitTestResult result)
		{
			if (!this.singleTestRun && !this.Canceled)
			{
				UnitTest t = this.GetLocalTest(suite);
				if (t != null)
				{
					t.RegisterResult(this.context, result);
					t.Status = TestStatus.Ready;
					this.context.Monitor.EndTest(t, result);
				}
			}
		}
		private UnitTest GetLocalTest(string sname)
		{
			UnitTest result;
			if (sname == null)
			{
				result = null;
			}
			else
			{
				if (sname == "<root>")
				{
					result = this.rootTest;
				}
				else
				{
					if (sname.StartsWith(this.rootFullName))
					{
						sname = sname.Substring(this.rootFullName.Length);
					}
					if (sname.StartsWith("."))
					{
						sname = sname.Substring(1);
					}
					UnitTest tt = this.FindTest(this.rootTest, sname);
					result = tt;
				}
			}
			return result;
		}
		private UnitTest FindTest(UnitTest t, string testPath)
		{
			UnitTest result;
			if (testPath == "")
			{
				result = t;
			}
			else
			{
				UnitTestGroup group = t as UnitTestGroup;
				if (group == null)
				{
					result = null;
				}
				else
				{
					UnitTest returnTest = group.Tests[testPath];
					if (returnTest != null)
					{
						result = returnTest;
					}
					else
					{
						string[] paths = testPath.Split(new char[]
						{
							'.'
						}, 2);
						if (paths.Length == 2)
						{
							string nextPathSection = paths[0];
							string nextTestCandidate = paths[1];
							UnitTest childTest = group.Tests[nextPathSection];
							if (childTest != null)
							{
								result = this.FindTest(childTest, nextTestCandidate);
								return result;
							}
						}
						result = null;
					}
				}
			}
			return result;
		}
		public override object InitializeLifetimeService()
		{
			return null;
		}
	}
}
