using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using FinLib.Model;
using System.Data;

namespace ChartLabFinCalculation
{
    class SNPAnalyticsDAO
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(SNPAnalyticsDAO));
        /// <summary>
        ///  import price of SNP (^GSPC) for calculating return since inception of SNP with WL
        /// </summary>
        /// <param name="fileName"></param>
        public static void SnPSpecificDatePriceImport(string fileName)
        {

            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            OdbcCommand deleteCommand = new OdbcCommand("DELETE from snpuniquedateprice", con);

            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE  '" + fileName + "' " +
                                              "INTO TABLE snpuniquedateprice " +
                                              "FIELDS TERMINATED BY ',' " +
                                              "LINES TERMINATED BY '\n' " +
                                              "(date,snpprice);", con);


            try
            {
                con.Open();
                deleteCommand.ExecuteReader();
                insertCommand.ExecuteReader();
                log.Info("SNP price inserted for Watchlict CreateDate ....");


            }
            catch (OdbcException ex)
            {
                log.Error("ERROR \n" + "============ \n" + ex.ToString());
            }
            finally
            {
                if (con != null)
                    con.Close();
            }

        }

        /// <summary>
        /// insert Synnopsis id in DB
        /// </summary>
        /// <param name="snpDatafilesPath"></param>
        internal static void InsertSynopsisIDInDB(string snpDatafilesPath)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand deleteCommand = new OdbcCommand("DELETE from temp_snpsymbolsynopsis", con);
            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE" + " '" + snpDatafilesPath + "/snpAnalyticsFile.csv' " +
                                                "INTO TABLE temp_snpsymbolsynopsis " +
                                                "FIELDS TERMINATED BY ',' " +
                                                "LINES TERMINATED BY '\n' " +
                                                "(symbol,synopsisid);", con);

            OdbcCommand updateCommand = new OdbcCommand(@"UPDATE snpsymbolsanalytics s 
                                                            LEFT JOIN  temp_snpsymbolsynopsis t
                                                            ON s.symbol=t.symbol 
                                                            SET s.synopsisid= t.synopsisid ", con);
            try
            {
                con.Open();

                deleteCommand.ExecuteNonQuery();
                insertCommand.ExecuteNonQuery();
                updateCommand.ExecuteNonQuery();
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

        /// <summary>
        /// insert Synnopsis id in DB
        /// </summary>
        /// <param name="snpDatafilesPath"></param>
        internal static void InsertSNPSymbolAnalyticsInDB(string snpDatafilesPath)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand deleteCommand = new OdbcCommand("DELETE from snpsymbolsanalytics", con);
            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE" + " '" + snpDatafilesPath + "/snpProEdgeFile.csv' " +
                                                "INTO TABLE snpsymbolsanalytics " +
                                                "FIELDS TERMINATED BY ',' " +
                                                "LINES TERMINATED BY '\n' " +
                                                "(symbol,synopsisid,proedgeid,proedgedate);", con);

            try
            {
                con.Open();

                deleteCommand.ExecuteNonQuery();
                insertCommand.ExecuteNonQuery();
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

        //internal static SnpAnalytics getSnpSymbolsGainLossProb(SnpAnalytics snpAnalytics, string symbol, int wlId)
        //{
        //    OdbcConnection con = new OdbcConnection(Constants.MyConString);
        //    OdbcCommand selectCmd = con.CreateCommand();
        //    selectCmd.CommandText = "{call getsnpgainlossProb(?,?)}";
        //    selectCmd.CommandType = CommandType.StoredProcedure;
        //    selectCmd.Parameters.AddWithValue("@symbol", symbol);
        //    selectCmd.Parameters.AddWithValue("@wlId", wlId);
        //    //selectCmd.Parameters["@symbolparam"].Value = symbol;
        //    //selectCmd.Parameters["@watchlistIdparam"].Value = wlId;

        //     try
        //    {
        //    con.Open();

        //    OdbcDataReader dr = selectCmd.ExecuteReader();

        //        while (dr.Read())
        //        {
        //            snpAnalytics.riskPct = Convert.ToDouble(dr.GetString(0));
        //            snpAnalytics.confidencePct = Convert.ToDouble(dr.GetString(1));
        //            snpAnalytics.gainPct = Convert.ToDouble(dr.GetString(2));
        //        }
        //         }
        //    catch (OdbcException ex)
        //    {
        //        throw ex;
        //    }
        //    finally
        //    {
        //        if (con != null)
        //            con.Close();
        //    }
        //     return snpAnalytics; 
        //}

        internal static List<SnpAnalytics> getProEdgeHistByDate(DateTime date)
        {
            List<SnpAnalytics> snpSymbolsAnalytics = new List<SnpAnalytics>();

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com;
            String sqlString = @"SELECT s.symbol, p.proedgeid,p.triggereddate,p.triggeredruleid FROM snpsymbols s
                                  LEFT JOIN proedgetriggeredhist p ON p.symbol= s.symbol where p.triggereddate=  '" + date.ToString("yyyy-MM-dd") + "'";
            com = new OdbcCommand(sqlString, con);

            try
            {
                con.Open();

                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    SnpAnalytics symbolAnalytics = new SnpAnalytics();
                    if (!Convert.IsDBNull(dr.GetValue(0)))
                        symbolAnalytics.symbol = dr.GetString(0);

                    if (!Convert.IsDBNull(dr.GetValue(1)))
                        symbolAnalytics.proEdgeId = dr.GetString(1);

                    if (!Convert.IsDBNull(dr.GetValue(2)))
                        symbolAnalytics.proEdgeTriggerDate = dr.GetDateTime(2);


                    if (!Convert.IsDBNull(dr.GetValue(3)))
                        symbolAnalytics.triggerRuleId = dr.GetInt32(3);


                    snpSymbolsAnalytics.Add(symbolAnalytics);

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

            return snpSymbolsAnalytics;
        }


        internal static void InsertProEdgeTriggerInDB(string snpDatafilesPath, DateTime date)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand deleteCommand = new OdbcCommand("DELETE from proedgetriggeredhist where triggereddate=  '" + date.ToString("yyyy-MM-dd") + "'", con);
            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE" + " '" + snpDatafilesPath + "/proEdgeFileHIst" + date.ToString("yyyyMMdd") + ".csv' " +
                                                "INTO TABLE proedgetriggeredhist " +
                                                "FIELDS TERMINATED BY ',' " +
                                                "LINES TERMINATED BY '\n' " +
                                                "(symbol,proedgeid,triggeredruleid,triggereddate);", con);

            try
            {
                con.Open();

                deleteCommand.ExecuteNonQuery();
                insertCommand.ExecuteNonQuery();
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

        internal static List<DateTime> getFinancialDayDatesFromDB(DateTime startDate)
        {
            
            List<DateTime> dateList = new List<DateTime>();
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com;

            com = new OdbcCommand("SELECT DISTINCT sh.ratingdate FROM historybuysellrating sh WHERE sh.symbol='AA' and sh.ratingdate > '" + startDate.ToString("yyyy-MM-dd") + "'  order by sh.ratingdate", con);

            try
            {
                con.Open();

                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    dateList.Add(DateTime.Parse(dr.GetString(0)));
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

            return dateList;
        }


        internal static Dictionary<String, SnpAnalytics> getCurSnpSymbolsAnalytics()
        {
            Dictionary<String, SnpAnalytics> snpSymbolsAnalytics = new Dictionary<string, SnpAnalytics>();

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com;
            String sqlString = @"SELECT s.symbol, p.proedgeid,p.proedgedate,sd.type,DATEDIFF(NOW(),proedgedate),p.synopsisid FROM snpsymbols s
LEFT JOIN snpsymbolsanalytics p ON p.symbol= s.symbol 
LEFT JOIN signal_details sd ON sd.proedgeid = p.proedgeid  ";
            com = new OdbcCommand(sqlString, con);

            try
            {
                con.Open();

                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    SnpAnalytics symbolAnalytics = new SnpAnalytics();
                    if (!Convert.IsDBNull(dr.GetValue(0)))
                        symbolAnalytics.symbol = dr.GetString(0);

                    if (!Convert.IsDBNull(dr.GetValue(1)))
                        symbolAnalytics.proEdgeId = dr.GetString(1);

                    if (!Convert.IsDBNull(dr.GetValue(2)))
                        symbolAnalytics.proEdgeTriggerDate = dr.GetDateTime(2);


                    if (!Convert.IsDBNull(dr.GetValue(3)))
                        symbolAnalytics.alertType = Convert.ToInt32(dr.GetValue(3));

                    if (!Convert.IsDBNull(dr.GetValue(4)))
                        symbolAnalytics.proEdgeTriggerDateDiff = Convert.ToInt32(dr.GetValue(4));
                   
                    if (!Convert.IsDBNull(dr.GetValue(5)))
                        symbolAnalytics.synopsisRuleId = dr.GetString(5);

                    if(!snpSymbolsAnalytics.ContainsKey(symbolAnalytics.symbol))
                        snpSymbolsAnalytics.Add(symbolAnalytics.symbol,symbolAnalytics);

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

            return snpSymbolsAnalytics;
        }
    }
}

