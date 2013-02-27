using Gdk;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui.Components;
using System;
namespace MonoDevelop.NUnit
{
	public class TestNodeBuilder : TypeNodeBuilder
	{
		private System.EventHandler testChanged;
		private System.EventHandler testStatusChanged;
		public override System.Type CommandHandlerType
		{
			get
			{
				return typeof(TestNodeCommandHandler);
			}
		}
		public override string ContextMenuAddinPath
		{
			get
			{
				return "/MonoDevelop/NUnit/ContextMenu/TestPad";
			}
		}
		public override System.Type NodeDataType
		{
			get
			{
				return typeof(UnitTest);
			}
		}
		public TestNodeBuilder()
		{
			this.testChanged = DispatchService.GuiDispatch<System.EventHandler>(new System.EventHandler(this.OnTestChanged));
			this.testStatusChanged = DispatchService.GuiDispatch<System.EventHandler>(new System.EventHandler(this.OnTestStatusChanged));
		}
		public override string GetNodeName(ITreeNavigator thisNode, object dataObject)
		{
			return ((UnitTest)dataObject).Name;
		}
		public override void BuildNode(ITreeBuilder treeBuilder, object dataObject, ref string label, ref Pixbuf icon, ref Pixbuf closedIcon)
		{
			UnitTest test = dataObject as UnitTest;
			if (test.Status == TestStatus.Running)
			{
				icon = CircleImage.Running;
				label = test.Title;
			}
			else
			{
				if (test.Status == TestStatus.Loading)
				{
					icon = CircleImage.Loading;
					label = test.Title + GettextCatalog.GetString(" (Loading)");
				}
				else
				{
					if (test.Status == TestStatus.LoadError)
					{
						icon = CircleImage.Failure;
						label = test.Title + GettextCatalog.GetString(" (Load failed)");
					}
					else
					{
						label = test.Title;
						UnitTestResult res = test.GetLastResult();
						if (res == null)
						{
							icon = CircleImage.None;
						}
						else
						{
							if (res.IsFailure && res.IsSuccess)
							{
								icon = CircleImage.SuccessAndFailure;
							}
							else
							{
								if (res.IsFailure)
								{
									icon = CircleImage.Failure;
								}
								else
								{
									if (res.IsSuccess)
									{
										icon = CircleImage.Success;
										if (treeBuilder.get_Options().get_Item("ShowTestTime"))
										{
											object obj = label;
											label = string.Concat(new object[]
											{
												obj, 
												" (", 
												res.Time.TotalMilliseconds, 
												" ms)"
											});
										}
									}
									else
									{
										if (res.IsIgnored)
										{
											icon = CircleImage.NotRun;
										}
										else
										{
											icon = CircleImage.None;
										}
									}
								}
							}
						}
						if (res != null && treeBuilder.get_Options().get_Item("ShowTestCounters") && test is UnitTestGroup)
						{
							label += string.Format(GettextCatalog.GetString(" ({0} success, {1} failed, {2} ignored)"), res.TotalSuccess, res.TotalFailures, res.TotalIgnored);
						}
					}
				}
			}
		}
		public override void BuildChildNodes(ITreeBuilder builder, object dataObject)
		{
			UnitTestGroup test = dataObject as UnitTestGroup;
			if (test != null)
			{
				foreach (UnitTest t in test.Tests)
				{
					builder.AddChild(t);
				}
			}
		}
		public override bool HasChildNodes(ITreeBuilder builder, object dataObject)
		{
			UnitTestGroup test = dataObject as UnitTestGroup;
			return test != null && test.Tests.Count > 0;
		}
		public override void OnNodeAdded(object dataObject)
		{
			UnitTest test = (UnitTest)dataObject;
			test.TestChanged += this.testChanged;
			test.TestStatusChanged += this.testStatusChanged;
		}
		public override void OnNodeRemoved(object dataObject)
		{
			UnitTest test = (UnitTest)dataObject;
			test.TestChanged -= this.testChanged;
			test.TestStatusChanged -= this.testStatusChanged;
		}
		public void OnTestChanged(object sender, System.EventArgs args)
		{
			ITreeBuilder tb = base.get_Context().GetTreeBuilder(sender);
			if (tb != null)
			{
				tb.UpdateAll();
			}
		}
		public void OnTestStatusChanged(object sender, System.EventArgs args)
		{
			ITreeBuilder tb = base.get_Context().GetTreeBuilder(sender);
			if (tb != null)
			{
				tb.Update();
			}
		}
	}
}
