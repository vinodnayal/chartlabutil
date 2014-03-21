using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using FinLib;

namespace ChartLabFinCalculation
{
    class SymbolHistoricalDAO
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(SymbolHistoricalDAO));

        public static List<BarData> GetDataFromFeedFromDB(DateTime from, DateTime to, string symbol)
        {

            List<BarData> barData = new List<BarData>();
            log.Info("\n\n\n\n\n Getting Data from DB ");

            try
            {
                OdbcConnection con = new OdbcConnection(Constants.MyConString);

                OdbcCommand com = new OdbcCommand("SELECT  open,high,low,close,date,volume from  symbolshistorical where symbol = '" + symbol + "'" + "order by date asc", con);
                try
                {
                    con.Open();
                    OdbcDataReader dr = com.ExecuteReader();

                    while (dr.Read())
                    {
                        BarData bar = new BarData()
                        {
                            open = float.Parse(dr.GetString(0)),
                            high = float.Parse(dr.GetString(1)),
                            low = float.Parse(dr.GetString(2)),
                            close = float.Parse(dr.GetString(3)),
                            date = DateTime.Parse(dr.GetString(4)),
                            volume = double.Parse(dr.GetString(5))
                        };
                        barData.Add(bar);
                    }
                    dr.Close();


                }
                catch (OdbcException ex)
                {
                    throw ex;
                }
                finally
                {
                    if (con != null)
                        con.Close();
                }
                return barData;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return barData;

        }


        public static void DeleteData(String symbol,String tableName)
        {
            OdbcConnection con = null;
            try
            {
                con = new OdbcConnection(Constants.MyConString);
                OdbcCommand com = new OdbcCommand("Delete from  " + tableName + " where symbol = '" + symbol + "'", con);

                con.Open();
                com.ExecuteNonQuery();


            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (con != null)
                    con.Close();
            }
        }

        internal static void DeleteData()
        {
            OdbcConnection con = null;
            try
            {
                con = new OdbcConnection(Constants.MyConString);
                OdbcCommand com = new OdbcCommand("Delete from  symbolshistorical", con);

                con.Open();
                com.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (con != null)
                    con.Close();
            }
        }
        internal static List<string> GetIndexMFFromDB()
        {
            List<string> lstSymbols = new List<string>();


            OdbcConnection MyConnection = new OdbcConnection(Constants.MyConString);
            MyConnection.Open();

            OdbcConnection con = new OdbcConnection(Constants.MyConString);


            OdbcCommand com = new OdbcCommand("SELECT DISTINCT yahooSymbol AS symbol FROM indices WHERE isduplicate IS NULL  UNION SELECT DISTINCT mutualFundName FROM mutualFund  WHERE isduplicate IS NULL ", con);

            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    lstSymbols.Add(dr.GetString(0));
                }
                dr.Close();

            }
            catch (OdbcException ex)
            {
                throw ex;
            }
            finally
            {
                if (con != null)
                    con.Close();
            }
            return lstSymbols;
        }


        internal static List<string> GetsymbolListFromDB()
        {
            return GetsymbolListFromDB(1, Constants.MAX_ID_DB);

        }

        internal static List<string> GetsymbolListFromDB(int from, int to)
        {
            List<string> lstSymbols = new List<string>();
            //Now we will create a connection


            OdbcConnection MyConnection = new OdbcConnection(Constants.MyConString);
            MyConnection.Open();

            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            //Now we will create a command
            OdbcCommand com = new OdbcCommand("SELECT distinct symbol FROM equitiesFundamental where (id BETWEEN " + from + " AND " + to + ") AND isnew=0 AND isactive =1 AND symbol NOT IN (SELECT symbol FROM invalid_symbols) order by symbol", con);
            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    lstSymbols.Add(dr.GetString(0));
                }
                dr.Close();

            }
            catch (OdbcException ex)
            {
                throw ex;
            }
            finally
            {
                if (con != null)
                    con.Close();
            }
            return lstSymbols;
        }

        internal static List<string> GetsymbolListForNewlyAddedSymbols()
        {
            List<string> lstSymbols = new List<string>();
            //Now we will create a connection


            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            //Now we will create a command
            // OdbcCommand com = new OdbcCommand("SELECT distinct symbol FROM equitiesFundamental where symbol='ZION'", con);
            OdbcCommand com = new OdbcCommand("SELECT distinct symbol FROM equitiesFundamental where isnew=1", con);
            // OdbcCommand com = new OdbcCommand("call newSymbols", con);
            // OdbcCommand com = new OdbcCommand("SELECT distinct symbol FROM equitiesFundamental where symbol in( 'WYN', 'NWSA', 'XRX', 'XRAY', 'WHR', 'YUM', 'WM') ", con);

            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    lstSymbols.Add(dr.GetString(0));
                }
                dr.Close();
                
            }
            catch (OdbcException ex)
            {
                throw ex;
            }
            finally
            {
                if (con != null)
                    con.Close();
            }

            return lstSymbols;
        }

        public static void SaveHistoricalDataCSVToDB(string filename, String tableName)
        {
            SaveHistoricalDataCSVToDB(filename, null, tableName);
        }

        public static void SaveHistoricalDataCSVToDB(string filename, string symbol,String tableName)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);


            OdbcCommand deleteCommand = new OdbcCommand("DELETE from " + tableName + " where symbol='" + symbol + "'", con);
            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE '" + filename + "' " +
                                                "INTO TABLE "+tableName +
                                                " FIELDS TERMINATED BY ',' " +
                                                "LINES TERMINATED BY '\n' " +
                                                "(`symbol`,open,high,low,close,actualclose,date,volume);", con);

            try
            {
                con.Open();
                if (symbol != null)
                {
                    deleteCommand.ExecuteReader();
                }
                insertCommand.ExecuteReader();
                log.Info("Historical File " + filename + " Saved....");

            }
            catch (OdbcException ex)
            {
                throw ex;
            }
            finally
            {
                if (con != null)
                    con.Close();
            }
        }

        public static void ChangeIsnewSymbols()
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            OdbcCommand updateCommand = new OdbcCommand("UPDATE equitiesFundamental SET isnew=0", con);

            try
            {
                con.Open();
                updateCommand.ExecuteReader();
                log.Info("EquitiesFundamental Table Updated...");

               
            }
            catch (OdbcException ex)
            {
                log.Error(ex);
            }
            finally
            {
                if (con != null)
                    con.Close();
            }
        }

        internal static bool CheckForDividendHistory(string symbol, DateTime currentdividendDate, DateTime todaysDate)
        {

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand searchCommand = new OdbcCommand("SELECT * FROM dividendhistory WHERE symbol='" + symbol + "'", con);
            bool shouldUpdateHistoricalData = false;

            try
            {
                con.Open();
                int dividendrowpresent = searchCommand.ExecuteNonQuery();
                if (dividendrowpresent > 0)
                {
                    OdbcCommand searchForDateCommand = new OdbcCommand("SELECT * FROM dividendhistory WHERE symbol='" + symbol + "' AND '" + currentdividendDate.Date.ToString("yyyy-MM-dd") + "'>=historicalupdatedate", con);
                    int dividendNotUpdated = searchForDateCommand.ExecuteNonQuery();
                    if (dividendNotUpdated > 0)
                    {
                        shouldUpdateHistoricalData = true;
                        DeleteDividendRow(symbol);
                    }
                    else
                    {
                        shouldUpdateHistoricalData = false;
                    }
                }
                else
                {
                    shouldUpdateHistoricalData = true;
                }

               
            }
            catch (OdbcException ex)
            {
                log.Error(ex);
            }
            finally
            {
                if (con != null)
                    con.Close();
            }

            return shouldUpdateHistoricalData;
        }

        internal static void DeleteDividendRow(string symbol)
        {

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand deleteCommand = new OdbcCommand("DELETE FROM dividendhistory WHERE symbol='" + symbol + "'", con);


            try
            {
                con.Open();
                deleteCommand.ExecuteNonQuery();
               
            }
            catch (OdbcException ex)
            {
                log.Error(ex);
            }
            finally
            {
                if (con != null)
                    con.Close();
            }

        }

        internal static void InsertDividendRow(string symbol, DateTime currentdividendDate, DateTime todaysDate)
        {

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand insertCommand = new OdbcCommand("INSERT INTO dividendhistory (symbol,dividenddate,historicalupdatedate) VALUES ('" + symbol + "','" + currentdividendDate.Date.ToString("yyyy-MM-dd") + "','" + todaysDate.Date.ToString("yyyy-MM-dd") + "')", con);


            try
            {
                con.Open();
                insertCommand.ExecuteNonQuery();
                
            }
            catch (OdbcException ex)
            {
                log.Error(ex);
            }
            finally
            {
                if (con != null)
                    con.Close();
            }

        }
    }
}
