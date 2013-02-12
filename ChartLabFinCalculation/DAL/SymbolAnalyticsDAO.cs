using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using FinLib;

namespace ChartLabFinCalculation
{
    class SymbolAnalyticsDAO
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(SymbolAnalyticsDAO));

        public static void SymbolAnalyticsCSVToDB(string foldername, string filename, bool deletePreviousData)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand deleteCommand = new OdbcCommand("DELETE from symbolAnalytics", con);
            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE '" + foldername + "/" + filename + "' " +
                                                "INTO TABLE symbolAnalytics " +
                                                "FIELDS TERMINATED BY ',' " +
                                                "LINES TERMINATED BY '\n' " +
                                                "(`symbol`,s1,s2,s3,r1,r2,r3,buySellRating,shortTerm,shortTermTrendDate,mediumTerm,longTerm,alert,YTD,MTD,QTD,WTD,osOBCurrent,osOBWeekly,perPositive,STD50Days,currentReturn,rsiCurrent,rsiOneWeekBefore,STD21Days,low52WeekRange,high52WeekRange) ", con);

            try
            {
                con.Open();
                if (deletePreviousData)
                {
                    deleteCommand.ExecuteNonQuery();
                }
                insertCommand.ExecuteReader();
                log.Info("SymbolAnalytics File " + filename + " Saved....");

                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }
        }

        public static void DeletePatterns()
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            OdbcCommand deleteCommand = new OdbcCommand("DELETE from patternHistory", con);




            try
            {
                con.Open();

                deleteCommand.ExecuteNonQuery();


                //log.Info("SymbolAnalytics File " + count + " Saved....");
                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }
        }

        public static void UpdatedPatterns(string foldername, string filename)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);


            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE '" + foldername + "/" + filename + "' " +
                                                "INTO TABLE patternHistory " +
                                                "FIELDS TERMINATED BY ',' " +
                                                "LINES TERMINATED BY '\n' " +
                                                "(symbol,startDate,endDate,PatternId);", con);



            try
            {
                con.Open();

                insertCommand.ExecuteReader();

                log.Info("SymbolAnalytics File " + filename + " Saved....");
                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }
        }

        public static void UpdatedAlertsInSymbolAnalytics()
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            OdbcCommand updateCommand = new OdbcCommand("UPDATE symbolAnalytics," +
                                        " (SELECT  ph.patternId,ph.symbol,ph.startDate FROM patternHistory ph JOIN" +
                                        " (SELECT symbol,MAX(startDate) AS startDate  FROM patternHistory" +
                                        " GROUP BY symbol ) TempPH" +

                                        " ON ph.startDate=TempPH.startDate AND ph.symbol=TempPH.symbol" +
                                        ") TempFinalPH" +
                                        " SET symbolAnalytics.alert=TempFinalPH.patternId, symbolAnalytics.alertDate=TempFinalPH.startDate" +
                                        " WHERE symbolAnalytics.symbol=TempFinalPH.symbol", con);



            try
            {
                con.Open();

                updateCommand.ExecuteReader();

                log.Info("Alerts Updates....");
                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }
        }


        public static string GetIndexSymbolName(string symbol)
        {

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand("SELECT indexSymbol FROM indices WHERE yahooSymbol='" + symbol + "'", con);

            string indexSymbol = "";

            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    indexSymbol = dr.GetString(0);
                }
                dr.Close();

                con.Close();
            }
            catch (OdbcException ex)
            {
                log.Error(ex);
            }

         return  indexSymbol;
        }

    
    }
}
