using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace ChartLabFinCalculation
{
    class RestartCache
    {
       static log4net.ILog log = log4net.LogManager.GetLogger(typeof(Program));
       public static void RestartingMemCache()
       {
           string url = string.Format("http://chartlabpro.com/restartCache.php");

           try
           {
               HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
               WebResponse response = request.GetResponse();
               
           }
           catch (Exception ex)
           {
               log.Error(ex);
           }
       }
    }
}
