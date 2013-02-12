using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

/*
    Title:        APR System
    Description:  Advanced Pattern Recognition using Fuzzy Logic
    Copyright:    Copyright (c) 2002-2010
    Company:      Modulus Financial Engineering, Inc.
    @author       Modulus FE
    @version      2.0
    
    !!! PATENT-PENDING !!!
  
*/


namespace FinLib
{
	public class PatternRecognizer
	{
		private const short ARRAY_LENGTH = 99;

		private readonly List<PatternInterval> _upper = new List<PatternInterval>();
		private readonly List<PatternInterval> _lower = new List<PatternInterval>();
		private readonly int[] _upperPoints = new int[ARRAY_LENGTH];
		private readonly int[] _lowerPoints = new int[ARRAY_LENGTH];
        private readonly List<BarData> _values = new List<BarData>();
		private readonly List<PatternValue> _results = new List<PatternValue>();
		private readonly System.Globalization.NumberFormatInfo _ni;

		public PatternRecognizer()
		{
			System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.InstalledUICulture;
			_ni = (System.Globalization.NumberFormatInfo)ci.NumberFormat.Clone();
			_ni.NumberDecimalSeparator = ".";
		}

		public delegate void ProgressCallbackHandler(object sender, int totalRecords, int currentRecord, ref bool initiateStop);

		public ProgressCallbackHandler ProgressCallback;

		#region Public Properties

		/// <summary>
		/// The pattern title.
		/// </summary>
		public string Title { get; private set; }

		/// <summary>
		/// Optional notes regarding pattern usage.
		/// </summary>
		public string Notes { get; private set; }

		/// <summary>
		/// Maximum number of bars in the pattern window.
		/// </summary>
		public int MaxBars { get; private set; }

		/// <summary>
		/// Minimum number of bars in the pattern window.
		/// </summary>
		public int MinBars { get; private set; }

		/// <summary>
		/// Threshold to filter patterns with a fitness ranking below the specified value.
		/// </summary>
		public double FitnessThreshold { get; private set; }

		/// <summary>
		/// Threshold to filter patterns if any bar is more than n-% outside the channel.
		/// </summary>
		public double BreakoutErrorThreshold { get; private set; }

		/// <summary>
		/// Threshold to filter patterns if any bar is more than n-% outside a data POINTF.
		/// </summary>
		public double PointBreakoutErrorThreshold { get; private set; }

		/// <summary>
		/// Filters patterns that have an accumulative error of abs(high | low - trend) that is more than n-%.
		/// </summary>
		public double ChannelError { get; private set; }

		/// <summary>
		/// Filters patterns that have an accumulative error of abs(high | low - trend) that is more than n-% near data points.
		/// </summary>
		public double PointError { get; private set; }

		/// <summary>
		/// The fitness ranking of the pattern that is found. 'Returned when a pattern is found.
		/// Ranking varies from 0-1. It is almost never above 0.7. A perfect pattern would be 1. Ranks below 0.3 are bad.
		/// </summary>
		public double Ranking { get; private set; }

		/// <summary>
		/// Will have the detected patterns
		/// </summary>
		public IEnumerable<PatternValue> Results { get { return _results; } }

		public int ResultCount { get { return _results.Count; } }

		#endregion

		#region Public Methods
		public void Init()
		{
			_values.Clear();
		}

		public void AppendRecords(IEnumerable<BarData> data)
		{
			_values.AddRange(data);
		}

        public void AppendRecord(BarData data)
		{
			AppendRecords(new[] { data });
		}

		public void AppendRecord(DateTime date, double open, double high, double low, double close, int volume)
		{
            AppendRecord(new BarData
			{
				date = date,
				open = open,
				high = high,
				low = low,
				close = close,
				volume = volume
			});
		}

