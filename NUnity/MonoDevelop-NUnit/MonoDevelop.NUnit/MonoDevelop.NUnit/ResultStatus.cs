using System;
namespace MonoDevelop.NUnit
{
	[System.Flags]
	public enum ResultStatus
	{
		Success = 1,
		Failure = 2,
		Ignored = 4
	}
}
