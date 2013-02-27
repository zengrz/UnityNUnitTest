using Gtk;
using Mono.Addins;
using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Commands;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Projects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
namespace MonoDevelop.NUnit
{
	public class NUnitService
	{
		private static NUnitService instance;
		private System.Collections.ArrayList providers = new System.Collections.ArrayList();
		private UnitTest[] rootTests;
		public event System.EventHandler TestSuiteChanged;
		public static NUnitService Instance
		{
			get
			{
				if (NUnitService.instance == null)
				{
					NUnitService.instance = new NUnitService();
					NUnitService.instance.RebuildTests();
				}
				return NUnitService.instance;
			}
		}
		public UnitTest[] RootTests
		{
			get
			{
				return this.rootTests;
			}
		}
		private NUnitService()
		{
			IdeApp.get_Workspace().add_ReferenceAddedToProject(new ProjectReferenceEventHandler(this.OnWorkspaceChanged));
			IdeApp.get_Workspace().add_ReferenceRemovedFromProject(new ProjectReferenceEventHandler(this.OnWorkspaceChanged));
			IdeApp.get_Workspace().add_WorkspaceItemOpened(new System.EventHandler<WorkspaceItemEventArgs>(this.OnWorkspaceChanged));
			IdeApp.get_Workspace().add_WorkspaceItemClosed(new System.EventHandler<WorkspaceItemEventArgs>(this.OnWorkspaceChanged));
			IdeApp.get_Workspace().add_ItemAddedToSolution(new SolutionItemChangeEventHandler(this.OnWorkspaceChanged));
			IdeApp.get_Workspace().add_ItemRemovedFromSolution(new SolutionItemChangeEventHandler(this.OnWorkspaceChanged));
			ProjectService ps = Services.get_ProjectService();
			ps.get_DataContext().IncludeType(typeof(UnitTestOptionsSet));
			ps.get_DataContext().RegisterProperty(typeof(SolutionItemConfiguration), "UnitTestInformation", typeof(UnitTestOptionsSet));
			AddinManager.AddExtensionNodeHandler("/MonoDevelop/NUnit/TestProviders", new ExtensionNodeEventHandler(this.OnExtensionChange));
		}
		private void OnExtensionChange(object s, ExtensionNodeEventArgs args)
		{
			if (args.get_Change() == 0)
			{
				ProjectService ps = Services.get_ProjectService();
				ITestProvider provider = args.get_ExtensionObject() as ITestProvider;
				this.providers.Add(provider);
				System.Type[] types = provider.GetOptionTypes();
				if (types != null)
				{
					System.Type[] array = types;
					for (int i = 0; i < array.Length; i++)
					{
						System.Type t = array[i];
						if (!typeof(System.ICloneable).IsAssignableFrom(t))
						{
							LoggingService.LogError("Option types must implement ICloneable: " + t);
						}
						else
						{
							ps.get_DataContext().IncludeType(t);
						}
					}
				}
			}
			else
			{
				ITestProvider provider = args.get_ExtensionObject() as ITestProvider;
				this.providers.Remove(provider);
			}
		}
		public IAsyncOperation RunTest(UnitTest test, IExecutionHandler context)
		{
			return this.RunTest(test, context, IdeApp.get_Preferences().get_BuildBeforeExecuting());
		}
		public IAsyncOperation RunTest(UnitTest test, IExecutionHandler context, bool buildOwnerObject)
		{
			string testName = test.FullName;
			IAsyncOperation result;
			if (buildOwnerObject)
			{
				IBuildTarget bt = test.OwnerObject as IBuildTarget;
				if (bt != null && bt.NeedsBuilding(IdeApp.get_Workspace().get_ActiveConfiguration()))
				{
					if (!IdeApp.get_ProjectOperations().get_CurrentRunOperation().get_IsCompleted())
					{
						StopHandler.StopBuildOperations();
						IdeApp.get_ProjectOperations().get_CurrentRunOperation().WaitForCompleted();
					}
					AsyncOperation retOper = new AsyncOperation();
					IAsyncOperation op = IdeApp.get_ProjectOperations().Build(bt);
					retOper.TrackOperation(op, false);
					op.add_Completed(delegate
					{
						System.Threading.ThreadPool.QueueUserWorkItem(delegate
						{
							if (op.get_Success())
							{
								this.RefreshTests();
								test = this.SearchTest(testName);
								if (test != null)
								{
									Application.Invoke(delegate
									{
										retOper.TrackOperation(this.RunTest(test, context, false), true);
									}
									);
								}
								else
								{
									retOper.SetCompleted(false);
								}
							}
						}
						);
					}
					);
					result = retOper;
					return result;
				}
			}
			if (!IdeApp.get_ProjectOperations().ConfirmExecutionOperation())
			{
				result = NullProcessAsyncOperation.Failure;
			}
			else
			{
				Pad resultsPad = IdeApp.get_Workbench().GetPad<TestResultsPad>();
				if (resultsPad == null)
				{
					resultsPad = IdeApp.get_Workbench().ShowPad(new TestResultsPad(), "MonoDevelop.NUnit.TestResultsPad", GettextCatalog.GetString("Test results"), "Bottom", "md-solution");
				}
				resultsPad.set_Sticky(true);
				resultsPad.BringToFront();
				TestSession session = new TestSession(test, context, (TestResultsPad)resultsPad.get_Content());
				session.Completed += delegate
				{
					Application.Invoke(delegate
					{
						resultsPad.set_Sticky(false);
					}
					);
				}
				;
				session.Start();
				IdeApp.get_ProjectOperations().set_CurrentRunOperation(session);
				result = session;
			}
			return result;
		}
		public void RefreshTests()
		{
			UnitTest[] array = this.RootTests;
			for (int i = 0; i < array.Length; i++)
			{
				UnitTest t = array[i];
				t.Refresh().WaitForCompleted();
			}
		}
		public UnitTest SearchTest(string fullName)
		{
			UnitTest[] array = this.RootTests;
			UnitTest result;
			for (int i = 0; i < array.Length; i++)
			{
				UnitTest t = array[i];
				UnitTest r = this.SearchTest(t, fullName);
				if (r != null)
				{
					result = r;
					return result;
				}
			}
			result = null;
			return result;
		}
		private UnitTest SearchTest(UnitTest test, string fullName)
		{
			UnitTest result2;
			if (test == null)
			{
				result2 = null;
			}
			else
			{
				if (test.FullName == fullName)
				{
					result2 = test;
				}
				else
				{
					UnitTestGroup group = test as UnitTestGroup;
					if (group != null)
					{
						foreach (UnitTest t in group.Tests)
						{
							UnitTest result = this.SearchTest(t, fullName);
							if (result != null)
							{
								result2 = result;
								return result2;
							}
						}
					}
					result2 = null;
				}
			}
			return result2;
		}
		public UnitTest FindRootTest(IWorkspaceObject item)
		{
			return this.FindRootTest(this.RootTests, item);
		}
		public UnitTest FindRootTest(System.Collections.Generic.IEnumerable<UnitTest> tests, IWorkspaceObject item)
		{
			UnitTest result;
			foreach (UnitTest t in tests)
			{
				if (t.OwnerObject == item)
				{
					result = t;
					return result;
				}
				UnitTestGroup tg = t as UnitTestGroup;
				if (tg != null)
				{
					UnitTest ct = this.FindRootTest(tg.Tests, item);
					if (ct != null)
					{
						result = ct;
						return result;
					}
				}
			}
			result = null;
			return result;
		}
		private void OnWorkspaceChanged(object sender, System.EventArgs e)
		{
			this.RebuildTests();
		}
		private void RebuildTests()
		{
			if (this.rootTests != null)
			{
				UnitTest[] array = this.rootTests;
				for (int i = 0; i < array.Length; i++)
				{
					System.IDisposable t = array[i];
					t.Dispose();
				}
			}
			System.Collections.Generic.List<UnitTest> list = new System.Collections.Generic.List<UnitTest>();
			foreach (WorkspaceItem it in IdeApp.get_Workspace().get_Items())
			{
				UnitTest t2 = this.BuildTest(it);
				if (t2 != null)
				{
					list.Add(t2);
				}
			}
			this.rootTests = list.ToArray();
			this.NotifyTestSuiteChanged();
		}
		public UnitTest BuildTest(IWorkspaceObject entry)
		{
			UnitTest result;
			foreach (ITestProvider p in this.providers)
			{
				UnitTest t = p.CreateUnitTest(entry);
				if (t != null)
				{
					result = t;
					return result;
				}
			}
			result = null;
			return result;
		}
		public static void ShowOptionsDialog(UnitTest test)
		{
			Properties properties = new Properties();
			properties.Set("UnitTest", test);
			MessageService.ShowCustomDialog(new UnitTestOptionsDialog(IdeApp.get_Workbench().get_RootWindow(), properties));
		}
		private void NotifyTestSuiteChanged()
		{
			if (this.TestSuiteChanged != null)
			{
				this.TestSuiteChanged(this, System.EventArgs.Empty);
			}
		}
	}
}
