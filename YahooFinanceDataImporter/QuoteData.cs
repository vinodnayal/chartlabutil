using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1
{
   
    // Summary:
    //     Stores informations about one historic trading period (day, week or month).
    public class QuoteData
    {
        public QuoteData()
        {

        }

        // Summary:
        //     The last value in trading period.
        public double Close { get; set; }
        //
        // Summary:
        //     The last value in trading period in relation to share splits.
        public double CloseAdjusted { get; set; }
        //
        // Summary:
        //     The highest value in trading period.
        public double High { get; set; }
        //
        // Summary:
        //     The lowest value in trading period.
        public double Low { get; set; }
        //
        // Summary:
        //     The first value in trading period.
        public double Open { get; set; }
        //
        // Summary:
        //     The close value of the previous HistQuoteData in chain.
        public double PreviousClose { get; set; }
        //
        // Summary:
        //     The startdate of the period.
        public DateTime TradingDate { get; set; }
        //
        // Summary:
        //     The traded volume.
        public long Volume { get; set; }
    }
}
