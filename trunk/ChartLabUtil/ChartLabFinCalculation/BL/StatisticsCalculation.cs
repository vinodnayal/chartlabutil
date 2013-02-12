using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinLib;
using FinLib.Model;
using ChartLabFinCalculation.DAL;

namespace ChartLabFinCalculation.BL
{
    public class StatisticsCalculation
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(StatisticsCalculation));
        private static List<StatisticsPerf> finalStatPerfData = new List<StatisticsPerf>();
        internal static List<StatisticsPerf> calculateStatistics()
        {
            try
            {
                List<String> symbolList = OBOSRatingDAO.GetSnPSymbols();
                DateTime startDate = new DateTime(2012, 02, 01);

                foreach (String symbol in symbolList)
                {
                    List<SymbolRatingAlert> SymbolRatingchanges = BuySellRatingDAO.getRatingChangeHistory(symbol);
                    Dictionary<DateTime, double> SymbolHistPriceList = CommonDAO.getSymbolsHistPrices(symbol, startDate);
                  
                    
                    if (SymbolRatingchanges.Count>0)
                    {
                        calculateStatsFor4_5Rating(SymbolRatingchanges, SymbolHistPriceList);
                        calculateStatsFor3_5Rating(SymbolRatingchanges, SymbolHistPriceList);
                        calculateStatsFor3_4_5Rating(SymbolRatingchanges, SymbolHistPriceList);
                    }
                    else
                    {
                        //if symbol dont have  buy sell rating  change from starting
                        SymbolRatingAlert symbolRating = BuySellRatingDAO.getSymbolCurrentRating(symbol);
                        if (symbolRating.currating == 5)
                        { 
                            double buyPrice= Common.GetDayWiseReturn(SymbolHistPriceList, startDate, 0, false);
                             double sellPrice= Common.GetDayWiseReturn(SymbolHistPriceList, DateTime.Now, 0, false);
                            double returnPct=0;
                            if(buyPrice!=0 && buyPrice!=0)
                            {
                             returnPct=(sellPrice-buyPrice)*100/buyPrice;
                            }else
                            {
                                returnPct=0;
                            }

                            finalStatPerfData.Add(new StatisticsPerf
                            {
                                symbol = symbol,
                                buyDate = startDate,
                                buyPrice = buyPrice,
                                sellDate = DateTime.Now,
                                sellPrice = sellPrice,
                                StatId = 1,
                                duration = (DateTime.Now - startDate).TotalDays,
                                StatReturn = returnPct
                            });
                        
                        
                        }
                }
            }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            return finalStatPerfData;

        
        }

        private static void calculateStatsFor3_4_5Rating(List<SymbolRatingAlert> SymbolRatingchanges, Dictionary<DateTime, double> SymbolHistPriceList)
        {
            int statperfPoints = 0;
            bool is3to4Rating = false;
            DateTime dateRatingChange3to4 = DateTime.MinValue;
            Dictionary<int, StatisticsPerf> statPerfDict = new Dictionary<int, StatisticsPerf>();
            try
            {
                foreach (SymbolRatingAlert changeAlert in SymbolRatingchanges)
                {

                    if (changeAlert.currating == 4 && changeAlert.prevrating == 3)
                    {
                        is3to4Rating = true;
                        dateRatingChange3to4 = changeAlert.ratingChangeDate;
                    }
                    else if (is3to4Rating == true && dateRatingChange3to4 != DateTime.MinValue && (changeAlert.currating == 5 && changeAlert.prevrating == 4))
                    {
                        if ((changeAlert.ratingChangeDate - dateRatingChange3to4).TotalDays <= 18)
                        {
                            statperfPoints++;
                            statPerfDict.Add(statperfPoints,
                                       new StatisticsPerf
                                       {
                                           symbol = changeAlert.symbol,
                                           buyPrice = Common.GetDayWiseReturn(SymbolHistPriceList, changeAlert.ratingChangeDate, 0, false),
                                           buyDate = changeAlert.ratingChangeDate


                                       });
                        }
                        else {
                            is3to4Rating = false;
                            dateRatingChange3to4 = DateTime.MinValue;
                        
                        }
                    }
                    else if (changeAlert.prevrating == 5 && is3to4Rating == true && dateRatingChange3to4 != DateTime.MinValue)
                    {


                        if (statPerfDict.ContainsKey(statperfPoints))
                        {
                            StatisticsPerf statsPerformance = statPerfDict[statperfPoints];

                            statsPerformance.sellPrice = Common.GetDayWiseReturn(SymbolHistPriceList, changeAlert.ratingChangeDate, 0, false);
                            statsPerformance.sellDate = changeAlert.ratingChangeDate;

                        }


                    }
                    else
                    {
                        is3to4Rating = false;
                        dateRatingChange3to4 = DateTime.MinValue;
                    }

                }
                calculateReturnAndDuration(statPerfDict, 3);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private static void calculateStatsFor3_5Rating(List<SymbolRatingAlert> SymbolRatingchanges, Dictionary<DateTime, double> SymbolHistPriceList)
        {

            int statperfPoints = 0;
            Dictionary<int, StatisticsPerf> statPerfDict = new Dictionary<int, StatisticsPerf>();
            try
            {
                foreach (SymbolRatingAlert changeAlert in SymbolRatingchanges)
                {

                    if (changeAlert.currating == 5 && changeAlert.prevrating == 3)
                    {
                        statperfPoints++;
                        statPerfDict.Add(statperfPoints,
                                   new StatisticsPerf
                                   {
                                       symbol = changeAlert.symbol,
                                       buyPrice = Common.GetDayWiseReturn(SymbolHistPriceList, changeAlert.ratingChangeDate, 0, false),
                                       buyDate = changeAlert.ratingChangeDate


                                   });

                    }
                    else if (changeAlert.prevrating == 5)
                    {
                        if (statPerfDict.ContainsKey(statperfPoints))
                        {
                            StatisticsPerf statsPerformance = statPerfDict[statperfPoints];
                              statsPerformance.sellPrice = Common.GetDayWiseReturn(SymbolHistPriceList, changeAlert.ratingChangeDate, 0, false);
                                statsPerformance.sellDate = changeAlert.ratingChangeDate;
                            
                        }


                    }

                }
                calculateReturnAndDuration(statPerfDict, 2);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private static void calculateStatsFor4_5Rating(List<SymbolRatingAlert> SymbolRatingchanges, Dictionary<DateTime, double> SymbolHistPriceList)
        {
           
            int count = 0;
            DateTime ratingStartDate = new DateTime(2012, 2, 2);
            int statperfPoints = 0;
            try
            {
                Dictionary<int, StatisticsPerf> statPerfDict = new Dictionary<int, StatisticsPerf>();
                foreach (SymbolRatingAlert changeAlert in SymbolRatingchanges)
                {
                    
                        if (count == 0)
                        {
                            if (changeAlert.prevrating == 5)
                            {
                                statperfPoints++;
                                statPerfDict.Add(statperfPoints,
                                    new StatisticsPerf
                                    {
                                        symbol = changeAlert.symbol,
                                        buyDate = ratingStartDate,
                                        buyPrice = Common.GetDayWiseReturn(SymbolHistPriceList, ratingStartDate, 0, false),
                                        sellPrice = Common.GetDayWiseReturn(SymbolHistPriceList, changeAlert.ratingChangeDate, 0, false),
                                        sellDate = changeAlert.ratingChangeDate


                                    });
                            }
                            count++;
                        }
                        else if (changeAlert.currating == 5 && changeAlert.prevrating == 4)
                        {
                            statperfPoints++;
                            statPerfDict.Add(statperfPoints,
                                       new StatisticsPerf
                                       {
                                           symbol = changeAlert.symbol,
                                           buyPrice = Common.GetDayWiseReturn(SymbolHistPriceList, changeAlert.ratingChangeDate, 0, false),
                                           buyDate = changeAlert.ratingChangeDate


                                       });

                        }
                        else if (changeAlert.prevrating == 5)
                        {
                            if (statPerfDict.ContainsKey(statperfPoints))
                            {
                                StatisticsPerf statsPerformance = statPerfDict[statperfPoints];
                                statsPerformance.sellPrice = Common.GetDayWiseReturn(SymbolHistPriceList, changeAlert.ratingChangeDate, 0, false);
                                statsPerformance.sellDate = changeAlert.ratingChangeDate;
                            }


                        }

                    }
                

                calculateReturnAndDuration(statPerfDict, 1);

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private static void calculateReturnAndDuration(Dictionary<int, StatisticsPerf> statPerfDict, int statId)
        {
            try
            {
                foreach (KeyValuePair<int, StatisticsPerf> stat in statPerfDict)
                {
                    StatisticsPerf statPerf = stat.Value;
                    if (statPerf.sellDate == DateTime.MinValue)
                    {
                        statPerf.sellDate = DateTime.Now;
                    }

                    statPerf.duration = (statPerf.sellDate - statPerf.buyDate).TotalDays;

                  
                   
                        if (statPerf.sellPrice == 0.0)
                        {
                            
                            DateTime fromDate = DateTime.Now.AddDays(-5);
                            DateTime todayDate = DateTime.Now;
                            Dictionary<DateTime, double> SymbolHistPriceList = CommonDAO.getSymbolsHistPrices(statPerf.symbol, fromDate);

                            statPerf.sellPrice = Common.GetDayWiseReturn(SymbolHistPriceList, todayDate, 0, false);
                        }
                        if (statPerf.buyPrice != 0 && statPerf.sellPrice != 0)
                        {
                       
                            statPerf.StatReturn = (statPerf.sellPrice - statPerf.buyPrice) * 100 / statPerf.buyPrice;
                        }
                    
                    statPerf.StatId = statId;

                    finalStatPerfData.Add(statPerf);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

        }

        internal static void calculateStatisticsPerf()
        {
            try
            {
                StatisticsDAO.insertStatsAvgReturn();
                CalculalatePositiveTimePct();

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private static void CalculalatePositiveTimePct()
        {
            try
            {
                Dictionary<int, TempState> PositivetimeDict = StatisticsDAO.getPositivetimeData();
                Dictionary<int, int> statIdAndUpDaysPctDict = new Dictionary<int, int>();
                foreach (KeyValuePair<int, TempState> stat in PositivetimeDict)
                {
                    Double upDaysPct = 0;
                    TempState tempStat = stat.Value;
                    if (tempStat.totalCount != 0)
                    {
                        upDaysPct = tempStat.upDaysCount * 100 / tempStat.totalCount;
                    }
                    StatisticsDAO.updatePositiveTimePct(stat.Key, upDaysPct);
                }

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
    }
}
