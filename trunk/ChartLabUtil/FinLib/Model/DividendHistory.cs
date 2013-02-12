using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinLib
{
    public class DividendHistory
    {
        public string symbol { get; set; }
        public DateTime dividendDate { get; set; }
        public DateTime todaysDate { get; set; }
        public bool isdividend { get; set; }
    }
}
