using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinLib;

namespace ChartLabFinCalculation
{
    class Util
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(Util));
        public static List<BuySellRatingChangeHist> getBuySellRatingChangelist(List<BuySellRating> historyBuySellRatingList)
        {

            List<BuySellRatingChangeHist> ChangeBuySellRatingHist = new List<BuySellRatingChangeHist>();
            try
            {
                for (int i = 1; i < historyBuySellRatingList.Count; i++)
                {
                    int prevRating = historyBuySellRatingList[i - 1].rating;
                    int currentRating = historyBuySellRatingList[i].rating;
                    string prevSymbol = historyBuySellRatingList[i - 1].symbol;
                    string currentSymbol = historyBuySellRatingList[i].symbol;
                    if (currentSymbol.Equals(prevSymbol))
                    {
                        if ((prevRating - currentRating) != 0)
                        {
                            ChangeBuySellRatingHist.Add(
                                new BuySellRatingChangeHist
                                {
                                    symbol = historyBuySellRatingList[i].symbol,
                                    newRating = currentRating,
                                    oldRating = prevRating,
                                    ratingDate = historyBuySellRatingList[i].ratingDate


                                });
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                log.Error("Error: Problem in calculating Buy sell rating change hist" + ex);
            }
            return ChangeBuySellRatingHist;
        }
    }
}
