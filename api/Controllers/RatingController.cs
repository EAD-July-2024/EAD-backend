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
        private readonly FirebaseService _firebaseService;

        public RatingController(RatingRepository ratingRepository, UserRepository userRepository, FirebaseService firebaseService)
        {
            _ratingRepository = ratingRepository;
            _userRepository = userRepository;
            _firebaseService = firebaseService;
        }

        [HttpPost]
        public async Task<IActionResult> RateVendor([FromBody] RatingDTO model)
        {

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

            // **Invoke Firebase notification**
            // Send a notification to the vendor after the customer adds a rating/comment
            var notificationTitle = "New Rating Received!";
            var notificationBody = $" stars and a comment: ";

            // You would ideally store and retrieve the vendor's FCM token in the database
            // For now, assume that you have the vendor's FCM token.
            var vendorFcmToken =
            "dPLfW9Zu-8qXQsnYI17iuf:APA91bE0sxGeVD2lzoA9XDYlS3yiFc5lHbqDqQ4EKgi-eG-5en-Aztu1pYCre00-j2l76LP86J1Qu3PhrDDPBpALqT40OFZ89WvytCPBu-VuJY5O6TsgNDQhmIR5dyjl7rq0fPrqdHEr";  // Make sure this is stored in your ApplicationUser model

            if (!string.IsNullOrEmpty(vendorFcmToken))
            {
                Console.WriteLine("Sending notification to vendor...");
                await _firebaseService.SendNotificationAsync(vendorFcmToken, notificationTitle, notificationBody);
            }

            return Ok("Comment updated successfully.");
        }
    }
}