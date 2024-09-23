using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/vendor")]
    public class VendorController: Controller
    {
        private readonly MongoDBService _mongoDBService;

        public VendorController(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        [HttpGet]
        public async Task<List<Vendor>> Get()
        {
            return await _mongoDBService.GetVendorsAsync();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Vendor vendor)
        {
            await _mongoDBService.CreateVendorAsync(vendor);
            return CreatedAtAction(nameof(Get), new { id = vendor.Id }, vendor);
        }
        
    }
}