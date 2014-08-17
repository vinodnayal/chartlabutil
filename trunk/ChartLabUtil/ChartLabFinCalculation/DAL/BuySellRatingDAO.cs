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


            OdbcCommand insertCommandSPY = new OdbcCommand("INSERT INTO temp_buysellrating(symbol,rating,ratingvalue,ctrating,ctratingvalue) SELECT '^GSPC',rating,ratingvalue,ctrating,ctratingvalue FROM temp_buysellrating WHERE symbol ='SPY'", con);
            OdbcCommand insertCommandGOOG = new OdbcCommand("INSERT INTO temp_buysellrating(symbol,rating,ratingvalue,ctrating,ctratingvalue) SELECT 'GOOG',rating,ratingvalue,ctrating,ctratingvalue FROM temp_buysellrating WHERE symbol='GOOGL'", con);


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
            OdbcCommand insertTop20symbols = new OdbcCommand("INSERT INTO topRatingSymbols (symbol,ratingvalue) " +
                                    "SELECT t.symbol, t.ratingvalue FROM temp_buySellRating AS t LEFT JOIN equitiesfundamental AS e " +
                                    "ON t.symbol=e.symbol LEFT JOIN symbolanalytics sa ON sa.symbol= t.symbol " +
                                    "WHERE e.sectorId IS NOT NULL " +
                                    "ORDER BY ratingvalue DESC,DATEDIFF(CURDATE(),sa.preSBRatingDate) DESC,ctratingvalue,symbol LIMIT 20", con);

            OdbcCommand deletetopRatingSymbolsHist = new OdbcCommand("DELETE from topRatingSymbolshist where ratingdate='" + DateTime.Now.ToString("yyyy-MM-dd") + "'", con);
            OdbcCommand insertTop20symbolsHist = new OdbcCommand("INSERT INTO topRatingSymbolshist (symbol,ratingvalue,ratingdate) " +
                                    "SELECT t.symbol, t.ratingvalue,'" + DateTime.Now.ToString("yyyy-MM-dd") + "' FROM temp_buySellRating AS t LEFT JOIN equitiesfundamental AS e " +
                                    "ON t.symbol=e.symbol LEFT JOIN symbolanalytics sa ON sa.symbol= t.symbol " +
                                    "WHERE e.sectorId IS NOT NULL " +
                                    "ORDER BY  ratingvalue DESC,DATEDIFF(CURDATE(),sa.preSBRatingDate) DESC,ctratingvalue,symbol LIMIT 20", con);

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
                insertCommandGOOG.ExecuteReader();
                insertCommandSPY.ExecuteReader();
                updateCommand.ExecuteReader();

                deleteBuySellRatingCmd.ExecuteNonQuery();
                inserthistoryBuySellRatingCmd.ExecuteNonQuery();
                log.Info("Rating: getting BuySell Rating Histroy From DB");
                List<BuySellRating> historyBuySellRatingList = BuySellRatingDAO.getBuySellRatingHistroyFromDB();
                log.Info("Rating: getting Change BuySell Rating Hist");
                List<BuySellRatingChangeHist> ChangeBuySellRatingHist = Util.getBuySellRatingChangelist(historyBuySellRatingList);
                CSVExporter.WriteToCSVChangeRatingHistory(ChangeBuySellRatingHist, BuySellRatingCalculation.BuySellRatingChangeHistCsvFilePath + "/ChangeRatingHistoryFile.csv");
                log.Info("Rating: Write To CSV Change Rating History");
                BuySellRatingDAO.InsertChangeRatingHistoryCSVToDB(BuySellRatingCalculation.BuySellRatingChangeHistCsvFilePath, "buySellRatingChangeHistory");
                log.Info("Rating: Inserted Change Rating History CSV To DB ");

                insertTempPrevRatingCmd.ExecuteReader();
                deletetopRatingSymbols.ExecuteNonQuery();
                insertTop20symbols.ExecuteReader();

                deletetopRatingSymbolsHist.ExecuteNonQuery();
                insertTop20symbolsHist.ExecuteReader();

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

            }
            catch (OdbcException ex)
            {
                throw ex;
            }
            finally
            {
                if (con != null)
                    con.Close();
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

            }
            catch (OdbcException ex)
            {
                log.Error(ex);
            }
            finally
            {
                if (con != null)
                    con.Close();
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

            }
            catch (OdbcException ex)
            {
                log.Error(ex);
            }
            finally
            {
                if (con != null)
                    con.Close();
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
                    BuySellRating rt = new BuySellRating();
                    try
                    {
                        
                        rt.symbol = dr.GetString(0).ToString();
                            rt.ratingDate = DateTime.Parse(dr.GetString(2));
                          rt.rating = Convert.ToInt32(dr[1]);
                          
                        
                        buySellRatingHist.Add(rt);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            log.Error("VINOD 1 ERROR in data " + ex.ToString());
                            log.Error("Symbol : " + rt.symbol);
                            log.Error("DATE:" + dr.GetString(2));
                        }
                        catch (Exception ex1)
                        {


                        }
                        
                 

                    }
                }
                dr.Close();

            }
            catch (Exception ex)
            {
                log.Error("VINOD 2 ERROR \n" + "============ \n" + ex.ToString());
            }
            finally
            {
                if (con != null)
                    con.Close();
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


            }
            catch (OdbcException ex)
            {
                log.Error("ERROR \n" + "============ \n" + ex.ToString());
            }
            finally
            {
                if (con != null)
                    con.Close();
            }
        }

        public static Dictionary<DateTime, double> GetDataForDateAlertFromDB(string symbol)
        {

            Dictionary<DateTime, double> DatePriceList = new Dictionary<DateTime, double>();
            log.Info("\n\n\n\n\n Getting Data from DB for symbol " + symbol);
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            try
            {


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


            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (con != null)
                    con.Close();
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

            }
            catch (Exception ex)
            {
                log.Info("ERROR \n" + "============ \n" + ex.ToString());
            }
            finally
            {
                if (con != null)
                    con.Close();
            }
            return DateListForAlert;
        }

        public static List<Rating> GetRatingsOfSymbol(String symbol, bool isHistorical)
        {
            List<Rating> symbolRatings = new List<Rating>();
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = null;
            if (isHistorical)
            {
                com = new OdbcCommand("SELECT symbol,ratingdate,ctrating FROM historyBuySellRating WHERE symbol='" + symbol + "'", con);
            }
            else
            {
                com = new OdbcCommand("SELECT symbol,ratingdate,ctrating FROM historyBuySellRating WHERE symbol='" + symbol + "' AND ratingdate=(SELECT DATE FROM historicaldates WHERE DateType='" + Constants.C + "')", con);
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
                            Rating ratingObj = new Rating();
                            ratingObj.symbol = dr.GetString(0);
                            ratingObj.ratingDate = (DateTime)dr.GetValue(1);
                            ratingObj.ctRating = (int)dr.GetValue(2);
                            symbolRatings.Add(ratingObj);
                        }
                    }
                }
                dr.Close();

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                if (con != null)
                    con.Close();
            }

            return symbolRatings;

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


            }
            catch (OdbcException ex)
            {
                log.Error(ex);
            }
            finally
            {
                if (con != null)
                    con.Close();
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

            }
            catch (OdbcException ex)
            {
                log.Error(ex);
            }
            finally
            {
                if (con != null)
                    con.Close();
            }
        }

        internal static List<DateTime> GetDistinctRatingDatesFromDB(DateTime fromDate, int backDateCount)
        {
            List<DateTime> DateList = new List<DateTime>();
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            try
            {
                String sqlQuery = "";
                if (fromDate == DateTime.MinValue)
                {
                    if (backDateCount == 0)
                        sqlQuery = " SELECT DISTINCT ratingDate FROM historybuysellrating where ratingDate >='2012-02-02'  order by ratingDate";
                    else
                        sqlQuery = @" SELECT ratingDate FROM (SELECT DISTINCT ratingDate FROM historybuysellrating ORDER BY ratingDate DESC LIMIT 2) temp
                                      ORDER BY  ratingDate";
                }
                else
                {
                    sqlQuery = " SELECT DISTINCT ratingDate FROM historybuysellrating where ratingDate>='" + fromDate.ToString("yyyy-MM-dd") + "' order by ratingDate";
                }
                OdbcCommand com = new OdbcCommand(sqlQuery, con);

                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    DateTime date = DateTime.Parse(dr.GetString(0));
                    DateList.Add(date);
                }


                dr.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (con != null)
                    con.Close();
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

            }
            catch (Exception ex)
            {
                log.Error("ERROR \n" + "============ \n" + ex.ToString());
            }
            finally
            {
                if (con != null)
                    con.Close();
            }
            return ctRatingHist;
        }

        internal static void InsertChangeCTRatingHistoryCSVToDB(string foldername, String ctRatingChangeTableName)
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


            }
            catch (OdbcException ex)
            {
                log.Error("ERROR \n" + "============ \n" + ex.ToString());
            }
            finally
            {
                if (con != null)
                    con.Close();
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


            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (con != null)
                    con.Close();
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

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (con != null)
                    con.Close();
            }
            return symbolsRatingDict;
        }

        internal static void InsertSnPAvgRating(Rating SnPAvgRating)
        {

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand deleteBuySellRatingCmd = new OdbcCommand("delete from historybuysellrating where ratingdate=DATE(NOW()) and symbol='" + SnPAvgRating.symbol + "'", con);

            OdbcCommand insertCommand = new OdbcCommand("INSERT INTO historybuysellrating (symbol,rating,ratingvalue,ctrating,ctratingvalue,ratingdate) Values('" + SnPAvgRating.symbol + "'," + SnPAvgRating.rating + "," + SnPAvgRating.ratingValue + "," + SnPAvgRating.ctRating + "," + SnPAvgRating.ctRatingValue + ",(SELECT DATE FROM historicaldates WHERE DateType='" + Constants.C + "'));", con);

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
            finally
            {
                if (con != null)
                    con.Close();
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
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (con != null)
                    con.Close();
            }
            return SymbolRatingchanges;
        }

        internal static List<SymbolRatingAlert> getSNPSymbolsRatingChange()
        {
            List<SymbolRatingAlert> SymbolRatingchanges = new List<SymbolRatingAlert>();
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand(@"SELECT sa.symbol,sa.previuosBSRating,sa.buySellRating,sa.ctrating,sa.preSBRatingDate FROM symbolanalytics sa
INNER JOIN temp_buysellrating t ON t.symbol=sa.symbol", con);

            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    SymbolRatingAlert symbolalert = new SymbolRatingAlert();

                    if (dr.GetValue(0) != DBNull.Value)
                    {
                        symbolalert.symbol = dr.GetString(0);
                    }
                    if (dr.GetValue(1) != DBNull.Value)
                    {
                        symbolalert.prevrating = Convert.ToInt32(dr.GetValue(1));
                    }
                    if (dr.GetValue(2) != DBNull.Value)
                    {
                        symbolalert.currating = Convert.ToInt32(dr.GetValue(2));
                    }
                    if (dr.GetValue(3) != DBNull.Value)
                    {
                        symbolalert.ctrating = Convert.ToInt32(dr.GetValue(3));
                    }
                    if (dr.GetValue(4) != DBNull.Value)
                    {
                        symbolalert.ratingChangeDate = dr.GetDateTime(4);
                    }


                    SymbolRatingchanges.Add(symbolalert);

                }
                dr.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (con != null)
                    con.Close();
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

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (con != null)
                    con.Close();
            }
            return symbolRating;
        }

        internal static void updateETFRatingsfromHistBSRatingTbl()
        {

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand deleteComm = new OdbcCommand(@" delete from etfhistbsctrating  WHERE ratingdate= (SELECT MAX(ratingdate) FROM historybuysellrating)", con);

            OdbcCommand insertComm = new OdbcCommand(@"INSERT INTO etfhistbsctrating (symbol,rating,ratingdate,ratingValue,ctrating,ctratingvalue)
                                                SELECT e.symbol,rating,ratingdate,ratingValue,ctrating,ctratingvalue FROM  historybuysellrating h
                                                INNER JOIN etfsymbols e ON e.symbol= h.symbol
                                                WHERE ratingdate= (SELECT MAX(ratingdate) FROM historybuysellrating)", con);
            try
            {
                con.Open();
                deleteComm.ExecuteNonQuery();
                insertComm.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (con != null)
                    con.Close();
            }

        }


        internal static void insertTopRatingAddRemoveHist(DateTime curdate, DateTime preDayDate)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            OdbcCommand deleteCmd = new OdbcCommand("delete from topratingsymbolsaddremove where date='" + curdate.ToString("yyyy-MM-dd") + "'", con);
            OdbcCommand addedSymbolsCommand = new OdbcCommand(@"INSERT INTO topratingsymbolsaddremove(DATE,symbol,isadded)
SELECT t2.ratingdate,t2.symbol,1 FROM topratingsymbolshist  t2
LEFT JOIN (SELECT symbol,ratingdate FROM topratingsymbolshist WHERE ratingdate = '" + preDayDate.ToString("yyyy-MM-dd") + "') AS t1 ON t1.symbol=t2.symbol" +
" WHERE t2.ratingdate= '" + curdate.ToString("yyyy-MM-dd") + "' AND t1.symbol IS NULL", con);

            OdbcCommand removedSymbolsCommand = new OdbcCommand(@"INSERT INTO topratingsymbolsaddremove(DATE,symbol,isadded) " +
" SELECT '" + curdate.ToString("yyyy-MM-dd") + "',t2.symbol,0  FROM topratingsymbolshist  t2 " +
"  LEFT JOIN (SELECT symbol,ratingdate FROM topratingsymbolshist WHERE ratingdate = '" + curdate.ToString("yyyy-MM-dd") + "' ) AS t1 ON t1.symbol=t2.symbol " +
" WHERE t2.ratingdate= '" + preDayDate.ToString("yyyy-MM-dd") + "' AND t1.symbol IS NULL", con);


            try
            {
                con.Open();
                deleteCmd.ExecuteNonQuery();
                addedSymbolsCommand.ExecuteNonQuery();
                removedSymbolsCommand.ExecuteNonQuery();

                log.Info("insert Top Rating Symbols Hist....");

            }
            catch (OdbcException ex)
            {
                throw ex;
            }
            finally
            {
                if (con != null)
                    con.Close();
            }
        }

        internal static List<Rating> getTopRatigSymbolOnSpecificDate(DateTime date)
        {
            List<Rating> topRatingSymbolsList = new List<Rating>();
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            // for testing
            //SELECT t.symbol, t.ratingvalue, t.ratingdate,ctratingvalue,DATEDIFF(CURDATE(),sa.preSBRatingDate) FROM historybuysellrating AS t 
            //LEFT JOIN equitiesfundamental AS e ON t.symbol=e.symbol 
            //LEFT JOIN symbolanalytics sa ON sa.symbol= t.symbol
            //WHERE t.ratingdate='2012-09-27' ORDER BY ratingvalue DESC,DATEDIFF(CURDATE(),sa.preSBRatingDate) DESC,ctratingvalue,symbol LIMIT 20

            OdbcCommand com = new OdbcCommand(@"SELECT t.symbol, t.ratingvalue, t.ratingdate FROM historybuysellrating AS t LEFT JOIN equitiesfundamental AS e ON t.symbol=e.symbol LEFT JOIN symbolanalytics sa ON sa.symbol= t.symbol  WHERE t.ratingdate='" + date.ToString("yyyy-MM-dd") + "' ORDER BY ratingvalue DESC,DATEDIFF(CURDATE(),sa.preSBRatingDate) DESC,ctratingvalue,symbol LIMIT 20", con);

            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    Rating symbolRating = new Rating();
                    if (dr.GetValue(0) != DBNull.Value)
                        symbolRating.symbol = dr.GetString(0);
                    if (dr.GetValue(1) != DBNull.Value)
                        symbolRating.ratingValue = dr.GetFloat(1);
                    if (dr.GetValue(2) != DBNull.Value)
                        symbolRating.ratingDate = dr.GetDateTime(2);
                    topRatingSymbolsList.Add(symbolRating);
                }
                dr.Close();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (con != null)
                    con.Close();
            }
            return topRatingSymbolsList;
        }

        internal static void InsertTopRatingSymbolsHistCSVToDB(string BuySellRatingCsvFilePath)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            OdbcCommand deleteCommand = new OdbcCommand("DELETE from  topratingsymbolshist", con);

            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE" + " '" + BuySellRatingCsvFilePath + "/TopRatingSymbolsHist.csv' " +
                                              "INTO TABLE  topratingsymbolshist" +
                                              " FIELDS TERMINATED BY ',' " +
                                              "LINES TERMINATED BY '\n' " +
                                              "(symbol,ratingvalue,ratingdate);", con);


            try
            {
                con.Open();
                deleteCommand.ExecuteReader();
                insertCommand.ExecuteReader();
                log.Info("Rating: topratingsymbolshist table updated....");


            }
            catch (OdbcException ex)
            {
                log.Error("ERROR \n" + "============ \n" + ex.ToString());
            }
            finally
            {
                if (con != null)
                    con.Close();
            }
        }

        internal static Dictionary<int, double> GetCTRatingValueOfSPY()
        {
            Dictionary<int, double> spyCtRating = new Dictionary<int, double>();
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand("SELECT ctrating,ctratingvalue FROM temp_buysellrating WHERE symbol='SPY'", con);

            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {


                    if (dr.GetValue(0) != DBNull.Value && dr.GetValue(1) != DBNull.Value)
                        spyCtRating.Add(dr.GetInt32(0), dr.GetFloat(1));


                }
                dr.Close();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (con != null)
                    con.Close();
            }
            return spyCtRating;

        }

        /// <summary>
        /// getting snp symbols hist ratings
        /// </summary>
        /// <returns> symbol wise rating Dict</returns>
        internal static Dictionary<String, List<Rating>> getSNPSymbolsHistRatings(DateTime date)
        {
            Dictionary<String, List<Rating>> symbolsRatingsDict = new Dictionary<string, List<Rating>>();

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com;
            string sqlString = @"SELECT  s.symbol,bs.ratingvalue,bs.ctratingvalue,bs.ratingdate,1 AS assetid,bs.rating FROM temp_buysellrating s
                                LEFT JOIN historybuysellrating bs ON s.symbol=bs.symbol
                                WHERE bs.ratingdate <= '" + date.ToString("yyyy-MM-dd") + "' AND bs.ratingdate > DATE_ADD('" + date.ToString("yyyy-MM-dd") + "',INTERVAL -25 DAY) ORDER BY symbol ASC, bs.ratingdate DESC ";

            com = new OdbcCommand(sqlString, con);

            try
            {
                con.Open();

                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    Rating rating = new Rating();

                    if (!Convert.IsDBNull(dr.GetValue(0)))
                    {
                        rating.symbol = dr.GetString(0);

                    }

                    if (!Convert.IsDBNull(dr.GetValue(1)))
                    {
                        rating.ratingValue = dr.GetFloat(1);

                    }
                    if (!Convert.IsDBNull(dr.GetValue(2)))
                    {
                        rating.ctRatingValue = dr.GetFloat(2);

                    }
                    if (!Convert.IsDBNull(dr.GetValue(3)))
                    {
                        rating.ratingDate = dr.GetDateTime(3);

                    }
                    if (!Convert.IsDBNull(dr.GetValue(5)))
                    {
                        rating.rating = dr.GetInt32(5);

                    }
                    if (symbolsRatingsDict.ContainsKey(rating.symbol))
                    {
                        symbolsRatingsDict[rating.symbol].Add(rating);

                    }
                    else
                    {
                        List<Rating> symbolRating = new List<Rating>();
                        symbolRating.Add(rating);
                        symbolsRatingsDict.Add(rating.symbol, symbolRating);
                    }


                }



            }
            catch (OdbcException ex)
            {
                log.Error(ex);
            }
            finally
            {
                con.Close();
            }


            return symbolsRatingsDict;
        }
    }
}
