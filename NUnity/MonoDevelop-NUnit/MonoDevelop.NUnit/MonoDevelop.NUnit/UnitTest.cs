using Gtk;
using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using MonoDevelop.Core.ProgressMonitoring;
using MonoDevelop.Core.Serialization;
using MonoDevelop.Projects;
using System;
using System.Collections;
namespace MonoDevelop.NUnit
{
	public abstract class UnitTest : System.IDisposable
	{
		private string name;
		private IResultsStore resultsStore;
		private UnitTestResult lastResult;
		private UnitTest parent;
		private TestStatus status;
		private System.Collections.Hashtable options;
		private IWorkspaceObject ownerSolutionItem;
		private SolutionEntityItem ownerSolutionEntityItem;
		private UnitTestResultsStore results;
		public event System.EventHandler TestChanged;
		public event System.EventHandler TestStatusChanged;
		public virtual string ActiveConfiguration
		{
			get
			{
				string result;
				if (this.ownerSolutionEntityItem != null)
				{
					if (this.ownerSolutionEntityItem.get_DefaultConfiguration() == null)
					{
						result = "";
					}
					else
					{
						result = this.ownerSolutionEntityItem.get_DefaultConfiguration().get_Id();
					}
				}
				else
				{
					if (this.Parent != null)
					{
						result = this.Parent.ActiveConfiguration;
					}
					else
					{
						result = "default";
					}
				}
				return result;
			}
		}
		public UnitTestResultsStore Results
		{
			get
			{
				if (this.results == null)
				{
					this.results = new UnitTestResultsStore(this, this.GetResultsStore());
				}
				return this.results;
			}
		}
		public virtual SourceCodeLocation SourceCodeLocation
		{
			get
			{
				return null;
			}
		}
		public UnitTest Parent
		{
			get
			{
				return this.parent;
			}
		}
		public UnitTest RootTest
		{
			get
			{
				UnitTest result;
				if (this.parent != null)
				{
					result = this.parent.RootTest;
				}
				else
				{
					result = this;
				}
				return result;
			}
		}
		public virtual string Name
		{
			get
			{
				return this.name;
			}
		}
		public virtual string Title
		{
			get
			{
				return this.Name;
			}
		}
		public TestStatus Status
		{
			get
			{
				return this.status;
			}
			set
			{
				this.status = value;
				this.OnTestStatusChanged();
			}
		}
		public string FullName
		{
			get
			{
				string result;
				if (this.parent != null)
				{
					result = this.parent.FullName + "." + this.Name;
				}
				else
				{
					result = this.Name;
				}
				return result;
			}
		}
		protected IWorkspaceObject OwnerSolutionItem
		{
			get
			{
				return this.ownerSolutionItem;
			}
		}
		public IWorkspaceObject OwnerObject
		{
			get
			{
				IWorkspaceObject result;
				if (this.ownerSolutionItem != null)
				{
					result = this.ownerSolutionItem;
				}
				else
				{
					if (this.parent != null)
					{
						result = this.parent.OwnerObject;
					}
					else
					{
						result = null;
					}
				}
				return result;
			}
		}
		internal string StoreRelativeName
		{
			get
			{
				string result;
				if (this.resultsStore != null || this.Parent == null)
				{
					result = "";
				}
				else
				{
					if (this.Parent.resultsStore != null)
					{
						result = this.Name;
					}
					else
					{
						result = this.Parent.StoreRelativeName + "." + this.Name;
					}
				}
				return result;
			}
		}
		protected IResultsStore ResultsStore
		{
			get
			{
				return this.resultsStore;
			}
			set
			{
				this.resultsStore = value;
			}
		}
		protected UnitTest(string name)
		{
			this.name = name;
		}
		protected UnitTest(string name, IWorkspaceObject ownerSolutionItem)
		{
			this.name = name;
			this.ownerSolutionItem = ownerSolutionItem;
			this.ownerSolutionEntityItem = (ownerSolutionItem as SolutionEntityItem);
			if (this.ownerSolutionEntityItem != null)
			{
				this.ownerSolutionEntityItem.add_DefaultConfigurationChanged(new ConfigurationEventHandler(this.OnConfugurationChanged));
			}
		}
		public virtual void Dispose()
		{
			if (this.ownerSolutionEntityItem != null)
			{
				this.ownerSolutionEntityItem.remove_DefaultConfigurationChanged(new ConfigurationEventHandler(this.OnConfugurationChanged));
			}
		}
		internal void SetParent(UnitTest t)
		{
			this.parent = t;
		}
		public virtual string[] GetConfigurations()
		{
			string[] result;
			if (this.ownerSolutionEntityItem != null)
			{
				string[] res = new string[this.ownerSolutionEntityItem.get_Configurations().Count];
				for (int i = 0; i < this.ownerSolutionEntityItem.get_Configurations().Count; i++)
				{
					res[i] = this.ownerSolutionEntityItem.get_Configurations()[i].get_Id();
				}
				result = res;
			}
			else
			{
				if (this.Parent != null)
				{
					result = this.Parent.GetConfigurations();
				}
				else
				{
					result = new string[]
					{
						"default"
					};
				}
			}
			return result;
		}
		public System.ICloneable GetOptions(System.Type optionsType)
		{
			return this.GetOptions(optionsType, this.ActiveConfiguration);
		}
		public bool HasOptions(System.Type optionsType, string configuration)
		{
			return this.GetOptions(optionsType, configuration, false) != null;
		}
		public void ResetOptions(System.Type optionsType, string configuration)
		{
			if (this.GetOptions(optionsType, configuration, false) != null)
			{
				if (this.options != null && this.options.ContainsKey(configuration))
				{
					System.Collections.Hashtable configOptions = (System.Collections.Hashtable)this.options[configuration];
					if (configOptions != null)
					{
						configOptions.Remove(optionsType);
					}
					this.SaveOptions();
				}
			}
		}
		public System.ICloneable GetOptions(System.Type optionsType, string configuration)
		{
			return this.GetOptions(optionsType, configuration, true);
		}
		public System.Collections.ICollection GetAllOptions(string configuration)
		{
			System.Collections.Hashtable localOptions = this.GetOptionsTable(configuration);
			System.Collections.ICollection result;
			if (localOptions == null || localOptions.Count == 0)
			{
				if (this.Parent != null)
				{
					result = this.Parent.GetAllOptions(configuration);
				}
				else
				{
					result = new object[0];
				}
			}
			else
			{
				if (this.Parent == null)
				{
					result = localOptions.Values;
				}
				else
				{
					System.Collections.ICollection parentOptions = this.Parent.GetAllOptions(configuration);
					if (parentOptions.Count == 0)
					{
						result = localOptions.Values;
					}
					else
					{
						System.Collections.Hashtable t = new System.Collections.Hashtable();
						foreach (object ob in parentOptions)
						{
							t[ob.GetType()] = ob;
						}
						foreach (System.ICloneable ob2 in localOptions.Values)
						{
							t[ob2.GetType()] = ob2.Clone();
						}
						result = t.Values;
					}
				}
			}
			return result;
		}
		private System.ICloneable GetOptions(System.Type optionsType, string configuration, bool createDefault)
		{
			System.Collections.Hashtable configOptions = this.GetOptionsTable(configuration);
			System.ICloneable result;
			if (configOptions != null)
			{
				System.ICloneable ob = (System.ICloneable)configOptions[optionsType];
				if (ob != null)
				{
					result = (System.ICloneable)ob.Clone();
					return result;
				}
			}
			if (!createDefault)
			{
				result = null;
			}
			else
			{
				if (this.parent != null)
				{
					result = this.parent.GetOptions(optionsType, configuration);
				}
				else
				{
					result = (System.ICloneable)System.Activator.CreateInstance(optionsType);
				}
			}
			return result;
		}
		private System.Collections.Hashtable GetOptionsTable(string configuration)
		{
			System.Collections.Hashtable configOptions = null;
			if (this.options == null || !this.options.ContainsKey(configuration))
			{
				System.Collections.ICollection col = this.OnLoadOptions(configuration);
				if (col != null && col.Count > 0)
				{
					if (this.options == null)
					{
						this.options = new System.Collections.Hashtable();
					}
					configOptions = (System.Collections.Hashtable)this.options[configuration];
					if (configOptions == null)
					{
						configOptions = new System.Collections.Hashtable();
						this.options[configuration] = configOptions;
					}
					foreach (object op in col)
					{
						configOptions[op.GetType()] = op;
					}
				}
			}
			else
			{
				configOptions = (System.Collections.Hashtable)this.options[configuration];
			}
			return configOptions;
		}
		public virtual void SetOptions(System.ICloneable ops, string configuration)
		{
			if (this.options == null)
			{
				this.options = new System.Collections.Hashtable();
			}
			System.Collections.Hashtable configOptions = (System.Collections.Hashtable)this.options[configuration];
			if (configOptions == null)
			{
				configOptions = new System.Collections.Hashtable();
				this.options[configuration] = configOptions;
			}
			configOptions[ops.GetType()] = ops.Clone();
			this.SaveOptions();
		}
		private void SaveOptions()
		{
			if (this.options == null)
			{
				this.OnSaveOptions(null);
			}
			else
			{
				System.Collections.ArrayList list = new System.Collections.ArrayList();
				foreach (System.Collections.DictionaryEntry e in this.options)
				{
					OptionsData d = new OptionsData((string)e.Key, ((System.Collections.Hashtable)e.Value).Values);
					list.Add(d);
				}
				this.OnSaveOptions((OptionsData[])list.ToArray(typeof(OptionsData)));
			}
		}
		public UnitTestResult GetLastResult()
		{
			return this.lastResult;
		}
		public void ResetLastResult()
		{
			this.lastResult = null;
			this.OnTestStatusChanged();
		}
		public UnitTestCollection GetRegressions(System.DateTime fromDate, System.DateTime toDate)
		{
			UnitTestCollection list = new UnitTestCollection();
			this.FindRegressions(list, fromDate, toDate);
			return list;
		}
		public virtual int CountTestCases()
		{
			return 1;
		}
		public virtual IAsyncOperation Refresh()
		{
			AsyncOperation op = new AsyncOperation();
			op.SetCompleted(true);
			return op;
		}
		public UnitTestResult Run(TestContext testContext)
		{
			testContext.Monitor.BeginTest(this);
			UnitTestResult res = null;
			object ctx = testContext.ContextData;
			try
			{
				this.Status = TestStatus.Running;
				res = this.OnRun(testContext);
			}
			catch (System.Exception ex)
			{
				res = UnitTestResult.CreateFailure(ex);
			}
			finally
			{
				this.Status = TestStatus.Ready;
				testContext.Monitor.EndTest(this, res);
			}
			this.RegisterResult(testContext, res);
			testContext.ContextData = ctx;
			return res;
		}
		public bool CanRun(IExecutionHandler executionContext)
		{
			if (executionContext == null)
			{
				executionContext = Runtime.get_ProcessService().get_DefaultExecutionHandler();
			}
			return this.OnCanRun(executionContext);
		}
		protected abstract UnitTestResult OnRun(TestContext testContext);
		protected virtual bool OnCanRun(IExecutionHandler executionContext)
		{
			return true;
		}
		public void RegisterResult(TestContext context, UnitTestResult result)
		{
			if (this.lastResult == null || !(this.lastResult.TestDate == context.TestDate))
			{
				result.TestDate = context.TestDate;
				if (result.Status == (ResultStatus)0)
				{
					result.Status = ResultStatus.Ignored;
				}
				this.lastResult = result;
				IResultsStore store = this.GetResultsStore();
				if (store != null)
				{
					store.RegisterResult(this.ActiveConfiguration, this, result);
				}
				this.OnTestStatusChanged();
			}
		}
		private IResultsStore GetResultsStore()
		{
			IResultsStore result;
			if (this.resultsStore != null)
			{
				result = this.resultsStore;
			}
			else
			{
				if (this.Parent != null)
				{
					result = this.Parent.GetResultsStore();
				}
				else
				{
					result = null;
				}
			}
			return result;
		}
		public virtual void SaveResults()
		{
			IResultsStore store = this.GetResultsStore();
			if (store != null)
			{
				store.Save();
			}
		}
		internal virtual void FindRegressions(UnitTestCollection list, System.DateTime fromDate, System.DateTime toDate)
		{
			UnitTestResult res = this.Results.GetLastResult(fromDate);
			UnitTestResult res2 = this.Results.GetLastResult(toDate);
			if ((res == null || res.IsSuccess) && res2 != null && !res2.IsSuccess)
			{
				list.Add(this);
			}
		}
		protected virtual void OnSaveOptions(OptionsData[] data)
		{
			IConfigurationTarget ce;
			string path;
			this.GetOwnerSolutionItem(this, out ce, out path);
			if (ce == null)
			{
				throw new System.InvalidOperationException("Options can't be saved.");
			}
			for (int i = 0; i < data.Length; i++)
			{
				OptionsData d = data[i];
				IExtendedDataItem edi = ce.get_Configurations().get_Item(d.Configuration);
				if (edi != null)
				{
					UnitTestOptionsSet oset = (UnitTestOptionsSet)edi.get_ExtendedProperties()["UnitTestInformation"];
					if (oset == null)
					{
						oset = new UnitTestOptionsSet();
						edi.get_ExtendedProperties()["UnitTestInformation"] = oset;
					}
					UnitTestOptionsEntry te = oset.FindEntry(path);
					if (d.Options.Count > 0)
					{
						if (te == null)
						{
							te = new UnitTestOptionsEntry();
							te.Path = path;
							oset.Tests.Add(te);
						}
						te.Options.Clear();
						te.Options.AddRange(d.Options);
					}
					else
					{
						if (te != null)
						{
							oset.Tests.Remove(te);
						}
					}
				}
			}
			ce.Save(new NullProgressMonitor());
		}
		protected virtual System.Collections.ICollection OnLoadOptions(string configuration)
		{
			IConfigurationTarget ce;
			string path;
			this.GetOwnerSolutionItem(this, out ce, out path);
			System.Collections.ICollection result;
			if (ce == null)
			{
				result = null;
			}
			else
			{
				IExtendedDataItem edi = ce.get_Configurations().get_Item(configuration);
				if (edi == null)
				{
					result = null;
				}
				else
				{
					UnitTestOptionsSet oset = (UnitTestOptionsSet)edi.get_ExtendedProperties()["UnitTestInformation"];
					if (oset == null)
					{
						result = null;
					}
					else
					{
						UnitTestOptionsEntry te = oset.FindEntry(path);
						if (te != null)
						{
							result = te.Options;
						}
						else
						{
							result = null;
						}
					}
				}
			}
			return result;
		}
		private void GetOwnerSolutionItem(UnitTest t, out IConfigurationTarget c, out string path)
		{
			if (this.OwnerSolutionItem is SolutionEntityItem)
			{
				c = (this.OwnerSolutionItem as SolutionEntityItem);
				path = "";
			}
			else
			{
				if (this.parent != null)
				{
					this.parent.GetOwnerSolutionItem(t, out c, out path);
					if (c != null)
					{
						if (path.Length > 0)
						{
							path = path + "/" + t.Name;
						}
						else
						{
							path = t.Name;
						}
					}
				}
				else
				{
					c = null;
					path = null;
				}
			}
		}
		private void OnConfugurationChanged(object ob, ConfigurationEventArgs args)
		{
			this.OnActiveConfigurationChanged();
		}
		protected virtual void OnActiveConfigurationChanged()
		{
			this.OnTestChanged();
		}
		protected virtual void OnTestChanged()
		{
			Application.Invoke(delegate
			{
				if (this.TestChanged != null)
				{
					this.TestChanged(this, System.EventArgs.Empty);
				}
			}
			);
		}
		protected virtual void OnTestStatusChanged()
		{
			Application.Invoke(delegate
			{
				if (this.TestStatusChanged != null)
				{
					this.TestStatusChanged(this, System.EventArgs.Empty);
				}
			}
			);
		}
	}
}
