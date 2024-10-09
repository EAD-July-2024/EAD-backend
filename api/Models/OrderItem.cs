/*
 * File: OrderItem.cs

 * Description:
 *     This file defines the OrderItem class, representing an individual item 
 *     within an order in the e-commerce platform. It is used to store and 
 *     manage the details of each product in an order, and it links each product 
 *     to the overall order in the MongoDB database.
 * 
 * Fields:
 *     - Id (string):
 *         The unique identifier for the order item, stored as a string and 
 *         represented by the ObjectId type in MongoDB. It is marked with 
 *         [BsonId] and [BsonRepresentation(BsonType.ObjectId)] to denote that 
 *         this field is the primary key and will be stored as an ObjectId.
 * 
 *     - OrderId (string):
 *         The ID of the order that this item belongs to. It links the order item 
 *         to the overall order.
 * 
 *     - ProductId (string):
 *         The unique identifier of the product that has been ordered. This 
 *         field links the order item to the product.
 * 
 *     - ProductName (string):
 *         The name of the product that has been ordered. This field helps with 
 *         easier readability and understanding when reviewing order details.
 * 
 *     - VendorId (string):
 *         The ID of the vendor who sells the product. This field links the order 
 *         item to the corresponding vendor.
 * 
 *     - Quantity (int):
 *         The quantity of the product that has been ordered in this order item.
 * 
 *     - Price (float):
 *         The price of the product at the time of the order. This represents 
 *         the unit price and will be used to calculate the total cost based on 
 *         the quantity.
 * 
 *     - Status (string):
 *         Represents the status of the order item. The default status is "Purchased", 
 *         but it can be changed to statuses like "Shipped", "Delivered", etc., 
 *         based on the order's lifecycle.
 * 
 *     - CreatedDate (DateTime):
 *         The date and time when the order item was created. It defaults to 
 *         the current date and time upon order item creation.
 * 
 *     - UpdatedDate (DateTime):
 *         The date and time when the order item was last updated. It also defaults 
 *         to the current date and time and should be modified whenever the item 
 *         is updated.
 * 

 * 
 * Usage:
 *     This class is used within the OrderItemRepository to manage the items in 
 *     an order in MongoDB. It provides a representation of the data structure for 
 *     each individual item within an order.
 * 

 */

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api.Models
{
    public class OrderItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        public string OrderId { get; set; } = null!;
        public string ProductId { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string VendorId { get; set; } = null!;
        public int Quantity { get; set; }
        public float Price { get; set; }
        public string Status { get; set; } = "Purchased";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}
