using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Services;
using Microsoft.AspNetCore.Mvc;
using api.Models;

namespace api.Controllers
{
    [Controller]
    [Route("api/vendor")]
    public class VendorController : Controller
    {
        private readonly VendorRepository _vendorRepository;

        public VendorController(VendorRepository vendorRepository)
        {
            _vendorRepository = vendorRepository;
        }

        // Get a single vendor's details including ratings and customer information
        [HttpGet("vendorSingle/{vendorId}")]
        public async Task<IActionResult> GetVendor(string vendorId)
        {
            var vendor = await _vendorRepository.GetVendorWithRatingsAsync(vendorId);

            if (vendor == null)
            {
                return NotFound(new { message = "Vendor not found" });
            }

            var result = new
            {
                vendor.UserId,
                vendor.FullName,
                vendor.ContactInfo,
                vendor.AverageRating,
                Ratings = vendor.Ratings.Select(r => new
                {
                    r.Stars,
                    r.Comment,
                    r.DateCreated,
                    r.CustomerId,  
                    
                    r.IsModified,
                    r.DateModified
                }).ToList()
            };

            return Ok(result);
        }

        // Get all vendors with their details, ratings, and customer information
        [HttpGet("Allvendors")]
        public async Task<IActionResult> GetAllVendors()
        {
            var vendors = await _vendorRepository.GetAllVendorsWithRatingsAsync();

            var result = vendors.Select(vendor => new
            {
                vendor.UserId,
                vendor.FullName,
                vendor.ContactInfo,
                vendor.AverageRating,
                Ratings = vendor.Ratings.Select(r => new
                {
                    r.Stars,
                    r.Comment,
                    r.DateCreated,
                    r.CustomerId,  
                    
                    r.IsModified,
                    r.DateModified
                }).ToList()
            });

            return Ok(result);
        }


        [HttpPut("updateVendor/{vendorId}")]
        public async Task<IActionResult> UpdateVendorByVendorId([FromRoute] string vendorId, [FromBody] ApplicationUser updatedVendor)
        {
            // Call the repository to update the vendor
            var success = await _vendorRepository.UpdateVendorByVendorIdAsync(vendorId, updatedVendor);
        
            if (success)
            {
                return Ok(new { message = "Vendor updated successfully." });
            }
            else
            {
                return NotFound(new { message = "Vendor not found." });
            }
        }

    }
}