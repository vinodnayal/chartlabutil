using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModulusFE.TASDK;
using log4net.Config;

namespace FinLib
{
    public class TrendCalculation
    {



        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(TrendCalculation));

        public TrendObjects CalulateAllTrends(List<BarData> barlist, String symbol)
        {

            int MA_3days = 3;
            int MA_5days = 5;
            int MA_9days = 9;
            int MA_12days = 12;
            int MA_13days = 13;
            int MA_20days = 20;
            int MA_26days = 26;
            int MA_50days = 50;
            int MA_200days = 200;
            int count = barlist.Count;
            List<MovingAvg> ema200 = null;
            List<MovingAvg> ema50 = null;
            List<MovingAvg> ema3 = null;
            List<MovingAvg> ema5 = null;
            List<MovingAvg> ema9 = null;
            List<MovingAvg> ema12 = null;
            List<MovingAvg> ema13 = null;
            List<MovingAvg> ema20 = null;
            List<MovingAvg> ema26 = null;
            List<MovingAvg> MACDList = null;
            List<MovingAvg> SignalList = null;
            List<MovingAvg> RSIList = new List<MovingAvg>();

            TrendObjects trendObject = new TrendObjects();

            try
            {
                if (shouldCalculateShortTrend(count))
                {
                    ema3 = ExponentialMovingAverage(barlist, _ => _.close, MA_3days, MA_3days + " Days",symbol);
                    ema5 = ExponentialMovingAverage(barlist, _ => _.close, MA_5days, MA_5days + " Days",symbol);
                    ema9 = ExponentialMovingAverage(barlist, _ => _.close, MA_9days, MA_9days + " Days",symbol);
                    ema12 = ExponentialMovingAverage(barlist, _ => _.close, MA_12days, MA_12days + " Days",symbol);
                    ema13 = ExponentialMovingAverage(barlist, _ => _.close, MA_13days, MA_13days + " Days",symbol);
                    ema20 = ExponentialMovingAverage(barlist, _ => _.close, MA_20days, MA_20days + " Days",symbol);
                    ema26 = ExponentialMovingAverage(barlist, _ => _.close, MA_26days, MA_26days + " Days",symbol);

                }
                if (shouldCalculateMediumTrend(count))
                {
                    ema50 = ExponentialMovingAverage(barlist, _ => _.close, MA_50days, MA_50days + " Days", symbol);

                }
                if (shouldCalculateLongTrend(count))
                {
                    ema200 = ExponentialMovingAverage(barlist, _ => _.close, MA_200days, MA_200days + " Days", symbol);

                }


                if (ema12 != null && ema26 != null)
                {
                    MACDList = CalculateMACD(ema12, ema26);
                }
                if (MACDList != null)
                {
                    SignalList = SignalLineData(MACDList, _ => _.movAvg, MA_9days, MA_9days + " Days Signal Line");

                }
                FinCalculator finRSI = new FinCalculator();

                if (count >= 20)
                {
                    List<Field> dateRSIValues = finRSI.CalulateRSI(barlist);
                    Field rsis = dateRSIValues[1];


                    for (int i = 0; i < rsis.RecordCount; i++)
                    {
                        MovingAvg ma = new MovingAvg();
                        if (rsis.Value(i) == null)
                        {
                            ma.movAvg = 0;
                        }

                        else
                        {
                            ma.movAvg = (double)rsis.Value(i);
                        }
                        RSIList.Add(ma);
                    }
                }
                else
                {
                    MovingAvg ma = new MovingAvg();
                    ma.movAvg = 0;
                    RSIList.Add(ma);
                }


                List<Trend> trendList = new List<Trend>();

                for (int i = 26; i < barlist.Count; i++)
                {
                    Trend tr = new Trend();


                    tr.date = barlist[i].date;
                    if (ema3 != null)
                    {
                        tr.movAvg_3 = ema3[i].movAvg;
                    }
                    if (ema5 != null)
                    {
                        tr.movAvg_5 = ema5[i].movAvg;
                    }
                    if (ema12 != null)
                    {
                        tr.movAvg_12 = ema12[i].movAvg;
                    }
                    if (ema13 != null)
                    {
                        tr.movAvg_13 = ema13[i].movAvg;
                    }
                    if (ema9 != null)
                    {
                        tr.movAvg_9 = ema9[i].movAvg;
                    }
                    if (ema20 != null)
                    {
                        tr.movAvg_20 = ema20[i].movAvg;
                    }
                    if (ema26 != null)
                    {
                        tr.movAvg_26 = ema26[i].movAvg;
                    }
                    if (ema200 != null)
                    {
                        tr.movAvg_200 = ema200[i].movAvg;
                    }
                    if (ema50 != null)
                    {
                        tr.movAvg_50 = ema50[i].movAvg;
                    }
                    if (MACDList != null)
                    {
                        tr.MACD = MACDList[i].movAvg;
                    }
                    if (SignalList != null)
                    {
                        tr.SignalLine = SignalList[i].movAvg;
                    }
                    if (MACDList != null && SignalList != null)
                    {
                        tr.MACDSignaldiff = MACDList[i].movAvg - SignalList[i].movAvg;
                    }
                    if (RSIList != null)
                    {
                        tr.RSI = RSIList[i].movAvg;
                    }
                    tr.symbol = symbol;
                    trendList.Add(tr);
                }

                 trendObject = CalculateRating(trendList, count, symbol);
            }
            catch (Exception ex)
            {
                throw new Exception ("Issue in Calculating Trend for symbol: " +symbol);
                
            }
            //  return SeriesCrossFinder.Stdev(sma21);
            //  return trendList;
            return trendObject;
        }



