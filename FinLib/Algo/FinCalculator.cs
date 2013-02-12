using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModulusFE.TASDK;
using FinLib;
using System.Net;
using System.IO;
using System.Globalization;

namespace FinLib
{
    public class FinCalculator
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(FinCalculator));
          private static readonly CultureInfo ciUS = new CultureInfo("en-us");
          

          public SymbolAnalytics CalculateAnalytics(String symbol, List<BarData> barlist, List<HistoricalDates> listHistoricalDates)
          {
              SymbolAnalytics symbolAnalytics = new SymbolAnalytics();
              try
              {

                  CalulatePriceToDate(barlist, listHistoricalDates, symbolAnalytics);
                  CalulateSupAndResi(barlist, symbolAnalytics);

                  
                  symbolAnalytics.STD50Days = CalulateSTD50Days(barlist);
                  symbolAnalytics.STD21Days = CalulateSTD21Days(barlist);

                  if (barlist.Count >= 20)
                  {
                      List<Field> dateRSIValues = CalulateRSI(barlist);


                      if (dateRSIValues != null)
                      {

                          Field dates = dateRSIValues[0];
                          Field rsis = dateRSIValues[1];
                          if (dates != null && rsis != null)
                          {
                              List<HistoricalDates> weeklyDate = listHistoricalDates.Where(x => x.dateType.Equals("weekly", StringComparison.OrdinalIgnoreCase)).ToList();
                              double weeklyRSI = 0;
                              for (int i = dates.RecordCount - 1; i > dates.RecordCount - 6; i--)
                              {
                                  DateTime rsiDate = Convert.ToDateTime(dates.ValueStr(i));
                                  if (weeklyDate[0].date == rsiDate)
                                  {
                                      weeklyRSI = (double)rsis.Value(i);

                                  }

                              }
                              symbolAnalytics.weeklyRSI = weeklyRSI;
                              symbolAnalytics.currentRSI = (double)rsis.Value(rsis.RecordCount - 1); 
                              symbolAnalytics.OBOSWeekly = (int)CalculateOBOSforRSI(weeklyRSI);
                              symbolAnalytics.OBOSCurrent = (int)CalculateOBOSforRSI(symbolAnalytics.currentRSI);
                          }


                      }
                  }
                  else
                  {
                              symbolAnalytics.weeklyRSI = 45;
                              symbolAnalytics.currentRSI =45 ;
                              symbolAnalytics.OBOSWeekly = (int)CalculateOBOSforRSI(symbolAnalytics.weeklyRSI);
                              symbolAnalytics.OBOSCurrent = (int)CalculateOBOSforRSI(symbolAnalytics.currentRSI);
                  }
                  List<double> _52weekRange = Calulate52WeekRange(symbol);
                  if (_52weekRange != null & _52weekRange.Count!=0)
                  {
                      symbolAnalytics.high52WeekRange = _52weekRange[0];
                      symbolAnalytics.low52WeekRange = _52weekRange[1];
                  }
              }
              
             
            catch (Exception ex)
              { log.Error(ex); }
              return symbolAnalytics;
          }

         

        private List<double> Calulate52WeekRange(string symbol)
        {
            List<double> _52weekRange = null;
            string url = string.Format("http://finance.yahoo.com/d/quotes.csv?s="+symbol+"&f=sl1c1p2kj");
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Proxy = null;
                using (WebResponse response = request.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        if (stream == null)
                        {

                        }

                        using (StreamReader sr = new StreamReader(stream))
                        {
                            string data = sr.ReadToEnd();
                            string[] rows = data.Split('\n');
                            _52weekRange = new List<double>();
                            foreach (string row in rows)
                            {
                                string[] values = row.Split(',');
                                if (values[0] != "" & values.Count() > 1)
                                {
                                    double high=0;
                                    double low=0;
                                    double.TryParse(values[4], out high);
                                    double.TryParse(values[5], out low);
                                    //if(
                                    _52weekRange.Add(high);
                                    _52weekRange.Add(low);
                                }   
                               
                            }


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error while getting data for 52 week range for symbol "+symbol);
                log.Error(ex);
            }
            return _52weekRange;
        }

        private obosEnum CalculateOBOSforRSI(double RSI)
        {
            obosEnum OBOS ;
            
            if (RSI <= 30)
            {
                OBOS = obosEnum.OS;

            }
            else if (RSI >= 70)
            {

                OBOS = obosEnum.OB;
            }
            else
            {

                OBOS = obosEnum.Nuetral;
            }
            return OBOS;
        }

        private double CalulateSTD50Days(List<BarData> barlist)
        {
            int rowCount = barlist.Count;
            //CalulateEMA(barlist, 51);
            //new check
            if (barlist.Count >= 51)
            {
                List<SeriesData> sma50 = SeriesCrossFinder.SMA(barlist, _ => _.close, 51);
                int startIndex = barlist.Count - 1 - 51;
                //double stddev = SeriesCrossFinder.Stdev(barlist.GetRange(startIndex, 51));
                double[] prices = new double[51];
                List<BarData> list = barlist.GetRange(startIndex, 51);
                for (int i = 0; i < list.Count; i++)
                {

                    prices[i] = list[i].close;
                }
                double stddev = SeriesCrossFinder.StandardDeviation(prices);
                return (barlist[rowCount - 1].close - sma50[sma50.Count - 1].Price) / stddev;
            }
            else
            {
                return 0;
            }
        }
        private double CalulateSTD21Days(List<BarData> barlist)
        {
            
            int rowCount = barlist.Count;
            //new check
            if (rowCount >= 21)
            {
                MovingAverage movAvg = new MovingAverage();
                //  movAvg.ExponentialMovingAverage(barlist, _ => _.Close, 21);
                List<SeriesData> sma21 = SeriesCrossFinder.SMA(barlist, _ => _.close, 21);
                int startIndex = barlist.Count - 1 - 21;
                // double sma21 = CalulateEMA(barlist, 21);
                //double stddev = SeriesCrossFinder.Stdev(barlist.GetRange(startIndex,21));
                double[] prices = new double[21];
                List<BarData> list = barlist.GetRange(startIndex, 21);
                for (int i = 0; i < list.Count; i++)
                {

                    prices[i] = list[i].close;
                }
                double stddev = SeriesCrossFinder.StandardDeviation(prices);

                return (barlist[rowCount - 1].close - sma21[sma21.Count - 1].Price) / stddev;
            }
            else
            {
                return 0;
            }
        }



        internal  List<Field> CalulateRSI(List<BarData> records)
        {
         

            Navigator m_nav;
            Recordset m_Recordset;
            Field m_Date;
            Field m_Open;
            Field m_High;
            Field m_Low;
            Field m_Close;
            Field m_Volume;
            //records.GetRange
          //  listDatePrices.Where(x => x.MyProperty==1).ToList()[0];

            //Requires 6 fields: Date, Open, High, Low, Close, Volume                 

            // Load data into arrays
           // string[] records;
            List<Field> fieldValues = new List<Field>();

            // Create TASDK objects
            try
            {
                m_Recordset = new Recordset();
                m_Date = new Field(records.Count, "Date");
                m_Open = new Field(records.Count, "Open");
                m_High = new Field(records.Count, "High");
                m_Low = new Field(records.Count, "Low");
                m_Close = new Field(records.Count, "Close");
                m_Volume = new Field(records.Count, "Volume");

                // Populate TASDK objects
                // string[] data;
                int n = 0;
                foreach (BarData data in records)
                {

                    // data = records[n].Split(new[] { ',' });
                    m_Date.ValueStr(n, data.date.ToString());
                    m_Open.Value(n, data.open);
                    m_High.Value(n, data.high);
                    m_Low.Value(n, data.low);
                    m_Close.Value(n, data.close);
                    m_Volume.Value(n, data.volume);
                    n++;
                }

                m_Recordset.AddField(m_Date);
                m_Recordset.AddField(m_Open);
                m_Recordset.AddField(m_High);
                m_Recordset.AddField(m_Low);
                m_Recordset.AddField(m_Close);
                m_Recordset.AddField(m_Volume);


                m_nav = new Navigator();
                m_nav.Recordset_ = m_Recordset;
                Index index = new Index();
                //Q
                Field rsi = index.RelativeStrengthIndex(m_nav, m_Close, 14, "RSI").GetField("RSI");

              //  var rsitoday = rsi.Value(rsi.RecordCount-1);
                MovingAverage ma = new MovingAverage();
                //Recordset results = ma.SimpleMovingAverage(m_nav, m_Close, 14, "SMA1");
                //Field sma=results.GetField("SMA1");
                //var x = sma.Value(16);
                //var f=m_nav.Recordset_.GetField("Date");
                //f.ValueStr(16);
                
                fieldValues.Add(m_Date);
                fieldValues.Add(rsi);


               
            }
             
            catch (Exception ex)
            { throw ex; }
            return fieldValues;
        }
        internal double CalulateEMA(List<BarData> records,int period)
        {

            Navigator m_nav;
            Recordset m_Recordset;
            Field m_Date;
            Field m_Open;
            Field m_High;
            Field m_Low;
            Field m_Close;
            Field m_Volume;
            
            List<Field> fieldValues = new List<Field>();

            try
            {
                m_Recordset = new Recordset();
                m_Date = new Field(records.Count, "Date");
                m_Open = new Field(records.Count, "Open");
                m_High = new Field(records.Count, "High");
                m_Low = new Field(records.Count, "Low");
                m_Close = new Field(records.Count, "Close");
                m_Volume = new Field(records.Count, "Volume");

                int n = 0;
                foreach (BarData data in records)
                {

                    // data = records[n].Split(new[] { ',' });
                    m_Date.ValueStr(n, data.date.ToString());
                    m_Open.Value(n, data.open);
                    m_High.Value(n, data.high);
                    m_Low.Value(n, data.low);
                    m_Close.Value(n, data.close);
                    m_Volume.Value(n, data.volume);
                    n++;
                }

                m_Recordset.AddField(m_Date);
                m_Recordset.AddField(m_Open);
                m_Recordset.AddField(m_High);
                m_Recordset.AddField(m_Low);
                m_Recordset.AddField(m_Close);
                m_Recordset.AddField(m_Volume);


                m_nav = new Navigator();
                m_nav.Recordset_ = m_Recordset;
                MovingAverage mov = new MovingAverage();
                Field ema = mov.ExponentialMovingAverage(m_nav, m_Close, period, "EMA").GetField("EMA");

                var ematoay = ema.Value(ema.RecordCount - 1);
                return (double)ematoay;
            }

            catch (Exception ex)
            {
                throw ex;
            }
            
        }
        public void CalulateSupAndResi(List<BarData> barlist, SymbolAnalytics symbolAnalytics)
        {
      
            double H = 0;
            double L = 0;
            double C = 0;
            List<Double> SupandResi = new List<double>();
            // calculate arithmetic average for H, L, C
            BarData barData = barlist[barlist.Count-1];
            //foreach (BarData barData in barlist)
            //{
                H += barData.high;
               L += barData.low;
               C += barData.close;
            //}

          //  H /= barlist.Count;
         //   L /= barlist.Count;
         //   C /= barlist.Count;

            // Pivot point
            double P = (H + L + C) / 3.0;

            //R1 = 2 * P - L;
            //S1 = 2 * P - H;

            //R2 = P + (H - L);
            //S2 = P - (H - L);

            //R3 = H + 2 * (P - L);
            //S3 = L - 2 * (H - P);

            symbolAnalytics.r1 = 2 * P - L;
            symbolAnalytics.s1 = 2 * P - H;

            symbolAnalytics.r2 = P + (H - L);
            symbolAnalytics.s2 = P - (H - L);

            symbolAnalytics.r3 = H + 2 * (P - L);
            symbolAnalytics.s3 = L - 2 * (H - P);
            
           

        }

        public void CalulatePriceToDate(List<BarData> barlist, List<HistoricalDates> listHistoricalDates, SymbolAnalytics symbolAnalytics)
        {
            Double QTDPrice = 0;
            Double YTDPrice = 0;
            Double MTDPrice = 0;
            Double WTDPrice = 0;

            //List<HistoricalDates> weeklyDate = listHistoricalDates.Where(x => x.dateType.Equals("weekly", StringComparison.OrdinalIgnoreCase)).ToList();
            //List<BarData> barRecord = barlist.Where(x => x.Timestamp.Equals(weeklyDate[0].date)).ToList();
            //if (barRecord.Count > 0)
            //    WTDPrice = barRecord[0].Close;

          

            foreach (HistoricalDates histDate in listHistoricalDates)
            {
                List<BarData> barRecord = barlist.Where(x => x.date > (histDate.date.AddDays(-5)) && x.date <= (histDate.date)).ToList();
                if (histDate.dateType.Equals("Weekly", StringComparison.OrdinalIgnoreCase))
                {
                    if (barRecord.Count > 0)
                    {
                        WTDPrice = barRecord[barRecord.Count -1].close;
                    }
                    else
                        WTDPrice = barlist[0].close;
                }
                else if (histDate.dateType.Equals("Quaterly", StringComparison.OrdinalIgnoreCase))
                {
                    if (barRecord.Count > 0)
                        QTDPrice = barRecord[barRecord.Count - 1].close;
                    else
                        QTDPrice = barlist[0].close;
                
                }
                else if (histDate.dateType.Equals("Yearly", StringComparison.OrdinalIgnoreCase))
                {
                   
                    if (barRecord.Count > 0)
                        YTDPrice = barRecord[barRecord.Count - 1].close;
                    else
                        YTDPrice = barlist[0].close;
                
                }
                else if (histDate.dateType.Equals("Monthly", StringComparison.OrdinalIgnoreCase))
                {
                    if (barRecord.Count > 0)
                        MTDPrice = barRecord[barRecord.Count - 1].close;
                    else
                        MTDPrice = barlist[0].close;
                   
                }
            }


            symbolAnalytics.QTDPrice= QTDPrice;
            symbolAnalytics.YTDPrice = YTDPrice;
            symbolAnalytics.MTDPrice = MTDPrice;
            symbolAnalytics.WTDPrice = WTDPrice;

            ////List<string> mylist = new List<string> { "hello", "world", "foo", "bar" };
            //List<string> listContainingLetterO = mylist.Where(x => x.Contains("o")).ToList();
            //List<BarData> listContainingLetter1 = list.GetRange(2, 15);
            // List<HistoricalDates> listDatePrice1 = listHistoricalDates.Where(x => x.dateType.Equals("weekly", StringComparison.OrdinalIgnoreCase)).ToList();
            // DateTime date1 = listDatePrice1[0].date;
            //BarData data1 = barlist.Where(y => y.Timestamp.Equals(date1)).ToList()[0];
            // fincalc.CalulateRSI("MSFT", fromdate.Date, toDate.Date, barlist);




        }



        public List<DateOBOS> CalculateOBOSForRange(List<BarData> barDataList)
        {
            try
            {
                List<DateOBOS> list = new List<DateOBOS>();
                List<Field> dateRSIValues = null;
                int count = barDataList.Count;
                if (barDataList.Count >= 20)
                {
                    dateRSIValues = CalulateRSI(barDataList);
                }
                Field dates = dateRSIValues[0];
                Field rsis = dateRSIValues[1];

                for (int i = 50; i < count; i++)
                {
                    DateOBOS obos = new DateOBOS();
                    obos.Date = Convert.ToDateTime(dates.ValueStr(i));
                    double rsi = (double)rsis.Value(i);
                    if (count >= 20)
                    {
                        obos.obos = CalculateOBOSforRSI(rsi);
                    }
                    else
                    {
                        obos.obos = CalculateOBOSforRSI(45);
                    }
                    //if (rsi < 30)
                    //{
                    //    obos.obos = obosEnum.OS;

                    //}
                    //else if(rsi>70){

                    //    obos.obos = obosEnum.OB;
                    //}
                    //else
                    //{

                    //    obos.obos = obosEnum.Nuetral;
                    //}
                    list.Add(obos);

                }

                return list;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public DateOBOS CalculateOBOS(String symbol, List<BarData> barDataList, DateTime date)
        {
            if (barDataList.Count >= 20)
            {
                List<Field> dateRSIValues = CalulateRSI(barDataList);
            }
            return null;
        }

        public double calPerPositiveTime(List<BarData> barlist)
        {
             int count = barlist.Count;
             int negativeTime = 0;
             int positiveTime = 0;
            double perPositiveTime = 0.0;

            try
            {
                    try
                    {
                        for (int i = count - 1; i >= count - 10; i--)
                        {
                            double change = barlist[i].close - barlist[i].open;
                            if (change < 0)
                                negativeTime++;
                            else
                                positiveTime++;

                        }
                        perPositiveTime = positiveTime * 100 / (negativeTime + positiveTime);
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }

                
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
          


            return perPositiveTime;
        }



        public List<double> calculate50DaysMA(List<BarData> barlist, List<HistoricalDates> listHistoricalDates)
        {
            List<double> abov50daysMAList= new List<double>();
            List<SeriesData> sma50 = SeriesCrossFinder.SMA(barlist, _ => _.close, 50);
            try
            {
                        if (sma50.Count >0)
                        {
                            double current50dayMA = sma50[sma50.Count - 1].Price;
                            double CurrentlastPrice = barlist[barlist.Count - 1].close;
                            double currentAbove50dayMA = CurrentlastPrice - current50dayMA;
                            abov50daysMAList.Add(currentAbove50dayMA);
                        }
           
                     List<HistoricalDates> weeklyDate = listHistoricalDates.Where(x => x.dateType.Equals("weekly", StringComparison.OrdinalIgnoreCase)).ToList();
                    List<BarData> barRecord = barlist.Where(x => x.date.Equals(weeklyDate[0].date)).ToList();
           
                    double weeklyLastPrice=0; 
                    if(barRecord.Count > 0)
                    weeklyLastPrice = barRecord[0].close;
                      List<SeriesData> sma50Record = sma50.Where(x => x.Timestamp.Equals(weeklyDate[0].date)).ToList();
                     double weekly50dayMA=0; 
                    if(barRecord.Count > 0)
                    weekly50dayMA = sma50Record[0].Price;
                      double weeklyAbove50dayMA= weeklyLastPrice - weekly50dayMA;
                      abov50daysMAList.Add(weeklyAbove50dayMA);
                    
            }
            catch (Exception ex)
            {
                log.Warn("Error in Calculating 50DaysMA" + ex);

            }
            return abov50daysMAList;
        }

        public List<DateAD> CalculateADForRange(List<BarData> barlist)
        {

            List<DateAD> dateADList = new List<DateAD>();
            
            
            for (int i=0;i<barlist.Count;i++)
            {
                DateAD dateAD = new DateAD();
                dateAD.date = barlist[i].date;
                double change=barlist[i].close-barlist[i].open;
                if (change >= 0)
                    dateAD.AD=1;
                else
                    dateAD.AD = 0; 

               dateADList.Add(dateAD);
            }


            return dateADList;
        }
       

        public List<DateADCount> CalDateADCount(List<DateAD> listdateAD)
        {
           List<DateADCount> ListDateADCount = new List<DateADCount>();
            
            foreach (DateAD dateAD in listdateAD)
            {
                int aCount = 0;
                int dCount = 0;
                 DateADCount dateADCount = new DateADCount();
                  dateADCount.date = dateAD.date;
                       
                        if (dateAD.AD== 1)
                            aCount++;
                        else
                            dCount++;

                        dateADCount.ADDiff = aCount - dCount;
                        dateADCount.aCount = aCount;
                        dateADCount.dCount = dCount;
                       ListDateADCount.Add(dateADCount);
                    }

            return ListDateADCount;
            }

        public List<DateADCount> CalDateADCountNext(List<DateAD> listdateAD, List<DateADCount> listDateADCount)
        {
            for (int i = 0; i < listDateADCount.Count; i++)
            {
                DateADCount dateADCount = listDateADCount[i];
                if (i >= listdateAD.Count)
                {
                    break;
                }
                 DateAD dateAD = listdateAD[i];

                    if (dateAD.AD == 1)
                    {

                        dateADCount.aCount++;
                        
                    }
                    else if (dateAD.AD == 0)
                    {

                        dateADCount.dCount++;
                        
                    }

                    dateADCount.ADDiff = dateADCount.aCount - dateADCount.dCount;
                
            }
           
            return listDateADCount;
            }


       
    }
    }
    