		public int Scan(string xmlFileName, string licenseCode)
		{
			int record;
			PatternInterval p = new PatternInterval();
			double[] normalOpen = new double[_values.Count];
			double[] normalHigh = new double[_values.Count];
			double[] normalLow = new double[_values.Count];
			double[] normalClose = new double[_values.Count];
			double[] ub = new double[_values.Count];
			double[] lb = new double[_values.Count];

#if DEMO
      if (string.Compare(licenseCode, "demo", true) != 0)
        return -100;
#else
			if (string.Compare(licenseCode, "{2E036561-F762-471d-93FD-869AFE438639}", true) != 0)
				return -100;
#endif

			p.x = p.y = -1;
			_results.Clear();

			for (int i = 0; i < ARRAY_LENGTH; i++)
			{
				_upper.Add(new PatternInterval { x = -1, y = -1 });
				_lower.Add(new PatternInterval { x = -1, y = -1 });
				_upperPoints[i] = _lowerPoints[i] = 0;
			}
			if (!LoadXmlDefinition(xmlFileName))
				return -1;

			double totalPointErr = 0;

			bool cancel = false;
			for (record = 0; !cancel && record < _values.Count; record++)
			{
				if (ProgressCallback != null)
				{
					ProgressCallback(this, _values.Count, record, ref cancel);
				}

				//For min to max periods in window size
				int window;
				for (window = MaxBars - 1; window >= MinBars - 1; window--) //More periods are better so step backwards
				{

					int rightWindowValue = Math.Min(record + window, _values.Count);
					//Normalize this window

					//Get the max value of highs
					//and
					//Get the min value of lows
					bool nextWindow = false;
					if (record + 1 >= _values.Count || window == 0) // Changed 4/7/10
					{
						// Window is past RecordCount
						//window--;
						//nextWindow = true;
						break;
					}


					double max = _values.Skip(record + 1).Take(window).Max(_ => _.high);// _values[record].High;
					double min = _values.Skip(record + 1).Take(window).Min(_ => _.low); //_values[record].Low;
					int n;


					//Normalize all values in this window          
					for (n = record; n <= record + window; n++)
					{
						if (record + window >= _values.Count)
						{
							// Window is past RecordCount
							window--;
							nextWindow = true;
							break;
						}

						normalOpen[n] = Normalize(max, min, _values[n].open);
						normalHigh[n] = Normalize(max, min, _values[n].high);
						normalLow[n] = Normalize(max, min, _values[n].low);
						normalClose[n] = Normalize(max, min, _values[n].close);
					}
					if (nextWindow) continue;

					//First, scale X periods 0 to 1
					//GetX function returns the period within window based on %
					//GetX = scale * [window period]

					//Now, find B and C factors for each POINTF and save in Field
					//B = (Y1 - Y2) / (X1 - X2)
					//C = Y1 - B * X1

					//Next, loop through and calculate running totals for error
					//and closeness of periods to boundaries.
					//If Y - (B * X + C) > 0 Then...
					//If Y - (B * X + C) < 0 Then...

					//Exit loop and increment Record (start period) if
					//total error exceeds oResults.ErrorPct

					//Begin by scaling the pattern to match this window:

					//Clear old values (reset with -1)
					for (int i = 0; i < ub.Length; i++)
					{
						ub[i] = -1;
						lb[i] = -1;
					}

					//Save the scaled x points for use later
					int upperEndArray = EndArray(_upper);
					int lowerEndArray = EndArray(_lower);

					int nearest;
					for (n = 0; n <= upperEndArray; n++)
					{
						nearest = GetNearestPeriod(window, _upper[n].x);
						if (record + nearest >= _values.Count)
						{
							// Window is past RecordCount
							window--;
							nextWindow = true;
							break;
						}
						_upperPoints[n] = record + nearest;
						//Scale upper boundry
						ub[record + nearest] = _upper[n].y;
					}
					if (nextWindow)
						continue;

					for (n = 0; n <= lowerEndArray; n++)
					{
						nearest = GetNearestPeriod(window, _lower[n].x);
						if (record + nearest >= _values.Count)
						{
							// Window is past RecordCount
							window--;
							nextWindow = true;
						}
						_lowerPoints[n] = record + nearest;
						//Scale lower boundry
						lb[record + nearest] = _lower[n].y;
					}
					if (nextWindow)
						continue;

					//Now we have a scaled window and scaled pattern boundaries within the window.
					//The next step is to find the B & C factors and draw a line between each POINTF
					//in the upper and lower pattern boundaries.

					//Upper B & C factors
					p.x = -1;
					double b;
					double c;
					int j;
					int k;
					for (n = record; n < rightWindowValue; n++)
					{
						if (p.x == -1.0 && ub[n] != -1.0)
						{
							//Found the beginning of the next POINTF, now find the next POINTF
							p.x = GetX(window, n);
							p.y = ub[n];

							for (j = n + 1; j <= record + window; j++)
							{
								if (ub[j] != -1.0)
								{
									//Found the next POINTF so calculate B & C factors
									b = (p.y - ub[j]) / (p.x - GetX(window, j));
									c = p.y - b * p.x;
									//Now draw the trend for each period in between:
									for (k = n + 1; k <= j - 1; k++)
									{
										ub[k] = (b * GetX(window, k) + c);
									}
									p.x = -1.0;
									break;
								}
							}
						}
					} //Upper B & C factors

					//Lower B & C factors
					p.x = -1;
					for (n = record; n < rightWindowValue; n++)
					{
						if (p.x == -1.0 && lb[n] != -1.0)
						{
							//Found the beginning of the next POINTF, now find the next POINTF
							p.x = GetX(window, n);
							p.y = lb[n];

							for (j = n + 1; j <= record + window; j++)
							{
								if (lb[j] != -1.0)
								{
									//Found the next POINTF so calculate B & C factors
									b = (p.y - lb[j]) / (p.x - GetX(window, j));
									c = p.y - b * p.x;
									//Now draw the trend for each period in-between:
									for (k = n + 1; k <= j - 1; k++)
									{
										lb[k] = (b * GetX(window, k) + c);
									}
									p.x = -1.0;
									break;
								}
							}
						}
					} //Lower B & C factors

					//We now have the data and pattern scaled in the window.
					//Now, see if a pattern exists in the data.

					//Ensure that most bars are WITHIN the pattern
					bool filtered = false;
					int overTop = 0;
					int underBottom = 0;
					double errArea = 0.0;
					double upperPointErr = 0.0;
					double lowerPointErr = 0.0;
					int upperPointCnt = 0;
					int lowerPointCnt = 0;

					for (n = record; n < rightWindowValue; n++)
					{
						if (normalHigh[n] > ub[n] + (1 + BreakoutErrorThreshold))
						{
							filtered = true; //Any one bar that is 30% over the top is bad
							break;
						}
						if (normalLow[n] < lb[n] - (1 - BreakoutErrorThreshold))
						{
							filtered = true; //Any one bar that is 30% under the bottom is bad
							break;
						}
						if (normalHigh[n] > ub[n])
						{
							overTop++; //Over the top
						}
						if (normalLow[n] < lb[n])
						{
							underBottom++; //Under bottom
						}
						//Calculate the distance from the price and the pattern
						if (normalHigh[n] < ub[n])
						{
							errArea = errArea + ub[n] - normalHigh[n];
						}
						if (normalLow[n] > lb[n])
						{
							errArea = errArea + normalLow[n] - lb[n];
						}

						//Similar to above, but special filter for distance from
						//the price and the boundry when near a turning POINTF.
						for (j = 0; j <= upperEndArray; j++)
						{
							if (Math.Abs(n - _upperPoints[j]) < 2) //One record before and after each POINTF
							{
								upperPointErr = upperPointErr + Math.Abs(normalHigh[n] - ub[n]);
								upperPointCnt++;

								//exit if any turning POINTF has more error than 25%
								if ((n == _upperPoints[j]) && (Math.Abs(normalHigh[n] - ub[n]) > PointBreakoutErrorThreshold))
									filtered = true;
								break;
							}
						} //j

						for (j = 0; j <= lowerEndArray; j++)
						{
							if (Math.Abs(n - _lowerPoints[j]) < 2) //One record before and after each POINTF
							{
								lowerPointErr = lowerPointErr + Math.Abs(lb[n] - normalLow[n]);
								lowerPointCnt++;

								//exit if any POINTF has more error than 25%
								if ((n == _lowerPoints[j]) && (Math.Abs(lb[n] - normalLow[n]) > PointBreakoutErrorThreshold))
									filtered = true;
								break;
							}
						} //j

						if (filtered)
							break;
					} //n


					//Percentage of bars that were outside the pattern
					double percentOut = ((overTop + underBottom) / 2.0) / window;

					//Percentage of error inside the pattern
					double percentErr = errArea / window;

					//Percentage of error inside the pattern NEAR THE POINTS
					if (upperPointErr > 0 && lowerPointErr > 0)
					{
						upperPointErr = upperPointErr / upperPointCnt;
						lowerPointErr = lowerPointErr / lowerPointCnt;
						totalPointErr = (upperPointErr + lowerPointErr) / 2;
					}

					//##################
					//Note: m_PointBreakoutErrorThreshold may need to be set to 1 and m_ChannelError increased
					//for certain patterns such as triangles, wedges or channels.
					//Also note that pennants may appear as channels depending on the chart scale.
					//##################

					if (filtered || percentOut > BreakoutErrorThreshold || percentErr > ChannelError || totalPointErr > PointError)
					{
						//Either a high percentage of bars were outside the pattern, too many bars
						//were too far away from the boundries, or it was filtered. No good.
						continue; //next window
					}

					//Rank by error
					Ranking = 1 - percentOut - percentErr - totalPointErr;

					if (Ranking < FitnessThreshold || Ranking == 1)
					{
						//Too much error, go to the next record
						continue; //next window
					}

					List<PatternBound> bound = new List<PatternBound>();
					for (n = record; n < rightWindowValue; n++)
					{
						bound.Add(new PatternBound(UnNormalize(max, min, ub[n]), UnNormalize(max, min, lb[n])));
					}
					_results.Add(new PatternValue(new PatternInterval { x = record, y = record + window }, Ranking, bound));

					record++;
				}
			}
			return _results.Count;
		}

