using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinLib;

namespace ChartLabFinCalculation
{
    class OBOSRatingCalculation
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(OBOSRatingCalculation));

        public static String OBOSRatingPath;

        public static void CalculateOBOSRating(bool isHistorical)
        {

            try
            {

                string type = null;
                if (isHistorical)
                {
                    type = "history";
                }
                else
                {
                    type = "current date";
                }

                log.Info("Process: Calculaitng OBOSCount for  " + type + "....");
                List<DateOBOSCount> listObOsCount = calculateOBOS(isHistorical);
                CSVExporter.WriteToCSVOBOS(listObOsCount, OBOSRatingPath + "/OBOSCount.csv");

                OBOSRatingDAO.OBOSPercentage(OBOSRatingPath, isHistorical);
            }
            catch (Exception ex )
            {

                log.Error("Error:"+ex);
            }


        }


          private  static List<DateOBOSCount> calculateOBOS(bool historical)
        {
            FinCalculator fincalc = new FinCalculator();
            List<DateOBOSCount> listOBOSCount = new List<DateOBOSCount>();
            try
            {
                DateTime fromdate = DateTime.Now.AddDays(-Constants.HIST_DAYS_LENGTH_FOR_SA);
                DateTime toDate = DateTime.Now;
                List<String> symbolList = OBOSRatingDAO.GetSnPSymbols();



                int count = 0;
                log.Info("Process: Calculating for symbols list count=" + symbolList.Count);
                foreach (string symbol in symbolList)
                {
                    log.Info("Calculating for symbol=" + symbol);
                    List<BarData> barlist = null;
                    try
                    {

                        barlist = SymbolHistoricalMongoDAO.GetHistoricalDataFromMongo(fromdate, toDate, symbol);
                        if (barlist == null || barlist.Count == 0)
                        {

                            log.Warn("Warn: Empty List Returned From Provider" + symbol);
                        }
                        else
                        {

                            List<DateOBOS> listOBOS = fincalc.CalculateOBOSForRange(barlist);
                            if (listOBOS.Count > 1)
                            {
                                if (!historical)
                                {
                                    listOBOS = listOBOS.GetRange(listOBOS.Count - 1, 1);

                                    if (count == 0)
                                    {
                                        listOBOSCount = calculateOBSOfirstTime(listOBOS, symbolList);


                                    }

                                    else
                                    {
                                        calculateOBOSNext(listOBOSCount, listOBOS, symbolList);

                                    }
                                    count++;
                                }

                                else
                                {
                                    if (count == 0)
                                    {
                                        listOBOSCount = calculateOBSOfirstTime(listOBOS, symbolList);


                                    }

                                    else
                                    {
                                        calculateOBOSNext(listOBOSCount, listOBOS, symbolList);



                                    }
                                    count++;
                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {

                        log.Error("Error" + ex);
                    }
                    
                }
            }
            catch (Exception ex)
            {

                log.Error("Error:" + ex);
            }
            return listOBOSCount;
        }

          private static List<DateOBOSCount> calculateOBSOfirstTime(List<DateOBOS> listOBOS, List<string> symbols)
          {
              log.Info("Process: calculate OBSO first Time");
              List<DateOBOSCount> listOBOSCount = new List<DateOBOSCount>();

              try
              {
                  foreach (DateOBOS obos in listOBOS)
                  {
                      DateOBOSCount obosCount = new DateOBOSCount();
                      obosCount.Date = obos.Date;
                      if (obos.obos == obosEnum.OS)
                      {

                          obosCount.osCount++;
                          obosCount.osPer = (obosCount.osCount * 100) / symbols.Count;
                      }
                      else if (obos.obos == obosEnum.OB)
                      {

                          obosCount.obCount++;
                          obosCount.obPer = (obosCount.obCount * 100) / symbols.Count;

                      }
                      listOBOSCount.Add(obosCount);
                  }
              }
              catch (Exception ex)
              {

                  log.Error("Error:" + ex);
              }
              return listOBOSCount;
          }

          private static void calculateOBOSNext(List<DateOBOSCount> listOBOSCount, List<DateOBOS> listOBOS, List<string> symbols)
          {
             
              try
              {
                  // int index = listOBOSCount.Count-1;
                  int countlistOBOS = listOBOS.Count;
                  //DateOBOSCount obosCount in listOBOSCount
                  for (int index = listOBOSCount.Count - 1; index >= 0; index--)
                  {
                      DateOBOSCount obosCount = listOBOSCount[index];
                      if (countlistOBOS - 1 >= index)
                      {
                          DateOBOS obos = listOBOS[index];
                          if (obos.obos == obosEnum.OS)
                          {

                              obosCount.osCount++;
                              obosCount.osPer = (obosCount.osCount * 100) / symbols.Count;
                          }
                          else if (obos.obos == obosEnum.OB)
                          {

                              obosCount.obCount++;
                              obosCount.obPer = (obosCount.obCount * 100) / symbols.Count;
                          }

                      }
                      else
                      {
                          break;
                      }



                  }
              }
              catch (Exception ex)
              {

                  log.Error("Error:" + ex);
              }

          }

    }
}
