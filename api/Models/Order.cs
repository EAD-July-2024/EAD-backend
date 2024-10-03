using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api.Models
{
    public class OrderItem
    {
        public string ProductCustomId { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Price { get; set; }  // Store the price for each product in the order
    }


    public class Order
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public List<OrderItem> Items { get; set; } = new List<OrderItem>();  // Now holds OrderItems
        public string Status { get; set; } = "Pending"!;
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public string CustomerId { get; set; } = null!;
        public string VendorId { get; set; } = null!;
        public decimal TotalPrice { get; set; }  // Store total order price
    }
}