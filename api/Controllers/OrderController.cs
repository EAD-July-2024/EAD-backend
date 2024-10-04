using api.Models;
using api.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace api.Controllers
{
    [Controller]
    [Route("api/order")]
    public class OrderController : Controller
    {

        private readonly OrderRepository _orderRepository;
        private readonly ProductRepository _productRepository;

        public OrderController(OrderRepository orderRepository, ProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;

        }


        // Create an order
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] List<OrderItem> orderItems)
        {
            decimal totalPrice = 0;

            foreach (var item in orderItems)
            {
                var product = await _productRepository.GetByCustomIdAsync(item.ProductCustomId);
                if (product == null)
                {
                    return NotFound($"Product with ID {item.ProductCustomId} not found");
                }

                item.Price = decimal.Parse(product.Price);
                totalPrice += item.Price * item.Quantity;
            }

            var order = new Order
            {
                Items = orderItems,
                CustomerId = "customer id",
                VendorId = "vendor id",
                TotalPrice = totalPrice
            };

            await _orderRepository.CreateOrderAsync(order);
            return Ok(order);
        }

        [HttpGet("getByCustomerId/{customerId}")]
        public async Task<List<Order>> GetOrdersByCustomer(string customerId)
        {
            return await _orderRepository.GetOrdersByCustomerAsync(customerId);
        }

        [HttpPatch("{orderId}")]
        public async Task<IActionResult> UpdateOrderStatus(string orderId, [FromBody] string status)
        {
            await _orderRepository.UpdateOrderStatusAsync(ObjectId.Parse(orderId), status);
            return Ok();
        }

    }
}