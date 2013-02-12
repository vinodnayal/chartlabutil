using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinLib;
using System.Data.Odbc;

namespace ChartLabFinCalculation
{
    class RulesCalculationDAO
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(RulesCalculationDAO));
        internal static Dictionary<string, SymbolRatingAlert> GetBuySellPlusCTRating()
        {

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand("SELECT symbol, buySellRating, ctrating,previuosBSRating,changedateprice,preSBRatingDate from symbolanalytics", con);

            Dictionary<string, SymbolRatingAlert> RatingDict = new Dictionary<string, SymbolRatingAlert>();
            

            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                  
                    object rating = dr.GetValue(1);
                    object ctrating = dr.GetValue(2);
                    object prevrating = dr.GetValue(3);
                    object price = dr.GetValue(4);
                    DateTime changedate = DateTime.MinValue;
                    double changeprice = 0;
                    int prevbsrating;

                    if (!Convert.IsDBNull(rating) & !Convert.IsDBNull(ctrating) &  !Convert.IsDBNull(price))
                    {
                        try
                        {
                            SymbolRatingAlert ratingsymbol = new SymbolRatingAlert();
                            string symbol = dr.GetString(0);
                            int ratingValue = (int)dr.GetValue(1);
                            int ctratingValue = (int)dr.GetValue(2);
                            if (!Convert.IsDBNull(prevrating))
                            {
                                 prevbsrating = (int)dr.GetValue(3);
                            }
                            else
                            {
                                prevbsrating = ratingValue; 
                            }
                            DateTime.TryParse(dr.GetString(5), out changedate);
                            changeprice=dr.GetFloat(4);
                           

                            ratingsymbol.currating = ratingValue;
                            ratingsymbol.ctrating = ctratingValue;
                            ratingsymbol.prevrating = prevbsrating;
                            ratingsymbol.ratingChangeDate = changedate;
                            ratingsymbol.changeDatePrice = changeprice;

                            RatingDict.Add(symbol, ratingsymbol);
                        }
                        catch(Exception e)
                        {
                            throw  e;
                        }
                    }
                }
                dr.Close();

                con.Close();
            }
            catch (OdbcException ex)
            {
                log.Error(ex);
            }

            return RatingDict;
        }

        internal static Dictionary<string, SymbolRatingAlert> GetLongShortAlerts(int type)
        {
          
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand("SELECT	a.symbol,a.prevrating,a.currating,a.changedate,h.close FROM lngshrtsymbols AS a LEFT JOIN "+
                                            "symbolshistorical AS h ON a.symbol=h.symbol "+
                                            "WHERE a.lngshrtid="+type+" AND h.date=a.changedate", con);

            Dictionary<string, SymbolRatingAlert> symbolLongShortAlertDict = new Dictionary<string, SymbolRatingAlert>();


            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    object prev = dr.GetValue(1);
                    object cur = dr.GetValue(2);
                    object date = dr.GetValue(3);
                    object price = dr.GetValue(4);

                    if (!Convert.IsDBNull(prev) & !Convert.IsDBNull(cur) & !Convert.IsDBNull(date))
                    {
                        SymbolRatingAlert symbolAlert = new SymbolRatingAlert();
                        string symbol = dr.GetString(0);
                        int prevrating = (int)dr.GetValue(1);
                        int currating = (int)dr.GetValue(2);
                        DateTime prevdate = (DateTime)dr.GetValue(3);
                        double prevprice = dr.GetFloat(4);
                        symbolAlert.prevrating = prevrating;
                        symbolAlert.currating = currating;
                        symbolAlert.ratingChangeDate = prevdate;
                        symbolAlert.changeDatePrice = prevprice;

                        symbolLongShortAlertDict.Add(symbol, symbolAlert);
                    }
                }
                dr.Close();

                con.Close();
            }
            catch (OdbcException ex)
            {
                log.Error(ex);
            }

            return symbolLongShortAlertDict;
        }

        internal static Dictionary<string, SymbolRatingAlert> GetIntermediateLongShortAlerts(int type)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand("SELECT	a.symbol,a.prevrating,a.currating,a.changedate,h.close FROM intermediatelngshrtsymbols AS a LEFT JOIN "+
                                            "symbolshistorical AS h ON a.symbol=h.symbol "+
                                            "WHERE a.intermediatelngshrtid="+type+" AND h.date=a.changedate", con);


            Dictionary<string, SymbolRatingAlert> symbolLongShortAlertDict = new Dictionary<string, SymbolRatingAlert>();
            

            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    object prev = dr.GetValue(1);
                    object cur = dr.GetValue(2);
                    object date = dr.GetValue(3);
                    object price = dr.GetValue(4);

                    if (!Convert.IsDBNull(prev) & !Convert.IsDBNull(cur) & !Convert.IsDBNull(date))
                    {
                        SymbolRatingAlert symbolAlert = new SymbolRatingAlert();
                        string symbol = dr.GetString(0);
                        int prevrating = (int)dr.GetValue(1);
                        int currating = (int)dr.GetValue(2);
                        DateTime prevdate = (DateTime)dr.GetValue(3);
                        double prevprice = dr.GetFloat(4);
                        symbolAlert.prevrating = prevrating;
                        symbolAlert.currating = currating;
                        symbolAlert.ratingChangeDate = prevdate;
                        symbolAlert.changeDatePrice = prevprice;

                        symbolLongShortAlertDict.Add(symbol, symbolAlert);
                    }
                }
                dr.Close();

                con.Close();
            }
            catch (OdbcException ex)
            {
                log.Error(ex);
            }

            return symbolLongShortAlertDict;
        }

        internal static void InsertRules(string filename)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            OdbcCommand deleteCommand = new OdbcCommand("DELETE from symbolrules", con);

            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE" + " '" + filename + "/SymbolRulesFile.csv' " +
                                              "INTO TABLE symbolrules " +
                                              "FIELDS TERMINATED BY ',' " +
                                              "LINES TERMINATED BY '\n' " +
                                              "(symbol,ruleid,ratingchangedate,changedateprice,currating,prevrating);", con);

            
            try
            {
                con.Open();
                deleteCommand.ExecuteNonQuery();
                insertCommand.ExecuteNonQuery();
                log.Info("Symbol Rules File Saved....");

                con.Close();
            }
            catch (OdbcException ex)
            {
                log.Error("ERROR \n" + "============ \n" + ex.ToString());
            }
        }

        internal static Dictionary<DateTime, SymbolRatingAlert> GetCTRatingHistory(string symbol)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand("SELECT ratingdate,ctrating,h.close FROM historybuysellrating AS r JOIN symbolshistorical AS h "+
                                            "ON r.symbol=h.symbol "+
                                            "WHERE r.symbol='"+symbol+"' AND h.date=r.ratingdate "+
                                            "ORDER BY r.ratingdate desc", con);


            Dictionary<DateTime, SymbolRatingAlert> ctRatingHistoryDict = new Dictionary<DateTime, SymbolRatingAlert>();


            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    object ctratingdate = dr.GetValue(0);
                    object ctrating = dr.GetValue(1);
                    object price = dr.GetValue(2);

                    if (!Convert.IsDBNull(ctratingdate) & !Convert.IsDBNull(ctrating) & !Convert.IsDBNull(price))
                    {
                        SymbolRatingAlert symbolrating = new SymbolRatingAlert();
                        symbolrating.ctrating=(int)dr.GetValue(1);
                        symbolrating.changeDatePrice=dr.GetFloat(2);

                        ctRatingHistoryDict.Add((DateTime)dr.GetValue(0), symbolrating);
                    }
                }
                dr.Close();
                con.Close();
            }
            catch (OdbcException ex)
            {
                log.Error(ex);
            }

            return ctRatingHistoryDict;
        }
    }
}
