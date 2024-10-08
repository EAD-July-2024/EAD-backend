namespace api.DTOs
{
    public class ProductWithDetailsDTO
    {
        public string Id { get; set; }
        public string ProductId { get; set; } = null!;
        public string Name { get; set; }
        public string? Description { get; set; }
        public float Price { get; set; }
        public int Quantity { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string VendorId { get; set; }
        public string VendorName { get; set; }
        public double VendorRating { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
        public bool IsDeleted { get; set; }

    }
}