using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChartLabFinCalculation.DAL;
using FinLib.Model;
using System.Net.Mail;
using FinLib;
using ChartLabFinCalculation.UTIL;
using System.Configuration;
using System.Threading;

namespace ChartLabFinCalculation.BL
{
    public class EmailAlertsCalculation
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(EmailAlertsCalculation));
        static string AlertsPath = ConfigurationManager.AppSettings["AlertsPath"];
        internal static void CalculateMyAlerts()
        {
            List<UserAlerts> portAlerts = new List<UserAlerts>();
           List<UserAlerts> watchlistAlerts = new List<UserAlerts>();
            try
            {

                List<int> UserList = EmailAlertsDAO.getAllUsers();
                foreach (int userId in UserList)
                {
                    watchlistAlerts = calculateWatchlistAlerts(userId, watchlistAlerts); 
                    String portfolioAlerts = calculatePortAlerts(userId);

                    portAlerts.Add(new UserAlerts
                    {
                        userId = userId,
                        portfolioAlerts = portfolioAlerts,

                    });
                }
                CSVExporter.WriteToCSVMyAlertsData(portAlerts, AlertsPath + "/UserAlerts.csv");
                CSVExporter.WriteToCSVWatchlistAlertData(watchlistAlerts, AlertsPath + "/WatchlistAlerts.csv");
                EmailAlertsDAO.InsertUserAlertsCSVToDB(AlertsPath);
                EmailAlertsDAO.updateWatchlistAlertInDB(AlertsPath);




            }
            catch (Exception ex)
            {
                log.Error("Error in calculating my alerts ");
                log.Error(ex);
            }
        }
        private static List<UserAlerts> calculateWatchlistAlerts(int userId, List<UserAlerts> watchlistAlerts)
        {

            try
            {
                Dictionary<int, String> userWatchLists = EmailAlertsDAO.getUserWatchLists(userId);
                foreach (KeyValuePair<int, String> watchlist in userWatchLists)
                {
                    String alertString = "";
                    int watchlistId = watchlist.Key;

                    Dictionary<String, SymbolAlerts> symbolAlertsList = EmailAlertsDAO.GetSpecificWLAlerts(watchlistId);
                    alertString = getAlertString(symbolAlertsList);
                    watchlistAlerts.Add(new UserAlerts { userId = userId, watchlistId = watchlistId, watchlistAlerts = alertString });
                  //  EmailAlertsDAO.updateWatchlistAlertInDB(userId, watchlistId, alertString);

                }

            }
            catch (Exception ex)
            {
                log.Error("Error in calculaing watchlist alerts ");
                log.Error(ex);
            }
            return watchlistAlerts;
        }
        public static void calculateCommonSubsAlerts()
        {

            try
            {
                Dictionary<int, int> commonSubs = EmailAlertsDAO.getCommonSubscriptions();
                foreach (KeyValuePair<int, int> subscription in commonSubs)
                {
                    int subsId = subscription.Key;
                    int watchlistId = subscription.Value;
                    Dictionary<String, SymbolAlerts> symbolAlertsList = EmailAlertsDAO.GetSpecificWLAlerts(watchlistId);
                    String alertString = getAlertString(symbolAlertsList);
                    EmailAlertsDAO.updateCommanAlertInDB(subsId, alertString);

                }

            }
            catch (Exception ex)
            {
                log.Error("Error in calculaing Div portfolio alerts ");
                log.Error(ex);
            }
        }
        private static String calculatePortAlerts(int userId)
        {
            String alertString = "";
            try
            {

                Dictionary<String, SymbolAlerts> symbolAlertsList = EmailAlertsDAO.getMyPortAlerts(userId);
                alertString = getAlertString(symbolAlertsList);

            }
            catch (Exception ex)
            {
                log.Error("Error in calculaing portfolio alerts ");
                log.Error(ex);
            }
            return alertString;
        }
        private static string getAlertString(Dictionary<String, SymbolAlerts> symbolAlertsDict)
        {
            StringBuilder AlertString = new StringBuilder();
            try
            {
                foreach (KeyValuePair<String, SymbolAlerts> symbolAlert in symbolAlertsDict)
                {
                   
                    SymbolAlerts alert = symbolAlert.Value;
                    alert.companyName = alert.companyName.Replace(","," " );
                    alert.watchlistName = alert.watchlistName.Replace(",", " ");
                    if (alert.preRating != 0)
                    {
                        if (alert.ratingAlertType == 1)
                        {
                            AlertString.Append(alert.companyName + " has been upgraded to a " + alert.curRating + " from a " + alert.preRating + ".:1" + ":" + alert.watchlistName + ":" + alert.Symbol);
                        }
                        else
                        {
                            AlertString.Append(alert.companyName + " has been down graded to a " + alert.curRating + " from a " + alert.preRating + ".:0" + ":" + alert.watchlistName + ":" + alert.Symbol);
                        }
                        AlertString.Append(";");
                    }
                    if (alert.curCTRating != 0)
                    {
                        int ctAlertType = 0;
                        if (alert.curCTRating > alert.preCTRating)
                        {
                            ctAlertType = 1;
                        }

                        AlertString.Append(alert.companyName + " has been changed Counter trend to  " + Enum.GetName(typeof(CTRatingEnum), alert.curCTRating) + " from  " + Enum.GetName(typeof(CTRatingEnum), alert.preCTRating) + ".:" + ctAlertType + ":" + alert.watchlistName + ":" + alert.Symbol);
                        AlertString.Append(";");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error in calculating alert String ");
                log.Error(ex);
            }
            return AlertString.ToString();
        }
        internal static void SendAlertsEmailtoUsers()
        {
            try
            {

                Dictionary<int, string> usersEmailDict = EmailAlertsDAO.GetUniqueSubsUser();


                String Subject = "ChartLab Alerts";
                String From = Constants.AdminEmail;
                int emailCounter = 0;
                foreach (KeyValuePair<int, String> user in usersEmailDict)
                {
                    emailCounter++;
                    int userId = user.Key;
                    String To = user.Value;
                    String AlertsString = getUserAlerts(userId);
                    if (emailCounter % 10 == 0)
                    {
                        Thread.Sleep(30000);
                    }
                    if (AlertsString != "")
                    {
                        String Body = Constants.HtmlStartString + AlertsString + Constants.HtmlEndString;
                        MailUtility.SendMail(Subject, Body, From, To);
                        log.Info("Alerts Mail sent to mail id :" + To);
                       
                    }
                }


            }
            catch (Exception ex)
            {
                log.Error("Error in Sending  email alerts ");
                log.Error(ex);
            }

        }
        private static String getUserAlerts(int userId)
        {
            StringBuilder alerts = new StringBuilder();
            try
            {
                List<int> subsIds = EmailAlertsDAO.getUserSubscriptions(userId);

                if (subsIds.IndexOf(0) > -1)
                {
                    UserAlerts userAlerts = EmailAlertsDAO.getPortWlAlertText(userId);
                    String myPortAlerts = "";
                    if (userAlerts.portfolioAlerts != null)
                    {
                        myPortAlerts = formateAlertText(userAlerts.portfolioAlerts);
                        if (myPortAlerts != "")
                        {
                            alerts.Append("<b>My Portfolio Alerts: </b><br>");
                            alerts.Append(myPortAlerts);

                        }
                    }
                }
                try
                {
                    List<int> watchlistIds = subsIds;
                    Dictionary<string, string> myWatchlisttAlerts = EmailAlertsDAO.getSubscribedWlAlert(userId, watchlistIds);
                    foreach (KeyValuePair<string, string> watchlistAlert in myWatchlisttAlerts)
                    {
                        String alert = watchlistAlert.Value;
                        if (alert != "")
                        {

                            String Alertsttext = formateAlertText(alert);

                            alerts.Append("<b>'" + watchlistAlert.Key + "' Watchlist Alerts: </b><br>");
                            alerts.Append(Alertsttext);

                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error in getting watchlist alerts from DB ");
                    log.Error(ex);
                }

            }
            catch (Exception ex)
            {
                log.Error("Error in getting email alerts from DB ");
                log.Error(ex);
            }


            //String CommanAlerts = EmailAlertsDAO.getCommonAlertText(userId);

            //if (CommanAlerts != "")
            //{
            //    alerts.Append(CommanAlerts);

            //}

            return alerts.ToString();
        }
        public static string formateAlertText(string alertText)
        {

            StringBuilder formattedText = new StringBuilder();
            try
            {
                if (alertText != "")
                {
                    String[] stringAlertsArray = alertText.Split(';');
                    foreach (String alertString in stringAlertsArray)
                    {
                        if (alertString != "")
                        {
                            String[] symbolAlert = alertString.Split(':');
                            if (symbolAlert.Length > 1)
                            {
                                int alertType;
                                bool isParsed = int.TryParse(symbolAlert[1], out alertType);
                                if (alertType == 1)
                                {
                                    formattedText.Append("<div Style='color:green; font-weight:bold' ><img align='middle' src='http://www.chartlabpro.com/images/checkGreen.png' />" + symbolAlert[0] + " </div><br>");
                                }
                                else
                                {
                                    formattedText.Append("<div Style='color:maroon;font-weight:bold' ><img align='middle' src='http://www.chartlabpro.com/images/crossRed.png' />" + symbolAlert[0] + " </div><br>");
                                }


                            }
                            else
                            {
                                String[] CTAlerts = alertString.Split('!');
                                if (CTAlerts.Length > 1)
                                {
                                    formattedText.Append("<li><b>" + CTAlerts[0] + "</b></li><br>");
                                }
                                else
                                {
                                    formattedText.Append("<li><b>" + alertString + "</b></li><br>");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error in formatting email alerts ");
                log.Error(ex);
            }
            return formattedText.ToString();
        }
    }
}
