using MonoDevelop.Core.Serialization;
using System;
using System.IO;
namespace MonoDevelop.NUnit
{
	public class TestAssembly : NUnitAssemblyTestSuite
	{
		[ItemProperty("Path")]
		private string path;
		public override string Name
		{
			get
			{
				return System.IO.Path.GetFileNameWithoutExtension(this.path);
			}
		}
		public string Path
		{
			get
			{
				return this.path;
			}
			set
			{
				this.path = value;
			}
		}
		protected override string AssemblyPath
		{
			get
			{
				return this.path;
			}
		}
		protected override string TestInfoCachePath
		{
			get
			{
				string result;
				if (base.Parent != null)
				{
					result = System.IO.Path.Combine(((RootTest)base.Parent).ResultsPath, System.IO.Path.GetFileName(this.path) + ".test-cache");
				}
				else
				{
					result = null;
				}
				return result;
			}
		}
		public TestAssembly() : base(null)
		{
		}
		public TestAssembly(string path) : base(null)
		{
			this.path = path;
		}
	}
}
