using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ImageByEventLib.Model
{
    public class Search
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string SqsId { get; set; }
        public string SearchTerm { get; set; }
        public List<EventDetail> EventDetails { get; set; }
    }

    public class EventDetail
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
    }
}
