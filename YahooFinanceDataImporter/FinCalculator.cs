using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModulusFE.TASDK;

namespace ConsoleApplication1
{
    class FinCalculator
    {
        Navigator m_nav;
        Recordset m_Recordset;
        Field m_Date;
        Field m_Open;
        Field m_High;
        Field m_Low;
        Field m_Close;
        Field m_Volume;
        public   bool LoadDataFromDB(string symbol, DateTime fromdate, DateTime toDate)
        {
            List<QuoteData> records = Helper.GetSymbolDataRange(symbol, fromdate.Date, toDate.Date);
            //Requires 6 fields: Date, Open, High, Low, Close, Volume                 

            // Load data into arrays
           // string[] records;
           

            // Create TASDK objects
            m_Recordset = new Recordset();
            m_Date = new Field(records.Count, "Date");
            m_Open = new Field(records.Count, "Open");
            m_High = new Field(records.Count, "High");
            m_Low = new Field(records.Count, "Low");
            m_Close = new Field(records.Count, "Close");
            m_Volume = new Field(records.Count, "Volume");

            // Populate TASDK objects
           // string[] data;
            int n = 0;
            foreach (QuoteData data in records)
            {

               // data = records[n].Split(new[] { ',' });
                m_Date.ValueStr(n,data.TradingDate.ToString());
                m_Open.Value(n, data.Open);
                m_High.Value(n, data.High);
                m_Low.Value(n, data.Low);
                m_Close.Value(n, data.Close);
                m_Volume.Value(n,data.Volume);
                n++;
            }

            m_Recordset.AddField(m_Date);
            m_Recordset.AddField(m_Open);
            m_Recordset.AddField(m_High);
            m_Recordset.AddField(m_Low);
            m_Recordset.AddField(m_Close);
            m_Recordset.AddField(m_Volume);


            m_nav = new Navigator();
            m_nav.Recordset_ = m_Recordset;
            Index index = new Index();
            Field value1 = index.RelativeStrengthIndex(m_nav, m_Close, 14, "value1").GetField("value1");
            MovingAverage ma = new MovingAverage();
            Recordset results = ma.SimpleMovingAverage(m_nav, m_Close, 14, "SMA1");
            Field sma=results.GetField("SMA1");
            var x = sma.Value(16);
            var f=m_nav.Recordset_.GetField("Date");
            f.ValueStr(16);
            return true;

        }
    }
}
