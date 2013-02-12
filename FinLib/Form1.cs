
// Copyright 2008 Modulus Financial Engineering, Inc.
// #Warning: Please see our license agreement:
// This code MUST be obfuscated in your release version to comply with our license agreement.
// www.modulusfe.com/support/license.asp

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using System.IO;
using ModulusFE.TASDK;

namespace TASDK
{
  public partial class Form1 : Form
  {

    // The data that is used for calculations.
    Navigator m_nav;
    Recordset m_Recordset;
    Field m_Date;
    Field m_Open;
    Field m_High;
    Field m_Low;
    Field m_Close;
    Field m_Volume;


    public Form1()
    {
      InitializeComponent();
    }


    private void Form1_Load(object sender, EventArgs e)
    {
     
      // Load data
      if(!LoadDataFromCSV(Application.StartupPath + @"\AmericanAirlines.csv")) return;


      // TASDK is very simple to use. The code below demonstrates how to call the functions.

      // Create a simple moving average
      MovingAverage ma = new MovingAverage();
      Recordset results = ma.SimpleMovingAverage(m_nav, m_Close, 14, "SMA1");

      // Output results for simple moving average
      for(int n = 1; n < m_nav.RecordCount + 1; ++n)
        Debug.WriteLine(results.Value("SMA1", n));


        


      // Some indicators return a note object, instead of a recordset object
      Note NT = new Note();
      CandleStick CS = new CandleStick();

      NT = CS.IdentifyPattern(m_nav.Recordset_, m_nav.RecordCount - 4);

      //Now the note object contains the candlestick pattern (if any)
      Debug.WriteLine("Candlestick at record " + (m_nav.RecordCount - 4) + ": " + NT.Note_);
      // For candlestick patterns, if NT.Value = -1, then bearish or if 1, bullish.




      // Neural network indicator:
      NeuralNetwork nn = new NeuralNetwork();
      results = nn.NeuralIndicator(m_nav, m_Close, 14, 1.5, 500, 0.5);

      // Output results for neural network indicator
      // The first n-values will be empty because the function used
      // half the data set for nn training.
      for (int n = m_nav.RecordCount / 2; n < m_nav.RecordCount; ++n)
        Debug.WriteLine(results.Value("NeuralIndicator", n));




      // Using the cylce class
      Cycle cycle = new Cycle();
      Debug.WriteLine("Cycle for m_Close: " + cycle.Analyze(m_Close));




      // Using the line study class
      LineStudy ls = new LineStudy();
      results = ls.SpeedResistanceLines(m_nav, m_nav.Recordset_, m_nav.RecordCount - 100, m_nav.RecordCount);
      for (int n = m_nav.RecordCount - 50; n < m_nav.RecordCount; ++n)
        Debug.WriteLine(results.Value("SpeedResistanceLineMedian", n));


      Index index = new Index();
      //Oscillator oscillator = new Oscillator();
      //Field value1 = oscillator.MACD(m_nav, m_Recordset, 9, 26, 13, "value1").GetField("value1");

      Field value1 = index.RelativeStrengthIndex(m_nav, m_Close, 14, "value1").GetField("value1");
      Oscillator os = new Oscillator();
      Field value2 = os.MACD(m_nav, m_nav.Recordset_, 9, 26, 12, "value2").GetField("value2");
      Field value3 = os.MACDHistogram(m_nav, m_nav.Recordset_, 9, 26, 12, "value3").GetField("value3");
   //   for (int n = 0; n != value1.RecordCount; ++n)
        //  value1.SetValue(n, 0.1 * (value1.Value(n) - 50));

    }



    // Loads a CSV file. Could be changed to read SQL or something else.
    private bool LoadDataFromCSV(string fileName)
    {

      //Requires 6 fields: Date, Open, High, Low, Close, Volume                 
      
      // Load data into arrays
      string[] records;
      try
      {
        StreamReader sr = new StreamReader(fileName);
        records = sr.ReadToEnd().Split(new[] { '\n' });
        sr.Close();
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, "Error:", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);        
        return false;
      }

      if (records.Length == 0)
      {
        MessageBox.Show("No data loaded", "Error:", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return false;
      }

      // Create TASDK objects
      m_Recordset = new Recordset();
      m_Date = new Field(records.Length, "Date");
      m_Open = new Field(records.Length, "Open");
      m_High = new Field(records.Length, "High");
      m_Low = new Field(records.Length, "Low");
      m_Close = new Field(records.Length, "Close");
      m_Volume = new Field(records.Length, "Volume"); 
      
      // Populate TASDK objects
      string[] data;
      for (int n = 0; n < records.Length; ++n)
      {
        data = records[n].Split(new[] { ',' });
        m_Date.ValueStr(n, data[0]);
        m_Open.Value(n, double.Parse(data[1]));
        m_High.Value(n, double.Parse(data[2]));
        m_Low.Value(n, double.Parse(data[3]));
        m_Close.Value(n, double.Parse(data[4]));
        m_Volume.Value(n, double.Parse(data[5]));
      }
      
      m_Recordset.AddField(m_Date);
      m_Recordset.AddField(m_Open);
      m_Recordset.AddField(m_High);
      m_Recordset.AddField(m_Low);
      m_Recordset.AddField(m_Close);
      m_Recordset.AddField(m_Volume);


      m_nav = new Navigator();
      m_nav.Recordset_ = m_Recordset;

      return true;

  }

    

  }
}
