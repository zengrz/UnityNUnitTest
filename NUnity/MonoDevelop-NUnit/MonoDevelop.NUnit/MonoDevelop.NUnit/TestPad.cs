using Gdk;
using Gtk;
using MonoDevelop.Components.Commands;
using MonoDevelop.Components.Docking;
using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Execution;
using MonoDevelop.Ide.Gui.Components;
using MonoDevelop.Ide.Gui.Pads;
using MonoDevelop.NUnit.Commands;
using MonoDevelop.Projects;
using System;
using System.Collections;
namespace MonoDevelop.NUnit
{
	public class TestPad : TreeViewPad
	{
		private NUnitService testService = NUnitService.Instance;
		private IAsyncOperation runningTestOperation;
		private VPaned paned;
		private TreeView detailsTree;
		private ListStore detailsStore;
		private HeaderLabel detailLabel;
		private TestChart chart;
		private UnitTest detailsTest;
		private System.DateTime detailsDate;
		private System.DateTime detailsReferenceDate;
		private ButtonNotebook infoBook;
		private TextView outputView;
		private TextView resultView;
		private TreeView regressionTree;
		private ListStore regressionStore;
		private TreeView failedTree;
		private ListStore failedStore;
		private int TestSummaryPage;
		private int TestRegressionsPage;
		private int TestFailuresPage;
		private int TestResultPage;
		private int TestOutputPage;
		private System.EventHandler testChangedHandler;
		private VBox detailsPad;
		private System.Collections.ArrayList testNavigationHistory = new System.Collections.ArrayList();
		private Button buttonRunAll;
		private Button buttonRun;
		private Button buttonStop;
		public override Widget Control
		{
			get
			{
				return this.paned;
			}
		}
		public override void Initialize(NodeBuilder[] builders, TreePadOption[] options, string menuPath)
		{
			base.Initialize(builders, options, menuPath);
			this.testChangedHandler = DispatchService.GuiDispatch<System.EventHandler>(new System.EventHandler(this.OnDetailsTestChanged));
			this.testService.TestSuiteChanged += DispatchService.GuiDispatch<System.EventHandler>(new System.EventHandler(this.OnTestSuiteChanged));
			this.paned = new VPaned();
			VBox vbox = new VBox();
			DockItemToolbar topToolbar = base.get_Window().GetToolbar(2);
			this.buttonRunAll = new Button(new Image(Stock.get_GoUp(), 1));
			this.buttonRunAll.add_Clicked(new System.EventHandler(this.OnRunAllClicked));
			this.buttonRunAll.set_Sensitive(true);
			this.buttonRunAll.set_TooltipText(GettextCatalog.GetString("Run all tests"));
			topToolbar.Add(this.buttonRunAll);
			this.buttonRun = new Button(new Image(Stock.get_Execute(), 1));
			this.buttonRun.add_Clicked(new System.EventHandler(this.OnRunClicked));
			this.buttonRun.set_Sensitive(true);
			this.buttonRun.set_TooltipText(GettextCatalog.GetString("Run test"));
			topToolbar.Add(this.buttonRun);
			this.buttonStop = new Button(new Image(Stock.get_Stop(), 1));
			this.buttonStop.add_Clicked(new System.EventHandler(this.OnStopClicked));
			this.buttonStop.set_Sensitive(false);
			this.buttonStop.set_TooltipText(GettextCatalog.GetString("Cancel running test"));
			topToolbar.Add(this.buttonStop);
			topToolbar.ShowAll();
			vbox.PackEnd(base.get_Control(), true, true, 0u);
			vbox.set_FocusChain(new Widget[]
			{
				base.get_Control()
			});
			this.paned.Pack1(vbox, true, true);
			this.detailsPad = new VBox();
			EventBox eb = new EventBox();
			HBox header = new HBox();
			eb.Add(header);
			this.detailLabel = new HeaderLabel();
			this.detailLabel.Padding = 6;
			Button hb = new Button(new Image("gtk-close", 2));
			hb.set_Relief(2);
			hb.add_Clicked(new System.EventHandler(this.OnCloseDetails));
			header.PackEnd(hb, false, false, 0u);
			hb = new Button(new Image("gtk-go-back", 2));
			hb.set_Relief(2);
			hb.add_Clicked(new System.EventHandler(this.OnGoBackTest));
			header.PackEnd(hb, false, false, 0u);
			header.Add(this.detailLabel);
			Color hcol = eb.get_Style().Background(0);
			hcol.Red = (ushort)((double)hcol.Red * 0.9);
			hcol.Green = (ushort)((double)hcol.Green * 0.9);
			hcol.Blue = (ushort)((double)hcol.Blue * 0.9);
			this.detailsPad.PackStart(eb, false, false, 0u);
			VPaned panedDetails = new VPaned();
			panedDetails.set_BorderWidth(3u);
			VBox boxPaned = new VBox();
			this.chart = new TestChart();
			this.chart.add_ButtonReleaseEvent(new ButtonReleaseEventHandler(this.OnChartPopupMenu));
			this.chart.add_SelectionChanged(new System.EventHandler(this.OnChartDateChanged));
			this.chart.set_HeightRequest(50);
			Toolbar toolbar = new Toolbar();
			toolbar.set_IconSize(2);
			toolbar.set_ToolbarStyle(0);
			toolbar.set_ShowArrow(false);
			ToolButton but = new ToolButton("gtk-zoom-in");
			but.add_Clicked(new System.EventHandler(this.OnZoomIn));
			toolbar.Insert(but, -1);
			but = new ToolButton("gtk-zoom-out");
			but.add_Clicked(new System.EventHandler(this.OnZoomOut));
			toolbar.Insert(but, -1);
			but = new ToolButton("gtk-go-back");
			but.add_Clicked(new System.EventHandler(this.OnChartBack));
			toolbar.Insert(but, -1);
			but = new ToolButton("gtk-go-forward");
			but.add_Clicked(new System.EventHandler(this.OnChartForward));
			toolbar.Insert(but, -1);
			but = new ToolButton("gtk-goto-last");
			but.add_Clicked(new System.EventHandler(this.OnChartLast));
			toolbar.Insert(but, -1);
			boxPaned.PackStart(toolbar, false, false, 0u);
			boxPaned.PackStart(this.chart, true, true, 0u);
			panedDetails.Pack1(boxPaned, false, false);
			this.infoBook = new ButtonNotebook();
			ButtonNotebook expr_472 = this.infoBook;
			expr_472.PageLoadRequired = (System.EventHandler)System.Delegate.Combine(expr_472.PageLoadRequired, new System.EventHandler(this.OnPageLoadRequired));
			Frame tf = new Frame();
			ScrolledWindow sw = new ScrolledWindow();
			this.detailsTree = new TreeView();
			this.detailsTree.set_HeadersVisible(true);
			this.detailsTree.set_RulesHint(true);
			this.detailsStore = new ListStore(new System.Type[]
			{
				typeof(object), 
				typeof(string), 
				typeof(string), 
				typeof(string), 
				typeof(string)
			});
			CellRendererText trtest = new CellRendererText();
			TreeViewColumn col3 = new TreeViewColumn();
			col3.set_Expand(false);
			col3.set_Widget(new Image(CircleImage.Success));
			col3.get_Widget().Show();
			CellRendererText tr = new CellRendererText();
			col3.PackStart(tr, false);
			col3.AddAttribute(tr, "markup", 2);
			this.detailsTree.AppendColumn(col3);
			TreeViewColumn col4 = new TreeViewColumn();
			col4.set_Expand(false);
			col4.set_Widget(new Image(CircleImage.Failure));
			col4.get_Widget().Show();
			tr = new CellRendererText();
			col4.PackStart(tr, false);
			col4.AddAttribute(tr, "markup", 3);
			this.detailsTree.AppendColumn(col4);
			TreeViewColumn col5 = new TreeViewColumn();
			col5.set_Expand(false);
			col5.set_Widget(new Image(CircleImage.NotRun));
			col5.get_Widget().Show();
			tr = new CellRendererText();
			col5.PackStart(tr, false);
			col5.AddAttribute(tr, "markup", 4);
			this.detailsTree.AppendColumn(col5);
			TreeViewColumn col6 = new TreeViewColumn();
			col6.set_Title("Test");
			col6.PackStart(trtest, true);
			col6.AddAttribute(trtest, "markup", 1);
			this.detailsTree.AppendColumn(col6);
			this.detailsTree.set_Model(this.detailsStore);
			sw.Add(this.detailsTree);
			tf.Add(sw);
			tf.ShowAll();
			this.TestSummaryPage = this.infoBook.AddPage(GettextCatalog.GetString("Summary"), tf);
			tf = new Frame();
			sw = new ScrolledWindow();
			tf.Add(sw);
			this.regressionTree = new TreeView();
			this.regressionTree.set_HeadersVisible(false);
			this.regressionTree.set_RulesHint(true);
			this.regressionStore = new ListStore(new System.Type[]
			{
				typeof(object), 
				typeof(string), 
				typeof(Pixbuf)
			});
			CellRendererText trtest2 = new CellRendererText();
			CellRendererPixbuf pr = new CellRendererPixbuf();
			TreeViewColumn col7 = new TreeViewColumn();
			col7.PackStart(pr, false);
			col7.AddAttribute(pr, "pixbuf", 2);
			col7.PackStart(trtest2, false);
			col7.AddAttribute(trtest2, "markup", 1);
			this.regressionTree.AppendColumn(col7);
			this.regressionTree.set_Model(this.regressionStore);
			sw.Add(this.regressionTree);
			tf.ShowAll();
			this.TestRegressionsPage = this.infoBook.AddPage(GettextCatalog.GetString("Regressions"), tf);
			tf = new Frame();
			sw = new ScrolledWindow();
			tf.Add(sw);
			this.failedTree = new TreeView();
			this.failedTree.set_HeadersVisible(false);
			this.failedTree.set_RulesHint(true);
			this.failedStore = new ListStore(new System.Type[]
			{
				typeof(object), 
				typeof(string), 
				typeof(Pixbuf)
			});
			trtest2 = new CellRendererText();
			pr = new CellRendererPixbuf();
			col7 = new TreeViewColumn();
			col7.PackStart(pr, false);
			col7.AddAttribute(pr, "pixbuf", 2);
			col7.PackStart(trtest2, false);
			col7.AddAttribute(trtest2, "markup", 1);
			this.failedTree.AppendColumn(col7);
			this.failedTree.set_Model(this.failedStore);
			sw.Add(this.failedTree);
			tf.ShowAll();
			this.TestFailuresPage = this.infoBook.AddPage(GettextCatalog.GetString("Failed tests"), tf);
			tf = new Frame();
			sw = new ScrolledWindow();
			tf.Add(sw);
			this.resultView = new TextView();
			this.resultView.set_Editable(false);
			sw.Add(this.resultView);
			tf.ShowAll();
			this.TestResultPage = this.infoBook.AddPage(GettextCatalog.GetString("Result"), tf);
			tf = new Frame();
			sw = new ScrolledWindow();
			tf.Add(sw);
			this.outputView = new TextView();
			this.outputView.set_Editable(false);
			sw.Add(this.outputView);
			tf.ShowAll();
			this.TestOutputPage = this.infoBook.AddPage(GettextCatalog.GetString("Output"), tf);
			panedDetails.Pack2(this.infoBook, true, true);
			this.detailsPad.PackStart(panedDetails, true, true, 0u);
			this.paned.Pack2(this.detailsPad, true, true);
			this.paned.ShowAll();
			this.infoBook.HidePage(this.TestResultPage);
			this.infoBook.HidePage(this.TestOutputPage);
			this.infoBook.HidePage(this.TestSummaryPage);
			this.infoBook.HidePage(this.TestRegressionsPage);
			this.infoBook.HidePage(this.TestFailuresPage);
			this.detailsPad.set_Sensitive(false);
			this.detailsPad.Hide();
			this.detailsTree.add_RowActivated(new RowActivatedHandler(this.OnTestActivated));
			this.regressionTree.add_RowActivated(new RowActivatedHandler(this.OnRegressionTestActivated));
			this.failedTree.add_RowActivated(new RowActivatedHandler(this.OnFailedTestActivated));
			UnitTest[] rootTests = this.testService.RootTests;
			for (int i = 0; i < rootTests.Length; i++)
			{
				UnitTest t = rootTests[i];
				base.get_TreeView().AddChild(t);
			}
		}
		private void OnTestSuiteChanged(object sender, System.EventArgs e)
		{
			if (this.testService.RootTests.Length > 0)
			{
				base.get_TreeView().Clear();
				UnitTest[] rootTests = this.testService.RootTests;
				for (int i = 0; i < rootTests.Length; i++)
				{
					UnitTest t = rootTests[i];
					base.get_TreeView().AddChild(t);
				}
			}
			else
			{
				base.get_TreeView().Clear();
				this.ClearDetails();
			}
		}
		public void SelectTest(UnitTest t)
		{
			ITreeNavigator node = this.FindTestNode(t);
			if (node != null)
			{
				node.ExpandToNode();
				node.set_Selected(true);
			}
		}
		private ITreeNavigator FindTestNode(UnitTest t)
		{
			ITreeNavigator nav = base.get_TreeView().GetNodeAtObject(t);
			ITreeNavigator result;
			if (nav != null)
			{
				result = nav;
			}
			else
			{
				if (t.Parent == null)
				{
					result = null;
				}
				else
				{
					nav = this.FindTestNode(t.Parent);
					if (nav == null)
					{
						result = null;
					}
					else
					{
						nav.MoveToFirstChild();
						result = base.get_TreeView().GetNodeAtObject(t);
					}
				}
			}
			return result;
		}
		[CommandHandler(TestCommands.RunTest)]
		protected void OnRunTest()
		{
			this.RunSelectedTest(null);
		}
		[CommandUpdateHandler(TestCommands.RunTest)]
		protected void OnUpdateRunTest(CommandInfo info)
		{
			info.set_Enabled(this.runningTestOperation == null);
		}
		[CommandHandler(TestCommands.RunTestWith)]
		protected void OnRunTest(object data)
		{
			IExecutionHandler h = ExecutionModeCommandService.GetExecutionModeForCommand(data);
			if (h != null)
			{
				this.RunSelectedTest(h);
			}
		}
		[CommandUpdateHandler(TestCommands.RunTestWith)]
		protected void OnUpdateRunTest(CommandArrayInfo info)
		{
			UnitTest test = this.GetSelectedTest();
			if (test != null)
			{
				SolutionEntityItem item = test.OwnerObject as SolutionEntityItem;
				ExecutionModeCommandService.GenerateExecutionModeCommands(item, new CanExecuteDelegate(test.CanRun), info);
			}
		}
		public TestPad()
		{
			base.get_TreeView().add_CurrentItemActivated(delegate
			{
				this.RunSelectedTest(null);
			}
			);
		}
		private void OnStopClicked(object sender, System.EventArgs args)
		{
			if (this.runningTestOperation != null)
			{
				this.runningTestOperation.Cancel();
			}
		}
		private void OnRunClicked(object sender, System.EventArgs args)
		{
			this.RunSelectedTest(null);
		}
		private UnitTest GetSelectedTest()
		{
			ITreeNavigator nav = base.get_TreeView().GetSelectedNode();
			UnitTest result;
			if (nav == null)
			{
				result = null;
			}
			else
			{
				result = (nav.get_DataItem() as UnitTest);
			}
			return result;
		}
		private void RunTest(ITreeNavigator nav, IExecutionHandler mode)
		{
			if (nav != null)
			{
				UnitTest test = nav.get_DataItem() as UnitTest;
				if (test != null)
				{
					TestSession.ResetResult(test.RootTest);
					this.buttonRun.set_Sensitive(false);
					this.buttonRunAll.set_Sensitive(false);
					this.buttonStop.set_Sensitive(true);
					IdeApp.get_Workbench().GetPad<TestPad>().BringToFront();
					this.runningTestOperation = this.testService.RunTest(test, mode);
					this.runningTestOperation.add_Completed(DispatchService.GuiDispatch<OperationHandler>(new OperationHandler(this.TestSessionCompleted)));
				}
			}
		}
		private void OnRunAllClicked(object sender, System.EventArgs args)
		{
			this.RunTest(base.get_TreeView().GetRootNode(), null);
		}
		private void RunSelectedTest(IExecutionHandler mode)
		{
			this.RunTest(base.get_TreeView().GetSelectedNode(), mode);
		}
		private void TestSessionCompleted(IAsyncOperation op)
		{
			if (op.get_Success())
			{
				this.RefreshDetails();
			}
			this.runningTestOperation = null;
			this.buttonRun.set_Sensitive(true);
			this.buttonRunAll.set_Sensitive(true);
			this.buttonStop.set_Sensitive(false);
		}
		protected override void OnSelectionChanged(object sender, System.EventArgs args)
		{
			base.OnSelectionChanged(sender, args);
			ITreeNavigator nav = base.get_TreeView().GetSelectedNode();
			if (nav != null)
			{
				UnitTest test = (UnitTest)nav.get_DataItem();
				if (test != null)
				{
					this.FillDetails(test, false);
				}
			}
		}
		private void OnGoBackTest(object sender, System.EventArgs args)
		{
			int c = this.testNavigationHistory.Count;
			if (c > 1)
			{
				UnitTest t = (UnitTest)this.testNavigationHistory[c - 2];
				this.testNavigationHistory.RemoveAt(c - 1);
				this.testNavigationHistory.RemoveAt(c - 2);
				this.FillDetails(t, true);
			}
		}
		private void OnCloseDetails(object sender, System.EventArgs args)
		{
			this.detailsPad.Hide();
		}
		[CommandHandler(TestCommands.ShowTestDetails)]
		protected void OnShowDetails()
		{
			if (!this.detailsPad.get_Visible())
			{
				this.detailsPad.Show();
				ITreeNavigator nav = base.get_TreeView().GetSelectedNode();
				if (nav != null)
				{
					UnitTest test = (UnitTest)nav.get_DataItem();
					this.FillDetails(test, false);
				}
				else
				{
					this.ClearDetails();
				}
			}
		}
		private void ClearDetails()
		{
			this.chart.Clear();
			this.detailLabel.Markup = "";
			this.detailsStore.Clear();
			if (this.detailsTest != null)
			{
				this.detailsTest.TestChanged -= this.testChangedHandler;
			}
			this.detailsTest = null;
			this.detailsDate = System.DateTime.MinValue;
			this.detailsReferenceDate = System.DateTime.MinValue;
			this.detailsPad.set_Sensitive(false);
		}
		private void RefreshDetails()
		{
			if (this.detailsTest != null)
			{
				this.FillDetails(this.detailsTest, false);
			}
		}
		private void FillDetails(UnitTest test, bool selectInTree)
		{
			if (this.detailsPad.get_Visible())
			{
				this.detailsPad.set_Sensitive(true);
				if (this.detailsTest != null)
				{
					this.detailsTest.TestChanged -= this.testChangedHandler;
				}
				if (this.detailsTest != test)
				{
					this.detailsTest = test;
					if (selectInTree)
					{
						this.SelectTest(test);
					}
					this.testNavigationHistory.Add(test);
					if (this.testNavigationHistory.Count > 50)
					{
						this.testNavigationHistory.RemoveAt(0);
					}
				}
				this.detailsTest.TestChanged += this.testChangedHandler;
				if (test is UnitTestGroup)
				{
					this.infoBook.HidePage(this.TestResultPage);
					this.infoBook.HidePage(this.TestOutputPage);
					this.infoBook.ShowPage(this.TestSummaryPage);
					this.infoBook.ShowPage(this.TestRegressionsPage);
					this.infoBook.ShowPage(this.TestFailuresPage);
				}
				else
				{
					this.infoBook.HidePage(this.TestSummaryPage);
					this.infoBook.HidePage(this.TestRegressionsPage);
					this.infoBook.HidePage(this.TestFailuresPage);
					this.infoBook.ShowPage(this.TestResultPage);
					this.infoBook.ShowPage(this.TestOutputPage);
				}
				this.detailLabel.Markup = "<b>" + test.Name + "</b>";
				this.detailsDate = System.DateTime.MinValue;
				this.detailsReferenceDate = System.DateTime.MinValue;
				this.chart.Fill(test);
				this.infoBook.Reset();
			}
		}
		private void FillTestInformation()
		{
			if (this.detailsPad.get_Visible())
			{
				if (this.detailsTest is UnitTestGroup)
				{
					UnitTestGroup group = this.detailsTest as UnitTestGroup;
					if (this.infoBook.get_Page() == this.TestSummaryPage)
					{
						this.detailsStore.Clear();
						foreach (UnitTest t in group.Tests)
						{
							UnitTestResult res = t.Results.GetLastResult(this.chart.CurrentDate);
							if (res != null)
							{
								ListStore arg_FF_0 = this.detailsStore;
								object[] array = new object[5];
								array[0] = t;
								array[1] = t.Name;
								object[] arg_D6_0 = array;
								int arg_D6_1 = 2;
								int num = res.TotalSuccess;
								arg_D6_0[arg_D6_1] = num.ToString();
								object[] arg_E9_0 = array;
								int arg_E9_1 = 3;
								num = res.TotalFailures;
								arg_E9_0[arg_E9_1] = num.ToString();
								object[] arg_FC_0 = array;
								int arg_FC_1 = 4;
								num = res.TotalIgnored;
								arg_FC_0[arg_FC_1] = num.ToString();
								arg_FF_0.AppendValues(array);
							}
							else
							{
								this.detailsStore.AppendValues(new object[]
								{
									t, 
									t.Name, 
									"", 
									"", 
									""
								});
							}
						}
					}
					else
					{
						if (this.infoBook.get_Page() == this.TestRegressionsPage)
						{
							this.regressionStore.Clear();
							UnitTestCollection regs = this.detailsTest.GetRegressions(this.chart.ReferenceDate, this.chart.CurrentDate);
							if (regs.Count > 0)
							{
								foreach (UnitTest t in regs)
								{
									this.regressionStore.AppendValues(new object[]
									{
										t, 
										t.Name, 
										CircleImage.Failure
									});
								}
							}
							else
							{
								this.regressionStore.AppendValues(new object[]
								{
									null, 
									GettextCatalog.GetString("No regressions found.")
								});
							}
						}
						else
						{
							if (this.infoBook.get_Page() == this.TestFailuresPage)
							{
								this.failedStore.Clear();
								UnitTestCollection regs = group.GetFailedTests(this.chart.CurrentDate);
								if (regs.Count > 0)
								{
									foreach (UnitTest t in regs)
									{
										this.failedStore.AppendValues(new object[]
										{
											t, 
											t.Name, 
											CircleImage.Failure
										});
									}
								}
								else
								{
									this.failedStore.AppendValues(new object[]
									{
										null, 
										GettextCatalog.GetString("No failed tests found.")
									});
								}
							}
						}
					}
				}
				else
				{
					UnitTestResult res = this.detailsTest.Results.GetLastResult(this.chart.CurrentDate);
					if (this.infoBook.get_Page() == this.TestOutputPage)
					{
						this.outputView.get_Buffer().Clear();
						if (res != null)
						{
							this.outputView.get_Buffer().InsertAtCursor(res.ConsoleOutput);
						}
					}
					else
					{
						if (this.infoBook.get_Page() == this.TestResultPage)
						{
							this.resultView.get_Buffer().Clear();
							if (res != null)
							{
								string msg = res.Message + "\n\n" + res.StackTrace;
								this.resultView.get_Buffer().InsertAtCursor(msg);
							}
						}
					}
				}
			}
		}
		private void OnDetailsTestChanged(object sender, System.EventArgs e)
		{
			this.RefreshDetails();
		}
		private void OnChartDateChanged(object sender, System.EventArgs e)
		{
			if (this.detailsTest != null && (this.detailsDate != this.chart.CurrentDate || this.detailsReferenceDate != this.chart.ReferenceDate))
			{
				this.detailsDate = this.chart.CurrentDate;
				this.detailsReferenceDate = this.chart.ReferenceDate;
				this.FillTestInformation();
			}
		}
		private void OnPageLoadRequired(object o, System.EventArgs args)
		{
			if (this.detailsTest != null)
			{
				this.FillTestInformation();
			}
		}
		protected virtual void OnTestActivated(object sender, RowActivatedArgs args)
		{
			TreeIter it;
			this.detailsStore.GetIter(ref it, args.get_Path());
			UnitTest t = (UnitTest)this.detailsStore.GetValue(it, 0);
			if (t != null)
			{
				this.FillDetails(t, true);
			}
		}
		protected virtual void OnRegressionTestActivated(object sender, RowActivatedArgs args)
		{
			TreeIter it;
			this.regressionStore.GetIter(ref it, args.get_Path());
			UnitTest t = (UnitTest)this.regressionStore.GetValue(it, 0);
			if (t != null)
			{
				this.FillDetails(t, true);
			}
		}
		protected virtual void OnFailedTestActivated(object sender, RowActivatedArgs args)
		{
			TreeIter it;
			this.failedStore.GetIter(ref it, args.get_Path());
			UnitTest t = (UnitTest)this.failedStore.GetValue(it, 0);
			if (t != null)
			{
				this.FillDetails(t, true);
			}
		}
		private void OnZoomIn(object sender, System.EventArgs a)
		{
			if (this.detailsTest != null)
			{
				this.chart.ZoomIn();
			}
		}
		private void OnZoomOut(object sender, System.EventArgs a)
		{
			if (this.detailsTest != null)
			{
				this.chart.ZoomOut();
			}
		}
		private void OnChartBack(object sender, System.EventArgs a)
		{
			if (this.detailsTest != null)
			{
				this.chart.GoPrevious();
			}
		}
		private void OnChartForward(object sender, System.EventArgs a)
		{
			if (this.detailsTest != null)
			{
				this.chart.GoNext();
			}
		}
		private void OnChartLast(object sender, System.EventArgs a)
		{
			if (this.detailsTest != null)
			{
				this.chart.GoLast();
			}
		}
		private void OnChartPopupMenu(object o, ButtonReleaseEventArgs args)
		{
			if (args.get_Event().get_Button() == 3u)
			{
				IdeApp.get_CommandService().ShowContextMenu("/MonoDevelop/NUnit/ContextMenu/TestChart");
			}
		}
		[CommandHandler(TestChartCommands.ShowResults)]
		protected void OnShowResults()
		{
			this.chart.Type = TestChartType.Results;
		}
		[CommandUpdateHandler(TestChartCommands.ShowResults)]
		protected void OnUpdateShowResults(CommandInfo info)
		{
			info.set_Checked(this.chart.Type == TestChartType.Results);
		}
		[CommandHandler(TestChartCommands.ShowTime)]
		protected void OnShowTime()
		{
			this.chart.Type = TestChartType.Time;
		}
		[CommandUpdateHandler(TestChartCommands.ShowTime)]
		protected void OnUpdateShowTime(CommandInfo info)
		{
			info.set_Checked(this.chart.Type == TestChartType.Time);
		}
		[CommandHandler(TestChartCommands.UseTimeScale)]
		protected void OnUseTimeScale()
		{
			this.chart.UseTimeScale = !this.chart.UseTimeScale;
		}
		[CommandUpdateHandler(TestChartCommands.UseTimeScale)]
		protected void OnUpdateUseTimeScale(CommandInfo info)
		{
			info.set_Checked(this.chart.UseTimeScale);
		}
		[CommandHandler(TestChartCommands.SingleDayResult)]
		protected void OnSingleDayResult()
		{
			this.chart.SingleDayResult = !this.chart.SingleDayResult;
		}
		[CommandUpdateHandler(TestChartCommands.SingleDayResult)]
		protected void OnUpdateSingleDayResult(CommandInfo info)
		{
			info.set_Checked(this.chart.SingleDayResult);
		}
		[CommandHandler(TestChartCommands.ShowSuccessfulTests)]
		protected void OnShowSuccessfulTests()
		{
			this.chart.ShowSuccessfulTests = !this.chart.ShowSuccessfulTests;
		}
		[CommandUpdateHandler(TestChartCommands.ShowSuccessfulTests)]
		protected void OnUpdateShowSuccessfulTests(CommandInfo info)
		{
			info.set_Enabled(this.chart.Type == TestChartType.Results);
			info.set_Checked(this.chart.ShowSuccessfulTests);
		}
		[CommandHandler(TestChartCommands.ShowFailedTests)]
		protected void OnShowFailedTests()
		{
			this.chart.ShowFailedTests = !this.chart.ShowFailedTests;
		}
		[CommandUpdateHandler(TestChartCommands.ShowFailedTests)]
		protected void OnUpdateShowFailedTests(CommandInfo info)
		{
			info.set_Enabled(this.chart.Type == TestChartType.Results);
			info.set_Checked(this.chart.ShowFailedTests);
		}
		[CommandHandler(TestChartCommands.ShowIgnoredTests)]
		protected void OnShowIgnoredTests()
		{
			this.chart.ShowIgnoredTests = !this.chart.ShowIgnoredTests;
		}
		[CommandUpdateHandler(TestChartCommands.ShowIgnoredTests)]
		protected void OnUpdateShowIgnoredTests(CommandInfo info)
		{
			info.set_Enabled(this.chart.Type == TestChartType.Results);
			info.set_Checked(this.chart.ShowIgnoredTests);
		}
	}
}
