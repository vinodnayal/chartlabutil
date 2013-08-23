using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChartLabFinCalculation.DAL;
using System.IO;
using FinLib;

namespace ChartLabFinCalculation.BL
{
    class ETFSymbolsDataCalculation
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(ETFSymbolsDataCalculation));
        public static string ETFDataFilesPath { get; set; }
        public static void CalculateFTFSymbolsData()
        {
            try
            {
                List<string> etfSymbolList = ETFSymbolsDAO.getETFSymbolsList();
                foreach (string etfSymbol in etfSymbolList)
                {
                    importDataforSymbol(etfSymbol);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error in importing data for etf symbols ");
                log.Error(ex);
            }


        }

        public static void importDataforSymbol(string etfSymbol)
        {
            try
            {
                log.Info("Process:  ETF Rating Data importing for symbol "+ etfSymbol);
                List<Rating> RatingList = new List<Rating>();
                Dictionary<DateTime, CTRating> ctRatingList = new Dictionary<DateTime, CTRating>();
                String symbolDataPath = ETFDataFilesPath + "/" + etfSymbol;
                List<FileInfo> symbolCTRatingFiles = new List<FileInfo>();
                DirectoryInfo symbolDirectory = new DirectoryInfo(symbolDataPath);
                if (symbolDirectory.Exists)
                {
                    FileInfo[] fileEntries = symbolDirectory.GetFiles();
                    foreach (FileInfo file in fileEntries)
                    {
                        //symbolFilesDict.Add("price"
                        String fileType = file.Extension;
                        if (fileType.Equals(".NBY")) // import price data
                        {
                            //log.Info("Process: calculating Prices ");
                            // ImportPriceDataForSymbolsInDB(etfSymbol, file); // Price will be import from Yahooo
                        }
                        else if (fileType.Equals(".TR1")) //calculate buysell
                        {
                            log.Info("Process: calculating Ratings ");
                            RatingList = CalculateBuySellRatings(etfSymbol, file);
                        }
                        else if (fileType.Equals(".CT2") || fileType.Equals(".CT3")) //calculate CT rating
                        {
                            log.Info("Process: calculating CT Ratings " + fileType);
                            symbolCTRatingFiles.Add(file);
                        }


                    }
                }
                else
                {
                    log.Warn("Process: Data file directory does not exists.");
                
                }
                if (symbolCTRatingFiles.Count > 0)
                {

                    ctRatingList = calculateCTRatings(etfSymbol, symbolCTRatingFiles);
                }
                foreach (Rating bsRating in RatingList)
                {
                    if (ctRatingList.ContainsKey(bsRating.ratingDate))
                    {
                        CTRating ctRating = ctRatingList[bsRating.ratingDate];

                        bsRating.ctRatingValue = ctRating.ctRatingValue;
                        bsRating.ctRating = ctRating.ctRating;

                    }

                }
                log.Info("Process: Ratinngs Saving in DB of symbol " + etfSymbol);
                CSVExporter.WriteToCSVRating(RatingList, ETFDataFilesPath + "/RatingFile.csv");
                ETFSymbolsDAO.InsertRatingDataInDB(ETFDataFilesPath, "etfhistbsctrating", etfSymbol, true);
            }
            catch (Exception ex)
            {
                log.Error("Error in importing data for symbol " + etfSymbol);
                log.Error(ex);
            }
        }
        //calculate datwise ctrating from historical data for symbol 
        internal static Dictionary<DateTime, CTRating> calculateCTRatings(string etfSymbol, List<FileInfo> symbolCTRatingFiles)
        {
            Dictionary<DateTime, CTRating> ctRatingList = new Dictionary<DateTime, CTRating>();
            try
            {

                Dictionary<DateTime, double> S1_Dict = new Dictionary<DateTime, double>();
                Dictionary<DateTime, double> S2_Dict = new Dictionary<DateTime, double>();
                foreach (FileInfo file in symbolCTRatingFiles)
                {
                    if (file.Extension.Equals(".CT2") || file.Extension.Equals(".CT3"))
                    {
                        StreamReader readFile = new StreamReader(file.Directory + "\\" + file.Name);


                        string line;
                        string[] row;
                        while ((line = readFile.ReadLine()) != null)
                        {

                            row = line.Split(' ');
                            String dateString = row[0];
                            DateTime ratingDate = convertStringToDate(dateString);

                            double ctRating = 0;
                            if (file.Extension.Equals(".CT2"))
                            {
                                ctRating = double.Parse(row[1]) + double.Parse(row[1]) * 0.015;
                                S1_Dict.Add(ratingDate, ctRating);
                            }
                            else
                            {
                                ctRating = double.Parse(row[1]);
                                S2_Dict.Add(ratingDate, ctRating);
                            }

                        }

                    }
                }
                foreach (KeyValuePair<DateTime, double> pair in S1_Dict)
                {
                    if (S2_Dict.ContainsKey(pair.Key))
                    {

                        CTRating ctRatingObj = new CTRating();
                        ctRatingObj.ctRatingDate = pair.Key;
                        ctRatingObj.symbol = etfSymbol;
                        ctRatingObj.ctRatingValue = (pair.Value + S2_Dict[pair.Key]) * 100 / 2;
                        CTRatingEnum rating = CTRatingCalculation.calculateCTRatingEnum(ctRatingObj.ctRatingValue);
                        ctRatingObj.ctRating = (int)rating;
                        ctRatingList.Add(pair.Key, ctRatingObj);
                    }
                    else
                    {
                        log.Warn(pair.Key + " does not exists in CT2 File");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error in calculating CT Rating data for etf symbols " +etfSymbol);
                log.Error(ex);
            }
            return ctRatingList;
        }

        //calculate datwise rating from historical data for symbol 
        internal static List<Rating> CalculateBuySellRatings(string symbol, FileInfo file)
        {
            List<Rating> RatingList = new List<Rating>();
            try
            {
                StreamReader readFile = new StreamReader(file.Directory + "\\" + file.Name);

                string line;
                string[] row;
                // RatingList
                while ((line = readFile.ReadLine()) != null)
                {
                    row = line.Split(' ');
                    String dateString = row[0];
                    DateTime ratingDate = convertStringToDate(dateString);
                    double ratingValue = double.Parse(row[1]);
                    RatingEnum rating = BuySellRatingCalculation.calculateBSRatingEnum(ratingValue);

                    Rating ratingObj = new Rating();
                    ratingObj.symbol = symbol;
                    ratingObj.ratingValue = ratingValue;
                    ratingObj.ratingDate = ratingDate;
                    ratingObj.rating = (int)rating;
                    RatingList.Add(ratingObj);

                }
            }
            catch (Exception ex)
            {
                log.Error("Error in calculating BS Rating data for  symbols " + symbol);
                log.Error(ex);
            }
            return RatingList;
        }

        //import prices in symbolhistorical 
        private static void ImportPriceDataForSymbolsInDB(string etfSymbol, FileInfo file)
        {

            List<InputBarData> listInputDataForSymbols = new List<InputBarData>();
            List<BarData> barlist = new List<BarData>();
            try
            {
                StreamReader readFile = new StreamReader(file.Directory + "\\" + file);

                string line;
                string[] row;

                while ((line = readFile.ReadLine()) != null)
                {
                    BarData bar = new BarData();
                    row = line.Split(' ');
                    String dateString = row[0];
                    DateTime ratingDate = convertStringToDate(dateString);
                   
                        
                    bar.open = double.Parse(row[2]);
                    bar.high = double.Parse(row[3]);
                    bar.low = double.Parse(row[4]);
                    bar.close = double.Parse(row[5]);
                    bar.date = ratingDate;
                    bar.symbol = etfSymbol;
                    barlist.Add(bar);

                }

                InputBarData _symbolData = new InputBarData();
                _symbolData.symbol = etfSymbol;
                _symbolData.barListRow = barlist;
                listInputDataForSymbols.Add(_symbolData);
                String fileName = ETFDataFilesPath + "/" + etfSymbol + "/" + etfSymbol + ".csv";
                CSVExporter.WriteToCSV(listInputDataForSymbols, fileName);
                SymbolHistoricalDAO.SaveHistoricalDataCSVToDB(fileName, etfSymbol, Constants.SymbolHistoricalTble);
            }
            catch (Exception ex)
            {
                log.Error("Error in importing price data for  symbols " + etfSymbol);
                log.Error(ex);
            }

        }

        public static DateTime convertStringToDate(string dateString)
        {
        
                    DateTime ratingDate = new DateTime();
                    if (dateString.Length == 6)
                    {
                        ratingDate = new DateTime(Convert.ToInt32("19" + dateString.Substring(0, 2)), Convert.ToInt32(dateString.Substring(2, 2)), Convert.ToInt32(dateString.Substring(4, 2)));
                   
                    }
                    else if (dateString.Length == 7)
                    {
                        ratingDate = new DateTime(Convert.ToInt32("20" + dateString.Substring(1, 2)), Convert.ToInt32(dateString.Substring(3, 2)), Convert.ToInt32(dateString.Substring(5, 2)));
                   
                    }
                    return ratingDate;
        }

        //calculating ct rating change hist
        public static void CTRatingChangeHistory()
        {
            try{
            List<CTRatingHistory> historyBuySellRatingList = ETFSymbolsDAO.getETFCTRatingHistroyFromDB();

            List<BuySellRatingChangeHist> ChangeBuySellRatingHist = CTRatingCalculation.getCtRatingChangeList(historyBuySellRatingList);
            CSVExporter.WriteToCSVChangeRatingHistory(ChangeBuySellRatingHist, ETFDataFilesPath + "/ChangeCTRatingHistoryFile.csv");
            BuySellRatingDAO.InsertChangeCTRatingHistoryCSVToDB(ETFDataFilesPath,"etfctratingchangehist");
           
                }
            catch (Exception ex)
            {
                log.Error("Error in calculating CT Rating  change hist for ETFs ");
                log.Error(ex);
            }
        }
        //calculating bs rating change hist
        public static void BuySellRatingChangeHistory()
        {
            try{
                List<BuySellRating> historyBuySellRatingList = ETFSymbolsDAO.getETFBuySellRatingHistroyFromDB();
            List<BuySellRatingChangeHist> ChangeBuySellRatingHist = BuySellRatingCalculation.getBuySellRatingChangelist(historyBuySellRatingList);
            CSVExporter.WriteToCSVChangeRatingHistory(ChangeBuySellRatingHist, ETFDataFilesPath + "/ChangeRatingHistoryFile.csv");
            BuySellRatingDAO.InsertChangeRatingHistoryCSVToDB(ETFDataFilesPath,"etfbuysellratingchangehist");
            }
            catch (Exception ex)
            {
                log.Error("Error in calculating BS Rating  change hist for ETFs ");
                log.Error(ex);
            }
        }






        internal static void importPriceforSymbol(string Symbol)
        {
            try
            {
                DateTime toDate = DateTime.Now;
                int customHistDataLength = 15;
                int year = DateTime.Now.AddYears(-customHistDataLength).Date.Year;
                DateTime fromDate = new DateTime(year, 1, 1);

                log.Info("Process: Starting Historical Data Import for specific symbol " + Symbol);
                                    SymbolHistoricalDAO.DeleteData(Symbol, "symbolshistorical");
                                    HistoricalDataImporter.SaveHistDataSymbol(fromDate, toDate, Symbol, false, true, "symbolshistorical");
                                    log.Info("Process: Done! Historical Data Import for specific symbol " + Symbol);
            }
            catch (Exception ex)
            {
                log.Error("Error in importing price for ETF Symbol " + Symbol);
                log.Error(ex);
            }
        }

        internal static void importPriceforALLETFSymbol()
        {
            try
            {
                List<string> etfSymbolList = ETFSymbolsDAO.getETFSymbolsList();
                foreach (string etfSymbol in etfSymbolList)
                {
                    importPriceforSymbol(etfSymbol);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error in importing Prices data for etf symbols ");
                log.Error(ex);
            }
        }
    }
}
