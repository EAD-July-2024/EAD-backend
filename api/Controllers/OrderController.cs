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

                // Check if the requested quantity is available
                if (product.Quantity < item.Quantity)
                {
                    return BadRequest($"Insufficient stock for product ID {item.ProductId}. Available: {product.Quantity}, Requested: {item.Quantity}");
                }

                totalPrice += product.Price * item.Quantity;

                // Deduct the ordered quantity from the product's available stock
                product.Quantity -= item.Quantity;
                await _productRepository.UpdateQuantityAsync(product.ProductId, product.Quantity);


                // Add product price to the order item for record keeping
                processedItems.Add(new OrderItem
                {
                    OrderId = customOrderId,
                    ProductId = item.ProductId,
                    ProductName = product.Name,
                    VendorId = product.VendorId,
                    Quantity = item.Quantity,
                    Price = product.Price,
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
        [HttpGet]
        public async Task<List<Order>> GetAllOrders()
        {
            return await _orderRepository.GetAllOrdersAsync();
        }

        // Get order by custom Order ID
        [HttpGet("{orderId}")]
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

        [HttpPut("{orderId}")]
        public async Task<IActionResult> UpdateOrder(string orderId, [FromBody] UpdateOrderRequest updateOrderRequest)
        {
            // Validate the request
            if (updateOrderRequest?.ProductList == null || updateOrderRequest.ProductList.Count == 0)
            {
                return BadRequest("Product list cannot be null or empty.");
            }

            // Fetch the existing order
            var existingOrder = await _orderRepository.GetOrderByOrderIdAsync(orderId);
            if (existingOrder == null)
            {
                return NotFound($"Order with ID {orderId} not found.");
            }

            // Check if the order status is not 'Dispatched' or 'Delivered'
            if (existingOrder.Status == "Dispatched" || existingOrder.Status == "Delivered")
            {
                return BadRequest("Cannot update the total price as the order has already been dispatched or delivered.");
            }

            // Initialize total price
            float totalPrice = 0;

            foreach (var productUpdate in updateOrderRequest.ProductList)
            {
                // Fetch product details by product ID
                var product = await _productRepository.GetByCustomIdAsync(productUpdate.ProductId);
                if (product == null)
                {
                    return NotFound($"Product with ID {productUpdate.ProductId} not found.");
                }

                var existingOrderItem = await _orderItemRepository.GetOrderItemByProductIdAndOrderIdAsync(orderId, productUpdate.ProductId);
                if (existingOrderItem == null)
                {
                    return NotFound($"Order item with OrderId: {orderId} and ProductId: {productUpdate.ProductId} not found.");
                }

                // Check if the requested quantity is available in stock
                if (product.Quantity + existingOrderItem.Quantity < productUpdate.Quantity) // Add back existing order item quantity before comparison
                {
                    return BadRequest($"Insufficient stock for product ID {productUpdate.ProductId}. Available: {product.Quantity + existingOrderItem.Quantity}, Requested: {productUpdate.Quantity}");
                }

                // Deduct the new quantity and restore the old quantity first
                product.Quantity += existingOrderItem.Quantity; // Restore old quantity back to stock
                product.Quantity -= productUpdate.Quantity; // Deduct the new quantity
                await _productRepository.UpdateQuantityAsync(product.ProductId, product.Quantity); // Save updated stock

                // Calculate the total price for the updated order
                totalPrice += product.Price * productUpdate.Quantity;

                // Update the order item
                existingOrderItem.Quantity = productUpdate.Quantity; // Update quantity
                existingOrderItem.Price = product.Price; // Update price
                existingOrderItem.UpdatedDate = DateTime.Now; // Update date
                await _orderItemRepository.UpdateOrderItemAsync(existingOrderItem); // Save changes to the order item
            }

            // Update the total price in the existing order
            existingOrder.TotalPrice = totalPrice;
            existingOrder.UpdatedDate = DateTime.Now; // Update the date to now

            // Save the updated order in the repository
            await _orderRepository.UpdateOrderTotalPriceAsync(orderId, totalPrice);

            return Ok(existingOrder);
        }


        // Update order status
        [HttpPatch("updateStatus/{orderId}")]
        public async Task<IActionResult> UpdateOrderStatus(string orderId, [FromBody] UpdateStatusRequest request)
        {
            // Fetch the existing order
            var existingOrder = await _orderRepository.GetOrderByOrderIdAsync(orderId);
            if (existingOrder == null)
            {
                return NotFound($"Order with ID {orderId} not found.");
            }

            // Check if the order status is not 'Dispatched' or 'Delivered'
            if (existingOrder.Status == "Dispatched" || existingOrder.Status == "Delivered")
            {
                return BadRequest("Cannot update the order status as it has already been dispatched or delivered.");
            }

            // Update the order status in the repository
            await _orderRepository.UpdateOrderStatusAsync(orderId, request.NewStatus); // Pass orderId and NewStatus directly

            // Update the UpdatedDate in memory (if needed)
            existingOrder.UpdatedDate = DateTime.Now;

            return Ok(existingOrder); // Return the updated order if necessary
        }
    }
}
