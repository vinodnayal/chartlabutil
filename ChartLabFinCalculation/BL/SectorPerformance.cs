using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinLib;
using ChartLabFinCalculation.BL;

namespace ChartLabFinCalculation
{
    class SectorPerformance
    {
        public static String SectorPerfHistFolderPath;
        public static String SectorPerfDailyFolderPath;

        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(SectorPerformance));

        public static void CalculateSectorDailyStocks()
        {

            try
            {
                List<int> sectorIdList = SectorWiseSymbolsDAO.GetSectorId();
                SectorPerformanceDAO.deleteStockSymbols();
                log.Info("Process: Getting Todays Top Rating Symbol Stocks");
                Dictionary<int, int> todayStockList = SectorPerformanceDAO.getTodayTopRatingSymbolStocks();
                log.Info("Process: Getting five days before Top Rating Symbol Stocks");
                Dictionary<int, int> fivedaysStockList = SectorPerformanceDAO.getfiveDaysTopRatingSymbolStocks();


                SectorPerformanceDAO.insertSectorPerf();
                log.Info("Process: calculating sector perf for sectors count " + sectorIdList.Count);
                foreach (int sectorId in sectorIdList)
                {
                    if (todayStockList.ContainsKey(sectorId))
                    {
                        SectorPerformanceDAO.updateTodaysStock(sectorId, todayStockList[sectorId]);
                    }

                    if (fivedaysStockList.ContainsKey(sectorId))
                    {
                        SectorPerformanceDAO.updatefiveDaysStock(sectorId, fivedaysStockList[sectorId]);
                    }

                }

                Dictionary<int, SectorPerfRating> ratingList = SectorPerformanceDAO.getRatingAvgValue();
                Dictionary<int, double> ratingValueChangePctDict = SectorPerformanceDAO.getRatingValueChangePct();

                foreach (int sectorId in sectorIdList)
                {
                    if (ratingList.ContainsKey(sectorId))
                    {
                        double ratingValueChangePct = 0;
                        int signalId = 0;
                        if (ratingValueChangePctDict.ContainsKey(sectorId))
                        {
                            ratingValueChangePct = ratingValueChangePctDict[sectorId];
                            signalId = SectorStrenghtCalculation.calculateSectStrengthAlertId(ratingList[sectorId].ratingValue, ratingList[sectorId].ctRatingValue, ratingValueChangePct);
                        }

                        SectorPerformanceDAO.updateAvgRating(sectorId, ratingList[sectorId].rating, ratingList[sectorId].ctrating, ratingList[sectorId].ratingValue, ratingList[sectorId].ctRatingValue, ratingValueChangePct, signalId);
                    }

                }
            }
            catch (Exception ex)
            {

                log.Error("Error: " + ex);
            }

        }

        public static void CalculateSectorPerfHistory(bool isHistorical)
        {
            try
            {

                List<int> sectorIdList = SectorWiseSymbolsDAO.GetSectorId();
                List<DateTime> dateList = new List<DateTime>();

                if (isHistorical)
                {
                    log.Info("Process: Getting historical Dates From DB: ");
                    dateList = BuySellRatingDAO.GetDistinctRatingDatesFromDB(new DateTime(), 0);
                }
                else
                {
                    log.Info("Process: Getting  Current Date From DB ");
                    dateList = BuySellRatingDAO.GetDistinctRatingDatesFromDB(new DateTime(), 1);
                    //dateList = SectorPerformanceDAO.GetCurrentDate();

                }
                List<SectorPerfHist> sectorHistPerfList = new List<SectorPerfHist>();
               
                try
                {
                    //log.Info("Process: Getting Avg Rating For Date " + date + "\n");
                    foreach (int secId in sectorIdList)
                    {
                        try
                        {

                            int count = 0;
                            SectorPerfHist lastSectPerf = new SectorPerfHist();
                            foreach (DateTime date in dateList)
                            {


                                SectorPerfHist sectorPerf = new SectorPerfHist();
                                double ratingValue = SectorPerformanceDAO.getSectorWiseAvgRating(date, secId);
                                RatingEnum rating = BuySellRatingCalculation.calculateBSRatingEnum(ratingValue);
                                sectorPerf.rating = (int)rating;
                                sectorPerf.ratingValue = ratingValue;
                                sectorPerf.date = date;
                                sectorPerf.sectorId = secId;
                                sectorPerf.ratingChangePct = calculateRatingChangePct(lastSectPerf.ratingValue, sectorPerf.ratingValue);

                                if (count!= 0)
                                {
                                    
                                    sectorHistPerfList.Add(sectorPerf);
                                }
                                
                                lastSectPerf = sectorPerf;
                                count++;
                            }
                        }
                        catch (Exception ex)
                        {

                            log.Error("Error: " + ex);
                        }

                    }
                }
                catch (Exception ex)
                {

                    log.Error("Error: " + ex);
                }

                string fileName = "";

                if (isHistorical)
                {
                    fileName = SectorPerfHistFolderPath + "/SectorAvgRatingList.csv";
                }
                else
                {
                    fileName = SectorPerfDailyFolderPath + "/SectorAvgRatingList.csv";
                }
                log.Info("Process: Writing To CSV SectorPerf ");
                CSVExporter.WriteToCSVSectorPerf(sectorHistPerfList, fileName);
                log.Info("Process: Saving Sector Perf Data CSV To DB ");
                SectorPerformanceDAO.SaveSectorPerfHistDataCSVToDB(fileName, isHistorical);
            }
            catch (Exception ex)
            {

                log.Error("Error: " + ex);
            }
        }

        private static double calculateRatingChangePct(double lastDayRatingVAlue, double curRatingValue)
        {
            double changePct = 0;
            try
            {
                if (lastDayRatingVAlue != 0 && curRatingValue != 0)
                {
                    changePct = (curRatingValue - lastDayRatingVAlue) * 100 * Math.Sign(lastDayRatingVAlue) / lastDayRatingVAlue;
                }
            }
            catch (Exception ex)
            {

                log.Error("Error: " + ex);
            }
            return changePct;
        }
    }
}
