using Gtk;
using System;
namespace Stetic
{
	internal class ActionGroups
	{
		public static ActionGroup GetActionGroup(System.Type type)
		{
			return ActionGroups.GetActionGroup(type.FullName);
		}
		public static ActionGroup GetActionGroup(string name)
		{
			return null;
		}
	}
}
