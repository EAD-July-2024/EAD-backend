/*
 * File: Rating.cs
 * Author: [Your Name]
 * Date: [Date]
 * Description:
 *     This file defines the Rating class, which represents a rating given by a customer 
 *     to a vendor in the e-commerce platform. The rating includes a star value, an optional 
 *     comment, and timestamps to track the creation and modification of the rating.
 * 
 * Fields:
 *     - Id (ObjectId):
 *         The unique identifier for the rating, stored as an ObjectId in MongoDB. 
 *         This field is marked with [BsonId] to indicate it's the primary key.
 * 
 *     - CustomerId (string):
 *         The ID of the customer who made the rating. This links the rating to the customer 
 *         in the system and enables tracking of ratings per customer.
 * 
 *     - VendorId (string):
 *         The ID of the vendor being rated. This links the rating to the specific vendor, 
 *         facilitating vendor rating management.
 * 
 *     - Stars (int):
 *         The rating value given by the customer, typically ranging from 1 to 5. 
 *         This field represents the quality of service or product from the vendor.
 * 
 *     - Comment (string):
 *         A textual comment provided by the customer regarding their experience with 
 *         the vendor. This field is mandatory and adds context to the rating.
 * 
 *     - DateCreated (DateTime):
 *         The timestamp of when the rating was created. This field is set to the current 
 *         UTC time when the rating is first submitted.
 * 
 *     - IsModified (bool):
 *         A flag indicating whether the rating has been modified after its initial creation. 
 *         This helps in tracking changes to ratings over time.
 * 
 *     - DateModified (DateTime?):
 *         An optional timestamp for when the rating was last modified. This field is 
 *         updated only if IsModified is true, providing a record of when changes were made.
 * 

 * 
 * Usage:
 *     This class is used to represent customer ratings for vendors in the e-commerce platform. 
 *     It allows customers to provide feedback, which can be aggregated to calculate average 
 *     ratings for vendors and display comments for potential customers.
 * 

 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api.Models
{
    public class Rating
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string CustomerId { get; set; } = null!;  

        public string VendorId { get; set; } = null!;    

        public int Stars { get; set; }  // Rating out of 5

        public string Comment { get; set; } = null!;

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public bool IsModified { get; set; } = false;
        
        public DateTime? DateModified { get; set; } = null;
    }
}