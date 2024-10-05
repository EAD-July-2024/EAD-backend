using api.Models;

namespace api.DTOs
{
    public class UpdateOrderItemStatusRequest
    {
        public string NewStatus { get; set; } = null!;
    }
}
