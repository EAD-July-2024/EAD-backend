using api.Models;

namespace api.DTOs
{
    public class OrderCreationRequest
    {
        public string CustomerId { get; set; } = null!;
        public List<OrderItem> ProductList { get; set; } = new();
    }
}