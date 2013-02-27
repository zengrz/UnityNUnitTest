using MonoDevelop.Ide.Gui;
using System;
namespace MonoDevelop.NUnit
{
	internal class TestMonitor : GuiSyncObject, ITestProgressMonitor
	{
		private ITestProgressMonitor monitor;
		private TestResultsPad pad;
		public event TestHandler CancelRequested
		{
			add
			{
				this.monitor.CancelRequested += value;
			}
			remove
			{
				this.monitor.CancelRequested -= value;
			}
		}
		public bool IsCancelRequested
		{
			get
			{
				return this.monitor.IsCancelRequested;
			}
		}
		public TestMonitor(TestResultsPad pad)
		{
			this.pad = pad;
			this.monitor = pad;
		}
		public void InitializeTestRun(UnitTest test)
		{
			this.pad.InitializeTestRun(test);
		}
		public void FinishTestRun()
		{
			this.pad.FinishTestRun();
		}
		public void Cancel()
		{
			this.pad.Cancel();
		}
		public void BeginTest(UnitTest test)
		{
			this.monitor.BeginTest(test);
		}
		public void EndTest(UnitTest test, UnitTestResult result)
		{
			this.monitor.EndTest(test, result);
		}
		public void ReportRuntimeError(string message, System.Exception exception)
		{
			this.monitor.ReportRuntimeError(message, exception);
		}
	}
}
