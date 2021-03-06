﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinLib;

namespace ChartLabFinCalculation
{
    public class GroupPerformance
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(GroupPerformance));
        public static void CalculateAndSaveGroupPerformance(string GroupPerformancePath)
        {

            try
            {
                List<DayWiseAvgReturnForGroup> finalGroupReturnList = new List<DayWiseAvgReturnForGroup>();
                log.Info("Process: gettiing historical Date List from DB ");
                List<HistoricalDates> historicalDateList = CommonDAO.getHistoricalDatesFromDB();
                log.Info("Process: getting Defensive Symbol List from DB ");
                List<string> DefensiveSymbolList = GroupPerformanceDAO.GetSymbolListForGroup(1);
                log.Info("Process: getting Aggressive Symbol List from DB ");
                List<string> AggressiveSymbolList = GroupPerformanceDAO.GetSymbolListForGroup(2);
                Dictionary<DateTime, double> datePriceList = new Dictionary<DateTime, double>();
                log.Info("Process: Calculate Avg Group Return for Defensive Group ");
                List<DayWiseAvgReturnForGroup> returnListDefensiveGroup = CalculateAvgGroupReturn(historicalDateList, DefensiveSymbolList, Constants.DEFENSIVE_GROUP);
                log.Info("Process: Calculate Avg Group Return for Aggressive Group ");
                List<DayWiseAvgReturnForGroup> returnListAggressiveGroup = CalculateAvgGroupReturn(historicalDateList, AggressiveSymbolList, Constants.AGGRESIVE_GROUP);
                finalGroupReturnList.AddRange(returnListDefensiveGroup);
                finalGroupReturnList.AddRange(returnListAggressiveGroup);
                log.Info("Process: Write To CSV GroupPerf ");
                CSVExporter.WriteToCSVGroupPerf(finalGroupReturnList, GroupPerformancePath + "/GroupPerformanceFile.csv");
                log.Info("Process: Update Group Perf CSV To DB ");
                GroupPerformanceDAO.UpdateGroupPerfCSVToDB(GroupPerformancePath);
            }
            catch (Exception ex)
            {
                
                log.Error("Error: "+ex);
            }
        }


        private static List<DayWiseAvgReturnForGroup> CalculateAvgGroupReturn(List<HistoricalDates> historyDateList, List<string> SymbolListByGroup, int GroupId)
        {
            List<DayWiseAvgReturnForGroup> GroupReturnList = new List<DayWiseAvgReturnForGroup>();

            try
            {
                List<double> AvgReturnList_Weekly = new List<double>();
                List<double> AvgReturnList_Quaterly = new List<double>();
                List<double> AvgReturnList_Monthly = new List<double>();
                List<double> AvgReturnList_Yearly = new List<double>();

                for (int i = 0; i < SymbolListByGroup.Count; i++)
                {
                    log.Info("Getting Data For Symbol " + SymbolListByGroup[i]);

                    List<DatePriceList> todaysDatePrice = GroupPerformanceDAO.TodaysDatePrice(SymbolListByGroup[i]);
                    for (int j = 0; j < historyDateList.Count; j++)
                    {
                        List<DatePriceList> datePrice = GroupPerformanceDAO.GetDataForGroupFromDB(SymbolListByGroup[i], historyDateList[j].date);

                        if (datePrice.Count != 0 && historyDateList[j].dateType == Constants.W)
                        {
                            AvgReturnList_Weekly.Add(100 * (todaysDatePrice[0].price - datePrice[0].price) / datePrice[0].price);
                        }
                        else if (datePrice.Count != 0 && historyDateList[j].dateType == Constants.M)
                        {
                            AvgReturnList_Monthly.Add(100 * (todaysDatePrice[0].price - datePrice[0].price) / datePrice[0].price);
                        }

                        else if (datePrice.Count != 0 && historyDateList[j].dateType == Constants.Q)
                        {
                            AvgReturnList_Quaterly.Add(100 * (todaysDatePrice[0].price - datePrice[0].price) / datePrice[0].price);
                        }

                        else if (datePrice.Count != 0 && historyDateList[j].dateType == Constants.Y)
                        {
                            AvgReturnList_Yearly.Add(100 * (todaysDatePrice[0].price - datePrice[0].price) / datePrice[0].price);
                        }


                    }



                }
                DayWiseAvgReturnForGroup groupPerformance = new DayWiseAvgReturnForGroup();
                groupPerformance.groupId = GroupId;
                groupPerformance.AvgReturn_Yearly = Common.CalculateAvgReturn(AvgReturnList_Yearly).AvgReturn;
                groupPerformance.AvgReturn_Weekly = Common.CalculateAvgReturn(AvgReturnList_Weekly).AvgReturn;
                groupPerformance.AvgReturn_Quaterly = Common.CalculateAvgReturn(AvgReturnList_Quaterly).AvgReturn;
                groupPerformance.AvgReturn_Monthly = Common.CalculateAvgReturn(AvgReturnList_Monthly).AvgReturn;
                GroupReturnList.Add(groupPerformance);
            }
            catch (Exception ex)
            {

                log.Error("Error: " + ex);
            }

            return GroupReturnList;
        }
       

    }

}
