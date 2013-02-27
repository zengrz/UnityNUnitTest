using Gtk;
using System;
using System.Collections;
namespace MonoDevelop.NUnit
{
	internal class ButtonNotebook : Notebook
	{
		private System.Collections.ArrayList loadedPages = new System.Collections.ArrayList();
		public System.EventHandler PageLoadRequired;
		public void Reset()
		{
			this.loadedPages.Clear();
			this.OnPageLoadRequired();
		}
		public int AddPage(string text, Widget widget)
		{
			return base.AppendPage(widget, new Label(text));
		}
		public void ShowPage(int n)
		{
			base.GetNthPage(n).Show();
		}
		public void HidePage(int n)
		{
			base.GetNthPage(n).Hide();
		}
		protected override void OnSwitchPage(NotebookPage page, uint n)
		{
			base.OnSwitchPage(page, n);
			if (!this.loadedPages.Contains(base.get_Page()))
			{
				this.OnPageLoadRequired();
			}
		}
		private void OnPageLoadRequired()
		{
			this.loadedPages.Add(base.get_Page());
			if (this.PageLoadRequired != null)
			{
				this.PageLoadRequired(this, System.EventArgs.Empty);
			}
		}
	}
}
