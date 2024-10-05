using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Product
{
    [BsonId]
    public ObjectId Id { get; set; }
    public string ProductId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public float Price { get; set; }
    public string CategoryId { get; set; } = null!;
    public string VendorId { get; set; } = null!;
    public List<string> ImageUrls { get; set; } = new List<string>();
    public bool IsDeleted { get; set; } = false;
    public int Quantity { get; set; }
}

// public string Id { get; set; }
// public string ProductId { get; set; }
// public string Name { get; set; }
// public string Description { get; set; }
// public string Price { get; set; }
// public string CategoryID { get; set; }
// public string VendorID { get; set; }
// public string? Id { get; set; }