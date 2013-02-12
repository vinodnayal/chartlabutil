using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModulusFE
{

  internal static class Constants
  {
    public const IndicatorType MA_START = IndicatorType.SimpleMovingAverage;
    public const IndicatorType MA_END = IndicatorType.WeightedMovingAverage;
  }

  public struct Bar
  {
    public double? OpenPrice;
    public double? HighPrice;
    public double? LowPrice;
    public double? ClosePrice;
    public double? Volume;
  } 
    /// <summary>
    /// Supported indicators
    /// </summary>
    public enum IndicatorType
    {
      /// <summary>
      /// Simple Moving Average
      /// </summary>
      SimpleMovingAverage,

      /// <summary>
      /// Exponential Moving Average
      /// </summary>
      ExponentialMovingAverage,

      /// <summary>
      /// Time Series Moving Average
      /// </summary>
      TimeSeriesMovingAverage,

      /// <summary>
      /// Triangular Moving Average
      /// </summary>
      TriangularMovingAverage,

      /// <summary>
      /// Variable Moving Average
      /// </summary>
      VariableMovingAverage,

      /// <summary>
      /// VIDYA Moving Average
      /// </summary>
      VIDYA,

      /// <summary>
      /// Welles Wilder Smoothing
      /// </summary>
      WellesWilderSmoothing,

      /// <summary>
      /// Weighted Moving Average
      /// </summary>
      WeightedMovingAverage,

      /// <summary>
      /// Williams R
      /// </summary>
      WilliamsPctR,

      /// <summary>
      /// Williams Accumulation Dist
      /// </summary>
      WilliamsAccumulationDistribution,

      /// <summary>
      /// Volume Oscillator
      /// </summary>
      VolumeOscillator,

      /// <summary>
      /// Vertical Horizontal Filter
      /// </summary>
      VerticalHorizontalFilter,

      /// <summary>
      /// Ultimate Oscillator
      /// </summary>
      UltimateOscillator,

      /// <summary>
      /// True Range
      /// </summary>
      TrueRange,

      /// <summary>
      /// TRIX
      /// </summary>
      TRIX,

      /// <summary>
      /// Rainbow Oscillator
      /// </summary>
      RainbowOscillator,

      /// <summary>
      /// Price Oscillator
      /// </summary>
      PriceOscillator,

      /// <summary>
      /// Parabolic SAR
      /// </summary>
      ParabolicSAR,

      /// <summary>
      /// Momentum Oscillator
      /// </summary>
      Momentum,

      /// <summary>
      /// MACD
      /// </summary>
      MACD,

      /// <summary>
      /// Ease Of Movement
      /// </summary>
      EaseOfMovement,

      /// <summary>
      /// Directional Movement System
      /// </summary>
      DirectionalMovementSystem,

      /// <summary>
      /// Detrended Price Oscillator
      /// </summary>
      DetrendedPriceOscillator,

      /// <summary>
      /// Chande Momentum Oscillator
      /// </summary>
      ChandeMomentumOscillator,

      /// <summary>
      /// Chaikin Volatility
      /// </summary>
      ChaikinVolatility,

      /// <summary>
      /// AroonOscillator
      /// </summary>
      Aroon,

      /// <summary>
      /// Linear Regression R-Squared, Forecast, Slop and Intercept
      /// </summary>
      Regression,

      /// <summary>
      /// Price Volume Trend
      /// </summary>
      PriceVolumeTrend,

      /// <summary>
      /// Performance Index
      /// </summary>
      Performance,

      /// <summary>
      /// Commodity Channel Index
      /// </summary>
      CommodityChannelIndex,

      /// <summary>
      /// Chaikin Money Flow
      /// </summary>
      ChaikinMoneyFlow,

      /// <summary>
      /// Weighted Close
      /// </summary>
      WeightedClose,

      /// <summary>
      /// Volume ROC
      /// </summary>
      VolumeROC,

      /// <summary>
      /// Typical Price
      /// </summary>
      TypicalPrice,

      /// <summary>
      /// Standard Deviation
      /// </summary>
      StandardDeviation,

      /// <summary>
      /// Price ROC
      /// </summary>
      PriceROC,

      /// <summary>
      /// Median Price
      /// </summary>
      MedianPrice,

      /// <summary>
      /// High Minus Low
      /// </summary>
      HighMinusLow,

      /// <summary>
      /// Bollinger Bands
      /// </summary>
      BollingerBands,

      /// <summary>
      /// Fractal Chaos Bands
      /// </summary>
      FractalChaosBands,

      /// <summary>
      /// High/Low Bands
      /// </summary>
      HighLowBands,

      /// <summary>
      /// Moving Average Envelope
      /// </summary>
      MovingAverageEnvelope,

      /// <summary>
      /// Swing Index
      /// </summary>
      SwingIndex,

      /// <summary>
      /// Accumulative Swing Index
      /// </summary>
      AccumulativeSwingIndex,

      /// <summary>
      /// Comparative RSI
      /// </summary>
      ComparativeRelativeStrength,

      /// <summary>
      /// Mass Index
      /// </summary>
      MassIndex,

      /// <summary>
      /// Money Flow Index
      /// </summary>
      MoneyFlowIndex,

      /// <summary>
      /// Negative Volume Index
      /// </summary>
      NegativeVolumeIndex,

      /// <summary>
      /// On Balance Volume
      /// </summary>
      OnBalanceVolume,

      /// <summary>
      /// Positive Volume Index
      /// </summary>
      PositiveVolumeIndex,

      /// <summary>
      /// Relative Strength Index
      /// </summary>
      RelativeStrengthIndex,

      /// <summary>
      /// Trade Volume Index
      /// </summary>
      TradeVolumeIndex,

      /// <summary>
      /// Stochastic Oscillator
      /// </summary>
      StochasticOscillator,

      /// <summary>
      /// Stochastic Momentum Index
      /// </summary>
      StochasticMomentumIndex,

      /// <summary>
      /// Fractal Chaos Oscillator
      /// </summary>
      FractalChaosOscillator,

      /// <summary>
      /// Prime Number Oscillator
      /// </summary>
      PrimeNumberOscillator,

      /// <summary>
      /// Prime Number Bands
      /// </summary>
      PrimeNumberBands,

      /// <summary>
      /// Historical Volatility
      /// </summary>
      HistoricalVolatility,

      /// <summary>
      /// MACD Histogram
      /// </summary>
      MACDHistogram,

      /// <summary>
      /// An indicator whos values are populated by the user
      /// </summary>
      CustomIndicator,

      /// <summary>
      /// Unknown
      /// </summary>
      Unknown
    }


 }
 