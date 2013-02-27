using MonoDevelop.Core;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;
namespace MonoDevelop.NUnit
{
	public class XmlResultsStore : IResultsStore
	{
		private System.Collections.Hashtable fileCache = new System.Collections.Hashtable();
		private string basePath;
		private string storeId;
		private System.Collections.Hashtable cachedRootList = new System.Collections.Hashtable();
		private static XmlSerializer serializer = new XmlSerializer(typeof(TestRecord));
		public XmlResultsStore(string directory, string storeId)
		{
			this.basePath = directory;
			this.storeId = storeId;
		}
		public void RegisterResult(string configuration, UnitTest test, UnitTestResult result)
		{
			string aname = test.StoreRelativeName;
			TestRecord root = this.GetRootRecord(configuration, result.TestDate);
			if (root == null)
			{
				root = new TestRecord();
				this.fileCache[this.GetRootFileName(configuration, result.TestDate)] = root;
			}
			root.Modified = true;
			TestRecord record = root;
			if (aname.Length > 0)
			{
				string[] path = test.StoreRelativeName.Split(new char[]
				{
					'.'
				});
				string[] array = path;
				for (int i = 0; i < array.Length; i++)
				{
					string p = array[i];
					TestRecord ctr = (record.Tests != null) ? record.Tests[p] : null;
					if (ctr == null)
					{
						ctr = new TestRecord();
						ctr.Name = p;
						if (record.Tests == null)
						{
							record.Tests = new TestRecordCollection();
						}
						record.Tests.Add(ctr);
					}
					record = ctr;
				}
			}
			if (record.Results == null)
			{
				record.Results = new UnitTestResultCollection();
			}
			record.Results.Add(result);
		}
		public UnitTestResult GetNextResult(string configuration, UnitTest test, System.DateTime date)
		{
			System.DateTime currentDate = date;
			TestRecord root = this.GetRootRecord(configuration, currentDate);
			if (root == null)
			{
				root = this.GetNextRootRecord(configuration, ref currentDate);
			}
			UnitTestResult result;
			while (root != null)
			{
				TestRecord tr = this.FindRecord(root, test.StoreRelativeName);
				if (tr != null && tr.Results != null)
				{
					foreach (UnitTestResult res in tr.Results)
					{
						if (res.TestDate > date)
						{
							result = res;
							return result;
						}
					}
				}
				root = this.GetNextRootRecord(configuration, ref currentDate);
			}
			result = null;
			return result;
		}
		public UnitTestResult GetPreviousResult(string configuration, UnitTest test, System.DateTime date)
		{
			System.DateTime currentDate = date;
			TestRecord root = this.GetRootRecord(configuration, currentDate);
			if (root == null)
			{
				root = this.GetPreviousRootRecord(configuration, ref currentDate);
			}
			UnitTestResult result;
			while (root != null)
			{
				TestRecord tr = this.FindRecord(root, test.StoreRelativeName);
				if (tr != null && tr.Results != null)
				{
					for (int i = tr.Results.Count - 1; i >= 0; i--)
					{
						UnitTestResult res = tr.Results[i];
						if (res.TestDate < date)
						{
							result = res;
							return result;
						}
					}
				}
				root = this.GetPreviousRootRecord(configuration, ref currentDate);
			}
			result = null;
			return result;
		}
		public UnitTestResult GetLastResult(string configuration, UnitTest test, System.DateTime date)
		{
			return this.GetPreviousResult(configuration, test, date.AddTicks(1L));
		}
		public UnitTestResult[] GetResults(string configuration, UnitTest test, System.DateTime startDate, System.DateTime endDate)
		{
			System.Collections.ArrayList list = new System.Collections.ArrayList();
			System.DateTime firstDay = new System.DateTime(startDate.Year, startDate.Month, startDate.Day);
			System.DateTime[] dates = this.GetStoreDates(configuration);
			System.DateTime[] array = dates;
			for (int i = 0; i < array.Length; i++)
			{
				System.DateTime date = array[i];
				if (!(date < firstDay))
				{
					if (date > endDate)
					{
						break;
					}
					TestRecord root = this.GetRootRecord(configuration, date);
					if (root != null)
					{
						TestRecord tr = this.FindRecord(root, test.StoreRelativeName);
						if (tr != null && tr.Results != null)
						{
							foreach (UnitTestResult res in tr.Results)
							{
								if (res.TestDate >= startDate && res.TestDate <= endDate)
								{
									list.Add(res);
								}
							}
						}
					}
				}
			}
			return (UnitTestResult[])list.ToArray(typeof(UnitTestResult));
		}
		public UnitTestResult[] GetResultsToDate(string configuration, UnitTest test, System.DateTime endDate, int count)
		{
			System.Collections.ArrayList list = new System.Collections.ArrayList();
			System.DateTime[] dates = this.GetStoreDates(configuration);
			int i = dates.Length - 1;
			while (i >= 0 && list.Count < count)
			{
				if (!(dates[i] > endDate))
				{
					TestRecord root = this.GetRootRecord(configuration, dates[i]);
					if (root != null)
					{
						TestRecord tr = this.FindRecord(root, test.StoreRelativeName);
						if (tr != null && tr.Results != null)
						{
							int j = tr.Results.Count - 1;
							while (j >= 0 && list.Count < count)
							{
								UnitTestResult res = tr.Results[j];
								if (res.TestDate <= endDate)
								{
									list.Add(res);
								}
								j--;
							}
						}
					}
				}
				i--;
			}
			UnitTestResult[] array = (UnitTestResult[])list.ToArray(typeof(UnitTestResult));
			System.Array.Reverse(array);
			return array;
		}
		public void Save()
		{
			if (!System.IO.Directory.Exists(this.basePath))
			{
				System.IO.Directory.CreateDirectory(this.basePath);
			}
			foreach (System.Collections.DictionaryEntry entry in this.fileCache)
			{
				TestRecord record = (TestRecord)entry.Value;
				if (record.Modified)
				{
					string file = System.IO.Path.Combine(this.basePath, (string)entry.Key);
					System.IO.StreamWriter writer = new System.IO.StreamWriter(file);
					try
					{
						XmlResultsStore.serializer.Serialize(writer, record);
					}
					finally
					{
						writer.Close();
					}
					record.Modified = false;
				}
			}
			this.cachedRootList.Clear();
		}
		private TestRecord FindRecord(TestRecord root, string aname)
		{
			TestRecord result;
			if (aname.Length == 0)
			{
				result = root;
			}
			else
			{
				string[] path = aname.Split(new char[]
				{
					'.'
				});
				TestRecord tr = root;
				string[] array = path;
				for (int i = 0; i < array.Length; i++)
				{
					string p = array[i];
					if (tr.Tests == null)
					{
						result = null;
						return result;
					}
					tr = tr.Tests[p];
					if (tr == null)
					{
						result = null;
						return result;
					}
				}
				result = tr;
			}
			return result;
		}
		private TestRecord GetRootRecord(string configuration, System.DateTime date)
		{
			string file = this.GetRootFileName(configuration, date);
			TestRecord res = (TestRecord)this.fileCache[file];
			TestRecord result;
			if (res != null)
			{
				result = res;
			}
			else
			{
				string filePath = System.IO.Path.Combine(this.basePath, file);
				if (!System.IO.File.Exists(filePath))
				{
					result = null;
				}
				else
				{
					System.IO.StreamReader s = new System.IO.StreamReader(filePath);
					try
					{
						res = (TestRecord)XmlResultsStore.serializer.Deserialize(s);
					}
					catch (System.Exception ex)
					{
						LoggingService.LogError(ex.ToString());
						result = null;
						return result;
					}
					finally
					{
						s.Close();
					}
					this.fileCache[file] = res;
					result = res;
				}
			}
			return result;
		}
		private TestRecord GetNextRootRecord(string configuration, ref System.DateTime date)
		{
			System.DateTime[] dates = this.GetStoreDates(configuration);
			System.DateTime[] array = dates;
			TestRecord result;
			for (int i = 0; i < array.Length; i++)
			{
				System.DateTime d = array[i];
				if (d > date)
				{
					date = d;
					result = this.GetRootRecord(configuration, d);
					return result;
				}
			}
			result = null;
			return result;
		}
		private TestRecord GetPreviousRootRecord(string configuration, ref System.DateTime date)
		{
			date = new System.DateTime(date.Year, date.Month, date.Day);
			System.DateTime[] dates = this.GetStoreDates(configuration);
			TestRecord result;
			for (int i = dates.Length - 1; i >= 0; i--)
			{
				if (dates[i] < date)
				{
					date = dates[i];
					result = this.GetRootRecord(configuration, dates[i]);
					return result;
				}
			}
			result = null;
			return result;
		}
		private string GetRootFileName(string configuration, System.DateTime date)
		{
			return string.Concat(new string[]
			{
				this.storeId, 
				"-", 
				configuration, 
				"-", 
				date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture), 
				".xml"
			});
		}
		private System.DateTime ParseFileNameDate(string configuration, string fileName)
		{
			fileName = System.IO.Path.GetFileNameWithoutExtension(fileName);
			fileName = fileName.Substring(this.storeId.Length + configuration.Length + 2);
			return System.DateTime.ParseExact(fileName, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
		}
		private System.DateTime[] GetStoreDates(string configuration)
		{
			System.DateTime[] result;
			if (!System.IO.Directory.Exists(this.basePath))
			{
				result = new System.DateTime[0];
			}
			else
			{
				System.DateTime[] res = (System.DateTime[])this.cachedRootList[configuration];
				if (res != null)
				{
					result = res;
				}
				else
				{
					System.Collections.ArrayList dates = new System.Collections.ArrayList();
					string[] files = System.IO.Directory.GetFiles(this.basePath, this.storeId + "-" + configuration + "-*");
					for (int i = 0; i < files.Length; i++)
					{
						string file = files[i];
						try
						{
							System.DateTime t = this.ParseFileNameDate(configuration, System.IO.Path.GetFileName(file));
							dates.Add(t);
						}
						catch
						{
						}
					}
					res = (System.DateTime[])dates.ToArray(typeof(System.DateTime));
					this.cachedRootList[configuration] = res;
					result = res;
				}
			}
			return result;
		}
	}
}
