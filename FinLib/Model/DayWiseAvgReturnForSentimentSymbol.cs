using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinLib
{
   public class DayWiseAvgReturnForSentimentSymbol
    {
        public int symbolId { get; set; }
        public double AvgReturn_Weekly { get; set; }
        public double AvgReturn_1_Month { get; set; }
        public double AvgReturn_2_Month { get; set; }
        public int Indicator { get; set; }
    }
}
