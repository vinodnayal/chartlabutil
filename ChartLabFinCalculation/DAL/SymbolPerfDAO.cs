using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;

namespace ChartLabFinCalculation
{
    class SymbolPerfDAO
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(SymbolPerfDAO));

        internal static Dictionary<string, double> GetSymbolPrice()
        {
            Dictionary<string, double> symbolPriceList = new Dictionary<string, double>();

            try
            {

                OdbcConnection con = new OdbcConnection(Constants.MyConString);

                OdbcCommand com = new OdbcCommand("SELECT DISTINCT symbol, CLOSE,date FROM symbolshistorical WHERE DATE=(SELECT DATE FROM historicaldates WHERE DateType='" + Constants.P + "')", con);


                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    string symbol = dr.GetString(0);
                    double price = Double.Parse(dr.GetString(1));
                    DateTime date = DateTime.Parse(dr.GetString(2));

                    if (!symbolPriceList.ContainsKey(symbol))
                    {
                        symbolPriceList.Add(symbol, price);
                    }
                    else
                    {
                        log.Warn("Duplicate data for symbol '" + symbol + "' in date " + date);
                    }

                }
                dr.Close();
                con.Close();

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return symbolPriceList;
        }

        internal static void InsertSymbolPrice(string symbol, double price)
        {

            try
            {

                OdbcConnection con = new OdbcConnection(Constants.MyConString);

                OdbcCommand com = new OdbcCommand("INSERT INTO symbolperformance (symbol,Previousdayprice) values('" + symbol + "'," + price + ")", con);


                con.Open();
                com.ExecuteNonQuery();
                

                con.Close();

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

        }

        internal static void DeleteSymbolPerformance()
        {
            try
            {

                OdbcConnection con = new OdbcConnection(Constants.MyConString);

                OdbcCommand com = new OdbcCommand("DELETE from symbolperformance", con);


                con.Open();
                com.ExecuteNonQuery();
                log.Info("\nData deleted from symbolperformance table....... \n");

                con.Close();

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }





        internal static void UpdateSymbolPerformance()
        {


            try
            {

                OdbcConnection con = new OdbcConnection(Constants.MyConString);

                OdbcCommand com = new OdbcCommand("UPDATE symbolperformance temp1, "+
                                                "(SELECT p.symbol,(p.Previousdayprice-a.YTD)*100/a.YTD AS ytd,(p.Previousdayprice-a.WTD)*100/a.WTD AS wtd,(p.Previousdayprice-a.MTD)*100/a.MTD AS mtd,(p.Previousdayprice-a.QTD)*100/a.QTD AS qtd " +
                                                "FROM symbolperformance AS p JOIN symbolanalytics AS a  "+
                                                "ON p.symbol=a.symbol) temp2 "+
                                                "SET temp1.ytdpct=temp2.ytd, temp1.wtdpct=temp2.wtd, temp1.mtdpct=temp2.mtd,temp1.qtdpct=temp2.qtd " +
                                                "WHERE temp1.symbol=temp2.symbol", con);


                con.Open();
                com.ExecuteNonQuery();
                log.Info("\nData updated in symbolperformance table....... \n");

                con.Close();

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

        }
    }
}
