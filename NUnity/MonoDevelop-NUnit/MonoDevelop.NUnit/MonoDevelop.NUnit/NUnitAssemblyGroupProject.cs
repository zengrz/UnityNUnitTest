using MonoDevelop.Core;
using MonoDevelop.Core.Serialization;
using MonoDevelop.Projects;
using System;
using System.Xml;
namespace MonoDevelop.NUnit
{
	[DataInclude(typeof(NUnitAssemblyGroupProjectConfiguration))]
	public class NUnitAssemblyGroupProject : SolutionEntityItem
	{
		private RootTest rootTest;
		public UnitTest RootTest
		{
			get
			{
				if (this.rootTest == null)
				{
					this.rootTest = new RootTest(this);
				}
				return this.rootTest;
			}
		}
		protected override void OnEndLoad()
		{
			base.OnEndLoad();
			if (base.get_Configurations().Count == 0)
			{
				base.get_Configurations().Add(this.CreateConfiguration("Default"));
			}
		}
		public override void InitializeFromTemplate(XmlElement element)
		{
			base.get_Configurations().Add(this.CreateConfiguration("Default"));
		}
		public override SolutionItemConfiguration CreateConfiguration(string name)
		{
			NUnitAssemblyGroupProjectConfiguration conf = new NUnitAssemblyGroupProjectConfiguration();
			conf.set_Name(name);
			return conf;
		}
		protected override void OnClean(IProgressMonitor monitor, ConfigurationSelector configuration)
		{
		}
		protected override BuildResult OnBuild(IProgressMonitor monitor, ConfigurationSelector configuration)
		{
			return null;
		}
		protected override void OnExecute(IProgressMonitor monitor, ExecutionContext context, ConfigurationSelector configuration)
		{
		}
		protected override bool OnGetNeedsBuilding(ConfigurationSelector configuration)
		{
			return false;
		}
		protected override void OnSetNeedsBuilding(bool value, ConfigurationSelector configuration)
		{
		}
	}
}
