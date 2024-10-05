using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace api.Services
{
    public class OrderRepository
    {
        private readonly IMongoCollection<Order> _orders;

        public OrderRepository(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _orders = database.GetCollection<Order>("Orders");
        }

        // Create order
        public async Task CreateOrderAsync(Order order) => await _orders.InsertOneAsync(order);

        // Get orders by customer
        public async Task<List<Order>> GetOrdersByCustomerAsync(string customerId) =>
            await _orders.Find(o => o.CustomerId == customerId).ToListAsync();


        // Update order status
        public async Task UpdateOrderStatusAsync(string orderId, string status)
        {
            var filter = Builders<Order>.Filter.Eq(o => o.OrderId, orderId);
            var update = Builders<Order>.Update.Set(o => o.Status, status).Set(o => o.UpdatedDate, DateTime.Now);

            await _orders.UpdateOneAsync(filter, update);
        }

        // Check if any order contains the product custom ID
        public async Task<bool> CheckProductInOrdersAsync(string productId)
        {

            var filter = Builders<Order>.Filter.ElemMatch(o => o.Products, product => product.ProductId == productId);
            var order = await _orders.Find(filter).FirstOrDefaultAsync();
            return order != null;
        }

        // Check if order ID exists
        public async Task<bool> getExistingIds(String oId)
        {
            return await _orders.Find(p => p.OrderId == oId).AnyAsync();
        }

        // Get all orders
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _orders.Find(new BsonDocument()).ToListAsync();
        }

        // Get order by custom Order ID
        public async Task<Order> GetOrderByOrderIdAsync(string orderId)
        {
            return await _orders.Find(order => order.OrderId == orderId).FirstOrDefaultAsync();
        }
    }
}