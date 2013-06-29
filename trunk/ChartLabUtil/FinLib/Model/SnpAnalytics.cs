using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinLib.Model
{
   public class SnpAnalytics
    {
        public string synopsisRuleId { get; set; }

        public string symbol { get; set; }

        public int proEdgeId { get; set; }

        private string _rules = "";
        public string rules { get { return _rules; } set { _rules= value; } }

        public double gainPct { get; set; }

        public double confidencePct { get; set; }

        public double riskPct { get; set; }
    }
}
