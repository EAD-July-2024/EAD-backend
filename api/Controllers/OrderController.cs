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

                // // Ensure the product price is valid before using it
                // if (!float.TryParse(product.Price.ToString(), out float itemPrice))
                // {
                //     return BadRequest($"Invalid price format for product ID {item.ProductId}");
                // }

                totalPrice += product.Price * item.Quantity;

                // Add product price to the order item for record keeping
                processedItems.Add(new OrderItem
                {
                    OrderId = customOrderId,
                    ProductId = item.ProductId,
                    ProductName = product.Name,
                    VendorId = product.VendorID,
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

        // // Update an entire order
        // [HttpPut("{orderId}")]
        // public async Task<IActionResult> UpdateOrder(string orderId, [FromBody] Order updatedOrder)
        // {
        //     // Fetch the existing order
        //     var existingOrder = await _orderRepository.GetOrderByOrderIdAsync(orderId);
        //     if (existingOrder == null)
        //     {
        //         return NotFound($"Order with ID {orderId} not found.");
        //     }

        //     // Check if the order status is not 'Dispatched' or 'Delivered'
        //     if (existingOrder.Status == "Dispatched" || existingOrder.Status == "Delivered")
        //     {
        //         return BadRequest("Cannot update the order as it has already been dispatched or delivered.");
        //     }

        //     // Set the OrderId to ensure it remains the same
        //     updatedOrder.OrderId = orderId;
        //     updatedOrder.CreatedDate = existingOrder.CreatedDate; // Preserve the original created date
        //     updatedOrder.UpdatedDate = DateTime.Now; // Set updated date to now

        //     // Update the order in the repository
        //     await _orderRepository.UpdateOrderAsync(updatedOrder);
        //     return Ok(updatedOrder);
        // }

        // Update only the total price of an order
        [HttpPut("{orderId}")]
        public async Task<IActionResult> UpdateOrder(string orderId, [FromBody] UpdateOrderRequest request)
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
                return BadRequest("Cannot update the total price as the order has already been dispatched or delivered.");
            }

            // Initialize total price
            float totalPrice = 0;

            // Update order items based on the product list
            foreach (var item in request.ProductList)
            {
                // Fetch product details by product ID
                var product = await _productRepository.GetByCustomIdAsync(item.ProductId);
                if (product == null)
                {
                    return NotFound($"Product with ID {item.ProductId} not found");
                }

                // // Ensure the product price is valid before parsing
                // if (!float.TryParse(product.Price, out float itemPrice))
                // {
                //     return BadRequest($"Invalid price format for product ID {item.ProductId}");
                // }


                totalPrice += product.Price * item.Quantity;

                // Update the corresponding OrderItem entry
                var existingOrderItem = await _orderItemRepository.GetOrderItemByIdAsync(item.ProductId); // Assuming productId maps to orderItem
                if (existingOrderItem != null)
                {
                    existingOrderItem.Quantity = item.Quantity; // Update quantity
                    existingOrderItem.Price = product.Price; // Update price if necessary
                    existingOrderItem.UpdatedDate = DateTime.Now; // Update date
                    await _orderItemRepository.UpdateOrderItemAsync(existingOrderItem); // Save changes
                }
                else
                {
                    // If order item doesn't exist, you might want to handle this case appropriately
                    return NotFound($"OrderItem with Product ID {item.ProductId} not found in the order.");
                }
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
