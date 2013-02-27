using MonoDevelop.Ide.Gui.Components;
using System;
namespace MonoDevelop.NUnit
{
	internal class TestAssemblyNodeCommandHandler : NodeCommandHandler
	{
		public override void DeleteItem()
		{
			TestAssembly asm = base.get_CurrentNode().get_DataItem() as TestAssembly;
			NUnitAssemblyGroupProjectConfiguration config = (NUnitAssemblyGroupProjectConfiguration)base.get_CurrentNode().GetParentDataItem(typeof(NUnitAssemblyGroupProjectConfiguration), false);
			config.Assemblies.Remove(asm);
		}
	}
}
