using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChartLabFinCalculation.DAL;

namespace ChartLabFinCalculation.BL
{
    class WeeklyReturnCaculation
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(WeeklyReturnCaculation));
        public static void calculateWeeklyReturns()
        {
            try
            {
                // update WorldMktReturn
                try
                {
                    String tableName = "weekly_perf_global_market";
                    WeeklyReturnDAO.updateWeeklyReturns(tableName);
                }
                catch (Exception ex)
                {
                    
                   log.Error("Error: Problem in WorldMktReturn Caculation " + ex);
                }
                // update asset Mkt return
                try
                {
                    String tableName = "weekly_perf_asset_class";
                    WeeklyReturnDAO.updateWeeklyReturns(tableName);
                }
                catch (Exception ex)
                {

                    log.Error("Error: Problem in WorldMktReturn Caculation " + ex);
                }
                // update sub sector returns
                try
                {
                   
                    WeeklyReturnDAO.updateSubSectorReturns();
                }
                catch (Exception ex)
                {

                    log.Error("Error: Problem in update SubSector Returns Caculation " + ex);
                }
                // update snp return and Rating change

                try
                {

                    WeeklyReturnDAO.updatesSNPSymbolsReturns();
                }
                catch (Exception ex)
                {

                    log.Error("Error: Problem in updates SNP Symbols Returns Caculation " + ex);
                }
               
            }
            catch (Exception ex)
            {

                log.Error("Error: Problem in Weekly Return Caculation " + ex);
            } 

        
        }

        
    }
}
