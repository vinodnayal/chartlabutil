using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using System.Configuration;
using System.Data;
using System.IO;
using FinLib;

namespace ChartLabFinCalculation
{
    public class CSVToDB
    {
      
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(CSVToDB));

      

        public static void SaveHistoricalData(string filename, List<string> symbols)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);

           
            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE '" + filename + "' " +
                                                "INTO TABLE symbolshistorical " +
                                                "FIELDS TERMINATED BY ',' " +
                                                "LINES TERMINATED BY '\n' " +
                                                "(`symbol`,open,high,low,close,actualclose,date,volume);", con);

           

            try
            {
                con.Open();
                foreach (String symbol in symbols)
                {
                    OdbcCommand deleteCommand = new OdbcCommand("DELETE from symbolshistorical where symbol ='"+symbol+"'", con);
                    deleteCommand.ExecuteNonQuery();
                }
                insertCommand.ExecuteReader();
                log.Info("Historical File " + filename + " Saved....");

                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }
        }

        public static void UpdatedTrends(string foldername, bool deletePreviousData,string filename)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);

            OdbcCommand deleteCommand = new OdbcCommand("DELETE from updatedTrends", con);
            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE" + " '"+foldername+"/" +filename+
                                                "' INTO TABLE updatedTrends " +
                                                "FIELDS TERMINATED BY ',' " +
                                                "LINES TERMINATED BY '\n' " +
                                                "(symbol,shortTermTrend,shortTermTrendDate,mediumTermTrend,longTermTrend);", con);


            OdbcCommand updateCommand = new OdbcCommand("UPDATE symbolAnalytics ,updatedTrends"+
                                                   " SET symbolAnalytics.shortTerm =updatedTrends.shortTermTrend,"+
                                                   "symbolAnalytics.mediumTerm =updatedTrends.mediumTermTrend," +
                                                   "symbolAnalytics.longTerm =updatedTrends.longTermTrend," +
                                                   "symbolAnalytics.shortTermTrendDate=updatedTrends.shortTermTrendDate "+
                                                    "WHERE symbolAnalytics.symbol=updatedTrends.symbol ;",con);
            try
            {
                con.Open();
                if (deletePreviousData)
                {
                    deleteCommand.ExecuteNonQuery();
                    deletePreviousData = false;
                }
                insertCommand.ExecuteReader();
                updateCommand.ExecuteReader();
                log.Info("Trends Updated....");
                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }
        }
      
        public static void IndusrtySymbolFileToDb(string foldername)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand deleteCommand = new OdbcCommand("DELETE from industryTopSymbols", con);
            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE" + " '" + foldername + "/industries.csv' " +
                                                "INTO TABLE industryTopSymbols " +
                                                "FIELDS TERMINATED BY ',' " +
                                                "LINES TERMINATED BY '\n' " +
                                                "(symbol,industryId);", con);

          
            try
            {
                con.Open();

                deleteCommand.ExecuteNonQuery();
                insertCommand.ExecuteReader();

                log.Info("Symbol Industry Updated....");
                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }
        }

        public static void ImportUsersToDB(string foldername)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE" + " '" + foldername + "/users.csv' " +
                                                "INTO TABLE users " +
                                                "FIELDS TERMINATED BY ',' " +
                                                "LINES TERMINATED BY '\n' " +
                                                "(username,password,firstName,lastName,emailAddress);", con);


            try
            {
                con.Open();

                insertCommand.ExecuteReader();

                log.Info("Users Added....");
                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }
        }

        


        public static void UpdateSentimentIndicatorPerf(string foldername)
        {
           OdbcConnection con = new OdbcConnection(Constants.MyConString);

            OdbcCommand deleteCommand = new OdbcCommand("DELETE from sentimenthistoricalperf", con);
            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE '" + foldername + "/SentimentIndicatorPerfFile.csv' " +
                                                "INTO TABLE sentimenthistoricalperf " +
                                                "FIELDS TERMINATED BY ',' " +
                                                "LINES TERMINATED BY '\n' " +
                                                "(symbolid,weeklyEffect,oneMonthEffect,twoMonthEffect,indicatorType);", con);



            try
            {
                con.Open();

                deleteCommand.ExecuteNonQuery();
                insertCommand.ExecuteReader();
                log.Info("\n Sentiment Indicator Performance Updated....");

                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }
        }

        public static void InsertLongShortPerf(string foldername)
        {
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand deleteCommand = new OdbcCommand("DELETE from longshortalertsperf", con);

            OdbcCommand insertCommand = new OdbcCommand("LOAD DATA LOCAL INFILE '" + foldername + "/LongShortPerf.csv' " +
                                                                    "INTO TABLE longshortalertsperf " +
                                                                    "FIELDS TERMINATED BY ',' " +
                                                                    "LINES TERMINATED BY '\n' " +
                                                                    "(yesterdayPerf,5dayperf,longshortid);", con);



            try
            {
                con.Open();

                deleteCommand.ExecuteNonQuery();
                insertCommand.ExecuteReader();
                log.Info("\nLong Short Alert Performance Saved....\n");

                con.Close();
            }
            catch (OdbcException ex)
            {
                throw ex;
            }
        }

        
    }
}
