using System;
namespace MonoDevelop.NUnit
{
	public class SourceCodeLocation
	{
		private string fileName;
		private int line;
		private int column;
		public string FileName
		{
			get
			{
				return this.fileName;
			}
		}
		public int Line
		{
			get
			{
				return this.line;
			}
		}
		public int Column
		{
			get
			{
				return this.column;
			}
		}
		public SourceCodeLocation(string fileName, int line, int column)
		{
			this.fileName = fileName;
			this.line = line;
			this.column = column;
		}
	}
}
