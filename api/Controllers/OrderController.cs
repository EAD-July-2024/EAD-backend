using api.Models;
using api.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using api.DTOs;

namespace api.Controllers
{
    [Controller]
    [Route("api/order")]
    public class OrderController : Controller
    {
        private readonly ProductRepository _productRepository;
        private readonly OrderRepository _orderRepository;
        private readonly OrderItemRepository _orderItemRepository;

        public OrderController(ProductRepository productRepository, OrderRepository orderRepository, OrderItemRepository orderItemRepository)
        {
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
        }

        // Method to generate a unique Order ID
        // Method to generate a unique Order ID
        private async Task<string> GenerateUniqueOrderIdAsync()
        {
            var random = new Random();
            string customId;
            bool exists;

            do
            {
                customId = "O" + random.Next(0, 99999).ToString("D5");
                exists = await _orderRepository.GetExistingIdsAsync(customId); // Updated method name here
            }
            while (exists);

            return customId;
        }


        // Create a new order
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreationRequest request)
        {
            // Create custom OrderId
            string customOrderId = await GenerateUniqueOrderIdAsync();

            float totalPrice = 0;
            List<OrderItem> processedItems = new List<OrderItem>();

            foreach (var item in request.ProductList)
            {
                // Fetch product details by product ID
                var product = await _productRepository.GetByCustomIdAsync(item.ProductId);
                if (product == null)
                {
                    return NotFound($"Product with ID {item.ProductId} not found");
                }

                // Calculate the total price for each order item (product price * quantity)
                float itemPrice = float.Parse(product.Price);
                totalPrice += itemPrice * item.Quantity;

                // Add product price to the order item for record keeping
                processedItems.Add(new OrderItem
                {
                    OrderId = customOrderId,
                    ProductId = item.ProductId,
                    ProductName = product.Name,
                    VendorId = product.VendorID,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Status = "Pending",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                });
            }

            // Create the new order
            var order = new Order
            {
                OrderId = customOrderId,
                CustomerId = request.CustomerId, // Use the customer ID from the request
                TotalPrice = totalPrice,
                Status = "Pending",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            // Save order to the database
            await _orderRepository.CreateOrderAsync(order);

            // Assign the generated order ID to the order items and save them
            foreach (var processedItem in processedItems)
            {
                processedItem.OrderId = order.OrderId;
                await _orderItemRepository.CreateOrderItemAsync(processedItem);  // Save each item with the new Order ID
            }

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
