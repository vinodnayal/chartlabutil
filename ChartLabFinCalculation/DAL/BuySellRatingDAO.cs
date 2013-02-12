using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using System.Data;
using FinLib;

namespace ChartLabFinCalculation
{
    class BuySellRatingDAO
    {

        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(BuySellRatingDAO));

        public static void InsertRating(string foldername)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand deleteCommand = new OdbcCommand("DELETE from temp_buySellRating", con);
            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE" + " '" + foldername + "/RatingFile.csv' " +
                                                "INTO TABLE temp_buySellRating " +
                                                "FIELDS TERMINATED BY ',' " +
                                                "LINES TERMINATED BY '\n' " +
                                                "(symbol,rating,ratingvalue,ctrating,ctratingvalue);", con);

            OdbcCommand updateCommand = new OdbcCommand("UPDATE symbolAnalytics ,temp_buySellRating" +
                                                  " SET symbolAnalytics.buySellRating =temp_buySellRating.rating, " +
                                                  "symbolAnalytics.ctrating =temp_buySellRating.ctrating" +
                                                  " WHERE symbolAnalytics.symbol=temp_buySellRating.symbol ;", con);


            OdbcCommand deleteBuySellRatingCmd = new OdbcCommand("delete from historyBuySellRating where ratingdate=DATE(NOW())", con);

            OdbcCommand inserthistoryBuySellRatingCmd = new OdbcCommand("INSERT INTO historyBuySellRating (symbol,rating,ratingvalue,ratingdate,ctrating,ctratingvalue) " +
                                                   "SELECT symbol,rating,ratingvalue,DATE(NOW()),ctrating,ctratingvalue FROM temp_buySellRating", con);

            OdbcCommand deleteTempRatingCmd = new OdbcCommand("DELETE from tempreviousrating", con);
            OdbcCommand insertTempPrevRatingCmd = con.CreateCommand();
            insertTempPrevRatingCmd.CommandText = "{ CALL prevRating }";
            insertTempPrevRatingCmd.CommandType = CommandType.StoredProcedure;

            //  OdbcCommand insertTempPrevRatingCmd = new OdbcCommand("prevRating", con);

            OdbcCommand deletetopRatingSymbols = new OdbcCommand("DELETE from topRatingSymbols", con);
            OdbcCommand insertTop10symbols = new OdbcCommand("INSERT INTO topRatingSymbols (symbol,ratingvalue) " +
                                    "SELECT t.symbol, t.ratingvalue FROM temp_buySellRating AS t LEFT JOIN equitiesfundamental AS e " +
                                    "ON t.symbol=e.symbol " +
                                    "WHERE e.sectorId IS NOT NULL " +
                                    "ORDER BY ratingvalue DESC LIMIT 20", con);

            OdbcCommand deleteSnPStrongestWatchlistSymbols = new OdbcCommand("DELETE from watchlistsymbolmapping where watchlistid=1", con);
            OdbcCommand insertSnPStrongestWatchlistSymbols = new OdbcCommand("INSERT INTO watchlistsymbolmapping (watchlistid,symbol,quantity,price)" +
                                                                            " SELECT 1,w.symbol,1,h.close FROM topratingsymbols AS w JOIN symbolshistorical AS h ON w.symbol=h.symbol" +
                                                                            " WHERE  h.date=(SELECT DATE FROM historicaldates WHERE DateType='PreviousDay')", con);



            OdbcCommand updateSymbolAnalytics = new OdbcCommand("UPDATE symbolAnalytics  SA, tempreviousrating T "
            + "SET SA.previuosbsrating = T.rating, SA.preSBRatingDate = T.ratingdate " +
            "WHERE T.symbol=SA.symbol", con);



            //OdbcCommand insertBSRatingChangeHist = new OdbcCommand("INSERT INTO buySellRatingChangeHistory (symbol,oldRating,date) " +
            //                        "SELECT symbol, rating,ratingdate FROM tempreviousrating", con);

            OdbcCommand contrarian = new OdbcCommand("call contrarianSymbols", con);
            OdbcCommand strongest = new OdbcCommand("call StrongestSymbols ", con);

            OdbcCommand deleteSnPrapidlyMovWatchlistSymbols = new OdbcCommand("DELETE from watchlistsymbolmapping where watchlistid=2", con);
            OdbcCommand insertSnPrapidlyMovWatchlistSymbols = new OdbcCommand("INSERT INTO watchlistsymbolmapping (watchlistid,symbol,quantity,price)" +
                                                                            "SELECT 2,w.symbol,1,h.close FROM strongestperformance AS w JOIN symbolshistorical AS h ON w.symbol=h.symbol" +
                                                                            " WHERE  h.date=(SELECT DATE FROM historicaldates WHERE DateType='PreviousDay')", con);


            try
            {
                con.Open();

                deleteCommand.ExecuteNonQuery();
                deleteTempRatingCmd.ExecuteNonQuery();
                insertCommand.ExecuteReader();
                updateCommand.ExecuteReader();

                deleteBuySellRatingCmd.ExecuteNonQuery();
                inserthistoryBuySellRatingCmd.ExecuteNonQuery();
                insertTempPrevRatingCmd.ExecuteReader();
                deletetopRatingSymbols.ExecuteNonQuery();
                insertTop10symbols.ExecuteReader();
                deleteSnPStrongestWatchlistSymbols.ExecuteNonQuery();
                insertSnPStrongestWatchlistSymbols.ExecuteNonQuery();

                updateSymbolAnalytics.ExecuteReader();
                updateSymbolAnalyticsPreDate();
                UpdateSymbolAnalyticsPrice();
                // insertBSRatingChangeHist.ExecuteNonQuery();
                contrarian.ExecuteNonQuery();
                strongest.ExecuteNonQuery();
                deleteSnPrapidlyMovWatchlistSymbols.ExecuteNonQuery();
                insertSnPrapidlyMovWatchlistSymbols.ExecuteNonQuery();

                log.Info("BuySellRating Updated....");
                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }

        }

        private static void updateSymbolAnalyticsPreDate()
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand update1Command = new OdbcCommand("UPDATE symbolAnalytics sa, " +
                                                        "(SELECT t2.symbol FROM " +
                                                        "(SELECT h.symbol,MIN(h.date) AS mindate FROM symbolshistorical AS h JOIN " +
                                                        "(SELECT  s.symbol FROM indicesSymbolsMapping AS i JOIN symbolAnalytics AS s " +
                                                        "ON i.symbol=s.symbol WHERE s.preSBRatingDate IS  NULL AND s.changedateprice IS NULL) t1 " +
                                                        "ON h.symbol=t1.symbol " +
                                                        "GROUP BY h.symbol) t2 " +
                                                        "WHERE mindate<='2012-02-01') t3 " +
                                                        "SET sa.preSBRatingDate='2012-02-02' " +
                                                        "WHERE sa.symbol=t3.symbol", con);

            OdbcCommand update2Command = new OdbcCommand("UPDATE symbolAnalytics SA, " +
                                                        "(SELECT S.symbol,MIN(S.date) AS chngdate FROM symbolshistorical AS S JOIN " +
                                                        "(SELECT sh.symbol,MIN(sh.date) AS mindatesymbol FROM symbolshistorical AS sh JOIN " +
                                                        "(SELECT t2.symbol FROM " +
                                                        "(SELECT h.symbol,MIN(h.date) AS mindate FROM symbolshistorical AS h JOIN " +
                                                        "(SELECT  s.symbol FROM indicesSymbolsMapping AS i JOIN symbolAnalytics AS s " +
                                                        "ON i.symbol=s.symbol WHERE s.preSBRatingDate IS  NULL AND s.changedateprice IS NULL) t1 " +
                                                        "ON h.symbol=t1.symbol " +
                                                        "GROUP BY h.symbol) t2 " +
                                                        "WHERE mindate>='2012-02-01') t3 " +
                                                        "ON sh.symbol=t3.symbol " +
                                                        "GROUP BY sh.symbol) t4 " +
                                                        "ON S.symbol=t4.symbol " +
                                                        "WHERE S.date>t4.mindatesymbol " +
                                                        "GROUP BY S.symbol) t5 " +
                                                        "SET SA.preSBRatingDate=t5.chngdate " +
                                                        "WHERE SA.symbol=t5.symbol", con);


            try
            {
                con.Open();
                update1Command.ExecuteReader();
                update2Command.ExecuteReader();
                log.Info("\nSymbol Analytics ChangeDate Updated\n");
                con.Close();
            }
            catch (OdbcException ex)
            {
                log.Error(ex);
            }
        }


        private static void UpdateSymbolAnalyticsPrice()
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand updateCommand = new OdbcCommand("UPDATE symbolAnalytics sa, " +
                                                        "(SELECT DISTINCT  t1.symbol,s.close FROM symbolshistorical AS s JOIN " +
                                                        "(SELECT a.symbol,a.preSBRatingDate,h.date,MAX(h.date) AS dt FROM symbolAnalytics AS a JOIN symbolshistorical AS h " +
                                                        "ON a.symbol=h.symbol " +
                                                        "WHERE h.date<=a.preSBRatingDate " +
                                                        "GROUP BY h.symbol) t1 " +
                                                        "ON t1.symbol=s.symbol " +
                                                        "WHERE s.date=t1.dt)t2 " +
                                                        "SET sa.changedateprice=t2.close " +
                                                        "WHERE sa.symbol=t2.symbol", con);


            try
            {
                con.Open();
                updateCommand.ExecuteReader();
                log.Info("\nSymbol Analytics ChangeDatePrice Updated\n");
                con.Close();
            }
            catch (OdbcException ex)
            {
                log.Error(ex);
            }
        }

        internal static List<BuySellRating> getBuySellRatingHistroyFromDB()
        {
            List<BuySellRating> buySellRatingHist = new List<BuySellRating>();
            //Now we will create a connection



            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            //Now we will create a command

            OdbcCommand historyBuySellRating = new OdbcCommand("SELECT * FROM historyBuySellRating ORDER BY symbol,ratingdate", con);


            try
            {
                con.Open();
                OdbcDataReader dr = historyBuySellRating.ExecuteReader();

                while (dr.Read())
                {
                    buySellRatingHist.Add(new BuySellRating
                    {
                        symbol = dr.GetString(0).ToString(),
                        ratingDate = DateTime.Parse(dr.GetString(2)),
                        rating = Convert.ToInt32(dr[1]),

                    });
                }
                dr.Close();
                con.Close();
            }
            catch (Exception ex)
            {
                log.Error("ERROR \n" + "============ \n" + ex.ToString());
            }
            return buySellRatingHist;
        }


        internal static void InsertChangeRatingHistoryCSVToDB(string filename, String buysellRatingChangeHistTbl)
        {
            //Delete

            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            OdbcCommand deleteCommand = new OdbcCommand("DELETE from " + buysellRatingChangeHistTbl, con);

            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE" + " '" + filename + "/ChangeRatingHistoryFile.csv' " +
                                              "INTO TABLE  " + buysellRatingChangeHistTbl +
                                              " FIELDS TERMINATED BY ',' " +
                                              "LINES TERMINATED BY '\n' " +
                                              "(symbol,newRating,oldRating,ratingDate);", con);


            try
            {
                con.Open();
                deleteCommand.ExecuteReader();
                insertCommand.ExecuteReader();
                log.Info("Historical BuySell Change rating File " + filename + " Saved....");

                con.Close();
            }
            catch (OdbcException ex)
            {
                log.Error("ERROR \n" + "============ \n" + ex.ToString());
            }
        }

        public static Dictionary<DateTime, double> GetDataForDateAlertFromDB(string symbol)
        {

            Dictionary<DateTime, double> DatePriceList = new Dictionary<DateTime, double>();
            log.Info("\n\n\n\n\n Getting Data from DB for symbol " + symbol);

            try
            {
                OdbcConnection con = new OdbcConnection(Constants.MyConString);

                OdbcCommand com = new OdbcCommand("SELECT date,close from  symbolshistorical where date>='2012-02-22' and symbol = '" + symbol + "'", con);
                //OdbcCommand com = new OdbcCommand("SELECT date,close,volume from  symbolshistorical where symbol = 'SPY' order by date desc", con);
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {

                    DateTime date = DateTime.Parse(dr.GetString(0));
                    double price = Double.Parse(dr.GetString(1));
                    if (!DatePriceList.ContainsKey(date))
                    {
                        DatePriceList.Add(date, price);
                    }
                    else
                    {
                        log.Error("Duplicate data in symbolshistorical for symbol: " + symbol + " for Date: " + date);
                    }

                }
                dr.Close();
                con.Close();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return DatePriceList;

        }


        public static List<DateForSymbolAlert> GetCTRatingForSymbol()
        {
            List<DateForSymbolAlert> DateListForAlert = new List<DateForSymbolAlert>();
            OdbcConnection con = new OdbcConnection(Constants.MyConString);


            OdbcCommand sentimentAlerts = new OdbcCommand("SELECT symbol,DATE FROM snpcthistory", con);



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

        public static List<CTRatingHistory> GetCtRatingHistoryForSnP(bool isHistorical)
        {
            List<CTRatingHistory> historyList = new List<CTRatingHistory>();
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = null;
            if (isHistorical)
            {
                com = new OdbcCommand("SELECT symbol,ratingdate,ctrating FROM historyBuySellRating WHERE symbol='" + Constants.SnPSymbol + "'", con);
            }
            else
            {
                com = new OdbcCommand("SELECT symbol,ratingdate,ctrating FROM historyBuySellRating WHERE symbol='" + Constants.SnPSymbol + "' AND ratingdate=(SELECT DATE FROM historicaldates WHERE DateType='" + Constants.C + "')", con);
            }


            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();
                while (dr.Read())
                {
                    object rating = dr.GetValue(2);
                    int neutral = (int)CTRatingEnum.Neutral;

                    // int intrating =int.Parse( dr.GetString(2));
                    if (!Convert.IsDBNull(rating))
                    {
                        int ratingValue = (int)dr.GetValue(2);
                        if (ratingValue != neutral)
                        {
                            CTRatingHistory CTratingObj = new CTRatingHistory();
                            CTratingObj.symbol = dr.GetString(0);
                            CTratingObj.Date = (DateTime)dr.GetValue(1);
                            CTratingObj.ctRating = (int)dr.GetValue(2);
                            historyList.Add(CTratingObj);
                        }
                    }
                }
                dr.Close();
                con.Close();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return historyList;

        }

        public static void InsertCTRatingSnPHistory(string foldername, bool forHistory)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand deleteCommand = new OdbcCommand("DELETE from SnPcthistory", con);

            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE '" + foldername + "/CTRatingHistory.csv' " +
                                                                    "INTO TABLE ctratingchangehistory " +
                                                                    "FIELDS TERMINATED BY ',' " +
                                                                    "LINES TERMINATED BY '\n' " +
                                                                    "(symbol,date,ctrating);", con);



            try
            {
                con.Open();
                if (forHistory)
                {
                    deleteCommand.ExecuteNonQuery();
                }
                insertCommand.ExecuteReader();
                log.Info("\nCounter Trend History for S&P Saved....\n");

                con.Close();
            }
            catch (OdbcException ex)
            {
                log.Error(ex);
            }
        }
        public static void UpdateCTRatingPerf(string foldername)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand deleteCommand = new OdbcCommand("DELETE from snpctperfmnce", con);

            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE '" + foldername + "/CTRatingPerf.csv' " +
                                                                    "INTO TABLE snpctperfmnce " +
                                                                    "FIELDS TERMINATED BY ',' " +
                                                                    "LINES TERMINATED BY '\n' " +
                                                                    "(symbol,oneweekavgreturn,onemonthavgreturn,twomonthavgreturn);", con);



            try
            {
                con.Open();
                deleteCommand.ExecuteNonQuery();
                insertCommand.ExecuteReader();
                log.Info("\nCounter Trend Performance for S&P Updated....\n");

                con.Close();
            }
            catch (OdbcException ex)
            {
                log.Error(ex);
            }
        }

        internal static List<DateTime> GetDistinctRatingDatesFromDB()
        {
            List<DateTime> DateList = new List<DateTime>();

            try
            {
                OdbcConnection con = new OdbcConnection(Constants.MyConString);

                OdbcCommand com = new OdbcCommand(" SELECT DISTINCT ratingDate FROM historybuysellrating", con);
                //OdbcCommand com = new OdbcCommand("SELECT date,close,volume from  symbolshistorical where symbol = 'SPY' order by date desc", con);
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {

                    DateTime date = DateTime.Parse(dr.GetString(0));

                    DateList.Add(date);



                }
                dr.Close();
                con.Close();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return DateList;



        }

        internal static List<CTRatingHistory> getCTRatingHistroyFromDB()
        {
            List<CTRatingHistory> ctRatingHist = new List<CTRatingHistory>();

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand historyBuySellRating = new OdbcCommand("SELECT symbol,ratingdate,ctrating FROM historyBuySellRating ORDER BY symbol,ratingdate", con);


            try
            {

                con.Open();
                OdbcDataReader dr = historyBuySellRating.ExecuteReader();

                while (dr.Read())
                {
                    object ctr = dr.GetValue(2);

                    if (!Convert.IsDBNull(ctr))
                    {

                        ctRatingHist.Add(new CTRatingHistory
                        {
                            symbol = dr.GetString(0),
                            Date = dr.GetDateTime(1),
                            ctRating = Convert.ToInt32(dr.GetInt32(2)),

                        });
                    }
                }
                dr.Close();
                con.Close();
            }
            catch (Exception ex)
            {
                log.Error("ERROR \n" + "============ \n" + ex.ToString());
            }
            return ctRatingHist;
        }

        internal static void InsertChangeCTRatingHistoryCSVToDB(string foldername,String ctRatingChangeTableName)
        {
            //Delete

            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            OdbcCommand deleteCommand = new OdbcCommand("DELETE from  " + ctRatingChangeTableName, con);

            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE" + " '" + foldername + "/ChangeCTRatingHistoryFile.csv' " +
                                              "INTO TABLE  " + ctRatingChangeTableName +
                                              " FIELDS TERMINATED BY ',' " +
                                              "LINES TERMINATED BY '\n' " +
                                              "(symbol,ctratingcurr,ctratingprev,changedate);", con);


            try
            {
                con.Open();
                deleteCommand.ExecuteReader();
                insertCommand.ExecuteReader();
                log.Info("Historical CT Change rating File Saved....");

                con.Close();
            }
            catch (OdbcException ex)
            {
                log.Error("ERROR \n" + "============ \n" + ex.ToString());
            }
        }

        internal static Dictionary<string, double> GetSymbolsWeight()
        {

            Dictionary<string, double> symbolsWeightDict = new Dictionary<string, double>();



            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand("SELECT symbol,weight from  snpsymbolsweightage", con);

            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {

                    string symbol = dr.GetString(0);
                    double weight = Double.Parse(dr.GetString(1));

                    if (!symbolsWeightDict.ContainsKey(symbol))
                    {
                        symbolsWeightDict.Add(symbol, weight);
                    }


                }
                dr.Close();
                con.Close();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return symbolsWeightDict;

        }

        internal static Dictionary<string, double> GetSymbolsRating()
        {
            Dictionary<string, double> symbolsRatingDict = new Dictionary<string, double>();



            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand("SELECT symbol,rating from  temp_buysellrating", con);

            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {

                    string symbol = dr.GetString(0);
                    double rating = Double.Parse(dr.GetString(1));

                    if (!symbolsRatingDict.ContainsKey(symbol))
                    {
                        symbolsRatingDict.Add(symbol, rating);
                    }


                }
                dr.Close();
                con.Close();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return symbolsRatingDict;
        }

        internal static void InsertSnPAvgRating(BuySellRating SnPAvgRating)
        {

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand deleteBuySellRatingCmd = new OdbcCommand("delete from historybuysellrating where ratingdate=DATE(NOW()) and symbol='" + SnPAvgRating.symbol + "'", con);

            OdbcCommand insertCommand = new OdbcCommand("INSERT INTO historybuysellrating (symbol,rating,ratingvalue,ratingdate) Values('" + SnPAvgRating.symbol + "'," + SnPAvgRating.rating + "," + SnPAvgRating.ratingValue + ",(SELECT DATE FROM historicaldates WHERE DateType='" + Constants.C + "'));", con);

            OdbcCommand updateCommand = new OdbcCommand("UPDATE symbolanalytics SET buySellRating=" + SnPAvgRating.rating + " WHERE symbol='" + SnPAvgRating.symbol + "';", con);

            try
            {
                con.Open();
                deleteBuySellRatingCmd.ExecuteNonQuery();
                insertCommand.ExecuteNonQuery();
                updateCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        internal static List<SymbolRatingAlert> getRatingChangeHistory(string symbol)
        {
            List<SymbolRatingAlert> SymbolRatingchanges = new List<SymbolRatingAlert>();
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand("SELECT bs.symbol,bs.oldRating,bs.newRating,bs.ratingDate FROM   buysellratingchangehistory bs WHERE bs.symbol='" + symbol + "' ORDER BY bs.ratingDate ASC", con);

            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    SymbolRatingchanges.Add(new SymbolRatingAlert
                    {
                        symbol = dr.GetString(0),
                        prevrating = int.Parse(dr.GetString(1)),
                        currating = int.Parse(dr.GetString(2)),
                        ratingChangeDate = DateTime.Parse(dr.GetString(3))


                    });

                }
                dr.Close();
                con.Close();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return SymbolRatingchanges;
        }

        internal static SymbolRatingAlert getSymbolCurrentRating(string symbol)
        {
            SymbolRatingAlert symbolRating = new SymbolRatingAlert();
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand("SELECT sa.symbol,sa.previuosBSRating,sa.buySellRating,sa.preSBRatingDate FROM   symbolanalytics sa WHERE sa.symbol= '" + symbol + "'", con);

            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    
                    symbolRating.symbol = dr.GetString(0);
                    if (dr.GetValue(1) != DBNull.Value)
                        symbolRating.prevrating = dr.GetInt32(0);
                    if (dr.GetValue(2) != DBNull.Value)
                        symbolRating.currating = Convert.ToInt32(dr.GetValue(2));
                    symbolRating.ratingChangeDate = DateTime.Parse(dr.GetString(3));
                }
                dr.Close();
                con.Close();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return symbolRating;
        }
    }
}
