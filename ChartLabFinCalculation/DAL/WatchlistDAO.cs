using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;

namespace ChartLabFinCalculation
{
    class WatchlistDAO
    {

        internal static List<DateTime>  GetUniqueCreateDates()
        {
            List<DateTime> dateList = new List<DateTime>();

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand("SELECT DISTINCT createddate FROM watchlist", con);
           
           

            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    DateTime date = DateTime.MinValue;
                    DateTime.TryParse(dr.GetString(0), out date);
                    if (date.Date.ToString("yyyy-MM-dd") != Constants.DEFAULT_DATE_FORMAT)
                    {
                        dateList.Add(date);
                    }
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

            // string mycon = System.Configuration.ConfigurationSettings.AppSettings.["sqlcon"].ConnectionString;
            return dateList;
        }
    }
}
