using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api.Models.DTO
{
    public class FCMToken
    {
        [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string UserId { get; set; } 
    public string FcmTokenValue { get; set; }
    public string Role { get; set; }
    public DateTime TokenCreatedAt { get; set; } = DateTime.Now;
    }
}