/*
 * File: FCMTokenRepository.cs
 * Author: [​Thilakarathne S.P. ]

 * Description:
 *     This file contains the FCMTokenRepository class, which is responsible for 
 *     managing Firebase Cloud Messaging (FCM) tokens in a MongoDB database. 
 *     It provides methods to store, retrieve, and update FCM tokens associated 
 *     with users, specifically for roles such as Customer Service Representatives (CSRs) 
 *     and vendors.
 * 
 * Dependencies:
 *     - MongoDB.Driver: For interacting with MongoDB.
 *     - Microsoft.Extensions.Options: For accessing configuration settings.
 * 
 * Methods:
 *     - FCMTokenRepository: Constructor that initializes the MongoDB collection for 
 *       FCM tokens using the provided MongoDB settings.
 *     
 *     - StoreTokenAsync:
 *         Takes a user ID, FCM token, and role as parameters and stores the FCM token 
 *         in the database as a new FCMToken document.
 * 
 *     - GetTokenByUserIdAsync:
 *         Retrieves the FCM token associated with a given user ID. Returns the first 
 *         matching FCMToken or null if none is found.
 * 
 *     - UpdateTokenAsync:
 *         Updates the FCM token value and the timestamp of the token creation for 
 *         a given user ID. Uses the user ID to filter the FCM tokens.
 * 
 *     - GetCsrFcmTokensAsync:
 *         Retrieves a list of FCM tokens for all Customer Service Representatives (CSRs).
 *         Filters tokens based on the role "CSR" and returns a list of FCM token values.
 * 
 *     - GetVendorFcmTokenAsync:
 *         Retrieves a list of FCM tokens for a specific vendor based on their user ID. 
 *         Returns a list of FCM token values associated with the vendor ID.
 * 

 */

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

            // Filter tokens where the Role is "CSR"
            var filter = Builders<FCMToken>.Filter.Eq(t => t.Role, "CSR");

            var tokens = await _fcmTokens.Find(filter).ToListAsync();

            Console.WriteLine("Got CSR FCM tokens: " + tokens);

            // Return list of FCM token values
            return tokens.Select(t => t.FcmTokenValue).ToList();
        }


        // Repository method to get vendor FCM tokens by VendorId
        public async Task<List<string>> GetVendorFcmTokenAsync(string vendorId)
        {
            Console.WriteLine("Here the vendor id is: " + vendorId);
            var filter = Builders<FCMToken>.Filter.Eq(t => t.UserId, vendorId);
            var tokens = await _fcmTokens.Find(filter).ToListAsync();

            Console.WriteLine("Got Vendor FCM tokens: " + tokens);
            return tokens.Select(t => t.FcmTokenValue).ToList();
        }


    }
}