using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;

namespace ChartLabFinCalculation.DAL
{

    class UsersDAO
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(UsersDAO));

        public static void updateTrialUsers()
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand updateCommand = new OdbcCommand("UPDATE users SET isundertrial = 0  WHERE DATEDIFF(CURDATE(),DATE)>14 AND isundertrial = 1", con);

            try
            {
                con.Open();
                updateCommand.ExecuteReader();
                log.Info("Process: update Trial Users\n");

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
