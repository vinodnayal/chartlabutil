using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinLib
{
    public class DayWiseAvgReturnForSymbol
    {
        public String symbol { get; set; } 
        public double AvgReturn_2_Days { get; set; }
        public double AvgReturn_5_Days { get; set; }
        public double AvgReturn_30_Days { get; set; }
        public double AvgReturn_90_Days { get; set; }

    }
}
