using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MaasOne.Base;
using MaasOne.YahooManaged.Finance;
using System.Collections;
using System.Configuration;

namespace ConsoleApplication1
{
    class HistoricalSecurityData
    {
        public static void Download(String path,List<string> lstSymbols, DateTime fromDate, DateTime toDate,int  numberOfSymbols)
    {
        //Parameters
      
       
       
        foreach (string symbol in lstSymbols)
         {


             IEnumerable<string> ids = new string[] {
                                                        symbol
                                                    };
           
        //     for (int iLoop = 0; iLoop < 10; iLoop++)
          //   {
               
                 MaasOne.YahooManaged.Finance.HistQuotesInterval interval = MaasOne.YahooManaged.Finance.HistQuotesInterval.Daily;
                 //Download
                 MaasOne.YahooManaged.Finance.API.HistQuotesDownload dl = new MaasOne.YahooManaged.Finance.API.HistQuotesDownload();
                 MaasOne.YahooManaged.Finance.API.HistQuotesResponse resp = dl.Download(ids, fromDate, toDate, interval);
                 List<ArrayList> listData = new List<ArrayList>();
                 //Response/Result
                 if (resp.Connection.State == ConnectionState.Success)
                 {
                     foreach (MaasOne.YahooManaged.Finance.HistQuotesDataChain qd in resp.Result)
                     {
                         string id = qd.ID;
                         // MaasOne.YahooManaged.Finance.HistQuoteData first = qd[0];
                         //MaasOne.YahooManaged.Finance.HistQuoteData last = qd[qd.Count - 1];
                         ArrayList row;
                         foreach (HistQuoteData data in qd)
                         {
                             row = new ArrayList();
                             row.Add(id);
                             row.Add(data.High);
                             row.Add(data.Low);
                             row.Add(data.Open);
                             row.Add(data.Close);
                             row.Add(data.Volume);
                             row.Add(data.TradingDate);
                             listData.Add(row);
                             //   Console.WriteLine("Symbol "+id +":"+ data.High +":"+ data.Low +":"+data.TradingDate );
                         }


                         //Console.WriteLine("Symbol "+id + first.);
                     }
                 }
                 //List<ArrayList> listData = new List<ArrayList>();
                 
                     string file = path + "/" + symbol + ".csv";
                     CSVExporter.WriteToCSV(listData, file);
                     Console.WriteLine("Data has been written.Please verify.Symbol :" + symbol + fromDate + " " + toDate);
                 
           //      year++;
          //   }           
               
         }
        Console.Read();
        }

        public static void DownloadAll(String path, List<string> lstSymbols, DateTime fromDate, DateTime toDate, String filename)
        {
            //Parameters


            List<ArrayList> listData = new List<ArrayList>();
            foreach (string symbol in lstSymbols)
            {


                IEnumerable<string> ids = new string[] {
                                                        symbol
                                                    };

                //     for (int iLoop = 0; iLoop < 10; iLoop++)
                //   {

                MaasOne.YahooManaged.Finance.HistQuotesInterval interval = MaasOne.YahooManaged.Finance.HistQuotesInterval.Daily;
                //Download
                MaasOne.YahooManaged.Finance.API.HistQuotesDownload dl = new MaasOne.YahooManaged.Finance.API.HistQuotesDownload();
                MaasOne.YahooManaged.Finance.API.HistQuotesResponse resp = dl.Download(ids, fromDate, toDate, interval);
              
                //Response/Result
                if (resp.Connection.State == ConnectionState.Success)
                {
                    foreach (MaasOne.YahooManaged.Finance.HistQuotesDataChain qd in resp.Result)
                    {
                        string id = qd.ID;
                        // MaasOne.YahooManaged.Finance.HistQuoteData first = qd[0];
                        //MaasOne.YahooManaged.Finance.HistQuoteData last = qd[qd.Count - 1];
                        ArrayList row;
                        foreach (HistQuoteData data in qd)
                        {
                            row = new ArrayList();
                            row.Add(id);
                            row.Add(data.High);
                            row.Add(data.Low);
                            row.Add(data.Open);
                            row.Add(data.Close);
                            row.Add(data.Volume);
                            row.Add(data.TradingDate);
                            listData.Add(row);
                            //   Console.WriteLine("Symbol "+id +":"+ data.High +":"+ data.Low +":"+data.TradingDate );
                        }


                        //Console.WriteLine("Symbol "+id + first.);
                    }
                }
                //List<ArrayList> listData = new List<ArrayList>();

                

                //      year++;
                //   }           

            }
           string file = path + "/" + filename + ".csv";
            CSVExporter.WriteToCSV(listData, file);
            Console.WriteLine("Data has been written.Please verify.Symbol :" + filename + fromDate + " " + toDate);
        }

        public static void DownLoadData(String[] args)
        {

            String option = args[0];
            switch (option)
            {
                case "h":
                    String option2 = args[2];
                    String option3 = args[1];
                    int year = 2011;
                    System.DateTime fromDate;// "1/1/2005 12:00:00 AM";
                    System.DateTime toDate;
                    if (option2 == "c")
                    {
                        fromDate = new DateTime(year, 12, 01);
                        toDate = DateTime.Now.AddDays(-1);


                    }
                    else
                    {

                        fromDate = new DateTime(year, 1, 1);
                        toDate = new DateTime(year, 11, 30);
                    }
                    //  AllSecuritiesDownload.Download();
                    string path = ConfigurationManager.AppSettings["HistoricalDataFilePath"];
                    List<string> lstSymbols = Helper.GetIndicesSymbols();
                    if (option3 == "i")
                    {
                        lstSymbols = Helper.GetIndicesSymbols();
                    }
                    else
                    {

                        lstSymbols = Helper.GetSymbols();
                    }
                    List<String> symbols = new List<string>();
                    if (option2 == "c")
                    {
                        for (int i = 0; i < lstSymbols.Count; i++)
                        {
                            if (i % 100 == 0 && i != 0)
                            {
                                HistoricalSecurityData.DownloadAll(path, symbols, fromDate, toDate, i.ToString());
                                symbols = new List<string>();
                            }
                            symbols.Add(lstSymbols[i]);

                        }
                        HistoricalSecurityData.DownloadAll(path, symbols, fromDate, toDate, lstSymbols.Count.ToString());
                    }
                    break;

                default:

                    break;


            }

        }
    }
}
