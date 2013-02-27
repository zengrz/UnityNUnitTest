using Gtk;
using Mono.Addins;
using MonoDevelop.Core;
using MonoDevelop.Ide.Extensions;
using MonoDevelop.Ide.Gui.Dialogs;
using System;
namespace MonoDevelop.NUnit
{
	public class UnitTestOptionsDialog : OptionsDialog
	{
		private ExtensionNode configurationNode;
		private UnitTest test;
		private OptionsDialogSection firstSection = null;
		public UnitTestOptionsDialog(Window parent, Properties properties) : base(parent, properties, "/MonoDevelop/NUnit/UnitTestOptions/GeneralOptions", false)
		{
			base.set_Title(GettextCatalog.GetString("Unit Test Options"));
			this.test = properties.Get<UnitTest>("UnitTest");
			this.configurationNode = AddinManager.GetExtensionNode("/MonoDevelop/NUnit/UnitTestOptions/ConfigurationOptions");
			TreeIter iter;
			if (this.store.GetIterFirst(ref iter))
			{
				OptionsDialogSection section = this.store.GetValue(iter, 0) as OptionsDialogSection;
				if (section != null && section.get_Id() == "Configurations")
				{
					this.FillConfigurations(iter);
				}
			}
			base.ExpandCategories();
			if (this.firstSection != null)
			{
				base.ShowPage(this.firstSection);
			}
		}
		protected override void OnResponse(ResponseType response_id)
		{
			base.OnResponse(response_id);
			this.Destroy();
		}
		private void FillConfigurations(TreeIter configIter)
		{
			string[] configurations = this.test.GetConfigurations();
			for (int i = 0; i < configurations.Length; i++)
			{
				string name = configurations[i];
				Properties configNodeProperties = new Properties();
				configNodeProperties.Set("UnitTest", this.test);
				configNodeProperties.Set("Config", name);
				System.Console.WriteLine("contig: " + name);
				foreach (OptionsDialogSection section in this.configurationNode.get_ChildNodes())
				{
					OptionsDialogSection s = (OptionsDialogSection)section.Clone();
					if (this.firstSection == null)
					{
						this.firstSection = s;
					}
					OptionsPanelNode arg_CB_0 = s;
					string arg_C6_0 = section.get_Label();
					string[,] array = new string[1, 2];
					array[0, 0] = "Configuration";
					array[0, 1] = name;
					arg_CB_0.set_Label(StringParserService.Parse(arg_C6_0, array));
					base.AddSection(configIter, s, configNodeProperties);
				}
			}
		}
	}
}
