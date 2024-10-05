using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api.Models
{
    public class Category
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string CategoryId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Status { get; set; } = "active"!;
        public bool isDeleted { get; set; } = false;
    }
}