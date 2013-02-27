using NUnit.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
namespace MonoDevelop.NUnit.External
{
	public class NUnitTestRunner : System.MarshalByRefObject
	{
		public void Initialize(string nunitPath, string nunitCorePath)
		{
			System.AppDomain.CurrentDomain.AssemblyResolve += delegate(object s, System.ResolveEventArgs args)
			{
				System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
				System.Reflection.Assembly result;
				for (int i = 0; i < assemblies.Length; i++)
				{
					System.Reflection.Assembly am = assemblies[i];
					if (am.GetName().FullName == args.Name)
					{
						result = am;
						return result;
					}
				}
				result = null;
				return result;
			}
			;
			System.Reflection.Assembly.LoadFrom(nunitCorePath);
			System.Reflection.Assembly.LoadFrom(nunitPath);
			if (!CoreExtensions.Host.Initialized)
			{
				CoreExtensions.Host.InitializeService();
			}
		}
		public TestResult Run(EventListener listener, ITestFilter filter, string path, string suiteName, System.Collections.Generic.List<string> supportAssemblies)
		{
			this.InitSupportAssemblies(supportAssemblies);
			if (filter == null)
			{
				filter = TestFilter.Empty;
			}
			RemoteTestRunner tr = new RemoteTestRunner();
			TestPackage package = new TestPackage(path);
			if (!string.IsNullOrEmpty(suiteName))
			{
				package.TestName = suiteName;
			}
			tr.Load(package);
			return tr.Run(listener, filter);
		}
		public NunitTestInfo GetTestInfo(string path, System.Collections.Generic.List<string> supportAssemblies)
		{
			this.InitSupportAssemblies(supportAssemblies);
			TestSuite rootTS = new TestSuiteBuilder().Build(new TestPackage(path));
			return this.BuildTestInfo(rootTS);
		}
		private NunitTestInfo BuildTestInfo(Test test)
		{
			NunitTestInfo ti = new NunitTestInfo();
			string tname = test.TestName.Name;
			int i = tname.LastIndexOf('.');
			if (i != -1)
			{
				tname = tname.Substring(i + 1);
			}
			ti.Name = tname;
			string testNameWithDelimiter = "." + tname;
			if (test.TestName.FullName.EndsWith(testNameWithDelimiter))
			{
				int pathLength = test.TestName.FullName.Length - testNameWithDelimiter.Length;
				ti.PathName = test.TestName.FullName.Substring(0, pathLength);
			}
			else
			{
				ti.PathName = null;
			}
			if (test.Tests != null && test.Tests.Count > 0)
			{
				ti.Tests = new NunitTestInfo[test.Tests.Count];
				for (int j = 0; j < test.Tests.Count; j++)
				{
					ti.Tests[j] = this.BuildTestInfo((Test)test.Tests[j]);
				}
			}
			return ti;
		}
		private void InitSupportAssemblies(System.Collections.Generic.List<string> supportAssemblies)
		{
			foreach (string asm in supportAssemblies)
			{
				System.Reflection.Assembly.LoadFrom(asm);
			}
		}
		public override object InitializeLifetimeService()
		{
			return null;
		}
	}
}
