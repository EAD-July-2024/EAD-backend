/*
 * File: VendorController.cs
 * Author: [Piyumantha W.U.]

 * Description: 
 *     This file contains the VendorController class, which manages vendor-related 
 *     operations in the E-commerce system, including retrieving individual vendor 
 *     details as well as fetching a list of all vendors. It includes vendor ratings 
 *     and customer information in the responses.
 * 
 * Methods:
 *     - GetVendor: Fetches the details of a specific vendor by vendorId, including their 
 *                  contact information, average rating, and associated customer ratings.
 *     - GetAllVendors: Retrieves a list of all vendors, each with their contact information, 
 *                      average rating, and associated customer ratings.
 * 
 * Dependencies:
 *     - VendorRepository: Used to access vendor data from the database, including vendor ratings.
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Services;
using Microsoft.AspNetCore.Mvc;

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
    }
}