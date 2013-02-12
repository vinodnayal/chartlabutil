using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModulusFE.TASDK;

namespace ModulusFE
{
  namespace TASDK
  {
    /// <summary>
    /// Candlestick recognition
    /// </summary>
    ///

    class CandleStick
    {

      private struct Candle
      {
        public string Name;
        public double Range;
        public double BullBear;
        public int Direction;
      }

      public Note IdentifyPattern(Recordset OHLCV, int Period)
      {
        
        Bar[] Bars = new Bar[4];
        CandleStick.Candle[] Cndl = new CandleStick.Candle[4];
        Note RetVal = new Note();
        short N = 0;
        double? Min = 0;
        double? Max = 0;
        double?[,] HL = new double?[3, 4];
        double?[] R = new double?[4];
        double? Value = 0;

        RetVal.Note_ = "NO PATTERN";

        if (Period < 3)
        {          
          throw new Exception("EndPeriod must be greater than 3");          
        }
        
        //Get 3 bars-------------------------------------------- 
        Bars[3].OpenPrice = OHLCV.Value("Open", Period);
        Bars[3].HighPrice = OHLCV.Value("High", Period);
        Bars[3].LowPrice = OHLCV.Value("Low", Period);
        Bars[3].ClosePrice = OHLCV.Value("Close", Period);

        Bars[2].OpenPrice = OHLCV.Value("Open", Period - 1);
        Bars[2].HighPrice = OHLCV.Value("High", Period - 1);
        Bars[2].LowPrice = OHLCV.Value("Low", Period - 1);
        Bars[2].ClosePrice = OHLCV.Value("Close", Period - 1);

        Bars[1].OpenPrice = OHLCV.Value("Open", Period - 2);
        Bars[1].HighPrice = OHLCV.Value("High", Period - 2);
        Bars[1].LowPrice = OHLCV.Value("Low", Period - 2);
        Bars[1].ClosePrice = OHLCV.Value("Close", Period - 2);

        for (N = 1; N <= 3; N++)
        {
          HL[1, N] = normalize(Bars[N].HighPrice, Bars[N].LowPrice, Bars[N].OpenPrice);
          HL[2, N] = normalize(Bars[N].HighPrice, Bars[N].LowPrice, Bars[N].ClosePrice);
        }

        //Get max and min then normalize for candle pattern 

        Max = System.Math.Abs((double)Bars[1].HighPrice - (double)Bars[1].LowPrice);
        Min = System.Math.Abs((double)Bars[1].HighPrice - (double)Bars[1].LowPrice);
        for (N = 1; N <= 3; N++)
        {
          if (System.Math.Abs((double)Bars[N].HighPrice - (double)Bars[N].LowPrice) > Max)
          {
            Max = System.Math.Abs((double)Bars[N].HighPrice - (double)Bars[N].LowPrice);
          }
          if (System.Math.Abs((double)Bars[N].HighPrice - (double)Bars[N].LowPrice) < Min)
          {
            Min = System.Math.Abs((double)Bars[N].HighPrice - (double)Bars[N].LowPrice);
          }
        }
        for (N = 1; N <= 3; N++)
        {
          if (Max == Min)
          {
            R[N] = 1;
          }
          else
          {
            R[N] = normalize(Max, Min, System.Math.Abs((double)Bars[N].HighPrice - (double)Bars[N].LowPrice));
          }
        }

        Max = Bars[1].HighPrice;
        if (Bars[2].HighPrice > Max) Max = Bars[2].HighPrice;
        if (Bars[3].HighPrice > Max) Max = Bars[3].HighPrice;

        Min = Bars[1].LowPrice;
        if (Bars[2].LowPrice < Min) Min = Bars[2].LowPrice;
        if (Bars[3].LowPrice < Min) Min = Bars[3].LowPrice;

        Bars[1].OpenPrice = normalize(Max, Min, Bars[1].OpenPrice);
        Bars[1].HighPrice = normalize(Max, Min, Bars[1].HighPrice);
        Bars[1].LowPrice = normalize(Max, Min, Bars[1].LowPrice);
        Bars[1].ClosePrice = normalize(Max, Min, Bars[1].ClosePrice);

        Bars[2].OpenPrice = normalize(Max, Min, Bars[2].OpenPrice);
        Bars[2].HighPrice = normalize(Max, Min, Bars[2].HighPrice);
        Bars[2].LowPrice = normalize(Max, Min, Bars[2].LowPrice);
        Bars[2].ClosePrice = normalize(Max, Min, Bars[2].ClosePrice);

        Bars[3].OpenPrice = normalize(Max, Min, Bars[3].OpenPrice);
        Bars[3].HighPrice = normalize(Max, Min, Bars[3].HighPrice);
        Bars[3].LowPrice = normalize(Max, Min, Bars[3].LowPrice);
        Bars[3].ClosePrice = normalize(Max, Min, Bars[3].ClosePrice);


        //Identify Bars 
        for (N = 1; N <= 3; N++)
        {

          //Long body------------------------------------ 
          if (System.Math.Abs((double)HL[1, N] - (double)HL[2, N]) > 0.5 & R[N] >= 0.4)
          {
            Cndl[N].Name = "Long Body";
            if (Bars[N].ClosePrice > Bars[N].OpenPrice)
            {
              Cndl[N].BullBear = 1;
            }
            else if (Bars[N].ClosePrice < Bars[N].OpenPrice)
            {
              Cndl[N].BullBear = -1;

            }
          }
          //Doji------------------------------------------ 
          else if (System.Math.Abs((double)HL[1, N] - (double)HL[2, N]) < 0.2 & R[N] >= 0.5)
          {
            Cndl[N].Name = "Doji";
            Cndl[N].BullBear = 0;

            if (N == 3) RetVal.Note_ = "Doji";
          }
          //Hammer---------------------------------------- 
          else if (System.Math.Abs((double)HL[1, N] - (double)HL[2, N]) < 0.45 & ((HL[1, N] > 0.65) | (HL[2, N] < 0.35)))
          {
            if (HL[1, N] > 0.65)
            {
              Cndl[N].Name = "Hammer";
              if ((Bars[N].ClosePrice >= Bars[N].HighPrice))
              {
                Cndl[N].Direction = 1;
              }
              else
              {
                Cndl[N].Direction = -1;
              }
              Cndl[N].BullBear = 1;
            }
            else if (HL[2, N] < 0.35)
            {
              Cndl[N].Name = "Hammer";
              Cndl[N].BullBear = -1;
              if ((Bars[N].ClosePrice >= Bars[N].HighPrice))
              {
                Cndl[N].Direction = 1;
              }
              else
              {
                Cndl[N].Direction = -1;
              }

            }
          }
          //Star----------------------------------------- 
          else if (System.Math.Abs((double)HL[1, N] - (double)HL[2, N]) < 0.66 & HL[1, N] < 0.8 & HL[2, N] > 0.2)
          {
            Cndl[N].Name = "Harami";
            Cndl[N].BullBear = 0;
            if (System.Math.Abs((double)HL[1, N] - (double)HL[2, N]) < 0.1)
            {
              Cndl[N].Name = "Harami";
              Cndl[N].BullBear = 0;
            }
            else if (N > 1)
            {
              if ((Bars[N].OpenPrice >= Bars[N - 1].HighPrice & Bars[N].ClosePrice >= Bars[N - 1].HighPrice))
              {
                Cndl[N].Name = "Star";
                Cndl[N].Direction = 1;
              }
              else if ((Bars[N].OpenPrice <= Bars[N - 1].LowPrice & Bars[N].ClosePrice <= Bars[N - 1].LowPrice))
              {
                Cndl[N].Name = "Star";
                Cndl[N].Direction = -1;
              }

            }

          }
        }

        //First, try to identify a pattern with 3 candles 
        //(Morning/Evening Star). If this pattern doesn't exist 
        //then look for patterns of 2 candles. If none 
        //exist then look for a single candle pattern. 

        //Morning Star------------------------------------------------- 
        if (Cndl[1].Name == "Long Body" & Cndl[1].BullBear == -1 & (Cndl[2].Name == "Star" | Cndl[2].Name == "Doji Star" | Cndl[2].Name == "Hammer") & Cndl[3].Name == "Long Body" & Cndl[3].BullBear == 1)
        {
          RetVal.Note_ = "Morning Star";

          RetVal.Value = 1;
        }
        //Evening Star--------------------------------------------------- 
        else if (Cndl[1].Name == "Long Body" & Cndl[1].BullBear == 1 & (Cndl[2].Name == "Star" | Cndl[2].Name == "Doji Star" | Cndl[2].Name == "Hammer") & Cndl[3].Name == "Long Body" & Cndl[3].BullBear == -1)
        {
          RetVal.Note_ = "Evening Star";

          RetVal.Value = -1;
        }
        //Piecering Line------------------------------------------------ 
        else if (Cndl[2].Name == "Long Body" & Cndl[2].BullBear == -1 & Cndl[3].Name == "Long Body" & Cndl[3].BullBear == 1 & Bars[3].LowPrice < Bars[2].LowPrice & Bars[3].ClosePrice >= ((Bars[2].HighPrice + Bars[2].LowPrice) / 2))
        {
          RetVal.Note_ = "Piercing Line";

          RetVal.Value = 1;
        }
        //Bullish Engulfing Line---------------------------------------- 
        else if (Bars[3].ClosePrice > maxVal(Bars[2].ClosePrice, Bars[2].OpenPrice) & Bars[3].LowPrice < minVal(Bars[2].ClosePrice, Bars[2].OpenPrice) & Bars[2].ClosePrice < Bars[2].OpenPrice & Bars[3].OpenPrice < Bars[3].ClosePrice)
        {
          RetVal.Note_ = "Engulfing Line";

          RetVal.Value = 1;
        }
        //Bullish Doji Star--------------------------------------------- 
        else if (Cndl[2].Name == "Long Body" & Cndl[3].Name == "Doji Star" & minVal(Bars[2].ClosePrice, Bars[2].OpenPrice) > maxVal(Bars[3].ClosePrice, Bars[3].OpenPrice))
        {
          RetVal.Note_ = "Doji Star";

          RetVal.Value = 1;
        }
        //Hanging Man--------------------------------------------------- 
        else if (Cndl[3].Name == "Hammer" & Cndl[3].BullBear == 1 & Cndl[3].Direction == 1)
        {
          RetVal.Note_ = "Hanging Man";

          RetVal.Value = -1;
        }
        //Dark Cloud---------------------------------------------------- 
        else if (maxVal(Bars[3].ClosePrice, Bars[3].OpenPrice) > maxVal(Bars[2].ClosePrice, Bars[2].OpenPrice) & Bars[3].LowPrice < (Bars[2].HighPrice + Bars[2].LowPrice) / 2 & Cndl[3].Name == "Long Body" & Cndl[3].BullBear == -1 & Cndl[2].Name == "Long Body" & Cndl[2].BullBear == 1 & minVal(Bars[3].ClosePrice, Bars[3].OpenPrice) > Bars[2].LowPrice)
        {
          RetVal.Note_ = "Dark Cloud Cover";

          RetVal.Value = -1;
        }
        //Bearish Engulfing Line---------------------------------------- 
        else if (Cndl[3].Name == "Long Body" & Cndl[3].BullBear == -1 & (Cndl[2].Name == "Long Body" | Cndl[2].Name == "Harami") & Bars[2].ClosePrice < maxVal(Bars[3].ClosePrice, Bars[3].OpenPrice) & Bars[2].OpenPrice > minVal(Bars[3].ClosePrice, Bars[3].OpenPrice))
        {
          RetVal.Note_ = "Engulfing Line";

          RetVal.Value = -1;
        }
        //Bearish Doji Star--------------------------------------------- 
        else if (Cndl[2].Name == "Long Body" & Cndl[3].Name == "Doji Star" & minVal(Bars[2].ClosePrice, Bars[2].OpenPrice) < maxVal(Bars[3].ClosePrice, Bars[3].OpenPrice))
        {
          RetVal.Note_ = "Doji Star";

          RetVal.Value = -1;
        }
        //Shooting Star------------------------------------------------- 
        else if (Cndl[2].Name == "Long Body" & Cndl[3].Name == "Star" & minVal(Bars[2].ClosePrice, Bars[2].OpenPrice) < maxVal(Bars[3].ClosePrice, Bars[3].OpenPrice))
        {
          RetVal.Note_ = "Shooting Star";

          RetVal.Value = -1;
        }
        //Spinning Tops------------------------------------------------- 
        else if (Cndl[2].Name == "Star" & Cndl[3].Name == "Star")
        {
          RetVal.Note_ = "Spinning Tops";

          RetVal.Value = 0;
        }
        //Harami Cross-------------------------------------------------- 
        else if (Cndl[2].Name == "Long Body" & Cndl[3].Name == "Harami" & maxVal(Bars[2].ClosePrice, Bars[2].OpenPrice) >= Bars[3].HighPrice & minVal(Bars[2].ClosePrice, Bars[2].OpenPrice) <= Bars[3].LowPrice)
        {
          RetVal.Note_ = "Harami Cross";

          RetVal.Value = -1;
        }

        if (string.IsNullOrEmpty(RetVal.Note_))
        {
          //No pattern so return last candle 
          RetVal.Note_ = Cndl[3].Name;
          RetVal.Value = Cndl[3].BullBear;
        }
        
        return RetVal;
      
      }


      public double? maxVal(double? Value1, double? Value2)
      {
        double? functionReturnValue = 0;
        if (Value1 > Value2)
        {
          functionReturnValue = Value1;
        }
        else if (Value2 > Value1)
        {
          functionReturnValue = Value2;
        }
        return functionReturnValue;
      }

      public double? minVal(double? Value1, double? Value2)
      {
        double? functionReturnValue = 0;
        if (Value1 < Value2)
        {
          functionReturnValue = Value1;
        }
        else if (Value2 < Value1)
        {
          functionReturnValue = Value2;
        }
        return functionReturnValue;
      }

      public double? normalize(double? Max, double? Min, double? Value)
      {
        if (Max == Min) return 0;

        return (Value - Min) / (Max - Min);
      } 


    }
  }
}