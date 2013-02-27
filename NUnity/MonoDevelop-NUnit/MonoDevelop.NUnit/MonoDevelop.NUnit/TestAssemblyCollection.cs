using System;
using System.Collections;
namespace MonoDevelop.NUnit
{
	public class TestAssemblyCollection : System.Collections.CollectionBase
	{
		private NUnitAssemblyGroupProjectConfiguration owner;
		public TestAssembly this[int n]
		{
			get
			{
				return (TestAssembly)base.List[n];
			}
		}
		internal TestAssemblyCollection(NUnitAssemblyGroupProjectConfiguration owner)
		{
			this.owner = owner;
		}
		public void Add(TestAssembly asm)
		{
			base.List.Add(asm);
		}
		public void Remove(TestAssembly asm)
		{
			base.List.Remove(asm);
		}
		protected override void OnInsertComplete(int index, object value)
		{
			this.owner.OnAssembliesChanged();
		}
		protected override void OnRemoveComplete(int index, object value)
		{
			this.owner.OnAssembliesChanged();
		}
		protected override void OnSetComplete(int index, object oldValue, object newValue)
		{
			this.owner.OnAssembliesChanged();
		}
		protected override void OnClearComplete()
		{
			this.owner.OnAssembliesChanged();
		}
	}
}
