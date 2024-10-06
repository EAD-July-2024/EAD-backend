using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api.Models
{
    public class Order
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string OrderId { get; set; } = null!;
        public string CustomerId { get; set; } = null!;
        public float TotalPrice { get; set; }
        public string Status { get; set; } = "Purchased"!;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}
