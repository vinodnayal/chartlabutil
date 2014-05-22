using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using FinLib;

namespace ChartLabFinCalculation
{
    class SectorPerformanceDAO
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(SectorPerformanceDAO));

        public static Dictionary<int, int> getTodayTopRatingSymbolStocks()
        {
            Dictionary<int, int> topTodayRatingSymbolList = new Dictionary<int, int>();

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand("SELECT temp3.sectorId,COUNT(*) AS stocks FROM " +
                                            "(SELECT t.symbol,t.ratingValue,e.sectorId FROM topratingsymbols AS t JOIN equitiesfundamental AS e " +
                                            "ON t.symbol=e.symbol) temp3 " +
                                            "GROUP BY temp3.sectorId ", con);

            OdbcCommand insert = new OdbcCommand("INSERT INTO sectorstocksymbols (sectorid,symbol,stockdayid) " +
                                            "(SELECT e.sectorId,t.symbol,1 FROM topratingsymbols AS t JOIN equitiesfundamental AS e " +
                                            "ON t.symbol=e.symbol) ", con);

            try
            {
                con.Open();

                OdbcDataReader dr = com.ExecuteReader();
                log.Info("\nGetting Todays Stock List...\n");
                while (dr.Read())
                {
                    int secId = dr.GetInt32(0);
                    int stocks = dr.GetInt32(1);
                    if (!topTodayRatingSymbolList.ContainsKey(secId))
                    {
                        topTodayRatingSymbolList.Add(secId, stocks);
                    }
                }
                dr.Close();

                insert.ExecuteNonQuery();
                log.Info("Data inserted into sectorstocksymbols Table...");

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


            return topTodayRatingSymbolList;
        }


        public static Dictionary<int, int> getfiveDaysTopRatingSymbolStocks()
        {
            Dictionary<int, int> topfiveDaysRatingSymbolList = new Dictionary<int, int>();

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            String sql1="SELECT temp1.sectorId,COUNT(*) AS stocks FROM " +
                                            "(SELECT h.symbol,h.ratingValue,e.sectorId  FROM historybuysellrating AS h JOIN equitiesfundamental AS e " +
                                            "ON h.symbol=e.symbol LEFT JOIN symbolanalytics sa ON sa.symbol= h.symbol " +
                                            "WHERE h.ratingdate=(SELECT DATE FROM historicaldates WHERE Datetype='" + Constants.D_5 + "' and  e.sectorid is not null)  " +
                                            "ORDER BY h.ratingvalue DESC,DATEDIFF(CURDATE(),sa.preSBRatingDate) DESC,ctratingvalue,symbol LIMIT 20) temp1 " +
                                            "GROUP BY temp1.sectorId";
            OdbcCommand com = new OdbcCommand(sql1, con);

            OdbcCommand insert = new OdbcCommand("INSERT INTO sectorstocksymbols (sectorid,symbol,stockdayid) " +
                                                "(SELECT e.sectorId,h.symbol,2 FROM historybuysellrating AS h JOIN equitiesfundamental AS e " +
                                                "ON h.symbol=e.symbol LEFT JOIN symbolanalytics sa ON sa.symbol= h.symbol " +
                                                "WHERE h.ratingdate=(SELECT DATE FROM historicaldates WHERE Datetype='5days') and e.sectorid is not null  " +
                                                "ORDER BY h.ratingvalue DESC,DATEDIFF(CURDATE(),sa.preSBRatingDate ) DESC,ctratingvalue,symbol LIMIT 20)", con);

            try
            {
                con.Open();

                OdbcDataReader dr = com.ExecuteReader();
                log.Info("\nGetting Five Todays Stock List...\n");
                while (dr.Read())
                {
                    int secId = dr.GetInt32(0);
                    int stocks = dr.GetInt32(1);
                    if (!topfiveDaysRatingSymbolList.ContainsKey(secId))
                    {
                        topfiveDaysRatingSymbolList.Add(secId, stocks);
                    }
                }
                dr.Close();

                insert.ExecuteNonQuery();
                log.Info("Data inserted into sectorstocksymbols Table...");

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


            return topfiveDaysRatingSymbolList;
        }

        public static void insertSectorPerf()
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand deleteCommand = new OdbcCommand("DELETE FROM sectorperfmnce", con);
            OdbcCommand insertCommand = new OdbcCommand("INSERT INTO sectorperfmnce (sectorid) (SELECT sectorId FROM sectors)", con);

            try
            {
                con.Open();
                deleteCommand.ExecuteNonQuery();
                insertCommand.ExecuteNonQuery();
                log.Info("Data Inserted into sectorperfmnce Table...");

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


        internal static void updateTodaysStock(int sectorId, int todayStocks)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand updateCommand = new OdbcCommand("UPDATE sectorperfmnce SET today_nuofstocks=" + todayStocks + " WHERE sectorid=" + sectorId, con);

            try
            {
                con.Open();
                updateCommand.ExecuteNonQuery();
                log.Info("Data Updating in sectorperfmnce Table...");

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

        internal static void updatefiveDaysStock(int sectorId, int fivedayStocks)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand updateCommand = new OdbcCommand("UPDATE sectorperfmnce SET 5days_nuofstocks=" + fivedayStocks + " WHERE sectorid=" + sectorId, con);

            try
            {
                con.Open();
                updateCommand.ExecuteNonQuery();
                log.Info("Data Updating in sectorperfmnce Table...");

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

        public static Dictionary<int, SectorPerfRating> getRatingAvgValue()
        {
            Dictionary<int, SectorPerfRating> sectorRatingList = new Dictionary<int, SectorPerfRating>();

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand("SELECT sectorId,AVG(ratingvalue) AS ratingaverage,AVG(ctratingvalue) AS ctratingaverage FROM " +
                                            "(SELECT t.symbol,t.ratingvalue,t.ctratingvalue,e.sectorId FROM temp_buysellrating AS t JOIN equitiesfundamental AS e " +
                                            "ON t.symbol=e.symbol) temp " +
                                            "GROUP BY sectorId ", con);


            try
            {
                con.Open();

                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    if (!Convert.DBNull.Equals(dr.GetValue(0)))
                    {
                        int secId = dr.GetInt32(0);
                        double ratingValue = double.Parse(dr.GetString(1));
                        double ctratingValue = double.Parse(dr.GetString(2));

                        SectorPerfRating ratingObj = new SectorPerfRating();

                        RatingEnum rating = BuySellRatingCalculation.calculateBSRatingEnum(ratingValue);
                        ratingObj.rating = (int)rating;
                        ratingObj.ratingValue = ratingValue;
                        CTRatingEnum ctrating = CTRatingCalculation.calculateCTRatingEnum(ctratingValue);
                        ratingObj.ctrating = (int)ctrating;
                        ratingObj.ctRatingValue = ctratingValue;
                        if (!sectorRatingList.ContainsKey(secId))
                        {
                            sectorRatingList.Add(secId, ratingObj);
                        }
                    }
                }
                dr.Close();


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


            return sectorRatingList;
        }

        internal static void updateAvgRating(int sectorId, int rating, int ctrating, double ratingValue, double ctRatingValue,double ratingChangePct,int signalId)
        {

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand updateCommand = new OdbcCommand("UPDATE sectorperfmnce SET bsrating=" + rating + " ,ctrating=" + ctrating + ", bsratingvalue=" + ratingValue + " ,ctratingvalue=" + ctRatingValue +
                                                        " ,ratingchangepct=" + ratingChangePct + " ,strengthalertid=" + signalId + " WHERE sectorid=" + sectorId, con);

            try
            {
                con.Open();
                updateCommand.ExecuteNonQuery();
                log.Info("Data Updating in sectorperfmnce Table...");

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

        internal static double getSectorWiseAvgRating(DateTime date, int secId)
        {

            string dbDate = date.Date.ToString("yyyy-MM-dd");
            double ratingValue = 0;

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand("SELECT e.sectorId,h.ratingdate,AVG(h.ratingValue) AS rating FROM historybuysellrating AS h JOIN equitiesfundamental AS e " +
                                             "ON h.symbol=e.symbol " +
                                             "WHERE h.ratingdate='" + dbDate + "' AND e.sectorId=" + secId, con);


            try
            {
                con.Open();

                OdbcDataReader dr = com.ExecuteReader();


                while (dr.Read())
                {
                    object rating = dr.GetValue(2);

                    if (!Convert.IsDBNull(rating))
                    {
                        ratingValue = (Double)dr.GetValue(2);
                    }

                }
                dr.Close();
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

            return ratingValue;

        }


        public static void SaveSectorPerfHistDataCSVToDB(string filename, bool isHistorical)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            OdbcCommand deleteHistCommand = new OdbcCommand("DELETE from sectorhistperfmnce", con);
            OdbcCommand deleteCurCommand = new OdbcCommand("DELETE from sectorhistperfmnce where date =" + DateTime.Now.ToString("yyyy-MM-dd"), con);
            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE '" + filename + "' " +
                                                "INTO TABLE sectorhistperfmnce " +
                                                "FIELDS TERMINATED BY ',' " +
                                                "LINES TERMINATED BY '\n' " +
                                                "(sectorid,date,rating,ratingvalue,ratingchangepct);", con);

            try
            {
                con.Open();
                if (isHistorical)
                {
                    deleteHistCommand.ExecuteReader();
                }
                else
                {
                    deleteCurCommand.ExecuteReader();
                }
                insertCommand.ExecuteReader();

                log.Info("Sector Performance File Saved....");

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

        internal static List<DateTime> GetCurrentDate()
        {
            List<DateTime> DateList = new List<DateTime>();
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            try
            {

                OdbcCommand com = new OdbcCommand("SELECT Date,historicalDatesId FROM historicaldates where DateType='" + Constants.C + "'", con);
                //OdbcCommand com = new OdbcCommand("SELECT date,close,volume from  symbolshistorical where symbol = 'SPY' order by date desc", con);
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

        internal static void deleteStockSymbols()
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand deleteCommand = new OdbcCommand("DELETE FROM sectorstocksymbols", con);


            try
            {
                con.Open();
                deleteCommand.ExecuteNonQuery();
                log.Info("Data deleted from sectorstocksymbols Table...");
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

        internal static Dictionary<int, double> getRatingValueChangePct()
        {
            Dictionary<int, double> ratingValueChangePctDict = new Dictionary<int, double>();
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand("SELECT sectorid,ratingchangepct FROM sectorhistperfmnce WHERE DATE =(SELECT MAX(DATE) FROM sectorhistperfmnce)", con);


            try
            {
                con.Open();

                OdbcDataReader dr = com.ExecuteReader();


                while (dr.Read())
                {
                    object ratingValueChangePct = dr.GetValue(1);
                    object sectorId = dr.GetValue(0);

                    if (!Convert.IsDBNull(ratingValueChangePct) && !Convert.IsDBNull(ratingValueChangePct))
                    {
                        if (!ratingValueChangePctDict.ContainsKey(dr.GetInt32(0)))
                            ratingValueChangePctDict.Add(dr.GetInt32(0), dr.GetDouble(1));

                    }

                }
                dr.Close();
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

            return ratingValueChangePctDict;
        }
    }
}
