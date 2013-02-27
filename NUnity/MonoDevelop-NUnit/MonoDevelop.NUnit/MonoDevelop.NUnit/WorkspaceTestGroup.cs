using MonoDevelop.Projects;
using System;
using System.IO;
namespace MonoDevelop.NUnit
{
	public class WorkspaceTestGroup : UnitTestGroup
	{
		private Workspace workspace;
		public WorkspaceTestGroup(Workspace ws) : base(ws.get_Name(), ws)
		{
			string storeId = ws.get_Name();
			string resultsPath = System.IO.Path.Combine(ws.get_BaseDirectory(), "test-results");
			base.ResultsStore = new XmlResultsStore(resultsPath, storeId);
			this.workspace = ws;
			this.workspace.add_ItemAdded(new System.EventHandler<WorkspaceItemChangeEventArgs>(this.OnEntryChanged));
			this.workspace.add_ItemRemoved(new System.EventHandler<WorkspaceItemChangeEventArgs>(this.OnEntryChanged));
			this.workspace.add_NameChanged(new System.EventHandler<WorkspaceItemRenamedEventArgs>(this.OnCombineRenamed));
		}
		public static WorkspaceTestGroup CreateTest(Workspace ws)
		{
			return new WorkspaceTestGroup(ws);
		}
		public override void Dispose()
		{
			this.workspace.remove_ItemAdded(new System.EventHandler<WorkspaceItemChangeEventArgs>(this.OnEntryChanged));
			this.workspace.remove_ItemRemoved(new System.EventHandler<WorkspaceItemChangeEventArgs>(this.OnEntryChanged));
			this.workspace.remove_NameChanged(new System.EventHandler<WorkspaceItemRenamedEventArgs>(this.OnCombineRenamed));
			base.Dispose();
		}
		private void OnEntryChanged(object sender, WorkspaceItemEventArgs e)
		{
			base.UpdateTests();
		}
		private void OnCombineRenamed(object sender, WorkspaceItemRenamedEventArgs e)
		{
			UnitTestGroup parent = base.Parent as UnitTestGroup;
			if (parent != null)
			{
				parent.UpdateTests();
			}
		}
		protected override void OnCreateTests()
		{
			NUnitService testService = NUnitService.Instance;
			foreach (WorkspaceItem e in this.workspace.get_Items())
			{
				UnitTest t = testService.BuildTest(e);
				if (t != null)
				{
					base.Tests.Add(t);
				}
			}
		}
	}
}
