using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Collections.Generic;


using System.Text;
using System.IO;
using System.Collections;
using FinLib;
using FinLib.Model;
namespace ChartLabFinCalculation
{




    /// <summary>
    /// Summary description for CSVExporter
    /// </summary>
    public class CSVExporter
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(CSVExporter));
        public static void WriteToCSV(List<SymbolAnalytics> listData, string path)
        {

            StreamWriter writer = new StreamWriter(path);
            WriteToStream(writer, listData, true, true);
        }
        public static void WriteToCSVOBOS(List<DateOBOSCount> listOBOSData, string path)
        {

            StreamWriter writer = new StreamWriter(path);
            WriteToStreamOBOS(writer, listOBOSData);
        }
        public static void WriteToStream(TextWriter stream, List<SymbolAnalytics> listData, bool header, bool quoteall)
        {
            try
            {
                foreach (SymbolAnalytics row in listData)
                {
                    string date = "";
                    if (row.shortTermTrendDate.Date.ToString("yyyy-MM-dd") == Constants.DEFAULT_DATE_FORMAT)
                    {
                        date = null;
                    }
                    else
                    {
                        date = row.shortTermTrendDate.Date.ToString("yyyy-MM-dd");
                    }

                    stream.Write(row.symbol);
                    stream.Write(',');
                    stream.Write(row.s1);
                    stream.Write(',');
                    stream.Write(row.s2);
                    stream.Write(',');
                    stream.Write(row.s3);
                    stream.Write(',');
                    stream.Write(row.r1);
                    stream.Write(',');
                    stream.Write(row.r2);
                    stream.Write(',');
                    stream.Write(row.r3);
                    stream.Write(',');
                    stream.Write(row.buySellRating);
                    stream.Write(',');
                    stream.Write(row.shortTermTrend);
                    stream.Write(',');
                    stream.Write(date);
                    stream.Write(',');
                    stream.Write(row.mediumTermTrend);
                    stream.Write(',');
                    stream.Write(row.longTermTrend);
                    stream.Write(',');
                    stream.Write(row.alert);
                    stream.Write(',');
                    stream.Write(row.YTDPrice);
                    stream.Write(',');
                    stream.Write(row.MTDPrice);
                    stream.Write(',');
                    stream.Write(row.QTDPrice);
                    stream.Write(',');
                    stream.Write(row.WTDPrice);
                    stream.Write(',');
                    stream.Write(row.OBOSCurrent);
                    stream.Write(',');
                    stream.Write(row.OBOSWeekly);
                    stream.Write(',');
                    stream.Write(10);
                    stream.Write(',');
                    stream.Write(row.STD50Days);
                    stream.Write(',');
                    stream.Write(10);
                    stream.Write(',');
                    stream.Write(row.currentRSI);
                    stream.Write(',');
                    stream.Write(row.weeklyRSI);
                    stream.Write(',');
                    stream.Write(row.STD21Days);
                    stream.Write(',');
                    stream.Write(row.low52WeekRange);
                    stream.Write(',');
                    stream.Write(row.high52WeekRange);

                    stream.Write('\n');

                }
                stream.Flush();
                stream.Close();
            }
            catch (Exception)
            {
                
                throw;
            }

        }

        public static void WriteToStreamOBOS(TextWriter stream, List<DateOBOSCount> listData)
        {

            try
            {


                foreach (DateOBOSCount row in listData)
                {

                    stream.Write(row.Date.ToString("yyyy/MM/dd"));
                    stream.Write(',');
                    stream.Write(row.obPer);
                    stream.Write(',');
                    stream.Write(row.osPer);
                    stream.Write(',');
                    stream.Write(row.obCount);
                    stream.Write(',');
                    stream.Write(row.osCount);
                    stream.Write('\n');


                }
                stream.Flush();
                stream.Close();
            }
            catch (Exception)
            {
                
                throw;
            }

        }
        private static void WriteItem(TextWriter stream, object item, bool quoteall)
        {

            if (item == null)

                return;

            string s = item.ToString();

            if (quoteall || s.IndexOfAny("\",\x0A\x0D".ToCharArray()) > -1)

                stream.Write("\"" + s.Replace("\"", "\"\"") + "\"");

            else

                stream.Write(s);

            stream.Flush();

        }

        public static void WriteToCSVPattern(List<PatternBarData> patternListData, string path)
        {

            StreamWriter writer = new StreamWriter(path);
            WriteToStreamPattern(writer, patternListData);
        }

        private static void WriteToStreamPattern(TextWriter stream, List<PatternBarData> patternListData)
        {
            try
            {
                foreach (PatternBarData row in patternListData)
                {
                    stream.Write(row.symbol);
                    stream.Write(',');
                    stream.Write(row.startDate.ToString("yyyy/MM/dd"));
                    stream.Write(',');
                    stream.Write(row.endDate.ToString("yyyy/MM/dd"));
                    stream.Write(',');
                    stream.Write(row.PatternId);
                    stream.Write('\n');


                }
                stream.Flush();
                stream.Close();
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public static void WriteToCSVTrend(List<TrendObjects> TrendObjectsList, string path)
        {

            StreamWriter writer = new StreamWriter(path);
            WriteToStreamTrend(writer, TrendObjectsList);

            
        }

        private static void WriteToStreamTrend(TextWriter stream, List<TrendObjects> TrendObjectsList)
        {
            try
            {
                foreach (TrendObjects row in TrendObjectsList)
                {

                    stream.Write(row.symbol);
                    stream.Write(',');
                    stream.Write(row.shortTermTrend);
                    stream.Write(',');
                    stream.Write(row.shortTermTrendDate.ToString("yyyy/MM/dd"));
                    stream.Write(',');
                    stream.Write(row.mediumTermTrend);
                    stream.Write(',');
                    stream.Write(row.longTermTrend);
                    stream.Write('\n');


                }
                stream.Flush();
                stream.Close();
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public static void WriteToCSVRating(List<Rating> ratingList, string path)
        {
            StreamWriter writer = new StreamWriter(path);
            WriteToStreamRating(writer, ratingList);
        }

        private static void WriteToStreamRating(TextWriter stream, List<Rating> ratingList)
        {
            try
            {
                foreach (Rating row in ratingList)
                {

                    stream.Write(row.symbol);
                    stream.Write(',');
                    stream.Write(row.rating);
                    stream.Write(',');
                    stream.Write(row.ratingValue);
                    stream.Write(',');
                    stream.Write(row.ctRating);
                    stream.Write(',');
                    stream.Write(row.ctRatingValue);
                    stream.Write(',');
                    stream.Write(row.ratingDate.ToString("yyyy-MM-dd"));
                    stream.Write('\n');


                }
                stream.Flush();
                stream.Close();
            }
            catch (Exception)
            {
                
                throw;
            }
        }


        public static void WriteToCSV(List<InputBarData> listData, string path)
        {

            StreamWriter writer = new StreamWriter(path);
            WriteToStream(writer, listData, true, true);
        }
        public static void WriteToStream(TextWriter stream, List<InputBarData> listData, bool header, bool quoteall)
        {
            try
            {


                DateTime shortTermTrendDate = DateTime.Now;
                shortTermTrendDate.ToString("yyyy/MM/dd");
                foreach (InputBarData row in listData)
                {
                    foreach (BarData data in row.barListRow)
                    {
                        row.symbol = row.symbol.ToUpper();
                        stream.Write(row.symbol);
                        stream.Write(',');
                        stream.Write(data.open);
                        stream.Write(',');
                        stream.Write(data.high);
                        stream.Write(',');
                        stream.Write(data.low);
                        stream.Write(',');
                        stream.Write(data.close);
                        stream.Write(',');
                        stream.Write(data.actualclose);
                        stream.Write(',');
                        stream.Write(data.date.Date.ToString("yyyy-MM-dd"));
                        stream.Write(',');
                        stream.Write(data.volume);
                        stream.Write('\n');

                    }


                }
                stream.Flush();
                stream.Close();
            }
            catch (Exception)
            {
                
                throw;
            }

        }


        internal static void WriteToCSVChangeRatingHistory(List<BuySellRatingChangeHist> ChangeBuySellRatingHist, string path)
        {
            StreamWriter writer = new StreamWriter(path);
            WriteToStreamChangeRatingHist(writer, ChangeBuySellRatingHist);
        }

        private static void WriteToStreamChangeRatingHist(TextWriter stream, List<BuySellRatingChangeHist> ChangeBuySellRatingHist)
        {
            try
            {
                foreach (BuySellRatingChangeHist row in ChangeBuySellRatingHist)
                {

                    stream.Write(row.symbol);
                    stream.Write(',');
                    stream.Write(row.newRating);
                    stream.Write(',');
                    stream.Write(row.oldRating);
                    stream.Write(',');
                    stream.Write(row.ratingDate.Date.ToString("yyyy-MM-dd"));
                    stream.Write('\n');


                }
                stream.Flush();
                stream.Close();
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public static void WriteToCSVDailyAvgVolume(List<DailyAverageVolume> listData, string path)
        {
            StreamWriter writer = new StreamWriter(path);
            WriteToDailyAvgVolumeStream(writer, listData, true, true);
        }

        private static void WriteToDailyAvgVolumeStream(TextWriter stream, List<DailyAverageVolume> DailyAverageVolumeList, bool header, bool quoteall)
        {

            try
            {
                foreach (DailyAverageVolume row in DailyAverageVolumeList)
                {
                    stream.Write(row.Symbol);
                    stream.Write(',');
                    stream.Write(row.Volume);

                    stream.Write('\n');

                }



                stream.Flush();
                stream.Close();
            }
            catch (Exception)
            {
                
                throw;
            }
        }



        public static void WriteToCSVVolumePerformance(List<DayWiseAvgReturnForSymbol> listData, string path)
        {
            StreamWriter writer = new StreamWriter(path);
            WriteToCSVVolumePerformanceStream(writer, listData, true, true);
        }

        private static void WriteToCSVVolumePerformanceStream(TextWriter stream, List<DayWiseAvgReturnForSymbol> listData, bool p, bool p_2)
        {

            log.Info("\n Writing in Historical Volume Alert Performance File");
            try
            {
                foreach (DayWiseAvgReturnForSymbol row in listData)
                {
                    stream.Write(row.symbol);
                    stream.Write(',');
                    stream.Write(row.AvgReturn_2_Days);
                    stream.Write(',');
                    stream.Write(row.AvgReturn_5_Days);
                    stream.Write(',');
                    stream.Write(row.AvgReturn_30_Days);
                    stream.Write(',');
                    stream.Write(row.AvgReturn_90_Days);
                    stream.Write('\n');

                }



                stream.Flush();
            }
            finally
            {
                stream.Close();
            }
           
        }

        public static void WriteToCSVHistoricalVolume(List<AlertDateList> totalAlertDateList, string path)
        {
            StreamWriter writer = new StreamWriter(path);
            WriteToCSVHistoricalVolumeStream(writer, totalAlertDateList, true, true);
        }
        private static void WriteToCSVHistoricalVolumeStream(TextWriter stream, List<AlertDateList> listData, bool p, bool p_2)
        {
            log.Info("\n Writing in Historical Volume Alert File");

            try
            {
                foreach (AlertDateList row in listData)
                {
                    stream.Write(row.Symbol);
                    stream.Write(',');
                    stream.Write(row.ChangeDate.Date.ToString("yyyy-MM-dd"));
                    stream.Write(',');
                    stream.Write(row.Volume);
                    stream.Write(',');
                    stream.Write(row.PctChange);
                    stream.Write(',');
                    stream.Write(row.volumeAlertType);
                    stream.Write(',');
                    stream.Write(row.avgVolume);
                    stream.Write('\n');

                }



                stream.Flush();
                stream.Close();
            }
            catch (Exception)
            {
                
                throw;
            }
        }


        public static void WriteToCSVSentimentIndicatorPerf(List<DayWiseAvgReturnForSentimentSymbol> Listdata, string path)
        {
            StreamWriter writer = new StreamWriter(path);
            WriteToCSVSentimentPerfStream(writer, Listdata, true, true);
        }

        private static void WriteToCSVSentimentPerfStream(TextWriter stream, List<DayWiseAvgReturnForSentimentSymbol> Listdata, bool p, bool p_2)
        {
            log.Info("\n Writing in Sentiment Performance File\n");

            try
            {
                foreach (DayWiseAvgReturnForSentimentSymbol row in Listdata)
                {
                    stream.Write(row.symbolId);
                    stream.Write(',');
                    stream.Write(row.AvgReturn_Weekly);
                    stream.Write(',');
                    stream.Write(row.AvgReturn_1_Month);
                    stream.Write(',');
                    stream.Write(row.AvgReturn_2_Month);
                    stream.Write(',');
                    stream.Write(row.Indicator);
                    stream.Write('\n');

                }



                stream.Flush();
                stream.Close();
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public static void WriteToCSVGroupPerf(List<DayWiseAvgReturnForGroup> ListData, string path)
        {
            StreamWriter writer = new StreamWriter(path);
            WriteToCSVGroupPerfStream(writer, ListData, true, true);
        }

        private static void WriteToCSVGroupPerfStream(TextWriter stream, List<DayWiseAvgReturnForGroup> ListData, bool p, bool p_2)
        {
            log.Info("\n Writing in Group Performance File\n");

            try
            {
                foreach (DayWiseAvgReturnForGroup row in ListData)
                {
                    stream.Write(row.groupId);
                    stream.Write(',');
                    stream.Write(row.AvgReturn_Weekly);
                    stream.Write(',');
                    stream.Write(row.AvgReturn_Monthly);
                    stream.Write(',');
                    stream.Write(row.AvgReturn_Quaterly);
                    stream.Write(',');
                    stream.Write(row.AvgReturn_Yearly);
                    stream.Write('\n');

                }



                stream.Flush();
                stream.Close();
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public static void WriteToCSVSectorWiseStrongWeakSymbols(List<SectorStrongWeakSymbol> ListData, string path)
        {
            StreamWriter writer = new StreamWriter(path);
            WriteToCSVSectorWiseStrongWeakStream(writer, ListData, true, true);
        }

        private static void WriteToCSVSectorWiseStrongWeakStream(TextWriter stream, List<SectorStrongWeakSymbol> ListData, bool p, bool p_2)
        {
            log.Info("\n Writing in Sector Wise Strong Weak File\n");

            try
            {
                foreach (SectorStrongWeakSymbol row in ListData)
                {
                    stream.Write(row.SectorId);
                    stream.Write(',');
                    stream.Write(row.Symbol);
                    stream.Write(',');
                    stream.Write(row.StrongWeakIndicator);
                    stream.Write(',');
                    stream.Write(row.RatingValue);
                    stream.Write('\n');

                }



                stream.Flush();
                stream.Close();
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public static void WriteToCSVLongShortPerf(List<ShortLongAlertPerf> ListData, string path)
        {
            StreamWriter writer = new StreamWriter(path);
            WriteToCSVLongShortPerfStream(writer, ListData, true, true);
        }

        private static void WriteToCSVLongShortPerfStream(TextWriter stream, List<ShortLongAlertPerf> ListData, bool p, bool p_2)
        {
            log.Info("\n Writing in Long Short Performance File\n");

            try
            {
                foreach (ShortLongAlertPerf row in ListData)
                {
                    stream.Write(row.YesterdayPerf);
                    stream.Write(',');
                    stream.Write(row.Day_5_Perf);
                    stream.Write(',');
                    stream.Write(row.longShortId);
                    stream.Write('\n');

                }



                stream.Flush();
                stream.Close();
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public static void WriteToCSVCTRatingHistory(List<CTRatingHistory> ListData, string path)
        {
            StreamWriter writer = new StreamWriter(path);
            WriteToCSVCTRatingHistoryStream(writer, ListData, true, true);
        }

        private static void WriteToCSVCTRatingHistoryStream(TextWriter stream, List<CTRatingHistory> ListData, bool p, bool p_2)
        {
            log.Info("\n Writing in Counter Trend Rating History File\n");

            try
            {
                foreach (CTRatingHistory row in ListData)
                {
                    stream.Write(row.symbol);
                    stream.Write(',');
                    stream.Write(row.Date.Date.ToString("yyyy-MM-dd"));
                    stream.Write(',');
                    stream.Write(row.ctRating);
                    stream.Write('\n');

                }



                stream.Flush();
                stream.Close();
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public static void WriteToCSVCTRatingPerf(List<DayWiseAvgReturnForCTRating> ListData, string path)
        {
            StreamWriter writer = new StreamWriter(path);
            WriteToCSVCTRatingPerfStream(writer, ListData, true, true);
        }

        private static void WriteToCSVCTRatingPerfStream(TextWriter stream, List<DayWiseAvgReturnForCTRating> ListData, bool p, bool p_2)
        {
            log.Info("\n Writing in Counter Trend Rating Performance File\n");

            foreach (DayWiseAvgReturnForCTRating row in ListData)
            {
                stream.Write(row.Symbol);
                stream.Write(',');
                stream.Write(row.AvgReturn_Weekly);
                stream.Write(',');
                stream.Write(row.AvgReturn_1_Month);
                stream.Write(',');
                stream.Write(row.AvgReturn_2_Month);
                stream.Write('\n');

            }



            stream.Flush();
            stream.Close(); 
        }

        public static void SaveErrorSymbols(List<string> errorSymbols, string ERRORSymbolsPath)
        {
            StreamWriter writer = new StreamWriter(ERRORSymbolsPath);

            try
            {

                foreach (String data in errorSymbols)
                {
                    writer.Write(data);
                    writer.Write(',');

                }
                if (errorSymbols.Count == 0)
                {
                    writer.Write("");

                }
                writer.Flush();
                writer.Close();
            }
            catch (Exception)
            {
                
                throw;
            }

        }


        internal static void WriteToCSVSectorPerf(List<SectorPerfHist> sectorHistPerfList, string fileName)
        {

            StreamWriter writer = new StreamWriter(fileName);


            try
            {
                foreach (SectorPerfHist row in sectorHistPerfList)
                {
                    writer.Write(row.sectorId);
                    writer.Write(',');
                    writer.Write(row.date.Date.ToString("yyyy-MM-dd"));
                    writer.Write(',');
                    writer.Write(row.rating);
                    writer.Write('\n');

                }

                writer.Flush();
                writer.Close();
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        internal static void WriteToCSVSymbolRule(List<SymbolRule> symbolRuleList, string fileName)
        {

            StreamWriter writer = new StreamWriter(fileName);


            try
            {
                foreach (SymbolRule row in symbolRuleList)
                {
                    writer.Write(row.symbol);
                    writer.Write(',');
                    writer.Write(row.RuleId);
                    writer.Write(',');
                    writer.Write(row.ratingChangeDate.Date.ToString("yyyy-MM-dd"));
                    writer.Write(',');
                    writer.Write(row.changeDatePrice);
                    writer.Write(',');
                    writer.Write(row.currating);
                    writer.Write(',');
                    writer.Write(row.prevrating);
                    writer.Write('\n');

                }

                writer.Flush();
                writer.Close();
            }
            catch (Exception)
            {
                
                throw;
            }

        }

        internal static void WriteToCSVSnPPriceList(Dictionary<DateTime, double> datePriceDict, string fileName)
        {

            try
            {

                StreamWriter writer = new StreamWriter(fileName);
                foreach (KeyValuePair<DateTime, double> row in datePriceDict)
                {
                    writer.Write(row.Key.Date.ToString("yyyy-MM-dd"));
                    writer.Write(',');
                    writer.Write(row.Value);
                    writer.Write('\n');

                }

                writer.Flush();
                writer.Close();
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        internal static void WriteToCSVStatsPerfData(List<StatisticsPerf> StatsPerfData, string fileName)
        {


            StreamWriter writer = new StreamWriter(fileName);
            try
            {
                foreach (StatisticsPerf row in StatsPerfData)
                {
                    writer.Write(row.symbol);
                    writer.Write(',');
                    writer.Write(row.buyPrice);
                    writer.Write(',');
                    writer.Write(row.buyDate.ToString("yyyy-MM-dd"));
                    writer.Write(',');
                    writer.Write(row.sellPrice);
                    writer.Write(',');
                    if (row.sellDate.ToString("yyyy-MM-dd") != null)
                    {
                        writer.Write(row.sellDate.ToString("yyyy-MM-dd"));
                    }
                    else
                    {
                        writer.Write(DateTime.Now.ToString("yyyy-MM-dd"));
                    }
                    writer.Write(',');
                    writer.Write(row.StatId);
                    writer.Write(',');
                    writer.Write(row.StatReturn);
                    writer.Write(',');
                    writer.Write(row.duration);
                    writer.Write('\n');

                }

                writer.Flush();
                writer.Close();
            }
            catch (Exception)
            {
                
                throw;
            }
        }


        internal static void WriteToCSVMyAlertsData(List<UserAlerts> myAlerts, string fileName)
        {
            StreamWriter writer = new StreamWriter(fileName);
            try
            {
                foreach (UserAlerts row in myAlerts)
                {
                    writer.Write(row.userId);
                    writer.Write(',');
                    writer.Write(row.portfolioAlerts);
                    writer.Write(',');

                    writer.Write(row.watchlistAlerts);
                    writer.Write('\n');

                }

                writer.Flush();
                writer.Close();
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        internal static void WriteToCSVWatchlistAlertData(List<UserAlerts> watchlistAlerts, string fileName)
        {
            StreamWriter writer = new StreamWriter(fileName);
            try
            {
                foreach (UserAlerts row in watchlistAlerts)
                {
                    writer.Write(row.userId);
                    writer.Write(',');
                    writer.Write(row.watchlistId);
                    writer.Write(',');

                    writer.Write(row.watchlistAlerts);
                    writer.Write('\n');

                }

                writer.Flush();
                writer.Close();
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        internal static void WriteToCSVTopRatingSymbolsHist(List<Rating> totalTopSymbolsList, string fileName)
        {
            StreamWriter writer = new StreamWriter(fileName);
            try
            {
                foreach (Rating row in totalTopSymbolsList)
                {
                    writer.Write(row.symbol);
                    writer.Write(',');
                    writer.Write(row.ratingValue);
                    writer.Write(',');

                    writer.Write(row.ratingDate.ToString("yyyy-MM-dd"));
                    writer.Write('\n');

                }

                writer.Flush();
                writer.Close();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }

}
