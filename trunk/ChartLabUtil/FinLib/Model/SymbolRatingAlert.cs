using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinLib
{
    public class SymbolRatingAlert
    {
        public string symbol { get; set; }
        public int ctrating { get; set; }
        public int prevrating { get; set; }
        public int currating { get; set; }
        public DateTime ratingChangeDate { get; set; }
        public double changeDatePrice { get; set; }
    }
}
