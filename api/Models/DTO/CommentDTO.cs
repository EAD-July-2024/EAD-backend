using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models.DTO
{
    public class CommentDTO
    {
         public string CustomerId { get; set; } = null!;
            public string VendorId { get; set; } = null!;
        public string NewComment { get; set; } = null!;

    }
}