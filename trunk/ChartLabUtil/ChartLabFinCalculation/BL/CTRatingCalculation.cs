using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinLib;
using System.IO;

namespace ChartLabFinCalculation
{
    public class CTRatingCalculation
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(Program));
        public static string CTRatingChangeHistCsvFilePath;

        public static List<CTRating> CalculateCTRating(string BuySellRatingFilePath)
        {
            DirectoryInfo diRating = new DirectoryInfo(BuySellRatingFilePath);
            FileInfo[] fileEntries = diRating.GetFiles();
            List<CTRating> ctRatingList = new List<CTRating>();
            Dictionary<string, double> S1_Dict = new Dictionary<string, double>();
            Dictionary<string, double> S2_Dict = new Dictionary<string, double>();
            foreach (FileInfo file in fileEntries)
            {
                if (file.ToString().Contains("S1") || file.ToString().Contains("S2"))
                {
                    StreamReader readFile = new StreamReader(BuySellRatingFilePath + "\\" + file);


                    string line;
                    string[] row;
                    int flag = 0;

                    while ((line = readFile.ReadLine()) != null)
                    {
                        string symbol = null;
                        double ratingValue = 0;
                        row = line.Split(' ');
                        for (int i = 1; i < row.Length; i++)
                        {

                            if (row[i] != "" && flag == 0)
                            {

                                symbol = row[i];
                                flag = 1;
                            }
                            else if (row[i] != "" && flag == 1)
                            {
                                if (FileContainsS1(file))
                                {
                                    ratingValue = double.Parse(row[i]) + double.Parse(row[i]) * 0.015;
                                }
                                else
                                {
                                    ratingValue = double.Parse(row[i]);
                                }
                                flag = 0;
                            }
                            if (i == row.Length - 1)
                            {
                                if (FileContainsS1(file))
                                {
                                    S1_Dict.Add(symbol, ratingValue);
                                }
                                else
                                {
                                    S2_Dict.Add(symbol, ratingValue);
                                }
                            }

                        }
                    }
                }


            }
            foreach (KeyValuePair<string, double> pair in S1_Dict)
            {
                if (S2_Dict.ContainsKey(pair.Key))
                {
                    log.Info("\nCalculating Counter Trend Rating for symbol: " + pair.Key);
                    CTRating ctRatingObj = new CTRating();
                    ctRatingObj.symbol = pair.Key;
                    ctRatingObj.ctRatingValue = (pair.Value + S2_Dict[pair.Key]) * 100 / 2;
                    CTRatingEnum rating = calculateCTRatingEnum(ctRatingObj.ctRatingValue);
                    ctRatingObj.ctRating = (int)rating;
                    ctRatingList.Add(ctRatingObj);
                }
                else
                {
                    log.Warn(pair.Key + " does not exists in S2 File");
                }
            }
            return ctRatingList;
        }


        private static bool FileContainsS1(FileInfo file)
        {
            if (file.ToString().Contains("S1"))
            {
                return true;
            }
            return false;
        }

        public static CTRatingEnum calculateCTRatingEnum(double p)
        {
            CTRatingEnum r;
            if (p > 55 && p <= 100)
            {
                r = CTRatingEnum.ExtremelyOversold;
            }
            else if (p > 30 && p <= 54)
            {
                r = CTRatingEnum.Oversold;
            }
            else if (p > 19 && p <= 29)
            {
                r = CTRatingEnum.ApproachingOversold;
            }
            else if (p > -49 && p <= 20)
            {
                r = CTRatingEnum.Neutral;
            }
            else if (p > -69 && p <= -50)
            {
                r = CTRatingEnum.ApproachingOverbought;
            }
            else if (p > -89 && p <= -70)
            {
                r = CTRatingEnum.Overbought;
            }
            else if (p > -100 && p <= -90)
            {
                r = CTRatingEnum.ExtremelyOverbought;
            }
            else
            {
                r = CTRatingEnum.Neutral;
            }
            return r;
        }



        public static void CalculateCTRatingPerf(string CTRatingPerfFolderPath)
        {
            Dictionary<DateTime, double> dict = BuySellRatingDAO.GetDataForDateAlertFromDB(Constants.SnPSymbol);
            List<DateForSymbolAlert> CTRatingHistory = BuySellRatingDAO.GetCTRatingForSymbol();
            List<DayWiseAvgReturnForCTRating> returnListCTRating = GetAverageCTRatingReturn(dict, CTRatingHistory);
            CSVExporter.WriteToCSVCTRatingPerf(returnListCTRating, CTRatingPerfFolderPath + "/CTRatingPerf.csv");
            BuySellRatingDAO.UpdateCTRatingPerf(CTRatingPerfFolderPath);
        }

        private static List<DayWiseAvgReturnForCTRating> GetAverageCTRatingReturn(Dictionary<DateTime, double> dict, List<DateForSymbolAlert> CTRatingHistoryList)
        {
            List<DayWiseAvgReturnForCTRating> avgReturnList = new List<DayWiseAvgReturnForCTRating>();

            List<double> AvgReturnList_Weekly = new List<double>();
            List<double> AvgReturnList_1_Month = new List<double>();
            List<double> AvgReturnList_2_Month = new List<double>();

            DayWiseAvgReturnForCTRating dayWiseAvgRetObj = new DayWiseAvgReturnForCTRating();

            if (CTRatingHistoryList.Count != 0)
            {
                for (int i = 0; i < CTRatingHistoryList.Count; i++)
                {
                    DateTime date = CTRatingHistoryList[i].ChangeDate;
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
                dayWiseAvgRetObj.Symbol = Constants.SnPSymbol;
                avgReturnList.Add(dayWiseAvgRetObj);


            }
            else
            {
                dayWiseAvgRetObj.AvgReturn_Weekly = 0;
                dayWiseAvgRetObj.AvgReturn_1_Month = 0;
                dayWiseAvgRetObj.AvgReturn_2_Month = 0;
                dayWiseAvgRetObj.Symbol = Constants.SnPSymbol;
                avgReturnList.Add(dayWiseAvgRetObj);
            }


            return avgReturnList;
        }


        public static void CTRatingChangeHistory()
        {

            List<CTRatingHistory> historyBuySellRatingList = BuySellRatingDAO.getCTRatingHistroyFromDB();
            List<BuySellRatingChangeHist> ChangeBuySellRatingHist =getCtRatingChangeList(historyBuySellRatingList);
            CSVExporter.WriteToCSVChangeRatingHistory(ChangeBuySellRatingHist, CTRatingChangeHistCsvFilePath + "/ChangeCTRatingHistoryFile.csv");
            BuySellRatingDAO.InsertChangeCTRatingHistoryCSVToDB(CTRatingChangeHistCsvFilePath, "ctratingchangehistory");

        }
        public static List<BuySellRatingChangeHist> getCtRatingChangeList(List<CTRatingHistory> historyBuySellRatingList)
        {
            List<BuySellRatingChangeHist> ChangeBuySellRatingHist = new List<BuySellRatingChangeHist>();
            try
            {
                
                for (int i = 1; i < historyBuySellRatingList.Count; i++)
                {
                    int prevRating = historyBuySellRatingList[i - 1].ctRating;
                    int currentRating = historyBuySellRatingList[i].ctRating;
                    string prevSymbol = historyBuySellRatingList[i - 1].symbol;
                    string currentSymbol = historyBuySellRatingList[i].symbol;
                    if (currentSymbol.Equals(prevSymbol))
                    {
                        if ((prevRating - currentRating) != 0)
                        {
                            ChangeBuySellRatingHist.Add(
                                new BuySellRatingChangeHist
                                {
                                    symbol = historyBuySellRatingList[i].symbol,
                                    newRating = currentRating,
                                    oldRating = prevRating,
                                    ratingDate = historyBuySellRatingList[i].Date


                                });
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }

            return ChangeBuySellRatingHist;
        }
    }
}
