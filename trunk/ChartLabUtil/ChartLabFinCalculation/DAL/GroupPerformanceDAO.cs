using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinLib;
using System.Data.Odbc;

namespace ChartLabFinCalculation
{
    class GroupPerformanceDAO
    {

        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(GroupPerformanceDAO));


        public static List<string> GetSymbolListForGroup(int groupId)
        {
            List<string> SymbolList = new List<string>();
            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            OdbcCommand selectMFByGroupComm = new OdbcCommand("SELECT DISTINCT mutualFundName FROM sectors WHERE mutualfundname IS NOT NULL AND groupid =" + groupId, con);

            try
            {

                con.Open();
                OdbcDataReader dr = selectMFByGroupComm.ExecuteReader();

                while (dr.Read())
                {

                    SymbolList.Add(dr.GetString(0));
                }
                dr.Close();
                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }

            return SymbolList;
        }

        public static List<DatePriceList> TodaysDatePrice(string symbol)
        {
            List<DatePriceList> todaysDatePriceList = new List<DatePriceList>();

            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            OdbcCommand dateListCmd = new OdbcCommand("SELECT date,close FROM symbolshistorical where symbol='" + symbol + "' and date=(SELECT `Date` FROM historicaldates WHERE DateType='" + Constants.P + "')", con);


            try
            {
                con.Open();
                OdbcDataReader dr = dateListCmd.ExecuteReader();
                while (dr.Read())
                {

                    todaysDatePriceList.Add(new DatePriceList
                    {
                        date = DateTime.Parse(dr.GetString(0)),
                        price = Double.Parse(dr.GetString(1)),

                    });
                }
                dr.Close();
                con.Close();
            }
            catch (Exception ex)
            {
                log.Info("ERROR \n" + "============ \n" + ex.ToString());
            }
            return todaysDatePriceList;
        }

        public static List<DatePriceList> GetDataForGroupFromDB(string symbol, DateTime historyDate)
        {
            List<DatePriceList> datePriceList = new List<DatePriceList>();
            log.Info("\n\n\n\n\n Getting Data from DB for symbol " + symbol);

            try
            {
                OdbcConnection con = new OdbcConnection(Constants.MyConString);
                string date = historyDate.Date.ToString("yyyy-MM-dd");
                OdbcCommand com = new OdbcCommand("SELECT date,close from  symbolshistorical where symbol = '" + symbol + "' and date='" + date + "'", con);
                //OdbcCommand com = new OdbcCommand("SELECT date,close,volume from  symbolshistorical where symbol = 'SPY' order by date desc", con);
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    DatePriceList datePriceObj = new DatePriceList();
                    datePriceObj.date = DateTime.Parse(dr.GetString(0));
                    datePriceObj.price = Double.Parse(dr.GetString(1));
                    datePriceList.Add(datePriceObj);


                }
                dr.Close();
                con.Close();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return datePriceList;
        }

        public static void UpdateGroupPerfCSVToDB(string foldername)
        {

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand deleteCommand = new OdbcCommand("DELETE from groupbyPerf", con);
            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE" + " '" + foldername + "/GroupPerformanceFile.csv' " +
                                                "INTO TABLE groupbyPerf " +
                                                "FIELDS TERMINATED BY ',' " +
                                                "LINES TERMINATED BY '\n' " +
                                                "(groupid,1week,1month,1qtr,ytd);", con);


            try
            {
                con.Open();

                deleteCommand.ExecuteNonQuery();
                insertCommand.ExecuteReader();

                log.Info("\nGroup Performance Updated....\n");
                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }
        }
    }
}
