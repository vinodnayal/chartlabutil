using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;

namespace ChartLabFinCalculation
{
    class SnPPriceDAO
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(SnPPriceDAO));
        public static void SnPSpecificDatePriceImport(string fileName)
        {

            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            OdbcCommand deleteCommand = new OdbcCommand("DELETE from snpuniquedateprice", con);

            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE  '"+fileName+"' " +
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
    }
}
