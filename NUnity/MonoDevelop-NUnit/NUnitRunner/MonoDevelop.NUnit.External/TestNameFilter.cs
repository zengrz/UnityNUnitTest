using NUnit.Core;
using System;
namespace MonoDevelop.NUnit.External
{
	[System.Serializable]
	public class TestNameFilter : ITestFilter
	{
		private string name;
		public bool IsEmpty
		{
			get
			{
				return false;
			}
		}
		public TestNameFilter(string name)
		{
			this.name = name;
		}
		public bool Pass(ITest test)
		{
			bool result;
			if (test is TestCase && test.TestName.FullName == this.name)
			{
				result = true;
			}
			else
			{
				if (test.Tests != null)
				{
					foreach (ITest ct in test.Tests)
					{
						if (this.Pass(ct))
						{
							result = true;
							return result;
						}
					}
				}
				result = false;
			}
			return result;
		}
		public bool Match(ITest test)
		{
			return this.Pass(test);
		}
	}
}