        private List<MovingAvg> CalculateMACD(List<MovingAvg> MA_12Days, List<MovingAvg> MA_26Days)
        {

            List<MovingAvg> MACDList = new List<MovingAvg>();

            for (int i = 0; i < MA_12Days.Count; i++)
            {
                MovingAvg MACD = new MovingAvg();
                MACD.movAvg = MA_12Days[i].movAvg - MA_26Days[i].movAvg;
                MACDList.Add(MACD);
            }

            return MACDList;
        }

        public List<MovingAvg> ExponentialMovingAverage(List<BarData> bars, Func<BarData, double> fieldGetter, int periods, string Alias, String symbol)
        {
            double dPrime = 0;
            int iRecord;
            double dValue=0;


            List<Field> Results = new List<Field>();

            int iRecordCount = bars.Count;
            Field Field1 = new Field(iRecordCount, Alias);
            
            double dExp = 2.0 / (periods + 1);

            // To prime the EMA, get an average for the first n periods
            try
            {
                for (iRecord = 1; iRecord < periods + 1; iRecord++)
                    dPrime += fieldGetter(bars[iRecord]);
                dPrime /= periods;
                dValue = (fieldGetter(bars[iRecord]) * (1 - dExp)) + (dPrime * dExp);
            }
            catch (Exception ex)
            {

                log.Error("Error: in calculating ExponentialMovingAverage (get an average for the first n periods) for symbol: " + symbol + "\n" + ex);
                log.Error("Error: "+ex);
            }
            Field1.Value(periods, dValue);
            try
            {

                //Loop through each record in recordset
                for (iRecord = periods + 1; iRecord < iRecordCount; iRecord++)
                {

                    dValue = (Field1.ValueEx(iRecord - 1) * (1 - dExp)) + (fieldGetter(bars[iRecord]) * dExp);
                    Field1.Value(iRecord, dValue);

                }


            }
            catch (Exception ex)
            {
                log.Error("Error: in calculating ExponentialMovingAverage (Loop through each record in recordset) for symbol :" + symbol +"\n" +ex);
                log.Error("Error:" + ex);
            }
            Results.Add(Field1);

            Field movAvg = Results[0];
            List<MovingAvg> Results1 = new List<MovingAvg>();

            for (int i = 0; i < movAvg.RecordCount; i++)
            {
                MovingAvg ma = new MovingAvg();
                if (movAvg.Value(i) == null)
                {
                    ma.movAvg = 0;
                }

                else
                {
                    ma.movAvg = (double)movAvg.Value(i);
                }
                Results1.Add(ma);
            }

            return Results1;
        }



