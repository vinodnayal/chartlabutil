using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinLib
{
    public  class BuySellRating
    {
        public string symbol { get; set; }
        public Int32 rating { get; set; }
        public double ratingValue { get; set; }
        public DateTime ratingDate { get; set; }
    }
}
