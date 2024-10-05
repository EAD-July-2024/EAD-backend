using api.Models;

namespace api.DTOs
{
    public class ProductUpdateRequest
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }
}