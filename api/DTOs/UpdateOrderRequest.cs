using api.Models;

namespace api.DTOs
{
    public class UpdateOrderRequest
    {
        // public string CustomerId { get; set; }
        public List<ProductUpdateRequest> ProductList { get; set; }
    }
}