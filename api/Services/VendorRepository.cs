using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace api.Services
{
    public class VendorRepository
    {
        private readonly IMongoCollection<ApplicationUser> _users;

        public VendorRepository(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
        var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
        _users = database.GetCollection<ApplicationUser>("Users");
        }

        // Get vendor details with ratings and customer details
        public async Task<ApplicationUser?> GetVendorWithRatingsAsync(string vendorId)
        {
            var filter = Builders<ApplicationUser>.Filter.Eq(u => u.UserId, vendorId) & Builders<ApplicationUser>.Filter.Eq(u => u.Role, "Vendor");
            var vendor = await _users.Find(filter).FirstOrDefaultAsync();
    
            if (vendor != null)
            {
                // Optionally, you can calculate the average rating again if needed:
                if (vendor.Ratings.Count > 0)
                {
                    vendor.AverageRating = vendor.Ratings.Average(r => r.Stars);
                }
            }
    
            return vendor;
        }
    
        // Get all vendors with their ratings and customer details
        public async Task<List<ApplicationUser>> GetAllVendorsWithRatingsAsync()
        {
            var filter = Builders<ApplicationUser>.Filter.Eq(u => u.Role, "Vendor");
            var vendors = await _users.Find(filter).ToListAsync();
    
            foreach (var vendor in vendors)
            {
                if (vendor.Ratings.Count > 0)
                {
                    vendor.AverageRating = vendor.Ratings.Average(r => r.Stars);
                }
            }
    
            return vendors;
        }
    }
}