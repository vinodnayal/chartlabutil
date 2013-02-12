using System;
using System.Collections.Generic;
using System.Drawing;


namespace FinLib
{
	public class SeriesCrossFinder
	{
		/// <summary>
		/// Standard deviation for moving averages.http://www.jmp.com/support/faq/jmp2095.shtml
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static double Stdev(IList<SeriesData> data)
		{
			double sum = 0;

			for (int i = 0; i < data.Count - 1; i++)
			{
				if (data[i].Price == Constants.NullValue || data[i + 1].Price == Constants.NullValue)
					continue;

				double d = data[i + 1].Price - data[i].Price;
				sum += (d * d);
			}

			if (sum == 0)
				return 0;

			double t = 1 / (2.0 * (data.Count - 1)) * sum;
			return Math.Sqrt(t);
		}

        public static double Stdev(IList<BarData> data)
        {
            double sum = 0;

            for (int i = 0; i < data.Count - 1; i++)
            {
                if (data[i].close == Constants.NullValue || data[i + 1].close == Constants.NullValue)
                    continue;

                double d = data[i + 1].close - data[i].close;
                sum += (d * d);
            }

            if (sum == 0)
                return 0;

            double t = 1 / (2.0 * (data.Count - 1)) * sum;
            return Math.Sqrt(t);
        }
        public static double StandardDeviation(double[] data)
        {

            double ret = 0;
            double DataAverage = 0;
            double TotalVariance = 0;
            int Max = 0;

            try
            {

                Max = data.Length;

                if (Max == 0) { return ret; }

                DataAverage = Average(data);

                for (int i = 0; i < Max; i++)
                {
                    TotalVariance += Math.Pow(data[i] - DataAverage, 2);
                }

                ret = Math.Sqrt(SafeDivide(TotalVariance, Max));

            }
            catch (Exception ex) { throw ex; }
            return ret;
        }
        private static double Average(double[] data)
        {

            double ret = 0;
            double DataTotal = 0;

            try
            {

                for (int i = 0; i < data.Length; i++)
                {
                    DataTotal += data[i];
                }

                return SafeDivide(DataTotal, data.Length);

            }
            catch (Exception ex) { throw ex; }
            return ret;
        }
        private static double SafeDivide(double value1, double value2)
        {

            double ret = 0;

            try
            {

                if ((value1 == 0) || (value2 == 0)) { return ret; }

                ret = value1 / value2;

            }
            catch(Exception ex) { throw ex; }
            return ret;
        }


		public static List<SeriesData> SMA(List<BarData> bars, Func<BarData, double> fieldGetter, int period)
		{
           // bars = bars.GetRange();
           
			List<SeriesData> data = new List<SeriesData>();
          
                for (int i = 0; i < bars.Count; i++)
                {
                    if (i < period)
                    {
                        data.Add(SeriesData.Null);
                        continue;
                    }

                    double sum = 0;
                    for (int j = i - period; j < i; j++)
                        sum += fieldGetter(bars[j]);

                    data.Add(new SeriesData
                                {
                                    Timestamp = bars[i].date,
                                    Price = (float)(sum / period)
                                });
                }
                return data;
         
           

			
		}

	}
}
