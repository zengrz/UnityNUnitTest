using Gdk;
using Gtk;
using Pango;
using System;
namespace MonoDevelop.NUnit
{
	internal class HeaderLabel : Widget
	{
		private string text;
		private Layout layout;
		private int padding;
		public string Markup
		{
			get
			{
				return this.text;
			}
			set
			{
				this.text = value;
				this.layout.SetMarkup(this.text);
				base.QueueDraw();
			}
		}
		public int Padding
		{
			get
			{
				return this.padding;
			}
			set
			{
				this.padding = value;
			}
		}
		public HeaderLabel()
		{
			base.set_WidgetFlags(base.get_WidgetFlags() | 32);
			this.layout = new Layout(base.get_PangoContext());
		}
		protected override bool OnExposeEvent(EventExpose args)
		{
			Gdk.GC gc = new Gdk.GC(base.get_GdkWindow());
			gc.set_ClipRectangle(base.get_Allocation());
			base.get_GdkWindow().DrawLayout(gc, this.padding, this.padding, this.layout);
			return true;
		}
	}
}