		private static int EndArray(IList<PatternInterval> ar)
		{
			for (int i = 0; i < ARRAY_LENGTH; i++)
			{
				if (ar[i].x == -1 || ar[i].y == -1)
					return i - 1;
			}
			return 0;
		}

		#endregion

		#region Private Methods
		private bool LoadXmlDefinition(string filename)
		{
			XmlDocument xDoc = new XmlDocument();
			try
			{
				xDoc.Load(filename);
				XmlElement elDesigner = (XmlElement)xDoc.SelectSingleNode("APR-Definition-File/designer");
				if (elDesigner == null)
					return false;
				XmlElement xElem;
				Title = (xElem = elDesigner["Title"]) != null ? xElem.InnerText : "";
				Notes = (xElem = elDesigner["Notes"]) != null ? xElem.InnerText : "";
				MaxBars = Convert.ToInt32((xElem = elDesigner["MaxBars"]) != null ? xElem.InnerText : "50", _ni);
				MinBars = Convert.ToInt32((xElem = elDesigner["MinBars"]) != null ? xElem.InnerText : "10", _ni);
				FitnessThreshold = Convert.ToDouble((xElem = elDesigner["FitnessThreshold"]) != null ? xElem.InnerText : "0.3", _ni);
				BreakoutErrorThreshold = Convert.ToDouble((xElem = elDesigner["BreakoutErrorThreshold"]) != null ? xElem.InnerText : "0.3", _ni);
				PointBreakoutErrorThreshold = Convert.ToDouble((xElem = elDesigner["PointBreakoutErrorThreshold"]) != null ? xElem.InnerText : "0.25", _ni);
				ChannelError = Convert.ToDouble((xElem = elDesigner["ChannelError"]) != null ? xElem.InnerText : "0.3", _ni);
				PointError = Convert.ToDouble((xElem = elDesigner["PointError"]) != null ? xElem.InnerText : "0.2", _ni);

				foreach (string sName in new[] { "LogicalHigh", "LogicalLow" })
				{
					XmlElement elPoints = elDesigner[sName];
					if (elPoints == null)
						continue;
					List<PatternInterval> list = sName == "LogicalHigh" ? _upper : _lower;

					var q = from XmlNode node in elPoints.ChildNodes
									let xmlElementX = node["X"]
									let xmlElementY = node["Y"]
									where xmlElementX != null && xmlElementY != null
									select
										new PatternInterval(Convert.ToDouble(xmlElementX.InnerText.Replace(',', '.'), _ni) / 100.0,
																				Convert.ToDouble(xmlElementY.InnerText.Replace(',', '.'), _ni) / 100.0);
					foreach (var interval in q.Select((interval, index) => new { interval, index }))
					{
						list[interval.index] = interval.interval;
					}
				}
			}
			catch
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Normalization function
		/// </summary>
		/// <param name="max"></param>
		/// <param name="min"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		private static double Normalize(double max, double min, double value)
		{
			return max == min ? 0.0 : (value - min) / (max - min);
		}

		/// <summary>
		/// Un-normalization function
		/// </summary>
		/// <param name="prevMax"></param>
		/// <param name="prevMin"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		private static double UnNormalize(double prevMax, double prevMin, double value)
		{
			if (prevMax == prevMin)
				return 0.0;
			return prevMin + (value * (prevMax - prevMin));
		}

		/// <summary>
		/// Returns X percent scaled to 1
		/// </summary>
		/// <param name="window"></param>
		/// <param name="position"></param>
		/// <returns></returns>
		private static double GetX(int window, int position)
		{
			return position / (double)window;
		}

		/// <summary>
		/// Returns nearest period in window based on x scale
		/// </summary>
		/// <param name="window"></param>
		/// <param name="pctX"></param>
		/// <returns></returns>
		private static int GetNearestPeriod(int window, double pctX)
		{
			for (int period = 0; period <= window; period++)
			{
				double nearest = GetX(window, period);
				if (nearest >= pctX)
					return period;
			}
			return 0;
		}
		#endregion
	}
}

