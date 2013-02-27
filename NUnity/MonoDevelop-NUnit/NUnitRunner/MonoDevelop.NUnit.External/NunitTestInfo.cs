using System;
namespace MonoDevelop.NUnit.External
{
	[System.Serializable]
	public class NunitTestInfo
	{
		public string Name;
		public string PathName;
		public NunitTestInfo[] Tests;
	}
}
