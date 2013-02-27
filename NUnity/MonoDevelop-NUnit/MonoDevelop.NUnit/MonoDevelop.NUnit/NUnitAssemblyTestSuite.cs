using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using MonoDevelop.Ide;
using MonoDevelop.NUnit.External;
using MonoDevelop.Projects;
using NUnit.Core;
using NUnit.Core.Filters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
namespace MonoDevelop.NUnit
{
	public abstract class NUnitAssemblyTestSuite : UnitTestGroup
	{
		private class LoadData
		{
			public string Path;
			public string TestInfoCachePath;
			public System.Exception Error;
			public NunitTestInfo Info;
			public NUnitAssemblyTestSuite.TestInfoCache InfoCache;
			public System.Threading.WaitCallback Callback;
			public System.Collections.Generic.List<string> SupportAssemblies;
		}
		private class RunData
		{
			public ExternalTestRunner Runner;
			public UnitTest Test;
			public LocalTestMonitor LocalMonitor;
			public void Cancel()
			{
				this.LocalMonitor.Canceled = true;
				this.Runner.Shutdown();
				this.ClearRunningStatus(this.Test);
			}
			private void ClearRunningStatus(UnitTest t)
			{
				t.Status = TestStatus.Ready;
				UnitTestGroup group = t as UnitTestGroup;
				if (group != null)
				{
					foreach (UnitTest ct in group.Tests)
					{
						this.ClearRunningStatus(ct);
					}
				}
			}
		}
		[System.Serializable]
		private class TestInfoCache
		{
			private System.Collections.Hashtable table = new System.Collections.Hashtable();
			[System.NonSerialized]
			private bool modified;
			public void SetInfo(string path, NunitTestInfo info)
			{
				if (System.IO.File.Exists(path))
				{
					NUnitAssemblyTestSuite.CachedTestInfo cti = new NUnitAssemblyTestSuite.CachedTestInfo();
					cti.LastWriteTime = System.IO.File.GetLastWriteTime(path);
					cti.Info = info;
					this.table[path] = cti;
					this.modified = true;
				}
			}
			public NunitTestInfo GetInfo(string path)
			{
				NUnitAssemblyTestSuite.CachedTestInfo cti = (NUnitAssemblyTestSuite.CachedTestInfo)this.table[path];
				NunitTestInfo result;
				if (cti != null && System.IO.File.Exists(path) && System.IO.File.GetLastWriteTime(path) == cti.LastWriteTime)
				{
					result = cti.Info;
				}
				else
				{
					result = null;
				}
				return result;
			}
			public static NUnitAssemblyTestSuite.TestInfoCache Read(string file)
			{
				System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
				System.IO.Stream s = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				NUnitAssemblyTestSuite.TestInfoCache result;
				try
				{
					result = (NUnitAssemblyTestSuite.TestInfoCache)bf.Deserialize(s);
				}
				finally
				{
					s.Close();
				}
				return result;
			}
			public void Write(string file)
			{
				if (this.modified)
				{
					System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
					System.IO.Stream s = new System.IO.FileStream(file, System.IO.FileMode.Create, System.IO.FileAccess.Write);
					try
					{
						bf.Serialize(s, this);
					}
					finally
					{
						s.Close();
					}
				}
			}
		}
		[System.Serializable]
		private class CachedTestInfo
		{
			public System.DateTime LastWriteTime;
			public NunitTestInfo Info;
		}
		private object locker = new object();
		private UnitTest[] oldList;
		private NUnitAssemblyTestSuite.TestInfoCache testInfoCache = new NUnitAssemblyTestSuite.TestInfoCache();
		private bool cacheLoaded;
		private System.DateTime lastAssemblyTime;
		private static Queue<NUnitAssemblyTestSuite.LoadData> loadQueue = new Queue<NUnitAssemblyTestSuite.LoadData>();
		private static bool loaderRunning;
		public override bool HasTests
		{
			get
			{
				return true;
			}
		}
		protected bool RefreshRequired
		{
			get
			{
				return this.lastAssemblyTime != this.GetAssemblyTime();
			}
		}
		protected abstract string AssemblyPath
		{
			get;
		}
		protected virtual System.Collections.Generic.IEnumerable<string> SupportAssemblies
		{
			get
			{
				yield break;
			}
		}
		protected virtual string TestInfoCachePath
		{
			get
			{
				return null;
			}
		}
		public NUnitAssemblyTestSuite(string name) : base(name)
		{
		}
		public NUnitAssemblyTestSuite(string name, SolutionItem ownerSolutionItem) : base(name, ownerSolutionItem)
		{
		}
		public override void Dispose()
		{
			try
			{
				if (this.TestInfoCachePath != null)
				{
					System.Console.WriteLine("saving TestInfoCachePath = " + this.TestInfoCachePath);
					this.testInfoCache.Write(this.TestInfoCachePath);
				}
			}
			catch
			{
			}
			base.Dispose();
		}
		protected override void OnActiveConfigurationChanged()
		{
			base.UpdateTests();
			base.OnActiveConfigurationChanged();
		}
		internal SourceCodeLocation GetSourceCodeLocation(UnitTest test)
		{
			SourceCodeLocation result;
			if (test is NUnitTestCase)
			{
				NUnitTestCase t = (NUnitTestCase)test;
				result = this.GetSourceCodeLocation(t.ClassName, t.Name);
			}
			else
			{
				if (test is NUnitTestSuite)
				{
					NUnitTestSuite t2 = (NUnitTestSuite)test;
					result = this.GetSourceCodeLocation(t2.ClassName, null);
				}
				else
				{
					result = null;
				}
			}
			return result;
		}
		protected virtual SourceCodeLocation GetSourceCodeLocation(string fullClassName, string methodName)
		{
			return null;
		}
		public override int CountTestCases()
		{
			lock (this.locker)
			{
				if (base.Status == TestStatus.Loading)
				{
					System.Threading.Monitor.Wait(this.locker, 10000);
				}
			}
			return base.CountTestCases();
		}
		public override IAsyncOperation Refresh()
		{
			AsyncOperation oper = new AsyncOperation();
			System.Threading.ThreadPool.QueueUserWorkItem(delegate
			{
				lock (this.locker)
				{
					try
					{
						while (this.Status == TestStatus.Loading)
						{
							System.Threading.Monitor.Wait(this.locker);
						}
						if (this.RefreshRequired)
						{
							this.lastAssemblyTime = this.GetAssemblyTime();
							this.UpdateTests();
							this.OnCreateTests();
							while (this.Status == TestStatus.Loading)
							{
								System.Threading.Monitor.Wait(this.locker);
							}
						}
						oper.SetCompleted(true);
					}
					catch
					{
						oper.SetCompleted(false);
					}
				}
			}
			);
			return oper;
		}
		private System.DateTime GetAssemblyTime()
		{
			string path = this.AssemblyPath;
			System.DateTime result;
			if (System.IO.File.Exists(path))
			{
				result = System.IO.File.GetLastWriteTime(path);
			}
			else
			{
				result = System.DateTime.MinValue;
			}
			return result;
		}
		protected override void OnCreateTests()
		{
			lock (this.locker)
			{
				if (base.Status == TestStatus.Loading)
				{
					return;
				}
				NunitTestInfo ti = this.testInfoCache.GetInfo(this.AssemblyPath);
				if (ti != null)
				{
					this.FillTests(ti);
					return;
				}
				base.Status = TestStatus.Loading;
			}
			this.lastAssemblyTime = this.GetAssemblyTime();
			if (this.oldList != null)
			{
				UnitTest[] array = this.oldList;
				for (int i = 0; i < array.Length; i++)
				{
					UnitTest t = array[i];
					base.Tests.Add(t);
				}
			}
			this.OnTestStatusChanged();
			NUnitAssemblyTestSuite.LoadData ld = new NUnitAssemblyTestSuite.LoadData();
			ld.Path = this.AssemblyPath;
			ld.TestInfoCachePath = (this.cacheLoaded ? null : this.TestInfoCachePath);
			ld.Callback = delegate
			{
				DispatchService.GuiDispatch(delegate
				{
					this.AsyncCreateTests(ld);
				}
				);
			}
			;
			ld.SupportAssemblies = new System.Collections.Generic.List<string>(this.SupportAssemblies);
			NUnitAssemblyTestSuite.AsyncLoadTest(ld);
			this.cacheLoaded = true;
		}
		private void AsyncCreateTests(object ob)
		{
			TestStatus newStatus = TestStatus.Ready;
			try
			{
				NUnitAssemblyTestSuite.LoadData loadData = (NUnitAssemblyTestSuite.LoadData)ob;
				if (loadData.Error != null)
				{
					newStatus = TestStatus.LoadError;
				}
				else
				{
					base.Tests.Clear();
					if (loadData.Info == null)
					{
						this.oldList = new UnitTest[0];
					}
					else
					{
						this.FillTests(loadData.Info);
						if (loadData.InfoCache != null)
						{
							this.testInfoCache = loadData.InfoCache;
						}
						this.testInfoCache.SetInfo(this.AssemblyPath, loadData.Info);
					}
				}
			}
			catch (System.Exception ex)
			{
				LoggingService.LogError(ex.ToString());
				newStatus = TestStatus.LoadError;
			}
			finally
			{
				lock (this.locker)
				{
					base.Status = newStatus;
					System.Threading.Monitor.PulseAll(this.locker);
				}
				this.OnTestChanged();
			}
		}
		private void FillTests(NunitTestInfo ti)
		{
			if (ti.Tests != null)
			{
				NunitTestInfo[] tests = ti.Tests;
				for (int i = 0; i < tests.Length; i++)
				{
					NunitTestInfo test = tests[i];
					if (test.Tests != null)
					{
						base.Tests.Add(new NUnitTestSuite(this, test));
					}
					else
					{
						base.Tests.Add(new NUnitTestCase(this, test, test.PathName));
					}
				}
				this.oldList = new UnitTest[base.Tests.Count];
				base.Tests.CopyTo(this.oldList, 0);
			}
		}
		private static void AsyncLoadTest(NUnitAssemblyTestSuite.LoadData ld)
		{
			lock (NUnitAssemblyTestSuite.loadQueue)
			{
				if (!NUnitAssemblyTestSuite.loaderRunning)
				{
					new System.Threading.Thread(new System.Threading.ThreadStart(NUnitAssemblyTestSuite.RunAsyncLoadTest))
					{
						Name = "NUnit test loader", 
						IsBackground = true
					}.Start();
					NUnitAssemblyTestSuite.loaderRunning = true;
				}
				NUnitAssemblyTestSuite.loadQueue.Enqueue(ld);
				System.Threading.Monitor.Pulse(NUnitAssemblyTestSuite.loadQueue);
			}
		}
		private static void RunAsyncLoadTest()
		{
			while (true)
			{
				NUnitAssemblyTestSuite.LoadData ld;
				lock (NUnitAssemblyTestSuite.loadQueue)
				{
					if (NUnitAssemblyTestSuite.loadQueue.Count == 0)
					{
						if (!System.Threading.Monitor.Wait(NUnitAssemblyTestSuite.loadQueue, 5000, true))
						{
							NUnitAssemblyTestSuite.loaderRunning = false;
							break;
						}
					}
					ld = NUnitAssemblyTestSuite.loadQueue.Dequeue();
				}
				try
				{
					if (ld.TestInfoCachePath != null && System.IO.File.Exists(ld.TestInfoCachePath))
					{
						ld.InfoCache = NUnitAssemblyTestSuite.TestInfoCache.Read(ld.TestInfoCachePath);
						NunitTestInfo info = ld.InfoCache.GetInfo(ld.Path);
						if (info != null)
						{
							ld.Info = info;
							ld.Callback(ld);
							continue;
						}
					}
				}
				catch (System.Exception ex)
				{
					LoggingService.LogError(ex.ToString());
				}
				ExternalTestRunner runner = null;
				try
				{
					if (System.IO.File.Exists(ld.Path))
					{
						runner = (ExternalTestRunner)Runtime.get_ProcessService().CreateExternalProcessObject(typeof(ExternalTestRunner), false);
						ld.Info = runner.GetTestInfo(ld.Path, ld.SupportAssemblies);
					}
				}
				catch (System.Exception ex)
				{
					System.Console.WriteLine(ex);
					ld.Error = ex;
				}
				finally
				{
					try
					{
						if (runner != null)
						{
							runner.Dispose();
						}
					}
					catch
					{
					}
				}
				try
				{
					ld.Callback(ld);
				}
				catch
				{
				}
			}
		}
		protected override UnitTestResult OnRun(TestContext testContext)
		{
			return this.RunUnitTest(this, "", "", null, testContext);
		}
		protected override bool OnCanRun(IExecutionHandler executionContext)
		{
			return Runtime.get_ProcessService().IsValidForRemoteHosting(executionContext);
		}
		internal UnitTestResult RunUnitTest(UnitTest test, string suiteName, string pathName, string testName, TestContext testContext)
		{
			ExternalTestRunner runner = (ExternalTestRunner)Runtime.get_ProcessService().CreateExternalProcessObject(typeof(ExternalTestRunner), testContext.ExecutionContext);
			LocalTestMonitor localMonitor = new LocalTestMonitor(testContext, runner, test, suiteName, testName != null);
			ITestFilter filter = null;
			if (testName != null)
			{
				filter = new TestNameFilter(pathName + "." + testName);
			}
			else
			{
				NUnitCategoryOptions categoryOptions = (NUnitCategoryOptions)test.GetOptions(typeof(NUnitCategoryOptions));
				if (categoryOptions.EnableFilter && categoryOptions.Categories.Count > 0)
				{
					string[] cats = new string[categoryOptions.Categories.Count];
					categoryOptions.Categories.CopyTo(cats, 0);
					filter = new CategoryFilter(cats);
					if (categoryOptions.Exclude)
					{
						filter = new NotFilter(filter);
					}
				}
			}
			NUnitAssemblyTestSuite.RunData rd = new NUnitAssemblyTestSuite.RunData();
			rd.Runner = runner;
			rd.Test = this;
			rd.LocalMonitor = localMonitor;
			testContext.Monitor.CancelRequested += new TestHandler(rd.Cancel);
			UnitTestResult result;
			try
			{
				if (string.IsNullOrEmpty(this.AssemblyPath))
				{
					string msg = GettextCatalog.GetString("Could not get a valid path to the assembly. There may be a conflict in the project configurations.");
					throw new System.Exception(msg);
				}
				System.Runtime.Remoting.RemotingServices.Marshal(localMonitor, null, typeof(IRemoteEventListener));
				result = runner.Run(localMonitor, filter, this.AssemblyPath, suiteName, new System.Collections.Generic.List<string>(this.SupportAssemblies));
				if (testName != null)
				{
					result = localMonitor.SingleTestResult;
				}
			}
			catch (System.Exception ex)
			{
				if (!localMonitor.Canceled)
				{
					LoggingService.LogError(ex.ToString());
					if (localMonitor.RunningTest == null)
					{
						testContext.Monitor.ReportRuntimeError(null, ex);
						throw ex;
					}
					this.RuntimeErrorCleanup(testContext, localMonitor.RunningTest, ex);
					result = UnitTestResult.CreateFailure(ex);
				}
				else
				{
					result = UnitTestResult.CreateFailure(GettextCatalog.GetString("Canceled"), null);
				}
			}
			finally
			{
				testContext.Monitor.CancelRequested -= new TestHandler(rd.Cancel);
				runner.Dispose();
				System.Runtime.Remoting.RemotingServices.Disconnect(localMonitor);
			}
			return result;
		}
		private void RuntimeErrorCleanup(TestContext testContext, UnitTest t, System.Exception ex)
		{
			UnitTestResult result = UnitTestResult.CreateFailure(ex);
			t.RegisterResult(testContext, result);
			while (t != null && t != this)
			{
				testContext.Monitor.EndTest(t, result);
				t.Status = TestStatus.Ready;
				t = t.Parent;
			}
		}
	}
}
