using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api.Models
{
    public class ApplicationUser
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string UserId { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = null!;

        public string ContactInfo { get; set; } = null!;
        public bool IsApproved { get; set; } = false;
        public List<Order> Orders { get; set; } = new List<Order>();

         //Properties for Vendor Ratings
        public List<Rating> Ratings { get; set; } = new List<Rating>();
        public double AverageRating { get; set; } = 0.0;
    }
}