﻿using System;
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
            try
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
                    log.Info("Rating: Calculating BuySell Rating ");
                    while ((line = readFile.ReadLine()) != null)
                    {
                        BuySellRating ratingObject = new BuySellRating();
                        row = line.Split(' ');

                        for (int i = 1; i < row.Length; i++)
                        {
                            if (row[i] != "" && flag == 0)
                            {
                                // log.Info("Rating: Calculating BuySell Rating for symbol: " + row[i]);
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
                    log.Info("Rating: Calculated BuySell Rating for symbols count: " + BuySellRatingList.Count);


                    List<CTRating> ctRatingList = CTRatingCalculation.CalculateCTRating(BuySellRatingFilePath);
                    log.Info("Rating: merging Ct ratings to rating object with bs ratings");
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
                    log.Error("Error: " + ex);

                }
                CSVExporter.WriteToCSVRating(RatingList, BuySellRatingCsvFilePath + "/RatingFile.csv");
                log.Info("Rating:Write To CSV  ");

                BuySellRatingDAO.InsertRating(BuySellRatingCsvFilePath);
                log.Info("Rating:Inserted Rating in to DB: ");

                log.Info("Rating: Calculating Avg Rating For SnP");
                BuySellRating SnPAvgRating = CalculateAvgRatingForSnP(Constants.GSPCSymbol);

                BuySellRatingDAO.InsertSnPAvgRating(SnPAvgRating);
                log.Info("Rating: inserted in DB (historybuysellrating, symbolanalytics tables), Avg Rating For SnP");

                List<DateTime> datelist = BuySellRatingDAO.GetDistinctRatingDatesFromDB(new DateTime(), 2);
                DateTime curdate = datelist[0];
                DateTime predate = datelist[1];
                BuySellRatingDAO.insertTopRatingAddRemoveHist(curdate, predate);
                log.Info("Rating: Calculated added/removed Top Rating symbols from pre day ");
            }
            catch (Exception ex)
            {
                log.Error("Error: " + ex);

            }

        }


        private static BuySellRating CalculateAvgRatingForSnP(string symbol)
        {
            BuySellRating snpAvgRatingObj = new BuySellRating();
            try
            {
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
                    else
                    {
                        log.Warn("Rating: " + pair.Key + " not found in Calculating eAvg Rating For SnP");
                    }

                }

                avgRatingValue = TotalRatingValue / TotalSymbols;
                RatingEnum rating = calculateBSRatingEnum(avgRatingValue);
                snpAvgRatingObj.symbol = Constants.GSPCSymbol;//GSPCGoogleSymbol;
                snpAvgRatingObj.rating = (int)rating;
                snpAvgRatingObj.ratingValue = avgRatingValue;
            }
            catch (Exception ex)
            {

                log.Error("Error: " + ex);
            }


            return snpAvgRatingObj;
        }



        public static RatingEnum calculateBSRatingEnum(double p)
        {
            RatingEnum r;
            try
            {
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
            }
            catch (Exception)
            {

                throw;
            }
            return r;
        }

        public static void BuySellChangeHistory()
        {

            try
            {
                log.Info("Rating: getting BuySell Rating Histroy From DB");
                List<BuySellRating> historyBuySellRatingList = BuySellRatingDAO.getBuySellRatingHistroyFromDB();
                log.Info("Rating: getting Change BuySell Rating Hist");
                List<BuySellRatingChangeHist> ChangeBuySellRatingHist = getBuySellRatingChangelist(historyBuySellRatingList);
                CSVExporter.WriteToCSVChangeRatingHistory(ChangeBuySellRatingHist, BuySellRatingChangeHistCsvFilePath + "/ChangeRatingHistoryFile.csv");
                log.Info("Rating: Write To CSV Change Rating History");
                BuySellRatingDAO.InsertChangeRatingHistoryCSVToDB(BuySellRatingChangeHistCsvFilePath, "buySellRatingChangeHistory");
                log.Info("Rating: Inserted Change Rating History CSV To DB ");
            }
            catch (Exception ex)
            {

                log.Error("Error: Problem in Buy Sell ChangeHistory" + ex);
            }
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
                log.Error("Error: Problem in calculating Buy sell rating change hist" + ex);
            }
            return ChangeBuySellRatingHist;
        }

        internal static void updateETFRatings()
        {
            try
            {
               
                BuySellRatingDAO.updateETFRatingsfromHistBSRatingTbl();
                log.Info("Rating: updated ETF Ratings from HistBuySellRating Tbl");
            }
            catch (Exception ex)
            {

                log.Error("Error: Problem in calculating Buy sell rating for ETFs" + ex);
            }
        }

        internal static void backupBSRatingFiles(String BuySellRatingFTPFilesPath)
        {


            try
            {
                log.Info("Rating: Copying ratings files to backup folder...");
                const String STOCK_PWTS = "STOCK_PWTS.TXT";
                const String STOCK_PWTS_S1 = "STOCK_PWTS_S1.TXT";
                const String STOCK_PWTS_S2 = "STOCK_PWTS_S2.TXT";

                string backupPath = BuySellRatingFTPFilesPath + "\\BuySellRatingsBackup\\";
                string prefix = DateTime.Now.ToString("yyyyMMddhhmm") + "_";


                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                }

                FileInfo STOCK_PWTS_INFO = new FileInfo(BuySellRatingFTPFilesPath + "\\" + STOCK_PWTS);
                if (STOCK_PWTS_INFO.Exists)
                {
                    STOCK_PWTS_INFO.CopyTo(string.Format("{0}{1}", backupPath, prefix + STOCK_PWTS));
                }

                FileInfo STOCK_PWTS_S1_INFO = new FileInfo(BuySellRatingFTPFilesPath + "\\" + STOCK_PWTS_S1);
                if (STOCK_PWTS_S1_INFO.Exists)
                {
                    STOCK_PWTS_S1_INFO.CopyTo(string.Format("{0}{1}", backupPath, prefix + STOCK_PWTS_S1));
                }

                FileInfo STOCK_PWTS_S2_INFO = new FileInfo(BuySellRatingFTPFilesPath + "\\" + STOCK_PWTS_S2);
                if (STOCK_PWTS_INFO.Exists)
                {
                    STOCK_PWTS_S2_INFO.CopyTo(string.Format("{0}{1}", backupPath, prefix + STOCK_PWTS_S2));
                }
                log.Info("Rating: done ! Copying ratings files to backup folder.");
            }
            catch (Exception ex)
            {

                log.Error("Error: " + ex);
            }
        }

        internal static void SaveTopRatingSymbolsHist()
        {
            try
            {
                log.Info("Rating: getting daily top rating symbols");
                List<Rating> totalTopSymbolsList = new List<Rating>();
                List<DateTime> ratingDatesList = BuySellRatingDAO.GetDistinctRatingDatesFromDB(DateTime.Now.AddDays(-300), 0);
                foreach (DateTime date in ratingDatesList)
                {
                    log.Info("Rating: getting top symbols for date " + date);
                    List<Rating> topSymbolsList = BuySellRatingDAO.getTopRatigSymbolOnSpecificDate(date);
                    totalTopSymbolsList.AddRange(topSymbolsList);
                }

                CSVExporter.WriteToCSVTopRatingSymbolsHist(totalTopSymbolsList, BuySellRatingCsvFilePath + "/TopRatingSymbolsHist.csv");
                log.Info("Rating: Write  To CSV Top Rating Symbols Hist");
                BuySellRatingDAO.InsertTopRatingSymbolsHistCSVToDB(BuySellRatingCsvFilePath);
                log.Info("Rating: Inserted Top Rating Symbols Hist CSV To DB ");

            }
            catch (Exception ex)
            {

                log.Error("Error: " + ex);
            }
        }

        internal static void SaveTopRatingAddRemoveSymbolsHist()
        {
            try
            {
                log.Info("Rating: 14 days add remove symbols");
                List<DateTime> ratingDatesList = BuySellRatingDAO.GetDistinctRatingDatesFromDB(DateTime.Now.AddDays(-300),0);
                int count = 0;
                DateTime predayDate = new DateTime();
                foreach (DateTime date in ratingDatesList)
                {
                    if (count != 0)
                    {
                        log.Info("Rating: insert TopRating Add/Remove Hist for date " + date);
                        BuySellRatingDAO.insertTopRatingAddRemoveHist(date, predayDate);
                    }
                    predayDate = date;
                    count++;

                }
            }
            catch (Exception ex)
            {

                log.Error("Error: " + ex);
            }
        }
    }
}
