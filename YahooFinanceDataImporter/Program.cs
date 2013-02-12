using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using FinLib;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime fromdate=DateTime.Now.AddDays(-150);
            DateTime toDate=DateTime.Now.AddDays(-2);
          
            FinCalculator fincalc = new FinCalculator();
          //  fincalc.CalulateRSI("MSFT", fromdate.Date, toDate.Date,null);

            if (args.Length == 3)
            {
                //HistoricalSecurityData.DownLoadData(args);
               

                //   CurrentDelayedData.InsertTest();
            }
        }
    }
}
