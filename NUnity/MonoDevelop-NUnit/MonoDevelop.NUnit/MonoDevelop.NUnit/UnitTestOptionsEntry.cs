using MonoDevelop.Core.Serialization;
using System;
using System.Collections;
namespace MonoDevelop.NUnit
{
	internal class UnitTestOptionsEntry
	{
		[ItemProperty("Path")]
		public string Path;
		[ItemProperty("Options"), ExpandedCollection]
		public System.Collections.ArrayList Options = new System.Collections.ArrayList();
	}
}
