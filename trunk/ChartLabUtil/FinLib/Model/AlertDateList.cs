using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinLib
{
    public class AlertDateList
    {
        public string Symbol { get; set; } 
        public DateTime ChangeDate { get; set; } 
        public double Volume { get; set; }
        public double PctChange { get; set; }
        public double volumeAlertType { get; set; }
        public double avgVolume { get; set; }

    }
}
