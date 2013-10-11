using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageByEventLib.Model
{
    public class ImageDetail
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string ImageId { get; set; }
        public int EventId { get; set; }
        public string Thumbnail { get; set; }
        public List<string> DominantColor { get; set; }   
    }
}
