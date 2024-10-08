/*
 * File: Product.cs

 * Description:
 *     This file defines the Product class, which represents a product within 
 *     the e-commerce platform. Each product is stored in the MongoDB database 
 *     with relevant information such as price, category, vendor, and associated images.
 * 
 * Fields:
 *     - Id (ObjectId):
 *         The unique identifier for the product, stored as an ObjectId in MongoDB. 
 *         This field is marked with [BsonId] to indicate it's the primary key.
 * 
 *     - ProductId (string):
 *         A custom ID for the product, used to uniquely identify products in the system.
 *         This is different from the MongoDB ObjectId and is manually assigned or auto-generated.
 * 
 *     - Name (string):
 *         The name of the product. This is a required field to provide a clear title 
 *         for the product.
 * 
 *     - Description (string?):
 *         An optional field to provide a detailed description of the product. This 
 *         can include features, specifications, or other important information.
 * 
 *     - Price (float):
 *         The price of the product. This represents the current selling price 
 *         and is used in calculating the total for an order.
 * 
 *     - CategoryId (string):
 *         The ID of the category to which the product belongs. This links the 
 *         product to its category and allows for categorization in the system.
 * 
 *     - VendorId (string):
 *         The ID of the vendor selling the product. This field links the product 
 *         to the vendor in the system, which helps with managing vendor-specific inventories.
 * 
 *     - ImageUrls (List<string>):
 *         A list of image URLs associated with the product. These URLs point to 
 *         images stored in a file system or cloud storage, allowing the product 
 *         to display multiple images.
 * 
 *     - IsDeleted (bool):
 *         A flag indicating whether the product has been deleted (soft delete). 
 *         If true, the product is considered removed from the active listings 
 *         but remains in the database for reference.
 * 
 *     - Quantity (int):
 *         The available stock of the product. This field tracks how many units 
 *         of the product are in inventory and is updated as products are sold or restocked.
 * 

 * 
 * Usage:
 *     This class is used to represent products in the e-commerce platform. It 
 *     interacts with other models like Category and Vendor through their respective IDs.
 *     The product details are stored and retrieved from MongoDB.
 * 

 */

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
    // public string CategoryName { get; set; } = null!;
    public string VendorId { get; set; } = null!;
    // public string VendorName { get; set; } = null!;
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