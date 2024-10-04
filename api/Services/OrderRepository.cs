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

        //create order
        public async Task CreateOrderAsync(Order order) => await _orders.InsertOneAsync(order);

        public async Task<List<Order>> GetOrdersByCustomerAsync(string customerId) =>
            await _orders.Find(o => o.CustomerId == customerId).ToListAsync();

        public async Task UpdateOrderStatusAsync(ObjectId orderId, string Status) => 
            await _orders.UpdateOneAsync(o => o.Id == orderId, Builders<Order>.Update.Set(o => o.Status, Status));

        
        // Check if any order contains the product custom ID
        public async Task<bool> CheckProductInOrdersAsync(string productCustomId)
        {
            
            var filter = Builders<Order>.Filter.ElemMatch(o => o.Items, item => item.ProductCustomId == productCustomId);
            var order = await _orders.Find(filter).FirstOrDefaultAsync();
            return order != null;
        }
    }
}