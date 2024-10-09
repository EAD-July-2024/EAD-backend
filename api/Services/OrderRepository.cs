/*
 * File: OrderRepository.cs
 * Author: [​Siriwardana S.M.K.S. ]

 * Description: 
 *     This file contains the OrderRepository class, which manages operations related to
 *     customer orders in the E-commerce system. It includes methods for creating, retrieving,
 *     updating, and deleting orders stored in MongoDB.
 * 
 * Dependencies:
 *     - MongoDB.Driver: Used for interacting with the MongoDB database for order storage.
 *     - Order: Represents the order model, including details such as OrderId, CustomerId, 
 *       TotalPrice, Status, and UpdatedDate.
 * 
 * Methods:
 *     - CreateOrderAsync: Inserts a new order into the database.
 *     - GetOrdersByCustomerAsync: Retrieves all orders associated with a specific customer ID.
 *     - GetExistingIdsAsync: Checks if an order with the specified order ID already exists.
 *     - GetAllOrdersAsync: Retrieves all orders from the database.
 *     - GetOrderByOrderIdAsync: Retrieves an order by its custom order ID.
 *     - GetOrdersByIdsAsync: Retrieves a list of orders matching the provided list of order IDs.
 *     - UpdateOrderAsync: Updates the details of an existing order.
 *     - UpdateOrderStatusAsync: Updates the status of an order.
 *     - UpdateOrderTotalPriceAsync: Updates the total price of an order.
 *     - UpdateOrderStatusToDeliveredAsync: Updates the status of an order to 'delivered'.
 * 

 */

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

        // Get Orders by a list of Order IDs
        public async Task<List<Order>> GetOrdersByIdsAsync(List<string> orderIds)
        {
            return await _orders.Find(o => orderIds.Contains(o.OrderId)).ToListAsync();
        }


        // Get orders by Vendor ID
        // public async Task<List<Order>> GetOrdersByVendorIdAsync(string vendorId)
        // {
        //     // Fetch all OrderItems associated with the given VendorId
        //     var orderItems = await _orderItemRepository.Find(oi => oi.VendorId == vendorId).ToListAsync();

        //     // Get a list of distinct OrderIds from the fetched OrderItems
        //     var orderIds = orderItems.Select(oi => oi.OrderId).Distinct().ToList();

        //     // Fetch the orders that match the OrderIds
        //     var orders = await _orders.Find(o => orderIds.Contains(o.OrderId)).ToListAsync();

        //     return orders;
        // }


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
    }
}
