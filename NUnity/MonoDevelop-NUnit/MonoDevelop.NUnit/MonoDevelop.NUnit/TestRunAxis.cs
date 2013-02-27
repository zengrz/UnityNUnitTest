using MonoDevelop.Components.Chart;
using System;
namespace MonoDevelop.NUnit
{
	internal class TestRunAxis : IntegerAxis
	{
		public UnitTestResult[] CurrentResults;
		public TestRunAxis(bool showLabel) : base(showLabel)
		{
		}
		public override string GetValueLabel(double value)
		{
			string result;
			if (this.CurrentResults == null)
			{
				result = "";
			}
			else
			{
				int val = (int)value;
				if (val >= this.CurrentResults.Length)
				{
					result = "";
				}
				else
				{
					UnitTestResult res = this.CurrentResults[this.CurrentResults.Length - val - 1];
					string arg_74_0 = "{0}/{1}";
					System.DateTime testDate = res.TestDate;
					object arg_74_1 = testDate.Day;
					testDate = res.TestDate;
					result = string.Format(arg_74_0, arg_74_1, testDate.Month);
				}
			}
			return result;
		}
	}
}
