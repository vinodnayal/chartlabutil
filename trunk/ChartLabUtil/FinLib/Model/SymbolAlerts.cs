using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinLib.Model
{
   public class SymbolAlerts
    {
        public string Symbol { get; set; }

        public int preRating { get; set; }

        public int curRating { get; set; }
        public int preCTRating { get; set; }

        public int curCTRating { get; set; }

        public string companyName { get; set; }

        public int ratingAlertType { get; set; }

        public int ctRatingAlertType { get; set; }

        public string watchlistName { get; set; }
    }
}
