using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using FinLib;

namespace ChartLabFinCalculation
{
   public class LongShortAlerts
    {
       static log4net.ILog log = log4net.LogManager.GetLogger(typeof(GroupPerformance));

       public static void CalculateLongShortAlerts()
       {
           LongShortAlertDAO.UpdateLongShortAlerts();
       }

        //public static void CaculateAndUpdateLongShortAlertPerf(string LongShortPerfPath)
        //{
        //    List<string> LongSymbolList = GetLongShortSymbol(1);
        //    List<string> ShortSymbolList = GetLongShortSymbol(2);
        //    List<string> SnPSymbol=new List<string>();
        //    string snpSymbol = "SPY";
        //    SnPSymbol.Add(snpSymbol);

        //    List<ShortLongAlertPerf> PerfList = new List<ShortLongAlertPerf>();


        //    Dictionary<string, double> LongSymbolTodayPriceList = GetAllPriceListForSymbol(LongSymbolList,Constants.P);
        //    Dictionary<string, double> LongSymbolPreviousDayPriceList = GetAllPriceListForSymbol(LongSymbolList,Constants.D_2 );
        //    Dictionary<string, double> LongSymbol5DaysPriceList = GetAllPriceListForSymbol(LongSymbolList, Constants.D_5);
        //    List<ShortLongAlertPerf> longPerfList = CalculateAlertPerf(LongSymbolList, LongSymbolTodayPriceList, LongSymbolPreviousDayPriceList, LongSymbol5DaysPriceList, Constants.LONG_ALERT);
        //    PerfList.AddRange(longPerfList);

        //    Dictionary<string, double> ShortSymbolTodayPriceList = GetAllPriceListForSymbol(ShortSymbolList, Constants.P);
        //    Dictionary<string, double> ShortSymbolPreviousDayPriceList = GetAllPriceListForSymbol(ShortSymbolList, Constants.D_2);
        //    Dictionary<string, double> ShortSymbol5DaysPriceList = GetAllPriceListForSymbol(ShortSymbolList, Constants.D_5);
        //    List<ShortLongAlertPerf> shortPerfList = CalculateAlertPerf(ShortSymbolList, ShortSymbolTodayPriceList, ShortSymbolPreviousDayPriceList, ShortSymbol5DaysPriceList,Constants.SHORT_ALERT);
        //    PerfList.AddRange(shortPerfList);

            
        //    Dictionary<string, double> SnPSymbolPreviousDayPriceList = GetAllPriceListForSymbol(SnPSymbol, Constants.D_2);
        //    Dictionary<string, double> SnPSymbol5DaysPriceList = GetAllPriceListForSymbol(SnPSymbol, Constants.D_5);
        //    ShortLongAlertPerf snpObj = new ShortLongAlertPerf();
        //    if (SnPSymbolPreviousDayPriceList.ContainsKey(snpSymbol))
        //    {
        //        snpObj.YesterdayPerf = SnPSymbolPreviousDayPriceList[snpSymbol];
        //    }
        //    if (SnPSymbol5DaysPriceList.ContainsKey(snpSymbol))
        //    {
        //        snpObj.Day_5_Perf = SnPSymbol5DaysPriceList[snpSymbol];
        //    }
        //    snpObj.longShortId = Constants.SNP_ALERT;
        //    PerfList.Add(snpObj);



        //    CSVExporter.WriteToCSVLongShortPerf(PerfList, LongShortPerfPath + "/LongShortPerf.csv");
        //    CSVToDB.InsertLongShortPerf(LongShortPerfPath);


        //}

        //private static List<ShortLongAlertPerf> CalculateAlertPerf(List<string> SymbolList, Dictionary<string, double> TodayPricedict, Dictionary<string, double> PreviousDayPricedict, Dictionary<string, double> Days_5_Pricedict,int alertType)
        //{
        //    List<ShortLongAlertPerf> perfList = new List<ShortLongAlertPerf>();
        //    List<double> AvgReturnYesterdayList = new List<double>();
        //    List<double> AvgReturn5dayList = new List<double>();

        //    for (int i = 0; i < SymbolList.Count; i++)
        //    {
        //        double yesterDayReturn = 0;
        //        double fiveDayReturn = 0;

        //            if (TodayPricedict.ContainsKey(SymbolList[i]) && PreviousDayPricedict.ContainsKey(SymbolList[i]))
        //            {
        //                 yesterDayReturn = (TodayPricedict[SymbolList[i]] - PreviousDayPricedict[SymbolList[i]]) * 100 / PreviousDayPricedict[SymbolList[i]];
        //            }

        //            if(TodayPricedict.ContainsKey(SymbolList[i]) && Days_5_Pricedict.ContainsKey(SymbolList[i]))
        //            {
        //                 fiveDayReturn = (TodayPricedict[SymbolList[i]] - Days_5_Pricedict[SymbolList[i]]) * 100 / Days_5_Pricedict[SymbolList[i]];
        //            }

        //            AvgReturnYesterdayList.Add(yesterDayReturn);
        //            AvgReturn5dayList.Add(fiveDayReturn);
        //    }
        //    ShortLongAlertPerf AlertPerfObj=new ShortLongAlertPerf();
        //    AlertPerfObj.YesterdayPerf = Common.CalculateAvgReturn(AvgReturnYesterdayList).AvgReturn;
        //    AlertPerfObj.Day_5_Perf = Common.CalculateAvgReturn(AvgReturn5dayList).AvgReturn;
        //    AlertPerfObj.longShortId = alertType;

        //    perfList.Add(AlertPerfObj);
        //        return perfList;
        //}

       

        //private static Dictionary<string, double> GetAllPriceListForSymbol(List<string> symbolList,string dayIndicator)
        //{
            

        //    Dictionary<string, double> priceList = new Dictionary<string, double>();

        //    try
        //    {
        //        for (int i = 0; i < symbolList.Count; i++)
        //        {
        //            OdbcConnection con = new OdbcConnection(Constants.MyConString);
        //            OdbcCommand com = new OdbcCommand("SELECT symbol,CLOSE FROM symbolshistorical WHERE symbol ='" + symbolList[i]+"' AND DATE= (SELECT DATE FROM historicaldates WHERE DateType='" + dayIndicator + "')", con);

        //            try
        //            {
        //                con.Open();

        //                OdbcDataReader dr = com.ExecuteReader();
        //                log.Info("\nGetting Data For Symbol " + symbolList[i] + "\n");
        //                while (dr.Read())
        //                {
        //                    priceList.Add(dr.GetString(0), double.Parse(dr.GetString(1)));
        //                }


        //                con.Close();
        //            }
        //            catch (OdbcException ex)
        //            {
        //                throw ex;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
          
        //    return priceList;
        //}

        //private static List<string> GetLongShortSymbol(int LongShortId)
        //{
        //    List<string> symbolList = new List<string>();
        //    OdbcConnection con = new OdbcConnection(Constants.MyConString);

        //    OdbcCommand com = new OdbcCommand("SELECT symbol FROM longshortAlerts WHERE lngshrtid="+LongShortId, con);

        //    try
        //    {
        //        con.Open();

        //        OdbcDataReader dr = com.ExecuteReader();

        //        while (dr.Read())
        //        {
        //            symbolList.Add(dr.GetString(0));
        //        }
                

        //        con.Close();
        //    }
        //    catch (OdbcException ex)
        //    {
        //        throw ex;
        //    }
        //    return symbolList;
        //}
    }
}
