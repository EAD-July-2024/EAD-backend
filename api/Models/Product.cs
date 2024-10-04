using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]  
    public string Id { get; set; }
    public string ProductId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Price { get; set; }
    public string CategoryID { get; set; }
    public string VendorID { get; set; }
    public List<string> ImageUrls { get; set; } = new List<string>();
    public bool IsActive { get; set; }

    public int Quantity { get; set; }
}