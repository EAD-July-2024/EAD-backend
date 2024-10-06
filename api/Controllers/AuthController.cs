using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Controller]
    [Route("api/auth")]
    public class AuthController : Controller
    {
        private readonly UserRepository _userRepository;
        private readonly JWTService _jwtService;

        private readonly FCMTokenRepository _fCMTokenRepository;

        private readonly FirebaseService _firebaseService;

        public AuthController(UserRepository userRepository, JWTService jwtService, FCMTokenRepository fCMTokenRepository, FirebaseService firebaseService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _fCMTokenRepository = fCMTokenRepository;
            _firebaseService = firebaseService;
        }

        //Login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userRepository.GetByEmailAsync(model.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid credentials");
            }

            if (!user.IsApproved)
            {
                return Unauthorized("Your account is not yet approved by CSR.");
            }

            var token = _jwtService.GenerateJwtToken(user);
            return Ok(new { Token = token, Role = user.Role, Email = user.Email, Name = user.FullName, UserId = user.UserId });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
            var userId = await GenerateUniqueUserIdAsync(model.Role);
            var user = new ApplicationUser
            {
                Email = model.Email,
                UserId = userId,
                FullName = model.FullName,
                ContactInfo = model.ContactInfo,
                PasswordHash = hashedPassword,
                Role = model.Role
            };
            if(model.Role == "CSR") user.IsApproved = true;
            if(model.Role == "Vendor") user.IsApproved = true;
            if(model.Role == "Customer") user.IsApproved = false;

            await _userRepository.CreateAsync(user);
            if(user.Role == "CSR" ) return Ok("User created. User can now login.");
            if(user.Role == "Vendor" ) return Ok("User created. User can now login.");

            // After registering the customer, send notification to all CSRs
            var csrFcmTokens = await _fCMTokenRepository.GetCsrFcmTokensAsync();
            if (csrFcmTokens.Any())
        {
            Console.WriteLine("This if works");
            var notificationTitle = "New Customer Registration";
            var notificationBody = $"A new customer, {model.FullName}, has registered in the system.";

            await _firebaseService.SendNotificationToCsrAsync(csrFcmTokens, notificationTitle, notificationBody);
        }
            return Ok("User created. Pending approval from CSR.");
        }

        //Generate unique User Id
        private async Task<string> GenerateUniqueUserIdAsync(string role)
        {
            var random = new Random();
            string userId;
            bool exists;

            do
            {
                if (role == "Customer")
                {
                    userId = "CUS" + random.Next(0, 999999).ToString("D5");
                }
                else if (role == "Vendor")
                {
                    userId = "VEND" + random.Next(0, 999999).ToString("D5");
                }
                else
                {
                    userId = "CSR" + random.Next(0, 999999).ToString("D5");
                }

                
                exists = await _userRepository.getExistingUserIds(userId);

            } while (exists);

            return userId;
        }


        //Approve customer by CSR
        [HttpPut("approveCustomer/{userId}")]
        public async Task<IActionResult> ApproveCustomer(string userId)
        {
            
            var customer = await _userRepository.GetUserByIdAsync(userId);
            if (customer == null)
            {
                return NotFound(new { message = "Customer not found" });
            }

            
            var success = await _userRepository.ApproveCustomerAsync(userId);

            if (success)
            {
                return Ok(new { message = "Customer approved successfully" });
            }
            else
            {
                return BadRequest(new { message = "Failed to approve customer" });
            }
        }
    }
}