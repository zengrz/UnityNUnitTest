using System;
namespace MonoDevelop.NUnit
{
	public class GeneralTestOptions : System.ICloneable
	{
		public object Clone()
		{
			return new GeneralTestOptions();
		}
	}
}
