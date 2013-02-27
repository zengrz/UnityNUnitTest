using System;
using System.Collections;
namespace MonoDevelop.NUnit
{
	public class TestRecordCollection : System.Collections.CollectionBase
	{
		public TestRecord this[int n]
		{
			get
			{
				return (TestRecord)((System.Collections.IList)this)[n];
			}
		}
		public TestRecord this[string name]
		{
			get
			{
				TestRecord result;
				for (int i = 0; i < base.List.Count; i++)
				{
					if (((TestRecord)base.List[i]).Name == name)
					{
						result = (TestRecord)base.List[i];
						return result;
					}
				}
				result = null;
				return result;
			}
		}
		public void Add(TestRecord test)
		{
			((System.Collections.IList)this).Add(test);
		}
	}
}
