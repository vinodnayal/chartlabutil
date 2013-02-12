using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinLib
{
 public class TrendObjects
    {
        public DateTime shortTermTrendDate { get; set; }
        public int shortTermTrend { get; set; }
        public int mediumTermTrend { get; set; }
        public int longTermTrend { get; set; }
        public string symbol { get; set; }
    }
}
