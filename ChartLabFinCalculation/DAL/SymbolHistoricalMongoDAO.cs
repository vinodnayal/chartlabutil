using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinLib;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Builders;

namespace ChartLabFinCalculation
{
    class SymbolHistoricalMongoDAO
    {
        
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(SymbolHistoricalMongoDAO));
        static SymbolHistoricalMongoDAO()
        {


        }
        public static List<BarData> GetHistoricalDataFromMongo(DateTime from, DateTime to, string symbol)
        {
            List<BarData> symbolDataList = new List<BarData>();


            MongoServer mongo = MongoServer.Create(Constants.MongoConString);

            mongo.Connect();
            var db = mongo.GetDatabase("chartlab");
            var collection = db.GetCollection<BsonDocument>("symbolshistorical");
             var query = Query.And(
             Query.EQ("symbol", symbol),
             Query.GT("date", from).LT(to)
           );

             log.Info("\n\n\n\n\n Getting Data from Mongo DB ");

             foreach (BsonDocument item in collection.Find(query).SetSortOrder(SortBy.Ascending("date")))
            {
                    BarData symboldata = new BarData();
                    symboldata.date = DateTime.Parse(item.GetElement("date").Value.ToString()).Date;
                    symboldata.symbol = item.GetElement("symbol").Value.ToString();
                    symboldata.open = double.Parse(item.GetElement("open").Value.ToString());
                    symboldata.low = double.Parse(item.GetElement("low").Value.ToString());
                    symboldata.high = double.Parse(item.GetElement("high").Value.ToString());
                    symboldata.close = double.Parse(item.GetElement("close").Value.ToString());

               //     log.Info("\n\nGOT the price of date  " + DateTime.Parse(item.GetElement("date").Value.ToString()).Date + " and Price   " + symboldata.close + "\n");
                    symbolDataList.Add(symboldata);
                
            }

            return symbolDataList;
        }

        public static double GetSymbolSpecificDatePrice(string symbol,DateTime date)
        {
            double price=0;
            
            MongoServer mongo = MongoServer.Create(Constants.MongoConString);
            mongo.Connect();
            var db = mongo.GetDatabase("chartlab");
            var collection = db.GetCollection<BsonDocument>("symbolshistorical");

            var query = Query.And(
            Query.EQ("symbol", symbol),
            Query.GT("date", date.Date.AddDays(-5)).LT(date.Date.AddDays(1))
          );

            log.Info("\n\n\n\n\n Getting Data from Mongo DB ");

            var coll=collection.Find(query);
            foreach (BsonDocument item in coll.SetSortOrder(SortBy.Descending("date")))
            {
                price = double.Parse(item.GetElement("close").Value.ToString());
                if (price != 0)
                {
                    log.Info("\n\nGOT "+symbol+" price of date  " + DateTime.Parse(item.GetElement("date").Value.ToString()).Date + " and Price   " + price+"\n");
                    break;
                }
               
            }


            return price;
        }
    }
}
