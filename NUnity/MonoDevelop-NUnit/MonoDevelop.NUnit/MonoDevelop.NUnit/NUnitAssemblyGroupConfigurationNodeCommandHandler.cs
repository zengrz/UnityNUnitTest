using MonoDevelop.Components;
using MonoDevelop.Components.Commands;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui.Components;
using MonoDevelop.NUnit.Commands;
using System;
namespace MonoDevelop.NUnit
{
	internal class NUnitAssemblyGroupConfigurationNodeCommandHandler : NodeCommandHandler
	{
		[CommandHandler(NUnitProjectCommands.AddAssembly)]
		protected void OnAddAssembly()
		{
			NUnitAssemblyGroupProjectConfiguration config = (NUnitAssemblyGroupProjectConfiguration)base.get_CurrentNode().get_DataItem();
			SelectFileDialog selectFileDialog = new SelectFileDialog(GettextCatalog.GetString("Add files"));
			selectFileDialog.set_TransientFor(IdeApp.get_Workbench().get_RootWindow());
			selectFileDialog.set_SelectMultiple(true);
			SelectFileDialog dlg = selectFileDialog;
			if (dlg.Run())
			{
				FilePath[] selectedFiles = dlg.get_SelectedFiles();
				for (int i = 0; i < selectedFiles.Length; i++)
				{
					string file = selectedFiles[i];
					config.Assemblies.Add(new TestAssembly(file));
				}
				IdeApp.get_Workspace().Save();
			}
		}
	}
}
