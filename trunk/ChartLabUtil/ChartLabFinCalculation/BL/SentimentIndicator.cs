using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using FinLib;

namespace ChartLabFinCalculation
{
    class SentimentIndicator
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(CSVToDB));

        public static void updateSentimentIndicatorRating()
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            OdbcCommand updateLTIRatingCommand = new OdbcCommand("UPDATE historysentimentindicators SET LTIrating = CASE " +
                                                            "WHEN LTI=0 THEN 0 " +
                                                            "WHEN (LTI>0 AND LTI<30) THEN 4 " +
                                                            "WHEN LTI=30 THEN 5 " +
                                                            "WHEN (LTI>30 AND LTI<=50) THEN 6 " +
                                                            "WHEN (LTI>50 AND LTI<70) THEN 1 " +
                                                            "WHEN LTI=70 THEN 2 " +
                                                            "WHEN LTI>70 THEN 3 " +
                                                            "ELSE 0 END ", con);



            OdbcCommand updateSTIRatingCommand = new OdbcCommand("UPDATE historysentimentindicators SET STIrating = CASE " +
                                                            "WHEN STI=0 THEN 0 " +
                                                            "WHEN (STI>0 AND STI<30) THEN 4 " +
                                                            "WHEN STI=30 THEN 5 " +
                                                            "WHEN (STI>30 AND STI<=50) THEN 6 " +
                                                            "WHEN (STI>50 AND STI<70) THEN 1 " +
                                                            "WHEN STI=70 THEN 2 " +
                                                            "WHEN STI>70 THEN 3 " +
                                                            "ELSE 0 END ", con);

            OdbcCommand updateBIRatingCommand = new OdbcCommand("UPDATE historysentimentindicators SET BIrating = CASE " +
                                                            "WHEN BI=0 THEN 0 " +
                                                            "WHEN (BI>0 AND BI<30) THEN 4 " +
                                                            "WHEN BI=30 THEN 5 " +
                                                            "WHEN (BI>30 AND BI<=50) THEN 6 " +
                                                            "WHEN (BI>50 AND BI<70) THEN 1 " +
                                                            "WHEN BI=70 THEN 2 " +
                                                            "WHEN BI>70 THEN 3 " +
                                                            "ELSE 0 END ", con);

            try
            {
                con.Open();

                updateLTIRatingCommand.ExecuteReader();
                updateSTIRatingCommand.ExecuteReader();
                updateBIRatingCommand.ExecuteReader();
                log.Info("Sentiment Indicator Rating Updated...");

                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }
        }

        public static void CalculateAndSaveSentimentPerformance(string SentimentIndicatorPerfPath)
        {
            List<SymbolNameId> symbolList = DataDownloader.GetSymbolListForSentimentAlert();
            List<DayWiseAvgReturnForSentimentSymbol> FinalAvgReturnList = new List<DayWiseAvgReturnForSentimentSymbol>();

            for (int i = 0; i < symbolList.Count; i++)
            {
                string symbol = symbolList[i].SentimentSymbol;
                Dictionary<DateTime, double> dict = BuySellRatingDAO.GetDataForDateAlertFromDB(symbol);
                List<DateForSymbolAlert> sentimentLTIAlertHistory = DataDownloader.GetSentimentAlertForSymbol(1, symbol);
                List<DayWiseAvgReturnForSentimentSymbol> returnListLTI = GetAverageSentimentSymbolReturn(dict, sentimentLTIAlertHistory, symbolList[i].SentimentSymbolId, 1);
                List<DateForSymbolAlert> sentimentSTIAlertHistory = DataDownloader.GetSentimentAlertForSymbol(2, symbol);
                List<DayWiseAvgReturnForSentimentSymbol> returnListSTI = GetAverageSentimentSymbolReturn(dict, sentimentLTIAlertHistory, symbolList[i].SentimentSymbolId, 2);
                List<DateForSymbolAlert> sentimentBIAlertHistory = DataDownloader.GetSentimentAlertForSymbol(3, symbol);
                List<DayWiseAvgReturnForSentimentSymbol> returnListBI = GetAverageSentimentSymbolReturn(dict, sentimentLTIAlertHistory, symbolList[i].SentimentSymbolId, 3);
                FinalAvgReturnList.AddRange(returnListLTI);
                FinalAvgReturnList.AddRange(returnListSTI);
                FinalAvgReturnList.AddRange(returnListBI);
            }
            CSVExporter.WriteToCSVSentimentIndicatorPerf(FinalAvgReturnList, SentimentIndicatorPerfPath + "/SentimentIndicatorPerfFile.csv");
            CSVToDB.UpdateSentimentIndicatorPerf(SentimentIndicatorPerfPath);

        }

        private static List<DayWiseAvgReturnForSentimentSymbol> GetAverageSentimentSymbolReturn(Dictionary<DateTime, double> dict, List<DateForSymbolAlert> sentimentIndicatorAlertList, int symbolId, int Indicator)
        {
            List<DayWiseAvgReturnForSentimentSymbol> AvgReturnList = new List<DayWiseAvgReturnForSentimentSymbol>();
            List<double> AvgReturnList_Weekly = new List<double>();
            List<double> AvgReturnList_1_Month = new List<double>();
            List<double> AvgReturnList_2_Month = new List<double>();

            DayWiseAvgReturnForSentimentSymbol dayWiseAvgRetObj = new DayWiseAvgReturnForSentimentSymbol();

            if (sentimentIndicatorAlertList.Count != 0)
            {
                for (int i = 0; i < sentimentIndicatorAlertList.Count; i++)
                {
                    DateTime date = sentimentIndicatorAlertList[i].ChangeDate;
                    if (date < DateTime.Now.AddDays(-1))
                    {
                        double todayPrice = Common.GetDayWiseReturn(dict, date, 0, true);
                        double sevendaysPrice = Common.GetDayWiseReturn(dict, date, 7, true);
                        double thirtydaysPrice = Common.GetDayWiseReturn(dict, date, 30, true);
                        double sixtydaysPrice = Common.GetDayWiseReturn(dict, date, 60, true);


                        if (todayPrice != 0 && sevendaysPrice != 0)
                        {
                            AvgReturnList_Weekly.Add(100 * (sevendaysPrice - todayPrice) / todayPrice);
                        }
                        if (todayPrice != 0 && thirtydaysPrice != 0)
                        {
                            AvgReturnList_1_Month.Add(100 * (thirtydaysPrice - todayPrice) / todayPrice);
                        }
                        if (todayPrice != 0 && sixtydaysPrice != 0)
                        {
                            AvgReturnList_2_Month.Add(100 * (sixtydaysPrice - todayPrice) / todayPrice);
                        }


                    }

                }

                dayWiseAvgRetObj.AvgReturn_Weekly = Common.CalculateAvgReturn(AvgReturnList_Weekly).AvgReturn;
                dayWiseAvgRetObj.AvgReturn_1_Month = Common.CalculateAvgReturn(AvgReturnList_1_Month).AvgReturn;
                dayWiseAvgRetObj.AvgReturn_2_Month = Common.CalculateAvgReturn(AvgReturnList_2_Month).AvgReturn;

                dayWiseAvgRetObj.symbolId = symbolId;
                dayWiseAvgRetObj.Indicator = Indicator;
                AvgReturnList.Add(dayWiseAvgRetObj);


            }
            else
            {
                dayWiseAvgRetObj.AvgReturn_Weekly = 0;
                dayWiseAvgRetObj.AvgReturn_1_Month = 0;
                dayWiseAvgRetObj.AvgReturn_2_Month = 0;
                dayWiseAvgRetObj.symbolId = symbolId;
                dayWiseAvgRetObj.Indicator = Indicator;
                AvgReturnList.Add(dayWiseAvgRetObj);
            }

            return AvgReturnList;
        }
    }
}
