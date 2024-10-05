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
        public string ProductId { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Price { get; set; }  // Store price of product at time of order
    }

    public class Order
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string OrderId { get; set; } = null!;
        public string CustomerId { get; set; } = null!;
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "Pending"!;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
        public List<OrderItem> Products { get; set; } = new List<OrderItem>();  // Changed from Items to Products
    }
}
