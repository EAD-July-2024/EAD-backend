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
        public async Task CreateOrderAsync(Order order) =>
            await _orders.InsertOneAsync(order);

        // Get orders by customer ID
        public async Task<List<Order>> GetOrdersByCustomerAsync(string customerId) =>
            await _orders.Find(o => o.CustomerId == customerId).ToListAsync();

        // Check if order ID exists
        public async Task<bool> GetExistingIdsAsync(string orderId) =>
            await _orders.Find(o => o.OrderId == orderId).AnyAsync();

        // Get all orders
        public async Task<List<Order>> GetAllOrdersAsync() =>
            await _orders.Find(new BsonDocument()).ToListAsync();

        // Get order by custom Order ID
        public async Task<Order?> GetOrderByOrderIdAsync(string orderId)
        {
            return await _orders.Find(order => order.OrderId == orderId).FirstOrDefaultAsync();
        }
        // Update an existing order
        public async Task UpdateOrderAsync(Order order)
        {
            var filter = Builders<Order>.Filter.Eq(o => o.OrderId, order.OrderId);
            var update = Builders<Order>.Update
                .Set(o => o.TotalPrice, order.TotalPrice)
                .Set(o => o.UpdatedDate, DateTime.Now);

            await _orders.UpdateOneAsync(filter, update);
        }

        // Update order status only
        // public async Task UpdateOrderStatusAsync(string orderId, string status)
        // {
        //     var filter = Builders<Order>.Filter.Eq(o => o.OrderId, orderId);
        //     var update = Builders<Order>.Update
        //         .Set(o => o.Status, status)
        //         .Set(o => o.UpdatedDate, DateTime.Now);

        //     await _orders.UpdateOneAsync(filter, update);
        // }

        // Update order status only
        public async Task UpdateOrderStatusAsync(Order order)
        {
            var filter = Builders<Order>.Filter.Eq(o => o.Id, order.Id);
            await _orders.ReplaceOneAsync(filter, order);
        }

        // Update only the total price of the order
        public async Task UpdateOrderTotalPriceAsync(string orderId, float totalPrice)
        {
            var filter = Builders<Order>.Filter.Eq(o => o.OrderId, orderId);
            var update = Builders<Order>.Update
                .Set(o => o.TotalPrice, totalPrice)
                .Set(o => o.UpdatedDate, DateTime.Now);

            await _orders.UpdateOneAsync(filter, update);
        }

        // Update order status to dilivered
        public async Task UpdateOrderStatusToDeliveredAsync(string orderId, string status)
        {
            var filter = Builders<Order>.Filter.Eq(o => o.OrderId, orderId);
            var update = Builders<Order>.Update
                .Set(o => o.Status, status)
                .Set(o => o.UpdatedDate, DateTime.Now);

            await _orders.UpdateOneAsync(filter, update);
        }


        public async Task<Order?> GetOrderrByOrderIdAsync(string orderId)
        {
            var filter = Builders<Order>.Filter.Eq(o => o.OrderId, orderId);
            return await _orders.Find(filter).FirstOrDefaultAsync();
        }

    }
}
