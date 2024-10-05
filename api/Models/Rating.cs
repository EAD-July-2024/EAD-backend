using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api.Models
{
    public class Rating
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string CustomerId { get; set; } = null!;  

        public string VendorId { get; set; } = null!;    

        public int Stars { get; set; }  // Rating out of 5

        public string Comment { get; set; } = null!;

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public bool IsModified { get; set; } = false;
        
        public DateTime? DateModified { get; set; } = null;
    }
}