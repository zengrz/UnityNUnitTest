using MonoDevelop.Core.Execution;
using MonoDevelop.NUnit.External;
using System;
namespace MonoDevelop.NUnit
{
	internal class NUnitTestSuite : UnitTestGroup
	{
		private NunitTestInfo testInfo;
		private NUnitAssemblyTestSuite rootSuite;
		private string fullName;
		public override bool HasTests
		{
			get
			{
				return true;
			}
		}
		public string ClassName
		{
			get
			{
				return this.fullName;
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
		public NUnitTestSuite(NUnitAssemblyTestSuite rootSuite, NunitTestInfo tinfo) : base(tinfo.Name)
		{
			this.fullName = ((!string.IsNullOrEmpty(tinfo.PathName)) ? (tinfo.PathName + "." + tinfo.Name) : tinfo.Name);
			this.testInfo = tinfo;
			this.rootSuite = rootSuite;
		}
		protected override UnitTestResult OnRun(TestContext testContext)
		{
			return this.rootSuite.RunUnitTest(this, this.fullName, this.fullName, null, testContext);
		}
		protected override bool OnCanRun(IExecutionHandler executionContext)
		{
			return this.rootSuite.CanRun(executionContext);
		}
		protected override void OnCreateTests()
		{
			if (this.testInfo.Tests != null)
			{
				NunitTestInfo[] tests = this.testInfo.Tests;
				for (int i = 0; i < tests.Length; i++)
				{
					NunitTestInfo test = tests[i];
					if (test.Tests != null)
					{
						base.Tests.Add(new NUnitTestSuite(this.rootSuite, test));
					}
					else
					{
						base.Tests.Add(new NUnitTestCase(this.rootSuite, test, this.ClassName));
					}
				}
			}
		}
	}
}
