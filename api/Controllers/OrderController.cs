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

        // Method to generate a unique Order ID
        private async Task<string> GenerateUniqueOrderIdAsync()
        {
            var random = new Random();
            string customId;
            bool exists;

            do
            {
                customId = "O" + random.Next(0, 99999).ToString("D5");
                exists = await _orderRepository.getExistingIds(customId);
            }
            while (exists);

            return customId;
        }

        // Create a new order
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] List<OrderItem> orderItems)
        {
            decimal totalPrice = 0;

            foreach (var item in orderItems)
            {
                var product = await _productRepository.GetByCustomIdAsync(item.ProductId);
                if (product == null)
                {
                    return NotFound($"Product with ID {item.ProductId} not found");
                }

                item.Price = decimal.Parse(product.Price);
                totalPrice += item.Price * item.Quantity;
            }

            var order = new Order
            {
                Id = ObjectId.GenerateNewId(),
                OrderId = await GenerateUniqueOrderIdAsync(), // Generate a new custom OrderID
                Products = orderItems,  // Changed from Items to Products
                CustomerId = "C0001",   // Static customer ID for now (or you can make it dynamic)
                TotalPrice = totalPrice,
                Status = "Pending",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            await _orderRepository.CreateOrderAsync(order);
            return Ok(order);
        }

        // Get all orders
        [HttpGet("getAllOrders")]
        public async Task<List<Order>> GetAllOrders()
        {
            return await _orderRepository.GetAllOrdersAsync();
        }

        // Get order by custom Order ID
        [HttpGet("getByOrderId/{orderId}")]
        public async Task<IActionResult> GetOrderByOrderId(string orderId)
        {
            var order = await _orderRepository.GetOrderByOrderIdAsync(orderId);
            if (order == null)
            {
                return NotFound($"Order with ID {orderId} not found");
            }

            return Ok(order);
        }

        // Get order using Customer ID
        [HttpGet("getByCustomerId/{customerId}")]
        public async Task<List<Order>> GetOrdersByCustomer(string customerId)
        {
            return await _orderRepository.GetOrdersByCustomerAsync(customerId);
        }

        // Update order status using custom OrderId
        [HttpPatch("{orderId}")]
        public async Task<IActionResult> UpdateOrderStatus(string orderId, [FromBody] string status)
        {
            var order = await _orderRepository.GetOrderByOrderIdAsync(orderId);
            if (order == null)
            {
                return NotFound($"Order with custom ID {orderId} not found");
            }

            await _orderRepository.UpdateOrderStatusAsync(orderId, status);  // Update status based on OrderId
            return Ok();
        }

    }
}
