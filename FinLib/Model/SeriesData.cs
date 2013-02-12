using System;

namespace FinLib
{
	public class SeriesData
	{
		public DateTime Timestamp { get; set; }
		public float Price { get; set; }

		public static SeriesData Null = new SeriesData {Price = Constants.NullValue};
	}
}
