using Gdk;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Components;
using MonoDevelop.Projects;
using System;
using System.Collections.Generic;
namespace MonoDevelop.NUnit
{
	public class NUnitAssemblyGroupNodeBuilder : TypeNodeBuilder
	{
		private ConfigurationEventHandler configsChanged;
		public override System.Type CommandHandlerType
		{
			get
			{
				return typeof(NUnitAssemblyGroupNodeCommandHandler);
			}
		}
		public override string ContextMenuAddinPath
		{
			get
			{
				return "/MonoDevelop/NUnit/ContextMenu/ProjectPad/NUnitAssemblyGroup";
			}
		}
		public override System.Type NodeDataType
		{
			get
			{
				return typeof(NUnitAssemblyGroupProject);
			}
		}
		public NUnitAssemblyGroupNodeBuilder()
		{
			this.configsChanged = DispatchService.GuiDispatch<ConfigurationEventHandler>(new ConfigurationEventHandler(this.OnConfigurationsChanged));
		}
		public override string GetNodeName(ITreeNavigator thisNode, object dataObject)
		{
			return ((NUnitAssemblyGroupProject)dataObject).get_Name();
		}
		public override void BuildNode(ITreeBuilder treeBuilder, object dataObject, ref string label, ref Pixbuf icon, ref Pixbuf closedIcon)
		{
			NUnitAssemblyGroupProject project = dataObject as NUnitAssemblyGroupProject;
			label = project.get_Name();
			icon = base.get_Context().GetIcon(Stock.Project);
		}
		public override void BuildChildNodes(ITreeBuilder builder, object dataObject)
		{
			NUnitAssemblyGroupProject project = dataObject as NUnitAssemblyGroupProject;
			using (System.Collections.Generic.IEnumerator<SolutionItemConfiguration> enumerator = project.get_Configurations().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					NUnitAssemblyGroupProjectConfiguration c = (NUnitAssemblyGroupProjectConfiguration)enumerator.Current;
					builder.AddChild(c);
				}
			}
		}
		public override bool HasChildNodes(ITreeBuilder builder, object dataObject)
		{
			NUnitAssemblyGroupProject project = dataObject as NUnitAssemblyGroupProject;
			return project.get_Configurations().Count > 0;
		}
		public override void OnNodeAdded(object dataObject)
		{
			NUnitAssemblyGroupProject project = dataObject as NUnitAssemblyGroupProject;
			project.add_ConfigurationAdded(this.configsChanged);
			project.add_ConfigurationRemoved(this.configsChanged);
		}
		public override void OnNodeRemoved(object dataObject)
		{
			NUnitAssemblyGroupProject project = dataObject as NUnitAssemblyGroupProject;
			project.remove_ConfigurationAdded(this.configsChanged);
			project.remove_ConfigurationRemoved(this.configsChanged);
		}
		public void OnConfigurationsChanged(object sender, ConfigurationEventArgs args)
		{
			ITreeBuilder tb = base.get_Context().GetTreeBuilder(sender);
			if (tb != null)
			{
				tb.UpdateAll();
			}
		}
	}
}
