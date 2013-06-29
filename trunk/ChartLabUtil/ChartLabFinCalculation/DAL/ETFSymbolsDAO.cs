using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using FinLib;

namespace ChartLabFinCalculation.DAL
{
    public class ETFSymbolsDAO
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(ETFSymbolsDAO));
        internal static List<string> getETFSymbolsList()
        {
            List<string> symbolList = new List<string>();

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com;
            String sqlString = @"SELECT symbol FROM etfsymbols";
            com = new OdbcCommand(sqlString, con);

            try
            {
                con.Open();

                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    object symbol = dr.GetValue(0);


                    if (!Convert.IsDBNull(symbol))
                    {
                        symbolList.Add(dr.GetString(0));

                    }

                }

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

            return symbolList;
        }

        internal static void InsertRatingDataInDB(string DataFilesPath,string tableName,String symbol, bool isHistorical)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            OdbcCommand deleteCommand = new OdbcCommand("DELETE from "+tableName+" where symbol= '" + symbol + "'", con);
            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE" + " '" + DataFilesPath + "/RatingFile.csv' " +
                                             "INTO TABLE "+tableName+
                                             " FIELDS TERMINATED BY ',' " +
                                             "LINES TERMINATED BY '\n' " +
                                             "(symbol,rating,ratingvalue,ctrating,ctratingvalue,ratingdate);", con);

            try
            {
                con.Open();

                deleteCommand.ExecuteNonQuery();
                insertCommand.ExecuteNonQuery();
                log.Info(" hist BuySellRating and ct ratings saved in table " + tableName + " for ...." + symbol);

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

        internal static List<CTRatingHistory> getETFCTRatingHistroyFromDB()
        {
            List<CTRatingHistory> ctRatingHist = new List<CTRatingHistory>();

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand historyBuySellRating = new OdbcCommand("SELECT symbol,ratingdate,ctrating FROM etfhistbsctrating ORDER BY symbol,ratingdate", con);


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
        internal static List<BuySellRating> getETFBuySellRatingHistroyFromDB()
        {
            List<BuySellRating> buySellRatingHist = new List<BuySellRating>();
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand historyBuySellRating = new OdbcCommand("SELECT symbol,rating,ratingdate FROM etfhistbsctrating ORDER BY symbol,ratingdate", con);

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
            return buySellRatingHist;
        }

    }
}
