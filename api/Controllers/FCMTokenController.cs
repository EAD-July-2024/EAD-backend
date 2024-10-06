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