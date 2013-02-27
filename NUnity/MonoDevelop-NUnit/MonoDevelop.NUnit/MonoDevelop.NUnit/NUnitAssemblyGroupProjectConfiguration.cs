using MonoDevelop.Core.Serialization;
using MonoDevelop.Projects;
using System;
namespace MonoDevelop.NUnit
{
	public class NUnitAssemblyGroupProjectConfiguration : SolutionItemConfiguration
	{
		private TestAssemblyCollection assemblies;
		public event System.EventHandler AssembliesChanged;
		[ItemProperty("Assembly", ValueType = typeof(TestAssembly), Scope = "*"), ItemProperty("Assemblies")]
		public TestAssemblyCollection Assemblies
		{
			get
			{
				return this.assemblies;
			}
		}
		public NUnitAssemblyGroupProjectConfiguration()
		{
			this.assemblies = new TestAssemblyCollection(this);
		}
		public override void CopyFrom(ItemConfiguration other)
		{
			base.CopyFrom(other);
			NUnitAssemblyGroupProjectConfiguration conf = other as NUnitAssemblyGroupProjectConfiguration;
			if (conf != null)
			{
				this.assemblies.Clear();
				foreach (TestAssembly ta in conf.Assemblies)
				{
					TestAssembly copy = new TestAssembly(ta.Path);
					this.assemblies.Add(copy);
				}
			}
		}
		internal void OnAssembliesChanged()
		{
			if (this.AssembliesChanged != null)
			{
				this.AssembliesChanged(this, System.EventArgs.Empty);
			}
		}
	}
}
