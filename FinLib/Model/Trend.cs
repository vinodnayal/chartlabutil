using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinLib
{
    public class Trend
    {
          public String symbol { get; set; }
          public DateTime date { get; set; }
          public double movAvg_3 { get; set; }
          public double movAvg_5 { get; set; }
          public double movAvg_9 { get; set; }
          public double movAvg_12 { get; set; }
          public double movAvg_13 { get; set; }
          public double movAvg_20 { get; set; }
          public double movAvg_26 { get; set; }
          public double movAvg_50 { get; set; }
          public double movAvg_200 { get; set; }
          public double MACD { get; set; }
          public double SignalLine { get; set; }
          public double MACDSignaldiff { get; set; }
          public double Rating { get; set; }
          public double RSI { get; set; }
          public TrendEnum shortTermTrend { get; set; }
          public TrendEnum mediumTermTrend { get; set; }
          public TrendEnum longTermTrend { get; set; }


    }
}
