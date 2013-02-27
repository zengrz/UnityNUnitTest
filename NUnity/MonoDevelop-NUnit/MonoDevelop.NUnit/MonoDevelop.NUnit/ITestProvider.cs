using MonoDevelop.Projects;
using System;
namespace MonoDevelop.NUnit
{
	public interface ITestProvider
	{
		UnitTest CreateUnitTest(IWorkspaceObject entry);
		System.Type[] GetOptionTypes();
	}
}
