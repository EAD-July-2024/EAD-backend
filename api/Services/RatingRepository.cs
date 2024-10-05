using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace api.Services
{
    public class RatingRepository
    {
        private readonly IMongoCollection<Rating> _ratings;

        public RatingRepository(IOptions<MongoDBSettings> mongoDBSettings){

            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _ratings = database.GetCollection<Rating>("Ratings");
        }


        //Add rating and comment
        public async Task AddRatingAsync(Rating rating) => await _ratings.InsertOneAsync(rating);

        //Calculate average rating
        public async Task<double> CalculateAverageRating(string vendorId)
        {
            var ratings = await GetRatingsForVendorAsync(vendorId);
            if (!ratings.Any()) return 0.0;

            return ratings.Average(r => r.Stars);
        }

        public async Task<List<Rating>> GetRatingsForVendorAsync(string vendorId)
        {
            return await _ratings.Find(r => r.VendorId == vendorId).ToListAsync();
        }

        //Get ratings by customer and vendor
        public async Task<Rating?> GetRatingByCustomerAndVendorAsync(string customerId, string vendorId)
        {
            return await _ratings.Find(r => r.CustomerId == customerId && r.VendorId == vendorId).FirstOrDefaultAsync();
        }

        //Update comment modifications
        public async Task UpdateRatingAsync(Rating rating)
        {
            await _ratings.ReplaceOneAsync(r => r.Id == rating.Id, rating);
        }


    }
}