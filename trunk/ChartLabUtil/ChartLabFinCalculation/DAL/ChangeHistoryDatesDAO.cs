using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinLib;
using System.Data.Odbc;

namespace ChartLabFinCalculation
{
    class ChangeHistoryDatesDAO
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(ChangeHistoryDatesDAO));

        public static void ChangeHistoryDate()
        {


            Dictionary<DateTime, DateTime> dateDict = new Dictionary<DateTime, DateTime>();
            DateTime oneMonth = new DateTime();
            DateTime five_days = new DateTime();
            DateTime startOfWeek = new DateTime();
            DateTime dbWeeklyDate = new DateTime();
            DateTime dbMonthlyDate = new DateTime();
            DateTime dbYearlyDate = new DateTime();
            DateTime dbQuaterlyDate = new DateTime();

            List<DateTime> dateList = new List<DateTime>();
            OdbcConnection con = new OdbcConnection(Constants.MyConString);
            OdbcCommand com = new OdbcCommand("SELECT DISTINCT DATE FROM symbolshistorical WHERE symbol='GOOG' ORDER BY DATE desc", con);
            OdbcCommand getWeeklyDate = new OdbcCommand("SELECT DATE from historicaldates where DateType='" + Constants.W + "'", con);
            OdbcCommand getMonthlyDate = new OdbcCommand("SELECT DATE from historicaldates where DateType='" + Constants.M + "'", con);
            OdbcCommand getYearlyDate = new OdbcCommand("SELECT DATE from historicaldates where DateType='" + Constants.Y + "'", con);
            OdbcCommand getQuaterlyDate = new OdbcCommand("SELECT DATE from historicaldates where DateType='" + Constants.Q + "'", con);


            try
            {
                con.Open();
                OdbcDataReader dr;
                dr = com.ExecuteReader();
                int count = 0;

                while (dr.Read())
                {
                    if (count < 3)
                    {
                        dateList.Add(DateTime.Parse(dr.GetString(0)));
                    }
                    dateDict.Add(DateTime.Parse(dr.GetString(0)), DateTime.Parse(dr.GetString(0)));
                    count++;
                }
                dr.Close();

                dr = getWeeklyDate.ExecuteReader();

                while (dr.Read())
                {
                    dbWeeklyDate = DateTime.Parse(dr.GetString(0));
                }
                dr.Close();

                dr = getMonthlyDate.ExecuteReader();

                while (dr.Read())
                {
                    dbMonthlyDate = DateTime.Parse(dr.GetString(0));
                }
                dr.Close();

                dr = getYearlyDate.ExecuteReader();

                while (dr.Read())
                {
                    dbYearlyDate = DateTime.Parse(dr.GetString(0));
                }
                dr.Close();

                dr = getQuaterlyDate.ExecuteReader();

                while (dr.Read())
                {
                    dbQuaterlyDate = DateTime.Parse(dr.GetString(0));
                }
                dr.Close();




                oneMonth = DateTime.Now.AddDays(-30);
                five_days = DateTime.Now.AddDays(-5);

                int dayOfWeek = (int)DateTime.Now.DayOfWeek;
                int month = DateTime.Now.Month;
                int year = DateTime.Now.Year;

                int quatermonth = 0;
                DateTime monthlydate = new DateTime(year, month, Constants.START_DATE);
                DateTime yearlydate = new DateTime(year, Constants.START_MONTH, Constants.START_DATE);

                AllHistoricalDates dates = new AllHistoricalDates();
                dates.Current = DateTime.Now.Date.ToString("yyyy-MM-dd");
                dates.PreviousDay = dateList[0].Date.ToString("yyyy-MM-dd");
                dates.two_days = dateList[1].Date.ToString("yyyy-MM-dd");
                dates.oneMonth = UpdateHistoryDates.GetBackwardDate(dateDict, oneMonth).Date.ToString("yyyy-MM-dd");
                dates.five_days = UpdateHistoryDates.GetBackwardDate(dateDict, five_days).Date.ToString("yyyy-MM-dd");
                if (dayOfWeek > 2)
                {
                    dayOfWeek = dayOfWeek == 0 ? 7 : dayOfWeek;
                    startOfWeek = dateList[0].AddDays(1 - (int)dateList[0].DayOfWeek);
                    dates.Weekly = UpdateHistoryDates.GetBackwardDate(dateDict, startOfWeek).Date.ToString("yyyy-MM-dd");
                   // dates.Weekly = UpdateHistoryDates.GetForwardDate(dateDict, startOfWeek, dbWeeklyDate).Date.ToString("yyyy-MM-dd");
                }
                else
                {
                    dates.Weekly = dbWeeklyDate.Date.ToString("yyyy-MM-dd");
                }

                dates.Monthly = UpdateHistoryDates.GetBackwardDate(dateDict, monthlydate).Date.ToString("yyyy-MM-dd");
                dates.Yearly = UpdateHistoryDates.GetBackwardDate(dateDict, yearlydate).Date.ToString("yyyy-MM-dd");
              //  dates.Monthly = UpdateHistoryDates.GetForwardDate(dateDict, monthlydate, dbMonthlyDate).Date.ToString("yyyy-MM-dd");
              //  dates.Yearly = UpdateHistoryDates.GetForwardDate(dateDict, yearlydate, dbYearlyDate).Date.ToString("yyyy-MM-dd");
                if (month > 3)
                {
                    if (month > 3 && month <= 6)
                    {
                        quatermonth = 4;
                    }
                    else if (month > 6 && month <= 9)
                    {
                        quatermonth = 7;
                    }
                    else if (month > 9 && month <= 12)
                    {
                        quatermonth = 10;
                    }
                }
                else
                {
                    quatermonth = 1;
                }

                DateTime quaterlyDate = new DateTime(year, quatermonth, Constants.START_DATE);
                dates.Quaterly = UpdateHistoryDates.GetBackwardDate(dateDict, quaterlyDate).Date.ToString("yyyy-MM-dd");
               // dates.Quaterly = UpdateHistoryDates.GetForwardDate(dateDict, quaterlyDate, dbQuaterlyDate).Date.ToString("yyyy-MM-dd");

                OdbcCommand updateHistoricalDates = new OdbcCommand("UPDATE historicaldates " +
                                                                 "SET DATE=CASE WHEN (DateType='" + Constants.W + "') THEN '" + dates.Weekly + "' WHEN (DateType='" + Constants.Q + "') THEN '" + dates.Quaterly + "' " +
                                                                 "WHEN (DateType='" + Constants.Y + "') THEN '" + dates.Yearly + "' WHEN (DateType='" + Constants.M + "') THEN '" + dates.Monthly + "' " +
                                                                 "WHEN (DateType='" + Constants.D_2 + "') THEN '" + dates.two_days + "' WHEN (DateType='" + Constants.P + "') THEN '" + dates.PreviousDay + "' " +
                                                                 "WHEN (DateType='" + Constants.D_5 + "') THEN '" + dates.five_days + "' WHEN (DateType='" + Constants.M_1 + "') THEN '" + dates.oneMonth + "' " +
                                                                 "WHEN (DateType='" + Constants.C + "') THEN '" + dates.Current + "' " +
                                                                 "END", con);

                updateHistoricalDates.ExecuteReader();
                log.Info("\nHistorical Dates Updated \n");
                con.Close();

            }

            catch (OdbcException ex)
            {
                log.Error(ex);
            }

        }

    }
}
