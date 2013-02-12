using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinLib
{
    public class BuySellRatingChangeHist
    {
        public string symbol { get; set; }

        public int newRating { get; set; }

        public int oldRating { get; set; }

        public DateTime ratingDate { get; set; }
    }
}
