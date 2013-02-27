using System;
namespace MonoDevelop.NUnit
{
	public class UnitTestResultsStore
	{
		private UnitTest test;
		private IResultsStore store;
		internal UnitTestResultsStore(UnitTest test, IResultsStore store)
		{
			this.test = test;
			this.store = store;
		}
		public UnitTestResult GetLastResult(System.DateTime date)
		{
			UnitTestResult result;
			if (this.store == null)
			{
				result = null;
			}
			else
			{
				result = this.store.GetLastResult(this.test.ActiveConfiguration, this.test, date);
			}
			return result;
		}
		public UnitTestResult GetNextResult(System.DateTime date)
		{
			UnitTestResult result;
			if (this.store == null)
			{
				result = null;
			}
			else
			{
				result = this.store.GetNextResult(this.test.ActiveConfiguration, this.test, date);
			}
			return result;
		}
		public UnitTestResult GetPreviousResult(System.DateTime date)
		{
			UnitTestResult result;
			if (this.store == null)
			{
				result = null;
			}
			else
			{
				result = this.store.GetPreviousResult(this.test.ActiveConfiguration, this.test, date);
			}
			return result;
		}
		public UnitTestResult[] GetResults(System.DateTime startDate, System.DateTime endDate)
		{
			UnitTestResult[] result;
			if (this.store == null)
			{
				result = new UnitTestResult[0];
			}
			else
			{
				result = this.store.GetResults(this.test.ActiveConfiguration, this.test, startDate, endDate);
			}
			return result;
		}
		public UnitTestResult[] GetResultsToDate(System.DateTime endDate, int count)
		{
			UnitTestResult[] result;
			if (this.store == null)
			{
				result = new UnitTestResult[0];
			}
			else
			{
				result = this.store.GetResultsToDate(this.test.ActiveConfiguration, this.test, endDate, count);
			}
			return result;
		}
	}
}
