using MonoDevelop.Core.Execution;
using NUnit.Core;
using NUnit.Framework;
using NUnit.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
namespace MonoDevelop.NUnit.External
{
	internal class ExternalTestRunner : RemoteProcessObject
	{
		private NUnitTestRunner runner;
		public ExternalTestRunner()
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
			string asm = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(base.GetType().Assembly.Location), "NUnitRunner.dll");
			System.Reflection.Assembly.LoadFrom(asm);
		}
		public UnitTestResult Run(IRemoteEventListener listener, ITestFilter filter, string path, string suiteName, System.Collections.Generic.List<string> supportAssemblies)
		{
			NUnitTestRunner runner = this.GetRunner(path);
			EventListenerWrapper listenerWrapper = (listener != null) ? new EventListenerWrapper(listener) : null;
			TestResult res = runner.Run(listenerWrapper, filter, path, suiteName, supportAssemblies);
			return listenerWrapper.GetLocalTestResult(res);
		}
		public NunitTestInfo GetTestInfo(string path, System.Collections.Generic.List<string> supportAssemblies)
		{
			NUnitTestRunner runner = this.GetRunner(path);
			return runner.GetTestInfo(path, supportAssemblies);
		}
		private NUnitTestRunner GetRunner(string assemblyPath)
		{
			TestPackage package = new TestPackage(assemblyPath);
			package.Settings["ShadowCopyFiles"] = false;
			DomainManager dm = new DomainManager();
			System.AppDomain domain = dm.CreateDomain(package);
			string asm = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(base.GetType().Assembly.Location), "NUnitRunner.dll");
			this.runner = (NUnitTestRunner)domain.CreateInstanceFromAndUnwrap(asm, "MonoDevelop.NUnit.External.NUnitTestRunner");
			this.runner.Initialize(typeof(Assert).Assembly.Location, typeof(Test).Assembly.Location);
			return this.runner;
		}
	}
}
