using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using FinLib;

namespace ChartLabFinCalculation
{
   public class UpdateHistoryDates
    {
       static log4net.ILog log = log4net.LogManager.GetLogger(typeof(UpdateHistoryDates));

       public static void ChangeHistoryDateInDB()
       {
           ChangeHistoryDatesDAO.ChangeHistoryDate();
       }

       internal static DateTime GetForwardDate(Dictionary<DateTime, DateTime> Dict, DateTime date,DateTime dbDate)
       {
           DateTime requiredDate = new DateTime();
           if (Dict.ContainsKey(date.Date))
           {
               requiredDate = date;
           }
           else
           {
               for (int count = 1; count < 10; count++)
               {
                   if (Dict.ContainsKey(date.Date.AddDays(count)))
                   {
                       requiredDate = Dict[date.Date.AddDays(count)];
                       break;
                   }
                   else
                   {
                       requiredDate = dbDate;
                   }
               }
           }
           return requiredDate;
       }

       internal static DateTime GetBackwardDate(Dictionary<DateTime, DateTime> Dict, DateTime date)
       {
           DateTime requiredDate = new DateTime();
         
               for (int count = 1; count < 10; count++)
               {
                   if (Dict.ContainsKey(date.Date.AddDays(-count)))
                   {
                       requiredDate = Dict[date.Date.AddDays(-count)];
                       break;
                   }
               }
          
           return requiredDate;
       }
       internal static DateTime GetNonHolidayDate(Dictionary<DateTime, DateTime> Dict, DateTime date)
       {
           DateTime requiredDate = new DateTime();
           if (Dict.ContainsKey(date.Date))
           {
               requiredDate = date;
           }
           else
           {
               for (int count = 1; count < 10; count++)
               {
                   if (Dict.ContainsKey(date.Date.AddDays(-count)))
                   {
                       requiredDate = Dict[date.Date.AddDays(-count)];
                       break;
                   }
               }
           }
           return requiredDate;
       }


    }
}
