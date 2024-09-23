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
    public class OrderController : Controller
    {
        private readonly OrderRepository _orderRepository;

        public OrderController(OrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }


        [HttpPost("approve")]
        public async Task<IActionResult> ApproveOrder([FromBody] ObjectId orderId){
            await _orderRepository.UpdateOrderStatusAsync(orderId, "Approved");
            return Ok("Order approved.");
        }


        [HttpGet("customer/{customerId}")]
        public async Task<List<Order>> GetCustomerOrders(string customerId)
        {
            return await _orderRepository.GetOrdersByCustomerAsync(customerId);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            await _orderRepository.CreateOrderAsync(order);
            return Ok("Order Created");
        }
    }
}