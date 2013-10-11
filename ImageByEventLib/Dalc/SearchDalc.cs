using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using ImageByEventLib.Model;

namespace ImageByEventLib.Dalc
{
    public class SearchDalc : IDisposable
    {
        private MongoServer server;
        private MongoDatabase db;

        public SearchDalc()
        {
            var mServerAddress = "mongodb://localhost:27017";
            var dbName = "ImageByEvent";

            server = new MongoClient(mServerAddress).GetServer();
            db = server.GetDatabase(dbName);
        }

        public void disconnect()
        {
            server.Disconnect();
        }

        public void Dispose()
        {
            //server.Disconnect();
        }

        public Search Add(Search search)
        {
            var searchDb = db.GetCollection<Search>("Search");
            search._id = ObjectId.GenerateNewId().ToString();
            searchDb.Insert(search);

            return search;
        }

        public List<Search> GetAll()
        {
            var resultItems = new List<Search>();
            var cursor = db.GetCollection<Search>("Search").FindAll().ToList();

            resultItems.AddRange(cursor);
            return resultItems;
        }

        public Search BySqsId(string sqsId)
        {
            var resultItems = new List<Search>();
            var query = Query.EQ("SqsId", sqsId);
            var cursor = db.GetCollection<Search>("Search").Find(query);
            resultItems.AddRange(cursor);

            return resultItems.FirstOrDefault();
        }
    }
}
