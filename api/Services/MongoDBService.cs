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
    public class MongoDBService
    {
        private readonly IMongoCollection<Product> _products;
        //private readonly IMongoCollection<Vendor> _vendors;

        public MongoDBService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _products = database.GetCollection<Product>(mongoDBSettings.Value.CollectionName);  
            //_vendors = database.GetCollection<Vendor>("vendors");
        }
        
        public async Task CreateAsync(Product product)
        {
            await _products.InsertOneAsync(product);
            return;
        }

        public async Task<List<Product>> GetAsync()
        {
            return await _products.Find(new BsonDocument()).ToListAsync();
        }

        
    }
}