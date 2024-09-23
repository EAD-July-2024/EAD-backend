using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace api.Models
{
    public class Product
    {
        public ObjectId Id { get; set; } 
        public string Name { get; set; } = null!;
        public string Description { get; set; }= null!;
        public string Price { get; set; } = null!;
        public string CategoryID { get; set; }= null!;
        public string VendorID { get; set; }= null!;
        public string ImageUrl { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}