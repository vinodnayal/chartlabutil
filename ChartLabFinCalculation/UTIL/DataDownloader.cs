using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinLib;
using System.Net;
using System.IO;
using System.Globalization;
using System.Configuration;
using System.Data.Odbc;
using System.Data;
using FinLib.Model;

namespace ChartLabFinCalculation
{
    class DataDownloader
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(DataDownloader));
        private static readonly CultureInfo ciUS = new CultureInfo("en-us");

        public static List<BarData> GetDataFromFeedFromYahoo(DateTime from, DateTime to, string symbol)
        {


            return GetDataFromFeedFromYahoo(from, to, symbol, 1);


        }
        public static DividendHistory CheckIfThereIsAnyDividend(DateTime from, DateTime to, string symbol)
        {

            from = from.AddDays(-7); //Check within a week

            string url = string.Format("http://ichart.finance.yahoo.com/table.csv?s={0}&a={1}&b={2}&c={3}&d={4}&e={5}&f={6}&g=v&ignore=.csv",
                                                                symbol,
                                                                 from.Month - 1, from.Day, from.Year,
                                                                 to.Month - 1, to.Day, to.Year);
            DividendHistory histDividend = new DividendHistory();
            try
            {
                
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Proxy = null;
                //String arrayrow= File.GetAttributes(url);
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
                          
                            if (rows.Length > 2)
                            {
                                string[] values = rows[1].Split(',');
                                log.Info("\n\n Some Dividend Data is present for this symbol  " + symbol);
                               
                                histDividend.symbol = symbol;
                                histDividend.dividendDate =DateTime.Parse(values[0]).Date;
                                histDividend.todaysDate = to.Date;
                                histDividend.isdividend = true;
                                return histDividend;
                            }
                            else
                            {
                                histDividend.isdividend = false;
                                return histDividend;
                            }


                        }
                    }

                }
            }
            catch (Exception ex)
            {
                log.Warn("\n\n Issue While Getting Dividend Info from yahoo finance " + symbol);
                //throw ex;
                histDividend.isdividend = false;
                return histDividend;
            }

        }

        private static List<BarData> GetDataFromFeedFromYahoo(DateTime from, DateTime to, string symbol, int retryCount)
        {


            //if (CheckIfThereIsAnyDividend(from, to, symbol))
            //{
            // delete data for symbol
            //SymbolHistoricalDAO.DeleteData(symbol);



            //  return GetDataFromFeedFromBATS(from, to, symbol);
            //}
            Dictionary<DateTime, BarData> map = new Dictionary<DateTime, BarData>();
           // List<BarData> barData = new List<BarData>();
            string url = string.Format("http://ichart.finance.yahoo.com/table.csv?s={0}&a={1}&b={2}&c={3}&d={4}&e={5}&f={6}&g=d&ignore=.csv",
                                                                symbol,
                                                                 from.Month - 1, from.Day, from.Year,
                                                                 to.Month - 1, to.Day, to.Year);
          


            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Proxy = null;
                //String arrayrow= File.GetAttributes(url);
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
                            
                            foreach (string row in rows.Skip(1))
                            {
                                string[] values = row.Split(',');
                                if (values.Length != 7)
                                {
                                    continue;
                                    //log.Error("Dividend Present for Symbol :" +symbol +" : "+ row);
                                    // May be Dividend Data

                                }

                               
                                BarData dataBar = new BarData
                                 {

                                     open = Convert.ToDouble(values[1], ciUS),
                                     high = Convert.ToDouble(values[2], ciUS),
                                     low = Convert.ToDouble(values[3], ciUS),
                                     close = Convert.ToDouble(values[6], ciUS),
                                     actualclose = Convert.ToDouble(values[4], ciUS),
                                     volume = Convert.ToDouble(values[5], ciUS),
                                     date = DateTime.Parse(values[0]).Date
                                 };
                                if (!map.ContainsKey(dataBar.date))
                                {
                                    map.Add(dataBar.date,dataBar);
                                }
                               

                               
                            }


                        }
                    }
                    log.Info("\n\n GOT Data from Yahoo Finance " + symbol);
                }
            }
            catch (Exception ex)
            {

                if (retryCount == 5)
                {
                    //log.Error("\n\n\n NO DATA for Symbol STOP Trying "+symbol);
                    throw new Exception("YAHOO FINANCE NO Data For Symbol " + symbol, ex);
                }
                else
                {
                    retryCount++;
                    log.Info("\n\n\n NO DATA for Symbol " + symbol + " Retry ...Count" + retryCount);
                    return GetDataFromFeedFromYahoo(from, to, symbol, retryCount);

                }

            }
          
           List<BarData> barData= map.Values.ToList();
           List<BarData> SortedList = barData.OrderBy(o => o.date).ToList();
           
            //SortedList.Reverse();
           // barData.Reverse();
           return SortedList;
            //return map.ToList<BarData>();

        }
        public static List<BarData> GetDataFromFeedFromYahooIfNotDB(DateTime from, DateTime to, string symbol)
        {

            try
            {
                List<BarData> barData = DataDownloader.GetDataFromFeedFromYahoo(from, to, symbol);
                return barData;
            }
            catch (Exception ex)
            {
                return SymbolHistoricalMongoDAO.GetHistoricalDataFromMongo(from, to, symbol);
            }

        }

        public static List<DailySymbolsData> getDataforSymbolListYahoo(List<string> symbolList)
        {
            string joined = string.Join(",", symbolList.ToArray());

            string url = string.Format("http://finance.yahoo.com/d/quotes.csv?s=" + joined + "&f=sl1c1p2kj");
            List<DailySymbolsData> dailySymbolsData = new List<DailySymbolsData>();

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Proxy = null;
                //String arrayrow= File.GetAttributes(url);
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

                            foreach (string row in rows.Skip(1))
                            {
                                string[] values = row.Split(',');
                                if (values.Length != 7)
                                    continue;


                                dailySymbolsData.Add(new DailySymbolsData
                                {
                                    symbol = values[0].ToString(),
                                    lastPrice = Convert.ToDouble(values[1], ciUS),
                                    change = Convert.ToDouble(values[2], ciUS),


                                });
                            }


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dailySymbolsData;

        }


        internal static List<string> GetErrorSymbols(string path)
        {
            List<string> symbols = new List<string>();
            StreamReader reader = new StreamReader(path);
            string strline = "";
            string[] _values = null;
            int x = 0;
            while (!reader.EndOfStream)
            {
                strline = reader.ReadLine();
                _values = strline.Split(',');
                foreach (string value in _values)
                {
                    symbols.Add(value);
                }

            }
            return symbols;
        }

        public static List<DailyAverageVolume> GetDailyAvgVolumeDataFromYahoo(List<String> symbolList)
        {
            List<DailyAverageVolume> dailyAvgVolumeList = new List<DailyAverageVolume>();

            string joined = string.Join(",", symbolList.ToArray());
            string url = string.Format("http://finance.yahoo.com/d/quotes.csv?s=" + joined + "&f=sa2");

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Proxy = null;
                //String arrayrow= File.GetAttributes(url);
                using (WebResponse response = request.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        if (stream == null)
                        {

                        }
                        //  using (StreamReader sr = new StreamReader(stream))
                        //{
                        //    string data = sr.ReadToEnd();
                        //    string[] rows = data.Split('\n');

                        //    foreach (string row in rows.Skip(1))
                        //    {
                        //        string[] values = row.Split(',');
                        //        if (values.Length != 7)
                        //            continue;

                        using (StreamReader sr = new StreamReader(stream))
                        {
                            string data = sr.ReadToEnd();
                            string[] rows = data.Split('\n');

                            foreach (string row in rows)
                            {

                                String delim = "\",/";
                                if (row != "")
                                {

                                    DailyAverageVolume dailyAvgVolumeObj = new DailyAverageVolume();
                                    string[] values = row.Split(',');
                                    log.Info("Getting  DailyAvgVolume for symbol " + (values[0].ToString()).Trim(delim.ToCharArray()));
                                    dailyAvgVolumeObj.Symbol = (values[0].ToString()).Trim(delim.ToCharArray());
                                    dailyAvgVolumeObj.Volume = Convert.ToDouble(values[1], ciUS);
                                    dailyAvgVolumeList.Add(dailyAvgVolumeObj);
                                }

                            }


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return dailyAvgVolumeList;
        }


        public static List<SymbolNameId> GetSymbolListForSentimentAlert()
        {
            List<SymbolNameId> lstSymbols = new List<SymbolNameId>();
            OdbcConnection MyConnection = new OdbcConnection(Constants.MyConString);
            MyConnection.Open();

            OdbcConnection con = new OdbcConnection(Constants.MyConString);


            OdbcCommand com = new OdbcCommand("SELECT DISTINCT h.sentimentindicatorid,s.ticker FROM historysentimentindicators AS h JOIN sentimentindicators AS s " +
                               "WHERE h.sentimentindicatorid=s.sentimentindicatorid", con);

            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    SymbolNameId symbolIdobj = new SymbolNameId();
                    symbolIdobj.SentimentSymbolId = Convert.ToInt32(dr.GetString(0));
                    symbolIdobj.SentimentSymbol = dr.GetString(1);
                    lstSymbols.Add(symbolIdobj);
                }
                dr.Close();
                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }

            return lstSymbols;
        }


        public static List<DateForSymbolAlert> GetSentimentAlertForSymbol(int Indicator, string symbol)
        {
            List<DateForSymbolAlert> DateListForAlert = new List<DateForSymbolAlert>();
            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            string rating;
            if (Indicator == 1)
            {
                rating = "LTIrating";
            }
            else if (Indicator == 2)
            {
                rating = "STIrating";
            }
            else
            {
                rating = "BIrating";
            }

            OdbcCommand sentimentAlerts = new OdbcCommand("SELECT s.ticker, h.date, h." + rating + " FROM historysentimentindicators " +
                                                       "AS h JOIN sentimentindicators AS s ON h.sentimentindicatorid=s.sentimentindicatorid WHERE " +
                                                       "h." + rating + " IN (2,5,3,6) AND s.ticker='" + symbol + "'", con);



            try
            {

                con.Open();



                OdbcDataReader dr = sentimentAlerts.ExecuteReader();

                while (dr.Read())
                {



                    DateListForAlert.Add(new DateForSymbolAlert
                    {
                        Symbol = dr.GetString(0).ToString(),
                        ChangeDate = DateTime.Parse(dr.GetString(1))

                    });
                }
                dr.Close();
                con.Close();
            }
            catch (Exception ex)
            {
                log.Info("ERROR \n" + "============ \n" + ex.ToString());
            }
            return DateListForAlert;
        }

        public static List<BarData> GetDataFeedFromGoogle(DateTime from, DateTime to, string symbol)
        {

            string fromMonth = from.ToString("MMM");
            string toMonth = to.ToString("MMM");

            List<BarData> barData = new List<BarData>();

            string url = string.Format("http://www.google.com/finance/historical?q={0}&startdate={1}+{2}+{3}&enddate={4}+{5}+{6}&output=csv",
                                                              symbol,
                                                              fromMonth, from.Day, from.Year,
                                                              toMonth, to.Day, to.Year);
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Proxy = null;
                //String arrayrow= File.GetAttributes(url);
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

                            foreach (string row in rows.Skip(1))
                            {
                                string[] values = row.Split(',');
                                if (values.Length != 6)
                                    continue;


                                barData.Add(new BarData
                                {

                                    open = Convert.ToDouble(values[1], ciUS),
                                    high = Convert.ToDouble(values[2], ciUS),
                                    low = Convert.ToDouble(values[3], ciUS),
                                    close = Convert.ToDouble(values[5], ciUS),
                                    actualclose = Convert.ToDouble(values[4], ciUS),
                                    volume = Convert.ToDouble(values[5], ciUS),
                                    date = DateTime.Parse(values[0]).Date
                                });
                            }


                        }
                    }
                    log.Info("\n\n GOT Data from Google Finance " + symbol);
                }
            }
            catch (Exception ex)
            {

                log.Error(ex);
                //if (retryCount == 5)
                //{
                //    //log.Error("\n\n\n NO DATA for Symbol STOP Trying "+symbol);
                //    throw new Exception("YAHOO FINANCE NO Data For Symbol " + symbol, ex);
                //}
                //else
                //{
                //    retryCount++;
                //    log.Info("\n\n\n NO DATA for Symbol " + symbol + " Retry ...Count" + retryCount);
                //    return GetDataFromFeedFromYahoo(from, to, symbol, retryCount);

                //}

            }
            barData.Reverse();
            return barData;

        }

    }



}
