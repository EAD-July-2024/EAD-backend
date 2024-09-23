using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api.Models
{
    public class Order
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public List<Product> Products { get; set; } = new List<Product>();
        public string Status { get; set; } = "Pending"!;
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public string CustomerId { get; set; } = null!;
        public string VendorId { get; set; } = null!;
    }
}