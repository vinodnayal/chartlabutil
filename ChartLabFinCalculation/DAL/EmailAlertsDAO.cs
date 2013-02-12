using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using FinLib;
using FinLib.Model;
using ChartLabFinCalculation.BL;

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
                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }

            // string mycon = System.Configuration.ConfigurationSettings.AppSettings.["sqlcon"].ConnectionString;
            return users;
        }
        internal static Dictionary<String, SymbolAlerts> getMyPortAlerts(int userId)
        {
            Dictionary<String, SymbolAlerts> symolAlertListDict = new Dictionary<String, SymbolAlerts>();

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand ratingChangeCom = new OdbcCommand("SELECT DISTINCT  up.symbol,bs.oldRating,bs.newRating,ef.companyName,bs.ratingDate FROM users u "
                                                        + "LEFT JOIN userportfolio up ON up.userId=u.userid "
                                                        + "INNER JOIN buysellratingchangehistory bs ON bs.symbol=up.symbol "
                                                        + "LEFT JOIN equitiesfundamental ef ON ef.symbol=up.symbol "
                                                         + "WHERE  u.userid=" + userId + " AND bs.ratingDate=(SELECT MAX(ratingDate) FROM buysellratingchangehistory)", con);

            OdbcCommand ctRatingChangeCom = new OdbcCommand("SELECT DISTINCT up.symbol,ct.ctratingprev,ct.ctratingcurr , ef.companyName, ct.changeDate FROM users u "
                                                            + "LEFT JOIN userportfolio up ON up.userId=u.userid "
                                                            + "INNER JOIN ctratingchangehistory ct ON ct.symbol=up.symbol "
                                                            + "LEFT JOIN equitiesfundamental ef ON ef.symbol=up.symbol "
                                                           + "WHERE u.userid=" + userId + " AND ct.changeDate=(SELECT MAX(changedate) FROM ctratingchangehistory)", con);
            try
            {
                con.Open();
                OdbcDataReader ratingChangeDr = ratingChangeCom.ExecuteReader();
                symolAlertListDict = getSymbolBSRatingAlertObj(ratingChangeDr, symolAlertListDict,false);

                con.Close();

                con.Open();
                OdbcDataReader ctRatingDr = ctRatingChangeCom.ExecuteReader();
                symolAlertListDict = getSymbolCTRatingAlert(ctRatingDr, symolAlertListDict, false);

                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }

            // string mycon = System.Configuration.ConfigurationSettings.AppSettings.["sqlcon"].ConnectionString;
            return symolAlertListDict;
        }
        internal static Dictionary<String, SymbolAlerts> getMyWatchlistAlerts(int userId)
        {
            Dictionary<String, SymbolAlerts> symolAlertListDict = new Dictionary<String, SymbolAlerts>();

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand ratingChangeCom = new OdbcCommand("SELECT DISTINCT wm.symbol,bs.oldRating,bs.newRating,ef.companyName,bs.ratingDate,wm.watchlistid,wl.watchlistname FROM users u "
                                                            + "LEFT JOIN watchlist wl ON wl.userid=u.userId "
                                                        + "LEFT JOIN watchlistsymbolmapping wm ON wm.watchlistid=wl.watchlistid "
                                                        + "INNER JOIN buysellratingchangehistory bs ON bs.symbol=wm.symbol "
                                                        + "LEFT JOIN equitiesfundamental ef ON ef.symbol=wm.symbol "
                                                        + "WHERE  u.userid=" + userId + "  AND bs.ratingDate=(SELECT MAX(ratingDate) FROM buysellratingchangehistory)", con);

            OdbcCommand ctRatingChangeCom = new OdbcCommand("SELECT DISTINCT wm.symbol,ct.ctratingprev,ct.ctratingcurr ,ef.companyName,ct.changeDate,wm.watchlistid,wl.watchlistname FROM users u "
                                                            + "LEFT JOIN watchlist wl ON wl.userid=u.userId "
                                                        + "LEFT JOIN watchlistsymbolmapping wm ON wm.watchlistid=wl.watchlistid "
                                                        + "INNER JOIN ctratingchangehistory ct ON ct.symbol=wm.symbol "
                                                        + "LEFT JOIN equitiesfundamental ef ON ef.symbol=wm.symbol "
                                                        + "WHERE  u.userid=" + userId + "  AND ct.changeDate=(SELECT MAX(changedate) FROM ctratingchangehistory)", con);
            try
            {


                con.Open();
                OdbcDataReader ratingChangeDr = ratingChangeCom.ExecuteReader();
                symolAlertListDict = getSymbolBSRatingAlertObj(ratingChangeDr, symolAlertListDict,true);
                
                con.Close();

                con.Open();
                OdbcDataReader ctRatingDr = ctRatingChangeCom.ExecuteReader();
                symolAlertListDict = getSymbolCTRatingAlert(ctRatingDr, symolAlertListDict, true);
                
                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }

            // string mycon = System.Configuration.ConfigurationSettings.AppSettings.["sqlcon"].ConnectionString;
            return symolAlertListDict;
        }

        private static Dictionary<string, SymbolAlerts> getSymbolCTRatingAlert(OdbcDataReader ctRatingDr, Dictionary<string, SymbolAlerts> symolAlertListDict,bool isWatchlistAlert)
        {
             try
            {
            while (ctRatingDr.Read())
            {
                int ctRatingAlertType = 0; //negative alert or positive
                int prevCTRating = 0;
                if (ctRatingDr.GetValue(1) != DBNull.Value)
                {
                    prevCTRating = ctRatingDr.GetInt32(1);
                }
                int currCTRating = 0;
                if (ctRatingDr.GetValue(2) != DBNull.Value)
                {
                    currCTRating = ctRatingDr.GetInt32(2);
                }
               
                if (currCTRating > prevCTRating)
                {
                    ctRatingAlertType = 1;
                }
                String watchlistName = "Portfolio";
                if (isWatchlistAlert)
                {
                    if (ctRatingDr.GetValue(6) != DBNull.Value)
                    {
                        watchlistName = ctRatingDr.GetString(6);
                    } 
                }
                String symbol = "";

                if (ctRatingDr.GetValue(0) != DBNull.Value)
                {
                    symbol = ctRatingDr.GetString(0);
                }
                String companyName = "";

                if (ctRatingDr.GetValue(3) != DBNull.Value)
                {
                    companyName = ctRatingDr.GetString(3);
                }
                if (!symolAlertListDict.ContainsKey(symbol))
                {
                    symolAlertListDict.Add(symbol, new SymbolAlerts
                    {
                        Symbol = symbol,
                        preCTRating = prevCTRating,
                        curCTRating = currCTRating,
                        companyName = companyName,
                        ctRatingAlertType = ctRatingAlertType,
                        watchlistName = watchlistName
                    });
                }
                else
                {
                    SymbolAlerts symAlert = symolAlertListDict[symbol];
                    symAlert.preCTRating = prevCTRating;
                    symAlert.curCTRating = currCTRating;
                    symAlert.companyName = companyName;
                    symAlert.ctRatingAlertType = ctRatingAlertType;
                }
            }
            ctRatingDr.Close();
            }
             catch (OdbcException ex)
             {
                 throw ex;
             }

             return symolAlertListDict;
        }

        private static Dictionary<string, SymbolAlerts> getSymbolBSRatingAlertObj(OdbcDataReader ratingChangeDr, Dictionary<string, SymbolAlerts> symolAlertListDict,bool isWatchlistAlert)
        {
            try
            {


                while (ratingChangeDr.Read())
                {
                    int ratingAlertType = 0; //negative alert or positive
                    int prevRating = 0;
                    if (ratingChangeDr.GetValue(1) != DBNull.Value)
                    {
                       prevRating = ratingChangeDr.GetInt32(1);
                    }
                    int currRating = 0;
                    if (ratingChangeDr.GetValue(2) != DBNull.Value)
                    {
                        currRating = ratingChangeDr.GetInt32(2);
                    }
                    if (currRating > prevRating)
                    {
                        ratingAlertType = 1;
                    }
                    String watchlistName = "Portfolio";
                    if (isWatchlistAlert)
                    {
                        if (isWatchlistAlert)
                        {
                            if (ratingChangeDr.GetValue(6) != DBNull.Value)
                            {
                                watchlistName = ratingChangeDr.GetString(6);
                            }
                        }
                    }
                    String symbol="";

                    if (ratingChangeDr.GetValue(0) != DBNull.Value)
                    {
                        symbol = ratingChangeDr.GetString(0);
                    }
                    String companyName = "";

                    if (ratingChangeDr.GetValue(3) != DBNull.Value)
                    {
                        companyName = ratingChangeDr.GetString(3);
                    }

                    if (!symolAlertListDict.ContainsKey(symbol))
                    {

                        symolAlertListDict.Add(symbol, new SymbolAlerts
                        {
                            Symbol = symbol,
                            preRating = prevRating,
                            curRating = currRating,
                            companyName = companyName,
                            ratingAlertType = ratingAlertType,
                            watchlistName = watchlistName
                        });
                    }
                    else
                    {
                        SymbolAlerts symAlert = symolAlertListDict[symbol];
                        symAlert.preRating = prevRating;
                        symAlert.curRating = currRating;
                        symAlert.companyName = companyName;
                        symAlert.ratingAlertType = ratingAlertType;
                    }
                }
                ratingChangeDr.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }

            return symolAlertListDict;
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
                dr.Close();
                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }

            return subsId;

        }

        internal static void updateMyAlertsInDB(int userId,int subscriptionId, string alertString)
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
        }

        internal static Dictionary<String, SymbolAlerts> GetSpecificWLAlerts(int watchlistId)
        {
            Dictionary<String, SymbolAlerts> symolAlertListDict = new Dictionary<String, SymbolAlerts>();

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand ratingChangeCom = new OdbcCommand("SELECT DISTINCT wm.symbol,bs.oldRating,bs.newRating,ef.companyName,bs.ratingDate,wm.watchlistid,wl.watchlistname FROM watchlist wl "
                                                           +"LEFT JOIN watchlistsymbolmapping wm ON wm.watchlistid=wl.watchlistid "
                                                           +"INNER JOIN buysellratingchangehistory bs ON bs.symbol=wm.symbol "
                                                           +"LEFT JOIN equitiesfundamental ef ON ef.symbol=wm.symbol "
                                                            + "WHERE  wl.watchlistid=" + watchlistId + " AND bs.ratingDate=(SELECT MAX(ratingDate) FROM buysellratingchangehistory)", con);

            OdbcCommand ctRatingChangeCom = new OdbcCommand("SELECT DISTINCT wm.symbol,ct.ctratingprev,ct.ctratingcurr ,ef.companyName,ct.changeDate,wm.watchlistid,wl.watchlistname FROM watchlist wl "
                                                           + "LEFT JOIN watchlistsymbolmapping wm ON wm.watchlistid=wl.watchlistid "
                                                        + "INNER JOIN ctratingchangehistory ct ON ct.symbol=wm.symbol "
                                                        + "LEFT JOIN equitiesfundamental ef ON ef.symbol=wm.symbol "
                                                        + "WHERE  wl.watchlistid=" + watchlistId + "  AND ct.changeDate=(SELECT MAX(changedate) FROM ctratingchangehistory)", con);
            try
            {


                con.Open();
                OdbcDataReader ratingChangeDr = ratingChangeCom.ExecuteReader();
                symolAlertListDict = getSymbolBSRatingAlertObj(ratingChangeDr, symolAlertListDict,true);

                con.Close();

                con.Open();
                OdbcDataReader ctRatingDr = ctRatingChangeCom.ExecuteReader();
                symolAlertListDict = getSymbolCTRatingAlert(ctRatingDr, symolAlertListDict,true);

                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }

            // string mycon = System.Configuration.ConfigurationSettings.AppSettings.["sqlcon"].ConnectionString;
            return symolAlertListDict;
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
        }

        internal static Dictionary<int, string> GetUniqueSubsUser()
        {
            Dictionary<int, string> users = new Dictionary<int, string>();

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand("SELECT DISTINCT usm.user_id,u.emailAddress FROM usersubscriptionmapping usm "
                                                + "INNER JOIN users u ON u.userId=usm.user_id where u.isPaid=1", con);
            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    users.Add(Convert.ToInt32(dr.GetString(0)), dr.GetString(1));
                }
                dr.Close();
                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
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
                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
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

                            alertText.Append("<b>"+subsType + ":</b><br>");
                        alertText.Append(EmailAlertsCalculation.formateAlertText(text));
                    }
                }
                dr.Close();
                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }

            return alertText.ToString();
        }

        internal static Dictionary<int, int> getCommonSubscriptions()
        {
            Dictionary<int, int> commonSubsList = new Dictionary<int, int>();

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand("SELECT subcrptin_Id,WatchlistId  FROM emailsubscription WHERE iscommonSubs=1", con);
            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    commonSubsList.Add(dr.GetInt32(0), dr.GetInt32(1));
                }
                dr.Close();
                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
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
                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
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

                con.Close();
            }
            catch (OdbcException ex)
            {
                log.Error("ERROR \n" + "============ \n" + ex.ToString());
            }
        }

        internal static Dictionary<int, String>  getUserWatchLists(int userId)
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
                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
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
        }

        internal static Dictionary<string, string> getSubscribedWlAlert(int userId, List<int> watchlistIds)
        {
            

             string watchlistString = string.Join(",", watchlistIds);
           // String.Join(',',watchlistIds);
             Dictionary<string, string> watchlistAlerts = new Dictionary<string, string>();
          
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand("SELECT userid,watchlistname,alerttext FROM watchlist "
                                                +" WHERE  watchlistid in ( "+watchlistString+")", con);
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
                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
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
        }
    }
}
