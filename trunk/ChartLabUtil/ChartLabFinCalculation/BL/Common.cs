using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinLib;

namespace ChartLabFinCalculation
{
    public class Common
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(Common));

        public static double GetDayWiseReturn(Dictionary<DateTime, double> dict, DateTime date, int days, bool forwardDays)
        {
            double price = 0;
            if (!forwardDays)
            {
                days = -days;
            }

            if (dict.ContainsKey(date.AddDays(days)))
            {
                price = dict[date.AddDays(days)];
            }
            else if (forwardDays)
            {
                int count = 1;
                while (count <= 5)
                {
                    String forwardDateString = date.AddDays(days + count).ToString("yyyy-MM-dd"); ;
                    DateTime forwardDate = DateTime.Parse(forwardDateString);
                    if (dict.ContainsKey(forwardDate))
                    {
                        price = dict[forwardDate];
                        break;
                    }
                    count++;
                }

            }
            else
            {
                int count = 1;
                while (count <= 5)
                {
                    String PredateString = date.AddDays(days - count).ToString("yyyy-MM-dd"); ;
                    DateTime Predate = DateTime.Parse(PredateString);
                    if (dict.ContainsKey(Predate))
                    {
                        price = dict[Predate];
                        break;
                    }
                    count++;
                }

            }
            return price;

        }

        public static AvgReturnAndCount CalculateAvgReturn(List<double> DayWiseReturnList)
        {
           // List<AvgReturnAndCount> avgReturnPlusCountList=new List<AvgReturnAndCount>();
            AvgReturnAndCount avgReturnCountObj = new AvgReturnAndCount();
            double avgReturn = 0;
            int count = 0;
            for (int i = 0; i < DayWiseReturnList.Count; i++)
            {
                avgReturn += DayWiseReturnList[i];
                if (DayWiseReturnList[i] != 0)
                {
                    count++;
                }
            }
            if (count != 0)
            {
                avgReturn = avgReturn / count;
            }

            avgReturnCountObj.AvgReturn = avgReturn;
            avgReturnCountObj.Count = count;
            //avgReturnPlusCountList.Add(avgReturnCountObj);
            return avgReturnCountObj;
        }
    }
}
