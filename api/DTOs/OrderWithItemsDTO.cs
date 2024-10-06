using api.Models;

namespace api.DTOs
{
    public class OrderWithItemsDTO
    {
        public Order Order { get; set; } = null!;
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
