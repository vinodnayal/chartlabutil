using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModulusFE
{
  namespace TASDK
  {
    class Cycle
    {

      public int Analyze(Field Source)
      {

        int Period = 0;
        int Record = 0;
        int RecordCount = 0;
        int Start = 0;
        double? StartValue = 0;
        double? EndValue1 = 0;
        double? EndValue2 = 0;
        int Total = 0;
        int N = 0;

        RecordCount = Source.RecordCount;

        for (Record = 2; Record <= RecordCount - 1; Record++)
        {
          Start = Record + 2;
          StartValue = Source.Value(Record);
          EndValue1 = Source.Value(Record - 1);
          EndValue2 = Source.Value(Record + 1);
          for (Period = Start; Period <= RecordCount; Period++)
          {
            if (Source.Value(Period) > EndValue2 & Source.Value(Period) < EndValue1)
            {
              EndValue1 = 0;
              break;
            }
          }
          if (EndValue1 == 0) Total = Total + (Period - Start) + 1;
          N = N + 1;
        }

        Total = Total / (RecordCount - 2);

        return Total;
      
      }

    }
  }
}