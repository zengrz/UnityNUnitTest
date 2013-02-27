using Gtk;
using System;
namespace Stetic
{
	internal class Gui
	{
		private static bool initialized;
		internal static void Initialize(Widget iconRenderer)
		{
			if (!Gui.initialized)
			{
				Gui.initialized = true;
			}
		}
	}
}
