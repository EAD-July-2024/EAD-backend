/*
 * File: FCMTokenController.cs
 * Author: [​Thilakarathne S.P. ]

 * Description:
 *     This file contains the FCMTokenController class, which manages the storage and 
 *     updating of Firebase Cloud Messaging (FCM) tokens for users in the E-commerce 
 *     system. It provides functionalities to store a new token or update an existing 
 *     token based on the user ID. The controller interacts with the FCMTokenRepository 
 *     to perform necessary database operations.
 * 
 * Methods:
 *     - StoreToken: Stores a new FCM token for a user or updates an existing token if 
 *                   one is already present.
 * 
 * Dependencies:
 *     - FCMTokenRepository: Used for accessing FCM token data from the database.
 * 
 * Data Models:
 *     - TokenRequest: Represents the request model for storing/updating FCM tokens, 
 *                     including the UserId, FCM token, and user role.
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
    [Route("api/fcm-token")]
    public class FCMTokenController: Controller
    {
        private readonly FCMTokenRepository _fcmTokenRepository;


        public FCMTokenController(FCMTokenRepository fcmTokenRepository)
        {
            _fcmTokenRepository = fcmTokenRepository;
        }

        [HttpPost("store")]
    public async Task<IActionResult> StoreToken([FromBody] TokenRequest request)
    {
        var existingToken = await _fcmTokenRepository.GetTokenByUserIdAsync(request.UserId);

        if (existingToken == null)
        {
            await _fcmTokenRepository.StoreTokenAsync(request.UserId, request.FcmToken, request.Role);
        }
        else
        {
            await _fcmTokenRepository.UpdateTokenAsync(request.UserId, request.FcmToken);
        }

        return Ok();
    }
    }

    public class TokenRequest
{
    public string UserId { get; set; }
    public string FcmToken { get; set; }

    public string Role { get; set; }
}
}