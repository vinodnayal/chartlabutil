using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinLib;
using System.IO;

namespace ChartLabFinCalculation
{
    class HistoricalDataImporter
    {
        public static String HistoricalDataFilePath ;
        public static String ERRORSymbolsPath;
        public static String NewSymbolsPath;

        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(HistoricalDataImporter));

        public static void SaveHistData(DateTime fromDate, DateTime toDate, bool isMF, bool specificSymbols)
        {
            List<string> errorSymbolList = new List<string>();
            try
            {
               

                List<String> symbolList = new List<string>();

               
                if (isMF)
                {

                    symbolList = SymbolHistoricalDAO.GetIndexMFFromDB();
                }
                else if (specificSymbols)
                {
                    symbolList = SymbolHistoricalDAO.GetsymbolListForNewlyAddedSymbols();
                }
                else
                {
                    symbolList = SymbolHistoricalDAO.GetsymbolListFromDB();
                }

                foreach (String symbol in symbolList)
                {
                    errorSymbolList = SaveHistDataSymbol(fromDate, toDate, symbol, isMF, specificSymbols);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            if (errorSymbolList.Count > 0)
            {
                CSVExporter.SaveErrorSymbols(errorSymbolList, ERRORSymbolsPath + "/Symbol.csv");
            }
        }

        public static List<String> SaveHistDataSymbol(DateTime fromDate, DateTime toDate, String symbol, bool isMF, bool specificSymbols)
        {
            List<String> errorSymbolList = new List<string>();
            List<InputBarData> listInputDataForSymbols =new List<InputBarData>();
            String fileName;
            try
            {
                List<InputBarData> bardata = ImportData(fromDate, toDate, symbol, isMF, specificSymbols);
                if (bardata.Count == 0)
                {
                    errorSymbolList.Add(symbol);
                    listInputDataForSymbols.AddRange(bardata);
                }
                else
                {
                    listInputDataForSymbols.AddRange(bardata);
                }
            }
            catch (Exception ex)
            {
                errorSymbolList.Add(symbol);
            }
            if (isMF)
            {
                symbol=symbol.Replace('*',' ');
            }
            if (specificSymbols)
            {
                fileName = NewSymbolsPath + "/" + symbol + ".csv";
            }
            else
            {
                fileName = HistoricalDataFilePath + "/" + symbol + ".csv";
            }
            
                CSVExporter.WriteToCSV(listInputDataForSymbols, fileName);
            
            if (isMF || specificSymbols)
            {
                SymbolHistoricalDAO.SaveHistoricalDataCSVToDB(fileName,symbol);
            }
            else
            {
                SymbolHistoricalDAO.SaveHistoricalDataCSVToDB(fileName);
            }

            return errorSymbolList;
               
            
        }



        private static List<InputBarData> ImportData(DateTime fromDate, DateTime toDate, string symbol, bool isMF, bool specificSymbols)
        {

            List<InputBarData> listInputDataForSymbols = new List<InputBarData>();

            List<BarData> barlist = new List<BarData>();
            try
            {
                //if (isMF & !isYahoo)
                //{
                //    barlist = DataDownloader.GetDataFeedFromGoogle(fromDate, toDate, symbol); //GOOGLE FINANCE
                //}
                //else
                //{
                    barlist = DataDownloader.GetDataFromFeedFromYahoo(fromDate, toDate, symbol);
                //}
            }
            catch (Exception ex)
            {
                
                log.Warn(ex);
                
            }
            if (barlist == null || barlist.Count == 0)
            {

                log.Info("Empty List Returned From Provider" + symbol);
            }
            else
            {
                InputBarData inputListRow = new InputBarData();
                inputListRow.barListRow = barlist;
                inputListRow.symbol = symbol;
                listInputDataForSymbols.Add(inputListRow);
              
            }
            return listInputDataForSymbols;

        }



        public static void SaveDailyData(DateTime fromDate, DateTime toDate, bool isMF)
        {


            try
            {
                List<String> errorSymbolList = new List<string>();

                List<InputBarData> listInputDataForSymbols = new List<InputBarData>();
                List<String> symbolList = new List<string>();
                if (isMF)
                {

                    symbolList = SymbolHistoricalDAO.GetIndexMFFromDB();
                }
                else
                {
                    symbolList = SymbolHistoricalDAO.GetsymbolListFromDB();
                }
                foreach (String symbol in symbolList)
                {
                    if (isMF)
                    {
                        listInputDataForSymbols.AddRange(ImportData(fromDate, toDate, symbol, isMF, false));
                    }
                    else
                    {
                        bool shouldUpdateHistoricalData = false;
                        DividendHistory dividendhistory=DataDownloader.CheckIfThereIsAnyDividend(fromDate, toDate, symbol);
                        
                        if (dividendhistory.isdividend)
                        {
                            shouldUpdateHistoricalData = SymbolHistoricalDAO.CheckForDividendHistory(symbol, dividendhistory.dividendDate, dividendhistory.todaysDate);
                        }

                        if (shouldUpdateHistoricalData)
                        {
                            SymbolHistoricalDAO.DeleteData(symbol);
                            DateTime newfromDate = DateTime.Now.AddYears(-Constants.HIST_DATA_LENGTH).Date;

                            SaveHistDataSymbol(newfromDate, toDate, symbol, isMF, false);
                            SymbolHistoricalDAO.InsertDividendRow(symbol, dividendhistory.dividendDate, dividendhistory.todaysDate);
                        }
                        else
                        {
                            List<InputBarData> bardata = ImportData(fromDate, toDate, symbol, isMF, false);
                         if (bardata.Count == 0)
                         {
                             errorSymbolList.Add(symbol);
                         }
                         else
                         {
                             listInputDataForSymbols.AddRange(bardata);
                         }
                        }
                    }
                   
                }
                String fileName = HistoricalDataFilePath + "/DailyAllSymbols.csv";
                CSVExporter.WriteToCSV(listInputDataForSymbols, fileName);
                SymbolHistoricalDAO.SaveHistoricalDataCSVToDB(fileName);
               
                    CSVExporter.SaveErrorSymbols(errorSymbolList, ERRORSymbolsPath + "/Symbol.csv");
                
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

        }



        public static void DailyErrorSymbolData(DateTime fromDate, DateTime toDate, string filename,bool isHistorical)
        {
            List<string> missingSymbolList = new List<string>();
            List<String> errorSymbolList = new List<string>();
            List<InputBarData> listInputDataForSymbols = new List<InputBarData>();

            StreamReader reader = new StreamReader(filename);
            string data = reader.ReadToEnd();
            string[] rows = data.Split(',');

            foreach (string row in rows)
            {
                missingSymbolList.Add(row);
                
            }
            missingSymbolList.RemoveRange(missingSymbolList.Count()-1,1);

            reader.Close();


            foreach (string symbol in missingSymbolList)
            {

                if (isHistorical)
                {
                    SymbolHistoricalDAO.DeleteData(symbol);
                  errorSymbolList.AddRange(SaveHistDataSymbol(fromDate, toDate, symbol, false, false));
                }
                else
                {
                    bool shouldUpdateHistoricalData = false;
                    DividendHistory dividendhistory = DataDownloader.CheckIfThereIsAnyDividend(fromDate, toDate, symbol);

                    if (dividendhistory.isdividend)
                    {
                        shouldUpdateHistoricalData = SymbolHistoricalDAO.CheckForDividendHistory(symbol, dividendhistory.dividendDate, dividendhistory.todaysDate);
                    }

                    if (shouldUpdateHistoricalData)
                    {
                        SymbolHistoricalDAO.DeleteData(symbol);
                        DateTime newfromDate = DateTime.Now.AddYears(-Constants.HIST_DATA_LENGTH).Date;

                        errorSymbolList.AddRange(SaveHistDataSymbol(newfromDate, toDate, symbol, false, false));
                        SymbolHistoricalDAO.InsertDividendRow(symbol, dividendhistory.dividendDate, dividendhistory.todaysDate);
                    }
                    else
                    {
                        List<InputBarData> bardata = ImportData(fromDate, toDate, symbol, false, false);
                        if (bardata.Count == 0)
                        {
                            errorSymbolList.Add(symbol);
                        }
                        else
                        {
                            listInputDataForSymbols.AddRange(bardata);
                        }
                    }
                   

                }

              
               
            }
          
                CSVExporter.SaveErrorSymbols(errorSymbolList, ERRORSymbolsPath + "/Symbol.csv");
            
            if (!isHistorical)
            {
                String fileName = HistoricalDataFilePath + "/DailyAllErrorSymbols.csv";
                CSVExporter.WriteToCSV(listInputDataForSymbols, fileName);
                SymbolHistoricalDAO.SaveHistoricalDataCSVToDB(fileName);

            }

           
        }
    }
}