        public static List<MovingAvg> SignalLineData(List<MovingAvg> movAvgList, Func<MovingAvg, double> fieldGetter, int periods, string Alias)
        {
            double dPrime = 0;
            int iRecord;


            List<Field> Results = new List<Field>();

            int iRecordCount = movAvgList.Count;
            Field Field1 = new Field(iRecordCount, Alias);

            double dExp = 2.0 / (periods + 1);

            // To prime the EMA, get an average for the first n periods
            for (iRecord = 1; iRecord < periods + 1; iRecord++)
                dPrime += fieldGetter(movAvgList[iRecord]);
            dPrime /= periods;

            double dValue = (fieldGetter(movAvgList[iRecord]) * (1 - dExp)) + (dPrime * dExp);
            Field1.Value(periods, dValue);
            try
            {

                //Loop through each record in recordset
                for (iRecord = periods + 1; iRecord < iRecordCount; iRecord++)
                {

                    dValue = (Field1.ValueEx(iRecord - 1) * (1 - dExp)) + (fieldGetter(movAvgList[iRecord]) * dExp);
                    Field1.Value(iRecord, dValue);

                }


            }
            catch (Exception ex)
            {
                log.Error("Error:" + ex);
            }
            Results.Add(Field1);

            Field movAvg = Results[0];
            List<MovingAvg> Results1 = new List<MovingAvg>();

            for (int i = 0; i < movAvg.RecordCount; i++)
            {
                MovingAvg ma = new MovingAvg();
                if (movAvg.Value(i) == null)
                {
                    ma.movAvg = 0;
                }

                else
                {
                    ma.movAvg = (double)movAvg.Value(i);
                }
                Results1.Add(ma);
            }

            return Results1;
        }

        private bool shouldCalculateShortTrend(int count)
        {
            if (count >= 30)
            {
                return true;
            }
            return false;

        }
        private bool shouldCalculateMediumTrend(int count)
        {
            if (count >= 60)
            {
                return true;
            }
            return false;

        }
        private bool shouldCalculateLongTrend(int count)
        {
            if (count >=200)
            {
                return true;
            }
            return false;

        }

        public TrendObjects CalculateRating(List<Trend> trendList, int barCount,string symbol)
        {


            int RowCount = trendList.Count();
            TrendObjects TrendShortTerm = new TrendObjects();
            TrendEnum mediumTermTrend = new TrendEnum();
            TrendEnum longTermTrend = new TrendEnum();

            if (shouldCalculateShortTrend(barCount))
            {
                TrendShortTerm = ShortTermTrendCalculation(trendList);
            }

            for (int i = 0; i < RowCount; i++)
            {
                trendList[i].Rating = 0.0;
            }
            if (shouldCalculateMediumTrend(barCount))
            {
                mediumTermTrend = MediumTermTrendCalculation(trendList);
            }
            for (int i = 0; i < RowCount; i++)
            {
                trendList[i].Rating = 0.0;
            }
            if (shouldCalculateLongTrend(barCount))
            {
                longTermTrend = LongTermTrendCalculation(trendList);
            }

            TrendObjects TrendUpdateRow = new TrendObjects();
            
                TrendUpdateRow.shortTermTrend = (int)(TrendShortTerm.shortTermTrend);
                TrendUpdateRow.mediumTermTrend = (int)(mediumTermTrend);
                TrendUpdateRow.longTermTrend = (int)(longTermTrend);
            if (TrendShortTerm.shortTermTrendDate != null)
            {
                TrendUpdateRow.shortTermTrendDate = TrendShortTerm.shortTermTrendDate;
            }
            TrendUpdateRow.symbol = symbol;



            return TrendUpdateRow;
        }

