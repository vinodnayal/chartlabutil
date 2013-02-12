using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using FinLib;

namespace ChartLabFinCalculation
{
    class SectorWiseSymbolsDAO
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(SectorWiseSymbolsDAO));


        public static List<int> GetSectorId()
        {
            List<int> sectorList = new List<int>();
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand("SELECT sectorId FROM sectors", con);



            try
            {
                con.Open();

                OdbcDataReader dr = com.ExecuteReader();
                log.Info("\nGetting Sector Id...\n");
                while (dr.Read())
                {

                    int symbolId = int.Parse(dr.GetString(0));
                    sectorList.Add(symbolId);


                }
                dr.Close();


                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }
            return sectorList;
        }


        public static List<SectorStrongWeakSymbol> GetStrongWeakSymbolBySector(int sectorId, int Indicator)
        {
            List<SectorStrongWeakSymbol> symbolList = new List<SectorStrongWeakSymbol>();
            string order;
            string sign;

            if (Indicator == 1)
            {
                order = "DESC";
                sign = ">";
            }
            else
            {
                order = "ASC";
                sign = "<=";
            }

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand("SELECT b.symbol, b.ratingvalue,T1.sectorId FROM temp_buySellRating AS b  JOIN " +
                                            "(SELECT e.symbol,e.sectorId FROM equitiesFundamental AS e JOIN indicesSymbolsMapping AS i " +
                                            "ON e.symbol=i.symbol " +
                                            "WHERE e.sectorId =" + sectorId + " AND i.indexSymbolId=1) T1 " +
                                            "ON T1.symbol=b.symbol " +
                                            "WHERE b.rating" + sign + "3 " +
                                            "ORDER BY b.ratingvalue " + order + " LIMIT 5", con);



            try
            {
                con.Open();

                OdbcDataReader dr = com.ExecuteReader();
                log.Info("\nGetting Strong/Weak Symbol List...\n");
                while (dr.Read())
                {
                    SectorStrongWeakSymbol strongWeakSymbolObj = new SectorStrongWeakSymbol();
                    strongWeakSymbolObj.Symbol = dr.GetString(0);
                    strongWeakSymbolObj.RatingValue = double.Parse(dr.GetString(1));
                    strongWeakSymbolObj.SectorId = int.Parse(dr.GetString(2));
                    strongWeakSymbolObj.StrongWeakIndicator = Indicator;

                    symbolList.Add(strongWeakSymbolObj);


                }
                dr.Close();


                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }

            return symbolList;
        }


        public static void SaveDailyStrongWeakSymbolsCSVToDB(string foldername)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            OdbcCommand deleteCommand = new OdbcCommand("DELETE from sectorstrngweaksymbols", con);
            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE '" + foldername + "/StrongWeakSymbolsFile.csv' " +
                                                "INTO TABLE sectorstrngweaksymbols " +
                                                "FIELDS TERMINATED BY ',' " +
                                                "LINES TERMINATED BY '\n' " +
                                                "(sectorId,symbol,strngweakid,ratingvalue);", con);



            try
            {
                con.Open();
                deleteCommand.ExecuteNonQuery();
                insertCommand.ExecuteReader();

                log.Info("\nSector Wise Strong Weak Symbol Saved....\n");
                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }
        }

    }
}
