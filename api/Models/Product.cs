using MongoDB.Bson;

namespace api.Models
{
    public class Product
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Price { get; set; } = null!;
        public string CategoryID { get; set; } = null!;
        public string VendorID { get; set; } = null!;
        public List<string> ImageUrls { get; set; } = new List<string>();
        public bool IsActive { get; set; }
    }
}