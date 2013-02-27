using Gdk;
using GLib;
using Gtk;
using MonoDevelop.Components;
using MonoDevelop.Components.Commands;
using MonoDevelop.Components.Docking;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoDevelop.NUnit.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
namespace MonoDevelop.NUnit
{
	internal class TestResultsPad : IPadContent, System.IDisposable, ITestProgressMonitor
	{
		public class ResultRecord
		{
			public UnitTest Test;
			public UnitTestResult Result;
		}
		private NUnitService testService = NUnitService.Instance;
		private IPadWindow window;
		private VBox panel;
		private HPaned book;
		private Label infoFailed = new Label(GettextCatalog.GetString("<b>Failed</b>: {0}", 0));
		private Label infoIgnored = new Label(GettextCatalog.GetString("<b>Ignored</b>: {0}", 0));
		private Label infoCurrent = new Label();
		private HBox labels;
		private Label resultLabel = new Label();
		private ProgressBar progressBar = new ProgressBar();
		private TreeView failuresTreeView;
		private TreeStore failuresStore;
		private TextView outputView;
		private TextTag bold;
		private System.Collections.Generic.Dictionary<UnitTest, int> outIters = new System.Collections.Generic.Dictionary<UnitTest, int>();
		private Widget outputViewScrolled;
		private VSeparator infoSep;
		private TreeIter startMessageIter;
		private Button buttonStop;
		private Button buttonRun;
		private ToggleButton buttonSuccess;
		private ToggleButton buttonFailures;
		private ToggleButton buttonIgnored;
		private ToggleButton buttonOutput;
		private bool running;
		private int testsToRun;
		private int testsRun;
		private int testsFailed;
		private int testsIgnored;
		private UnitTest rootTest;
		private string configuration;
		private System.Collections.ArrayList results = new System.Collections.ArrayList();
		private System.Exception error;
		private string errorMessage;
		private bool cancel;
		public event TestHandler CancelRequested;
		private bool Running
		{
			get
			{
				return this.running;
			}
			set
			{
				this.running = value;
				this.window.set_IsWorking(value);
			}
		}
		public Widget Control
		{
			get
			{
				return this.panel;
			}
		}
		bool ITestProgressMonitor.IsCancelRequested
		{
			get
			{
				return this.cancel;
			}
		}
		public TestResultsPad()
		{
			this.testService.TestSuiteChanged += new System.EventHandler(this.OnTestSuiteChanged);
			this.panel = new VBox();
			this.book = new HPaned();
			this.panel.PackStart(this.book, true, true, 0u);
			this.panel.set_FocusChain(new Widget[]
			{
				this.book
			});
			this.failuresTreeView = new TreeView();
			this.failuresTreeView.set_HeadersVisible(false);
			this.failuresStore = new TreeStore(new System.Type[]
			{
				typeof(Pixbuf), 
				typeof(string), 
				typeof(object), 
				typeof(string)
			});
			CellRendererPixbuf pr = new CellRendererPixbuf();
			CellRendererText tr = new CellRendererText();
			TreeViewColumn col = new TreeViewColumn();
			col.PackStart(pr, false);
			col.AddAttribute(pr, "pixbuf", 0);
			col.PackStart(tr, false);
			col.AddAttribute(tr, "markup", 1);
			this.failuresTreeView.AppendColumn(col);
			this.failuresTreeView.set_Model(this.failuresStore);
			CompactScrolledWindow sw = new CompactScrolledWindow();
			sw.set_ShadowType(0);
			sw.Add(this.failuresTreeView);
			this.book.Pack1(sw, true, true);
			this.outputView = new TextView();
			this.outputView.set_Editable(false);
			this.bold = new TextTag("bold");
			this.bold.set_Weight(700);
			this.outputView.get_Buffer().get_TagTable().Add(this.bold);
			sw = new CompactScrolledWindow();
			sw.set_ShadowType(0);
			sw.Add(this.outputView);
			this.book.Pack2(sw, true, true);
			this.outputViewScrolled = sw;
			this.failuresTreeView.add_ButtonReleaseEvent(new ButtonReleaseEventHandler(this.OnPopupMenu));
			this.failuresTreeView.add_RowActivated(new RowActivatedHandler(this.OnRowActivated));
			this.failuresTreeView.get_Selection().add_Changed(new System.EventHandler(this.OnRowSelected));
			this.Control.ShowAll();
			this.outputViewScrolled.Hide();
		}
		void IPadContent.Initialize(IPadWindow window)
		{
			this.window = window;
			DockItemToolbar toolbar = window.GetToolbar(2);
			this.buttonSuccess = new ToggleButton();
			this.buttonSuccess.set_Label(GettextCatalog.GetString("Successful Tests"));
			this.buttonSuccess.set_Active(false);
			this.buttonSuccess.set_Image(new Image(CircleImage.Success));
			this.buttonSuccess.get_Image().Show();
			this.buttonSuccess.add_Toggled(new System.EventHandler(this.OnShowSuccessfulToggled));
			this.buttonSuccess.set_TooltipText(GettextCatalog.GetString("Show Successful Tests"));
			toolbar.Add(this.buttonSuccess);
			this.buttonFailures = new ToggleButton();
			this.buttonFailures.set_Label(GettextCatalog.GetString("Failed Tests"));
			this.buttonFailures.set_Active(true);
			this.buttonFailures.set_Image(new Image(CircleImage.Failure));
			this.buttonFailures.get_Image().Show();
			this.buttonFailures.add_Toggled(new System.EventHandler(this.OnShowFailuresToggled));
			this.buttonFailures.set_TooltipText(GettextCatalog.GetString("Show Failed Tests"));
			toolbar.Add(this.buttonFailures);
			this.buttonIgnored = new ToggleButton();
			this.buttonIgnored.set_Label(GettextCatalog.GetString("Ignored Tests"));
			this.buttonIgnored.set_Active(true);
			this.buttonIgnored.set_Image(new Image(CircleImage.NotRun));
			this.buttonIgnored.get_Image().Show();
			this.buttonIgnored.add_Toggled(new System.EventHandler(this.OnShowIgnoredToggled));
			this.buttonIgnored.set_TooltipText(GettextCatalog.GetString("Show Ignored Tests"));
			toolbar.Add(this.buttonIgnored);
			this.buttonOutput = new ToggleButton();
			this.buttonOutput.set_Label(GettextCatalog.GetString("Output"));
			this.buttonOutput.set_Active(false);
			this.buttonOutput.set_Image(ImageService.GetImage(Stock.OutputIcon, 1));
			this.buttonOutput.get_Image().Show();
			this.buttonOutput.add_Toggled(new System.EventHandler(this.OnShowOutputToggled));
			this.buttonOutput.set_TooltipText(GettextCatalog.GetString("Show Output"));
			toolbar.Add(this.buttonOutput);
			toolbar.Add(new SeparatorToolItem());
			this.buttonRun = new Button();
			this.buttonRun.set_Label(GettextCatalog.GetString("Run Test"));
			this.buttonRun.set_Image(new Image(Stock.get_Execute(), 1));
			this.buttonRun.get_Image().Show();
			this.buttonRun.set_Sensitive(false);
			toolbar.Add(this.buttonRun);
			this.buttonStop = new Button(new Image(Stock.get_Stop(), 1));
			toolbar.Add(this.buttonStop);
			toolbar.ShowAll();
			this.buttonStop.add_Clicked(new System.EventHandler(this.OnStopClicked));
			this.buttonRun.add_Clicked(new System.EventHandler(this.OnRunClicked));
			DockItemToolbar runPanel = window.GetToolbar(3);
			this.infoSep = new VSeparator();
			this.resultLabel.set_UseMarkup(true);
			this.infoCurrent.set_Ellipsize(1);
			this.infoCurrent.set_WidthRequest(0);
			runPanel.Add(this.resultLabel);
			runPanel.Add(this.progressBar);
			runPanel.Add(this.infoCurrent, true, 10);
			this.labels = new HBox(false, 10);
			this.infoFailed.set_UseMarkup(true);
			this.infoIgnored.set_UseMarkup(true);
			this.labels.PackStart(this.infoFailed, true, false, 0u);
			this.labels.PackStart(this.infoIgnored, true, false, 0u);
			runPanel.Add(new Label(), true);
			runPanel.Add(this.labels);
			runPanel.Add(this.infoSep, false, 10);
			this.progressBar.set_HeightRequest(this.infoFailed.SizeRequest().Height);
			runPanel.ShowAll();
		}
		public void Dispose()
		{
		}
		public void OnTestSuiteChanged(object sender, System.EventArgs e)
		{
			this.results.Clear();
			this.error = null;
			this.errorMessage = null;
			this.failuresStore.Clear();
			this.outputView.get_Buffer().Clear();
			this.outIters.Clear();
			this.progressBar.set_Fraction(0.0);
			this.progressBar.set_Text("");
			this.testsRun = 0;
			this.testsFailed = 0;
			this.testsIgnored = 0;
			this.UpdateCounters();
			if (this.rootTest != null)
			{
				this.rootTest = this.testService.SearchTest(this.rootTest.FullName);
				if (this.rootTest == null)
				{
					this.buttonRun.set_Sensitive(false);
				}
			}
		}
		public void RedrawContent()
		{
		}
		private void UpdateCounters()
		{
			this.infoFailed.set_Markup(GettextCatalog.GetString("<b>Failed</b>: {0}", this.testsFailed));
			this.infoIgnored.set_Markup(GettextCatalog.GetString("<b>Ignored</b>: {0}", this.testsIgnored));
		}
		public void InitializeTestRun(UnitTest test)
		{
			this.rootTest = test;
			this.results.Clear();
			this.testsToRun = test.CountTestCases();
			this.error = null;
			this.errorMessage = null;
			this.progressBar.set_Fraction(0.0);
			this.progressBar.set_Text("");
			this.progressBar.set_Text("0 / " + this.testsToRun);
			this.testsRun = 0;
			this.testsFailed = 0;
			this.testsIgnored = 0;
			this.UpdateCounters();
			this.infoSep.Show();
			this.infoCurrent.Show();
			this.progressBar.Show();
			this.resultLabel.Hide();
			this.labels.Show();
			this.buttonStop.set_Sensitive(true);
			this.buttonRun.set_Sensitive(false);
			this.failuresStore.Clear();
			this.outputView.get_Buffer().Clear();
			this.outIters.Clear();
			this.cancel = false;
			this.Running = true;
			this.configuration = IdeApp.get_Workspace().get_ActiveConfigurationId();
			this.AddStartMessage(true);
		}
		public void AddStartMessage(bool isRunning = true)
		{
			if (this.rootTest != null)
			{
				Pixbuf infoIcon = this.failuresTreeView.RenderIcon(Stock.get_DialogInfo(), 1, "");
				string msg = string.Format(isRunning ? GettextCatalog.GetString("Running tests for <b>{0}</b> configuration <b>{1}</b>") : GettextCatalog.GetString("Test results for <b>{0}</b> configuration <b>{1}</b>"), this.rootTest.Name, this.configuration);
				this.startMessageIter = this.failuresStore.AppendValues(new object[]
				{
					infoIcon, 
					msg, 
					this.rootTest
				});
			}
			else
			{
				this.startMessageIter = TreeIter.Zero;
			}
		}
		public void ReportRuntimeError(string message, System.Exception exception)
		{
			this.error = exception;
			this.errorMessage = message;
			this.AddErrorMessage();
		}
		public void AddErrorMessage()
		{
			string msg = GettextCatalog.GetString("Internal error");
			if (this.errorMessage != null)
			{
				msg = msg + ": " + this.errorMessage;
			}
			Pixbuf stock = this.failuresTreeView.RenderIcon(Stock.get_DialogError(), 1, "");
			TreeStore arg_5E_0 = this.failuresStore;
			object[] array = new object[3];
			array[0] = stock;
			array[1] = msg;
			TreeIter testRow = arg_5E_0.AppendValues(array);
			TreeStore arg_A4_0 = this.failuresStore;
			TreeIter arg_A4_1 = testRow;
			array = new object[3];
			array[1] = this.Escape(this.error.GetType().Name + ": " + this.error.Message);
			arg_A4_0.AppendValues(arg_A4_1, array);
			TreeStore arg_C9_0 = this.failuresStore;
			TreeIter arg_C9_1 = testRow;
			array = new object[3];
			array[1] = GettextCatalog.GetString("Stack Trace");
			TreeIter row = arg_C9_0.AppendValues(arg_C9_1, array);
			this.AddStackTrace(row, this.error.StackTrace, null);
		}
		private void AddStackTrace(TreeIter row, string stackTrace, UnitTest test)
		{
			string[] stackLines = stackTrace.Replace("\r", "").Split(new char[]
			{
				'\n'
			});
			string[] array = stackLines;
			for (int j = 0; j < array.Length; j++)
			{
				string line = array[j];
				Regex r = new Regex(".*?\\(.*?\\)\\s\\[.*?\\]\\s.*?\\s(?<file>.*)\\:(?<line>\\d*)");
				Match i = r.Match(line);
				string file;
				if (i.Groups["file"] != null && i.Groups["line"] != null)
				{
					file = i.Groups["file"].Value + ":" + i.Groups["line"].Value;
				}
				else
				{
					file = null;
				}
				this.failuresStore.AppendValues(row, new object[]
				{
					null, 
					this.Escape(line), 
					test, 
					file
				});
			}
		}
		public void FinishTestRun()
		{
			if (!TreeIter.Zero.Equals(this.startMessageIter))
			{
				string msg = string.Format(GettextCatalog.GetString("Test results for <b>{0}</b> configuration <b>{1}</b>"), this.rootTest.Name, this.configuration);
				this.failuresStore.SetValue(this.startMessageIter, 1, msg);
				this.startMessageIter = TreeIter.Zero;
			}
			this.infoCurrent.set_Text("");
			this.progressBar.set_Fraction(1.0);
			this.progressBar.set_Text("");
			this.infoSep.Hide();
			this.infoCurrent.Hide();
			this.progressBar.Hide();
			this.resultLabel.Show();
			this.labels.Hide();
			this.buttonStop.set_Sensitive(false);
			this.buttonRun.set_Sensitive(true);
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.Append(GettextCatalog.GetString("<b>Tests</b>: {0}", this.testsRun)).Append("  ");
			sb.Append(GettextCatalog.GetString("<b>Failed</b>: {0}", this.testsFailed)).Append("  ");
			sb.Append(GettextCatalog.GetString("<b>Ignored</b>: {0}", this.testsIgnored));
			this.resultLabel.set_Markup(sb.ToString());
			this.Running = false;
		}
		private void OnStopClicked(object sender, System.EventArgs args)
		{
			if (this.running)
			{
				this.Cancel();
			}
		}
		private void OnRunClicked(object sender, System.EventArgs args)
		{
			if (this.rootTest != null)
			{
				NUnitService.Instance.RunTest(this.rootTest, null);
			}
		}
		private void OnPopupMenu(object o, ButtonReleaseEventArgs args)
		{
			if (args.get_Event().get_Button() == 3u)
			{
				IdeApp.get_CommandService().ShowContextMenu("/MonoDevelop/NUnit/ContextMenu/TestResultsPad");
			}
		}
		private void OnRowActivated(object s, System.EventArgs a)
		{
			TreeIter iter;
			if (this.failuresTreeView.get_Selection().GetSelected(ref iter))
			{
				string file = (string)this.failuresStore.GetValue(iter, 3);
				if (file != null)
				{
					int i = file.LastIndexOf(':');
					if (i != -1)
					{
						int line;
						if (int.TryParse(file.Substring(i + 1), out line))
						{
							IdeApp.get_Workbench().OpenDocument(file.Substring(0, i), line, -1, 3);
							return;
						}
					}
				}
			}
			this.OnShowTest();
		}
		private void OnRowSelected(object s, System.EventArgs a)
		{
			UnitTest test = this.GetSelectedTest();
			if (test != null)
			{
				int offset;
				if (this.outIters.TryGetValue(test, out offset))
				{
					TextIter it = this.outputView.get_Buffer().GetIterAtOffset(offset);
					this.outputView.get_Buffer().MoveMark(this.outputView.get_Buffer().get_InsertMark(), it);
					this.outputView.get_Buffer().MoveMark(this.outputView.get_Buffer().get_SelectionBound(), it);
					this.outputView.ScrollToMark(this.outputView.get_Buffer().get_InsertMark(), 0.0, true, 0.0, 0.0);
				}
			}
		}
		[CommandHandler(TestCommands.SelectTestInTree)]
		protected void OnSelectTestInTree()
		{
			Pad pad = IdeApp.get_Workbench().GetPad<TestPad>();
			pad.BringToFront();
			TestPad content = (TestPad)pad.get_Content();
			content.SelectTest(this.GetSelectedTest());
		}
		[CommandUpdateHandler(TestCommands.SelectTestInTree)]
		protected void OnUpdateSelectTestInTree(CommandInfo info)
		{
			UnitTest test = this.GetSelectedTest();
			info.set_Enabled(test != null);
		}
		[CommandHandler(TestCommands.GoToFailure)]
		protected void OnShowTest()
		{
			UnitTest test = this.GetSelectedTest();
			if (test != null)
			{
				SourceCodeLocation loc = null;
				UnitTestResult res = test.GetLastResult();
				if (res != null && res.IsFailure)
				{
					loc = res.GetFailureLocation();
				}
				if (loc == null)
				{
					loc = test.SourceCodeLocation;
				}
				if (loc != null)
				{
					IdeApp.get_Workbench().OpenDocument(loc.FileName, loc.Line, loc.Column, 3);
				}
			}
		}
		[CommandHandler(TestCommands.ShowTestCode)]
		protected void OnShowTestCode()
		{
			UnitTest test = this.GetSelectedTest();
			if (test != null)
			{
				SourceCodeLocation loc = test.SourceCodeLocation;
				if (loc != null)
				{
					IdeApp.get_Workbench().OpenDocument(loc.FileName, loc.Line, loc.Column, 3);
				}
			}
		}
		[CommandUpdateHandler(TestCommands.ShowTestCode), CommandUpdateHandler(TestCommands.GoToFailure)]
		protected void OnUpdateRunTest(CommandInfo info)
		{
			UnitTest test = this.GetSelectedTest();
			info.set_Enabled(test != null && test.SourceCodeLocation != null);
		}
		private UnitTest GetSelectedTest()
		{
			TreeModel foo;
			TreeIter iter;
			UnitTest result;
			if (!this.failuresTreeView.get_Selection().GetSelected(ref foo, ref iter))
			{
				result = null;
			}
			else
			{
				UnitTest t = (UnitTest)this.failuresStore.GetValue(iter, 2);
				result = t;
			}
			return result;
		}
		private void OnShowSuccessfulToggled(object sender, System.EventArgs args)
		{
			this.RefreshList();
		}
		private void OnShowFailuresToggled(object sender, System.EventArgs args)
		{
			this.RefreshList();
		}
		private void OnShowIgnoredToggled(object sender, System.EventArgs args)
		{
			this.RefreshList();
		}
		private void OnShowOutputToggled(object sender, System.EventArgs args)
		{
			this.outputViewScrolled.set_Visible(this.buttonOutput.get_Active());
		}
		private void RefreshList()
		{
			this.failuresStore.Clear();
			this.outputView.get_Buffer().Clear();
			this.outIters.Clear();
			this.AddStartMessage(this.running);
			foreach (TestResultsPad.ResultRecord res in this.results)
			{
				this.ShowTestResult(res.Test, res.Result);
			}
			if (this.error != null)
			{
				this.AddErrorMessage();
			}
		}
		private void ShowTestResult(UnitTest test, UnitTestResult result)
		{
			if (result.IsSuccess)
			{
				if (!this.buttonSuccess.get_Active())
				{
					return;
				}
				TreeIter testRow = this.failuresStore.AppendValues(new object[]
				{
					CircleImage.Success, 
					this.Escape(test.FullName), 
					test
				});
				this.failuresTreeView.ScrollToCell(this.failuresStore.GetPath(testRow), null, false, 0f, 0f);
			}
			if (result.IsFailure)
			{
				if (!this.buttonFailures.get_Active())
				{
					return;
				}
				string file = (test.SourceCodeLocation != null) ? (test.SourceCodeLocation.FileName + ":" + test.SourceCodeLocation.Line) : null;
				TreeIter testRow = this.failuresStore.AppendValues(new object[]
				{
					CircleImage.Failure, 
					this.Escape(test.FullName), 
					test, 
					file
				});
				bool hasMessage = result.Message != null && result.Message.Length > 0;
				if (hasMessage)
				{
					this.failuresStore.AppendValues(testRow, new object[]
					{
						null, 
						this.Escape(result.Message), 
						test
					});
				}
				if (result.StackTrace != null && result.StackTrace.Length > 0)
				{
					TreeIter row = testRow;
					if (hasMessage)
					{
						row = this.failuresStore.AppendValues(testRow, new object[]
						{
							null, 
							GettextCatalog.GetString("Stack Trace"), 
							test
						});
					}
					this.AddStackTrace(row, result.StackTrace, test);
				}
				this.failuresTreeView.ScrollToCell(this.failuresStore.GetPath(testRow), null, false, 0f, 0f);
			}
			if (!result.IsIgnored)
			{
				goto IL_2AE;
			}
			if (this.buttonIgnored.get_Active())
			{
				TreeIter testRow = this.failuresStore.AppendValues(new object[]
				{
					CircleImage.NotRun, 
					this.Escape(test.FullName), 
					test
				});
				if (result.Message != null)
				{
					this.failuresStore.AppendValues(testRow, new object[]
					{
						null, 
						this.Escape(result.Message), 
						test
					});
				}
				this.failuresTreeView.ScrollToCell(this.failuresStore.GetPath(testRow), null, false, 0f, 0f);
				goto IL_2AE;
			}
			return;
			IL_2AE:
			string msg = GettextCatalog.GetString("Running {0} ...", test.FullName);
			TextIter it = this.outputView.get_Buffer().get_EndIter();
			this.outIters[test] = it.get_Offset();
			this.outputView.get_Buffer().InsertWithTags(ref it, msg, new TextTag[]
			{
				this.bold
			});
			this.outputView.get_Buffer().Insert(ref it, "\n");
			if (result.ConsoleOutput != null)
			{
				this.outputView.get_Buffer().Insert(ref it, result.ConsoleOutput);
			}
			if (result.ConsoleError != null)
			{
				this.outputView.get_Buffer().Insert(ref it, result.ConsoleError);
			}
			this.outputView.ScrollMarkOnscreen(this.outputView.get_Buffer().get_InsertMark());
		}
		private string Escape(string s)
		{
			return Markup.EscapeText(s);
		}
		void ITestProgressMonitor.EndTest(UnitTest test, UnitTestResult result)
		{
			if (!(test is UnitTestGroup))
			{
				this.testsRun++;
				TestResultsPad.ResultRecord rec = new TestResultsPad.ResultRecord();
				rec.Test = test;
				rec.Result = result;
				if (result.IsFailure)
				{
					this.testsFailed++;
				}
				if (result.IsIgnored)
				{
					this.testsIgnored++;
				}
				this.results.Add(rec);
				this.ShowTestResult(test, result);
				this.UpdateCounters();
				double frac;
				if (this.testsToRun != 0)
				{
					frac = (double)this.testsRun / (double)this.testsToRun;
				}
				else
				{
					frac = 1.0;
				}
				this.progressBar.set_Fraction(frac);
				this.progressBar.set_Text(this.testsRun + " / " + this.testsToRun);
			}
		}
		void ITestProgressMonitor.BeginTest(UnitTest test)
		{
			this.infoCurrent.set_Text(GettextCatalog.GetString("Running ") + test.FullName);
			this.infoCurrent.set_Xalign(0f);
		}
		public void Cancel()
		{
			if (!this.cancel)
			{
				this.cancel = true;
				Application.Invoke(delegate
				{
					TreeStore arg_24_0 = this.failuresStore;
					object[] array = new object[3];
					array[0] = CircleImage.Failure;
					array[1] = GettextCatalog.GetString("Test execution cancelled.");
					arg_24_0.AppendValues(array);
				}
				);
				if (this.CancelRequested != null)
				{
					this.CancelRequested();
				}
			}
		}
	}
}
