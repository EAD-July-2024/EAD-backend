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
        private readonly FCMTokenRepository _fCMTokenRepository;

        private readonly FirebaseService _firebaseService;

        public OrderController(ProductRepository productRepository, OrderRepository orderRepository, OrderItemRepository orderItemRepository, FCMTokenRepository fCMTokenRepository, FirebaseService firebaseService)
        {
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _fCMTokenRepository = fCMTokenRepository;
            _firebaseService = firebaseService;
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
            // Set a stock threshold value
            const int stockThreshold = 10; // Example threshold

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

                // Check if the stock falls below the threshold and notify the vendor
                if (product.Quantity < stockThreshold)
                {
                    Console.WriteLine($"Stock for product {product.Name} has dropped below {stockThreshold}. Current stock: {product.Quantity}");
                    // Get the vendor's FCM token
                    var vendorFcmTokens = await _fCMTokenRepository.GetVendorFcmTokenAsync(product.VendorId);

                    if (vendorFcmTokens != null && vendorFcmTokens.Any())
                    {
                        Console.WriteLine("This if works");
                        // Send notification to the vendor
                        string notificationTitle = "Stock Alert";
                        string notificationBody = $"Stock for product {product.Name} has dropped below {stockThreshold}. Current stock: {product.Quantity}";

                        foreach (var token in vendorFcmTokens)
                        {
                            await _firebaseService.SendNotificationAsync(token, notificationTitle, notificationBody);
                        }
                    }
                }

                // Add product price to the order item for record keeping
                processedItems.Add(new OrderItem
                {
                    OrderId = customOrderId,
                    ProductId = item.ProductId,
                    ProductName = product.Name,
                    VendorId = product.VendorId,
                    Quantity = item.Quantity,
                    Price = product.Price,
                    Status = "Purchased",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                });
            }

    // Create the new order
    var order = new Order
    {
        OrderId = customOrderId,
        CustomerId = request.CustomerId,
        TotalPrice = totalPrice,
        Status = "Purchased",
        Note = "",
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
        public async Task<IActionResult> GetAllOrdersWithItems()
        {
            var orders = await _orderRepository.GetAllOrdersAsync();
            var ordersResponse = new List<object>();

            foreach (var order in orders)
            {
                var orderItems = await _orderItemRepository.GetOrderItemsByOrderIdAsync(order.OrderId);

                var orderResponse = new
                {
                    order.Id,
                    order.OrderId,
                    order.CustomerId,
                    order.TotalPrice,
                    order.Status,
                    order.Note,
                    order.CreatedDate,
                    order.UpdatedDate,
                    OrderItems = orderItems.Select(item => new
                    {
                        item.Id,
                        item.OrderId,
                        item.ProductId,
                        item.ProductName,
                        item.VendorId,
                        item.Quantity,
                        item.Price,
                        item.Status,
                        item.CreatedDate,
                        item.UpdatedDate
                    }).ToList()
                };

                ordersResponse.Add(orderResponse);
            }

            return Ok(ordersResponse);
        }

        // Get order by Order ID
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderByOrderIdWithItems(string orderId)
        {
            var order = await _orderRepository.GetOrderByOrderIdAsync(orderId);
            if (order == null)
            {
                return NotFound($"Order with ID {orderId} not found.");
            }

            var orderItems = await _orderItemRepository.GetOrderItemsByOrderIdAsync(orderId);

            var orderResponse = new
            {
                order.Id,
                order.OrderId,
                order.CustomerId,
                order.TotalPrice,
                order.Status,
                order.Note,
                order.CreatedDate,
                order.UpdatedDate,
                OrderItems = orderItems.Select(item => new
                {
                    item.Id,
                    item.OrderId,
                    item.ProductId,
                    item.ProductName,
                    item.VendorId,
                    item.Quantity,
                    item.Price,
                    item.Status,
                    item.CreatedDate,
                    item.UpdatedDate
                }).ToList()
            };

            return Ok(orderResponse);
        }


        // Get specific order details for a vendor
        [HttpGet("vendorSpecificOrder/{orderId}/{vendorId}")]
        public async Task<IActionResult> GetOrderDetailsForVendor([FromRoute] string vendorId, [FromRoute] string orderId)
        {
            // Fetch the order by orderId
            var order = await _orderRepository.GetOrderByOrderIdAsync(orderId);
            if (order == null)
            {
                return NotFound(new { message = $"Order with ID {orderId} not found." });
            }

            // Fetch the order items for the given order
            var orderItems = await _orderItemRepository.GetOrderItemsByOrderIdAsync(orderId);

            // Filter order items by vendorId (return only the items that belong to this vendor)
            var vendorOrderItems = orderItems.Where(item => item.VendorId == vendorId).ToList();
            if (!vendorOrderItems.Any())
            {
                return NotFound(new { message = $"No order items found for vendor {vendorId} in order {orderId}." });
            }

            // Build the response containing only the vendor's order items
            var orderResponse = new
            {
                order.Id,
                order.OrderId,
                order.CustomerId,
                order.TotalPrice,
                order.Status,
                order.Note,
                order.CreatedDate,
                order.UpdatedDate,
                OrderItems = vendorOrderItems.Select(item => new
                {
                    item.Id,
                    item.OrderId,
                    item.ProductId,
                    item.ProductName,
                    item.VendorId,
                    item.Quantity,
                    item.Price,
                    item.Status,
                    item.CreatedDate,
                    item.UpdatedDate
                }).ToList()
            };

            return Ok(orderResponse);
        }



        // Get order using Customer ID
        [HttpGet("getByCustomerId/{customerId}")]
        public async Task<IActionResult> GetOrdersByCustomerIdWithItems(string customerId)
        {
            // Fetch the orders by CustomerId
            var orders = await _orderRepository.GetOrdersByCustomerAsync(customerId);
            if (orders == null || orders.Count == 0)
            {
                return NotFound($"No orders found for Customer ID {customerId}.");
            }

            // Prepare the list to hold the final response
            var ordersResponse = new List<object>();

            // Fetch the corresponding order items for each order
            foreach (var order in orders)
            {
                var orderItems = await _orderItemRepository.GetOrderItemsByOrderIdAsync(order.OrderId);

                // Construct the response for each order
                var orderResponse = new
                {
                    order.Id,
                    order.OrderId,
                    order.CustomerId,
                    order.TotalPrice,
                    order.Status,
                    order.Note,
                    order.CreatedDate,
                    order.UpdatedDate,
                    OrderItems = orderItems.Select(item => new
                    {
                        item.Id,
                        item.OrderId,
                        item.ProductId,
                        item.ProductName,
                        item.VendorId,
                        item.Quantity,
                        item.Price,
                        item.Status,
                        item.CreatedDate,
                        item.UpdatedDate
                    }).ToList()
                };

                ordersResponse.Add(orderResponse);
            }

            return Ok(ordersResponse);
        }



        // Update an existing order
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

            // Update the status if changed
            if (!string.IsNullOrEmpty(request.NewStatus))
            {
                existingOrder.Status = request.NewStatus;
            }

            // Update the note if present in the request
            if (!string.IsNullOrEmpty(request.Note))
            {
                existingOrder.Note = request.Note;
            }

            // Update the UpdatedDate in memory
            existingOrder.UpdatedDate = DateTime.Now;

            // Save the updated order back to the repository
            await _orderRepository.UpdateOrderStatusAsync(existingOrder);

            return Ok(existingOrder);
        }

        // // Update order status
        // [HttpPatch("updateStatus/{orderId}")]
        // public async Task<IActionResult> UpdateOrderStatus(string orderId, [FromBody] UpdateStatusRequest request)
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
        //         return BadRequest("Cannot update the order status as it has already been dispatched or delivered.");
        //     }

        //     // Update the order status in the repository
        //     await _orderRepository.UpdateOrderStatusAsync(orderId, request.NewStatus); // Pass orderId and NewStatus directly

        //     // Update the UpdatedDate in memory (if needed)
        //     existingOrder.UpdatedDate = DateTime.Now;

        //     return Ok(existingOrder); // Return the updated order if necessary
        // }
    }
}
