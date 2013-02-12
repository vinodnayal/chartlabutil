using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModulusFE
{
  namespace TASDK
  {
    class NeuralNetwork
    {
 
      private double? m_LearningRate; 

      //Neurons 
      double? Neuron1;
      double? Neuron2;
      double? Neuron4;
      double? Neuron3;
      double? Neuron5;
      double? Neuron6; 

      //Weights 
      double? W14;
      double? W13;
      double? W15;
      double? W24;
      double? W23;
      double? W25;
      double? W46;
      double? W36;
      double? W56; 

      //Biases 
      double? B4;
      double? B3;
      double? B5;
      double? B6; 

      private void initialize() 
      {
          Random rnd = new Random();
          W13 = (rnd.NextDouble() * -2) + 1; 
          W14 = (rnd.NextDouble() * -2) + 1; 
          W15 = (rnd.NextDouble() * -2) + 1; 
          W23 = (rnd.NextDouble() * -2) + 1; 
          W24 = (rnd.NextDouble() * -2) + 1; 
          W25 = (rnd.NextDouble() * -2) + 1; 
          W36 = (rnd.NextDouble() * -2) + 1; 
          W46 = (rnd.NextDouble() * -2) + 1; 
          W56 = (rnd.NextDouble() * -2) + 1; 
          B3 = (rnd.NextDouble() * -2) + 1; 
          B4 = (rnd.NextDouble() * -2) + 1; 
          B5 = (rnd.NextDouble() * -2) + 1; 
          B6 = (rnd.NextDouble() * -2) + 1; 
      }

      private void train(double? Input1, double? Input2, double? Target) 
      {

        double? OutErr = 0;
        double? HiddenErr = 0; 
          
          feedForward(Input1, Input2); 
          OutErr = (Target - Neuron6) * Neuron6 * (1 - Neuron6); 
          W36 = W36 + m_LearningRate * OutErr * Neuron3; 
          W46 = W46 + m_LearningRate * OutErr * Neuron4; 
          W56 = W56 + m_LearningRate * OutErr * Neuron5; 
          HiddenErr = Neuron3 * (1 - Neuron3) * OutErr * W36; 
          W13 = W13 + m_LearningRate * HiddenErr * Neuron1; 
          W23 = W23 + m_LearningRate * HiddenErr * Neuron2; 
          HiddenErr = Neuron4 * (1 - Neuron4) * OutErr * W46; 
          W14 = W14 + m_LearningRate * HiddenErr * Neuron1; 
          W24 = W24 + m_LearningRate * HiddenErr * Neuron2; 
          HiddenErr = Neuron5 * (1 - Neuron5) * OutErr * W56; 
          W15 = W15 + m_LearningRate * HiddenErr * Neuron1; 
          W25 = W25 + m_LearningRate * HiddenErr * Neuron2; 
          
      }

      private double? feedForward(double? Input1, double? Input2) 
      {                 
          Neuron1 = Input1; 
          Neuron2 = Input2; 
          Neuron3 = activation((Neuron1 * W13) + (Neuron2 * W23) + B3); 
          Neuron4 = activation((Neuron1 * W14) + (Neuron2 * W24) + B4); 
          Neuron5 = activation((Neuron1 * W15) + (Neuron2 * W25) + B5); 
          Neuron6 = activation((Neuron3 * W36) + (Neuron4 * W46) + (Neuron5 * W56) + B6); 
          return Neuron6; 
      }

      private double activation(double? Value) 
      {
        if (Value == null) Value = 0;
         return 1 / (1 +  System.Math.Exp((double)Value * -1)); 
      }

      public Recordset NeuralIndicator(Navigator pNav, Field Source, int Periods, double? LearningRate, int Epochs, double? PercentTrain) 
      {

          int iRecordCount = pNav.RecordCount;
          Recordset Results;

          Field Field1 = new Field(iRecordCount, "NeuralIndicator");
          Field fInput1 = new Field(iRecordCount, "Input1");
          Field fInput2 = new Field(iRecordCount, "Input2");
          Field fTarget = new Field(iRecordCount, "Target");
          General G = new General(); 
          Note Nt = default(Note); 
          int Record = 0; 
          int TrainRecords = 0;           
          int Epoch = 0; 
          int Start = 0;
          double? Input1 = 0;
          double? Input2 = 0;
          double? Target = 0;
          double? Max = 0; 
          double? Min = 0; 
        
          if (PercentTrain > 0.98 | PercentTrain < 0.2)
              throw new Exception("Invalid PercentTrain");          
          
          //Divide Source into training set and forecast set 
          TrainRecords = (int)((double?)iRecordCount * PercentTrain); 
          
          //Build neural network data sets 
          Start = Periods + 1; 
          int position = Start;

          for(Record = 0; Record < Start + 1; Record++)
          {
            fInput1.Value(Record, 0);
            fInput2.Value(Record, 0);
          }

          for (Record = Start; Record <= iRecordCount; Record++)
          { 
              
              Input1 = Source.Value(position - Periods); 
              Input2 = Source.Value(position); 
              
              fInput1.Value(position, Input1); 
              fInput2.Value(position, Input2);

              position++;
          } 
          
          //Training set target values 
          Start = Periods + 1;
          position = Start;
          for (Record = Start; Record <= TrainRecords - Periods; Record++) {

            Target = Source.Value(position) - Source.Value(position + 1);

            fTarget.Value(position, Target);

            position++;
          } 
          
          //Normalize vectors 
          Nt = G.MaxValue(fInput1, 1, iRecordCount); 
          Max = Nt.Value; 
          Nt = G.MinValue(fInput1, 1, iRecordCount); 
          Min = Nt.Value; 
          for (Record = 1; Record <= iRecordCount; Record++) { 
              fInput1.Value(Record, normalize(Max, Min, fInput1.Value(Record))); 
          }

          Nt = G.MaxValue(fInput2, 1, iRecordCount); 
          Max = Nt.Value; 
          Nt = G.MinValue(fInput2, 1, iRecordCount); 
          Min = Nt.Value; 
          for (Record = 1; Record <= iRecordCount; Record++) { 
              fInput2.Value(Record, normalize(Max, Min, fInput2.Value(Record))); 
          } 
          
          Nt = G.MaxValue(fTarget, 1, iRecordCount); 
          Max = Nt.Value; 
          Nt = G.MinValue(fTarget, 1, iRecordCount); 
          Min = Nt.Value; 
          for (Record = 1; Record <= TrainRecords; Record++) { 
              fTarget.Value(Record, normalize(Max, Min, fTarget.Value(Record))); 
          } 
          
          Start = TrainRecords; 
          
          //Initialize neural network 
          m_LearningRate = LearningRate; 
          initialize(); 
          
          //Train neural network 
          for (Epoch = 1; Epoch <= Epochs; Epoch++) { 
              for (Record = 1; Record <= TrainRecords; Record++) { 
                  train(fInput1.Value(Record), fInput2.Value(Record), fTarget.Value(Record)); 
                  System.Windows.Forms.Application.DoEvents(); 
              } 
          } 
          
          //Output neural network forecasts from TrainRecords + 1 to RecordCount 
          for (Record = TrainRecords; Record <= iRecordCount; Record++) { 
              Field1.Value(Record, feedForward(fInput1.Value(Record), fInput2.Value(Record))); 
          } 
          
          
          Results = new Recordset(); 
          Results.AddField(Field1); 
        
          return Results;
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