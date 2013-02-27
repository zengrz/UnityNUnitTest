using System;
using System.Collections;
namespace MonoDevelop.NUnit
{
	public class OptionsData
	{
		private string configuration;
		private System.Collections.ICollection options;
		public string Configuration
		{
			get
			{
				return this.configuration;
			}
		}
		public System.Collections.ICollection Options
		{
			get
			{
				return this.options;
			}
		}
		public OptionsData(string configuration, System.Collections.ICollection options)
		{
			this.configuration = configuration;
			this.options = options;
		}
	}
}
