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
            existingOrderItem.Status = request.NewStatus; // Assuming `NewStatus` is a property in the request
            existingOrderItem.UpdatedDate = DateTime.Now; // Set updated date to now

            // Update the status in the repository
            await _orderItemRepository.UpdateOrderItemStatusAsync(existingOrderItem.Id, existingOrderItem.Status); // Call repository method

            return Ok(existingOrderItem);
        }




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
    }
}
