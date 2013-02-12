using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinLib.Model
{
    public class DailySymbolsData
    {
        public string symbol { get; set; }

        public double lastPrice { get; set; }

        public double change { get; set; }
    }
}
