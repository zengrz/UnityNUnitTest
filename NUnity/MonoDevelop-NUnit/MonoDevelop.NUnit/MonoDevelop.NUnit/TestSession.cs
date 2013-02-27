using Gtk;
using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using MonoDevelop.Ide;
using System;
using System.Collections.Generic;
using System.Threading;
namespace MonoDevelop.NUnit
{
	internal class TestSession : IAsyncOperation, ITestProgressMonitor
	{
		private UnitTest test;
		private TestMonitor monitor;
		private System.Threading.Thread runThread;
		private bool success;
		private System.Threading.ManualResetEvent waitEvent;
		private IExecutionHandler context;
		public event OperationHandler Completed;
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
		bool ITestProgressMonitor.IsCancelRequested
		{
			get
			{
				return this.monitor.IsCancelRequested;
			}
		}
		public bool IsCompleted
		{
			get
			{
				return this.runThread == null;
			}
		}
		public bool Success
		{
			get
			{
				return this.success;
			}
		}
		public bool SuccessWithWarnings
		{
			get
			{
				return false;
			}
		}
		public TestSession(UnitTest test, IExecutionHandler context, TestResultsPad resultsPad)
		{
			this.test = test;
			this.context = context;
			this.monitor = new TestMonitor(resultsPad);
		}
		public void Start()
		{
			this.runThread = new System.Threading.Thread(new System.Threading.ThreadStart(this.RunTests));
			this.runThread.Name = "NUnit test runner";
			this.runThread.IsBackground = true;
			this.runThread.Start();
		}
		private void RunTests()
		{
			try
			{
				TestSession.ResetResult(this.test);
				this.monitor.InitializeTestRun(this.test);
				TestContext ctx = new TestContext(this.monitor, this.context, System.DateTime.Now);
				this.test.Run(ctx);
				this.test.SaveResults();
				this.success = true;
			}
			catch (System.Exception ex)
			{
				LoggingService.LogError(ex.ToString());
				this.monitor.ReportRuntimeError(null, ex);
				this.success = false;
			}
			finally
			{
				this.monitor.FinishTestRun();
				this.runThread = null;
			}
			bool flag = false;
			try
			{
				System.Threading.Monitor.Enter(this, ref flag);
				if (this.waitEvent != null)
				{
					this.waitEvent.Set();
				}
			}
			finally
			{
				if (flag)
				{
					System.Threading.Monitor.Exit(this);
				}
			}
			if (this.Completed != null)
			{
				this.Completed.Invoke(this);
			}
		}
		public static void ResetResult(UnitTest test)
		{
			if (test != null)
			{
				test.ResetLastResult();
				UnitTestGroup group = test as UnitTestGroup;
				if (group != null)
				{
					foreach (UnitTest t in new System.Collections.Generic.List<UnitTest>(group.Tests))
					{
						TestSession.ResetResult(t);
					}
				}
			}
		}
		void ITestProgressMonitor.BeginTest(UnitTest test)
		{
			this.monitor.BeginTest(test);
		}
		void ITestProgressMonitor.EndTest(UnitTest test, UnitTestResult result)
		{
			this.monitor.EndTest(test, result);
		}
		void ITestProgressMonitor.ReportRuntimeError(string message, System.Exception exception)
		{
			this.monitor.ReportRuntimeError(message, exception);
		}
		void IAsyncOperation.Cancel()
		{
			this.monitor.Cancel();
		}
		public void WaitForCompleted()
		{
			if (!this.IsCompleted)
			{
				if (DispatchService.get_IsGuiThread())
				{
					while (!this.IsCompleted)
					{
						while (Application.EventsPending())
						{
							Application.RunIteration();
						}
						System.Threading.Thread.Sleep(100);
					}
				}
				else
				{
					bool flag = false;
					try
					{
						System.Threading.Monitor.Enter(this, ref flag);
						if (this.waitEvent == null)
						{
							this.waitEvent = new System.Threading.ManualResetEvent(false);
						}
					}
					finally
					{
						if (flag)
						{
							System.Threading.Monitor.Exit(this);
						}
					}
					this.waitEvent.WaitOne();
				}
			}
		}
	}
}
