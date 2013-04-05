using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinLib;

namespace ChartLabFinCalculation
{
    public class SectorWiseTopBottomSymbols
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(SectorWiseTopBottomSymbols));

        public static void CalculateTopBottomSymbols(string StrongWeakSymbolsPath)
        {
            List<int> sectorList = SectorWiseSymbolsDAO.GetSectorId();
            log.Info("Process: calculating sectors top and bottom symbols");
            List<SectorStrongWeakSymbol> StrongWeakSymbolList = new List<SectorStrongWeakSymbol>();
            try
            {
                for (int i = 0; i < sectorList.Count; i++)
                {
                    int sectorId = sectorList[i];
                    List<SectorStrongWeakSymbol> ListOfStrongSymbols = SectorWiseSymbolsDAO.GetStrongWeakSymbolBySector(sectorId, Constants.Strong_Symbol);
                    List<SectorStrongWeakSymbol> ListOfWeakSymbols = SectorWiseSymbolsDAO.GetStrongWeakSymbolBySector(sectorId, Constants.Weak_Symbol);
                    StrongWeakSymbolList.AddRange(ListOfStrongSymbols);
                    StrongWeakSymbolList.AddRange(ListOfWeakSymbols);

                }
            }
            catch (Exception ex)
            {
                log.Error("Error :"+ex);
            }

            log.Info("Process: Write T oCSV Sector Wise Strong Weak Symbols");
            CSVExporter.WriteToCSVSectorWiseStrongWeakSymbols(StrongWeakSymbolList, StrongWeakSymbolsPath + "/StrongWeakSymbolsFile.csv");
            log.Info("Process: Save Daily Strong Weak Symbols CSV To DB");
            SectorWiseSymbolsDAO.SaveDailyStrongWeakSymbolsCSVToDB(StrongWeakSymbolsPath);

        }

       
    }
}
