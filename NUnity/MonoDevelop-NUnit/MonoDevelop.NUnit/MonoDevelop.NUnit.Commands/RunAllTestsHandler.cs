using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Projects;
using System;
namespace MonoDevelop.NUnit.Commands
{
	internal class RunAllTestsHandler : CommandHandler
	{
		protected override void Run()
		{
			IWorkspaceObject ob = IdeApp.get_ProjectOperations().get_CurrentSelectedBuildTarget();
			if (ob != null)
			{
				UnitTest test = NUnitService.Instance.FindRootTest(ob);
				if (test != null)
				{
					NUnitService.Instance.RunTest(test, null);
				}
			}
		}
		protected override void Update(CommandInfo info)
		{
			IWorkspaceObject ob = IdeApp.get_ProjectOperations().get_CurrentSelectedBuildTarget();
			if (ob != null)
			{
				UnitTest test = NUnitService.Instance.FindRootTest(ob);
				info.set_Enabled(test != null);
			}
			else
			{
				info.set_Enabled(false);
			}
		}
	}
}
