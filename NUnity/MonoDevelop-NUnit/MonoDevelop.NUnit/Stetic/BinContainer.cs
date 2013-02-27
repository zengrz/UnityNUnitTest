using Gtk;
using System;
namespace Stetic
{
	internal class BinContainer
	{
		private Widget child;
		private UIManager uimanager;
		public static BinContainer Attach(Bin bin)
		{
			BinContainer bc = new BinContainer();
			bin.add_SizeRequested(new SizeRequestedHandler(bc.OnSizeRequested));
			bin.add_SizeAllocated(new SizeAllocatedHandler(bc.OnSizeAllocated));
			bin.add_Added(new AddedHandler(bc.OnAdded));
			return bc;
		}
		private void OnSizeRequested(object sender, SizeRequestedArgs args)
		{
			if (this.child != null)
			{
				args.set_Requisition(this.child.SizeRequest());
			}
		}
		private void OnSizeAllocated(object sender, SizeAllocatedArgs args)
		{
			if (this.child != null)
			{
				this.child.set_Allocation(args.get_Allocation());
			}
		}
		private void OnAdded(object sender, AddedArgs args)
		{
			this.child = args.get_Widget();
		}
		public void SetUiManager(UIManager uim)
		{
			this.uimanager = uim;
			this.child.add_Realized(new System.EventHandler(this.OnRealized));
		}
		private void OnRealized(object sender, System.EventArgs args)
		{
			if (this.uimanager != null)
			{
				Widget w = this.child.get_Toplevel();
				if (w != null && typeof(Window).IsInstanceOfType(w))
				{
					((Window)w).AddAccelGroup(this.uimanager.get_AccelGroup());
					this.uimanager = null;
				}
			}
		}
	}
}
