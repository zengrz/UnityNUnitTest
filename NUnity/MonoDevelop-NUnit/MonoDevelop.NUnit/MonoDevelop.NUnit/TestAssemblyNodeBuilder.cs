using Gdk;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Components;
using System;
using System.IO;
namespace MonoDevelop.NUnit
{
	public class TestAssemblyNodeBuilder : TypeNodeBuilder
	{
		public override System.Type CommandHandlerType
		{
			get
			{
				return typeof(TestAssemblyNodeCommandHandler);
			}
		}
		public override string ContextMenuAddinPath
		{
			get
			{
				return "/MonoDevelop/NUnit/ContextMenu/ProjectPad/TestAssembly";
			}
		}
		public override System.Type NodeDataType
		{
			get
			{
				return typeof(TestAssembly);
			}
		}
		public override string GetNodeName(ITreeNavigator thisNode, object dataObject)
		{
			return System.IO.Path.GetFileName(((TestAssembly)dataObject).Path);
		}
		public override void BuildNode(ITreeBuilder treeBuilder, object dataObject, ref string label, ref Pixbuf icon, ref Pixbuf closedIcon)
		{
			TestAssembly asm = dataObject as TestAssembly;
			label = System.IO.Path.GetFileName(asm.Path);
			icon = base.get_Context().GetIcon(Stock.Reference);
		}
	}
}
