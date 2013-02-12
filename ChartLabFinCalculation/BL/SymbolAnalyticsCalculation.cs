using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinLib;
using System.IO;

namespace ChartLabFinCalculation
{
    class SymbolAnalyticsCalculation
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(SymbolAnalyticsCalculation));

        public static String SymbolAnalyticsPath;
        public static String patternsFilePath;
        public static String PatternsCsvFilePath;

        public static void CalculateSymbolAnalytics(int from,int to,DateTime fromDate,DateTime toDate,bool isMF)
        {
            
            string type = null;
           
            List<InputBarData> listInputDataForSymbols = new List<InputBarData>();
            List<String> symbolList = new List<string>();

            if (isMF)
            {

                symbolList = SymbolHistoricalDAO.GetIndexMFFromDB();
            }
            else
            {
                symbolList = SymbolHistoricalDAO.GetsymbolListFromDB(from, to); 
                //new List<string>();
              //  symbolList.Add("GOOG");
                // SymbolHistoricalDAO.GetsymbolListFromDB(from, to);
            }

            foreach (String symbol in symbolList)
            {
                List<BarData> barlist = null;
                log.Info("Getting Data For symbol: " + symbol);

                try
                {
                    barlist = SymbolHistoricalMongoDAO.GetHistoricalDataFromMongo(fromDate, toDate, symbol);
                }
                catch (Exception ex)
                {

                    log.Error("Error:" + ex);
                }
                if (barlist == null || barlist.Count == 0)
                {

                    log.Info("Empty List Returned From Provider" + symbol);
                }
                else
                {
                    InputBarData inputListRow = new InputBarData();
                    inputListRow.barListRow = barlist;
                    inputListRow.symbol = symbol;
                    listInputDataForSymbols.Add(inputListRow);

                }

            }
            if (isMF)
            {
                type = "indicies";
            }
            else
            {
                type = "symbols";
            }

            log.Info("Calculaitng SymbolAnalytics for "+type+"....");
            List<SymbolAnalytics> symbolAnalyticsList = CalculateAnalytics(listInputDataForSymbols,isMF);
            if (isMF)
            {
                CSVExporter.WriteToCSV(symbolAnalyticsList, SymbolAnalyticsPath + "/SymbolAnalytics_Index_MF.csv");
            }
            else
            {
                CSVExporter.WriteToCSV(symbolAnalyticsList, SymbolAnalyticsPath + "/SymbolAnalytics_" + from + "_" + to + ".csv");
            }
           
            log.Info("Calculaitng Patterns for "+type+"....");
            List<PatternBarData> PatternMixData = calculatePatterns(listInputDataForSymbols);
            DirectoryInfo di = new DirectoryInfo(patternsFilePath);//AppDomain.CurrentDomain.BaseDirectory);
            try
            {
                foreach (FileInfo file in di.GetFiles("*.xml"))
                {

                    String PatternType = (file.Name).Replace(".apr.xml", "");
                    List<PatternBarData> PatternSeperateData = PatternMixData.Where(x => x.Pattern.Equals(PatternType, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (isMF)
                    {
                        CSVExporter.WriteToCSVPattern(PatternSeperateData, PatternsCsvFilePath + "/Pattern_MF" + ".csv");
                    }
                    else
                    {
                        CSVExporter.WriteToCSVPattern(PatternSeperateData, PatternsCsvFilePath + "/Pattern_" + PatternType + "_" + from + "_" + to + ".csv");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error:" + ex);
            }

        }

        public static void CalculateSymbolAnalytics(DateTime fromDate,DateTime toDate,bool isMF)
        {
             CalculateSymbolAnalytics(0, 0, fromDate,toDate,isMF);

        }

        private  static List<SymbolAnalytics> CalculateAnalytics(List<InputBarData> inputBarList,bool isMF)
        {


            List<SymbolAnalytics> symbolAnalyticsList = new List<SymbolAnalytics>();
            List<HistoricalDates> listHistoricalDates = CommonDAO.getHistoricalDatesFromDB();


            foreach (InputBarData inputRow in inputBarList)
            {
                FinCalculator fincalc = new FinCalculator();
                List<BarData> barlist = inputRow.barListRow;
                String symbol = inputRow.symbol;
                log.Info("Calculaitng SymbolAnalytics for symbol: " + symbol);
                try
                {
                    SymbolAnalytics symbolAnalytics = fincalc.CalculateAnalytics(symbol, barlist, listHistoricalDates);

                    //if (isMF)
                    //{
                    //   // symbolAnalytics.symbol = SymbolAnalyticsDAO.GetIndexSymbolName(symbol);
                    //}
                    //else
                    //{
                        symbolAnalytics.symbol = symbol;
                    //}

                    TrendObjects trends = calculateTrend(symbol, barlist);
                    symbolAnalytics.longTermTrend = trends.longTermTrend;
                    symbolAnalytics.mediumTermTrend = trends.mediumTermTrend;
                    symbolAnalytics.shortTermTrend = trends.shortTermTrend;
                    symbolAnalytics.shortTermTrendDate = trends.shortTermTrendDate;

                    symbolAnalyticsList.Add(symbolAnalytics);
                }
                catch (Exception ex)
                {
                    log.Error("Error for Symbol While calculaing Analytics " + symbol);
                    log.Error(ex);
                }
            }


            return symbolAnalyticsList;



        }

        private static List<PatternBarData> calculatePatterns(List<InputBarData> inputBarList)
        {
            List<PatternBarData> patternRowList = new List<PatternBarData>();
            try
            {
               
                if (!Directory.Exists(patternsFilePath))
                {
                    Directory.CreateDirectory(patternsFilePath);
                }
                DirectoryInfo di = new DirectoryInfo(patternsFilePath);//AppDomain.CurrentDomain.BaseDirectory);
                foreach (InputBarData inputRow in inputBarList)
                {
                    List<BarData> barlist1 = inputRow.barListRow;
                    List<BarData> barlistNew = new List<BarData>();
                    // barlist.Reverse();


                    if (barlist1.Count > 51)
                    {
                        barlistNew = barlist1.GetRange(barlist1.Count - 51, 50);
                    }
                    //  barlist.Reverse();
                    String symbol = inputRow.symbol;



                    foreach (FileInfo file in di.GetFiles("*.xml"))
                    {
                        PatternRecognizer patternRecognizer = new PatternRecognizer();
                        String PatternType = (file.Name).Replace(".apr.xml", "");
                        if (file.Name.Contains("Uptrend"))
                        {
                            log.Info("Calculating " + PatternType + " Pattern for symbol " + symbol);
                        }
                        log.Info("Calculating " + PatternType + " Pattern for symbol " + symbol);

                        patternRecognizer.Init();
                        patternRecognizer.AppendRecords(barlistNew);
                        int count = patternRecognizer.Scan((file).FullName, "{2E036561-F762-471d-93FD-869AFE438639}");
                        if (count == -1)
                        {
                            log.Info("Wrong License code.");

                        }


                        //analize results, in this sample we just display in a list
                        int i = 0;
                        foreach (var patternValue in patternRecognizer.Results)
                        {

                            PatternBarData patternRow = new PatternBarData();
                            int StartIndex = (int)patternValue.Interval.x;
                            int EndIndex = (int)patternValue.Interval.y;
                            DateTime PatternStartDate = barlistNew[StartIndex].date;
                            DateTime PatternEndDate = barlistNew[EndIndex].date;


                            patternRow.startDate = PatternStartDate;
                            patternRow.endDate = PatternEndDate;
                            patternRow.symbol = symbol;

                            PatternEnum pattern = (PatternEnum)Enum.Parse(typeof(PatternEnum), PatternType);
                            patternRow.Pattern = PatternType;
                            patternRow.PatternId = (int)pattern;
                            bool alreadyPresent = false;
                            foreach (PatternBarData checkData in patternRowList)
                            {

                                if (checkData.symbol == patternRow.symbol && checkData.endDate == patternRow.endDate && checkData.PatternId == patternRow.PatternId)
                                {

                                    alreadyPresent = true;
                                }
                            }
                            if (!alreadyPresent)
                            {
                                patternRowList.Add(patternRow);
                            }


                        }


                    }

                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return patternRowList;

        }

        private static TrendObjects calculateTrend(String symbol, List<BarData> barlist)
        {

            TrendObjects TrendObject = null;

            if (barlist == null || barlist.Count == 0)
            {
                log.Info("Empty List Returned From Provider" + symbol);
            }

            else
            {
                try
                {
                    log.Info("Calculating Trend For symbol:  " + symbol);
                    
                    TrendCalculation trendcalc = new TrendCalculation();
                    TrendObject = trendcalc.CalulateAllTrends(barlist, symbol);
                }
                catch (Exception ex)
                {
                    log.Error("Error for Symbol While calculaing Trend " + symbol);
                    log.Error(ex);
                }
            }



            return TrendObject;

        }

        public static void SaveSymbolAnalytics()
        {
            
            #region   update symbol analytics in db
            DirectoryInfo di = new DirectoryInfo(SymbolAnalyticsPath);
            FileInfo[] fileEntries = di.GetFiles("*.csv");
            int count = 0;
            foreach (FileInfo fileName in fileEntries)
            {
                if (count == 0)
                {
                    SymbolAnalyticsDAO.SymbolAnalyticsCSVToDB(SymbolAnalyticsPath, fileName.Name, true);
                }
                else
                {
                    SymbolAnalyticsDAO.SymbolAnalyticsCSVToDB(SymbolAnalyticsPath, fileName.Name, false);
                }
                count++;
            }
            #endregion

            #region update patterns in db

            DirectoryInfo diPatterns = new DirectoryInfo(PatternsCsvFilePath);
            FileInfo[] fileEntriesPatterns = diPatterns.GetFiles("*.csv");
            SymbolAnalyticsDAO.DeletePatterns();
            foreach (FileInfo fileName in fileEntriesPatterns)
            {
                SymbolAnalyticsDAO.UpdatedPatterns(PatternsCsvFilePath, fileName.Name);

            }

            SymbolAnalyticsDAO.UpdatedAlertsInSymbolAnalytics();

            #endregion
        }
    }
}
