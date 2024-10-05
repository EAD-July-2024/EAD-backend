using api.Models;

namespace api.DTOs
{
    public class UpdateStatusRequest
    {
        public string NewStatus { get; set; } = null!;
    }
}
