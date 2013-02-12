using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinLib
{
   public class Rating
    {
        public string symbol { get; set; }
        public double ctRatingValue { get; set; }
        public Int32 ctRating { get; set; }
        public Int32 rating { get; set; }
        public double ratingValue { get; set; }
        public DateTime ratingDate { get; set; }
    }
}
