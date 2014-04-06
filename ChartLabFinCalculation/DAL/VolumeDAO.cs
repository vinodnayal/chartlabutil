using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using FinLib;

namespace ChartLabFinCalculation
{
    class VolumeDAO
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(VolumeDAO));

        public static void SaveDailyAvgVolumeData(string foldername)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand deleteCommand = new OdbcCommand("DELETE from volume", con);

            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE '" + foldername + "/DailyAvgVolume.csv' " +
                                                                    "INTO TABLE volume " +
                                                                    "FIELDS TERMINATED BY ',' " +
                                                                    "LINES TERMINATED BY '\n' " +
                                                                    "(`symbol`,avgvolume);", con);



            try
            {
                con.Open();

                deleteCommand.ExecuteNonQuery();
                insertCommand.ExecuteReader();
                log.Info("Daily Average Volume File Saved....");


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

        public static void UpdateVolumeTable()
        {

            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            String sql_update = "update volume , symbol_live_data set volume.avgvolume=symbol_live_data.volume where volume.symbol= symbol_live_data.symbol";
            OdbcCommand updateCommand1 = new OdbcCommand(sql_update,con);

            OdbcCommand updateCommand = new OdbcCommand("UPDATE volume v, " +
                                                        "(SELECT FORMAT((T2.close-T1.close)*100/T1.close,2) AS PctPriceChg,T1.symbol,T2.volume FROM " +
                                                        "(SELECT * FROM symbolshistorical WHERE `date`=( SELECT `Date` FROM historicaldates WHERE DateType='" + Constants.D_2 + "'))  T1, " +
                                                        "(SELECT * FROM symbolshistorical WHERE `date`=( SELECT `Date` FROM historicaldates WHERE DateType='" + Constants.P + "') ) T2 " +
                                                        "WHERE T1.symbol=T2.symbol) T3 " +
                                                        "SET v.previousvolume=T3.volume,v.percentagechange=(T3.volume-v.avgvolume)*100/T3.volume,v.volumealertid= CASE WHEN (((T3.volume-v.avgvolume)*100/T3.volume)>45 AND (T3.PctPriceChg>0)) THEN 1 WHEN (((T3.volume-v.avgvolume)*100/T3.volume)>45 AND (T3.PctPriceChg<0)) THEN -1 ELSE 0 END " +
                                                        "WHERE v.symbol=T3.symbol", con);





            try
            {
                con.Open();
                updateCommand1.ExecuteReader();
                updateCommand.ExecuteReader();
                log.Info("Volume Table Updated...");

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


        public static void InsertDailyVolumeAlert()
        {


            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            OdbcCommand insertDailyVolAlertCommand = new OdbcCommand("INSERT INTO historicalvolumealerts (symbol,changedate,volume,pctchange,volumealerttype,avgvolume) " +
                                   "SELECT symbol,( SELECT `Date` FROM historicaldates WHERE DateType='" + Constants.P + "'),previousvolume,percentagechange,volumealertid,avgvolume FROM volume WHERE percentagechange>45", con);

            try
            {
                con.Open();

                insertDailyVolAlertCommand.ExecuteReader();
                log.Info("Volume Alerts Table Updated...");


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

        public static List<VolumeHistoryList> GetDataForAlertFromDB(string symbol)
        {

            List<VolumeHistoryList> DateVolumeList = new List<VolumeHistoryList>();
            log.Info("\n\n\n\n\n Getting Data from DB for symbol " + symbol);
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            try
            {


                OdbcCommand com = new OdbcCommand("SELECT DISTINCT date,close,volume from  symbolshistorical where symbol = '" + symbol + "' order by date desc", con);
                //OdbcCommand com = new OdbcCommand("SELECT date,close,volume from  symbolshistorical where symbol = 'SPY' order by date desc", con);
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    VolumeHistoryList volumeDateObj = new VolumeHistoryList();
                    volumeDateObj.ChangeDate = DateTime.Parse(dr.GetString(0));
                    volumeDateObj.Close = Double.Parse(dr.GetString(1));
                    volumeDateObj.Volume = Double.Parse(dr.GetString(2));

                    DateVolumeList.Add(volumeDateObj);

                }
                dr.Close();


            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (con != null)
                    con.Close();
            }
            return DateVolumeList;

        }


        public static void DeleteFromHistoricalVolAlerts()
        {

            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            OdbcCommand deleteCommand = new OdbcCommand("DELETE from historicalvolumealerts", con);

            try
            {
                con.Open();

                deleteCommand.ExecuteNonQuery();
                log.Info("\n Data from historicalvolumealerts table deleted....");

                con.Close();
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


        public static void SaveHistoricalVolumeAlertData(string foldername)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);


            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE '" + foldername + "/HistoricalVolume.csv' " +
                                                "INTO TABLE historicalvolumealerts " +
                                                "FIELDS TERMINATED BY ',' " +
                                                "LINES TERMINATED BY '\n' " +
                                                "(symbol,changedate,volume,pctchange,volumealerttype,avgvolume);", con);

            try
            {
                con.Open();

                insertCommand.ExecuteReader();

                log.Info("\n Historical Volume Alert Saved....");
                con.Close();
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

        public static void InsertHistoricalVolumeAlertPerf(string foldername)
        {

            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            OdbcCommand deleteCommand = new OdbcCommand("DELETE from volalertprmfnce", con);
            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE '" + foldername + "/HistoricalVolumePerformance.csv' " +
                                                "INTO TABLE volalertprmfnce " +
                                                "FIELDS TERMINATED BY ',' " +
                                                "LINES TERMINATED BY '\n' " +
                                                "(`symbol`,2days,5days,30days,90days);", con);

            try
            {
                con.Open();

                deleteCommand.ExecuteNonQuery();
                insertCommand.ExecuteReader();
                log.Info("\n Historical Volume Alert Performance Updated....");

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
