using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;

namespace ChartLabFinCalculation.BL
{
    class WebServices
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(WebServices));

        public static void getSNPSymbolsData()
        {
            try
            {

                log.Info("SnpDataImport: Requesting for data import 0 to 250 symbols....");
                startSNPDataImport(0, 250);

                Thread.Sleep(1000 * 20);  //20 second wait
                log.Info("SnpDataImport: Requesting for data import 251 to 500 symbols...");
                startSNPDataImport(251, 500);


            }
            catch (Exception ex)
            {

                log.Error("Error: error in importing snp symbols cur data using web request " + ex);
            }

        }

        private static void startSNPDataImport(int startindex, int range)
        {
            string url = string.Format("http://chartlabpro.com/SnpDataImport?index=" + startindex + "&range=" + range);

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                WebResponse response = request.GetResponse();
                log.Info("SnpDataImport : Done");
            }
            catch (Exception ex)
            {
                log.Error("Error : " + ex);
            }
        }






    }
}
