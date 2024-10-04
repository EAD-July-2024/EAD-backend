using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace api.Controllers
{
    [Controller]
    public class OrderController : Controller
    {

        private readonly OrderRepository _orderRepository;
        private readonly ProductRepository _productRepository;

        public OrderController(OrderRepository orderRepository, ProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            
        }
        

        //create an order
        [HttpPost("api/order/create")]
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

        [HttpGet("api/order/get/{customerId}")]
        public async Task<List<Order>> GetOrdersByCustomer(string customerId)
        {
            return await _orderRepository.GetOrdersByCustomerAsync(customerId);
        }

        [HttpPost("api/order/update/{orderId}")]
        public async Task<IActionResult> UpdateOrderStatus(string orderId, [FromBody] string status)
        {
            await _orderRepository.UpdateOrderStatusAsync(ObjectId.Parse(orderId), status);
            return Ok();
        }
        
    }
}