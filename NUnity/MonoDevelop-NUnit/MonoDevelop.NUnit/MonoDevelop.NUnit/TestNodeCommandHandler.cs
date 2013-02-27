using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui.Components;
using MonoDevelop.NUnit.Commands;
using System;
namespace MonoDevelop.NUnit
{
	internal class TestNodeCommandHandler : NodeCommandHandler
	{
		[CommandHandler(TestCommands.ShowTestCode)]
		protected void OnShowTest()
		{
			UnitTest test = base.get_CurrentNode().get_DataItem() as UnitTest;
			UnitTestResult res = test.GetLastResult();
			SourceCodeLocation loc = test.SourceCodeLocation;
			if (loc != null)
			{
				IdeApp.get_Workbench().OpenDocument(loc.FileName, loc.Line, loc.Column, 3);
			}
		}
		[CommandHandler(TestCommands.GoToFailure)]
		protected void OnShowFailure()
		{
			UnitTest test = base.get_CurrentNode().get_DataItem() as UnitTest;
			SourceCodeLocation loc = null;
			UnitTestResult res = test.GetLastResult();
			if (res != null && res.IsFailure)
			{
				loc = res.GetFailureLocation();
			}
			if (loc == null)
			{
				loc = test.SourceCodeLocation;
			}
			if (loc != null)
			{
				IdeApp.get_Workbench().OpenDocument(loc.FileName, loc.Line, loc.Column, 3);
			}
		}
		[CommandUpdateHandler(TestCommands.ShowTestCode)]
		protected void OnUpdateRunTest(CommandInfo info)
		{
			UnitTest test = base.get_CurrentNode().get_DataItem() as UnitTest;
			info.set_Enabled(test.SourceCodeLocation != null);
		}
		[CommandUpdateHandler]
		protected void OnUpdateShowOptions(CommandInfo info)
		{
			info.set_Visible(!(base.get_CurrentNode().get_DataItem() is SolutionFolderTestGroup));
		}
		[CommandHandler]
		protected void OnShowOptions()
		{
			UnitTest test = base.get_CurrentNode().get_DataItem() as UnitTest;
			NUnitService.ShowOptionsDialog(test);
		}
	}
}
