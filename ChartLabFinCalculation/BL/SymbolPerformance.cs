using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChartLabFinCalculation
{
    class SymbolPerformance
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(SymbolPerformance));

        public static void CalculateSymbolPerf()
        {

          Dictionary<string,double> symbolPriceList=  SymbolPerfDAO.GetSymbolPrice();
          SymbolPerfDAO.DeleteSymbolPerformance();
          foreach (KeyValuePair<string,double> pair in symbolPriceList)
          {
              SymbolPerfDAO.InsertSymbolPrice(pair.Key,pair.Value);
              
          }
          log.Info("\nData inserted in symbolperformance table....... \n");

          SymbolPerfDAO.UpdateSymbolPerformance();
        
        }

    }
}
