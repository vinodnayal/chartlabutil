using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinLib;
using FinLib.Model;
using System.IO;
using ChartLabFinCalculation.DAL;

namespace ChartLabFinCalculation.BL
{
    internal class SnPSymbolsCalculations
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(SnPSymbolsCalculations));

        public static string snpDatafilesPath { get; set; }
        /// <summary>
        /// calculate synopsis rule id for chart page synopsis section 
        /// </summary>
        internal static void calculateSNPSymbolAnalytics()
        {
            try
            {
                List<SymbolRatingAlert> snpSymbolsRatingChanges = BuySellRatingDAO.getSNPSymbolsRatingChange();
                List<SnpAnalytics> snpSymbolsAnalytics = new List<SnpAnalytics>();
                int snpCTRating = 4; //neutral trend
                List<Rating> snpRating = BuySellRatingDAO.GetRatingsOfSymbol("SPY", false); //Constants.SnPSymbol
                if (snpRating.Count > 0)
                {
                    snpCTRating = snpRating[0].ctRating;
                }
                foreach (SymbolRatingAlert symbolRating in snpSymbolsRatingChanges)
                {
                    SnpAnalytics snpAnalytics= new SnpAnalytics();
                    snpAnalytics.symbol = symbolRating.symbol;
                    String ruleId = getSynopsisIdOnRatingChange(symbolRating);
                    if (snpCTRating < 4)
                    {
                        ruleId = "S" + snpCTRating + ruleId;
                    }
                    snpAnalytics.synopsisRuleId = ruleId;

                    snpAnalytics = SNPAnalyticsDAO.getSnpSymbolsGainLossProb(snpAnalytics,symbolRating.symbol, 11); // 11 for s&P watchlist
                    
                    snpSymbolsAnalytics.Add(snpAnalytics);
                }

                //caculating pro edge ID for snp 500 symbols to show in first tab of pro edge portfolio
                snpSymbolsAnalytics = calculateSNPProEdgeOnRatingsID(snpSymbolsAnalytics);


                CSVExporter.WriteToCSVRating(snpSymbolsAnalytics, snpDatafilesPath + "/snpAnalyticsFile.csv");
                log.Info("Process :Write snp Analytics To CSV  ");

                SNPAnalyticsDAO.InsertRating(snpDatafilesPath);
                log.Info("Process :Inserted snp Analytics csv File in to DB: ");



            }
            catch (Exception ex)
            {

                log.Error("Error: When claculation snp Symbol Analytics for snp symbols, " + ex);
            }

        }
        #region synopsis ID
        private static string getSynopsisIdOnRatingChange(SymbolRatingAlert symbolRating)
        {
            StringBuilder ruleId = new StringBuilder();

            try
            {

                int curRating = symbolRating.currating;
                int preRating = symbolRating.prevrating;
                int ctRating = symbolRating.ctrating;
                if (preRating > 3)
                    ruleId.Append("R" + preRating + curRating);
                if (ctRating <= 3)
                    ruleId.Append("C" + ctRating);
                if ((DateTime.Now - symbolRating.ratingChangeDate).TotalDays > 110 && curRating == 4)
                {

                    ruleId.Append("D");
                }

            }
            catch (Exception ex)
            {

                log.Error("Error: When claculation synopsis rule Id for symbol : " + symbolRating.symbol + ex);
            }
            return ruleId.ToString();
        }
        #endregion

        #region proEdge ID
        private static List<SnpAnalytics> calculateSNPProEdgeOnRatingsID(List<SnpAnalytics> snpSymbolsAnalytics)
        {
            try
            {
                Dictionary<String, List<Rating>> snpSymbolsRatingDict = BuySellRatingDAO.getSNPSymbolsHistRatings();
                foreach (SnpAnalytics symbolAnaltytics in snpSymbolsAnalytics)
                {
                    if (snpSymbolsRatingDict.ContainsKey(symbolAnaltytics.symbol))
                    {
                        SnpAnalytics tempAnalytics = getProEdgeId(snpSymbolsRatingDict[symbolAnaltytics.symbol], symbolAnaltytics);
                        symbolAnaltytics.proEdgeId = tempAnalytics.proEdgeId;
                        symbolAnaltytics.rules = tempAnalytics.rules;
                    }

                }
            }
            catch (Exception ex)
            {

                log.Error("Error: When claculation Pro Edge rule Id for snp symbols, " + ex);
            }
            return snpSymbolsAnalytics;


        }
        private static SnpAnalytics getProEdgeId(List<Rating> symbolRatings, SnpAnalytics symbolAnaltytics)
        {
            try
            {

                symbolAnaltytics = checkforRule1(symbolRatings, symbolAnaltytics);

                if (symbolAnaltytics.proEdgeId == 0)
                    symbolAnaltytics = checkforRule2(symbolRatings, symbolAnaltytics);

                if (symbolAnaltytics.proEdgeId == 0)
                    symbolAnaltytics = checkforRule3(symbolRatings, symbolAnaltytics);

                if (symbolAnaltytics.proEdgeId == 0)
                    symbolAnaltytics = checkforRule4(symbolRatings, symbolAnaltytics);

                if (symbolAnaltytics.proEdgeId == 0)
                    symbolAnaltytics = checkforRule5(symbolRatings, symbolAnaltytics);

                if (symbolAnaltytics.proEdgeId == 0)
                    symbolAnaltytics = checkforRule6(symbolRatings, symbolAnaltytics);

                if (symbolAnaltytics.proEdgeId == 0)
                    symbolAnaltytics = checkforRule7(symbolRatings, symbolAnaltytics);

                if (symbolAnaltytics.proEdgeId == 0)
                    symbolAnaltytics = checkforRule8to12(symbolRatings, symbolAnaltytics);

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return symbolAnaltytics;
        }

        private static SnpAnalytics checkforRule1(List<Rating> symbolRatings, SnpAnalytics symbolAnaltytics)
        {
            try
            {
                int proEdgeId = 0;
                double curDayRating = symbolRatings[0].ratingValue;
                double preDayRating = symbolRatings[1].ratingValue;
                StringBuilder sb = new StringBuilder();
                //Buy: If there is a 45% increase in rating that with a minimum rating of .599 rating
                if (curDayRating >= .599 && preDayRating < .599)
                {
                    if (BuyCheckRatingIncreasePctFromPreDay(symbolRatings, 45.0))
                    {

                        proEdgeId = 1;
                        sb.Append("Buy: If there is a 45% increase in rating that with a minimum rating of .599 rating ,");
                    }
                }
                //Buy: If there is a 30% increase the preceeding rating starting with at least a .70 rating
                if (curDayRating >= .70 && preDayRating < .70)
                {
                    if (BuyCheckRatingIncreasePctFromPreDay(symbolRatings, 30.0))
                    {
                        proEdgeId = 1;
                        sb.Append("Buy: If there is a 30% increase the preceeding rating starting with at least a .70 rating ,");
                    }
                }
                //Buy: If there are 6 prior consecutive rating increase up to or greater than. 67
                if (curDayRating >= .67)
                {
                    if (BuyCheckRatingIncreasingByCount(symbolRatings, 6))
                    {
                        proEdgeId = 1;
                        sb.Append("Buy: If there are 6 prior consecutive rating increase up to or greater than. 65 ,");
                    }
                }
                symbolAnaltytics.proEdgeId = proEdgeId;
                symbolAnaltytics.rules = sb.ToString();
            }
            catch (Exception)
            {
                
                throw;
            }
            return symbolAnaltytics;

        }

        private static SnpAnalytics checkforRule2(List<Rating> symbolRatings, SnpAnalytics symbolAnaltytics)
        {
            try
            {
                int proEdgeId = 0;
                double curDayRating = symbolRatings[0].ratingValue;
                double preDayRating = symbolRatings[1].ratingValue;
                StringBuilder sb = new StringBuilder();


                //If there is a 600% increase in rating that with a minimum rating of .11 rating
                if (curDayRating >= .11 && preDayRating < .11)
                {
                    if (BuyCheckRatingIncreasePctFromPreDay(symbolRatings, 600.0))
                    {
                        proEdgeId = 2;
                        sb.Append("Buy: If there is a 130% increase in rating that with a minimum rating of .14 rating ,");
                    }
                }
                //If there is a 130% increase in rating that with a minimum rating of .14 rating
                if (curDayRating >= .14 && preDayRating < .14)
                {
                    if (BuyCheckRatingIncreasePctFromPreDay(symbolRatings, 130.0))
                    {
                        proEdgeId = 2;
                        sb.Append("If there is a 130% increase in rating that with a minimum rating of .36 rating ,");
                    }
                }
                symbolAnaltytics.proEdgeId = proEdgeId;
                symbolAnaltytics.rules = sb.ToString();
            }
            catch (Exception)
            {
                
                throw;
            }
            return symbolAnaltytics;

        }

        private static SnpAnalytics checkforRule3(List<Rating> symbolRatings, SnpAnalytics symbolAnaltytics)
        {
            try
            {
                int proEdgeId = 0;
                double curDayRating = symbolRatings[0].ratingValue;
                double preDayRating = symbolRatings[1].ratingValue;
                StringBuilder sb = new StringBuilder();
                // Sell: if a rating above .92 drops by 13% or more the preceeding day

                if (curDayRating < .92 && preDayRating >= .92)
                {
                    if (SellCheckForRatingDropsFromPreDay(symbolRatings, -13.0))
                    {
                        proEdgeId = 3;
                        sb.Append("Sell: if a rating above .92 drops by 13% or more the preceeding day ,");
                    }
                }


                //Sell: Any rating above .8599 that drops by 20% the prceeding day

                if (curDayRating < .8599 && preDayRating >= .8599)
                {
                    if (SellCheckForRatingDropsFromPreDay(symbolRatings, -20.00))
                    {
                        proEdgeId = 3;
                        sb.Append("Sell: Any rating above .8599 that drops by 20% the prceeding day ,");
                    }

                }
                //Sell: If a rating above .68 drops 22% the preceding day
                if (curDayRating < .68 && preDayRating >= .68)
                {
                    if (SellCheckForRatingDropsFromPreDay(symbolRatings, -22.0))
                    {
                        proEdgeId = 3;
                        sb.Append("Sell: If a rating above .68 drops 22% the preceding day ,");
                    }
                }
                symbolAnaltytics.proEdgeId = proEdgeId;
                symbolAnaltytics.rules = sb.ToString();
            }
            catch (Exception)
            {
                
                throw;
            }
            return symbolAnaltytics;

        }

        private static SnpAnalytics checkforRule4(List<Rating> symbolRatings, SnpAnalytics symbolAnaltytics)
        {
            try
            {
                int proEdgeId = 0;
                double curDayRating = symbolRatings[0].ratingValue;
                double preDayRating = symbolRatings[1].ratingValue;
                StringBuilder sb = new StringBuilder();

                //Sell: If a rating above .60 drops 50% the preceding day
                if (curDayRating < .60 && preDayRating >= .60)
                {
                    if (SellCheckForRatingDropsFromPreDay(symbolRatings, -50.0))
                    {
                        proEdgeId = 4;
                        sb.Append("Sell: If a rating above .60 drops 50% the preceding day ,");
                    }

                }
                //Sell: If a rating above .40 drops 62% the preceding day
                if (curDayRating < .40 && preDayRating >= .40)
                {
                    if (SellCheckForRatingDropsFromPreDay(symbolRatings, -62.0))
                    {
                        proEdgeId = 4;
                        sb.Append("Sell: If a rating above .40 drops 62% the preceding day ,");
                    }

                }

                symbolAnaltytics.proEdgeId = proEdgeId;
                symbolAnaltytics.rules = sb.ToString();
            }
            catch (Exception)
            {
                
                throw;
            }
            return symbolAnaltytics;

        }

        private static SnpAnalytics checkforRule5(List<Rating> symbolRatings, SnpAnalytics symbolAnaltytics)
        {
            try
            {
                int proEdgeId = 0;
                double curDayRating = symbolRatings[0].ratingValue;
                double preDayRating = symbolRatings[1].ratingValue;
                StringBuilder sb = new StringBuilder();
                //  Any rating that is positive that drops to or below .00 by 200% sell
                if (curDayRating < 0 && preDayRating >= 0)
                {
                    if (SellCheckForRatingDropsFromPreDay(symbolRatings, -200.0))
                    {
                        proEdgeId = 5;
                        sb.Append("Any rathing that is positive that drops to or below .00 by 200% sell ,");
                    }

                }
                //Sell if we fall below .00 for 5 consecutive days
                if (curDayRating <= 0.0 && symbolRatings[6].ratingValue > 0)
                {
                    if (SellCheckForSpecificRatingForDays(symbolRatings, 5, 0))
                    {
                        proEdgeId = 5;
                        sb.Append("Sell if we fall below .00 for 5 consecutive days ,");
                    }

                }
                symbolAnaltytics.proEdgeId = proEdgeId;
                symbolAnaltytics.rules = sb.ToString();
            }
            catch (Exception)
            {
                
                throw;
            }
            return symbolAnaltytics;

        }

        private static SnpAnalytics checkforRule7(List<Rating> symbolRatings, SnpAnalytics symbolAnaltytics)
        {
            try
            {
                int proEdgeId = 0;
                double curDayRating = symbolRatings[0].ratingValue;
                double preDayRating = symbolRatings[1].ratingValue;
                StringBuilder sb = new StringBuilder();
                //Sell Short when your have a 12 consecutive decline starting with at least a -.388 rating
                if (curDayRating <= -.38)
                {
                    if (SellCheckForRatingDecreasingForCount(symbolRatings, 12, -.38))
                    {
                        proEdgeId = 7;
                        sb.Append("Sell Short when your have a 12 consecutive decline starting with at least a -.388 rating ,");
                    }

                }
                //Sell Short when you have a 3 consecutive decline starting with at least a -.60 rating 
                if (curDayRating <= -.52)
                {
                    if (SellCheckForRatingDecreasingForCount(symbolRatings, 3, -.60))
                    {
                        proEdgeId = 7;
                        sb.Append("Sell Short when you have a 3 consecutive decline starting with at least a -.52 rating ,");
                    }

                }
                //Sell short when a rating decreases by 45% on any rating at or below -.40
                if (curDayRating < -.40 && preDayRating >= -.40)
                {
                    if (SellCheckForRatingDropsFromPreDay(symbolRatings, -45.0))
                    {
                        proEdgeId = 7;
                        sb.Append("Sell short when a rating decreases by 45% on any rating at or below -.40 ,");
                    }

                }
                symbolAnaltytics.proEdgeId = proEdgeId;
                symbolAnaltytics.rules = sb.ToString();
            }
            catch (Exception)
            {
                
                throw;
            }
            return symbolAnaltytics;

        }

        private static SnpAnalytics checkforRule6(List<Rating> symbolRatings, SnpAnalytics symbolAnaltytics)
        {
            try
            {
                int proEdgeId = 0;
                double curDayRating = symbolRatings[0].ratingValue;
                double preDayRating = symbolRatings[1].ratingValue;
                StringBuilder sb = new StringBuilder();
                //Sell 50%: Approaching Overbought counter Trend -56. or over


                if (symbolRatings[0].ctRatingValue < -56)
                {
                    proEdgeId = 6;
                    sb.Append("Sell 50%: Approaching Overbought counter Trend -56. or over ,");
                }



                symbolAnaltytics.proEdgeId = proEdgeId;
                symbolAnaltytics.rules = sb.ToString();
            }
            catch (Exception)
            {
                
                throw;
            }
            return symbolAnaltytics;

        }

        private static SnpAnalytics checkforRule8to12(List<Rating> symbolRatings, SnpAnalytics symbolAnaltytics)
        {
            try
            {
                int proEdgeId = 0;
                double curDayRating = symbolRatings[0].ratingValue;
                double preDayRating = symbolRatings[1].ratingValue;
                StringBuilder sb = new StringBuilder();
                if (curDayRating >= .90)
                {
                    proEdgeId = 8;
                }
                else if (curDayRating >= .70)
                {
                    proEdgeId = 9;
                }
                else if (curDayRating >= .55)
                {
                    proEdgeId = 10;
                }
                else if (curDayRating >= .30)
                {
                    proEdgeId = 11;
                }
                else if (curDayRating >= .0)
                {
                    proEdgeId = 12;
                }
                else if (curDayRating >= -.20)
                {
                    proEdgeId = 13;
                }
                else if (curDayRating >= -.40)
                {
                    proEdgeId = 14;
                }
                else if (curDayRating >= -.65)
                {
                    proEdgeId = 15;
                }
                else if (curDayRating >= -1.0)
                {
                    proEdgeId = 16;
                }

                if (proEdgeId > 7)
                {
                    if (BuyCheckRatingIncreasingByCount(symbolRatings, 3))
                    {
                        proEdgeId = proEdgeId * 10 + 1;
                    }
                    else if (SellCheckForRatingDecreasingForCount(symbolRatings, 3, curDayRating))
                    {
                        proEdgeId = proEdgeId * 10 + 2;
                    }
                    else if (SellCheckForSpecificRatingForDays(symbolRatings, 3, curDayRating))
                    {
                        proEdgeId = proEdgeId * 10 + 3;
                    }
                }
                symbolAnaltytics.proEdgeId = proEdgeId;
                symbolAnaltytics.rules = sb.ToString();
            }
            catch (Exception)
            {
                
                throw;
            }
            return symbolAnaltytics;

        }

        private static bool SellCheckForSpecificRatingForDays(List<Rating> symbolRatings, int days, double ratingValue)
        {
            bool isbelowSpecificRating = false;
            for (int i = 0; i < days; i++)
            {
                if (symbolRatings[i].ratingValue <= ratingValue)
                {
                    isbelowSpecificRating = true;
                }
                else
                {

                    isbelowSpecificRating = false;
                    break;
                }
            }

            return isbelowSpecificRating;

        }

        private static bool SellCheckForRatingDecreasingForCount(List<Rating> symbolRatings, int decreaseCount, double value)
        {
            int decreasingRatingCount = 0;
            bool isDecreasigFormValue = false;
            try
            {

                for (int i = 0; i < symbolRatings.Count - 1; i++)
                {
                    if (decreasingRatingCount < decreaseCount)
                    {
                        double curRating = symbolRatings[i].ratingValue;
                        double preDayRating = symbolRatings[i + 1].ratingValue;
                        double diff = curRating - preDayRating;
                        if (diff < 0)
                        {
                            decreasingRatingCount++;
                        }
                        else if (diff > 0)
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (symbolRatings[i].ratingValue >= value)
                        {
                            isDecreasigFormValue = true;
                        }
                        break;
                    }

                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            if (decreasingRatingCount >= decreaseCount && isDecreasigFormValue)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool SellCheckForRatingDropsFromPreDay(List<Rating> symbolRatings, double dropPctValue)
        {
            double curDayRating = symbolRatings[0].ratingValue;
            double preDayRating = symbolRatings[1].ratingValue;
            Double dropPercentage = 0;
            if (preDayRating != 0)
            {
                dropPercentage = (curDayRating - preDayRating) * 100 / preDayRating * Math.Sign(preDayRating);
            }

            if (dropPercentage < dropPctValue)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool BuyCheckRatingIncreasingByCount(List<Rating> symbolRatings, int increaseCount)
        {
            int increaseRatingCount = 0;
            try
            {

                for (int i = 0; i < symbolRatings.Count - 1; i++)
                {
                    if (increaseRatingCount < increaseCount)
                    {
                        double curRating = symbolRatings[i].ratingValue;
                        double preDayRating = symbolRatings[i + 1].ratingValue;
                        double diff = curRating - preDayRating;
                        if (diff > 0)
                        {
                            increaseRatingCount++;
                        }
                        else if (diff < 0)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }

                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            if (increaseRatingCount >= increaseCount)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool BuyCheckRatingIncreasePctFromPreDay(List<Rating> symbolRatings, Double pctIncreaseValue)
        {
            double curDayRating = symbolRatings[0].ratingValue;
            double preDayRating = symbolRatings[1].ratingValue;
            Double increasePercentage = 0;
            if (preDayRating != 0)
            {
                increasePercentage = (curDayRating - preDayRating) * 100 / preDayRating * Math.Sign(preDayRating);
            }

            if (increasePercentage >= pctIncreaseValue)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region import raings data from files sent by louise for 30 years of data.
        /// <summary>
        /// calculating ratings from rating files 
        /// </summary>
        public static void CalculateSNPSymbolsRatingsData()
        {
            try
            {
                log.Info("Start importng historical rating data for SNP 500 ");
                List<string> snpSymbolList = SnPPriceDAO.getSnpSymbolsList();
                foreach (string snpSymbol in snpSymbolList)
                {
                    try
                    {
                        List<Rating> RatingList = new List<Rating>();
                        Dictionary<DateTime, CTRating> ctRatingList = new Dictionary<DateTime, CTRating>();
                        String symbolDataPath = snpDatafilesPath + "/SNPRatingData";
                        List<FileInfo> symbolCTRatingFiles = new List<FileInfo>();
                        FileInfo symbolRatingFile = new FileInfo(symbolDataPath + "/" + snpSymbol.ToUpper() + ".TR1");
                        if (symbolRatingFile.Exists)
                        {
                            RatingList = ETFSymbolsDataCalculation.CalculateBuySellRatings(snpSymbol, symbolRatingFile);
                        }
                        else
                        {
                            log.Info("BS Rating file not found fot symbol :" + snpSymbol);
                        }
                        FileInfo ctRatingFile1 = new FileInfo(symbolDataPath + "/" + snpSymbol.ToUpper() + ".CT2");
                        FileInfo ctRatingFile2 = new FileInfo(symbolDataPath + "/" + snpSymbol.ToUpper() + ".CT3");

                        if (ctRatingFile1.Exists && ctRatingFile2.Exists)
                        {
                            symbolCTRatingFiles.Add(ctRatingFile1);
                            symbolCTRatingFiles.Add(ctRatingFile2);
                            ctRatingList = ETFSymbolsDataCalculation.calculateCTRatings(snpSymbol, symbolCTRatingFiles);
                        }
                        else
                        {
                            log.Info("CT rating file not found fot symbol :" + snpSymbol);
                        }


                        foreach (Rating bsRating in RatingList)
                        {
                            if (ctRatingList.ContainsKey(bsRating.ratingDate))
                            {
                                CTRating ctRating = ctRatingList[bsRating.ratingDate];

                                bsRating.ctRatingValue = ctRating.ctRatingValue;
                                bsRating.ctRating = ctRating.ctRating;

                            }

                        }
                        log.Info("saving ratings counts :" + RatingList.Count + " for symbol" + snpSymbol);
                        CSVExporter.WriteToCSVRating(RatingList, snpDatafilesPath + "/RatingFile.csv");
                        ETFSymbolsDAO.InsertRatingDataInDB(snpDatafilesPath, "snpbsctratings", snpSymbol, true);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Error in importing data for symbol " + snpSymbol);
                        log.Error(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error in importing data for etf symbols ");
                log.Error(ex);
            }


        }
        #endregion

        #region import price in snpSymbolhistoricals
        public static void ImportSNPSymbolsPriceData(DateTime fromDate)
        {
            try
            {
                DateTime toDate = DateTime.Now;
                List<string> snpSymbolList = SnPPriceDAO.getSnpSymbolsList();
                foreach (string snpSymbol in snpSymbolList)
                {
                    try
                    {
                        log.Info("Process: Starting Historical Data Import for specific symbol " + snpSymbol);
                        SymbolHistoricalDAO.DeleteData(snpSymbol, "snpsymbolshistorical");
                        HistoricalDataImporter.SaveHistDataSymbol(fromDate, toDate, snpSymbol, false, true, "snpsymbolshistorical");
                        log.Info("Process: Done! Historical Data Import for specific symbol " + snpSymbol);

                    }
                    catch (Exception ex)
                    {
                        log.Error("Error in importing data for symbol " + snpSymbol);
                        log.Error(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error in importing data for etf symbols ");
                log.Error(ex);
            }


        }
        #endregion


    }
}
