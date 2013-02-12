using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinLib;
using System.Data.Odbc;

namespace ChartLabFinCalculation
{
    class CommonDAO
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(CommonDAO));
        public static List<HistoricalDates> getHistoricalDatesFromDB()
        {

            List<HistoricalDates> historicalDateslist = new List<HistoricalDates>();
            //OdbcConnection MyConnection = new OdbcConnection(DBConnection.MyConString);

            //MyConnection.Open();

            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            OdbcCommand com = new OdbcCommand("SELECT dateType,date FROM historicaldates where historicalDatesId<5", con);

            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    HistoricalDates historicalDate = new HistoricalDates();
                    historicalDate.dateType = dr.GetString(0).ToString();
                    historicalDate.date = DateTime.Parse(dr.GetString(1));
                    //qd.Low = double.Parse(dr.GetString(2));

                    historicalDateslist.Add(historicalDate);
                }
                dr.Close();
                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }

            // string mycon = System.Configuration.ConfigurationSettings.AppSettings.["sqlcon"].ConnectionString;
            return historicalDateslist;
        }



        internal static Dictionary<DateTime, double> getSymbolsHistPrices(String symbol, DateTime startDate)
        {

            Dictionary<DateTime, double> DatePriceList = new Dictionary<DateTime, double>();
            log.Info("\n\n\n\n\n Getting Data from mongo DB for symbol " + symbol);

            try
            {
                DateTime toDate = DateTime.Now;
                List<BarData> barlist = SymbolHistoricalMongoDAO.GetHistoricalDataFromMongo(startDate, toDate, symbol);

                foreach (BarData bar in barlist)
                {
                    DateTime date =bar.date;
                    double price = bar.close;
                    if (!DatePriceList.ContainsKey(date))
                    {
                        DatePriceList.Add(date, price);
                    }
                    else
                    {
                        log.Error("Duplicate data in mongo symbolshistorical for symbol: " + symbol + " for Date: " + date);
                    }
                }


            }



            catch (Exception ex)
            {
                throw ex;
            }
            return DatePriceList;
        }
    }
}
