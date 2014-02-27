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
        public double price { get; set; }
        public double change { get; set; }
        public double changePct { get; set; }
        public int longTermTrend { get; set; }
        public DateTime longTermTrendDate { get; set; }


        public double support { get; set; }

        public double resistance { get; set; }

        public string longTermTrendText { get; set; }
        public string ratingAlertText { get; set; }
        public string ctRatingAlertText { get; set; }

        public string priceChangeText { get; set; }

        public string wlHeaderCss { get; set; }
    }
}
