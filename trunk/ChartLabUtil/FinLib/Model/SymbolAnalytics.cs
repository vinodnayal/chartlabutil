using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinLib
{
  public  class SymbolAnalytics
    {
        public String symbol { get; set; }
        public double r1 { get; set; }
        public double r2 { get; set; }
        public double r3 { get; set; }
        public double s1 { get; set; }
        public double s2 { get; set; }
        public double s3 { get; set; }
        public double YTDPrice { get; set; }
        public double QTDPrice { get; set; }
        public double MTDPrice { get; set; }
        public double WTDPrice { get; set; }
        public int buySellRating { get; set; }
        public int shortTermTrend { get; set; }
        public int longTermTrend { get; set; }
        public int mediumTermTrend { get; set; }
        public double STD50Days { get; set; }
        public double STD21Days { get; set; }
        public double low52WeekRange { get; set; }
        public double high52WeekRange { get; set; }
       
        public double alert { get; set; }
        public double OBOSCurrent { get; set; }
        public double OBOSWeekly { get; set; }


        public double weeklyRSI { get; set; }

        public double currentRSI { get; set; }

        public DateTime shortTermTrendDate { get; set; }
        public string RuleId { get; set; }
    }
}
