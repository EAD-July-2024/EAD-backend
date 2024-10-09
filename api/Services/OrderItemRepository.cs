/*
 * File: OrderItemRepository.cs
 * Author: [​Siriwardana S.M.K.S. ]

 * Description: 
 *     This file contains the OrderItemRepository class, which manages operations related to
 *     order items in the E-commerce system. It provides methods for creating, retrieving,
 *     updating, and deleting order items stored in MongoDB.
 * 
 * Dependencies:
 *     - MongoDB.Driver: Used for interacting with the MongoDB database for order item storage.
 *     - OrderItem: Represents the order item model, including details such as Id, OrderId, 
 *       ProductId, Quantity, Price, Status, and UpdatedDate.
 * 
 * Methods:
 *     - CreateOrderItemAsync: Inserts a new order item into the database.
 *     - GetAllOrderItemsAsync: Retrieves all order items from the database.
 *     - GetOrderItemByIdAsync: Retrieves an order item by its ID.
 *     - GetOrderItemsByOrderIdAsync: Retrieves all order items associated with a specific order ID.
 *     - GetOrderItemsByVendorIdAsync: Retrieves all order items associated with a specific vendor ID.
 *     - UpdateOrderItemAsync: Updates the details of an existing order item.
 *     - UpdateOrderItemStatusAsync: Updates the status of an order item.
 *     - DeleteOrderItemAsync: Deletes an order item by its ID.
 *     - CheckIfProductInOrderItemsAsync: Checks if a given product ID is used in any order item record.
 *     - GetOrderItemByProductIdAndOrderIdAsync: Retrieves an order item by its order ID and product ID.
 * 

 */

using api.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace api.Services
{
    public class OrderItemRepository
    {
        private readonly IMongoCollection<OrderItem> _orderItems;

        public OrderItemRepository(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _orderItems = database.GetCollection<OrderItem>("OrderItems");
        }

        // Create a new OrderItem
        public async Task CreateOrderItemAsync(OrderItem orderItem)
        {
            await _orderItems.InsertOneAsync(orderItem);
        }

        // Get all OrderItems
        public async Task<List<OrderItem>> GetAllOrderItemsAsync()
        {
            return await _orderItems.Find(new BsonDocument()).ToListAsync();
        }

        // Get OrderItem by ID
        public async Task<OrderItem?> GetOrderItemByIdAsync(string id)
        {
            return await _orderItems.Find(o => o.Id == id).FirstOrDefaultAsync();
        }

        // Get OrderItems by Order ID
        public async Task<List<OrderItem>> GetOrderItemsByOrderIdAsync(string orderId)
        {
            return await _orderItems.Find(o => o.OrderId == orderId).ToListAsync();
        }

        // Get OrderItems by Vendor ID
        public async Task<List<OrderItem>> GetOrderItemsByVendorIdAsync(string vendorId)
        {
            return await _orderItems.Find(o => o.VendorId == vendorId).ToListAsync();
        }

        // Update an OrderItem
        public async Task UpdateOrderItemAsync(OrderItem updatedOrderItem)
        {
            var filter = Builders<OrderItem>.Filter.Eq(o => o.Id, updatedOrderItem.Id);
            var update = Builders<OrderItem>.Update
                .Set(o => o.Quantity, updatedOrderItem.Quantity)
                .Set(o => o.Price, updatedOrderItem.Price)
                .Set(o => o.Status, updatedOrderItem.Status)
                .Set(o => o.UpdatedDate, DateTime.Now);

            await _orderItems.UpdateOneAsync(filter, update);
        }

        // Update only the status of an OrderItem
        public async Task UpdateOrderItemStatusAsync(string id, string newStatus)
        {
            var filter = Builders<OrderItem>.Filter.Eq(o => o.Id, id);
            var update = Builders<OrderItem>.Update
                .Set(o => o.Status, newStatus)
                .Set(o => o.UpdatedDate, DateTime.Now); // Update the timestamp

            await _orderItems.UpdateOneAsync(filter, update);
        }


        // Delete an OrderItem by ID
        public async Task DeleteOrderItemAsync(string id)
        {
            await _orderItems.DeleteOneAsync(o => o.Id == id);
        }

        // Method to check if a product ID is used in any record in the order item table
        public async Task<bool> CheckIfProductInOrderItemsAsync(string productId)
        {
            // Check if any OrderItem contains the given product ID
            return await _orderItems.Find(o => o.ProductId == productId).AnyAsync();
        }

        // Method to get an order item by OrderId and ProductId
        public async Task<OrderItem?> GetOrderItemByProductIdAndOrderIdAsync(string orderId, string productId)
        {
            var filter = Builders<OrderItem>.Filter.And(
                Builders<OrderItem>.Filter.Eq(o => o.OrderId, orderId),
                Builders<OrderItem>.Filter.Eq(o => o.ProductId, productId)
            );

            return await _orderItems.Find(filter).FirstOrDefaultAsync();
        }
    }
}
