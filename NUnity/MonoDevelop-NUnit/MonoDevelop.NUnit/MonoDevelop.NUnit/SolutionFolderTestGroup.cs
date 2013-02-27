using MonoDevelop.Projects;
using System;
using System.IO;
namespace MonoDevelop.NUnit
{
	public class SolutionFolderTestGroup : UnitTestGroup
	{
		private SolutionFolder combine;
		public SolutionFolderTestGroup(SolutionFolder c) : base(c.get_Name(), c)
		{
			string storeId = c.get_ItemId();
			string resultsPath = System.IO.Path.Combine(c.get_BaseDirectory(), "test-results");
			base.ResultsStore = new XmlResultsStore(resultsPath, storeId);
			this.combine = c;
			this.combine.add_ItemAdded(new SolutionItemChangeEventHandler(this.OnEntryChanged));
			this.combine.add_ItemRemoved(new SolutionItemChangeEventHandler(this.OnEntryChanged));
			this.combine.add_NameChanged(new SolutionItemRenamedEventHandler(this.OnCombineRenamed));
		}
		public static SolutionFolderTestGroup CreateTest(SolutionFolder c)
		{
			return new SolutionFolderTestGroup(c);
		}
		public override void Dispose()
		{
			this.combine.remove_ItemAdded(new SolutionItemChangeEventHandler(this.OnEntryChanged));
			this.combine.remove_ItemRemoved(new SolutionItemChangeEventHandler(this.OnEntryChanged));
			this.combine.remove_NameChanged(new SolutionItemRenamedEventHandler(this.OnCombineRenamed));
			base.Dispose();
		}
		private void OnEntryChanged(object sender, SolutionItemEventArgs e)
		{
			base.UpdateTests();
		}
		private void OnCombineRenamed(object sender, SolutionItemRenamedEventArgs e)
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
			foreach (SolutionItem e in this.combine.get_Items())
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
