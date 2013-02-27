using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Projects;
using System;
namespace MonoDevelop.NUnit
{
	public class NUnitProjectServiceExtension : ProjectServiceExtension
	{
		public override void Execute(IProgressMonitor monitor, IBuildTarget item, ExecutionContext context, ConfigurationSelector configuration)
		{
			if (base.CanExecute(item, context, configuration))
			{
				base.Execute(monitor, item, context, configuration);
			}
			else
			{
				if (item != null)
				{
					UnitTest test = NUnitService.Instance.FindRootTest(item);
					if (test != null)
					{
						IAsyncOperation oper = null;
						DispatchService.GuiSyncDispatch(delegate
						{
							oper = NUnitService.Instance.RunTest(test, context.get_ExecutionHandler(), false);
						}
						);
						if (oper != null)
						{
							monitor.add_CancelRequested(delegate
							{
								oper.Cancel();
							}
							);
							oper.WaitForCompleted();
						}
					}
				}
			}
		}
		public override bool CanExecute(IBuildTarget item, ExecutionContext context, ConfigurationSelector configuration)
		{
			bool res = base.CanExecute(item, context, configuration);
			bool result;
			if (!res && item != null)
			{
				UnitTest test = NUnitService.Instance.FindRootTest(item);
				result = (test != null && test.CanRun(context.get_ExecutionHandler()));
			}
			else
			{
				result = res;
			}
			return result;
		}
	}
}
