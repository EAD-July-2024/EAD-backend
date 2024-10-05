using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models.DTO
{
    public class RatingDTO
    {
        public string CustomerId { get; set; } = null!;
    public string VendorId { get; set; } = null!;
    public int Stars { get; set; }  // Rating between 1 to 5
    public string Comment { get; set; } = null!;
    }
}