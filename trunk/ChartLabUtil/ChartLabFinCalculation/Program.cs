using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinLib;
using HelperLib;
using System.Configuration;
using System.IO;
using System.Data.Odbc;
using FinLib.Model;
using log4net.Config;
using ChartLabFinCalculation.BL;
using ChartLabFinCalculation.DAL;
using ChartLabFinCalculation.UTIL;

namespace ChartLabFinCalculation
{
    class Program
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(Program));
        static log4net.ILog logTime = log4net.LogManager.GetLogger(typeof(Program));
        static string SymbolAnalyticsPath = ConfigurationManager.AppSettings["SymbolAnalyticsFolder"];
        static string patternsFilePath = ConfigurationManager.AppSettings["PatternsFilePath"];
        static string OBOSRatingPath = ConfigurationManager.AppSettings["OBOSRatingFolder"];
        static string PatternsCsvFilePath = ConfigurationManager.AppSettings["PatternsCsvFolder"];
        static string MyConString = ConfigurationManager.AppSettings["ConnectionString"];
        static string UpdatedTrendCsvFilePath = ConfigurationManager.AppSettings["UpdatedTrendFolder"];
        static string BuySellRatingFilePath = ConfigurationManager.AppSettings["BuySellRatingFolder"];
        static string BuySellRatingCsvFilePath = ConfigurationManager.AppSettings["BSRatingCsvFolder"];
        static string BuySellRatingChangeHistCsvFilePath = ConfigurationManager.AppSettings["BSRatingChangeHistCsvFolder"];
        static string UsersInfoFilePath = ConfigurationManager.AppSettings["UsersInfoFolder"];
        static string HistoricalDataFilePath = ConfigurationManager.AppSettings["HistoricalDataFilePath"];
        static string ERRORSymbolsPath = ConfigurationManager.AppSettings["ERRORSymbolsPath"];
        static string NewSymbolsPath = ConfigurationManager.AppSettings["NewSymbolsFilePath"];
        static string IndustrySymbolFilePath = ConfigurationManager.AppSettings["IndustrySymbolFolder"];
        static string DailyAvgVolumeFolderPath = ConfigurationManager.AppSettings["DailyAvgVolumePath"];
        static string HistoricalVolumeFolderPath = ConfigurationManager.AppSettings["HistoricalVolumePath"];
        static string HistoricalVolumeAlertPerfFolderPath = ConfigurationManager.AppSettings["HistoricalVolumeAlertPerfPath"];
        static string SentimentIndicatorPerfFolderPath = ConfigurationManager.AppSettings["SentimentIndicatorPerfPath"];
        static string GroupPerformanceFolderPath = ConfigurationManager.AppSettings["GroupPerformancePath"];
        static string StrongWeakSymbolsFolderPath = ConfigurationManager.AppSettings["StrongWeakSymbolsFilePath"];
        static string LongShortPerfFolderPath = ConfigurationManager.AppSettings["LongShortPerfFilePath"];
        static string SectorPerfHistFolderPath = ConfigurationManager.AppSettings["SectorPerfHistFilePath"];
        static string SectorPerfDailyFolderPath = ConfigurationManager.AppSettings["SectorPerfDailyFilePath"];
        static string SymbolRuleCsvFilePath = ConfigurationManager.AppSettings["SymbolRuleFilePath"];
        static string SnPSpecificDatePricesPath = ConfigurationManager.AppSettings["SnPSpecificDatePricesPath"];
        static string StatisticsPath = ConfigurationManager.AppSettings["StatisticsPath"];
        static string CTRatingChangeHistFolderPath = ConfigurationManager.AppSettings["CTRatingChangeHistFilePath"];
        static string AlertsPath = ConfigurationManager.AppSettings["AlertsPath"];
        static string ETFSymbolsDataPath = ConfigurationManager.AppSettings["ETFSymbolsDataPath"];
        static string BuySellRatingFTPFilesPath = ConfigurationManager.AppSettings["BuySellRatingFTPFilesPath"];
        //static string CTRatingHistoryFolderPath = ConfigurationManager.AppSettings["CTRatingHistoryFilePath"];
        //static string CTRatingPerfFolderPath = ConfigurationManager.AppSettings["CTRatingPerfFilePath"];

        static void Main(string[] args)
        {

            DOMConfigurator.Configure();


            logTime.Info("Starting Programme ");

            try
            {

                #region DirectoryExists

                if (!Directory.Exists(OBOSRatingPath))
                {
                    Directory.CreateDirectory(OBOSRatingPath);
                }
                if (!Directory.Exists(HistoricalDataFilePath))
                {
                    Directory.CreateDirectory(HistoricalDataFilePath);

                }
                if (!Directory.Exists(NewSymbolsPath))
                {
                    Directory.CreateDirectory(NewSymbolsPath);
                }
                if (!Directory.Exists(SymbolAnalyticsPath))
                {
                    Directory.CreateDirectory(SymbolAnalyticsPath);
                }

                if (!Directory.Exists(PatternsCsvFilePath))
                {
                    Directory.CreateDirectory(PatternsCsvFilePath);
                }

                if (!Directory.Exists(UpdatedTrendCsvFilePath))
                {
                    Directory.CreateDirectory(UpdatedTrendCsvFilePath);
                }

                if (!Directory.Exists(BuySellRatingCsvFilePath))
                {
                    Directory.CreateDirectory(BuySellRatingCsvFilePath);
                }
                if (!Directory.Exists(ERRORSymbolsPath))
                {
                    Directory.CreateDirectory(ERRORSymbolsPath);
                }
                if (!Directory.Exists(DailyAvgVolumeFolderPath))
                {
                    Directory.CreateDirectory(DailyAvgVolumeFolderPath);
                }
                if (!Directory.Exists(BuySellRatingChangeHistCsvFilePath))
                {
                    Directory.CreateDirectory(BuySellRatingChangeHistCsvFilePath);
                }

                if (!Directory.Exists(HistoricalVolumeFolderPath))
                {
                    Directory.CreateDirectory(HistoricalVolumeFolderPath);
                }
                if (!Directory.Exists(HistoricalVolumeAlertPerfFolderPath))
                {
                    Directory.CreateDirectory(HistoricalVolumeAlertPerfFolderPath);
                }
                if (!Directory.Exists(SentimentIndicatorPerfFolderPath))
                {
                    Directory.CreateDirectory(SentimentIndicatorPerfFolderPath);
                }
                if (!Directory.Exists(GroupPerformanceFolderPath))
                {
                    Directory.CreateDirectory(GroupPerformanceFolderPath);
                }
                if (!Directory.Exists(StrongWeakSymbolsFolderPath))
                {
                    Directory.CreateDirectory(StrongWeakSymbolsFolderPath);
                }
                if (!Directory.Exists(LongShortPerfFolderPath))
                {
                    Directory.CreateDirectory(LongShortPerfFolderPath);
                }
                if (!Directory.Exists(SectorPerfHistFolderPath))
                {
                    Directory.CreateDirectory(SectorPerfHistFolderPath);
                }
                if (!Directory.Exists(SectorPerfDailyFolderPath))
                {
                    Directory.CreateDirectory(SectorPerfDailyFolderPath);
                }
                if (!Directory.Exists(SymbolRuleCsvFilePath))
                {
                    Directory.CreateDirectory(SymbolRuleCsvFilePath);
                }
                if (!Directory.Exists(SnPSpecificDatePricesPath))
                {
                    Directory.CreateDirectory(SnPSpecificDatePricesPath);
                }
                if (!Directory.Exists(StatisticsPath))
                {
                    Directory.CreateDirectory(StatisticsPath);
                }
                if (!Directory.Exists(CTRatingChangeHistFolderPath))
                {
                    Directory.CreateDirectory(CTRatingChangeHistFolderPath);
                }
                if (!Directory.Exists(AlertsPath))
                {
                    Directory.CreateDirectory(AlertsPath);
                }

                //if (!Directory.Exists(CTRatingHistoryFolderPath))
                //{
                //    Directory.CreateDirectory(CTRatingHistoryFolderPath);
                //}
                //if (!Directory.Exists(CTRatingPerfFolderPath))
                //{
                //    Directory.CreateDirectory(CTRatingPerfFolderPath);
                //}

                #endregion

                switch (args[0])
                {
                    case "S":

                        #region Symbol Analytics

                        log.Info("Symbol Analytics: Starting Symbol Analytics Programme at: " + DateTime.Now);

                        DateTime fromDate = DateTime.Now.AddDays(-400);
                        DateTime toDate = DateTime.Now;
                        SymbolAnalyticsCalculation.SymbolAnalyticsPath = SymbolAnalyticsPath;
                        SymbolAnalyticsCalculation.patternsFilePath = patternsFilePath;
                        SymbolAnalyticsCalculation.PatternsCsvFilePath = PatternsCsvFilePath;
                        if (args.Length > 1)
                        {
                            int from = int.Parse(args[1]);
                            int to = int.Parse(args[2]);

                            SymbolAnalyticsCalculation.CalculateSymbolAnalytics(from, to, fromDate, toDate, false);
                        }
                        else
                        {
                            SymbolAnalyticsCalculation.CalculateSymbolAnalytics(fromDate, toDate, true);
                        }
                        log.Info("Symbol Analytics: calculation done. " + DateTime.Now);
                        #endregion
                        break;
                    case "Save":

                        #region Save Symbol Analytics
                        log.Info("Symbol Analytics: Starting Save Symbol Analytics Programme at: " + DateTime.Now);

                        SymbolAnalyticsCalculation.SymbolAnalyticsPath = SymbolAnalyticsPath;
                        SymbolAnalyticsCalculation.PatternsCsvFilePath = PatternsCsvFilePath;
                        SymbolAnalyticsCalculation.SaveSymbolAnalytics();
                        log.Info("Symbol Analytics: Save Symbol Analytics done. " + DateTime.Now);
                        break;

                        #endregion
                    case "Daily":

                        #region Daily Data Import
                        logTime.Info("Process: Starting Daily Data Import Programme at: " + DateTime.Now);
                        toDate = DateTime.Now.Date;
                        fromDate = DateTime.Now.Date;
                        int daysBehind = 0;

                        if (args.Length == 2)
                        {
                            daysBehind = int.Parse(args[1]);
                            fromDate = DateTime.Now.Date.AddDays(-daysBehind);
                        }
                        if (args.Length > 2)
                        {
                            logTime.Info("Process: Daily Data Import for Daily Error Symbol Data ");
                            daysBehind = int.Parse(args[1]);
                            fromDate = DateTime.Now.Date.AddDays(-daysBehind);

                            string filename = ERRORSymbolsPath + "/" + args[2];
                            HistoricalDataImporter.HistoricalDataFilePath = HistoricalDataFilePath;
                            HistoricalDataImporter.ERRORSymbolsPath = ERRORSymbolsPath;
                            HistoricalDataImporter.DailyErrorSymbolData(fromDate, toDate, filename, false);
                        }
                        else
                        {

                            HistoricalDataImporter.HistoricalDataFilePath = HistoricalDataFilePath;
                            HistoricalDataImporter.ERRORSymbolsPath = ERRORSymbolsPath;

                            HistoricalDataImporter.SaveDailyData(fromDate, toDate, true);
                            HistoricalDataImporter.SaveDailyData(fromDate, toDate, false);
                        }
                        logTime.Info("Process: Done! Daily Data Import Programme at: " + DateTime.Now);
                        break;

                        #endregion

                    case "NewSymbol":

                        #region Newly Added Symbols Data Import

                        log.Info("Process: Starting Newly Added Symbols Data Import Programme at: " + DateTime.Now);

                        toDate = DateTime.Now.Date;
                        fromDate = DateTime.Now.AddYears(-Constants.HIST_DATA_LENGTH).Date;

                        HistoricalDataImporter.NewSymbolsPath = NewSymbolsPath;

                        HistoricalDataImporter.SaveHistData(fromDate, toDate, false, true);

                        SymbolHistoricalDAO.ChangeIsnewSymbols();
                        log.Info("Process: Done! Newly Added Symbols Data Import Programme at: " + DateTime.Now);
                        break;
                        #endregion
                    case "Hist":

                        #region Historical Data Import

                        logTime.Info("Process: Starting Historical Data Import Programme at: " + DateTime.Now);
                        toDate = DateTime.Now.Date;

                        fromDate = DateTime.Now.AddYears(-Constants.HIST_DATA_LENGTH).Date;
                        HistoricalDataImporter.HistoricalDataFilePath = HistoricalDataFilePath;
                        HistoricalDataImporter.ERRORSymbolsPath = ERRORSymbolsPath;
                        if (args.Length > 1)
                        {
                            switch (args[1])
                            {
                                case "D":   //specific Date
                                    
                                    toDate = DateTime.Now.Date;
                                    fromDate = DateTime.Now.Date;
                                    if (args.Length > 2)
                                    {
                                        DateTime date = DateTime.Parse(args[2]);
                                        fromDate =toDate = date;
                                        logTime.Info("Process: Starting  Data Import for specific date " + date);
                                    }
                                    HistoricalDataImporter.HistoricalDataFilePath = HistoricalDataFilePath;
                                    HistoricalDataImporter.ERRORSymbolsPath = ERRORSymbolsPath;

                                    HistoricalDataImporter.SaveDailyData(fromDate, toDate, true);
                                    HistoricalDataImporter.SaveDailyData(fromDate, toDate, false);

                                    break;

                                case "S":   //specific Symbol

                                    if (args.Length > 3)
                                    {
                                        int customHistDataLength = Convert.ToInt32(args[3]);
                                        int year = DateTime.Now.AddYears(-customHistDataLength).Date.Year;
                                        fromDate = new DateTime(year, 1, 1);
                                    }
                                    string symbol = args[2];
                                    logTime.Info("Process: Starting Historical Data Import for specific symbol " + symbol);
                                    SymbolHistoricalDAO.DeleteData(symbol);
                                    HistoricalDataImporter.SaveHistDataSymbol(fromDate, toDate, symbol, false, true);
                                    logTime.Info("Process: Done! Historical Data Import for specific symbol " + symbol);
                                    break;

                                case "E":
                                    logTime.Info("Process: Starting Historical Data Import Daily Error Symbol Data ");
                                    string filename = ERRORSymbolsPath + "/" + args[2];
                                    HistoricalDataImporter.HistoricalDataFilePath = HistoricalDataFilePath;
                                    HistoricalDataImporter.DailyErrorSymbolData(fromDate, toDate, filename, true);
                                    logTime.Info("Process: Done! Historical Data Import Daily Error Symbol Data ");
                                    break;

                                case "M":
                                    logTime.Info("Process: Starting Historical Data Import for mutual funds ");
                                    HistoricalDataImporter.SaveHistData(fromDate, toDate, true, false);
                                    logTime.Info("Process: Done! Historical Data Import for mutual funds ");
                                    break;
                                case "A":
                                    logTime.Info("Process: Starting Historical Data Import for All Symbols ");
                                    SymbolHistoricalDAO.DeleteData();
                                    HistoricalDataImporter.SaveHistData(fromDate, toDate, true, false);
                                    HistoricalDataImporter.SaveHistData(fromDate, toDate, false, false);
                                    logTime.Info("Process: Done! Historical Data Import for All Symbols ");
                                    break;

                                default:
                                    break;
                            }
                        }


                        #endregion
                        break;

                    case "B":

                        #region Buy Sell Rating
                        logTime.Info("Rating: Starting Buy Sell Rating Programme at: " + DateTime.Now);

                        BuySellRatingCalculation.BuySellRatingFilePath = BuySellRatingFilePath;
                        BuySellRatingCalculation.BuySellRatingCsvFilePath = BuySellRatingCsvFilePath;

                        BuySellRatingCalculation.calculateBuySellRating();
                        logTime.Info("Rating: updating ETF Ratings from HistBuySellRating Tbl" + DateTime.Now);
                        BuySellRatingCalculation.updateETFRatings();
                        BuySellRatingCalculation.backupBSRatingFiles(BuySellRatingFTPFilesPath);
                        logTime.Info("Rating: backup BS Rating Files Programme at: " + DateTime.Now);
                        logTime.Info("Rating: Done! Buy Sell Rating Programme at: " + DateTime.Now);
                        break;

                        #endregion

                    case "R":

                        #region OBOS
                        logTime.Info("Process: Starting OBOS Programme at: " + DateTime.Now);

                        OBOSRatingCalculation.OBOSRatingPath = OBOSRatingPath;
                        if (args.Length > 1 && args[1].Equals("H"))
                        {
                            logTime.Info("Process: Calculate OBOS Rating in history mode");
                            OBOSRatingCalculation.CalculateOBOSRating(true);

                        }
                        else
                        {
                            OBOSRatingCalculation.CalculateOBOSRating(false);
                        }
                        logTime.Info("Process: Done! OBOS Programme at: " + DateTime.Now);
                        #endregion
                        break;
                    case "MI":

                        #region Calculate Market Internals
                        logTime.Info("Process: Starting Calculate Market Internals Programme at: " + DateTime.Now);

                        try
                        {
                            MarketInternalsCalculation.CalculateMarketInternals();
                        }
                        catch (Exception ex)
                        {
                            log.Error("Error:"+ex);
                        }
                        logTime.Info("Process: Done! Calculate Market Internals Programme at: " + DateTime.Now);
                        #endregion
                        break;

                    case "CH":

                        #region Buy Sell Change History

                        logTime.Info("Rating: Starting Buy Sell Change History Programme at: " + DateTime.Now);
                        BuySellRatingCalculation.BuySellRatingChangeHistCsvFilePath = BuySellRatingChangeHistCsvFilePath;

                        BuySellRatingCalculation.BuySellChangeHistory();
                        logTime.Info("Rating: Done! Buy Sell Change History Programme at: " + DateTime.Now);
                        #endregion
                        break;

                    case "VD":
                        #region  Daily Average Volume
                        logTime.Info("Process: Starting Daily Average Volume Programme at: " + DateTime.Now);
                        VolumeAlerts.CalculateDailyAverageVolume(DailyAvgVolumeFolderPath);
                        logTime.Info("Process: Dane! Daily Average Volume Programme at: " + DateTime.Now);
                        #endregion
                        break;

                    case "VDA":
                        #region  Daily Volume Alert

                        logTime.Info("Process: Starting Daily Volume Alert Programme at: " + DateTime.Now);
                        VolumeAlerts.InsertVolumeAlertDaily();
                        logTime.Info("Process: Done! Daily Volume Alert Programme at: " + DateTime.Now);
                        #endregion
                        break;

                    case "VH":
                        #region Historical Volume Alert

                        logTime.Info("Process: Starting Volume Alert Historical Programme at: " + DateTime.Now);

                        VolumeAlerts.CalculateAndSaveHistoricalAlerts(HistoricalVolumeFolderPath, HistoricalVolumeAlertPerfFolderPath);
                        logTime.Info("Process: Done! Volume Alert Historical Programme at: " + DateTime.Now);
                        #endregion
                        break;

                    case "CHD":
                        #region Change History Dates

                        logTime.Info("Process: Starting Change Historical Dates Programme at: " + DateTime.Now);
                        UpdateHistoryDates.ChangeHistoryDateInDB();
                        logTime.Info("Process: Done! Change Historical Dates Programme at: " + DateTime.Now);
                        #endregion
                        break;

                    case "GP":
                        #region Group Performance

                        logTime.Info("Process:  Starting Group Performance Programme at: " + DateTime.Now);
                        GroupPerformance.CalculateAndSaveGroupPerformance(GroupPerformanceFolderPath);
                        logTime.Info("Process:  done! Group Performance Programme at: " + DateTime.Now);
                        #endregion
                        break;

                    case "SW":
                        #region Sector Wise Top Bottom Symbols

                        logTime.Info("Process: Starting calculation for Sector Wise Top Bottom Symbols Programme at: " + DateTime.Now);
                        SectorWiseTopBottomSymbols.CalculateTopBottomSymbols(StrongWeakSymbolsFolderPath);
                        logTime.Info("Process: Done!  Sector Wise Top Bottom Symbols Programme at: " + DateTime.Now);
                        #endregion
                        break;

                    case "LSA":
                        #region Long Short Alerts

                        logTime.Info("Process: Starting  Long Short Alerts Programme at: " + DateTime.Now);
                        LongShortAlerts.CalculateLongShortAlerts();
                        logTime.Info("Process: Done!  Long Short Alerts Programme at: " + DateTime.Now);
                        #endregion
                        break;

                    case "ADP":
                        #region Aggresive Defensive Performance

                        logTime.Info("Process: Starting Aggresive Defensive Performance Programme at: " + DateTime.Now);
                        AggresiveDefensivePerformance.UpdateADPerformance();
                        logTime.Info("Process: Done Aggresive Defensive Performance Programme at: " + DateTime.Now);
                        #endregion
                        break;

                    case "RC":
                        #region Restart MemCache

                        logTime.Info("Process: Restarting MemCache Programme at: " + DateTime.Now);
                        RestartCache.RestartingMemCache();
                        logTime.Info("Process: done Restarting MemCache Programme at: " + DateTime.Now);
                        #endregion
                        break;

                    case "SP":
                        #region Sector Performance Daily Stocks

                        logTime.Info("Process: Starting Sector Daily Stocks Programme at: " + DateTime.Now);
                        SectorPerformance.CalculateSectorDailyStocks();
                        logTime.Info("Process: Done Sector Daily Stocks Programme at: " + DateTime.Now);
                        #endregion

                        break;


                    case "SPH":
                        #region Sector Performance Historical
                        logTime.Info("Process: Starting Sector Historical Performance Programme at: " + DateTime.Now);

                        SectorPerformance.SectorPerfHistFolderPath = SectorPerfHistFolderPath;
                        SectorPerformance.CalculateSectorPerfHistory(true);
                        logTime.Info("Process: done, Sector Historical Performance Programme at: " + DateTime.Now);
                        #endregion

                        break;

                    case "SPD":
                        #region Sector Performance Daily
                        logTime.Info("Process: Starting Sector Daily Performance Programme at: " + DateTime.Now);

                        SectorPerformance.SectorPerfDailyFolderPath = SectorPerfDailyFolderPath;
                        SectorPerformance.CalculateSectorPerfHistory(false);
                        logTime.Info("Process: Done, Sector Daily Performance Programme at: " + DateTime.Now);
                        #endregion

                        break;

                    case "SymbolPerf":
                        #region Symbol Performance
                        logTime.Info("Process: Starting Symbol Daily Performance Programme at: " + DateTime.Now);

                        SymbolPerformance.CalculateSymbolPerf();
                        logTime.Info("Process: done! Symbol Daily Performance Programme at: " + DateTime.Now);
                        #endregion
                        break;

                    case "Rules":

                        #region Symbol Rules Calculation
                        logTime.Info("Process: Starting Symbol Rules Calculation Programme at: " + DateTime.Now);

                        RulesCalculation.SymbolRuleCsvFilePath = SymbolRuleCsvFilePath;
                        RulesCalculation.CalculateSymbolRules();
                        logTime.Info("Process: Done Symbol Rules Calculation Programme at: " + DateTime.Now);
                        break;

                        #endregion


                    case "SnPPrice":

                        #region SnP Watchlist Create Date Price
                        logTime.Info("Process:  Starting SnP Watchlist Create Date Price  Programme at: " + DateTime.Now);
                        SnPPriceCalculation.SnPSpecificDatePricesPath = SnPSpecificDatePricesPath;
                        SnPPriceCalculation.CalculateSnPPrice();
                        logTime.Info("Process:  Done! SnP Watchlist Create Date Price  Programme at: " + DateTime.Now);
                        break;

                        #endregion


                    case "CTCH":

                        #region CT Rating CHange History
                        logTime.Info("Rating:  Starting CT Rating CHange History Programme at: " + DateTime.Now);
                        CTRatingCalculation.CTRatingChangeHistCsvFilePath = CTRatingChangeHistFolderPath;
                        CTRatingCalculation.CTRatingChangeHistory();
                        logTime.Info("Rating:  Done! CT Rating CHange History Programme at: " + DateTime.Now);
                        break;

                        #endregion
                    case "AC":

                        #region Alerts calculation
                        logTime.Info("Process:  Starting Email Alerts calculation Programme at: " + DateTime.Now);
                        EmailAlertsCalculation.CalculateMyAlerts();
                        EmailAlertsCalculation.calculateCommonSubsAlerts();
                        logTime.Info("EmailAlert:  done Email Alerts calculation Programme at: " + DateTime.Now);

                        break;

                        #endregion
                    case "EA":

                        #region Email Alerts sending
                        logTime.Info("EmailAlert:  Starting Email Alerts sending Programme at: " + DateTime.Now);
                        //DayOfWeek dayOfWeek = DateTime.Now.DayOfWeek;

                        EmailAlertsCalculation.SendAlertsEmailtoUsers();
                        logTime.Info("EmailAlert:  Done Email Alerts sending Programme at: " + DateTime.Now);
                        break;

                        #endregion
                    case "Stats":

                        #region Calculate Statistics
                        logTime.Info("Process:  Starting Calculate Statistics Programme at: " + DateTime.Now);

                        List<StatisticsPerf> StatsPerfData = StatisticsCalculation.calculateStatistics();
                        CSVExporter.WriteToCSVStatsPerfData(StatsPerfData, StatisticsPath + "/StatisticsData.csv");
                        StatisticsDAO.InsertStatsDataCSVToDB(StatisticsPath);
                        StatisticsCalculation.calculateStatisticsPerf();
                        logTime.Info("Process:  Done. Calculate Statistics Programme at: " + DateTime.Now);
                        break;

                        #endregion
                    case "ETF":

                        #region ETF data import
                        logTime.Info("Process:  Starting calculate and import ETF market data in DB Programme at: " + DateTime.Now);
                        ETFSymbolsDataCalculation.ETFDataFilesPath = ETFSymbolsDataPath;
                        ETFSymbolsDataCalculation.CalculateFTFSymbolsData();
                        break;

                        #endregion

                    case "TRH":

                       
                        #region Top Rating symbols Hist
                        
                        logTime.Info("Rating:  Top Rating symbols Hist: " + DateTime.Now);
                        BuySellRatingCalculation.SaveTopRatingSymbolsHist();
                        BuySellRatingCalculation.SaveTopRatingAddRemoveSymbolsHist();
                       
                        break;

                        #endregion

                    case "V":

                        #region Validation check for data
                        logTime.Info("Process: starting Validation check for data: " + DateTime.Now);
                        ValidationDataHelper.ValidateData();

                        break;

                        #endregion

                    case "MailTest":

                        #region MailTest
                         String From = ConfigurationManager.AppSettings["AdminEmail"];
                         MailUtility.SendMail("testing", "testing", From, "om.omshiv@gmail.com");

                        break;

                        #endregion


                    case "MPA":

                        #region Model portfolio Signal Alert
                        
                        break;

                        #endregion

                    case "WR":

                        #region calculate Returns for weekly report
                        logTime.Info("Process:  calculate Returns for weekly report " + DateTime.Now);
                        WeeklyReturnCaculation.calculateWeeklyReturns();

                        break;

                        #endregion

                    case "U":

                        #region unsubscribe Free trial user with promocode 
                        logTime.Info("Process:  update Free Trial Users Programme at: " + DateTime.Now);
                        UsersManager.ManageFreeTrialUsers();

                        break;

                        #endregion

                    case "Synopsis":

                        #region calculating synopsys id for chart page
                        logTime.Info("Process:  calculating synopsys id for chart page Programme at: " + DateTime.Now);
                        SnPSymbolsCalculations.snpDatafilesPath= SnPSpecificDatePricesPath;
                        SnPSymbolsCalculations.calculateSynosisRuleID();

                        break;

                        #endregion

                    //case "ETFRating":

                    //    #region ETFRating calculations
                    //    logTime.Info("Starting calculate ETF Rating change hist Programme at: " + DateTime.Now);
                    //    ETFSymbolsDataCalculation.CTRatingChangeHistory();
                    //    ETFSymbolsDataCalculation.BuySellRatingChangeHistory();
                    //    break;

                    //    #endregion
                    default:

                        break;


                }
                log.Info("Ending Programme at: " + DateTime.Now);
                #region Commented
                /*
                  if (args.Length !=0 && args[0].Equals("Ssssss"))
                {
                    #region symbolAnalytics OLD
                    //{
                    //    logTime.Info("Starting Symbol Analytics Programme at: " + DateTime.Now);
                    //    List<String> symbolList = null;
                    //    DateTime fromdate = DateTime.Now.AddDays(-200);
                    //    DateTime toDate = DateTime.Now;
                    //    if (args.Length > 1)
                    //    {

                    //        int from = int.Parse(args[1]);
                    //        int to = int.Parse(args[2]);
                    //        List<InputBarData> listInputDataForSymbols = new List<InputBarData>();
                    //        symbolList = SymbolHistoricalDAO.GetsymbolListFromDB(from, to);
                    //        foreach (String symbol in symbolList)
                    //        {
                    //            List<BarData> barlist = null;
                    //            log.Info("Getting Data For symbol: " + symbol);

                    //            try
                    //            {
                    //                barlist = HistoricalDataFeedFromDbDAO.GetDataFromFeedFromDB(fromdate, toDate, symbol);
                    //            }
                    //            catch (Exception ex)
                    //            {

                    //                log.Error("Error:" + ex);
                    //            }
                    //            if (barlist == null || barlist.Count == 0)
                    //            {

                    //                log.Info("Empty List Returned From Provider" + symbol);
                    //            }
                    //            else
                    //            {
                    //                InputBarData inputListRow = new InputBarData();
                    //                inputListRow.barListRow = barlist;
                    //                inputListRow.symbol = symbol;
                    //                listInputDataForSymbols.Add(inputListRow);

                    //            }

                    //        }
                    //        log.Info("Calculaitng SymbolAnalytics for symbols....");
                    //        List<SymbolAnalytics> symbolAnalyticsList = CalculateSymbolAnalytics(listInputDataForSymbols);
                    //        CSVExporter.WriteToCSV(symbolAnalyticsList, SymbolAnalyticsPath + "/SymbolAnalytics_" + from + "_" + to + ".csv");
                    //        log.Info("Calculaitng Patterns for symbols....");
                    //        List<PatternBarData> PatternMixData = calculatePatterns(listInputDataForSymbols);
                    //        DirectoryInfo di = new DirectoryInfo(patternsFilePath);//AppDomain.CurrentDomain.BaseDirectory);
                    //        try
                    //        {
                    //            foreach (FileInfo file in di.GetFiles("*.xml"))
                    //            {

                    //                String PatternType = (file.Name).Replace(".apr.xml", "");
                    //                List<PatternBarData> PatternSeperateData = PatternMixData.Where(x => x.Pattern.Equals(PatternType, StringComparison.OrdinalIgnoreCase)).ToList();
                    //                CSVExporter.WriteToCSVPattern(PatternSeperateData, PatternsCsvFilePath + "/Pattern_" + PatternType + "_" + from + "_" + to + ".csv");
                    //            }
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            log.Error("Error:" + ex);
                    //        }
                    //    }
                    //    else
                    //    {

                    //        symbolList = SymbolHistoricalDAO.GetIndexMFFromDB();
                    //        List<InputBarData> listInputDataForIndicies = new List<InputBarData>();
                    //        foreach (String symbol in symbolList)
                    //        {
                    //            List<BarData> barlist = null;
                    //            log.Info("Getting Data For index: " + symbol);

                    //            try
                    //            {
                    //                barlist = HistoricalDataFeedFromDbDAO.GetDataFromFeedFromDB(fromdate, toDate, symbol);


                    //            }
                    //            catch (Exception ex)
                    //            {

                    //                log.Error("Error:" + ex);
                    //            }
                    //            if (barlist == null || barlist.Count == 0)
                    //            {

                    //                log.Info("No data for index: " + symbol);
                    //            }
                    //            else
                    //            {
                    //                InputBarData inputListRow = new InputBarData();
                    //                inputListRow.barListRow = barlist;
                    //                inputListRow.symbol = symbol;
                    //                listInputDataForIndicies.Add(inputListRow);

                    //            }

                    //        }
                    //        log.Info("Calculaitng SymbolAnalytics For Indicies....");
                    //        List<SymbolAnalytics> symbolAnalyticsList = CalculateSymbolAnalytics(listInputDataForIndicies);
                    //        CSVExporter.WriteToCSV(symbolAnalyticsList, SymbolAnalyticsPath + "/SymbolAnalytics_Index_MF.csv");


                    //        log.Info("Calculaitng Patterns for Indicies....");
                    //        List<PatternBarData> PatternMixData = calculatePatterns(listInputDataForIndicies);
                    //        DirectoryInfo di = new DirectoryInfo(patternsFilePath);//AppDomain.CurrentDomain.BaseDirectory);
                    //        foreach (FileInfo file in di.GetFiles("*.xml"))
                    //        {

                    //            String PatternType = (file.Name).Replace(".apr.xml", "");
                    //            List<PatternBarData> PatternSeperateData = PatternMixData.Where(x => x.Pattern.Equals(PatternType, StringComparison.OrdinalIgnoreCase)).ToList();
                    //            CSVExporter.WriteToCSVPattern(PatternSeperateData, PatternsCsvFilePath + "/Pattern_MF" + ".csv");
                    //        }



                    //    }

                    //    // CSVToDB.SymbolAnalytics();
                    //}
                    #endregion
                }
                else if (args.Length != 0 && args[0].Equals("Bbbbbb"))
                {
                    #region BuysellRating OLD
                    //List<Rating> ratingList = calculateBuySellRating();
                    //CSVExporter.WriteToCSVRating(ratingList, BuySellRatingCsvFilePath + "/RatingFile.csv");
                    //CSVToDB.InsertRating(BuySellRatingCsvFilePath);
                    #endregion
                }


                    // Calculate Trends and save to DB
                else if (args.Length != 0 && args[0].Equals("T"))
                {
                    logTime.Info("Starting Trend Programme at: " + DateTime.Now);
                    List<String> symbolList = DataDownloader.GetAllsymbolListFromDB();
                   
                    List<TrendObjects> TrendObjectsList = calculateTrend(symbolList);
                    CSVExporter.WriteToCSVTrend(TrendObjectsList, UpdatedTrendCsvFilePath + "/UpdatedTrend.csv");
                    CSVToDB.UpdatedTrends(UpdatedTrendCsvFilePath, true, "UpdatedTrend.csv");
                }
                else if (args.Length != 0 && args[0].Equals("TM"))
                {
                    logTime.Info("Starting Trend For MF Programme at: " + DateTime.Now);
                    List<String> symbolList = SymbolHistoricalDAO.GetIndexMFFromDB();
                    
                    List<TrendObjects> TrendObjectsList = calculateTrend(symbolList);
                    CSVExporter.WriteToCSVTrend(TrendObjectsList, UpdatedTrendCsvFilePath + "/UpdatedTrendMF.csv");
                    CSVToDB.UpdatedTrends(UpdatedTrendCsvFilePath, true, "UpdatedTrendMF.csv");
                }
                else if (args.Length != 0 && args[0].Equals("TN")) // newly added symbols Trends
                {
                    logTime.Info("Starting Newly Added Symbol Programme at: " + DateTime.Now);
                    getHistoryDataForNewSymbolAndCalculateAnalytics();
                }
                else if (args.Length != 0 && args[0].Equals("CHhhhh")) // BuySellRating change history
                {
                    #region BuySellRating change history OLD
                  //  logTime.Info("Starting Buy Sell Change History Programme at: " + DateTime.Now);
                  //List<BuySellRating> historyBuySellRatingList=  DataDownloader.getBuySellRatingHistroyFromDB();
                  //List<BuySellRatingChangeHist> ChangeBuySellRatingHist = new List<BuySellRatingChangeHist>();
                  //for (int i=1; i<historyBuySellRatingList.Count;i++)
                  //{
                  //    int prevRating = historyBuySellRatingList[i-1].rating;
                  //    int currentRating=historyBuySellRatingList[i].rating;
                  //    string prevSymbol = historyBuySellRatingList[i-1].symbol;
                  //    string currentSymbol=historyBuySellRatingList[i].symbol;
                  //    if (currentSymbol.Equals(prevSymbol))
                  //    {
                  //        if ((prevRating - currentRating) != 0)
                  //        {
                  //            ChangeBuySellRatingHist.Add(
                  //                new BuySellRatingChangeHist
                  //                {
                  //                    symbol = historyBuySellRatingList[i].symbol,
                  //                    newRating = currentRating,
                  //                    oldRating = prevRating,
                  //                    ratingDate = historyBuySellRatingList[i].ratingDate


                  //                });
                  //        }
                  //    }


                  //}
                  //CSVExporter.WriteToCSVChangeRatingHistory(ChangeBuySellRatingHist, BuySellRatingChangeHistCsvFilePath + "/ChangeRatingHistoryFile.csv");
                  //CSVToDB.InsertChangeRatingHistoryDB(BuySellRatingChangeHistCsvFilePath);

                    #endregion

                }
                // calculate s & P analytics
                else if (args.Length != 0 && args[0].Equals("Rrrrrrrr"))
                {
                    #region OBOS old
                    //logTime.Info("Starting OBOS Programme at: " + DateTime.Now);
                    //if (args.Length > 1 && args[1].Equals("H"))
                    //{
                    //    log.Info("Calculaitng OBOSCount for history....");
                    //    bool historical = true;
                    //    List<DateOBOSCount> listObOsCount = calculateOBOS(historical);
                    //    CSVExporter.WriteToCSVOBOS(listObOsCount, OBOSRatingPath + "/OBOSCount.csv");
                    //    CSVToDB.OBOSPercentage(OBOSRatingPath, true);
                    //}
                    //else
                    //{
                    //    log.Info("Calculaitng OBOSCount for current date....");
                    //    bool historical = false;
                    //    List<DateOBOSCount> listObOsCount = calculateOBOS(historical);
                    //    CSVExporter.WriteToCSVOBOS(listObOsCount, OBOSRatingPath + "/OBOSCount.csv");
                    //    CSVToDB.OBOSPercentage(OBOSRatingPath, false);

                    //}
                    //try
                    //{
                    //    CalculateMarketInternals();
                    //}
                    //catch (Exception ex)
                    //{
                    //    log.Warn(ex);
                    //}
                    #endregion
                }
                // DataBase saving of created symbol analytics files
                else if (args.Length != 0 && args[0].Equals("Ddddd"))
                {
                    #region symbolAnalytics Save OLD
                    //logTime.Info("Starting Save Symbol Analytics Programme at: " + DateTime.Now);
                    //#region   update symbol analytics in db
                    //DirectoryInfo di = new DirectoryInfo(SymbolAnalyticsPath);
                    //FileInfo[] fileEntries = di.GetFiles("*.csv");
                    //int count = 0;
                    //foreach (FileInfo fileName in fileEntries)
                    //{
                    //    if (count == 0)
                    //    {
                    //        CSVToDB.SymbolAnalytics(SymbolAnalyticsPath, fileName.Name, true);
                    //    }
                    //    else
                    //    {
                    //        CSVToDB.SymbolAnalytics(SymbolAnalyticsPath, fileName.Name, false);
                    //    }
                    //    count++;
                    //}
                    //#endregion

                    //#region update patterns in db

                    //DirectoryInfo diPatterns = new DirectoryInfo(PatternsCsvFilePath);
                    //FileInfo[] fileEntriesPatterns = diPatterns.GetFiles("*.csv");
                    //CSVToDB.DeletePatterns();
                    //foreach (FileInfo fileName in fileEntriesPatterns)
                    //{
                    //    CSVToDB.UpdatedPatterns(PatternsCsvFilePath, fileName.Name);

                    //}

                    //CSVToDB.UpdatedAlertsInSymbolAnalytics();

                    //#endregion
                    #endregion
                }
                else if (args.Length != 0 && args[0].Equals("I"))
                {
                  CSVToDB.IndusrtySymbolFileToDb(IndustrySymbolFilePath);
                }

                else if (args.Length != 0 && args[0].Equals("U"))
                {
                    CSVToDB.ImportUsersToDB(UsersInfoFilePath);
                }
                else if (args.Length != 0 && args[0].Equals("Hhhh"))
                {
                        #region DataImport Save OLD

                //    logTime.Info("Starting Data Import Programme at: " + DateTime.Now);

                //    int from = 0;// int.Parse(args[1]);
                //    int to = 0;// int.Parse(args[2]);
                //    bool forMF = false;
                //    bool isHistorical = true;
                //    bool forErrorSymbols = false;
                //    DateTime toDate;
                //    DateTime fromDate;
                //    List<String> symbolList=null;
                //    bool specificSymbols = false;
                //   if(args.Length==7 && args[1]=="D")
                //   {
                //       from = int.Parse(args[4]);
                //       to = int.Parse(args[5]);
                //       fromDate = DateTime.Parse(args[2]);
                //       toDate = DateTime.Parse(args[3]);
                //       HistoricalDataFilePath = HistoricalDataFilePath + "/HistoricalData-" + from + "-" + to + ".csv";

                //       symbolList = SymbolHistoricalDAO.GetsymbolListFromDB(from, to);
                //       isHistorical = Boolean.Parse(args[6]);

                //   }
                //    else
                // {
                //    if (args.Length == 4 && args[1]=="S")
                //    {
                //        specificSymbols = true;
                //    }
                //    else
                //    {
                //         from =  int.Parse(args[1]);
                //         to = int.Parse(args[2]);
                //    }
                //     if (args.Length>4)
                //    {
                //         toDate = DateTime.Now.Date;
                //         fromDate = DateTime.Now.Date;
                //         isHistorical = false;
                //    }
                //    else
                //    {
                //         toDate = DateTime.Now.Date;
                //         fromDate = DateTime.Now.AddYears(-4).Date;
                //    }
                   
                    
                   
                   
                //    if (args[3] == "M")
                //    {
                //        forMF = true;
                //    }
                //    else if (args[3] == "E")
                //    {
                //        forErrorSymbols = true;
                //    }
                //    if (specificSymbols)
                //    {
                //        HistoricalDataFilePath = HistoricalDataFilePath + "/Specific-Symbols.csv";
                //        symbolList = DataDownloader.GetsymbolListForNewlyAddedSymbols();
                //    }
                //    else
                //    {
                //        if (!forErrorSymbols)
                //        {
                //            if (forMF)
                //            {
                //                HistoricalDataFilePath = HistoricalDataFilePath + "/HistoricalData-MF.csv";
                //                symbolList = SymbolHistoricalDAO.GetIndexMFFromDB();
                //            }
                //            else
                //            {
                //                HistoricalDataFilePath = HistoricalDataFilePath + "/HistoricalData-" + from + "-" + to + ".csv";
                //                symbolList = SymbolHistoricalDAO.GetsymbolListFromDB(from, to);
                //            }
                //        }
                //        else
                //        {

                //            HistoricalDataFilePath = HistoricalDataFilePath + "/HistoricalData-ErrorSymbols.csv";
                //            symbolList = DataDownloader.GetErrorSymbols(ERRORSymbolsPath + "/Symbol-" + from + "-" + to + ".csv");
                //        }
                //    }
                //}
                //    SaveHistoryData(symbolList, HistoricalDataFilePath, specificSymbols, forMF, fromDate, toDate, isHistorical);
                                #endregion 
                }

                else if (args.Length != 0 && args[0].Equals("SIR"))
                {
                    logTime.Info("Starting Sentiment Indicator Rating Programme at: " + DateTime.Now);
                    SentimentIndicator.updateSentimentIndicatorRating();
                }

                else if (args.Length != 0 && args[0].Equals("VDddd"))
                {
                    logTime.Info("Starting Daily Average Volume Programme at: " + DateTime.Now);
                    VolumeAlerts.CalculateDailyAverageVolume(DailyAvgVolumeFolderPath);
                }
                else if (args.Length != 0 && args[0].Equals("VDAaaaa"))
                {
                    logTime.Info("Starting Daily Volume Alert Programme at: " + DateTime.Now);
                    VolumeAlerts.InsertVolumeAlertDaily();
                }

                else if (args.Length != 0 && args[0].Equals("VHhhh"))
                {
                    logTime.Info("Starting Volume Alert Historical Programme at: " + DateTime.Now);
                    VolumeAlerts.CalculateAndSaveHistoricalAlerts(HistoricalVolumeFolderPath, HistoricalVolumeAlertPerfFolderPath);

                }
                else if (args.Length != 0 && args[0].Equals("SP"))
                {
                    logTime.Info("Starting Sentiment Indicator Performance Programme at: " + DateTime.Now);
                    SentimentIndicator.CalculateAndSaveSentimentPerformance(SentimentIndicatorPerfFolderPath);


                }

                else if (args.Length != 0 && args[0].Equals("CHDdddd"))
                {
                    logTime.Info("Starting Change Historical Dates Programme at: " + DateTime.Now);
                    UpdateHistoryDates.ChangeHistoryDateInDB();

                }

                else if (args.Length != 0 && args[0].Equals("GPpppp"))
                {
                    logTime.Info("Starting Group Performance Programme at: " + DateTime.Now);
                    GroupPerformance.CalculateAndSaveGroupPerformance(GroupPerformanceFolderPath);

                }
                else if (args.Length != 0 && args[0].Equals("SWwwwww"))
                {
                    logTime.Info("Starting  Sector Wise Top Bottom Symbols Programme at: " + DateTime.Now);
                    SectorWiseTopBottomSymbols.CalculateTopBottomSymbols(StrongWeakSymbolsFolderPath);

                }
                else if (args.Length != 0 && args[0].Equals("LSAaaa"))
                {
                    logTime.Info("Starting  Long Short Alerts Programme at: " + DateTime.Now);
                    LongShortAlerts.CalculateLongShortAlerts();

                }
                
                else if (args.Length != 0 && args[0].Equals("ADPppp"))
                {
                    logTime.Info("Starting Aggresive Defensive Performance Programme at: " + DateTime.Now);
                    AggresiveDefensivePerformance.UpdateADPerformance();

                }
                else if (args.Length != 0 && args[0].Equals("CTRH"))
                {
                    logTime.Info("Starting CT Rating Historical Calculation Programme at: " + DateTime.Now);
                   // CTRatingCalculation.CalculateCTRatingHistoryorDaily(CTRatingHistoryFolderPath, true);

                }
                else if (args.Length != 0 && args[0].Equals("CTRD"))
                {
                    logTime.Info("Starting CT Rating Daily Calculation Programme at: " + DateTime.Now);
                  //  CTRatingCalculation.CalculateCTRatingHistoryorDaily(CTRatingHistoryFolderPath, false);

                }
                else if (args.Length != 0 && args[0].Equals("CTRP"))
                {
                    logTime.Info("Starting CT Rating Performance Calculation Programme at: " + DateTime.Now);
                  //  CTRatingCalculation.CalculateCTRatingPerf(CTRatingPerfFolderPath);

                }
                else if (args.Length != 0 && args[0].Equals("RC"))
                {
                    logTime.Info("Restarting MemCache Programme at: " + DateTime.Now);
                    RestartCache.RestartingMemCache();

                }
            }
             
            catch (Exception ex)
            {

                log.Error("Error: "+ex);
              
            }

            log.Info("Ending Programme at: " + DateTime.Now);
        }

        
        

         private static void SaveHistoryData(List<String> symbolList,String HistoricalSymbolDataFilePath,bool specificSymbols,bool forMF,DateTime fromDate,DateTime toDate,bool isHistorical)
         {
             try
             {
                 List<InputBarData> listInputDataForSymbols = new List<InputBarData>();
                 List<String> errorSymbols = new List<string>();
               
                 foreach (String symbol in symbolList)
                 {

                     List<BarData> barlist = null;
                     log.Info("Getting Data For symbol: " + symbol);
                     try
                     {
                         if (forMF || specificSymbols)
                         {
                             barlist = DataDownloader.GetDataFromFeedFromBATS(fromDate, toDate, symbol);

                         }
                         else
                         {
                             barlist = DataDownloader.GetDataFromFeedFromYahoo(fromDate, toDate, symbol);
                         }

                     }
                     catch (Exception ex)
                     {
                         errorSymbols.Add(symbol);
                         //log.Error(ex);
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
                 CSVExporter.WriteToCSV(listInputDataForSymbols, HistoricalSymbolDataFilePath);
                 if (specificSymbols)
                 {
                     CSVToDB.SaveHistoricalData(HistoricalSymbolDataFilePath, symbolList);
                 }
                 else
                 {

                    // CSVToDB.SaveHistoricalData(HistoricalSymbolDataFilePath);
                 }


                 if (errorSymbols.Count > 0)
                 {
                     CSVToDB.SaveErrorSymbols(errorSymbols, ERRORSymbolsPath + "/Symbol.csv");
                 }
             }
             catch (Exception ex)
             {

                 log.Error(ex);
             }
         }
         private static void getHistoryDataForNewSymbolAndCalculateAnalytics()
         {

            // String HistoricalSymbolDataFilePath = HistoricalDataFilePath + "/Specific-Symbols.csv";
            // List<String> symbolList = DataDownloader.GetsymbolListForNewlyAddedSymbols();

            // DateTime toDate = DateTime.Now.Date;
            // DateTime fromDate = DateTime.Now.AddYears(-4).Date;

             
            // SaveHistoryData(symbolList, HistoricalSymbolDataFilePath, true, false, fromDate, toDate, true);
            //List<TrendObjects> TrendObjectsList=calculateTrend(symbolList);
            // CSVExporter.WriteToCSVTrend(TrendObjectsList, UpdatedTrendCsvFilePath + "/UpdatedNewTrend.csv");
            // CSVToDB.UpdatedTrends(UpdatedTrendCsvFilePath, true, "UpdatedNewTrend.csv");
         }

        private static List<TrendObjects> calculateTrend( List<String> symbolList)
        {
            DateTime fromdate = DateTime.Now.AddDays(-500);
            DateTime toDate = DateTime.Now;

            List<TrendObjects> TrendObjectsList = new List<TrendObjects>();
           
              foreach (String symbol in symbolList)
              {
                   List<BarData> barlist = null;
                     try
                     {
                         barlist = HistoricalDataFeedFromDbDAO.GetDataFromFeedFromDB(fromdate, toDate, symbol);
                     }
                     catch (Exception ex)
                     {

                        throw ex;
                     }
                     if (barlist == null || barlist.Count == 0)
                     {
                         log.Info("Empty List Returned From Provider" + symbol);
                     }

                     else
                     {
                         try
                         {
                             log.Info("Calculating Trend For symbol:  " + symbol);
                             List<Trend> trendList = new List<Trend>();
                             TrendCalculation trendcalc = new TrendCalculation();
                             TrendObjects TrendObject = trendcalc.CalulateAllTrends(barlist, symbol);
                             TrendObjectsList.Add(TrendObject);
                         }
                         catch (Exception ex)
                         {
                             throw (ex);
                         }
                     }

              }

              return TrendObjectsList;
           
        }
                  */

                #endregion

            }
            catch (Exception ex)
            {

                log.Error(ex);
            }

        }


    }


}
