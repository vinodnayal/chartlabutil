using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;

namespace ChartLabFinCalculation
{
    class AggresiveDefensivePerformanceDAO
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(AggresiveDefensivePerformanceDAO));

        public static void UpdatePerformance()
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);


            OdbcCommand updateAgrsvPerfComm = new OdbcCommand("CALL aggresivePerformance", con);

            OdbcCommand updateDefncvPerfComm = new OdbcCommand("CALL defensivePerformance", con);

            try
            {

                con.Open();

                updateAgrsvPerfComm.ExecuteNonQuery();
                log.Info("\nUpdated Aggresive Performance...\n");
                updateDefncvPerfComm.ExecuteNonQuery();
                log.Info("\nUpdated Defensive Performance...\n");

                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }
        }
    }
}
