using System;

namespace FinLib
{
	public class BarData
	{
        public string symbol { get; set; }
		public DateTime date { get; set; }
		public double open { get; set; }
		public double high { get; set; }
		public double low { get; set; }
		public double close { get; set; }
        public double actualclose { get; set; }
        public double volume { get; set; }
    }
}
