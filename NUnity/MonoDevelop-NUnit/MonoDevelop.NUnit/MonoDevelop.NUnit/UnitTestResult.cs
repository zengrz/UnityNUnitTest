using System;
using System.IO;
using System.Text.RegularExpressions;
namespace MonoDevelop.NUnit
{
	[System.Serializable]
	public class UnitTestResult
	{
		private System.DateTime testDate;
		private ResultStatus status;
		private System.TimeSpan time;
		private string message;
		private string output;
		private string stackTrace;
		private int totalFailures;
		private int totalSuccess;
		private int totalIgnored;
		private string cerror;
		public System.DateTime TestDate
		{
			get
			{
				return this.testDate;
			}
			set
			{
				this.testDate = value;
			}
		}
		public ResultStatus Status
		{
			get
			{
				return this.status;
			}
			set
			{
				this.status = value;
			}
		}
		public bool IsFailure
		{
			get
			{
				return (this.status & ResultStatus.Failure) != (ResultStatus)0;
			}
		}
		public bool IsIgnored
		{
			get
			{
				return (this.status & ResultStatus.Ignored) != (ResultStatus)0;
			}
		}
		public bool IsSuccess
		{
			get
			{
				return (this.status & ResultStatus.Success) != (ResultStatus)0;
			}
		}
		public int TotalFailures
		{
			get
			{
				return this.totalFailures;
			}
			set
			{
				this.totalFailures = value;
			}
		}
		public int TotalSuccess
		{
			get
			{
				return this.totalSuccess;
			}
			set
			{
				this.totalSuccess = value;
			}
		}
		public int TotalIgnored
		{
			get
			{
				return this.totalIgnored;
			}
			set
			{
				this.totalIgnored = value;
			}
		}
		public System.TimeSpan Time
		{
			get
			{
				return this.time;
			}
			set
			{
				this.time = value;
			}
		}
		public string Message
		{
			get
			{
				return this.message;
			}
			set
			{
				this.message = value;
			}
		}
		public string StackTrace
		{
			get
			{
				return this.stackTrace;
			}
			set
			{
				this.stackTrace = value;
			}
		}
		public string ConsoleOutput
		{
			get
			{
				return this.output;
			}
			set
			{
				this.output = value;
			}
		}
		public string ConsoleError
		{
			get
			{
				return this.cerror;
			}
			set
			{
				this.cerror = value;
			}
		}
		public static UnitTestResult CreateFailure(System.Exception ex)
		{
			return new UnitTestResult
			{
				status = ResultStatus.Failure, 
				Message = ex.Message, 
				stackTrace = ex.StackTrace
			};
		}
		public static UnitTestResult CreateFailure(string message, System.Exception ex)
		{
			return new UnitTestResult
			{
				status = ResultStatus.Failure, 
				Message = message, 
				stackTrace = ex.Message + "\n" + ex.StackTrace
			};
		}
		public static UnitTestResult CreateIgnored(string message)
		{
			return new UnitTestResult
			{
				status = ResultStatus.Ignored, 
				Message = message
			};
		}
		public static UnitTestResult CreateSuccess()
		{
			return new UnitTestResult
			{
				status = ResultStatus.Success
			};
		}
		public SourceCodeLocation GetFailureLocation()
		{
			SourceCodeLocation result;
			if (string.IsNullOrEmpty(this.stackTrace))
			{
				result = null;
			}
			else
			{
				string[] stackLines = this.stackTrace.Replace("\r", "").Split(new char[]
				{
					'\n'
				});
				string[] array = stackLines;
				for (int j = 0; j < array.Length; j++)
				{
					string line = array[j];
					if (line.IndexOf("NUnit.Framework") == -1)
					{
						Regex r = new Regex(".*?\\(.*?\\)\\s\\[.*?\\]\\s.*?\\s(?<file>.*)\\:(?<line>\\d*)");
						Match i = r.Match(line);
						if (i.Groups["file"] != null && i.Groups["line"] != null && System.IO.File.Exists(i.Groups["file"].Value))
						{
							int lin;
							if (int.TryParse(i.Groups["line"].Value, out lin))
							{
								result = new SourceCodeLocation(i.Groups["file"].Value, lin, -1);
								return result;
							}
						}
					}
				}
				result = null;
			}
			return result;
		}
	}
}
