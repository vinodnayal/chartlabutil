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

        public String proEdgeId { get; set; }

        private string _rules = "";
        public string rules { get { return _rules; } set { _rules= value; } }

        public DateTime proEdgeTriggerDate { get; set; }

        public int triggerRuleId { get; set; }

        public int alertType { get; set; }

        public long proEdgeTriggerDateDiff { get; set; }

        public string companyName { get; set; }
    }
}
