using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChartLabFinCalculation.DAL;

namespace ChartLabFinCalculation.BL
{
    internal class SnpUpdateAlerts
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(SnpUpdateAlerts));

        /// <summary>
        /// Get snp alert from mongo
        /// </summary>
        /// <returns String>snpHtmlAlertView</returns>
        internal static String GetSNPUpdateAlert()
        {
            String snpHtmlAlertView = string.Empty;
            try
            {
                SnpAlert snpAlert = EmailAlertsDAO.getSnpUpdateAlertMongo();

                string status = snpAlert.status.Replace(" ", "%20");
                string ctDial = snpAlert.ctRatingStatus.Replace(" ", "%20");

                snpHtmlAlertView = @"<table style=' border-bottom: 2px solid gray '><tr><td style='vertical-align: top; width: 70%'>
                                   <div style='height:178px'> " + snpAlert.synopsis + " <a href='www.chartlabpro.com/portManagement?id=1'>See More details...</a></div><div><img src='www.chartlabpro.com/images/alert" +
                                    status + ".png'></img></div></td><td style='vertical-align: top; text-align:center'><img src='www.chartlabpro.com/images/ctDial" +
                                   ctDial + ".png'></img><div style='font-size:18px'>" +
                                   snpAlert.ctRatingStatus + " </div></td></tr></table>";


                
            }
            catch (Exception ex)
            {
                log.Error("Error:  in calclating SNP update alert from mongo for sending in alert mail ");
                log.Error(ex);
            }
            return snpHtmlAlertView;
        }

       
    }
}
