using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinLib;
using FinLib.Model;
using System.IO;
using ChartLabFinCalculation.DAL;
using System.Configuration;
using ChartLabFinCalculation.UTIL;

namespace ChartLabFinCalculation.BL
{
    internal class SNPSymbolsCalculations
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(SNPSymbolsCalculations));

        public static string snpDatafilesPath { get; set; }
        /// <summary>
        /// calculate synopsis rule id for chart page synopsis section 
        /// </summary>
        internal static void calculateSNPSymbolsSynopsis()
        {
            try
            {
                log.Info("Process :Calcualating synopsis ids for 500 symbols ");
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
                    SnpAnalytics snpAnalytics = new SnpAnalytics();
                    snpAnalytics.symbol = symbolRating.symbol;
                    String ruleId = getSynopsisIdOnRatingChange(symbolRating);
                    if (snpCTRating < 4)
                    {
                        ruleId = "S" + snpCTRating + ruleId;
                    }
                    snpAnalytics.synopsisRuleId = ruleId;

                    snpSymbolsAnalytics.Add(snpAnalytics);
                }

                //caculating pro edge ID for snp 500 symbols to show in first tab of pro edge portfolio
                // snpSymbolsAnalytics = calculateSNPProEdgeIDOnRatings(snpSymbolsAnalytics);


                CSVExporter.WriteToCSVSynopsisID(snpSymbolsAnalytics, snpDatafilesPath + "/snpAnalyticsFile.csv");
                log.Info("Process :Write snp Analytics To CSV  ");

                SNPAnalyticsDAO.InsertSynopsisIDInDB(snpDatafilesPath);
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
                if (preRating >= 3)
                {
                    ruleId.Append("R" + preRating + curRating);
                }
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
        internal static void calculateSNPProEdgeID(DateTime date)
        {
            try
            {
                log.Info("ProEdge: Calcualating Pro-Edge for s&p 500 symbols for date: " + date);
                //DateTime currentDate = DateTime.Now;
                Dictionary<String, SnpAnalytics> snpSymbolsAnalytics = SNPAnalyticsDAO.getCurSnpSymbolsAnalytics();

                List<SnpAnalytics> snpSymbolsProEdgeDetails = new List<SnpAnalytics>();
                Dictionary<String, List<Rating>> snpSymbolsRatingDict = BuySellRatingDAO.getSNPSymbolsHistRatings(date);
                foreach (KeyValuePair<String, List<Rating>> symbolDetail in snpSymbolsRatingDict)
                {
                    try
                    {
                        List<Rating> symbolRatings = symbolDetail.Value;
                        if (symbolRatings.Count > 1)
                        {
                            SnpAnalytics tempAnalytics = getProEdgeId(symbolDetail.Key, symbolRatings);
                            if (!tempAnalytics.proEdgeId.Equals(""))
                            {
                                snpSymbolsProEdgeDetails.Add(tempAnalytics);

                                //update snpsymbolAnalytics table with new alert
                                if (snpSymbolsAnalytics.ContainsKey(symbolDetail.Key))
                                {
                                    SnpAnalytics symbolAnalytics = snpSymbolsAnalytics[symbolDetail.Key];
                                    symbolAnalytics.proEdgeId = tempAnalytics.proEdgeId;
                                    symbolAnalytics.proEdgeTriggerDate = tempAnalytics.proEdgeTriggerDate;
                                    //if (tempAnalytics.alertType == 3 && symbolAnalytics.proEdgeTriggerDateDiff <= 10 && symbolAnalytics.proEdgeTriggerDateDiff > 0 && (symbolAnalytics.alertType == 1 || symbolAnalytics.alertType == 2))
                                    //{
                                    //}
                                    //else
                                    //{
                                    //    symbolAnalytics.proEdgeId = tempAnalytics.proEdgeId;
                                    //    symbolAnalytics.proEdgeTriggerDate = tempAnalytics.proEdgeTriggerDate;
                                    //}

                                }
                            }


                        }
                    }
                    catch (Exception ex)
                    {

                        log.Error("Error: When claculation Pro Edge rule Id for snp symbol : " + symbolDetail.Key + ex);
                    }
                }



                CSVExporter.WriteToCSVProEdgeID(snpSymbolsProEdgeDetails, snpDatafilesPath + "/proEdgeFileHIst" + date.ToString("yyyyMMdd") + ".csv");
                log.Info("Process :Write pro EdgeFile To CSV  for date  " + date);

                SNPAnalyticsDAO.InsertProEdgeTriggerInDB(snpDatafilesPath, date);
                log.Info("Process :Inserted proEdge csv File in to DB: for date  " + date);

                CSVExporter.WriteToCSVSynopsisID(snpSymbolsAnalytics, snpDatafilesPath + "/snpProEdgeFile.csv");
                log.Info("Process :Write snp Analytics To CSV  for date  " + date);

                SNPAnalyticsDAO.InsertSNPSymbolAnalyticsInDB(snpDatafilesPath);
                log.Info("Process :Inserted snp Analytics csv File in to DB: for date  " + date);


            }
            catch (Exception ex)
            {

                log.Error("Error: When claculation Pro Edge rule Id for snp symbols, " + ex);
            }



        }
        #region proEdge ID rules
        private static SnpAnalytics getProEdgeId(String symbol, List<Rating> symbolRatings)
        {
            SnpAnalytics symbolAnaltytics = new SnpAnalytics();
            symbolAnaltytics.symbol = symbol;
            symbolAnaltytics.proEdgeTriggerDate = symbolRatings[0].ratingDate;
            try
            {

                symbolAnaltytics = checkforRule1(symbolRatings, symbolAnaltytics);

                if (symbolAnaltytics.proEdgeId.Equals(""))
                    symbolAnaltytics = checkforRule2(symbolRatings, symbolAnaltytics);

                if (symbolAnaltytics.proEdgeId.Equals(""))
                    symbolAnaltytics = checkforRule3(symbolRatings, symbolAnaltytics);

                if (symbolAnaltytics.proEdgeId.Equals(""))
                    symbolAnaltytics = checkforRule4(symbolRatings, symbolAnaltytics);

                if (symbolAnaltytics.proEdgeId.Equals(""))
                    symbolAnaltytics = checkforRule5(symbolRatings, symbolAnaltytics);

                if (symbolAnaltytics.proEdgeId.Equals(""))
                    symbolAnaltytics = checkforOverboughtRules(symbolRatings, symbolAnaltytics);

                if (symbolAnaltytics.proEdgeId.Equals(""))
                    symbolAnaltytics = checkforRule7(symbolRatings, symbolAnaltytics);

                if (symbolAnaltytics.proEdgeId.Equals(""))
                    symbolAnaltytics = checkforNeutralRules(symbolRatings, symbolAnaltytics);

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
                String proEdgeId = "";
                double curDayRating = symbolRatings[0].ratingValue;
                double preDayRating = symbolRatings[1].ratingValue;
                StringBuilder sb = new StringBuilder();
                //Buy: If there is a 45% increase in rating that with a minimum rating of .599 rating
                if (curDayRating >= .599 && preDayRating < .599)
                {
                    if (BuyCheckRatingIncreasePctFromPreDay(symbolRatings, 45.0))
                    {

                        proEdgeId = "B1";
                        sb.Append("Buy: If there is a 45% increase in rating that with a minimum rating of .599 rating ,");
                    }
                }
                //Buy: If there are 6 prior consecutive rating increase up to or greater than. 67
                if (curDayRating >= .67 && preDayRating < .67)
                {
                    if (BuyCheckRatingIncreasingByCount(symbolRatings, 6))
                    {
                        proEdgeId = "B2";
                        sb.Append("Buy: If there are 6 prior consecutive rating increase up to or greater than. 65 ,");
                    }
                }
                //Buy: If there is a 30% increase the preceeding rating starting with at least a .70 rating
                if (curDayRating >= .70 && preDayRating < .70)
                {
                    if (BuyCheckRatingIncreasePctFromPreDay(symbolRatings, 30.0))
                    {
                        proEdgeId = "B3";
                        sb.Append("Buy: If there is a 30% increase the preceeding rating starting with at least a .70 rating ,");
                    }
                }

                symbolAnaltytics.proEdgeId = proEdgeId;
                symbolAnaltytics.alertType = 1;
                symbolAnaltytics.rules = sb.ToString();
                if (proEdgeId != "")
                    log.Info("ProEdge: for Symbol " + symbolAnaltytics.symbol + " Triggered Rule :" + symbolAnaltytics.rules + "on Date " + symbolAnaltytics.proEdgeTriggerDate);

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
                String proEdgeId = "";
                double curDayRating = symbolRatings[0].ratingValue;
                double preDayRating = symbolRatings[1].ratingValue;
                StringBuilder sb = new StringBuilder();


                //If there is a 600% increase in rating that with a minimum rating of .11 rating
                if (curDayRating >= .11)
                {
                    if (BuyCheckRatingIncreasePctFromPreDay(symbolRatings, 600.0))
                    {
                        proEdgeId = "B4";
                        sb.Append("If there is a 600% increase in rating that with a minimum rating of .11 rating ,");
                    }
                }
                //If there is a 130% increase in rating that with a minimum rating of .14 rating
                if (curDayRating >= .14)
                {
                    if (BuyCheckRatingIncreasePctFromPreDay(symbolRatings, 130.0))
                    {
                        proEdgeId = "B5";
                        sb.Append("If there is a 130% increase in rating that with a minimum rating of .36 rating ,");
                    }
                }
                symbolAnaltytics.proEdgeId = proEdgeId;
                symbolAnaltytics.alertType = 1;
                symbolAnaltytics.rules = sb.ToString();
                if (proEdgeId != "")
                    log.Info("ProEdge: for Symbol " + symbolAnaltytics.symbol + " Triggered Rule :" + symbolAnaltytics.rules + "on Date " + symbolAnaltytics.proEdgeTriggerDate);
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
                String proEdgeId = "";
                double curDayRating = symbolRatings[0].ratingValue;
                double preDayRating = symbolRatings[1].ratingValue;
                StringBuilder sb = new StringBuilder();
                // Sell: if a rating above .92 drops by 13% or more the preceeding day

                if (curDayRating < .92 && preDayRating >= .92)
                {
                    if (SellCheckForRatingDropsFromPreDay(symbolRatings, -13.0))
                    {
                        proEdgeId = "S1";
                        sb.Append("Sell: if a rating above .92 drops by 13% or more the preceeding day ,");
                    }
                }

                //Sell: If a rating above .68 drops 22% the preceding day
                if (curDayRating < .68 && preDayRating >= .68)
                {
                    if (SellCheckForRatingDropsFromPreDay(symbolRatings, -22.0))
                    {
                        proEdgeId = "S2";
                        sb.Append("Sell: If a rating above .68 drops 22% the preceding day ,");
                    }
                }
                //Sell: Any rating above .8599 that drops by 20% the prceeding day

                if (curDayRating < .8599 && preDayRating >= .8599)
                {
                    if (SellCheckForRatingDropsFromPreDay(symbolRatings, -20.00))
                    {
                        proEdgeId = "S3";
                        sb.Append("Sell: Any rating above .8599 that drops by 20% the prceeding day ,");
                    }

                }

                symbolAnaltytics.proEdgeId = proEdgeId;
                symbolAnaltytics.alertType = 2;
                symbolAnaltytics.rules = sb.ToString();
                if (proEdgeId != "")
                    log.Info("ProEdge: for Symbol " + symbolAnaltytics.symbol + " Triggered Rule :" + symbolAnaltytics.rules + "on Date " + symbolAnaltytics.proEdgeTriggerDate);
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
                String proEdgeId = "";
                double curDayRating = symbolRatings[0].ratingValue;
                double preDayRating = symbolRatings[1].ratingValue;
                StringBuilder sb = new StringBuilder();

                //Sell: If a rating above .60 drops 50% the preceding day
                if (curDayRating < .60 && preDayRating >= .60)
                {
                    if (SellCheckForRatingDropsFromPreDay(symbolRatings, -50.0))
                    {
                        proEdgeId = "S4";
                        sb.Append("Sell: If a rating above .60 drops 50% the preceding day ,");
                    }

                }
                //Sell: If a rating above .40 drops 62% the preceding day
                if (curDayRating < .40 && preDayRating >= .40)
                {
                    if (SellCheckForRatingDropsFromPreDay(symbolRatings, -62.0))
                    {
                        proEdgeId = "S5";
                        sb.Append("Sell: If a rating above .40 drops 62% the preceding day ,");
                    }

                }

                symbolAnaltytics.proEdgeId = proEdgeId;
                symbolAnaltytics.alertType = 2;
                symbolAnaltytics.rules = sb.ToString();
                if (proEdgeId != "")
                    log.Info("ProEdge: for Symbol " + symbolAnaltytics.symbol + " Triggered Rule :" + symbolAnaltytics.rules + "on Date " + symbolAnaltytics.proEdgeTriggerDate);
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
                String proEdgeId = "";
                double curDayRating = symbolRatings[0].ratingValue;
                double preDayRating = symbolRatings[1].ratingValue;
                StringBuilder sb = new StringBuilder();

                //Sell if we fall below .00 for 5 consecutive days
                if (curDayRating <= 0.0 && symbolRatings[6].ratingValue > 0)
                {
                    if (SellCheckForSpecificRatingForDays(symbolRatings, 5, 0))
                    {
                        proEdgeId = "S6";
                        sb.Append("Sell if we fall below .00 for 5 consecutive days ,");
                    }

                }
                //  Any rating that is positive that drops to or below .00 by 200% sell
                if (curDayRating < 0 && preDayRating >= 0)
                {
                    if (SellCheckForRatingDropsFromPreDay(symbolRatings, -200.0))
                    {
                        proEdgeId = "S7";
                        sb.Append("Any rathing that is positive that drops to or below .00 by 200% sell ,");
                    }

                }

                symbolAnaltytics.proEdgeId = proEdgeId;
                symbolAnaltytics.alertType = 2;
                symbolAnaltytics.rules = sb.ToString();
                if (proEdgeId != "")
                    log.Info("ProEdge: for Symbol " + symbolAnaltytics.symbol + " Triggered Rule :" + symbolAnaltytics.rules + "on Date " + symbolAnaltytics.proEdgeTriggerDate);
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
                String proEdgeId = "";
                double curDayRating = symbolRatings[0].ratingValue;
                double preDayRating = symbolRatings[1].ratingValue;
                StringBuilder sb = new StringBuilder();
                //Sell Short when your have a 12 consecutive decline starting with at least a -.388 rating
                if (curDayRating <= -.38)
                {
                    if (SellCheckForRatingDecreasingForCount(symbolRatings, 12, -.38))
                    {
                        proEdgeId = "SS1";
                        sb.Append("Sell Short when your have a 12 consecutive decline starting with at least a -.388 rating ,");
                    }

                }
                //Sell Short when you have a 3 consecutive decline starting with at least a -.60 rating 
                if (curDayRating <= -.52)
                {
                    if (SellCheckForRatingDecreasingForCount(symbolRatings, 3, -.60))
                    {
                        proEdgeId = "SS2";
                        sb.Append("Sell Short when you have a 3 consecutive decline starting with at least a -.52 rating ,");
                    }

                }
                //Sell short when a rating decreases by 45% on any rating at or below -.40
                if (curDayRating < -.40 && preDayRating >= -.40)
                {
                    if (SellCheckForRatingDropsFromPreDay(symbolRatings, -45.0))
                    {
                        proEdgeId = "SS3";
                        sb.Append("Sell short when a rating decreases by 45% on any rating at or below -.40 ,");
                    }

                }
                symbolAnaltytics.proEdgeId = proEdgeId;
                symbolAnaltytics.alertType = 2;
                symbolAnaltytics.rules = sb.ToString();
                if (proEdgeId != "")
                    log.Info("ProEdge: for Symbol" + symbolAnaltytics.symbol + " Triggered Rule :" + symbolAnaltytics.rules + "on Date " + symbolAnaltytics.proEdgeTriggerDate);
            }
            catch (Exception)
            {

                throw;
            }
            return symbolAnaltytics;

        }

        private static SnpAnalytics checkforOverboughtRules(List<Rating> symbolRatings, SnpAnalytics symbolAnaltytics)
        {
            try
            {
                String proEdgeId = "";
                double curDayRating = symbolRatings[0].ratingValue;
                double preDayRating = symbolRatings[1].ratingValue;
                StringBuilder sb = new StringBuilder();
                //Sell 100%: Approaching Overbought counter Trend -69. or over

                if (symbolRatings[0].ctRatingValue < -69)
                {
                    proEdgeId = "O69";
                    sb.Append("Sell 100%: Approaching Overbought counter Trend -69. or over ,");
                }
                //Sell 50%: Approaching Overbought counter Trend -56. or over
                else if (symbolRatings[0].ctRatingValue < -56)
                {
                    proEdgeId = "O56";
                    sb.Append("Sell 50%: Approaching Overbought counter Trend -56. or over ,");
                }



                symbolAnaltytics.proEdgeId = proEdgeId;
                symbolAnaltytics.alertType = 2;
                symbolAnaltytics.rules = sb.ToString();
                if (proEdgeId != "")
                    log.Info("ProEdge: for Symbol" + symbolAnaltytics.symbol + " Triggered Rule :" + symbolAnaltytics.rules + "on Date " + symbolAnaltytics.proEdgeTriggerDate);
            }
            catch (Exception)
            {

                throw;
            }
            return symbolAnaltytics;

        }

        private static SnpAnalytics checkforNeutralRules(List<Rating> symbolRatings, SnpAnalytics symbolAnaltytics)
        {
            try
            {
                String proEdgeId = "";
                double curDayRating = symbolRatings[0].ratingValue;
                double preDayRating = symbolRatings[1].ratingValue;
                StringBuilder sb = new StringBuilder();

                // check for rating change
                if (symbolRatings[0].rating == 5)
                {
                    proEdgeId = "R5";
                    sb.Append("Rating is " + symbolRatings[0].rating);
                }

                else if (curDayRating == 3)
                {
                    proEdgeId = "R34V30";
                    sb.Append("Rating change " + preDayRating + " to " + curDayRating);
                }
                else if (symbolRatings[0].rating == 4)
                {
                    proEdgeId = "R4";
                    sb.Append("Rating is " + symbolRatings[0].rating);
                }

                else if (curDayRating == 2 )
                {
                    proEdgeId = "R23";
                    sb.Append("Rating change " + preDayRating + " to " + curDayRating);
                }
                else if (curDayRating == 1 )
                {
                    proEdgeId = "R12VN65";
                    sb.Append("Rating change " + preDayRating + " to " + curDayRating);
                }

                // check for rating Value lies in range of 
                if (proEdgeId != "" && ! (proEdgeId.Contains("R34") || proEdgeId.Contains("R12")))
                {
                    if (curDayRating >= .90)
                    {
                        proEdgeId = proEdgeId + "V90";
                    }
                    else if (curDayRating >= .70)
                    {
                        proEdgeId = proEdgeId + "V70";
                    }
                    else if (curDayRating >= .55)
                    {
                        proEdgeId = "R4R5V55";
                    }
                    else if (curDayRating >= .30)
                    {
                        proEdgeId = proEdgeId + "V30";
                    }
                    else if (curDayRating >= 0)
                    {
                        proEdgeId = proEdgeId + "V0";
                    }
                    else if (curDayRating >= -.20)
                    {
                        proEdgeId = proEdgeId + "VN20";
                    }
                    else if (curDayRating >= -.40)
                    {
                        proEdgeId = proEdgeId + "VN40";
                    }
                    else if (curDayRating >= -.65)
                    {
                        proEdgeId = proEdgeId + "VN65";
                    }
                    else if (curDayRating >= -1.0)
                    {
                        proEdgeId = proEdgeId + "VN100";
                    }

                    sb.Append(", rating value of " + curDayRating);

                    // check for rating value increasing, decreasing, or same for specific days

                }
                String increaseOrDecrease = "";
                if (BuyCheckRatingIncreasingByCount(symbolRatings, 3))
                {
                    increaseOrDecrease = "I3";
                    sb.Append(", conseutive increase for 3 days");
                }
                else if (SellCheckForRatingDecreasingForCount(symbolRatings, 3, curDayRating))
                {
                    increaseOrDecrease = "D3";
                    sb.Append(", conseutive decrease for 3 days");
                }
                else if (SellCheckForSpecificRatingForDays(symbolRatings, 3, curDayRating))
                {
                    increaseOrDecrease = "S3";
                    sb.Append(", same for 3 days");
                }
                else if (checkRatingIncreasingOrEqualByCount(symbolRatings, 6, curDayRating))
                {
                    increaseOrDecrease = "IE6";
                    sb.Append(", conseutive 6 equal or increase");
                }
                else
                {
                    increaseOrDecrease = "S3";
                    //increaseOrDecrease = "";
                }

                if (increaseOrDecrease != "")
                {
                    if (proEdgeId != "")
                    {
                        proEdgeId = proEdgeId + increaseOrDecrease;

                    }
                    else if (symbolRatings[0].rating < 5 && (increaseOrDecrease.Equals("I3") || increaseOrDecrease.Equals("D3")))
                    {
                        proEdgeId = increaseOrDecrease;
                    }
                }
                else
                {
                   // proEdgeId = "";

                }

                symbolAnaltytics.proEdgeId = proEdgeId;
                symbolAnaltytics.alertType = 3;
                symbolAnaltytics.rules = sb.ToString();
                if (proEdgeId != "")
                    log.Info("ProEdge: for Symbol" + symbolAnaltytics.symbol + " Triggered Rule :" + symbolAnaltytics.rules + "on Date " + symbolAnaltytics.proEdgeTriggerDate);
            }
            catch (Exception ex)
            {

                log.Error(ex);
            }
            return symbolAnaltytics;

        }

        private static bool checkRatingIncreasingOrEqualByCount(List<Rating> symbolRatings, int increaseCount, double curDayRating)
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
                        if (diff >= 0)
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

        private static bool SellCheckForSpecificRatingForDays(List<Rating> symbolRatings, int days, double ratingValue)
        {
            bool isSpecificRating = false;
            try
            {
                if (symbolRatings.Count > days)
                {
                    for (int i = 0; i < days; i++)
                    {
                        if (symbolRatings[i].ratingValue <= ratingValue)
                        {
                            isSpecificRating = true;
                        }
                        else
                        {

                            isSpecificRating = false;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return isSpecificRating;

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

        private static bool BuyCheckRatingIncreasingByCount(List<Rating> symbolRatings, int increaseCountRequired)
        {
            int increaseRatingCount = 0;
            try
            {

                for (int i = 0; i < symbolRatings.Count - 1; i++)
                {
                    if (increaseRatingCount < increaseCountRequired)
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
            if (increaseRatingCount >= increaseCountRequired)
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


        #region proEdge hist
        internal static void calculateSNPProEdgeIDHIst(DateTime startDate)
        {
            try
            {
                List<DateTime> financialDates = SNPAnalyticsDAO.getFinancialDayDatesFromDB(startDate);
                foreach (DateTime date in financialDates)
                {
                    calculateSNPProEdgeID(date);
                }

            }
            catch (Exception ex)
            {

                log.Error("Error: When claculation hist Pro Edge rule Id  for snp symbols, " + ex);
            }
        }
        #endregion

        internal static void SendProEdgeEmailAlerts()
        {
            try
            {
                log.Info("EmailAlert: Geting subscribed user list");
                List<string> usersEmailsList = EmailAlertsDAO.GetProEdgeSubsUser();
                log.Info("EmailAlert: Geting symbols alerts from DB");
                List<SnpAnalytics> symbolAlerts = SNPAnalyticsDAO.getTodaysProEgeTriggers(); ;

                String Subject = "ChartLab - PROPlus Alerts";
                String From = ConfigurationManager.AppSettings["AdminEmail"];

                StringBuilder AlertsString = new StringBuilder();
                log.Info("EmailAlert: Sending Proplus alerts for users count :" + usersEmailsList.Count);
                foreach (SnpAnalytics symbol in symbolAlerts)
                {
                    if (symbol.alertType == 1)
                    {
                        AlertsString.Append("<div Style='color:green; font-weight:bold' ><img align='middle' src='http://www.chartlabpro.com/images/checkGreen.png' />" + symbol.symbol + " - " + symbol.companyName + " : Unusual Strong Strength.  </div><br>");
                    }
                    else if (symbol.alertType == 2)
                    {
                        AlertsString.Append("<div Style='color:maroon;font-weight:bold' ><img align='middle' src='http://www.chartlabpro.com/images/crossRed.png' />" + symbol.symbol + " - " + symbol.companyName + " : Unusual Weakness.  </div><br>");
                    }

                }
                if (AlertsString.ToString() != "" )
                {
                    String finalAlertsString = "<b>PROPlus Alerts: </b><br>" + AlertsString.ToString();
                    String Body = Constants.HtmlStartString + finalAlertsString + Constants.HtmlEndString;
                    int chunkSize = 40;
                    double temp = (int)usersEmailsList.Count / chunkSize;
                    int chunkCounts = (int) Math.Ceiling(temp);
                    for (int i = 0; i <= chunkCounts; i++)
                    {
                        int index = 40 * i;
                        if (i == chunkCounts)
                        {
                            try
                            {
                                MailUtility.SendMail(Subject, Body, From, usersEmailsList.GetRange(index, usersEmailsList.Count - index));
                            }
                            catch (Exception ex)
                            {
                                log.Info("EmailAlert: PROPlus Alerts Mail sent in range of " + index + " to " + (usersEmailsList.Count - index));
                            }
                        }
                        else
                        {
                            try
                            {
                                MailUtility.SendMail(Subject, Body, From, usersEmailsList.GetRange(index, chunkSize));
                            }
                            catch (Exception ex)
                            {


                            }

                            log.Info("EmailAlert: PROPlus Alerts Mail sent in range of " + index + " to " + (index + chunkSize - 1));
                        }
                    
                    }
                }
                else
                {
                    log.Info("EmailAlert: Today there is no alert.");
                }
            }
            catch (Exception ex)
            {

                log.Error("Error: When Sending ProPlus alerts, " + ex);
            }
        }
    }
}
