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
    public class ImageDalc : IDisposable
    {
        private MongoServer server;
        private MongoDatabase db;

        public ImageDalc()
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

        public ImageDetail Add(ImageDetail imageDetail)
        {
            var imageDb = db.GetCollection<ImageDetail>("ImageDetail");
            imageDetail._id = ObjectId.GenerateNewId().ToString();
            imageDb.Insert(imageDetail);

            return imageDetail;
        }

        public List<ImageDetail> GetAll()
        {
            var resultItems = new List<ImageDetail>();
            var cursor = db.GetCollection<ImageDetail>("ImageDetail").FindAll().ToList();

            resultItems.AddRange(cursor);
            return resultItems;
        }

        public List<ImageDetail> ByEventId(string eventId)
        {
            var resultItems = new List<ImageDetail>();
            var query = Query.EQ("EventId", eventId);
            var cursor = db.GetCollection<ImageDetail>("ImageDetail").Find(query);
            resultItems.AddRange(cursor);

            return resultItems;
        }
    }
}
