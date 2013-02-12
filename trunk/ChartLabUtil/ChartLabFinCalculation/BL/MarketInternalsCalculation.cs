using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinLib;

namespace ChartLabFinCalculation
{
    class MarketInternalsCalculation
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(MarketInternalsCalculation));

        public static void CalculateMarketInternals()
        {

            try
            {

                FinCalculator fincalc = new FinCalculator();
                DateTime fromdate = DateTime.Now.AddDays(-Constants.HIST_DAYS_LENGTH_FOR_SA);
                DateTime toDate = DateTime.Now;
                List<HistoricalDates> listHistoricalDates = CommonDAO.getHistoricalDatesFromDB();
                log.Info("Calculating OB and OS count for S&P symbols --- ");
                List<DateOBOSCount> obosCountList = MarketInternalsDAO.getOBOSCountFromDB(listHistoricalDates);
                int weeklyOSCount = 0;
                int weeklyOBCount = 0;
                int currentOSCount = 0;
                int currentOBCount = 0;
                if (obosCountList.Count > 1)
                {
                    weeklyOSCount = obosCountList[0].osCount;
                    weeklyOBCount = obosCountList[0].obCount;
                    currentOSCount = obosCountList[1].osCount;
                    currentOBCount = obosCountList[1].obCount;
                }
                List<HistoricalDates> weeklyDate = listHistoricalDates.Where(x => x.dateType.Equals("weekly", StringComparison.OrdinalIgnoreCase)).ToList();



                log.Info("Calculating % positive time for S&P symbols --- ");
                List<String> topOS_Symbols = MarketInternalsDAO.GetSnPTopOSSymbols();
                List<String> topOB_Symbols = MarketInternalsDAO.GetSnPTopOBSymbols();
                calculatePerPositiveTime(topOS_Symbols, "oSIndexTopSymbols");
                calculatePerPositiveTime(topOB_Symbols, "oBIndexTopSymbols");


                log.Info("Calculating 50days MA for S&P --- ");

                String symbol = Constants.GSPCSymbol;

                List<BarData> barlist = SymbolHistoricalMongoDAO.GetHistoricalDataFromMongo(fromdate, toDate, symbol);

                double currentAbove50dayMA = 0;
                double weeklyAbove50dayMA = 0;

                if (barlist != null || barlist.Count != 0)
                {

                    List<double> Above50daysMA = fincalc.calculate50DaysMA(barlist, listHistoricalDates);

                    if (Above50daysMA.Count >1)
                    {
                        currentAbove50dayMA = Above50daysMA[0];
                        weeklyAbove50dayMA = Above50daysMA[1];

                    }

                }
                else
                {
                    log.Info("Empty List Returned From Provider" + symbol);


                }
                log.Info("Calculating 10 day A/D line for S&P symbols --- ");
                List<String> symbolList = OBOSRatingDAO.GetSnPSymbols();

                List<DateADCount> listDateADCount = calculate10DaysADLine(symbolList);
                List<DateADCount> DateADCountRecord = listDateADCount.Where(x => x.date.Equals(weeklyDate[0].date)).ToList();
               //Confusion
                
                
                float currentADLine10Days = 0;
                try
                {
                    for (int i = listDateADCount.Count - 1; i >= listDateADCount.Count - 10; i--)
                    {
                        currentADLine10Days += listDateADCount[i].ADDiff;

                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }
                currentADLine10Days = currentADLine10Days / 10;
                int count = 1;
                float weeklyADLine10Days = 0;

                try
                {    
                    if(DateADCountRecord.Count>0)
                   {
                    int index = listDateADCount.IndexOf(DateADCountRecord[0]);
                
                        //Confusion
                        for (int j = index; j > index - 10; j--)
                        {
                            weeklyADLine10Days = listDateADCount[j].ADDiff;
                            count++;
                        }
                    }
                }
                catch (Exception ex)
                {
                   log.Error("Error in weeklyADLine10Days calculations \n"+ex);;
                }
                weeklyADLine10Days = weeklyADLine10Days / 10;

                MarketInternal marketint = new MarketInternal();
                marketint.weeklyOSCount = weeklyOSCount;
                marketint.weeklyOBCount = weeklyOBCount;
                marketint.currentOSCount = currentOSCount;
                marketint.currentOBCount = currentOBCount;
                marketint.weeklyADLine10Days = weeklyADLine10Days;
                marketint.currentADLine10Days = currentADLine10Days;
                marketint.currentAbove50dayMA = currentAbove50dayMA;
                marketint.weeklyAbove50dayMA = weeklyAbove50dayMA;
                MarketInternalsDAO.writeMarketIntToDB(marketint);

            }
            catch (Exception ex)
            {
                log.Error("Error in market internal calculations \n"+ex);
            }
        }


        private static void calculatePerPositiveTime(List<String> symbolList, String tableName)
        {
           
            FinCalculator fincalc = new FinCalculator();
            DateTime date20days = DateTime.Now.AddDays(-20);
            DateTime toDate = DateTime.Now;

            try
            {
                foreach (string symbol in symbolList)
                {

                    List<BarData> barlist = null;
                    try
                    {
                        barlist = SymbolHistoricalMongoDAO.GetHistoricalDataFromMongo(date20days, toDate, symbol);

                    }

                    catch (Exception ex)
                    {

                        log.Error(ex);
                    }
                    if (barlist == null || barlist.Count == 0)
                    {

                        log.Info("Empty List Returned From Provider" + symbol);
                    }
                    else
                    {
                        double perPositiveTime = fincalc.calPerPositiveTime(barlist);
                        MarketInternalsDAO.UpdateMarketInternals(tableName, perPositiveTime, symbol);
                        
                    }

                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

        }

        private static List<DateADCount> calculate10DaysADLine(List<String> symbolList)
        {
            List<DateADCount> listDateADCount = new List<DateADCount>();
            try
            {
                FinCalculator fincalc = new FinCalculator();
                int count = 0;
                DateTime fromdateAD = DateTime.Now.AddDays(-30);
                DateTime toDate = DateTime.Now;

                foreach (String symbol in symbolList)
                {


                    log.Info("Calculating for symbol=" + symbol);
                    List<BarData> barlist = null;
                    try
                    {
                        barlist = SymbolHistoricalMongoDAO.GetHistoricalDataFromMongo(fromdateAD, toDate, symbol);
                    }
                    catch (Exception ex)
                    {

                        log.Error(ex);
                    }
                    if (barlist == null || barlist.Count == 0)
                    {

                        log.Info("Empty List Returned From Provider" + symbol);
                    }
                    else
                    {

                        List<DateAD> listdateAD = fincalc.CalculateADForRange(barlist);
                        if (count == 0)
                            listDateADCount = fincalc.CalDateADCount(listdateAD);
                        else
                            listDateADCount = fincalc.CalDateADCountNext(listdateAD, listDateADCount);
                        count++;
                    }


                }

                return listDateADCount;
            }
            catch (Exception ex)
            {
                log.Info(ex);
                return listDateADCount;

            }


        }

    }
}
