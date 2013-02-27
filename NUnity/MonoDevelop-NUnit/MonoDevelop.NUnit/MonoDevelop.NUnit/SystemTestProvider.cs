using MonoDevelop.Projects;
using System;
namespace MonoDevelop.NUnit
{
	public class SystemTestProvider : ITestProvider
	{
		public UnitTest CreateUnitTest(IWorkspaceObject entry)
		{
			UnitTest test = null;
			if (entry is SolutionFolder)
			{
				test = SolutionFolderTestGroup.CreateTest((SolutionFolder)entry);
			}
			if (entry is Solution)
			{
				test = SolutionFolderTestGroup.CreateTest(((Solution)entry).get_RootFolder());
			}
			if (entry is Workspace)
			{
				test = WorkspaceTestGroup.CreateTest((Workspace)entry);
			}
			if (entry is DotNetProject)
			{
				test = NUnitProjectTestSuite.CreateTest((DotNetProject)entry);
			}
			if (entry is NUnitAssemblyGroupProject)
			{
				test = ((NUnitAssemblyGroupProject)entry).RootTest;
			}
			UnitTestGroup grp = test as UnitTestGroup;
			UnitTest result;
			if (grp != null && !grp.HasTests)
			{
				result = null;
			}
			else
			{
				result = test;
			}
			return result;
		}
		public System.Type[] GetOptionTypes()
		{
			return new System.Type[]
			{
				typeof(GeneralTestOptions), 
				typeof(NUnitCategoryOptions)
			};
		}
	}
}
