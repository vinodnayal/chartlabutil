using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinLib;
using System.Net;
using System.IO;
using System.Globalization;

namespace ChartLabFinCalculation
{
    class UnusedUTIL
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(UnusedUTIL));
        private static readonly CultureInfo ciUS = new CultureInfo("en-us");

        public static List<BarData> GetDataFromFeedFromBATS(DateTime from, DateTime to, string symbol)
        {
            log.Info("\n\n Getting Data from BATS  " + symbol);
            List<BarData> barData = new List<BarData>();

            // chartlab   // testdev
            string url = string.Format("http://ds01.ddfplus.com/historical/queryeod.ashx?username=" + Constants.BATSUsername + "&password=" + Constants.BATSPassword + "&symbol=" + symbol + "&start=" + from.ToString("yyyyMMdd") + "&end=" + to.ToString("yyyyMMdd") + "&order=&data=daily");
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Proxy = null;
                //String arrayrow= File.GetAttributes(url);
                using (WebResponse response = request.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        if (stream == null)
                        {

                        }

                        using (StreamReader sr = new StreamReader(stream))
                        {
                            string data = sr.ReadToEnd();
                            string[] rows = data.Split('\n');

                            foreach (string row in rows)
                            {
                                string[] values = row.Split(',');
                                if (values.Length < 7)
                                    continue;


                                barData.Add(new BarData
                                {

                                    open = Convert.ToDouble(values[2], ciUS),
                                    high = Convert.ToDouble(values[3], ciUS),
                                    low = Convert.ToDouble(values[4], ciUS),
                                    close = Convert.ToDouble(values[5], ciUS),
                                    actualclose = Convert.ToDouble(values[5], ciUS),
                                    volume = Convert.ToDouble(values[6], ciUS),
                                    date = DateTime.Parse(values[1])
                                });
                            }


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return barData;

        }

    }
}
