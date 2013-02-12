using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinLib
{
    public class MarketInternal
    {
        public int weeklyOSCount { get; set; }

        public int weeklyOBCount { get; set; }

        public int currentOSCount { get; set; }

        public int currentOBCount { get; set; }

        public float weeklyADLine10Days { get; set; }

        public float currentADLine10Days { get; set; }

        public double currentAbove50dayMA { get; set; }

        public double weeklyAbove50dayMA { get; set; }
    }
}
