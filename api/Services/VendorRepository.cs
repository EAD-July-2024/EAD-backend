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
        private readonly IMongoCollection<Vendor> _vendors;
        
        public VendorRepository(IOptions<MongoDBSettings> mongoDBSettings)
        {
            MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            //_products = database.GetCollection<Product>(mongoDBSettings.Value.CollectionName);  
            _vendors = database.GetCollection<Vendor>("vendors");
        }

        public async Task CreateVendorAsync(Vendor vendor)
        {
            await _vendors.InsertOneAsync(vendor);
        }

        public async Task<List<Vendor>> GetVendorsAsync()
        {
            return await _vendors.Find(_ => true).ToListAsync();
        }
    }
}