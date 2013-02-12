using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModulusFE.TASDK;

namespace ModulusFE
{
  namespace TASDK
  {
    class LineStudy
    {


      public Recordset QuadrantLines(Navigator pNav, Recordset OHLCV, int StartPeriod, int EndPeriod)
      {

        int iRecordCount = pNav.RecordCount;
        Field Field1 = new Field(iRecordCount, "QuadrantLines1");
        Field Field2 = new Field(iRecordCount, "QuadrantLines2");
        Field Field3 = new Field(iRecordCount, "QuadrantLines3");
        Field Field4 = new Field(iRecordCount, "QuadrantLines4");
        Recordset Results = new Recordset();
        General G = new General();
        Note Nt = default(Note);
        int Record = 0;
        int RecordCount = pNav.RecordCount;
        int Periods = 0;        
        double HH = 0;
        double LL = 0;

        Periods = EndPeriod - StartPeriod;
        if (RecordCount < EndPeriod)        
          throw new Exception("Invalid end period");        
        else if (StartPeriod < 1)        
          throw new Exception("Invalid start period");        

        Nt = G.MaxValue(OHLCV.GetField("High"), StartPeriod, EndPeriod);
        HH = Nt.Value;
        Nt = G.MinValue(OHLCV.GetField("Low"), StartPeriod, EndPeriod);
        LL = Nt.Value;
        
        for (Record = StartPeriod; Record <= EndPeriod; Record++)
        {
          Field1.Value(Record, HH);
          Field2.Value(Record, LL + (((HH - LL) / 4) * 3));
          Field3.Value(Record, LL + ((HH - LL) / 4));
          Field4.Value(Record, LL);          
        }

        Results.AddField(Field1);
        Results.AddField(Field2);
        Results.AddField(Field3);
        Results.AddField(Field4);

        return Results;

      }


      public Recordset TironeLevel(Navigator pNav, Recordset OHLCV, int StartPeriod, int EndPeriod)
      {

        int iRecordCount = pNav.RecordCount;
        Field Field1 = new Field(iRecordCount, "TironeTop");
        Field Field2 = new Field(iRecordCount, "TironeMedian");
        Field Field3 = new Field(iRecordCount, "TironeBottom");

        Recordset Results = new Recordset();
        General G = new General();
        Note Nt = default(Note);
        int Record = 0;
        int RecordCount = pNav.RecordCount;
        int Periods = 0;        
        double HH = 0;
        double LL = 0;
        double Median = 0;
        double Top = 0;
        double Bottom = 0;

        Periods = EndPeriod - StartPeriod;
        if (RecordCount < EndPeriod)
          throw new Exception("Invalid end period");
        else if (StartPeriod < 1)
          throw new Exception("Invalid start period");

        Nt = G.MaxValue(OHLCV.GetField("High"), StartPeriod, EndPeriod);
        HH = Nt.Value;
        Nt = G.MinValue(OHLCV.GetField("Low"), StartPeriod, EndPeriod);
        LL = Nt.Value;

        Top = HH - (HH - LL) / 3;
        Median = LL + (HH - LL) / 2;
        Bottom = LL + (HH - LL) / 3;
        
        for (Record = StartPeriod; Record <= EndPeriod; Record++)
        {
          Field1.Value(Record, Top);
          Field2.Value(Record, Median);
          Field3.Value(Record, Bottom);          
        }

        Results.AddField(Field1);
        Results.AddField(Field2);
        Results.AddField(Field3);

        return Results;

      }


