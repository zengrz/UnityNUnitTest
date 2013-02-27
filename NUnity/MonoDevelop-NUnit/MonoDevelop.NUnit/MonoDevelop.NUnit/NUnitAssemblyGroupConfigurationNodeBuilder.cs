using Gdk;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Components;
using MonoDevelop.Projects;
using System;
namespace MonoDevelop.NUnit
{
	public class NUnitAssemblyGroupConfigurationNodeBuilder : TypeNodeBuilder
	{
		private System.EventHandler assembliesChanged;
		public override System.Type CommandHandlerType
		{
			get
			{
				return typeof(NUnitAssemblyGroupConfigurationNodeCommandHandler);
			}
		}
		public override string ContextMenuAddinPath
		{
			get
			{
				return "/MonoDevelop/NUnit/ContextMenu/ProjectPad/NUnitAssemblyGroupConfiguration";
			}
		}
		public override System.Type NodeDataType
		{
			get
			{
				return typeof(NUnitAssemblyGroupProjectConfiguration);
			}
		}
		public NUnitAssemblyGroupConfigurationNodeBuilder()
		{
			this.assembliesChanged = DispatchService.GuiDispatch<System.EventHandler>(new System.EventHandler(this.OnAssembliesChanged));
		}
		public override string GetNodeName(ITreeNavigator thisNode, object dataObject)
		{
			return ((SolutionItemConfiguration)dataObject).get_Id();
		}
		public override void BuildNode(ITreeBuilder treeBuilder, object dataObject, ref string label, ref Pixbuf icon, ref Pixbuf closedIcon)
		{
			SolutionItemConfiguration conf = (SolutionItemConfiguration)dataObject;
			label = conf.get_Id();
			icon = base.get_Context().GetIcon(Stock.ClosedFolder);
		}
		public override void BuildChildNodes(ITreeBuilder builder, object dataObject)
		{
			NUnitAssemblyGroupProjectConfiguration config = (NUnitAssemblyGroupProjectConfiguration)dataObject;
			foreach (TestAssembly ta in config.Assemblies)
			{
				builder.AddChild(ta);
			}
		}
		public override bool HasChildNodes(ITreeBuilder builder, object dataObject)
		{
			NUnitAssemblyGroupProjectConfiguration config = (NUnitAssemblyGroupProjectConfiguration)dataObject;
			return config.Assemblies.Count > 0;
		}
		public override void OnNodeAdded(object dataObject)
		{
			NUnitAssemblyGroupProjectConfiguration config = (NUnitAssemblyGroupProjectConfiguration)dataObject;
			config.AssembliesChanged += this.assembliesChanged;
		}
		public override void OnNodeRemoved(object dataObject)
		{
			NUnitAssemblyGroupProjectConfiguration config = (NUnitAssemblyGroupProjectConfiguration)dataObject;
			config.AssembliesChanged -= this.assembliesChanged;
		}
		public void OnAssembliesChanged(object sender, System.EventArgs args)
		{
			ITreeBuilder tb = base.get_Context().GetTreeBuilder(sender);
			if (tb != null)
			{
				tb.UpdateAll();
			}
		}
	}
}
