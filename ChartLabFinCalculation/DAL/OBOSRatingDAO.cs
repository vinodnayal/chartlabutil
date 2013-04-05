using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;

namespace ChartLabFinCalculation
{
    class OBOSRatingDAO
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(OBOSRatingDAO));

        internal static List<string> GetSnPSymbols()
        {
            List<string> SymbolList = new List<string>();
            //Now we will create a connection


            OdbcConnection MyConnection = new OdbcConnection(Constants.MyConString);
            MyConnection.Open();

            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            //Now we will create a command
            OdbcCommand com = new OdbcCommand("SELECT distinct symbol FROM indicesSymbolsMapping where indexSymbolId=1", con);

            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {

                    SymbolList.Add(dr.GetString(0));
                }
                dr.Close();
               
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

            return SymbolList;
        }

        public static void OBOSPercentage(string foldername, bool deletePreviousData)
        {


            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            OdbcCommand deleteCommand = new OdbcCommand("DELETE from dailyIndicesObOsPerAnalysis", con);
            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE" + " '" + foldername + "/OBOSCount.csv' " +
                                                "INTO TABLE dailyIndicesObOsPerAnalysis " +
                                                "FIELDS TERMINATED BY ',' " +
                                                "LINES TERMINATED BY '\n' " +
                                                "(date,oBPer,oSPer,obCount,osCount);", con);

            try
            {
                con.Open();
                if (deletePreviousData)
                {
                    deleteCommand.ExecuteNonQuery();
                }
                insertCommand.ExecuteReader();
                log.Info("OBOS Percentage Saved....");
              
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

    }
}
