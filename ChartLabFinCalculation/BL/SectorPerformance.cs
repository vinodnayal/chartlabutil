using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinLib;

namespace ChartLabFinCalculation
{
    class SectorPerformance
    {
        public static String SectorPerfHistFolderPath;
        public static String SectorPerfDailyFolderPath;

        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(SectorPerformance));

        public static void CalculateSectorDailyStocks()
        {
            
            List<int> sectorIdList = SectorWiseSymbolsDAO.GetSectorId();
            SectorPerformanceDAO.deleteStockSymbols();
            Dictionary<int, int> todayStockList = SectorPerformanceDAO.getTodayTopRatingSymbolStocks();
            Dictionary<int, int> fivedaysStockList = SectorPerformanceDAO.getfiveDaysTopRatingSymbolStocks();

            SectorPerformanceDAO.insertSectorPerf();

            foreach(int sectorId in sectorIdList)
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

            foreach (int sectorId in sectorIdList)
            {
                if (ratingList.ContainsKey(sectorId))
                {
                    SectorPerformanceDAO.updateAvgRating(sectorId, ratingList[sectorId].rating,ratingList[sectorId].ctrating);
                }

            }

        }

        public static void CalculateSectorPerfHistory(bool isHistorical)
        {
            List<int> sectorIdList = SectorWiseSymbolsDAO.GetSectorId();
            List<DateTime> dateList = new List<DateTime>();

            if (isHistorical)
            {
                 dateList = BuySellRatingDAO.GetDistinctRatingDatesFromDB();
            }
            else
            {
                dateList = SectorPerformanceDAO.GetCurrentDate();
                
            }
            List<SectorPerfHist> sectorHistPerfList = new List<SectorPerfHist>();

            try
            {
                foreach (DateTime date in dateList)
                {

                    try
                    {
                        log.Info("\nGetting Avg Rating For Date " + date + "\n");

                        foreach (int secId in sectorIdList)
                        {
                            SectorPerfHist sectorPerf = new SectorPerfHist();

                            double ratingValue = SectorPerformanceDAO.getSectorWiseAvgRating(date, secId);
                            RatingEnum rating = BuySellRatingCalculation.calculateBSRatingEnum(ratingValue);
                            sectorPerf.rating = (int)rating;
                            sectorPerf.date = date;
                            sectorPerf.sectorId = secId;

                            sectorHistPerfList.Add(sectorPerf);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }

                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
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
            CSVExporter.WriteToCSVSectorPerf(sectorHistPerfList, fileName);
            SectorPerformanceDAO.SaveSectorPerfHistDataCSVToDB(fileName,isHistorical);
        }
    }
}
