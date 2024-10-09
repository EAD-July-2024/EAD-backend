/*
 * File: OrderItemController.cs
 * Author: [​Siriwardana S.M.K.S. ]

 * Description:
 *     This file contains the OrderItemController class, which manages order item operations 
 *     for the E-commerce system. It provides functionalities to create, retrieve, update, 
 *     and delete order items, as well as update their status. The controller ensures that 
 *     updates to order items can only occur if the associated order has not been dispatched 
 *     or delivered. It interacts with the OrderItemRepository and OrderRepository to perform 
 *     necessary database operations.
 * 
 * Methods:
 *     - CreateOrderItem: Creates a new order item based on the provided data.
 *     - GetAllOrderItems: Retrieves a list of all order items.
 *     - GetOrderItemById: Fetches a specific order item by its ID.
 *     - UpdateOrderItem: Updates an existing order item, checking if the associated order 
 *                        is not dispatched or delivered.
 *     - UpdateOrderItemStatus: Updates the status of a specific order item, with checks 
 *                              on the associated order's status.
 *     - DeleteOrderItem: Deletes a specific order item by its ID.
 *     - GetOrderItem: Retrieves an order item based on its order ID and product ID.
 * 
 * Dependencies:
 *     - OrderItemRepository: Used for accessing order item data from the database.
 *     - OrderRepository: Used for accessing order data and verifying order statuses.
 */

