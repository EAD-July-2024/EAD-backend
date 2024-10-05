using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Services;
using Microsoft.AspNetCore.Mvc;
using api.Models;
using api.Models.DTO;
using DnsClient.Protocol;

namespace api.Controllers
{
    [Controller]
    [Route("api/rating")]
    public class RatingController : Controller
    {

        private readonly RatingRepository _ratingRepository;
        private readonly UserRepository _userRepository;

        public RatingController(RatingRepository ratingRepository, UserRepository userRepository )
        {
            _ratingRepository = ratingRepository;
            _userRepository = userRepository;
        }

        [HttpPost]
        public async Task<IActionResult> RateVendor([FromBody] RatingDTO model){

            var vendor = await _userRepository.GetUserByIdAsync(model.VendorId);

            if (vendor == null || vendor.Role != "Vendor")
            {
                return BadRequest("Vendor not found or invalid.");
            }

            // Check if rating already exists
            var existingRating = await _ratingRepository.GetRatingByCustomerAndVendorAsync(model.CustomerId, model.VendorId);

            if (existingRating != null)
            {
                // Allow comment update but prevent ranking change
                existingRating.Comment = model.Comment;
                existingRating.IsModified = true;
                existingRating.DateModified = DateTime.UtcNow;

                await _ratingRepository.UpdateRatingAsync(existingRating);
            }
            else
            {
                // Create new rating if none exists
                var rating = new Rating
                {
                    CustomerId = model.CustomerId,
                    VendorId = model.VendorId,
                    Stars = model.Stars,
                    Comment = model.Comment
                };

                await _ratingRepository.AddRatingAsync(rating);

                vendor.Ratings.Add(rating);
            }

            // Update vendor's average rating
            vendor.AverageRating = await _ratingRepository.CalculateAverageRating(model.VendorId);
            
            await _userRepository.UpdateAsync(vendor);
    
            return Ok("Rating submitted successfully.");

        }

        //Update comment only
        [HttpPut("update-comment")]
        public async Task<IActionResult> UpdateComment([FromBody] CommentDTO model)
        {
            var rating = await _ratingRepository.GetRatingByCustomerAndVendorAsync(model.CustomerId, model.VendorId);

            if (rating == null)
            {
                return NotFound("Rating not found.");
            }

            // Update comment only
            rating.Comment = model.NewComment;
            rating.IsModified = true;
            rating.DateModified = DateTime.UtcNow;

            await _ratingRepository.UpdateRatingAsync(rating);

            return Ok("Comment updated successfully.");
        }

        
    }
}