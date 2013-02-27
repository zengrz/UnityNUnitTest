using Gtk;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui.Dialogs;
using System;
namespace MonoDevelop.NUnit
{
	public class NUnitOptionsPanel : OptionsPanel
	{
		private NUnitOptionsWidget widget;
		public override Widget CreatePanelWidget()
		{
			return this.widget = new NUnitOptionsWidget((Properties)base.get_DataObject());
		}
		public override void ApplyChanges()
		{
			System.Console.WriteLine("STORE !!!!");
			this.widget.Store((Properties)base.get_DataObject());
		}
	}
}
