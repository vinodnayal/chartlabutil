using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using FinLib;

namespace ChartLabFinCalculation
{
    class MarketInternalsDAO
    {

        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(MarketInternalsDAO));

        public static void UpdateMarketInternals(string tableName, double perPositiveTime, string symbol)
        {

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand updateCommand = new OdbcCommand("update " + tableName + " set perPositiveTime=" + perPositiveTime + " where Symbol='" + symbol + "'", con);

            try
            {
                con.Open();



                updateCommand.ExecuteReader();


                con.Close();
            }
            catch (OdbcException ex)
            {
                log.Error(ex);
            }
        }

        public static List<DateOBOSCount> getOBOSCountFromDB(List<HistoricalDates> listHistoricalDates)
        {

            List<HistoricalDates> weeklyDate = listHistoricalDates.Where(x => x.dateType.Equals("weekly", StringComparison.OrdinalIgnoreCase)).ToList();
            // List<BarData> barRecord = barlist.Where(x => x.Timestamp.Equals(weeklyDate[0].date)).ToList();
            string weeklyDates = weeklyDate[0].date.ToString("yyyy-MM-dd");
            string todaysDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            List<DateOBOSCount> obosCountList = new List<DateOBOSCount>();

            String maxDateStr = "SELECT MAX(DATE) FROM dailyIndicesObOsPerAnalysis";
            OdbcCommand maxDatecom = new OdbcCommand(maxDateStr, con);
            //Now we will create a command

            //SELECT obCount,osCount,DATE FROM dailyIndicesObOsPerAnalysis WHERE DATE IN ('2012-01-23')
            try
            {
                con.Open();

                OdbcDataReader drMaxDate = maxDatecom.ExecuteReader();
                while (drMaxDate.Read())
                {
                    todaysDate = drMaxDate.GetString(0);
                }
                OdbcCommand com = new OdbcCommand("SELECT obCount,osCount,date FROM dailyIndicesObOsPerAnalysis where date in ('" + weeklyDates + "','" + todaysDate + "')", con);


                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    obosCountList.Add(new DateOBOSCount
                    {
                        obCount = Convert.ToInt32(dr.GetString(0)),
                        osCount = Convert.ToInt32(dr.GetString(1)),
                        Date = Convert.ToDateTime(dr.GetString(2))


                    });

                }
                dr.Close();
                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }

            return obosCountList;
        }

        public static List<string> GetSnPTopOSSymbols()
        {
            List<string> SymbolList = new List<string>();
            //Now we will create a connection



            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            //Now we will create a command
            OdbcCommand deleteTopOSComm = new OdbcCommand("delete FROM oSIndexTopSymbols", con);
            OdbcCommand insertTopOSComm = new OdbcCommand("INSERT INTO osindextopsymbols(symbol,indexSymbolId) SELECT a.symbol,1 FROM symbolanalytics AS a JOIN historybuysellrating AS b ON a.symbol=b.symbol "+
                                                        "WHERE b.ratingdate=(SELECT MAX(ratingdate) FROM historybuysellrating) AND a.ctrating IN (6,7,5) " +
                                                        "ORDER BY b.ctratingvalue DESC LIMIT 10", con);

            //OdbcCommand insertTopOSComm = new OdbcCommand("INSERT INTO osindextopsymbols(symbol,indexSymbolId) SELECT a.symbol,1 FROM symbolanalytics AS a JOIN historybuysellrating AS b ON a.symbol=b.symbol "+
            //                                            "WHERE b.ratingdate=(SELECT DATE FROM historicaldates WHERE DateType='"+Constants.C+"') AND a.ctrating IN (6,7,5) "+
            //                                            "ORDER BY b.ctratingvalue DESC LIMIT 10", con);

            
            OdbcCommand selectTopOSComm = new OdbcCommand("SELECT distinct symbol FROM oSIndexTopSymbols", con);

            try
            {

                con.Open();

                deleteTopOSComm.ExecuteNonQuery();

                insertTopOSComm.ExecuteReader();
                //  log.Info("OBOS Percentage Saved....");

                OdbcDataReader dr = selectTopOSComm.ExecuteReader();

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

        public static List<string> GetSnPTopOBSymbols()
        {
            List<string> SymbolList = new List<string>();
            //Now we will create a connection



            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            //Now we will create a command
            OdbcCommand deleteTopOSComm = new OdbcCommand("delete FROM oBIndexTopSymbols", con);
            OdbcCommand insertTopOSComm = new OdbcCommand(@"INSERT INTO oBIndexTopSymbols(symbol,indexSymbolId) SELECT a.symbol,1 FROM symbolanalytics AS a 
                                                            JOIN historybuysellrating AS b ON a.symbol=b.symbol 
                                                            WHERE b.ratingdate=(SELECT MAX(ratingdate) FROM historybuysellrating) AND a.ctrating IN (1,2,3) 
                                                            ORDER BY b.ctratingvalue ASC LIMIT 10", con);
            OdbcCommand selectTopOSComm = new OdbcCommand("SELECT distinct symbol FROM oBIndexTopSymbols", con);

            try
            {

                con.Open();

                deleteTopOSComm.ExecuteNonQuery();

                insertTopOSComm.ExecuteReader();
                //  log.Info("OBOS Percentage Saved....");

                OdbcDataReader dr = selectTopOSComm.ExecuteReader();

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

        public static void writeMarketIntToDB(MarketInternal marketint)
        {
            int weeklyOSCount = marketint.weeklyOSCount;
            int weeklyOBCount = marketint.weeklyOBCount;
            int currentOSCount = marketint.currentOSCount;
            int currentOBCount = marketint.currentOBCount;
            double weeklyADLine10Days = marketint.weeklyADLine10Days;
            double currentADLine10Days = marketint.currentADLine10Days;
            double currentAbove50dayMA = marketint.currentAbove50dayMA;
            double weeklyAbove50dayMA = marketint.weeklyAbove50dayMA;

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand updateCommand = new OdbcCommand("update indicesInternals set " +

             "osWeek=" + weeklyOSCount +
             ",obWeek=" + weeklyOBCount +
             ",osCur=" + currentOSCount +
             ",obCur=" + currentOBCount +
            ",tendayADlineWeek=" + weeklyADLine10Days +
             ",tendayADlineCur=" + currentADLine10Days +
            ",fiftydayMovAvgCur=" + currentAbove50dayMA +
            ",fiftydayMovAvgWeek=" + weeklyAbove50dayMA


            , con);

            try
            {
                con.Open();



                updateCommand.ExecuteReader();


                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }

        }
    }
}