        private TrendObjects ShortTermTrendCalculation(List<Trend> trendList)
        {

            int TrendListCount = trendList.Count();

            try
            {
                for (int i = TrendListCount - 1; i > TrendListCount - 3; i--)
                {

                    Trend currentDay = trendList[i];
                    Trend previousDay = trendList[i - 1];
                    Trend thirdPreviousDay = trendList[i - 2];

                    if (currentDay.MACDSignaldiff > 0)
                    {
                        currentDay.Rating += 0.5;
                    }
                    else
                    {
                        currentDay.Rating -= 0.5;
                    }

                    if (currentDay.MACD > 0)
                    {
                        currentDay.Rating += 0.5;
                    }
                    else
                    {
                        currentDay.Rating -= 0.5;
                    }


                    if (currentDay.RSI > 50)
                    {
                        currentDay.Rating += 0.5;
                    }
                    else
                    {
                        currentDay.Rating -= 0.5;
                    }

                    if (currentDay.RSI > 70 && previousDay.RSI > 70 && thirdPreviousDay.RSI > 70)
                    {
                        currentDay.Rating += 0.5;
                    }
                    else if (currentDay.RSI < 30 && previousDay.RSI < 30 && thirdPreviousDay.RSI < 30)
                    {
                        currentDay.Rating -= 0.5;
                    }
                    else
                    {
                        currentDay.Rating += 0.0;
                    }

                    if (currentDay.movAvg_3 > currentDay.movAvg_9 && previousDay.movAvg_3 > previousDay.movAvg_9)
                    {
                        currentDay.Rating += 0.5;
                    }
                    else if (currentDay.movAvg_3 < currentDay.movAvg_9 && previousDay.movAvg_3 < previousDay.movAvg_9)
                    {
                        currentDay.Rating -= 0.5;
                    }
                    else
                    {
                        currentDay.Rating += 0.0;
                    }

                    if (currentDay.movAvg_5 > currentDay.movAvg_13 && previousDay.movAvg_5 > previousDay.movAvg_13)
                    {
                        currentDay.Rating += 0.5;
                    }
                    else if (currentDay.movAvg_5 < currentDay.movAvg_13 && previousDay.movAvg_5 < previousDay.movAvg_13)
                    {
                        currentDay.Rating -= 0.5;
                    }
                    else
                    {
                        currentDay.Rating += 0.0;
                    }

                    if (currentDay.movAvg_5 > currentDay.movAvg_20 && previousDay.movAvg_5 > previousDay.movAvg_20)
                    {
                        currentDay.Rating += 0.5;
                    }
                    else if (currentDay.movAvg_5 < currentDay.movAvg_20 && previousDay.movAvg_5 < previousDay.movAvg_20)
                    {
                        currentDay.Rating -= 0.5;
                    }
                    else
                    {
                        currentDay.Rating += 0.0;
                    }

                }

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return CalculateShortTermTrend(trendList);

        }





        private TrendEnum MediumTermTrendCalculation(List<Trend> trendList)
        {

            int TrendListCount = trendList.Count();
            Trend LastDayTrend = trendList[TrendListCount - 1];
            try
            {
                for (int i = TrendListCount - 1; i > TrendListCount - 3; i--)
                {
                    Trend currentDay = trendList[i];
                    Trend previousDay = trendList[i - 1];

                    if (currentDay.movAvg_50 == 0)
                    {
                        return 0;
                    }


                    if (currentDay.movAvg_13 > currentDay.movAvg_20 && previousDay.movAvg_13 > previousDay.movAvg_20)
                    {
                        currentDay.Rating += 0.5;
                    }
                    else if (currentDay.movAvg_13 < currentDay.movAvg_20 && previousDay.movAvg_13 < previousDay.movAvg_20)
                    {
                        currentDay.Rating -= 0.5;
                    }
                    else
                    {
                        currentDay.Rating += 0.0;
                    }

                    if (currentDay.movAvg_13 > currentDay.movAvg_50 && previousDay.movAvg_13 > previousDay.movAvg_50)
                    {
                        currentDay.Rating += 0.5;
                    }
                    else if (currentDay.movAvg_13 < currentDay.movAvg_50 && previousDay.movAvg_13 < previousDay.movAvg_50)
                    {
                        currentDay.Rating -= 0.5;
                    }
                    else
                    {
                        currentDay.Rating += 0.0;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }



            return GetMediumLongTermTrendByRating(LastDayTrend.Rating);
        }

        private TrendEnum LongTermTrendCalculation(List<Trend> trendList)
        {

            int TrendListCount = trendList.Count();
            Trend LastDayTrend = trendList[TrendListCount - 1];
            try
            {
                for (int i = TrendListCount - 1; i > TrendListCount - 3; i--)
                {

                    Trend currentDay = trendList[i];
                    Trend previousDay = trendList[i - 1];
                    if (currentDay.movAvg_50 == 0 || currentDay.movAvg_200 == 0)
                    {
                        return 0;
                    }



                    if (currentDay.movAvg_13 > currentDay.movAvg_20 && previousDay.movAvg_13 > previousDay.movAvg_20)
                    {
                        currentDay.Rating += 0.5;
                    }
                    else if (currentDay.movAvg_13 < currentDay.movAvg_20 && previousDay.movAvg_13 < previousDay.movAvg_20)
                    {
                        currentDay.Rating -= 0.5;
                    }
                    else
                    {
                        currentDay.Rating += 0.0;
                    }

                    if (currentDay.movAvg_50 > currentDay.movAvg_200 && previousDay.movAvg_50 > previousDay.movAvg_200)
                    {
                        currentDay.Rating += 0.5;
                    }
                    else if (currentDay.movAvg_50 < currentDay.movAvg_200 && previousDay.movAvg_50 < previousDay.movAvg_200)
                    {
                        currentDay.Rating -= 0.5;
                    }
                    else
                    {
                        currentDay.Rating += 0.0;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }


            return GetMediumLongTermTrendByRating(LastDayTrend.Rating);

        }

        private TrendObjects CalculateShortTermTrend(List<Trend> trendList)
        {
            TrendObjects TrendUpdateRow = new TrendObjects();
            int TrendListCount = trendList.Count();
            Trend LastDayTrend = trendList[TrendListCount - 1];
            try
            {
                for (int i = TrendListCount - 1; i > 0; i--)
                {


                    trendList[i].shortTermTrend = GetShortTermTrendByRating(trendList[i].Rating);


                    if (i <= TrendListCount - 2)
                    {

                        if (LastDayTrend.shortTermTrend != trendList[i].shortTermTrend)
                        {
                            TrendUpdateRow.shortTermTrendDate = trendList[i].date;
                            TrendUpdateRow.shortTermTrend = (int)(LastDayTrend.shortTermTrend);
                            //LastDayTrend.shortTermTrendDate = trendList[i].date;
                            break;
                        }


                    }


                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }


            return TrendUpdateRow;
        }


        private TrendEnum GetShortTermTrendByRating(double rating)
        {



            if (rating > 0 && rating <= 2)
            {
                return TrendEnum.Bullish;

            }
            else if (rating > 2)
            {
                return TrendEnum.VeryBullish;

            }
            else if (rating < 0 && rating >= -2)
            {
                return TrendEnum.Bearish;

            }
            else if (rating < -2)
            {
                return TrendEnum.VeryBearish;

            }
            else
            {
                return TrendEnum.Neutral;
            }


        }

        private TrendEnum GetMediumLongTermTrendByRating(double rating)
        {



            if (rating > 0 && rating <= 0.5)
            {
                return TrendEnum.Bullish;

            }
            else if (rating >= 1)
            {
                return TrendEnum.VeryBullish;

            }
            else if (rating >= -0.5 && rating < 0)
            {
                return TrendEnum.Bearish;

            }
            else if (rating >= -1.0 && rating < -0.5)
            {
                return TrendEnum.VeryBearish;

            }
            else
            {
                return TrendEnum.Neutral;
            }

        }

    }
}
