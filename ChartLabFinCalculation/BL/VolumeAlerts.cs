using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinLib;
using System.IO;

namespace ChartLabFinCalculation
{
    class VolumeAlerts
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(VolumeAlerts));

       

        public static void CalculateAndSaveHistoricalAlerts(string HistoricalVolumePath, string HistoricalVolumeAlertPerfPath)
        {
            List<String> symbolList = SymbolHistoricalDAO.GetsymbolListFromDB(1, Constants.MAX_ID_DB);
            List<VolumeHistoryList> subsetSymbolData = new List<VolumeHistoryList>();
            List<AlertDateList> totalAlertDateList = new List<AlertDateList>();
            List<DayWiseAvgReturnForSymbol> alertPerformanceList = new List<DayWiseAvgReturnForSymbol>();
            try
            {
                log.Info("Process:  Volume Alert Historical for symbols  " + symbolList.Count);
                for (int i = 0; i < symbolList.Count; i++)
                {
                    string symbol = symbolList[i];
                    Dictionary<DateTime, double> dict = new Dictionary<DateTime, double>();
                    List<VolumeHistoryList> volumeAlertHistory = VolumeDAO.GetDataForAlertFromDB(symbolList[i]);
                    if (volumeAlertHistory.Count != 0)
                    {
                        try
                        {
                            for (int k = 0; k < volumeAlertHistory.Count; k++)
                            {
                                if (dict.ContainsKey(volumeAlertHistory[k].ChangeDate))
                                {
                                    log.Warn("Warn: Duplicate Data in symbolshistorical Symbol : " + symbol + "  Date : " + volumeAlertHistory[k].ChangeDate);
                                }
                                else
                                {
                                    dict.Add(volumeAlertHistory[k].ChangeDate, volumeAlertHistory[k].Close);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error("Error: "+ex);
                        }

                        List<AlertDateList> alertDateList = new List<AlertDateList>();
                        try
                        {
                            for (int j = volumeAlertHistory.Count - 1; j >= 0; j--)
                            {

                                if (j % 60 == 0 && j != 0)
                                {

                                    alertDateList = CalculateAlertListFromAvgVolume(subsetSymbolData, dict, symbolList[i]);
                                    totalAlertDateList.AddRange(alertDateList);
                                    subsetSymbolData.Clear();
                                    subsetSymbolData.Add(volumeAlertHistory[j]);

                                }
                                else
                                {
                                    subsetSymbolData.Add(volumeAlertHistory[j]);
                                    if (j == 0)
                                    {
                                        alertDateList = CalculateAlertListFromAvgVolume(subsetSymbolData, dict, symbolList[i]);
                                        totalAlertDateList.AddRange(alertDateList);
                                        List<DayWiseAvgReturnForSymbol> avgReturnList = GetAverageReturn(dict, totalAlertDateList, symbol);
                                        alertPerformanceList.AddRange(avgReturnList);
                                        subsetSymbolData.Clear();
                                    }
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error("Error: " + ex);
                        }
                    }

                }
            
            CSVExporter.WriteToCSVHistoricalVolume(totalAlertDateList, HistoricalVolumePath + "/HistoricalVolume.csv");
            VolumeDAO.DeleteFromHistoricalVolAlerts();
            VolumeDAO.SaveHistoricalVolumeAlertData(HistoricalVolumePath);
            CSVExporter.WriteToCSVVolumePerformance(alertPerformanceList, HistoricalVolumeAlertPerfPath + "/HistoricalVolumePerformance.csv");
            VolumeDAO.InsertHistoricalVolumeAlertPerf(HistoricalVolumeAlertPerfPath);
            }
            catch (Exception ex)
            {
                log.Error("Error: " + ex);
            }
           

        }

        private static List<AlertDateList> CalculateAlertListFromAvgVolume(List<VolumeHistoryList> subsetSymbolData,Dictionary<DateTime,double> dict, string Symbol)
        {
            List<AlertDateList> alertDateList = new List<AlertDateList>();
            double avgVolume = 0;
            for (int i = 0; i < subsetSymbolData.Count;i++ )
            {
                avgVolume += subsetSymbolData[i].Volume;
            }
            avgVolume = avgVolume / subsetSymbolData.Count;
            try
            {
                for (int i = 0; i < subsetSymbolData.Count; i++)
                {
                    double PctChange = (subsetSymbolData[i].Volume - avgVolume) * 100 / subsetSymbolData[i].Volume;
                    if (PctChange > 45)
                    {
                        AlertDateList alertDateObj = new AlertDateList();
                        alertDateObj.ChangeDate = subsetSymbolData[i].ChangeDate;
                        alertDateObj.Volume = subsetSymbolData[i].Volume;
                        alertDateObj.PctChange = PctChange;
                        alertDateObj.Symbol = Symbol;
                        alertDateObj.avgVolume = avgVolume;
                        double previousDayPrice=0;
                        double todayPrice = 0;
                        double PctChangeInPrice = 0;
                        DateTime changeDate = subsetSymbolData[i].ChangeDate;

                        if (dict.ContainsKey(changeDate))
                            {
                                int count = 1;
                                if (dict.ContainsKey(changeDate.AddDays(-count)))
                                {
                                    previousDayPrice = dict[changeDate.AddDays(-count)];
                                   
                                }
                                else
                                {
                                    for (int k = 2; k <= 5; k++)
                                    {
                                        if (dict.ContainsKey(changeDate.AddDays(-k)))
                                        {
                                            previousDayPrice = dict[changeDate.AddDays(-k)];
                                            break;
                                        }
                                    }
                                }
                               
                            }
                      
                            
                             todayPrice = subsetSymbolData[i].Close;
                             PctChangeInPrice = (todayPrice - previousDayPrice) * 100 / previousDayPrice;

                            if (previousDayPrice != 0 || todayPrice != 0 || PctChangeInPrice != 0)
                            {
                                if (PctChangeInPrice > 0)
                                {
                                    alertDateObj.volumeAlertType = Constants.Vol_ALERT_POSITIVE;
                                }
                                else
                                {
                                    alertDateObj.volumeAlertType = Constants.Vol_ALERT_NEGATIVE;
                                }
                                
                            }
                            else
                            {
                                alertDateObj.volumeAlertType = Constants.Vol_ALERT_NEUTRAL;
                            }
                        
                        alertDateList.Add(alertDateObj);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error: " + ex);
            }
                return alertDateList;
        }
        public static void CalculateDailyAverageVolume(string DailyAvgVolumeFolderPath)
        {
            try
            {
                //log.Info("Process: Getting symbol List From DB");
                //List<String> symbolList = SymbolHistoricalDAO.GetsymbolListFromDB(1, Constants.MAX_ID_DB);
                //log.Info("Process: Getting Daily Avg Volume for symbol list for symbol count " + symbolList.Count);
                //List<DailyAverageVolume> dailyAvgVolumeList = GetDailyAvgVolume(symbolList);
                //log.Info("Process: Write To CSV Daily Avg Volume");
                //CSVExporter.WriteToCSVDailyAvgVolume(dailyAvgVolumeList, DailyAvgVolumeFolderPath + "/DailyAvgVolume.csv");
                //log.Info("Process: Save Daily Avg Volume Data");
                //VolumeDAO.SaveDailyAvgVolumeData(DailyAvgVolumeFolderPath);
                
                log.Info("Process: Update Volume Table");
                VolumeDAO.UpdateVolumeTable();

            }
            catch (Exception ex)
            {
                
                log.Error("Error: "+ex);
            }


        }

        private static List<DailyAverageVolume> GetDailyAvgVolume(List<String> symbolList)
        {
            List<DailyAverageVolume> dailyAvgVolumeList = new List<DailyAverageVolume>();
            List<String> subsetSymbolList = new List<String>();
            try
            {
                for (int i = 0; i < symbolList.Count; i++)
                {

                    if (i % 20 == 0 && i != 0)
                    {
                        dailyAvgVolumeList.AddRange(DataDownloader.GetDailyAvgVolumeDataFromYahoo(subsetSymbolList));
                        subsetSymbolList.Clear();
                        subsetSymbolList.Add(symbolList[i]);

                    }
                    else
                    {
                        subsetSymbolList.Add(symbolList[i]);
                        if (i == symbolList.Count - 1)
                        {
                            dailyAvgVolumeList.AddRange(DataDownloader.GetDailyAvgVolumeDataFromYahoo(subsetSymbolList));
                            subsetSymbolList.Clear();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error: " + ex);
            }

                return dailyAvgVolumeList;
        }

        private static List<DayWiseAvgReturnForSymbol> GetAverageReturn(Dictionary<DateTime, double> dict, List<AlertDateList> volumeAlertDateList, String symbol)
         {
             List<DayWiseAvgReturnForSymbol> AvgReturnList = new List<DayWiseAvgReturnForSymbol>();
             List<double> currentDate = new List<double>();
             List<double> AvgReturnList_2_Days = new List<double>();
             List<double> AvgReturnList_5_Days = new List<double>();
             List<double> AvgReturnList_30_Days = new List<double>();
             List<double> AvgReturnList_90_Days = new List<double>();
             DayWiseAvgReturnForSymbol dayWiseAvgRetObj = new DayWiseAvgReturnForSymbol();

             try
             {
                 if (volumeAlertDateList.Count != 0)
                 {
                     log.Info("Calculating Volume Alert Performance for symbol " + symbol);
                     for (int i = 0; i < volumeAlertDateList.Count; i++)
                     {
                         DateTime date = volumeAlertDateList[i].ChangeDate;
                         if (date < DateTime.Now.AddDays(-1))
                         {
                             double todayprice = Common.GetDayWiseReturn(dict, date, 0, true);
                             double twoday = Common.GetDayWiseReturn(dict, date, 2, true);
                             double fiveday = Common.GetDayWiseReturn(dict, date, 5, true);
                             double thirtyday = Common.GetDayWiseReturn(dict, date, 30, true);
                             double nintyday = Common.GetDayWiseReturn(dict, date, 90, true);


                             if (todayprice != 0 && twoday != 0)
                             {
                                 AvgReturnList_2_Days.Add(100*(twoday - todayprice) / todayprice);
                             }
                             if (todayprice != 0 && fiveday != 0)
                             {
                                 AvgReturnList_5_Days.Add(100 * (fiveday - todayprice) / todayprice);
                             }
                             if (todayprice != 0 && thirtyday != 0)
                             {
                                 AvgReturnList_30_Days.Add(100 * (thirtyday - todayprice) / todayprice);
                             }
                             if (todayprice != 0 && nintyday != 0)
                             {
                                 AvgReturnList_90_Days.Add(100 * (nintyday - todayprice) / todayprice);
                             }

                             

                         }

                     }
                    
                     dayWiseAvgRetObj.AvgReturn_2_Days = Common.CalculateAvgReturn(AvgReturnList_2_Days).AvgReturn;
                     dayWiseAvgRetObj.AvgReturn_5_Days = Common.CalculateAvgReturn(AvgReturnList_5_Days).AvgReturn;
                     dayWiseAvgRetObj.AvgReturn_30_Days = Common.CalculateAvgReturn(AvgReturnList_30_Days).AvgReturn;
                     dayWiseAvgRetObj.AvgReturn_90_Days = Common.CalculateAvgReturn(AvgReturnList_90_Days).AvgReturn;

                     log.Info(symbol +" : two day return avg ="+ dayWiseAvgRetObj.AvgReturn_2_Days);

                     dayWiseAvgRetObj.symbol = symbol;
                     AvgReturnList.Add(dayWiseAvgRetObj);


                 }
                 else
                 {
                     dayWiseAvgRetObj.AvgReturn_2_Days = 0;
                     dayWiseAvgRetObj.AvgReturn_5_Days = 0;
                     dayWiseAvgRetObj.AvgReturn_30_Days = 0;
                     dayWiseAvgRetObj.AvgReturn_90_Days = 0;
                     dayWiseAvgRetObj.symbol = symbol;
                     AvgReturnList.Add(dayWiseAvgRetObj);
                 }
             }
             catch (Exception ex)
             {
                 log.Error("Error: " + ex);
             }
             return AvgReturnList;

         }


        public static void InsertVolumeAlertDaily()
         {
             try
             {
                 log.Info("Process: Insert Daily Volume Alert in historicalvolumealerts");
                 VolumeDAO.InsertDailyVolumeAlert();
             }
             catch (Exception ex)
             {
                 
                 log.Error("Error: "+ex);
             }
         }
    }
}
