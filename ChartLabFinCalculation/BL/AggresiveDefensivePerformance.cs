using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;

namespace ChartLabFinCalculation
{
    public  class AggresiveDefensivePerformance
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(AggresiveDefensivePerformance));

        public static void UpdateADPerformance()
        {
            try
            {
                log.Info("Process: Update Performance in DB using SP aggresivePerformance and defensivePerformance");
                AggresiveDefensivePerformanceDAO.UpdatePerformance();
            }
            catch (Exception ex)
            {

                log.Error("Error: " + ex);
            }
        }
    }
}
