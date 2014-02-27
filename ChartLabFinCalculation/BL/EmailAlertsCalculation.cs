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
                log.Info("EmailAlert: getting subscribed user list");
                List<int> UserList = EmailAlertsDAO.getAllUsers();
                foreach (int userId in UserList)
                {
                    log.Info("EmailAlert: Calculating watchlist alerts for user id : " + userId);
                    watchlistAlerts = calculateWatchlistAlerts(userId, watchlistAlerts);

                    log.Info("EmailAlert: Calculating portfolio alerts for user id : " + userId);
                    //  String portfolioAlerts = calculatePortAlerts(userId);

                    portAlerts.Add(new UserAlerts
                    {
                        userId = userId,
                        //   portfolioAlerts = portfolioAlerts,

                    });
                }
                //CSVExporter.WriteToCSVMyAlertsData(portAlerts, AlertsPath + "/UserAlerts.csv");
                //CSVExporter.WriteToCSVWatchlistAlertData(watchlistAlerts, AlertsPath + "/WatchlistAlerts.csv");
                //EmailAlertsDAO.InsertUserAlertsCSVToDB(AlertsPath);
                //EmailAlertsDAO.updateWatchlistAlertInDB(AlertsPath);




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
                //Dictionary<int, String> userWatchLists = EmailAlertsDAO.getUserWatchLists(userId);
                //foreach (KeyValuePair<int, String> watchlist in userWatchLists)
                //{
                //    String alertString = "";
                //    int watchlistId = watchlist.Key;

                Dictionary<int, List<SymbolAlerts>> symbolAlertsList = EmailAlertsDAO.getMyWatchlistAlerts(userId);
                //if (symbolAlertsList.Count > 0)
                //{
                //    alertString = getAlertString(symbolAlertsList);
                //    watchlistAlerts.Add(new UserAlerts { userId = userId, watchlistId = watchlistId, watchlistAlerts = alertString });
                //    //  EmailAlertsDAO.updateWatchlistAlertInDB(userId, watchlistId, alertString);
                //}


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
            log.Info("EmailAlert: Calculating common model portfolio alerts");
            try
            {
                Dictionary<int, int> commonSubs = EmailAlertsDAO.getCommonSubscriptions();
                foreach (KeyValuePair<int, int> subscription in commonSubs)
                {
                    int subsId = subscription.Key;
                    int watchlistId = subscription.Value;
                    Dictionary<int, List<SymbolAlerts>> symbolAlertsList = EmailAlertsDAO.GetSpecificWLAlerts(watchlistId);
                   


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

                Dictionary<int, List<SymbolAlerts>> symbolAlertsList = EmailAlertsDAO.getMyPortAlerts(userId);
                // alertString = getAlertString(symbolAlertsList);

            }
            catch (Exception ex)
            {
                log.Error("Error in calculaing portfolio alerts for user ID " + userId);
                log.Error(ex);
            }
            return alertString;
        }
        private static string getAlertString(Dictionary<int, List<SymbolAlerts>> wlAlerts)
        {
            StringBuilder AlertString = new StringBuilder();
            try
            {
                AlertString.Append("<table class='alertTable'>");
                AlertString.Append("<tr><th>Stock</th><th>Yesterday Price</th><th>Price Change</th><th>Support</th><th>Resistance</th><th>Alert</th><th>Long Term Trend</th></tr>");
               
                
               
                foreach (KeyValuePair<int, List<SymbolAlerts>> symbolAlertsItem in wlAlerts)
                {
                    int count = 0;
                    List<SymbolAlerts> alerts = symbolAlertsItem.Value;
                    foreach (SymbolAlerts alert in alerts)
                    {
                        if (count == 0)
                        {
                            AlertString.Append("<tr class='" + alert.wlHeaderCss + "'><td>" + alert.watchlistName.ToUpper() + "</td><td></td><td></td><td></td><td></td><td></td><td></td></tr>");
                        }
                        AlertString.Append("<tr><td class='stock'>" + alert.companyName + " - " + alert.Symbol + "</td><td class='lastprice'> $" + alert.price + " </td><td class='change'>  " +alert.priceChangeText+"</td><td class='supportTd'> <div class='support'>" + alert.support + "</div></td><td class='resistanceTd'><div class='resistance'>" + alert.resistance + "</div></td><td class='alert'>" + alert.ratingAlertText + alert.ctRatingAlertText + "</td><td class='lngterm'>" + alert.longTermTrendText + "</td></tr>");
                        count++;     
                    }
                }
                AlertString.Append("</table><br/>");
            }
            catch (Exception ex)
            {
                log.Error("Error in calculating alert String ");
                log.Error(ex);
            }
            return AlertString.ToString();
        }

        /// <summary>
        /// get all users list
        /// get snp Alert
        /// calculate alert for user based on subscritons and send on their mail id 
        /// </summary>
        internal static void SendAlertsEmailtoUsers()
        {
            try
            {
                log.Info("EmailAlert: Geting subscribed user list");
                Dictionary<int, string> usersEmailDict = EmailAlertsDAO.GetUniqueSubsUser();


                String Subject = "ChartLab Alerts";
                String From = ConfigurationManager.AppSettings["AdminEmail"];
                int emailCounter = 0;

                String snpAlertHtmlView = SnpUpdateAlerts.GetSNPUpdateAlert();

                foreach (KeyValuePair<int, String> user in usersEmailDict)
                {
                    emailCounter++;
                    int userId = user.Key;
                    String To = user.Value;
                    log.Info("EmailAlert: Geting user's alert from DB");
                    //snp alert from mongo
                    String AlertsString = "<b>S&P 500 Alert: </b><br/>" + snpAlertHtmlView + "<br/><br/>";

                    //get user alerts
                    AlertsString += getUserAlerts(userId);

                    if (emailCounter % 10 == 0)
                    {
                        Thread.Sleep(30000);
                    }
                    if (AlertsString != "")
                    {
                        //final HTML boly for mail
                        String Body = Constants.HtmlStartStringWithCss + AlertsString + Constants.HtmlEndString;

                      //  string htmlSource = File.ReadAllText(@"C:\Workspace\testmail.html");
                      //  PreMailer


                       InlineResult result = PreMailer.Net.PreMailer.MoveCssInline(Body, true);
                       
                        //sending alert by mail
                        MailUtility.SendMail(Subject, Body, From, To);
                        log.Info("EmailAlert: Alerts Mail sent to mail id :" + To);

                    }
                }


            }
            catch (Exception ex)
            {
                log.Error("Error:  in Sending  email alerts ");
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
                    Dictionary<int, List<SymbolAlerts>> PortAlerts = EmailAlertsDAO.getMyPortAlerts(userId);
                    String myPortAlerts = "";
                    if (PortAlerts.Count > 0)
                    {
                        myPortAlerts = getAlertString(PortAlerts);
                        if (myPortAlerts != "")
                        {
                            alerts.Append("<b>My Portfolio Alerts: </b><br/>");
                            alerts.Append(myPortAlerts);

                        }
                    }
                }
                try
                {
                    List<int> watchlistIds = subsIds;
                    //Dictionary<string, string> myWatchlisttAlerts = EmailAlertsDAO.getSubscribedWlAlert(userId, watchlistIds);
                   Dictionary<int, List<SymbolAlerts>> wlAlerts = EmailAlertsDAO.getMyWatchlistAlerts(userId);
                   String myWlAlerts = "";
                    if (wlAlerts.Count > 0)
                   {
                       myWlAlerts = getAlertString(wlAlerts);
                       if (myWlAlerts != "")
                       {
                           alerts.Append("<b>My Watchlist Alerts: </b><br/>");
                           alerts.Append(myWlAlerts);

                       }
                   }
                   
                }
                catch (Exception ex)
                {
                    log.Error("Error in getting watchlist alerts from DB ");
                    log.Error(ex);
                }

                try
                {
                    Dictionary<int, int> commonSubs = EmailAlertsDAO.getCommonSubscriptions();
                    foreach (KeyValuePair<int, int> subscription in commonSubs)
                    {
                        int subsId = subscription.Key;
                        int watchlistId = subscription.Value;
                        Dictionary<int, List<SymbolAlerts>> commonWlAlerts = EmailAlertsDAO.GetSpecificWLAlerts(watchlistId);
                        String commonWlAlertsStr = "";
                        if (commonWlAlerts.Count > 0)
                        {
                            commonWlAlertsStr = getAlertString(commonWlAlerts);
                            if (commonWlAlertsStr != "")
                            {
                                alerts.Append("<b>My Watchlist Alerts: </b><br/>");
                                alerts.Append(commonWlAlertsStr);

                            }
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
        //public static string formateAlertText(Dictionary<int, List<SymbolAlerts>> wlAlerts)
        //{

        //    StringBuilder formattedText = new StringBuilder();
        //    try
        //    {

        //        String[] stringAlertsArray = //alertText.Split(';');
        //        foreach (String alertString in stringAlertsArray)
        //        {
        //            if (alertString != "")
        //            {
        //                String[] symbolAlert = alertString.Split(':');
        //                if (symbolAlert.Length > 1)
        //                {
        //                    int alertType;
        //                    bool isParsed = int.TryParse(symbolAlert[1], out alertType);
        //                    if (alertType == 1)
        //                    {
        //                        formattedText.Append("<div Style='color:green; font-weight:bold' ><img align='middle' src='http://www.chartlabpro.com/images/checkGreen.png' />" + symbolAlert[0] + " </div><br>");
        //                    }
        //                    else
        //                    {
        //                        formattedText.Append("<div Style='color:maroon;font-weight:bold' ><img align='middle' src='http://www.chartlabpro.com/images/crossRed.png' />" + symbolAlert[0] + " </div><br>");
        //                    }


        //                }
        //                else
        //                {
        //                    String[] CTAlerts = alertString.Split('!');
        //                    if (CTAlerts.Length > 1)
        //                    {
        //                        formattedText.Append("<li><b>" + CTAlerts[0] + "</b></li><br>");
        //                    }
        //                    else
        //                    {
        //                        formattedText.Append("<li><b>" + alertString + "</b></li><br>");
        //                    }
        //                }
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error("Error in formatting email alerts ");
        //        log.Error(ex);
        //    }
        //    return formattedText.ToString();
        //}
    }
}
