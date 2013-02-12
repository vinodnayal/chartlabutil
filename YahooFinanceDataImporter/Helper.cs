using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using System.Data;
using FinLib;


namespace ConsoleApplication1
{
    class Helper
    {

        //static log4net.ILog log = log4net.LogManager.GetLogger(typeof(Helper));
        static string MyConString = "DRIVER={MySQL ODBC 5.1 Driver};" +
                      "SERVER=192.168.1.20;" +
                      "DATABASE=securityMaster;" +
                      "UID=root;" +
                      "PASSWORD=root;" +
                      "OPTION=3 ";
         public static List<string> GetSymbols()
        {
            List<string> lstSymbols = new List<string>();
            //Now we will create a connection
           

            OdbcConnection MyConnection = new OdbcConnection(MyConString);
            MyConnection.Open();

            OdbcConnection con = new OdbcConnection(MyConString);
     
            //Now we will create a command
            OdbcCommand com = new OdbcCommand("SELECT distinct symbol FROM equitiesFundamental", con);

            try
            {
                  con.Open();
                  OdbcDataReader dr = com.ExecuteReader();
                
                  while(dr.Read())
                  {
                      lstSymbols.Add(dr.GetString(0));                        
                  }
                  dr.Close();
                  con.Close();
            } 
            catch(OdbcException ex)
            {
                Console.WriteLine("ERROR \n" + "============ \n" + ex.ToString());
            }
            
            return lstSymbols;
        }

         public static List<string> GetIndicesSymbols()
         {
             List<string> lstSymbols = new List<string>();


             OdbcConnection MyConnection = new OdbcConnection(MyConString);
             MyConnection.Open();

             OdbcConnection con = new OdbcConnection(MyConString);

             //Now we will create a command
             OdbcCommand com = new OdbcCommand("SELECT distinct indexSymbol FROM indices", con);

             try
             {
                 con.Open();
                 OdbcDataReader dr = com.ExecuteReader();

                 while (dr.Read())
                 {
                     lstSymbols.Add(dr.GetString(0));
                 }
                 dr.Close();
                 con.Close();
             }
             catch (OdbcException ex)
             {
                 Console.WriteLine("ERROR \n" + "============ \n" + ex.ToString());
             }

             return lstSymbols;
         }
         public static List<BarData> GetSymbolDataRange(String symbol,DateTime fromDate,DateTime toDate)
         {
             List<BarData> qdList = new List<BarData>();


             OdbcConnection MyConnection = new OdbcConnection(MyConString);
             MyConnection.Open();

             OdbcConnection con = new OdbcConnection(MyConString);

             //Now we will create a command
             OdbcCommand com = new OdbcCommand("SELECT OPEN,high,low,CLOSE,volume,DATE  FROM symbolsHistorical WHERE symbol=? AND DATE BETWEEN ? AND ? order by date", con);
             com.Parameters.Add("@symbol", symbol);
             com.Parameters.Add("@fromDate", fromDate);
             com.Parameters.Add("@toDate", toDate);
             try
             {
                 con.Open();
                 OdbcDataReader dr = com.ExecuteReader();
                 
                 while (dr.Read())
                 {
                     BarData qd = new BarData();
                     qd.open = double.Parse(dr.GetString(0));
                     qd.high = double.Parse(dr.GetString(1));
                     qd.low = double.Parse(dr.GetString(2));
                     qd.close = double.Parse(dr.GetString(3));
                     qd.volume = Int64.Parse(dr.GetString(4));
                     qd.date = DateTime.Parse(dr.GetString(5));
                     qdList.Add(qd);
                 }
                 dr.Close();
                 con.Close();
             }
             catch (OdbcException ex)
             {
                 Console.WriteLine("ERROR \n" + "============ \n" + ex.ToString());
             }

             return qdList;
         }
    }
}
