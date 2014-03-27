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
using PreMailer.Net;


namespace ChartLabFinCalculation.BL
{
    public class EmailAlertsCalculation
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(EmailAlertsCalculation));
        static string AlertsPath = ConfigurationManager.AppSettings["AlertsPath"];

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

                //todo for testing-om
                //Dictionary<int, string> usersEmailDict = new Dictionary<int, string>();
                //usersEmailDict.Add(120, "om.omshiv@gmail.com");

                String Subject = "ChartLab Alerts";
                String From = ConfigurationManager.AppSettings["AdminEmail"];
                int emailCounter = 0;

                String snpAlertHtmlView = SnpUpdateAlerts.GetSNPUpdateAlert();
                Dictionary<int, String> commonWlAlerts = getCommonSubAlerts();
                foreach (KeyValuePair<int, String> user in usersEmailDict)
                {
                    emailCounter++;
                    int userId = user.Key;
                    String To = user.Value;
                    log.Info("EmailAlert: Geting user's alert from DB userId:" + userId);
                    //snp alert from mongo
                    StringBuilder alertsSB = new StringBuilder();
                    alertsSB.Append("<b>S&P 500 Alert: </b><br/>" + snpAlertHtmlView + "<br/><br/>");


                    //get user alerts
                    alertsSB.Append(getUserAlerts(userId, commonWlAlerts));

                    if (emailCounter % 10 == 0)
                    {
                        Thread.Sleep(30000);
                    }
                    if (alertsSB.Length > 0)
                    {
                        //final HTML boly for mail
                        String Body = Constants.HtmlStartStringWithCss + alertsSB.ToString() + Constants.HtmlEndString;

                        //  string htmlSource = File.ReadAllText(@"C:\Workspace\testmail.html");
                        //  PreMailer


                        InlineResult result = PreMailer.Net.PreMailer.MoveCssInline(Body, true);

                        //sending alert by mail
                        if (!string.IsNullOrEmpty(To))
                        {
                            log.Info("EmailAlert: Alerts Mail sending to mail id :" + To);
                            MailUtility.SendMail(Subject, result.Html, From, To);
                            log.Info("EmailAlert: Alerts Mail sent to mail id :" + To);
                        }
                        else
                        {
                            log.Info("EmailAlert: user mail id is blank for userid :" + userId);
                        }
                    }
                    else
                    {
                        log.Info("EmailAlert: No mail alerts found for userid :" + userId);
                    }
                }


            }
            catch (Exception ex)
            {
                log.Error("Error:  in Sending  email alerts ");
                log.Error(ex);
            }

        }

        /// <summary>
        /// get formmated alert in HTMl
        /// </summary>
        /// <param name="wlAlerts"></param>
        /// <returns></returns>
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
                        AlertString.Append("<tr><td class='stock'>" + alert.companyName + " - " + alert.Symbol + "</td><td class='lastprice'> $" + Math.Round(alert.price, 2) + " </td><td class='change'>  " + alert.priceChangeText + "</td><td class='supportTd'> <div class='support'>" + alert.support + "</div></td><td class='resistanceTd'><div class='resistance'>" + alert.resistance + "</div></td><td class='alert'>" + alert.ratingAlertText + alert.ctRatingAlertText + "</td><td class='lngterm'>" + alert.longTermTrendText + "</td></tr>");
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
        /// get Common wl Alerts
        /// </summary>
        /// <returns></returns>
        private static Dictionary<int, String> getCommonSubAlerts()
        {
            Dictionary<int, String> alerts = new Dictionary<int, String>();
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
                            if (!alerts.ContainsKey(subsId))
                                alerts.Add(subsId, commonWlAlertsStr);

                        }
                    }


                }


            }
            catch (Exception ex)
            {
                log.Error("Error in getting watchlist alerts from DB ");
                log.Error(ex);
            }
            return alerts;
        }

        /// <summary>
        /// get users all alerts (port,wl,common)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="commonWlAlertsDict"></param>
        /// <returns></returns>
        private static String getUserAlerts(int userId, Dictionary<int, String> commonWlAlertsDict)
        {
            StringBuilder alerts = new StringBuilder();
            try
            {
                List<int> subsIds = EmailAlertsDAO.getUserSubscriptions(userId);

                if (subsIds.IndexOf(0) > -1)
                {
                    try
                    {
                        Dictionary<int, List<SymbolAlerts>> PortAlerts = EmailAlertsDAO.getMyPortAlerts(userId);
                        String myPortAlerts = "";
                        if (PortAlerts.Count > 0)
                        {
                            myPortAlerts = getAlertString(PortAlerts);
                            if (myPortAlerts != "")
                            {
                                alerts.Append("<b>My Portfolio Alerts: </b><br/><br/>");
                                alerts.Append(myPortAlerts);

                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        log.Error("Error in getting porfolio alerts from DB ");
                        log.Error(ex);
                    }
                }
                try
                {
                    List<int> watchlistIds = subsIds;
                    Dictionary<int, List<SymbolAlerts>> wlAlerts = EmailAlertsDAO.getMyWatchlistAlerts(userId);
                    String myWlAlerts = "";
                    if (wlAlerts.Count > 0)
                    {
                        myWlAlerts = getAlertString(wlAlerts);
                        if (myWlAlerts != "")
                        {
                            alerts.Append("<b>My Watchlist Alerts: </b><br/><br/>");
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
                    bool isCommonAlertFound = false;
                    alerts.Append("<b>Model Portfolio Alerts: </b><br/><br/>");
                    foreach (int subsId in subsIds)
                    {
                        if (commonWlAlertsDict.ContainsKey(subsId))
                        {
                            alerts.Append(commonWlAlertsDict[subsId]);
                            isCommonAlertFound = true;
                        }
                    }

                    if (!isCommonAlertFound)
                    {
                        alerts.Append("</b><b>No Alerts! </b><br/>");
                    }

                }
                catch (Exception ex)
                {
                    log.Error("Error in getting common wl alerts from commonAlerts Dic ");
                    log.Error(ex);
                }

            }
            catch (Exception ex)
            {
                log.Error("Error in getting email alerts from DB ");
                log.Error(ex);
            }

            return alerts.ToString();
        }

    }
}
