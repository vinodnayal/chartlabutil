using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinLib;
using System.IO;

namespace ChartLabFinCalculation
{
    class BuySellRatingCalculation
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(BuySellRatingCalculation));

        public static String BuySellRatingFilePath;
        public static String BuySellRatingCsvFilePath;
        public static String BuySellRatingChangeHistCsvFilePath;


        public static void calculateBuySellRating()
        {


            List<BuySellRating> BuySellRatingList = new List<BuySellRating>();
            List<Rating> RatingList = new List<Rating>();

            StreamReader readFile = new StreamReader(BuySellRatingFilePath + "\\STOCK_PWTS.TXT");
            try
            {

                string line;
                string[] row;
                int flag = 0;
                // RatingList
                while ((line = readFile.ReadLine()) != null)
                {
                    BuySellRating ratingObject = new BuySellRating();
                    row = line.Split(' ');
                    for (int i = 1; i < row.Length; i++)
                    {

                        if (row[i] != "" && flag == 0)
                        {
                            log.Info("\nCalculating BuySell Rating for symbol: " + row[i]);
                            ratingObject.symbol = row[i];
                            flag = 1;
                        }
                        else if (row[i] != "" && flag == 1)
                        {
                            double ratingValue = double.Parse(row[i]);
                            RatingEnum rating = calculateBSRatingEnum(ratingValue);
                            ratingObject.rating = (int)rating;
                            ratingObject.ratingValue = ratingValue;
                            flag = 0;
                        }
                        if (i == row.Length - 1)
                            BuySellRatingList.Add(ratingObject);
                    }



                }



                List<CTRating> ctRatingList = CTRatingCalculation.CalculateCTRating(BuySellRatingFilePath);
                for (int i = 0; i < BuySellRatingList.Count; i++)
                {
                    for (int j = 0; j < ctRatingList.Count; j++)
                    {
                        if (BuySellRatingList[i].symbol == ctRatingList[j].symbol)
                        {
                            Rating ratingObj = new Rating();
                            ratingObj.symbol = BuySellRatingList[i].symbol;
                            ratingObj.ratingValue = BuySellRatingList[i].ratingValue;
                            ratingObj.ratingDate = BuySellRatingList[i].ratingDate;
                            ratingObj.rating = BuySellRatingList[i].rating;
                            ratingObj.ctRatingValue = ctRatingList[j].ctRatingValue;
                            ratingObj.ctRating = ctRatingList[j].ctRating;
                            RatingList.Add(ratingObj);
                        }

                    }
                }


            }
            catch (Exception ex)
            {
                log.Error(ex);

            }
            CSVExporter.WriteToCSVRating(RatingList, BuySellRatingCsvFilePath + "/RatingFile.csv");
            BuySellRatingDAO.InsertRating(BuySellRatingCsvFilePath);

            BuySellRating SnPAvgRating = CalculateAvgRatingForSnP(Constants.GSPCSymbol);
            BuySellRatingDAO.InsertSnPAvgRating(SnPAvgRating);
        
        
        
        
        }

        private static BuySellRating CalculateAvgRatingForSnP(string symbol)
        {
            BuySellRating snpAvgRatingObj = new BuySellRating();
            Dictionary<string, double> symbolsWeight = BuySellRatingDAO.GetSymbolsWeight();
            Dictionary<string, double> symbolsRatingValue = BuySellRatingDAO.GetSymbolsRating();

            int TotalSymbols = 0;
            double TotalRatingValue = 0;
            double avgRatingValue = 0;

            foreach (KeyValuePair<string, double> pair in symbolsWeight)
            {
                if (symbolsRatingValue.ContainsKey(pair.Key))
                {
                    TotalRatingValue = TotalRatingValue + (pair.Value * symbolsRatingValue[pair.Key]);
                    TotalSymbols++;
                }

            }

            avgRatingValue = TotalRatingValue / TotalSymbols;
            RatingEnum rating = calculateBSRatingEnum(avgRatingValue);
            snpAvgRatingObj.symbol = Constants.GSPCSymbol;//GSPCGoogleSymbol;
            snpAvgRatingObj.rating = (int)rating;
            snpAvgRatingObj.ratingValue = avgRatingValue;


            return snpAvgRatingObj;
        }



        public static RatingEnum calculateBSRatingEnum(double p)
        {
            RatingEnum r;
            if (p > -.3 && p <= .3)
            {
                r = RatingEnum.Neutral;
            }
            else if (p > .3 && p <= .6)
            {
                r = RatingEnum.Buy;
            }
            else if (p > -.6 && p <= -.3)
            {
                r = RatingEnum.Sell;
            }
            else if (p > .6)
            {
                r = RatingEnum.StrongBuy;
            }
            else
            {
                r = RatingEnum.StrongSell;
            }
            return r;
        }

        public static void BuySellChangeHistory()
        {

            List<BuySellRating> historyBuySellRatingList = BuySellRatingDAO.getBuySellRatingHistroyFromDB();
            List<BuySellRatingChangeHist> ChangeBuySellRatingHist = getBuySellRatingChangelist(historyBuySellRatingList);
            CSVExporter.WriteToCSVChangeRatingHistory(ChangeBuySellRatingHist, BuySellRatingChangeHistCsvFilePath + "/ChangeRatingHistoryFile.csv");
            BuySellRatingDAO.InsertChangeRatingHistoryCSVToDB(BuySellRatingChangeHistCsvFilePath, "buySellRatingChangeHistory");

        }
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
                log.Error("Problem in calculating Buy sell rating change hist");
            }
            return ChangeBuySellRatingHist;
        }

        internal static void updateETFRatings()
        {
            try
            {
                BuySellRatingDAO.updateETFRatingsfromHistBSRatingTbl();
            }
            catch (Exception ex)
            {
                
                log.Error("Problem in calculating Buy sell rating for ETFs"+ex);
            }
        }
    }
}
