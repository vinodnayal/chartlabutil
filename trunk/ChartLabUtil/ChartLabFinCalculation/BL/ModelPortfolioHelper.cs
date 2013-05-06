using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChartLabFinCalculation.DAL;
using System.Configuration;
using System.Threading;
using ChartLabFinCalculation.UTIL;

namespace ChartLabFinCalculation.BL
{
    class ModelPortfolioHelper
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(ModelPortfolioHelper));
        internal static void sendModelPortSignals()
        {
            try
            {
                String Subject = "ChartLab Alerts";
                String From = ConfigurationManager.AppSettings["AdminEmail"];
                // get all model portfolio list
                Dictionary<int, string> modelportfolioSignalsList = EmailAlertsDAO.getModelPortfolioIdsFromDB();
                foreach (KeyValuePair<int, string> modelPort in modelportfolioSignalsList)
                {
                    int watchListId = modelPort.Key;
                    String signal = modelPort.Value;
                   
                    if(signal!="" && signal!=null)
                    log.Info("EmailAlert: Geting subscribed user list");
                    Dictionary<int, string> usersEmailDict = EmailAlertsDAO.GetModelPortUniqueUser(watchListId);

                    try
                    {

                        int emailCounter = 0;
                        foreach (KeyValuePair<int, String> user in usersEmailDict)
                        {
                            emailCounter++;
                            int userId = user.Key;
                            String To = user.Value;
                            log.Info("EmailAlert: Geting user's alert from DB");
                            String AlertsString = "";
                            if (emailCounter % 10 == 0)
                            {
                                Thread.Sleep(30000);
                            }
                            if (AlertsString != "")
                            {
                                String Body = Constants.HtmlStartString + AlertsString + Constants.HtmlEndString;
                                MailUtility.SendMail(Subject, Body, From, To);
                                log.Info("EmailAlert: Alerts Mail sent to mail id :" + To);

                            }
                        }


                    }
                    catch (Exception ex)
                    {
                   
                        log.Error("Error:  in Sending  email alerts for port folio  " +watchListId);
                        log.Error(ex);
                    }


                }
            }
            catch (Exception ex)
            {
                
               log.Error("Error:  in Sending  email alerts ");
                        log.Error(ex);
            }

            

        }
}
           
       
    
}
