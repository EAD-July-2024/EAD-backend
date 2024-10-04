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

        public AuthController(UserRepository userRepository, JWTService jwtService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
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
            return Ok(new { Token = token, Role = user.Role, Email = user.Email, Name = user.FullName });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
            var user = new ApplicationUser
            {
                Email = model.Email,
                FullName = model.FullName,
                PasswordHash = hashedPassword,
                Role = model.Role
            };

            await _userRepository.CreateAsync(user);
            await _userRepository.NotifyCSR(); // Notify CSR on new account
            return Ok("User created. Pending approval from CSR.");
        }

    }
}