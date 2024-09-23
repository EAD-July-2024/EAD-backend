using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class Vendor
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string ContactInfo { get; set; } = null!;
    }
}