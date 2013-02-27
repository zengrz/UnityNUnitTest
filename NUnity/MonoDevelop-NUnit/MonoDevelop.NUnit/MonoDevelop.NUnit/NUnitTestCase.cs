using MonoDevelop.Core.Execution;
using MonoDevelop.NUnit.External;
using System;
namespace MonoDevelop.NUnit
{
	internal class NUnitTestCase : UnitTest
	{
		private NUnitAssemblyTestSuite rootSuite;
		private string className;
		private string pathName;
		public string ClassName
		{
			get
			{
				return this.className;
			}
		}
		public override SourceCodeLocation SourceCodeLocation
		{
			get
			{
				SourceCodeLocation result;
				for (UnitTest p = base.Parent; p != null; p = p.Parent)
				{
					NUnitAssemblyTestSuite root = p as NUnitAssemblyTestSuite;
					if (root != null)
					{
						result = root.GetSourceCodeLocation(this);
						return result;
					}
				}
				result = null;
				return result;
			}
		}
		public NUnitTestCase(NUnitAssemblyTestSuite rootSuite, NunitTestInfo tinfo, string className) : base(tinfo.Name)
		{
			this.className = className;
			this.pathName = tinfo.PathName;
			this.rootSuite = rootSuite;
		}
		protected override UnitTestResult OnRun(TestContext testContext)
		{
			return this.rootSuite.RunUnitTest(this, this.className, this.pathName, this.Name, testContext);
		}
		protected override bool OnCanRun(IExecutionHandler executionContext)
		{
			return this.rootSuite.CanRun(executionContext);
		}
	}
}
