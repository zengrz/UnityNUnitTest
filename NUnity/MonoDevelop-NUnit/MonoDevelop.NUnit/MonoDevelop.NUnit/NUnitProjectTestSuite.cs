using MonoDevelop.Ide;
using MonoDevelop.Projects;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Parser;
using System;
using System.Collections.Generic;
using System.IO;
namespace MonoDevelop.NUnit
{
	public class NUnitProjectTestSuite : NUnitAssemblyTestSuite
	{
		private DotNetProject project;
		private string resultsPath;
		private string storeId;
		protected override string AssemblyPath
		{
			get
			{
				return this.project.GetOutputFileName(IdeApp.get_Workspace().get_ActiveConfiguration());
			}
		}
		protected override string TestInfoCachePath
		{
			get
			{
				return System.IO.Path.Combine(this.resultsPath, this.storeId + ".test-cache");
			}
		}
		protected override System.Collections.Generic.IEnumerable<string> SupportAssemblies
		{
			get
			{
				DotNetProject dotNetProject = base.OwnerSolutionItem as DotNetProject;
				if (dotNetProject != null)
				{
					foreach (ProjectReference current in dotNetProject.get_References())
					{
						if (current.get_ReferenceType() != 2 && !current.get_LocalCopy())
						{
							try
							{
								string[] referencedFileNames = current.GetReferencedFileNames(IdeApp.get_Workspace().get_ActiveConfiguration());
								for (int i = 0; i < referencedFileNames.Length; i++)
								{
									string text = referencedFileNames[i];
									yield return text;
								}
							}
							finally
							{
							}
						}
					}
				}
				yield break;
			}
		}
		public NUnitProjectTestSuite(DotNetProject project) : base(project.get_Name(), project)
		{
			this.storeId = System.IO.Path.GetFileName(project.get_FileName());
			this.resultsPath = System.IO.Path.Combine(project.get_BaseDirectory(), "test-results");
			base.ResultsStore = new XmlResultsStore(this.resultsPath, this.storeId);
			this.project = project;
			project.add_NameChanged(new SolutionItemRenamedEventHandler(this.OnProjectRenamed));
			IdeApp.get_ProjectOperations().add_EndBuild(new BuildEventHandler(this.OnProjectBuilt));
		}
		public static NUnitProjectTestSuite CreateTest(DotNetProject project)
		{
			NUnitProjectTestSuite result;
			foreach (ProjectReference p in project.get_References())
			{
				if (p.get_Reference().IndexOf("nunit.framework", System.StringComparison.OrdinalIgnoreCase) != -1 || p.get_Reference().IndexOf("nunit.core", System.StringComparison.OrdinalIgnoreCase) != -1)
				{
					result = new NUnitProjectTestSuite(project);
					return result;
				}
			}
			result = null;
			return result;
		}
		protected override SourceCodeLocation GetSourceCodeLocation(string fullClassName, string methodName)
		{
			ProjectDom ctx = ProjectDomService.GetProjectDom(this.project);
			IType cls = ctx.GetType(fullClassName);
			SourceCodeLocation result;
			if (cls == null)
			{
				result = null;
			}
			else
			{
				DomLocation location;
				foreach (IMethod met in cls.get_Methods())
				{
					if (met.get_Name() == methodName)
					{
						string arg_85_0 = cls.get_CompilationUnit().get_FileName();
						location = met.get_Location();
						int arg_85_1 = location.get_Line();
						location = met.get_Location();
						result = new SourceCodeLocation(arg_85_0, arg_85_1, location.get_Column());
						return result;
					}
				}
				string arg_E0_0 = cls.get_CompilationUnit().get_FileName();
				location = cls.get_Location();
				int arg_E0_1 = location.get_Line();
				location = cls.get_Location();
				result = new SourceCodeLocation(arg_E0_0, arg_E0_1, location.get_Column());
			}
			return result;
		}
		public override void Dispose()
		{
			this.project.remove_NameChanged(new SolutionItemRenamedEventHandler(this.OnProjectRenamed));
			IdeApp.get_ProjectOperations().remove_EndBuild(new BuildEventHandler(this.OnProjectBuilt));
			base.Dispose();
		}
		private void OnProjectRenamed(object sender, SolutionItemRenamedEventArgs e)
		{
			UnitTestGroup parent = base.Parent as UnitTestGroup;
			if (parent != null)
			{
				parent.UpdateTests();
			}
		}
		private void OnProjectBuilt(object s, BuildEventArgs args)
		{
			if (base.RefreshRequired)
			{
				base.UpdateTests();
			}
		}
	}
}
