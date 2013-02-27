using System;
using System.Xml.Serialization;
namespace MonoDevelop.NUnit
{
	public class TestRecord
	{
		private string name;
		private UnitTestResultCollection results;
		private TestRecordCollection tests;
		internal bool Modified;
		[XmlAttribute]
		public string Name
		{
			get
			{
				return this.name;
			}
			set
			{
				this.name = value;
			}
		}
		public UnitTestResultCollection Results
		{
			get
			{
				return this.results;
			}
			set
			{
				this.results = value;
			}
		}
		public TestRecordCollection Tests
		{
			get
			{
				return this.tests;
			}
			set
			{
				this.tests = value;
			}
		}
	}
}
