using System;
using System.Collections.ObjectModel;
namespace MonoDevelop.NUnit
{
	public class UnitTestCollection : System.Collections.ObjectModel.Collection<UnitTest>
	{
		private UnitTest owner;
		public UnitTest this[string name]
		{
			get
			{
				UnitTest result;
				for (int i = 0; i < base.Items.Count; i++)
				{
					if (base.Items[i].Name == name)
					{
						result = base.Items[i];
						return result;
					}
				}
				result = null;
				return result;
			}
		}
		internal UnitTestCollection(UnitTest owner)
		{
			this.owner = owner;
		}
		public UnitTestCollection()
		{
		}
		protected override void SetItem(int index, UnitTest item)
		{
			if (this.owner != null)
			{
				base[index].SetParent(null);
			}
			base.SetItem(index, item);
			if (this.owner != null)
			{
				item.SetParent(this.owner);
			}
		}
		protected override void RemoveItem(int index)
		{
			if (this.owner != null)
			{
				base[index].SetParent(null);
			}
			base.RemoveItem(index);
		}
		protected override void InsertItem(int index, UnitTest item)
		{
			base.InsertItem(index, item);
			if (this.owner != null)
			{
				item.SetParent(this.owner);
			}
		}
		protected override void ClearItems()
		{
			if (this.owner != null)
			{
				foreach (UnitTest t in this)
				{
					t.SetParent(null);
				}
			}
			base.ClearItems();
		}
	}
}
