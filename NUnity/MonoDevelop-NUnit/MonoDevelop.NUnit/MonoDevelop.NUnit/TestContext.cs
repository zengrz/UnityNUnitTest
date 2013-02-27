using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using System;
namespace MonoDevelop.NUnit
{
	public class TestContext
	{
		private ITestProgressMonitor monitor;
		private System.DateTime testDate;
		private object contextData;
		private IExecutionHandler executionContext;
		public ITestProgressMonitor Monitor
		{
			get
			{
				return this.monitor;
			}
		}
		public System.DateTime TestDate
		{
			get
			{
				return this.testDate;
			}
		}
		public object ContextData
		{
			get
			{
				return this.contextData;
			}
			set
			{
				this.contextData = value;
			}
		}
		public IExecutionHandler ExecutionContext
		{
			get
			{
				return this.executionContext;
			}
		}
		public TestContext(ITestProgressMonitor monitor, IExecutionHandler executionContext, System.DateTime testDate)
		{
			this.monitor = monitor;
			if (executionContext == null)
			{
				executionContext = Runtime.get_ProcessService().get_DefaultExecutionHandler();
			}
			this.executionContext = executionContext;
			this.testDate = new System.DateTime(testDate.Ticks / 10000000L * 10000000L);
		}
	}
}