using api.Models;
using api.Services;
using api.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Controller]
    [Route("api/orderItem")]
    public class OrderItemController : Controller
    {
        private readonly OrderItemRepository _orderItemRepository;
        private readonly OrderRepository _orderRepository;

        public OrderItemController(OrderItemRepository orderItemRepository, OrderRepository orderRepository)
        {
            _orderItemRepository = orderItemRepository;
            _orderRepository = orderRepository;
        }

        // Create a new OrderItem
        [HttpPost]
        public async Task<IActionResult> CreateOrderItem([FromBody] OrderItem orderItem)
        {
            await _orderItemRepository.CreateOrderItemAsync(orderItem);
            return Ok(orderItem);
        }

        // Get all OrderItems
        [HttpGet]
        public async Task<IActionResult> GetAllOrderItems()
        {
            var orderItems = await _orderItemRepository.GetAllOrderItemsAsync();
            return Ok(orderItems);
        }

        // Get an OrderItem by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderItemById(string id)
        {
            var orderItem = await _orderItemRepository.GetOrderItemByIdAsync(id);
            if (orderItem == null)
            {
                return NotFound($"OrderItem with ID {id} not found.");
            }
            return Ok(orderItem);
        }

        // Update an OrderItem by ID
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrderItem(string id, [FromBody] OrderItem updatedOrderItem)
        {
            // Fetch the existing OrderItem
            var existingOrderItem = await _orderItemRepository.GetOrderItemByIdAsync(id);
            if (existingOrderItem == null)
            {
                return NotFound($"OrderItem with ID {id} not found.");
            }

            // Check if the order associated with this OrderItem is not dispatched or delivered
            var order = await _orderRepository.GetOrderByOrderIdAsync(existingOrderItem.OrderId);
            if (order == null)
            {
                return NotFound($"Order with ID {existingOrderItem.OrderId} not found.");
            }

            // Allow update only if the order status is not 'Dispatched' or 'Delivered'
            if (order.Status == "Dispatched" || order.Status == "Delivered")
            {
                return BadRequest("Cannot update the OrderItem as the associated order has already been dispatched or delivered.");
            }

            // Update the OrderItem
            updatedOrderItem.Id = id;
            await _orderItemRepository.UpdateOrderItemAsync(updatedOrderItem);
            return Ok(updatedOrderItem);
        }

        // Update only the status of an OrderItem
        [HttpPatch("updateStatus/{id}")]
        public async Task<IActionResult> UpdateOrderItemStatus(string id, [FromBody] UpdateOrderItemStatusRequest request)
        {
            // Fetch the existing OrderItem
            var existingOrderItem = await _orderItemRepository.GetOrderItemByIdAsync(id);
            if (existingOrderItem == null)
            {
                return NotFound($"OrderItem with ID {id} not found.");
            }

            // Fetch the order associated with this OrderItem
            var order = await _orderRepository.GetOrderByOrderIdAsync(existingOrderItem.OrderId);
            if (order == null)
            {
                return NotFound($"Order with ID {existingOrderItem.OrderId} not found.");
            }

            // Allow update only if the order status is not 'Dispatched' or 'Delivered'
            if (order.Status == "Dispatched" || order.Status == "Delivered")
            {
                return BadRequest("Cannot update the OrderItem status as the associated order has already been dispatched or delivered.");
            }

            // Update the OrderItem status
            existingOrderItem.Status = request.NewStatus;
            existingOrderItem.UpdatedDate = DateTime.Now;

            // Update the status in the repository
            await _orderItemRepository.UpdateOrderItemStatusAsync(existingOrderItem.Id, existingOrderItem.Status);

            // Update the order status as 'Delivered' if all OrderItems are 'Delivered'
            if (existingOrderItem.Status == "Delivered")
            {
                var orderItems = await _orderItemRepository.GetOrderItemsByOrderIdAsync(existingOrderItem.OrderId);
                var allItemsDelivered = orderItems.All(item => item.Status == "Delivered");
                if (allItemsDelivered)
                {
                    await _orderRepository.UpdateOrderStatusToDeliveredAsync(existingOrderItem.OrderId, "Delivered");
                }
            }
            return Ok(existingOrderItem);
        }


        // [HttpPatch("updateStatus/{id}")]
        // public async Task<IActionResult> UpdateOrderItemStatus(string id, [FromBody] UpdateOrderItemStatusRequest request)
        // {
        //     // Fetch the existing OrderItem
        //     var existingOrderItem = await _orderItemRepository.GetOrderItemByIdAsync(id);
        //     if (existingOrderItem == null)
        //     {
        //         return NotFound($"OrderItem with ID {id} not found.");
        //     }

        //     // Fetch the order associated with this OrderItem
        //     var order = await _orderRepository.GetOrderByOrderIdAsync(existingOrderItem.OrderId);
        //     if (order == null)
        //     {
        //         return NotFound($"Order with ID {existingOrderItem.OrderId} not found.");
        //     }

        //     // Allow update only if the order status is not 'Dispatched' or 'Delivered'
        //     if (order.Status == "Dispatched" || order.Status == "Delivered")
        //     {
        //         return BadRequest("Cannot update the OrderItem status as the associated order has already been dispatched or delivered.");
        //     }

        //     // Update the OrderItem status
        //     existingOrderItem.Status = request.NewStatus;
        //     existingOrderItem.UpdatedDate = DateTime.Now;

        //     // Update the status in the repository
        //     await _orderItemRepository.UpdateOrderItemStatusAsync(existingOrderItem.Id, existingOrderItem.Status);

        //     return Ok(existingOrderItem);
        // }

        // Delete an OrderItem by ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderItem(string id)
        {
            var existingOrderItem = await _orderItemRepository.GetOrderItemByIdAsync(id);
            if (existingOrderItem == null)
            {
                return NotFound($"OrderItem with ID {id} not found.");
            }

            await _orderItemRepository.DeleteOrderItemAsync(id);
            return Ok();
        }

        // Get an OrderItem by OrderId and ProductId
        [HttpGet("getItemByOrderProductIds/{orderId}/{productId}")]
        public async Task<IActionResult> GetOrderItem(string orderId, string productId)
        {
            var orderItem = await _orderItemRepository.GetOrderItemByProductIdAndOrderIdAsync(orderId, productId);

            if (orderItem == null)
            {
                return NotFound($"Order item with OrderId: {orderId} and ProductId: {productId} not found.");
            }

            return Ok(orderItem);
        }
    }
}
