using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MaasOne.YahooManaged.Finance;
using MaasOne.YahooManaged.Finance.API;
using System.Configuration;
using System.Collections;
namespace ConsoleApplication1
{
    class AllSecuritiesDownload
    {
       
        public static void Download()
        {
              string path = ConfigurationManager.AppSettings["FundamentalPath"];
            string masterDataFilePath = ConfigurationManager.AppSettings["MasterDataFilePath"];
              //  Console.WriteLine("Path: {0}", value);
                      

            Int64 count=0;
            //Download Sectors
           MarketDownload dl = new MarketDownload();
           SectorResponse sr= dl.DownloadAllSectors();
            
           List<ArrayList> listData = null;
           int sectorId = 1;
           List<ArrayList> listSectors = new List<ArrayList>();
           int industryId = 1;
           List<ArrayList> listIndustries = new List<ArrayList>();
            //foreach(SectorData sector1 in sr.Result)
           // {
              //  IEnumerable<Sector> sectors = new Sector[] { sector1. };
              //  SectorResponse respSectors = dl.DownloadSectors(sectors);

                //Response/Result
                //if (respSectors.Connection.State == MaasOne.YahooManaged.Base.ConnectionState.Success)
                // {
                foreach (MaasOne.YahooManaged.Finance.SectorData sector in sr.Result)
                {
                    MaasOne.YahooManaged.Finance.Sector sectorID = sector.ID;
                    string sectorName = sector.Name;
                    List<MaasOne.YahooManaged.Finance.IndustryData> industries = sector.Industries;
                    int industryCount = industries.Count;
                    ArrayList rowSector = new ArrayList();
                    rowSector.Add(sectorId);
                    rowSector.Add(sectorName);
                    listSectors.Add(rowSector);
                    //Download Industries
                    MaasOne.YahooManaged.Finance.API.IndustryResponse respIndustries = dl.DownloadIndustries(industries);

                    //Response/Result
                    // if (respIndustries.Connection.State == MaasOne.YahooManaged.Base.ConnectionState.Success)
                    //{
                     string fileName= String.Empty;
                    foreach (MaasOne.YahooManaged.Finance.IndustryData industry in respIndustries.Result)
                    {                                        
                        listData = null; new List<ArrayList>();
                        string industryName = industry.Name;
                        ArrayList rowIndustry = new ArrayList();
                        rowIndustry.Add(industryId);
                        rowIndustry.Add(industryName);
                        listIndustries.Add(rowIndustry);

                        fileName = path + "/" + sectorName.Replace(" ", "").Replace("/", "") + "_" + industryName.Replace(" ", "").Replace("/", "") + ".csv";

                        List<MaasOne.YahooManaged.Finance.CompanyInfoData> companies = industry.Companies;
                        int companyCount = companies.Count;
                        ArrayList row=null;
                        listData = new List<ArrayList>();
                        foreach (MaasOne.YahooManaged.Finance.CompanyInfoData company in companies)
                        {
                            
                            string companyID = company.ID;
                            string companyName = company.Name;
                            int employees = company.FullTimeEmployees;
                            System.DateTime start = company.StartDate;
                            string industryNameByCompany = company.IndustryName;
                            Console.WriteLine(companyID + " industryName:" + industryName + " sectorName:" + sectorName);

                            row = new ArrayList();
                            row.Add(companyID);
                            row.Add(companyName);
                            row.Add(sectorId);
                            row.Add(industryId);
                            
                            row.Add(start);
                            listData.Add(row);

                            count++;
                        }
                        CSVExporter.WriteToCSV(listData, fileName);
                        industryId++;
                    }

                    sectorId++;
                    //}

                }
           // }

                string fileNameForSectors = masterDataFilePath + "/" + "Sectors.csv";
                string fileNameForIndustries = masterDataFilePath + "/" + "Industries.csv";
               CSVExporter.WriteToCSV(listSectors, fileNameForSectors);
               CSVExporter.WriteToCSV(listIndustries, fileNameForIndustries);
               Console.WriteLine(count);
               Console.Read();
            //}

        }
    }
}
