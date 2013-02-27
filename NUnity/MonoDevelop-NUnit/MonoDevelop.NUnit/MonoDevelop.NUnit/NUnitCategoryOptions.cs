using MonoDevelop.Core;
using MonoDevelop.Core.Serialization;
using System;
using System.Collections.Specialized;
using System.Text;
namespace MonoDevelop.NUnit
{
	public class NUnitCategoryOptions : System.ICloneable
	{
		[ItemProperty("Categories"), ItemProperty("Category", ValueType = typeof(string), Scope = "*")]
		private StringCollection categories = new StringCollection();
		private bool enableFilter;
		private bool exclude;
		public StringCollection Categories
		{
			get
			{
				return this.categories;
			}
		}
		[ItemProperty]
		public bool EnableFilter
		{
			get
			{
				return this.enableFilter;
			}
			set
			{
				this.enableFilter = value;
			}
		}
		[ItemProperty]
		public bool Exclude
		{
			get
			{
				return this.exclude;
			}
			set
			{
				this.exclude = value;
			}
		}
		public object Clone()
		{
			NUnitCategoryOptions op = new NUnitCategoryOptions();
			op.enableFilter = this.enableFilter;
			op.exclude = this.exclude;
			op.categories = new StringCollection();
			foreach (string s in this.categories)
			{
				op.categories.Add(s);
			}
			return op;
		}
		public override string ToString()
		{
			string result;
			if (this.EnableFilter && this.Categories.Count > 0)
			{
				System.Text.StringBuilder s = new System.Text.StringBuilder();
				if (this.Exclude)
				{
					s.Append(GettextCatalog.GetString("Exclude the following categories: "));
				}
				else
				{
					s.Append(GettextCatalog.GetString("Include the following categories: "));
				}
				for (int i = 0; i < this.Categories.Count; i++)
				{
					if (i > 0)
					{
						s.Append(", ");
					}
					s.Append(this.Categories[i]);
				}
				result = s.ToString();
			}
			else
			{
				result = "";
			}
			return result;
		}
	}
}
