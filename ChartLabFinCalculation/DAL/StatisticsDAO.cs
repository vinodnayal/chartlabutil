using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using FinLib.Model;

 namespace ChartLabFinCalculation.DAL
{
    public class StatisticsDAO
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(StatisticsDAO));
        internal static void InsertStatsDataCSVToDB(string StatisticsPath)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            OdbcCommand deleteCommand = new OdbcCommand("DELETE from statsdata", con);

            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE" + " '" + StatisticsPath + "/StatisticsData.csv' " +
                                              "INTO TABLE statsdata " +
                                              "FIELDS TERMINATED BY ',' " +
                                              "LINES TERMINATED BY '\n' " +
                                              "(symbol,buy_price,buy_date,sell_price,sell_date,stat_id,stat_prfmnce,stat_duration);", con);


            try
            {
                con.Open();
                deleteCommand.ExecuteReader();
                insertCommand.ExecuteReader();
                log.Info("Statistics Data File Saved....");

                con.Close();
            }
            catch (OdbcException ex)
            {
                log.Error("ERROR \n" + "============ \n" + ex.ToString());
            }
        }

        internal static void insertStatsAvgReturn()
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            OdbcCommand calRatingStatsPerfCom = new OdbcCommand("CALL calRatingStatsPerf", con);

            try
            {
                con.Open();
                calRatingStatsPerfCom.ExecuteNonQuery();
                log.Info("statsperformance inserted....");

                con.Close();
            }
            catch (OdbcException ex)
            {
                log.Error("ERROR \n" + "============ \n" + ex.ToString());
            }
        }

       
        internal static Dictionary<int, TempState> getPositivetimeData()
        {
            Dictionary<int, TempState> PositiveTimeDict = new Dictionary<int, TempState>();
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand("SELECT counts,stat_id,name_type FROM tempstats", con);
            try
            {
                con.Open();
                OdbcDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                  int  daysCount = Convert.ToInt32(dr.GetString(0));
                  int statId = Convert.ToInt32(dr.GetString(1));
                    if(!PositiveTimeDict.ContainsKey(Convert.ToInt32(dr.GetString(1))))
                    {
                    if (dr.GetString(2).Equals("All"))
                    {
                        PositiveTimeDict.Add(Convert.ToInt32(dr.GetString(1)),
                            new TempState
                            {
                                totalCount = daysCount,
                                
                            });

                    }
                    else {

                        PositiveTimeDict.Add(Convert.ToInt32(dr.GetString(1)),
                            new TempState
                            {
                                upDaysCount = daysCount
                               

                            });
                    }
                    }else
                    {
                        TempState stat = PositiveTimeDict[Convert.ToInt32(dr.GetString(1))];
                        if (dr.GetString(2).Equals("All"))
                        {
                            stat.totalCount = daysCount;
                        }
                        else
                        {
                            stat.upDaysCount = daysCount;
                        }
                    
                    }
                }
                dr.Close();
                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }

            return PositiveTimeDict;
        }

        internal static void updatePositiveTimePct(int statId, double upDaysPct)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            OdbcCommand upDateCommand = new OdbcCommand("update statsperformance set avgUp=" + upDaysPct + " where stat_id=" + statId, con);

           
            try
            {
                con.Open();
                upDateCommand.ExecuteReader();
                
                log.Info("Statistics positive time percentage updated....");

                con.Close();
            }
            catch (OdbcException ex)
            {
                log.Error("ERROR \n" + "============ \n" + ex.ToString());
            }
        }
    }
}
