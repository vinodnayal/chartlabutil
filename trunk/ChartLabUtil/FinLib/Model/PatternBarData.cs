using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinLib
{
   public class PatternBarData
    {
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public String symbol { get; set; }
        public String Pattern { get; set; }
        public int PatternId { get; set; }
    }
}
