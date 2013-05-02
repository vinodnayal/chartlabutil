using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using System.Data;

namespace ChartLabFinCalculation.DAL
{
    class WeeklyReturnDAO
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(WeeklyReturnDAO));

        internal static void updateWeeklyReturns(String tableName)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            String sqlQuery = @"UPDATE " + tableName + " as t, (SELECT w.symbol, IFNULL((t1.close- t2.close)*100/t1.close,0) AS returnPct FROM " + tableName + " w "
                            +" LEFT JOIN "
                            +" (SELECT sh.symbol,sh.close FROM symbolshistorical  sh "
                            +" WHERE  sh.date = (SELECT DATE FROM historicaldates WHERE dateType='Weekly')) AS t1 ON t1.symbol=w.symbol "
                            +" LEFT JOIN "
                            +" (SELECT sh.symbol,sh.close FROM symbolshistorical  sh "
                            + " WHERE  sh.date = (SELECT MAX(DATE) FROM  symbolshistorical) ) AS t2 ON t2.symbol=w.symbol) AS t3"
                            + " SET t.returnPct= t3.returnPct WHERE t.symbol=t3.symbol";
            OdbcCommand updateCmd = new OdbcCommand(sqlQuery, con);
            try
            {

                    con.Open();

                    updateCmd.ExecuteNonQuery();
                
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



        internal static void updateSubSectorReturns()
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            try
            {
                
                OdbcCommand updateSubSectorReturnsCmd = con.CreateCommand();
                updateSubSectorReturnsCmd.CommandText = "{ CALL getsubsectorweelyreturn }";
                updateSubSectorReturnsCmd.CommandType = CommandType.StoredProcedure;
                con.Open();
                updateSubSectorReturnsCmd.ExecuteReader();
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

        internal static void updatesSNPSymbolsReturns()
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            try
            {
               
                OdbcCommand updateSubSectorReturnsCmd = con.CreateCommand();
                updateSubSectorReturnsCmd.CommandText = "{ CALL getsnpsymbolsweelyreturn }";
                updateSubSectorReturnsCmd.CommandType = CommandType.StoredProcedure;
                con.Open();
                updateSubSectorReturnsCmd.ExecuteReader();
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
