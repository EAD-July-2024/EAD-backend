using api.Models;
using api.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Controller]
    [Route("api/orderItem")]
    public class OrderItemController : Controller
    {
        private readonly OrderItemRepository _orderItemRepository;

        public OrderItemController(OrderItemRepository orderItemRepository)
        {
            _orderItemRepository = orderItemRepository;
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
            var existingOrderItem = await _orderItemRepository.GetOrderItemByIdAsync(id);
            if (existingOrderItem == null)
            {
                return NotFound($"OrderItem with ID {id} not found.");
            }

            updatedOrderItem.Id = id; // Ensure the ID is the same
            await _orderItemRepository.UpdateOrderItemAsync(updatedOrderItem);
            return Ok(updatedOrderItem);
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
