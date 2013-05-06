using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinLib
{
    public class SectorPerfHist
    {
        public int sectorId { get; set; }
        public int rating { get; set; }
        public DateTime date { get; set; }

        public double ratingValue { get; set; }
    }
}
