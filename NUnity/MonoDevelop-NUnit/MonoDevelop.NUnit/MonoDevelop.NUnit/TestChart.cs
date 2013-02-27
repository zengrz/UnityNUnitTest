using Cairo;
using MonoDevelop.Components.Chart;
using System;
using System.Collections;
namespace MonoDevelop.NUnit
{
	public class TestChart : BasicChart
	{
		private Serie serieFailed;
		private Serie serieSuccess;
		private Serie serieIgnored;
		private Serie serieTime;
		private bool timeScale = false;
		private bool singleDayResult = false;
		private TestChartType type;
		private System.TimeSpan currentSpan = System.TimeSpan.FromDays(5.0);
		private int testCount = 20;
		private UnitTest test;
		private bool showLastTest = true;
		private bool resetCursors = true;
		private double lastDateValue;
		private double lastTestNumber;
		private UnitTestResult[] currentResults;
		private TestRunAxis testRunAxis;
		public bool ShowSuccessfulTests
		{
			get
			{
				return this.serieSuccess.get_Visible();
			}
			set
			{
				this.serieSuccess.set_Visible(value);
			}
		}
		public bool ShowFailedTests
		{
			get
			{
				return this.serieFailed.get_Visible();
			}
			set
			{
				this.serieFailed.set_Visible(value);
			}
		}
		public bool ShowIgnoredTests
		{
			get
			{
				return this.serieIgnored.get_Visible();
			}
			set
			{
				this.serieIgnored.set_Visible(value);
			}
		}
		public bool UseTimeScale
		{
			get
			{
				return this.timeScale;
			}
			set
			{
				this.timeScale = value;
				this.UpdateMode();
			}
		}
		public bool SingleDayResult
		{
			get
			{
				return this.singleDayResult;
			}
			set
			{
				this.singleDayResult = value;
				this.UpdateMode();
			}
		}
		public TestChartType Type
		{
			get
			{
				return this.type;
			}
			set
			{
				this.type = value;
				this.UpdateMode();
			}
		}
		public System.DateTime CurrentDate
		{
			get
			{
				System.DateTime result;
				if (this.timeScale)
				{
					result = new System.DateTime((long)base.get_SelectionEnd().get_Value());
				}
				else
				{
					int i = (int)base.get_SelectionStart().get_Value();
					if (this.currentResults != null && i >= 0 && i < this.currentResults.Length)
					{
						result = this.currentResults[this.currentResults.Length - i - 1].TestDate;
					}
					else
					{
						result = System.DateTime.MinValue;
					}
				}
				return result;
			}
		}
		public System.DateTime ReferenceDate
		{
			get
			{
				System.DateTime result;
				if (this.timeScale)
				{
					result = new System.DateTime((long)base.get_SelectionStart().get_Value());
				}
				else
				{
					int i = (int)base.get_SelectionEnd().get_Value();
					if (this.currentResults != null && i >= 0 && i < this.currentResults.Length)
					{
						result = this.currentResults[this.currentResults.Length - i - 1].TestDate;
					}
					else
					{
						result = System.DateTime.MinValue;
					}
				}
				return result;
			}
		}
		public TestChart()
		{
			base.set_AllowSelection(true);
			base.SetAutoScale(1, false, true);
			base.set_StartY(0.0);
			this.serieFailed = new Serie("Failed tests");
			this.serieFailed.set_Color(new Color(1.0, 0.0, 0.0));
			this.serieSuccess = new Serie("Successful tests");
			this.serieSuccess.set_Color(new Color(0.0, 0.65, 0.0));
			this.serieIgnored = new Serie("Ignored tests");
			this.serieIgnored.set_Color(new Color(0.8, 0.8, 0.0));
			this.serieTime = new Serie("Time");
			this.serieTime.set_Color(new Color(0.0, 0.0, 1.0));
			this.UpdateMode();
			base.set_EndX(5.0);
			base.set_StartX(0.0);
		}
		private void UpdateMode()
		{
			base.set_AllowSelection(false);
			base.Reset();
			if (this.type == TestChartType.Results)
			{
				base.AddSerie(this.serieIgnored);
				base.AddSerie(this.serieFailed);
				base.AddSerie(this.serieSuccess);
			}
			else
			{
				base.AddSerie(this.serieTime);
			}
			if (this.timeScale)
			{
				base.set_ReverseXAxis(false);
				Axis ax = new DateTimeAxis(true);
				base.AddAxis(new DateTimeAxis(false), 1);
				base.AddAxis(ax, 3);
				ChartCursor arg_B1_0 = base.get_SelectionEnd();
				double value;
				base.get_SelectionStart().set_Value(value = (double)System.DateTime.Now.Ticks);
				arg_B1_0.set_Value(value);
				base.get_SelectionStart().set_LabelAxis(ax);
				base.get_SelectionEnd().set_LabelAxis(ax);
			}
			else
			{
				base.set_ReverseXAxis(true);
				base.AddAxis(new TestRunAxis(false), 1);
				this.testRunAxis = new TestRunAxis(true);
				base.AddAxis(this.testRunAxis, 3);
				ChartCursor arg_123_0 = base.get_SelectionEnd();
				double value;
				base.get_SelectionStart().set_Value(value = 0.0);
				arg_123_0.set_Value(value);
				base.get_SelectionStart().set_LabelAxis(this.testRunAxis);
				base.get_SelectionEnd().set_LabelAxis(this.testRunAxis);
			}
			this.showLastTest = true;
			this.resetCursors = true;
			base.AddAxis(new IntegerAxis(true), 0);
			base.AddAxis(new IntegerAxis(true), 2);
			if (this.test != null)
			{
				this.Fill(this.test);
			}
			base.set_AllowSelection(true);
		}
		public void Clear()
		{
			base.Clear();
			this.test = null;
		}
		public void ZoomIn()
		{
			if (this.test != null)
			{
				if (this.timeScale)
				{
					this.currentSpan = new System.TimeSpan(this.currentSpan.Ticks / 2L);
					if (this.currentSpan.TotalSeconds < 60.0)
					{
						this.currentSpan = System.TimeSpan.FromSeconds(60.0);
					}
				}
				else
				{
					this.testCount /= 2;
					if (this.testCount < 5)
					{
						this.testCount = 5;
					}
				}
				this.Fill(this.test);
			}
		}
		public void ZoomOut()
		{
			if (this.test != null)
			{
				if (this.timeScale)
				{
					this.currentSpan = new System.TimeSpan(this.currentSpan.Ticks * 2L);
					if (this.currentSpan.TotalDays > 18250.0)
					{
						this.currentSpan = System.TimeSpan.FromDays(18250.0);
					}
				}
				else
				{
					this.testCount *= 2;
					if (this.testCount > 100000)
					{
						this.testCount = 100000;
					}
				}
				this.Fill(this.test);
			}
		}
		public void GoNext()
		{
			if (!this.showLastTest)
			{
				if (this.timeScale)
				{
					this.lastDateValue += (base.get_EndX() - base.get_StartX()) / 3.0;
					UnitTestResult lastResult = this.test.Results.GetLastResult(System.DateTime.Now);
					if (lastResult != null && new System.DateTime((long)this.lastDateValue) > lastResult.TestDate)
					{
						this.showLastTest = true;
					}
				}
				else
				{
					this.lastTestNumber -= (base.get_EndX() - base.get_StartX()) / 3.0;
					if (this.lastTestNumber < 0.0)
					{
						this.showLastTest = true;
					}
				}
				this.Fill(this.test);
			}
		}
		public void GoPrevious()
		{
			if (this.timeScale)
			{
				this.lastDateValue -= (base.get_EndX() - base.get_StartX()) / 3.0;
			}
			else
			{
				this.lastTestNumber += (base.get_EndX() - base.get_StartX()) / 3.0;
			}
			this.showLastTest = false;
			this.Fill(this.test);
		}
		public void GoLast()
		{
			this.showLastTest = true;
			this.resetCursors = true;
			this.Fill(this.test);
		}
		public void Fill(UnitTest test)
		{
			this.serieFailed.Clear();
			this.serieSuccess.Clear();
			this.serieIgnored.Clear();
			this.serieTime.Clear();
			this.test = test;
			if (this.showLastTest)
			{
				if (this.timeScale)
				{
					System.DateTime dateTime = System.DateTime.Now;
					this.lastDateValue = (double)dateTime.Ticks;
				}
				else
				{
					this.lastTestNumber = 0.0;
				}
			}
			UnitTestResult first = null;
			UnitTestResult lastResult = test.Results.GetLastResult(System.DateTime.Now);
			if (lastResult != null)
			{
				UnitTestResult[] results;
				if (this.timeScale)
				{
					System.DateTime startDate;
					if (this.showLastTest)
					{
						startDate = lastResult.TestDate - this.currentSpan;
						base.set_StartX((double)startDate.Ticks);
						System.DateTime dateTime = lastResult.TestDate;
						base.set_EndX((double)dateTime.Ticks);
						first = test.Results.GetLastResult(startDate);
						results = test.Results.GetResults(startDate, lastResult.TestDate);
					}
					else
					{
						System.DateTime endDate = new System.DateTime((long)this.lastDateValue);
						startDate = endDate - this.currentSpan;
						base.set_StartX((double)startDate.Ticks);
						base.set_EndX((double)endDate.Ticks);
						first = test.Results.GetLastResult(startDate);
						results = test.Results.GetResults(startDate, lastResult.TestDate);
					}
					if (this.singleDayResult)
					{
						first = test.Results.GetPreviousResult(new System.DateTime(startDate.Year, startDate.Month, startDate.Day));
						System.Collections.ArrayList list = new System.Collections.ArrayList();
						if (first != null)
						{
							list.Add(first);
						}
						for (int i = 0; i < results.Length - 1; i++)
						{
							System.DateTime d = results[i].TestDate;
							System.DateTime d2 = results[i + 1].TestDate;
							if (d.Day != d2.Day || d.Month != d2.Month || d.Year != d2.Year)
							{
								list.Add(results[i]);
							}
						}
						list.Add(results[results.Length - 1]);
						results = (UnitTestResult[])list.ToArray(typeof(UnitTestResult));
					}
					if (this.resetCursors)
					{
						base.get_SelectionEnd().set_Value(base.get_EndX());
						if (results.Length > 1)
						{
							ChartCursor arg_2BE_0 = base.get_SelectionStart();
							System.DateTime dateTime = results[results.Length - 2].TestDate;
							arg_2BE_0.set_Value((double)dateTime.Ticks);
						}
						else
						{
							base.get_SelectionStart().set_Value(base.get_EndX());
						}
						this.resetCursors = false;
					}
				}
				else
				{
					if (this.singleDayResult)
					{
						System.Collections.ArrayList list = new System.Collections.ArrayList();
						list.Add(lastResult);
						while (list.Count < this.testCount + (int)this.lastTestNumber + 1)
						{
							UnitTestResult res = test.Results.GetPreviousResult(lastResult.TestDate);
							if (res == null)
							{
								break;
							}
							System.DateTime dateTime = res.TestDate;
							int arg_355_0 = dateTime.Day;
							dateTime = lastResult.TestDate;
							if (arg_355_0 != dateTime.Day)
							{
								goto IL_39B;
							}
							dateTime = res.TestDate;
							int arg_376_0 = dateTime.Month;
							dateTime = lastResult.TestDate;
							if (arg_376_0 != dateTime.Month)
							{
								goto IL_39B;
							}
							dateTime = res.TestDate;
							int arg_397_0 = dateTime.Year;
							dateTime = lastResult.TestDate;
							bool arg_39C_0 = arg_397_0 == dateTime.Year;
							IL_39C:
							if (!arg_39C_0)
							{
								list.Add(res);
							}
							lastResult = res;
							continue;
							IL_39B:
							arg_39C_0 = false;
							goto IL_39C;
						}
						results = (UnitTestResult[])list.ToArray(typeof(UnitTestResult));
						System.Array.Reverse(results);
					}
					else
					{
						results = test.Results.GetResultsToDate(System.DateTime.Now, this.testCount + (int)this.lastTestNumber + 1);
					}
					base.set_EndX(this.lastTestNumber + (double)this.testCount);
					base.set_StartX(this.lastTestNumber);
					if (this.resetCursors)
					{
						base.get_SelectionStart().set_Value(base.get_StartX());
						base.get_SelectionEnd().set_Value(base.get_StartX() + 1.0);
						this.resetCursors = false;
					}
				}
				this.currentResults = results;
				if (this.testRunAxis != null)
				{
					this.testRunAxis.CurrentResults = this.currentResults;
				}
				if (this.Type == TestChartType.Results)
				{
					if (first != null)
					{
						double arg_4E4_0;
						if (!this.timeScale)
						{
							arg_4E4_0 = (double)((long)results.Length);
						}
						else
						{
							System.DateTime dateTime = first.TestDate;
							arg_4E4_0 = (double)dateTime.Ticks;
						}
						double x = arg_4E4_0;
						this.serieFailed.AddData(x, (double)first.TotalFailures);
						this.serieSuccess.AddData(x, (double)first.TotalSuccess);
						this.serieIgnored.AddData(x, (double)first.TotalIgnored);
					}
					for (int i = 0; i < results.Length; i++)
					{
						UnitTestResult res = results[i];
						double arg_556_0;
						if (!this.timeScale)
						{
							arg_556_0 = (double)((long)(results.Length - i - 1));
						}
						else
						{
							System.DateTime dateTime = res.TestDate;
							arg_556_0 = (double)dateTime.Ticks;
						}
						double x = arg_556_0;
						this.serieFailed.AddData(x, (double)res.TotalFailures);
						this.serieSuccess.AddData(x, (double)res.TotalSuccess);
						this.serieIgnored.AddData(x, (double)res.TotalIgnored);
					}
				}
				else
				{
					if (first != null)
					{
						double arg_5E1_0;
						if (!this.timeScale)
						{
							arg_5E1_0 = (double)((long)results.Length);
						}
						else
						{
							System.DateTime dateTime = first.TestDate;
							arg_5E1_0 = (double)dateTime.Ticks;
						}
						double x = arg_5E1_0;
						Serie arg_5FB_0 = this.serieTime;
						double arg_5FB_1 = x;
						System.TimeSpan time = first.Time;
						arg_5FB_0.AddData(arg_5FB_1, time.TotalMilliseconds);
					}
					for (int i = 0; i < results.Length; i++)
					{
						UnitTestResult res = results[i];
						double arg_631_0;
						if (!this.timeScale)
						{
							arg_631_0 = (double)((long)(results.Length - i - 1));
						}
						else
						{
							System.DateTime dateTime = res.TestDate;
							arg_631_0 = (double)dateTime.Ticks;
						}
						double x = arg_631_0;
						Serie arg_64E_0 = this.serieTime;
						double arg_64E_1 = x;
						System.TimeSpan time = results[i].Time;
						arg_64E_0.AddData(arg_64E_1, time.TotalMilliseconds);
					}
				}
			}
		}
	}
}
