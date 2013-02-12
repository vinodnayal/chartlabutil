using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinLib.Model
{
   public class StatisticsPerf
    {
        public string symbol { get; set; }

        public double buyPrice { get; set; }

        public DateTime buyDate { get; set; }

        public double sellPrice { get; set; }

        public DateTime sellDate { get; set; }

        public double StatReturn { get; set; }

        public double duration { get; set; }

        public int StatId { get; set; }
    }
}
