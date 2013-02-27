using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui.Components;
using MonoDevelop.NUnit.Commands;
using System;
namespace MonoDevelop.NUnit
{
	internal class NUnitAssemblyGroupNodeCommandHandler : NodeCommandHandler
	{
		[CommandHandler(NUnitProjectCommands.AddAssembly)]
		protected void OnShowTest()
		{
		}
		public override void DeleteItem()
		{
			NUnitAssemblyGroupProject project = base.get_CurrentNode().get_DataItem() as NUnitAssemblyGroupProject;
			project.get_ParentFolder().get_Items().Remove(project);
			project.Dispose();
			IdeApp.get_Workspace().Save();
		}
	}
}
