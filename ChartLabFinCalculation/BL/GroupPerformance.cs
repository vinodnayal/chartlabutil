using System;
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
            List<DayWiseAvgReturnForGroup> finalGroupReturnList = new List<DayWiseAvgReturnForGroup>();
            List<HistoricalDates> historicalDateList = CommonDAO.getHistoricalDatesFromDB();
            List<string> DefensiveSymbolList =GroupPerformanceDAO.GetSymbolListForGroup(1);
            List<string> AggressiveSymbolList = GroupPerformanceDAO.GetSymbolListForGroup(2);
            Dictionary<DateTime, double> datePriceList=new Dictionary<DateTime,double>();
            List<DayWiseAvgReturnForGroup> returnListDefensiveGroup = CalculateAvgGroupReturn(historicalDateList, DefensiveSymbolList, Constants.DEFENSIVE_GROUP);
            List<DayWiseAvgReturnForGroup> returnListAggressiveGroup = CalculateAvgGroupReturn(historicalDateList, AggressiveSymbolList,Constants.AGGRESIVE_GROUP);
            finalGroupReturnList.AddRange(returnListDefensiveGroup);
            finalGroupReturnList.AddRange(returnListAggressiveGroup);

            CSVExporter.WriteToCSVGroupPerf(finalGroupReturnList, GroupPerformancePath + "/GroupPerformanceFile.csv");
            GroupPerformanceDAO.UpdateGroupPerfCSVToDB(GroupPerformancePath);
        }


        private static List<DayWiseAvgReturnForGroup> CalculateAvgGroupReturn(List<HistoricalDates> historyDateList, List<string> SymbolListByGroup, int GroupId)
        {
            List<DayWiseAvgReturnForGroup> GroupReturnList = new List<DayWiseAvgReturnForGroup>();

            List<double> AvgReturnList_Weekly = new List<double>();
            List<double> AvgReturnList_Quaterly = new List<double>();
            List<double> AvgReturnList_Monthly = new List<double>();
            List<double> AvgReturnList_Yearly = new List<double>();

            for (int i = 0; i < SymbolListByGroup.Count; i++)
            {
                log.Info("Getting Data For Symbol "+SymbolListByGroup[i]);

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

            return GroupReturnList;
        }
       

    }

}
