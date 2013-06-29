using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinLib;

namespace ChartLabFinCalculation
{
    class SnPPriceCalculation
    {
        public static string SnPSpecificDatePricesPath;
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(SnPPriceCalculation));
        internal static void CalculateSnPPrice()
        {
           List<DateTime> dateList= WatchlistDAO.GetUniqueCreateDates();
           Dictionary<DateTime,double> dateDict=new Dictionary<DateTime,double>();
           Dictionary<DateTime, double> datePriceDict = new Dictionary<DateTime, double>();

           try
           {
               foreach (DateTime date in dateList)
               {
                   if (!dateDict.ContainsKey(date))
                   {
                       dateDict.Add(date, 0);
                   }
               }
               if (!dateDict.ContainsKey(DateTime.Now.Date))
               {
                   dateDict.Add(DateTime.Now.Date, 0);
               }
           }
           catch (Exception ex)
           {
               log.Error(ex);
           }

           try
           {
               foreach (KeyValuePair<DateTime, double> pair in dateDict)
               {
                   double price = SymbolHistoricalMongoDAO.GetSymbolSpecificDatePrice(Constants.GSPCSymbol, pair.Key);
                   if (price != 0 & !datePriceDict.ContainsKey(pair.Key.Date))
                   {
                       datePriceDict.Add(pair.Key, price);
                   }
                   else
                   {
                       log.Warn("Process:  pair.Key.Date not found on Calculating SnP Price " + DateTime.Now);
                   }
               }
           }
           catch (Exception ex)
           {
               log.Error(ex);
           }
           log.Info("Process:  SNP price inserted for Watchlict CreateDate for return inception compare");
           CSVExporter.WriteToCSVSnPPriceList(datePriceDict, SnPSpecificDatePricesPath + "/SnPPriceList.csv");
           SnPPriceDAO.SnPSpecificDatePriceImport(SnPSpecificDatePricesPath + "/SnPPriceList.csv");
        }
       
        
      
    
    }
}
