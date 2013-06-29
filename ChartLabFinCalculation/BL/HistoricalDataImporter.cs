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
                log.Info("Process: Historical data import for specific symbol");

                List<String> symbolList = new List<string>();

               
                if (isMF)
                {

                    symbolList = SymbolHistoricalDAO.GetIndexMFFromDB();
                }
                else if (specificSymbols)
                {
                    log.Info("Process: Historical data import for specific symbol newly added");
                    symbolList = SymbolHistoricalDAO.GetsymbolListForNewlyAddedSymbols();
                }
                else
                {
                    
                    symbolList = SymbolHistoricalDAO.GetsymbolListFromDB();
                    log.Info("Process: Historical data import Get symbol List From DB");
                }

                foreach (String symbol in symbolList)
                {
                    errorSymbolList = SaveHistDataSymbol(fromDate, toDate, symbol, isMF, specificSymbols, Constants.SymbolHistoricalTble);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error: "+ex);
            }
            if (errorSymbolList.Count > 0)
            {
                log.Info("Process: Save Error Symbols list in CSV file");
                CSVExporter.SaveErrorSymbols(errorSymbolList, ERRORSymbolsPath + "/Symbol.csv");
            }
        }

        public static List<String> SaveHistDataSymbol(DateTime fromDate, DateTime toDate, String symbol, bool isMF, bool specificSymbols,String tableName)
        {
            List<String> errorSymbolList = new List<string>();
            try
            {
                List<InputBarData> listInputDataForSymbols = new List<InputBarData>();
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
                    log.Error("Error: " + ex);
                    errorSymbolList.Add(symbol);
                }
                if (isMF)
                {
                    symbol = symbol.Replace('*', ' ');
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
                    SymbolHistoricalDAO.SaveHistoricalDataCSVToDB(fileName, symbol, tableName);
                }
                else
                {
                    SymbolHistoricalDAO.SaveHistoricalDataCSVToDB(fileName, tableName);
                }
            }
            catch (Exception ex)
            {

                log.Error("Error: " + ex);
            }

            return errorSymbolList;
               
            
        }

        internal static List<InputBarData> ImportData(DateTime fromDate, DateTime toDate, string symbol, bool isMF, bool specificSymbols)
        {
            
            List<InputBarData> listInputDataForSymbols = new List<InputBarData>();

            List<BarData> barlist = new List<BarData>();
            try
            {
               
              barlist = DataDownloader.GetDataFromFeedFromYahoo(fromDate, toDate, symbol);
                
            }
            catch (Exception ex)
            {

                log.Error("Error: " + ex);
                
            }
            if (barlist == null || barlist.Count == 0)
            {

                log.Warn("Warn: Daily data import, Empty List Returned From Provider " + symbol);
            }
            else
            {
                try
                {
                    InputBarData inputListRow = new InputBarData();
                    inputListRow.barListRow = barlist;
                    inputListRow.symbol = symbol;
                    listInputDataForSymbols.Add(inputListRow);
                }
                catch (Exception ex)
                {
                    
                     log.Error("Error: "+ex);
                }
              
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
                    log.Info("Process: Daily Data Import getting mutual fund symbols list DB");
                    symbolList = SymbolHistoricalDAO.GetIndexMFFromDB();
                }
                else
                {
                    log.Info("Process: Daily Data Import getting  symbols list from DB");
                    symbolList = SymbolHistoricalDAO.GetsymbolListFromDB();
                }
                log.Info("Process: import data from yahoo symbols list count: " + symbolList.Count);
                foreach (String symbol in symbolList)
                {
                    if (isMF)
                    {
                       
                        listInputDataForSymbols.AddRange(ImportData(fromDate, toDate, symbol, isMF, false));
                    }
                    else
                    {
                        bool shouldUpdateHistoricalData = false;
                      //  log.Info("Process: Check If There Is Any Dividend for symbol: " + symbol);
                        DividendHistory dividendhistory=DataDownloader.CheckIfThereIsAnyDividend(fromDate, toDate, symbol);
                        
                        if (dividendhistory.isdividend)
                        {
                           // log.Info("Process: Check For Dividend History for symbol: " + symbol);
                            shouldUpdateHistoricalData = SymbolHistoricalDAO.CheckForDividendHistory(symbol, dividendhistory.dividendDate, dividendhistory.todaysDate);
                        }

                        if (shouldUpdateHistoricalData)
                        {
                            SymbolHistoricalDAO.DeleteData(symbol, Constants.SymbolHistoricalTble);
                            DateTime newfromDate = DateTime.Now.AddYears(-Constants.HIST_DATA_LENGTH).Date;

                            SaveHistDataSymbol(newfromDate, toDate, symbol, isMF, false, Constants.SymbolHistoricalTble);
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
                log.Info("Process: Daily Data Write To CSV");
                CSVExporter.WriteToCSV(listInputDataForSymbols, fileName);
                log.Info("Process: Daily Data Write  CSV To DB");
                SymbolHistoricalDAO.SaveHistoricalDataCSVToDB(fileName,Constants.SymbolHistoricalTble);
               
                CSVExporter.SaveErrorSymbols(errorSymbolList, ERRORSymbolsPath + "/Symbol.csv");
                
            }
            catch (Exception ex)
            {
                log.Error("Error: "+ex);
            }

        }
        /// <summary>
        /// daily error symbol import 
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="filename"></param>
        /// <param name="isHistorical"></param>
        public static void DailyErrorSymbolData(DateTime fromDate, DateTime toDate, string filename,bool isHistorical)
        {

            try
            {
                log.Info("Process: Daily Error Symbol Data Import ");

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
                missingSymbolList.RemoveRange(missingSymbolList.Count() - 1, 1);

                reader.Close();

                log.Info("Process: Daily Error Symbol Data Import for symbol count " + missingSymbolList.Count);
                foreach (string symbol in missingSymbolList)
                {

                    if (isHistorical)
                    {
                        SymbolHistoricalDAO.DeleteData(symbol, Constants.SymbolHistoricalTble);
                        errorSymbolList.AddRange(SaveHistDataSymbol(fromDate, toDate, symbol, false, false, Constants.SymbolHistoricalTble));
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
                            SymbolHistoricalDAO.DeleteData(symbol, Constants.SymbolHistoricalTble);
                            DateTime newfromDate = DateTime.Now.AddYears(-Constants.HIST_DATA_LENGTH).Date;

                            errorSymbolList.AddRange(SaveHistDataSymbol(newfromDate, toDate, symbol, false, false, Constants.SymbolHistoricalTble));
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
                log.Info("Process: Save Error Symbols " + errorSymbolList.Count);
                CSVExporter.SaveErrorSymbols(errorSymbolList, ERRORSymbolsPath + "/Symbol.csv");

                if (!isHistorical)
                {
                    String fileName = HistoricalDataFilePath + "/DailyAllErrorSymbols.csv";
                    log.Info("Process: Write To CSV if historical data for error symbols ");
                    CSVExporter.WriteToCSV(listInputDataForSymbols, fileName);
                    log.Info("Process: Write Data CSV To DB ");
                    SymbolHistoricalDAO.SaveHistoricalDataCSVToDB(fileName,Constants.SymbolHistoricalTble);

                }
            }
            catch (Exception ex)
            {

                log.Error("Error: "+ex);
            }

           
        }
    }
}
