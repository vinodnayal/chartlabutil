using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinLib;
using FinLib.Model;

namespace ChartLabFinCalculation.BL
{
   internal class SnPSymbolsCalculations
    {
       static log4net.ILog log = log4net.LogManager.GetLogger(typeof(SnPSymbolsCalculations));
        internal static void calculateSynosisRuleID()
        {
            try
            {
              List<SymbolRatingAlert> snpSymbolsRatingChanges=  BuySellRatingDAO.getSNPSymbolsRatingChange();
              List<SnpAnalytics> snpSymbolsAnalytics = new List<SnpAnalytics>();
                int snpCTRating=4; //neutral trend
                List<Rating> snpRating = BuySellRatingDAO.GetRatingsOfSymbol("SPY", false); //Constants.SnPSymbol
                if (snpRating.Count > 0)
                {
                    snpCTRating = snpRating[0].ctRating;
                }
              foreach (SymbolRatingAlert symbolRating in snpSymbolsRatingChanges)
              {
                  String ruleId="";

                  switch (snpCTRating)
                  {
                      case (int)CTRatingEnum.Overbought:

                        ruleId=  getSynopsisIdOnRatingChange(symbolRating);
                        ruleId = "S2" + ruleId;
                          break;
                      default:
                          ruleId = getSynopsisIdOnRatingChange(symbolRating);

                          break;

                  }

                  snpSymbolsAnalytics.Add(new SnpAnalytics { symbol = symbolRating.symbol,synopsisRuleId=ruleId });

              
              
              }

              CSVExporter.WriteToCSVRating(snpSymbolsAnalytics, snpDatafilesPath + "/snpAnalyticsFile.csv");
              log.Info("Process :Write snp Analytics To CSV  ");

              SNPAnalyticsDAO.InsertRating(snpDatafilesPath);
              log.Info("Process :Inserted snp Analytics csv File in to DB: ");


               
            }
            catch (Exception ex)
            {

                log.Error("Error: When claculation synopsis rule Id for snp symbols, " +ex);
            }

        }

        private static string getSynopsisIdOnRatingChange(SymbolRatingAlert symbolRating)
        {
            StringBuilder ruleId = new StringBuilder();
            
            try
            {
                
                int curRating = symbolRating.currating;
                int preRating = symbolRating.prevrating;
                int ctRating = symbolRating.ctrating;
                if(preRating>3)
                ruleId.Append("R" + preRating + curRating);
                if (ctRating <= 3)
                ruleId.Append("C" + ctRating);
                if ((DateTime.Now - symbolRating.ratingChangeDate).TotalDays > 110 && curRating==4)
                {

                    ruleId.Append("D");
                }

            }
            catch (Exception ex)
            {

                log.Error("Error: When claculation synopsis rule Id for symbol : "+symbolRating.symbol + ex);
            }
           return ruleId.ToString();
        }


        public static string snpDatafilesPath { get; set; }
    }
}
