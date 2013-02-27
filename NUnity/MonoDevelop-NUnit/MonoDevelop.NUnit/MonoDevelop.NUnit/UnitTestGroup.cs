using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using MonoDevelop.Core.ProgressMonitoring;
using MonoDevelop.Projects;
using System;
namespace MonoDevelop.NUnit
{
	public class UnitTestGroup : UnitTest
	{
		private UnitTestCollection tests;
		public virtual bool HasTests
		{
			get
			{
				bool result;
				foreach (UnitTest t in this.Tests)
				{
					if (!(t is UnitTestGroup))
					{
						result = true;
						return result;
					}
					if (((UnitTestGroup)t).HasTests)
					{
						result = true;
						return result;
					}
				}
				result = false;
				return result;
			}
		}
		public UnitTestCollection Tests
		{
			get
			{
				if (this.tests == null)
				{
					this.tests = new UnitTestCollection(this);
					this.OnCreateTests();
				}
				return this.tests;
			}
		}
		public UnitTestGroup(string name) : base(name)
		{
		}
		protected UnitTestGroup(string name, IWorkspaceObject ownerSolutionItem) : base(name, ownerSolutionItem)
		{
		}
		public UnitTestCollection GetFailedTests(System.DateTime date)
		{
			UnitTestCollection col = new UnitTestCollection();
			this.CollectFailedTests(col, date);
			return col;
		}
		private void CollectFailedTests(UnitTestCollection col, System.DateTime date)
		{
			foreach (UnitTest t in this.Tests)
			{
				if (t is UnitTestGroup)
				{
					((UnitTestGroup)t).CollectFailedTests(col, date);
				}
				else
				{
					UnitTestResult res = t.Results.GetLastResult(date);
					if (res != null && res.IsFailure)
					{
						col.Add(t);
					}
				}
			}
		}
		public void UpdateTests()
		{
			if (this.tests != null)
			{
				foreach (UnitTest t in this.tests)
				{
					t.Dispose();
				}
				this.tests = null;
				this.OnTestChanged();
			}
		}
		public override void SaveResults()
		{
			base.SaveResults();
			if (this.tests != null)
			{
				foreach (UnitTest t in this.tests)
				{
					t.SaveResults();
				}
			}
		}
		public override int CountTestCases()
		{
			int total = 0;
			foreach (UnitTest t in this.Tests)
			{
				total += t.CountTestCases();
			}
			return total;
		}
		protected virtual void OnCreateTests()
		{
		}
		public override IAsyncOperation Refresh()
		{
			AggregatedAsyncOperation oper = new AggregatedAsyncOperation();
			foreach (UnitTest t in this.Tests)
			{
				oper.Add(t.Refresh());
			}
			oper.StartMonitoring();
			return oper;
		}
		protected override UnitTestResult OnRun(TestContext testContext)
		{
			UnitTestResult tres = new UnitTestResult();
			this.OnBeginTest(testContext);
			try
			{
				foreach (UnitTest t in this.Tests)
				{
					UnitTestResult res;
					try
					{
						res = this.OnRunChildTest(t, testContext);
						if (testContext.Monitor.IsCancelRequested)
						{
							break;
						}
					}
					catch (System.Exception ex)
					{
						res = UnitTestResult.CreateFailure(ex);
					}
					UnitTestResult expr_60 = tres;
					expr_60.Time += res.Time;
					tres.Status |= res.Status;
					tres.TotalFailures += res.TotalFailures;
					tres.TotalSuccess += res.TotalSuccess;
					tres.TotalIgnored += res.TotalIgnored;
				}
			}
			finally
			{
				this.OnEndTest(testContext);
			}
			return tres;
		}
		protected override bool OnCanRun(IExecutionHandler executionContext)
		{
			bool result;
			foreach (UnitTest t in this.Tests)
			{
				if (!t.CanRun(executionContext))
				{
					result = false;
					return result;
				}
			}
			result = true;
			return result;
		}
		protected virtual void OnBeginTest(TestContext testContext)
		{
		}
		protected virtual UnitTestResult OnRunChildTest(UnitTest test, TestContext testContext)
		{
			return test.Run(testContext);
		}
		protected virtual void OnEndTest(TestContext testContext)
		{
		}
		internal override void FindRegressions(UnitTestCollection list, System.DateTime fromDate, System.DateTime toDate)
		{
			foreach (UnitTest test in this.Tests)
			{
				test.FindRegressions(list, fromDate, toDate);
			}
		}
		public override void Dispose()
		{
			base.Dispose();
			if (this.tests != null)
			{
				foreach (UnitTest t in this.tests)
				{
					t.Dispose();
				}
			}
		}
	}
}
