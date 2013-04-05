using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;

namespace ChartLabFinCalculation
{
    class DataImportLocalDAO
    {

        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(DataImportLocalDAO));
        internal static void SymbolsHistoirical()
        {
            //Delete

            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            OdbcCommand deleteCommand = new OdbcCommand("DELETE from symbolshistorical", con);

            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE  'D:/HistFile/symbolshist.csv' " +
                                              "INTO TABLE symbolshistorical " +
                                              "FIELDS TERMINATED BY ',' " +
                                              "LINES TERMINATED BY '\n' " +
                                              "(open,high,low,close,@date,volume,actualclose) SET date = DATE_FORMAT(STR_TO_DATE(@DATE, '%m/%d/%Y'), '%Y-%m-%d');", con);


            try
            {
                con.Open();
                deleteCommand.ExecuteReader();
                insertCommand.ExecuteReader();
                log.Info("Historical BuySell Change rating File  Saved....");

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
