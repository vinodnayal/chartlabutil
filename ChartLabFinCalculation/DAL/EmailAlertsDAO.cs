using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using FinLib;
using FinLib.Model;
using ChartLabFinCalculation.BL;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Builders;

namespace ChartLabFinCalculation.DAL
{
    public class EmailAlertsDAO
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(EmailAlertsDAO));
        internal static List<int> GetUniqueSubsUser(int subscriptionId)
        {
            List<int> users = new List<int>();

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand("SELECT DISTINCT user_id FROM usersubscriptionmapping where subcrptin_Id=" + subscriptionId, con);

            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    if (dr.GetValue(0) != DBNull.Value)
                    {
                        users.Add(Convert.ToInt32(dr.GetInt32(0)));
                    }
                }
                dr.Close();

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


            // string mycon = System.Configuration.ConfigurationSettings.AppSettings.["sqlcon"].ConnectionString;
            return users;
        }
        internal static Dictionary<int, List<SymbolAlerts>> getMyPortAlerts(int userId)
        {
            Dictionary<int, List<SymbolAlerts>> symolAlertList = new Dictionary<int,List<SymbolAlerts>>();

            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            OdbcCommand ratingChangeCom = new OdbcCommand(@" SELECT DISTINCT  up.symbol,bs.oldRating,bs.newRating,ef.companyName,bs.ratingDate,ct.ctratingprev,ct.ctratingcurr,ct.changedate,sl.last,''AS date1,sl.prev_close,'' AS preDate,sa.r1,sa.s3,sa.longTerm,0 AS watchlistid,'Portfolio' AS watchlistname FROM users u 
LEFT JOIN userportfolio up ON up.userId=u.userid 
LEFT JOIN symbolanalytics sa ON sa.symbol=up.symbol 
LEFT JOIN equitiesfundamental ef ON ef.symbol=up.symbol
LEFT JOIN (SELECT * FROM buysellratingchangehistory WHERE ratingDate= (SELECT MAX(ratingDate) FROM buysellratingchangehistory)) AS bs ON bs.symbol=up.symbol
LEFT JOIN (SELECT * FROM ctratingchangehistory WHERE changedate= (SELECT MAX(changedate) FROM ctratingchangehistory)) AS ct ON ct.symbol=up.symbol
LEFT JOIN symbol_live_data sl ON sl.symbol=up.symbol

WHERE  u.userid=" + userId + " AND (ratingDate IS NOT NULL OR changedate IS NOT NULL)", con);
           try
            {
                con.Open();
                OdbcDataReader ratingChangeDr = ratingChangeCom.ExecuteReader();
              symolAlertList = getSymbolBSRatingAlertObj(ratingChangeDr);

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
       
            return symolAlertList;
        }
        internal static Dictionary<int, List<SymbolAlerts>> getMyWatchlistAlerts(int userId)
        {
            Dictionary<int, List<SymbolAlerts>> symolAlertList = new Dictionary<int, List<SymbolAlerts>>();

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand ratingChangeCom = new OdbcCommand(@"  SELECT DISTINCT  wm.symbol,bs.oldRating,bs.newRating,ef.companyName,bs.ratingDate,ct.ctratingprev,ct.ctratingcurr,ct.changedate,sl.last,''AS date1,sl.prev_close,'' AS preDate,sa.r3,sa.s3,sa.longTerm,wm.watchlistid,wl.watchlistname FROM users u 
INNER JOIN watchlist wl ON wl.userid=u.userId 
INNER JOIN watchlistsymbolmapping wm ON wm.watchlistid=wl.watchlistid 
LEFT JOIN symbolanalytics sa ON sa.symbol=wm.symbol 
LEFT JOIN equitiesfundamental ef ON ef.symbol=wm.symbol
LEFT JOIN paidwatchlistusermapping pwm ON pwm.user_wl_id = wl.watchlistid
LEFT JOIN (SELECT * FROM buysellratingchangehistory WHERE ratingDate= (SELECT MAX(ratingDate) FROM buysellratingchangehistory)) AS bs ON bs.symbol=wm.symbol
LEFT JOIN (SELECT * FROM ctratingchangehistory WHERE changedate= (SELECT MAX(changedate) FROM ctratingchangehistory)) AS ct ON ct.symbol=wm.symbol
LEFT JOIN symbol_live_data sl ON sl.symbol=wm.symbol
WHERE u.userid=" + userId + " AND (ratingDate IS NOT NULL OR changedate IS NOT NULL) AND pwm.user_wl_id IS null", con);

           
            try
            {

                con.Open();
                OdbcDataReader ratingChangeDr = ratingChangeCom.ExecuteReader();
                symolAlertList = getSymbolBSRatingAlertObj(ratingChangeDr);
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

            
            return symolAlertList;
        }

  

        private static Dictionary<int, List<SymbolAlerts>> getSymbolBSRatingAlertObj(OdbcDataReader alertDr)
        {
            Dictionary<int, List<SymbolAlerts>> wlAlertDict = new Dictionary<int, List<SymbolAlerts>>();
            try
            {

               
                while (alertDr.Read())
                {
                    String symbol = "";

                    if (alertDr.GetValue(0) != DBNull.Value)
                    {
                        symbol = alertDr.GetString(0);
                    }
                    int ratingAlertType = 0; //negative alert or positive
                    int prevRating = 0;
                    if (alertDr.GetValue(1) != DBNull.Value)
                    {
                        prevRating = alertDr.GetInt32(1);
                    }
                    int currRating = 0;
                    if (alertDr.GetValue(2) != DBNull.Value)
                    {
                        currRating = alertDr.GetInt32(2);
                    }
                    if (currRating > prevRating)
                    {
                        ratingAlertType = 1;
                    }
                    String companyName = "";

                    if (alertDr.GetValue(3) != DBNull.Value)
                    {
                        companyName = alertDr.GetString(3);
                    }


                    int ctRatingAlertType = 0; //negative alert or positive

                    int prevCTRating = 0;
                    if (alertDr.GetValue(5) != DBNull.Value)
                    {
                        prevCTRating = alertDr.GetInt32(5);
                    }
                    int currCTRating = 0;
                    if (alertDr.GetValue(6) != DBNull.Value)
                    {
                        currCTRating = alertDr.GetInt32(6);
                    }

                    if (currCTRating > prevCTRating)
                    {
                        ctRatingAlertType = 1;
                    }

                    double yesterdayPrice = 0;
                    if (alertDr.GetValue(10) != DBNull.Value)
                    {
                        yesterdayPrice = alertDr.GetFloat(10);
                    }

                    double todayPrice = 0;
                    if (alertDr.GetValue(8) != DBNull.Value)
                    {
                        todayPrice = alertDr.GetFloat(8);
                    }

                    double change = 0;
                    double changePct = 0;
                    if (todayPrice != 0 && yesterdayPrice != 0)
                    {
                        change = todayPrice - yesterdayPrice;
                        changePct = (todayPrice - yesterdayPrice) * 100 / yesterdayPrice;
                    }






                    double r3 = 0.0;
                    if (alertDr.GetValue(12) != DBNull.Value)
                    {
                        r3 = Convert.ToDouble(alertDr.GetValue(12));
                    }

                    double s3 = 0.0;
                    if (alertDr.GetValue(13) != DBNull.Value)
                    {
                        s3 = Convert.ToDouble(alertDr.GetValue(13));
                    }

                    int longTermAlertId = 0;

                    if (alertDr.GetValue(14) != DBNull.Value)
                    {
                        longTermAlertId = alertDr.GetInt32(14);
                    }


                    String ratingAlertText = getRatingAlertText(currRating,prevRating);
                    String ctRatingAlertText = getCtRatingAlertText(currCTRating,prevCTRating);
                    String longTermAlertText = getLongTermText(longTermAlertId);
                    string changeCssClass = "";
                    if (change < 0)
                    {
                        changeCssClass = "redInfo";
                    }
                    else
                    {
                        changeCssClass = "greenInfo";
                    }
                    String priceChangeText = "<div class='"+changeCssClass+"'>"+Math.Round(change,2)+"&nbsp;&nbsp;"+Math.Round(changePct,2)+"%</div>";

                    String watchlistName = "";
                    if (alertDr.GetValue(16) != DBNull.Value)
                    {
                        watchlistName = alertDr.GetString(16);
                    }

                    int watchlistId = 0;
                    if (alertDr.GetValue(15) != DBNull.Value)
                    {
                        watchlistId = Convert.ToInt32(alertDr.GetValue(15));
                    }

                    String wlHeaderCss = "";
                    if (watchlistId < 20 && watchlistId > 0)
                    {
                        wlHeaderCss = "commonWlHeader";
                    }
                    else if (watchlistId ==0)
                    {
                        wlHeaderCss = "portHeader";
                    }else
                    {
                        wlHeaderCss = "wlHeader";
                    }



                    //  wm.symbol,bs.oldRating,bs.newRating,ef.companyName,bs.ratingDate,ct.ctratingprev,ct.ctratingcurr,
                    //ct.changedate,t1.close,t1.date,t2.close,t2.date,sa.r3,sa.s3,sa.longTerm,wm.watchlistid,wl.watchlistname 
               SymbolAlerts symbolAlert= new SymbolAlerts
                            {
                                Symbol = symbol,
                                preRating = prevRating,
                                curRating = currRating,
                                companyName = companyName,
                                ratingAlertType = ratingAlertType,
                                ctRatingAlertType = ctRatingAlertType,
                                watchlistName = watchlistName,
                                preCTRating = prevCTRating,
                                curCTRating = currCTRating,
                                change = change,
                                changePct = changePct,
                                resistance = Math.Round(r3,2),
                                support = Math.Round(s3, 2),
                                longTermTrendText = longTermAlertText,
                                price = todayPrice,
                                ratingAlertText = ratingAlertText,
                                ctRatingAlertText = ctRatingAlertText,
                                priceChangeText = priceChangeText,
                                wlHeaderCss = wlHeaderCss

                            };
                    if (wlAlertDict.ContainsKey(watchlistId))
                    {
                        List<SymbolAlerts> symbolAlerts = wlAlertDict[watchlistId];
                       symbolAlerts.Add(symbolAlert);
                        
                        wlAlertDict[watchlistId] = symbolAlerts;
                    }
                    else
                    {
                        List<SymbolAlerts> symbolAlerts = new List<SymbolAlerts>();
                        symbolAlerts.Add(symbolAlert);
                        wlAlertDict.Add(watchlistId, symbolAlerts);
                    }
                }
                alertDr.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }

            return wlAlertDict;
        }

        private static string getCtRatingAlertText(int currCTRating, int prevCTRating)
        {
            string text = string.Empty;
            try
            {
                if (currCTRating > prevCTRating)
                {
                    text = "<div style='Color:green'><img align='middle' src='http://www.chartlabpro.com/images/checkGreen.png' /> " + Enum.GetName(typeof(CTRatingEnum), currCTRating) + "  from " + Enum.GetName(typeof(CTRatingEnum), prevCTRating) + "</div>";
                }
                else if (currCTRating < prevCTRating)
                {
                    text = "<div style='Color:maroon'><img align='middle' src='http://www.chartlabpro.com/images/crossRed.png' />"+ Enum.GetName(typeof(CTRatingEnum), currCTRating) + "  from " + Enum.GetName(typeof(CTRatingEnum), prevCTRating) + "</div>";
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return text;
        }

        private static string getRatingAlertText(int currRating, int prevRating)
        {
            string text = string.Empty;
            try
            {
                if (currRating > prevRating)
                {
                    text = "<div style='Color:green'><img align='middle' src='http://www.chartlabpro.com/images/checkGreen.png' /> Upgraded "+currRating+"  from "+prevRating+"</div>";
                }
                else if(currRating < prevRating)
                {
                    text = "<div style='Color:maroon'><img align='middle' src='http://www.chartlabpro.com/images/crossRed.png' /> Downgraded " + currRating + "  from " + prevRating + "</div>";
                }
                
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return text;
        }

        private static string getLongTermText(int longTermAlertId)
        {
            string text = string.Empty;
            try
            {
                if (longTermAlertId == 3)
                {
                    text = "<div style='Color:green'>Bullish <img align='middle' src='http://www.chartlabpro.com/images/bull.png' /> </div>";
                }
                else if (longTermAlertId == 2)
                {
                    text = "<div style='Color:maroon' >Bearish <img align='middle' src='http://www.chartlabpro.com/images/Bear.png' /></div>";
                } 
                else if (longTermAlertId == 4)
                {
                    text = "<div style='Color:green'>Very Bullish <img align='middle' src='http://www.chartlabpro.com/images/bull.png' /> </div>";
                }
                else if (longTermAlertId == 1)
                {
                    text = "<div style='Color:maroon' >Very Bearish <img align='middle' src='http://www.chartlabpro.com/images/Bear.png' /></div>";
                }
                else
                {
                    text = "<div style='Color:gray' >Neutral </div>";
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return text;
        }

        internal static List<int> getUserSubscriptions(int userId)
        {
            List<int> subsId = new List<int>();
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand("SELECT user_id, subcrptin_Id,substype FROM usersubscriptionmapping WHERE user_id=" + userId, con);
            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    subsId.Add(dr.GetInt32(1));
                }


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

            return subsId;

        }

        internal static void updateMyAlertsInDB(int userId, int subscriptionId, string alertString)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand updateCommand = new OdbcCommand("UPDATE alerts SET portalertstext='" + alertString + "' WHERE userid=" + userId, con);

            try
            {
                con.Open();

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

        internal static Dictionary<int, List<SymbolAlerts>> GetSpecificWLAlerts(int watchlistId)
        {
            Dictionary<int, List<SymbolAlerts>> symolAlertList = new Dictionary<int, List<SymbolAlerts>>();

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand ratingChangeCom = new OdbcCommand(@" SELECT DISTINCT wm.symbol,bs.oldRating,bs.newRating,ef.companyName,bs.ratingDate,ct.ctratingprev,ct.ctratingcurr,ct.changedate,sl.last,''AS date1,sl.prev_close,'' AS preDate,sa.r3,sa.s3,sa.longTerm,wm.watchlistid,wl.watchlistname FROM watchlist wl 
LEFT JOIN watchlistsymbolmapping wm ON wm.watchlistid=wl.watchlistid 
LEFT JOIN symbolanalytics sa ON sa.symbol=wm.symbol 
LEFT JOIN equitiesfundamental ef ON ef.symbol=wm.symbol
LEFT JOIN (SELECT * FROM buysellratingchangehistory WHERE ratingDate= (SELECT MAX(ratingDate) FROM buysellratingchangehistory)) AS bs ON bs.symbol=wm.symbol
LEFT JOIN (SELECT * FROM ctratingchangehistory WHERE changedate= (SELECT MAX(changedate) FROM ctratingchangehistory)) AS ct ON ct.symbol=wm.symbol
LEFT JOIN symbol_live_data sl ON sl.symbol=wm.symbol
WHERE  wl.watchlistid=" + watchlistId + " AND (ratingDate IS NOT NULL OR changedate IS NOT NULL)", con);

           try
            {


                con.Open();
                OdbcDataReader ratingChangeDr = ratingChangeCom.ExecuteReader();
                symolAlertList = getSymbolBSRatingAlertObj(ratingChangeDr);
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

            return symolAlertList;
        }

        internal static void updateCommanAlertInDB(int subsId, string alertString)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand updateCommand = new OdbcCommand("UPDATE commonsubcrptinstext SET alertText='" + alertString + "' WHERE subcrptin_Id=" + subsId + ";", con);

            try
            {
                con.Open();

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

        internal static Dictionary<int, string> GetUniqueSubsUser()
        {
            Dictionary<int, string> users = new Dictionary<int, string>();

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand("SELECT DISTINCT usm.user_id,u.emailAddress FROM usersubscriptionmapping usm "
                                                + "INNER JOIN users u ON u.userId=usm.user_id where u.isPaid=1 OR u.isundertrial=1", con);
            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    users.Add(Convert.ToInt32(dr.GetString(0)), dr.GetString(1));
                }
                dr.Close();

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

            return users;
        }

        internal static UserAlerts getPortWlAlertText(int userId)
        {

            UserAlerts userAlert = new UserAlerts();
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand("SELECT userid,portalertstext FROM alerts WHERE userid=" + userId, con);
            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    userAlert.userId = dr.GetInt32(0);
                    userAlert.portfolioAlerts = dr.GetString(1);

                }
                dr.Close();

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

            return userAlert;

        }

        internal static string getCommonAlertText(int userId)
        {
            StringBuilder alertText = new StringBuilder(); ;
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand("SELECT DISTINCT usm.user_id,ct.alertText,es.Subcrptin_name FROM usersubscriptionmapping usm "
                                                + "LEFT JOIN commonsubcrptinstext ct ON ct.subcrptin_Id= usm.subcrptin_Id "
                                                + "LEFT JOIN emailsubscription es ON es.subcrptin_Id= usm.subcrptin_Id "
                                                + "WHERE  ct.alertText IS NOT null AND usm.user_id=" + userId, con);
            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    if (dr.GetString(1) != null)
                    {
                        String text = dr.GetString(1);
                        String subsType = dr.GetString(2);
                        if (text != "" && text != null)

                            alertText.Append("<b>" + subsType + ":</b><br>");
                       // alertText.Append(EmailAlertsCalculation.formateAlertText(text));
                    }
                }
                dr.Close();

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

            return alertText.ToString();
        }

        internal static Dictionary<int, int> getCommonSubscriptions()
        {
            Dictionary<int, int> commonSubsList = new Dictionary<int, int>();

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand("SELECT watchlistid,watchlistid  FROM watchlist WHERE (ispaid=0 OR ispaid IS NULL) AND userid=1", con);
            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    if (dr.GetValue(0) != DBNull.Value && dr.GetValue(1) != DBNull.Value)
                    {
                        commonSubsList.Add(dr.GetInt32(0), dr.GetInt32(1));
                    }
                }
                dr.Close();

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

            return commonSubsList;
        }

        internal static List<int> getAllUsers()
        {
            List<int> users = new List<int>();

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand("SELECT DISTINCT userid FROM users", con);

            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    if (dr.GetValue(0) != DBNull.Value)
                    {
                        users.Add(Convert.ToInt32(dr.GetInt32(0)));
                    }
                }
                dr.Close();

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

            return users;
        }

        internal static void InsertUserAlertsCSVToDB(string AlertsPath)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            OdbcCommand deleteCommand = new OdbcCommand("DELETE from alerts", con);

            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE" + " '" + AlertsPath + "/UserAlerts.csv' " +
                                              "INTO TABLE alerts " +
                                              "FIELDS TERMINATED BY ',' " +
                                              "LINES TERMINATED BY '\n' " +
                                              "(userid,portalertstext);", con);


            try
            {
                con.Open();
                deleteCommand.ExecuteReader();
                insertCommand.ExecuteReader();
                log.Info("Alerts Data File Saved....");

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

        internal static Dictionary<int, String> getUserWatchLists(int userId)
        {
            Dictionary<int, String> userWatchlists = new Dictionary<int, string>();
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand("SELECT DISTINCT watchlistid, watchlistname FROM watchlist WHERE userid=" + userId, con);
            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    userWatchlists.Add((dr.GetInt32(0)), dr.GetString(1));
                }
                dr.Close();

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

            return userWatchlists;
        }

        internal static void updateWatchlistAlertInDB(string foldername)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand deleteCommand = new OdbcCommand("DELETE from temp_watchlistalerts", con);
            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE" + " '" + foldername + "/WatchlistAlerts.csv' " +
                                               "INTO TABLE temp_watchlistalerts " +
                                               "FIELDS TERMINATED BY ',' " +
                                               "LINES TERMINATED BY '\n' " +
                                               "(userid,watchlistid,watchlistalerttext);", con);

            OdbcCommand updateCommand = new OdbcCommand("UPDATE watchlist ,temp_watchlistalerts" +
                                                  " SET watchlist.alerttext = temp_watchlistalerts.watchlistalerttext "
                                                  + " WHERE watchlist.watchlistid=temp_watchlistalerts.watchlistid ;", con);
            try
            {
                con.Open();

                deleteCommand.ExecuteNonQuery();
                insertCommand.ExecuteReader();
                updateCommand.ExecuteReader();
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

        internal static Dictionary<string, string> getSubscribedWlAlert(int userId, List<int> watchlistIds)
        {


            string watchlistString = string.Join(",", watchlistIds);
            // String.Join(',',watchlistIds);
            Dictionary<string, string> watchlistAlerts = new Dictionary<string, string>();

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand("SELECT userid,watchlistname,alerttext FROM watchlist "
                                                + " WHERE  watchlistid in ( " + watchlistString + ")", con);
            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    if (dr.GetValue(2) != DBNull.Value)
                        watchlistAlerts.Add(dr.GetString(1), dr.GetString(2));

                }
                dr.Close();

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

            return watchlistAlerts;
        }
        internal static void updateWatchlistAlertInDB(int userId, int watchlistId, string alertString)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);


            OdbcCommand updateCommand = new OdbcCommand("UPDATE watchlist " +
                                                  " SET watchlist.alerttext = '" + alertString
                                                  + "' WHERE watchlist.watchlistid=" + watchlistId, con);
            try
            {
                con.Open();

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

        internal static Dictionary<int, string> getModelPortfolioIdsFromDB()
        {
            throw new NotImplementedException();
        }

        internal static Dictionary<int, string> GetModelPortUniqueUser(int watchListId)
        {
            throw new NotImplementedException();
        }

        internal static List<String> GetProEdgeSubsUser()
        {
            List<String> users = new List<String>();

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand(@"SELECT DISTINCT emailAddress FROM paidwatchlistusermapping p
            JOIN users AS u ON u.userId=p.userid WHERE p.watchlistid =11 AND p.isuserpaid=1", con);

            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    if (dr.GetValue(0) != DBNull.Value && dr.GetString(0) != "")
                    {
                        users.Add(dr.GetString(0));
                    }
                }
                dr.Close();

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

            return users;
        }

        /// <summary>
        ///  Get snp alert from mongo
        /// </summary>
        /// <returns SnpAlert>snpAlert</returns>
        internal static SnpAlert getSnpUpdateAlertMongo()
        {

            MongoServer mongo = MongoServer.Create(Constants.MongoConString);
            SnpAlert snpAlert = new SnpAlert();
            try
            {
                mongo.Connect();
                var db = mongo.GetDatabase("chartlab");
                var collection = db.GetCollection<BsonDocument>("ratingsynopsis");
                var query = Query.EQ("symbol", "SPY");



                log.Info("\n\n\n\n\n Getting Data from Mongo DB ");

                foreach (BsonDocument item in collection.Find(query))
                {

                    snpAlert.status = item.GetElement("Status").Value.ToString();
                    if (item.GetElement("CTRatingValue").Value.ToString() != null)
                        snpAlert.ctRatingValue = double.Parse(item.GetElement("CTRatingValue").Value.ToString());

                    snpAlert.synopsis = item.GetElement("synopsis").Value.ToString();
                    snpAlert.ctRatingStatus = item.GetElement("CTRatingStatus").Value.ToString();


                }
            }
            catch (Exception)
            {

                throw;
            }

            return snpAlert;



        }


        internal static Dictionary<int, List<SymbolAlerts>> getPaidWatchlistAlerts(int userId, int paidWatchlistId)
        {
            Dictionary<int, List<SymbolAlerts>> symolAlertList = new Dictionary<int, List<SymbolAlerts>>();

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand ratingChangeCom = new OdbcCommand(@"SELECT mt.symbol,ef.companyName,sl.last,''AS date1,sl.prev_close,'' AS preDate,sa.r3,sa.s3,
CASE WHEN ACTION='Sell'  THEN  'Removing' ELSE 'Adding' END AS alert,
pwm.user_wl_id, watchlistname
FROM paidwatchlistusermapping pwm 
LEFT JOIN model_trades_data mt ON mt.watchlistId = pwm.user_wl_id 
LEFT JOIN watchlist wl ON wl.watchlistId = pwm.user_wl_id
LEFT JOIN symbolanalytics sa ON sa.symbol=mt.symbol 
LEFT JOIN equitiesfundamental ef ON ef.symbol=mt.symbol
LEFT JOIN symbol_live_data sl ON sl.symbol=mt.symbol
WHERE  mt.DATE >=DATE_ADD(CURDATE(),INTERVAL -1 DAY) AND pwm.userid=" + userId + " AND  pwm.watchlistId= " + paidWatchlistId, con);


            try
            {

                con.Open();
                OdbcDataReader ratingChangeDr = ratingChangeCom.ExecuteReader();
                symolAlertList = getPaidWlBuySellAlerts(ratingChangeDr);
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


            return symolAlertList;

        }

        private static Dictionary<int, List<SymbolAlerts>> getPaidWlBuySellAlerts(OdbcDataReader alertDr)
        {
            Dictionary<int, List<SymbolAlerts>> wlAlertDict = new Dictionary<int, List<SymbolAlerts>>();
            try
            {
                while (alertDr.Read())
                {
                    String symbol = "";

                    if (alertDr.GetValue(0) != DBNull.Value)
                    {
                        symbol = alertDr.GetString(0);
                    }
                    
                    String companyName = "";

                    if (alertDr.GetValue(1) != DBNull.Value)
                    {
                        companyName = alertDr.GetString(1);
                    }

                    double yesterdayPrice = 0;
                    if (alertDr.GetValue(4) != DBNull.Value)
                    {
                        yesterdayPrice = alertDr.GetFloat(4);
                    }

                    double todayPrice = 0;
                    if (alertDr.GetValue(2) != DBNull.Value)
                    {
                        todayPrice = alertDr.GetFloat(2);
                    }

                    double change = 0;
                    double changePct = 0;
                    if (todayPrice != 0 && yesterdayPrice != 0)
                    {
                        change = todayPrice - yesterdayPrice;
                        changePct = (todayPrice - yesterdayPrice) * 100 / yesterdayPrice;
                    }

                    double r3 = 0.0;
                    if (alertDr.GetValue(6) != DBNull.Value)
                    {
                        r3 = Convert.ToDouble(alertDr.GetValue(6));
                    }

                    double s3 = 0.0;
                    if (alertDr.GetValue(7) != DBNull.Value)
                    {
                        s3 = Convert.ToDouble(alertDr.GetValue(7));
                    }

                    String buySellAlertText = "";

                    if (alertDr.GetValue(8) != DBNull.Value)
                    {
                        if (alertDr.GetString(8).Equals("Adding"))
                        {
                            buySellAlertText = "<div style='text-align:center; font-weight:bold; color:green'>" + alertDr.GetString(8) + "</div>";
                        }
                        else
                        {
                            buySellAlertText = "<div style='text-align:center; font-weight:bold;color:red'>" + alertDr.GetString(8) + "</div>";
                        }
                    }

                    String longTermAlertText = "";
                    string changeCssClass = "";
                    if (change < 0)
                    {
                        changeCssClass = "redInfo";
                    }
                    else
                    {
                        changeCssClass = "greenInfo";
                    }
                    String priceChangeText = "<div class='" + changeCssClass + "'>" + Math.Round(change, 2) + "&nbsp;&nbsp;" + Math.Round(changePct, 2) + "%</div>";

                    String watchlistName = "";
                    if (alertDr.GetValue(10) != DBNull.Value)
                    {
                        watchlistName = alertDr.GetString(10);
                    }

                    int watchlistId = 0;
                    if (alertDr.GetValue(9) != DBNull.Value)
                    {
                        watchlistId = Convert.ToInt32(alertDr.GetValue(9));
                    }

                    String wlHeaderCss = "";
                    if (watchlistId < 20 && watchlistId > 0)
                    {
                        wlHeaderCss = "commonWlHeader";
                    }
                    else if (watchlistId == 0)
                    {
                        wlHeaderCss = "portHeader";
                    }
                    else
                    {
                        wlHeaderCss = "wlHeader";
                    }
                    SymbolAlerts symbolAlert = new SymbolAlerts
                    {
                        Symbol = symbol,
                      
                        companyName = companyName,
                      
                        watchlistName = watchlistName,
                      
                        change = change,
                        changePct = changePct,
                        resistance = Math.Round(r3, 2),
                        support = Math.Round(s3, 2),
                        longTermTrendText = longTermAlertText,
                        price = todayPrice,
                        ratingAlertText = buySellAlertText, //alert for buy sell (adding / removing)
                        priceChangeText = priceChangeText,
                        wlHeaderCss = wlHeaderCss

                    };
                    if (wlAlertDict.ContainsKey(watchlistId))
                    {
                        List<SymbolAlerts> symbolAlerts = wlAlertDict[watchlistId];
                        symbolAlerts.Add(symbolAlert);

                        wlAlertDict[watchlistId] = symbolAlerts;
                    }
                    else
                    {
                        List<SymbolAlerts> symbolAlerts = new List<SymbolAlerts>();
                        symbolAlerts.Add(symbolAlert);
                        wlAlertDict.Add(watchlistId, symbolAlerts);
                    }
                }
                alertDr.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }

            return wlAlertDict;
        }
    }
}
