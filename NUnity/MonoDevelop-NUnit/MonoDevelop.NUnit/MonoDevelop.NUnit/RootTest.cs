using MonoDevelop.Projects;
using System;
using System.IO;
namespace MonoDevelop.NUnit
{
	internal class RootTest : UnitTestGroup
	{
		private NUnitAssemblyGroupProject project;
		private string resultsPath;
		private NUnitAssemblyGroupProjectConfiguration lastConfig;
		internal string ResultsPath
		{
			get
			{
				return this.resultsPath;
			}
		}
		public override bool HasTests
		{
			get
			{
				return true;
			}
		}
		public RootTest(NUnitAssemblyGroupProject project) : base(project.get_Name(), project)
		{
			this.project = project;
			this.resultsPath = System.IO.Path.Combine(project.get_BaseDirectory(), "test-results");
			base.ResultsStore = new XmlResultsStore(this.resultsPath, System.IO.Path.GetFileName(project.get_FileName()));
			this.lastConfig = (NUnitAssemblyGroupProjectConfiguration)project.get_DefaultConfiguration();
			if (this.lastConfig != null)
			{
				this.lastConfig.AssembliesChanged += new System.EventHandler(this.OnAssembliesChanged);
			}
		}
		public override void Dispose()
		{
			if (this.lastConfig != null)
			{
				this.lastConfig.AssembliesChanged -= new System.EventHandler(this.OnAssembliesChanged);
			}
		}
		private void OnAssembliesChanged(object sender, System.EventArgs args)
		{
			base.UpdateTests();
		}
		protected override void OnActiveConfigurationChanged()
		{
			if (this.lastConfig != null)
			{
				this.lastConfig.AssembliesChanged -= new System.EventHandler(this.OnAssembliesChanged);
			}
			this.lastConfig = (NUnitAssemblyGroupProjectConfiguration)this.project.get_DefaultConfiguration();
			if (this.lastConfig != null)
			{
				this.lastConfig.AssembliesChanged += new System.EventHandler(this.OnAssembliesChanged);
			}
			base.UpdateTests();
			base.OnActiveConfigurationChanged();
		}
		protected override void OnCreateTests()
		{
			NUnitAssemblyGroupProjectConfiguration conf = (NUnitAssemblyGroupProjectConfiguration)this.project.GetConfiguration((ItemConfigurationSelector)this.ActiveConfiguration);
			if (conf != null)
			{
				foreach (TestAssembly t in conf.Assemblies)
				{
					base.Tests.Add(t);
				}
			}
		}
	}
}
