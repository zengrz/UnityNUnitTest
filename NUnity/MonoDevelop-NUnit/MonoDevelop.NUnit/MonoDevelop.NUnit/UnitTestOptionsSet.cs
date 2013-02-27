using MonoDevelop.Core.Serialization;
using System;
using System.Collections;
namespace MonoDevelop.NUnit
{
	internal class UnitTestOptionsSet
	{
		[ExpandedCollection, ItemProperty("Test", ValueType = typeof(UnitTestOptionsEntry))]
		public System.Collections.ArrayList Tests = new System.Collections.ArrayList();
		public UnitTestOptionsEntry FindEntry(string testPath)
		{
			UnitTestOptionsEntry result;
			foreach (UnitTestOptionsEntry t in this.Tests)
			{
				if (t.Path == testPath)
				{
					result = t;
					return result;
				}
			}
			result = null;
			return result;
		}
	}
}