      public Recordset SpeedResistanceLines(Navigator pNav, Recordset OHLCV, int StartPeriod, int EndPeriod)
      {

        int iRecordCount = pNav.RecordCount;
        Field Field1 = new Field(iRecordCount, "SpeedResistanceLineTop");
        Field Field2 = new Field(iRecordCount, "SpeedResistanceLineMedian");
        Field Field3 = new Field(iRecordCount, "SpeedResistanceLineBottom");

        Recordset Results = new Recordset();
        General G = new General();
        Note Nt = default(Note);
        int Record = 0;
        int RecordCount = pNav.RecordCount;
        int Periods = 0;        
        int NewStartPeriod = 0;
        int NewEndPeriod = 0;
        double Value = 0;
        double HH = 0;
        double LL = 0;                
        double Line2 = 0;
        double Line1 = 0;
        double Line3 = 0;
        double Delta = 0;
        double Angle = 0;

        Periods = EndPeriod - StartPeriod;
        if (RecordCount < EndPeriod)
          throw new Exception("Invalid end period");
        else if (StartPeriod < 1)
          throw new Exception("Invalid start period");

        Nt = G.MaxValue(OHLCV.GetField("High"), StartPeriod, EndPeriod);
        HH = Nt.Value;
        NewEndPeriod = Nt.Period;
        Nt = G.MinValue(OHLCV.GetField("Low"), StartPeriod, EndPeriod);
        LL = Nt.Value;
        NewStartPeriod = Nt.Period;

        StartPeriod = NewStartPeriod;
        EndPeriod = NewEndPeriod;

        Value = (HH - LL) / 3;
        Line1 = Value + LL;
        Line2 = (Value * 2) + LL;
        Line3 = (Value * 3) + LL;

        Delta = (Line1 - LL) / (EndPeriod - StartPeriod);
        Angle = 0;
        
        for (Record = StartPeriod; Record <= EndPeriod; Record++)
        {
          Angle = Angle + Delta;
          Field1.Value(Record, Angle + LL);
        }

        Delta = (Line2 - LL) / (EndPeriod - StartPeriod);
        Angle = 0;
        
        for (Record = StartPeriod; Record <= EndPeriod; Record++)
        {
          Angle = Angle + Delta;
          Field2.Value(Record, Angle + LL);          
        }

        Delta = (Line3 - LL) / (EndPeriod - StartPeriod);
        Angle = 0;        
        for (Record = StartPeriod; Record <= EndPeriod; Record++)
        {
          Angle = Angle + Delta;
          Field3.Value(Record, Angle + LL);          
        }


        Results.AddField(Field1);
        Results.AddField(Field2);
        Results.AddField(Field3);

        return Results;

      } 



      public Recordset Fibonacci(Field Source, int StartPeriod, int EndPeriod)
      { 
          
          Recordset Results = new Recordset();
          int iRecordCount = Source.RecordCount;
          Field Field1 = new Field(iRecordCount, "Fibonacci");
          int Period = 0; 
          double Fib1 = 0; 
          double Fib2 = 0; 
          double Fib3 = 0; 
          int Total = 0; 
          Fib1 = 1;          
                    
          Total = EndPeriod - StartPeriod; 
          
          for (Period = StartPeriod; Period <= EndPeriod; Period++) { 
              Fib3 = Fib2; 
              Fib2 = Fib1; 
              Fib1 = Fib2 + Fib3; 
              if (Period + Fib1 > EndPeriod) { 
                  break;
              } 
              else { 
                  Field1.Value((int)((Period + Fib1) - 1), Fib1); 
              } 
          } 
          
          Results.AddField(Field1); 
          return Results; 
          
      } 


      public Recordset TrendLinePenetration(Field Source, int TrendStartPeriod, int TrendStartValue, int TrendEndPeriod, double TrendEndValue) 
      { 
          
          Recordset Results = new Recordset();
          int iRecordCount = Source.RecordCount;
          Field Field1 = new Field(iRecordCount, "Trend");
          Field Field2 = new Field(iRecordCount, "TrendLinePenetration");
          int Record = 0; 
          double? Trend = 0; 
          double? Incr = 0; 
          
          if (TrendEndPeriod < TrendStartPeriod | TrendEndPeriod - TrendStartPeriod < 5 | TrendEndPeriod > iRecordCount)
            throw new Exception("Invalid arguments");          
          
          Incr = (TrendEndValue - TrendStartValue) / (TrendEndPeriod - TrendStartPeriod); 
          
          Trend = Source.Value(TrendStartPeriod) - Incr;
          for (Record = TrendStartPeriod; Record <= TrendEndPeriod; Record++) { 
              Trend = Trend + Incr; 
              Field1.Value(Record, Trend); 
          } 
          
          for (Record = TrendStartPeriod + 1; Record <= TrendEndPeriod; Record++) { 
              if (Field1.Value(Record - 1) < Source.Value(Record - 1) & Field1.Value(Record) >= Source.Value(Record))
              { 
                  Field2.Value(Record, -1); 
              } 
              else if (Field1.Value(Record - 1) > Source.Value(Record - 1) & Field1.Value(Record) <= Source.Value(Record))
              { 
                  Field2.Value(Record, 1); 
              } 
          } 
          
          Results.AddField(Field1);
          Results.AddField(Field2);
          return Results;
          
      }





    }
  }
}