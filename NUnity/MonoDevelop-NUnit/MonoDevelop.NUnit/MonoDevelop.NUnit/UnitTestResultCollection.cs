using System;
using System.Collections;
namespace MonoDevelop.NUnit
{
	public class UnitTestResultCollection : System.Collections.CollectionBase
	{
		public UnitTestResult this[int n]
		{
			get
			{
				return (UnitTestResult)((System.Collections.IList)this)[n];
			}
		}
		public void Add(UnitTestResult test)
		{
			((System.Collections.IList)this).Add(test);
		}
	}
}
