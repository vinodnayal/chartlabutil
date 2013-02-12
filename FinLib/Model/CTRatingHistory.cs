using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinLib
{
   public class CTRatingHistory
    {
        public string symbol { get; set; }
        public DateTime Date { get; set; }
        public int ctRating { get; set; }
    }
}
