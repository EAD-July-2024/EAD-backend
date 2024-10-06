using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models.DTO;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using api.Models;
using MongoDB.Bson;

namespace api.Services
{
    public class FCMTokenRepository
    {
        private readonly IMongoCollection<FCMToken> _fcmTokens;

        public FCMTokenRepository(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _fcmTokens = database.GetCollection<FCMToken>("FCMTokens");
        }


        public async Task StoreTokenAsync(string userId, string token, string role)
        {
            var fcmToken = new FCMToken
            {
                UserId = userId,
                FcmTokenValue = token,
                Role = role
            };
    
            await _fcmTokens.InsertOneAsync(fcmToken);
        }
    
        public async Task<FCMToken> GetTokenByUserIdAsync(string userId)
        {
            return await _fcmTokens.Find(t => t.UserId == userId).FirstOrDefaultAsync();
        }
    
        public async Task UpdateTokenAsync(string userId, string token)
        {
            var filter = Builders<FCMToken>.Filter.Eq(t => t.UserId, userId);
            var update = Builders<FCMToken>.Update.Set(t => t.FcmTokenValue, token)
                                                  .Set(t => t.TokenCreatedAt, DateTime.Now);
            await _fcmTokens.UpdateOneAsync(filter, update);
        }


        // Method to get FCM tokens of all CSRs
        public async Task<List<string>> GetCsrFcmTokensAsync()
        {
            Console.WriteLine("Getting CSR FCM tokens");
            var filter = Builders<FCMToken>.Filter.Regex(t => t.UserId, new BsonRegularExpression("^CSR"));
            var tokens = await _fcmTokens.Find(filter).ToListAsync();
             Console.WriteLine("Got CSR FCM tokens: " + tokens);
            return tokens.Select(t => t.FcmTokenValue).ToList();  
        }
    }
}