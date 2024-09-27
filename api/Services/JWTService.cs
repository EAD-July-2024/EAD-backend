using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using api.Models;
using Microsoft.IdentityModel.Tokens;

namespace api.Services
{
    public class JWTService
    {
        private readonly IConfiguration _config;

    public JWTService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateJwtToken(ApplicationUser user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);


        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var payload = new JwtPayload(
            _config["Jwt:Issuer"],
            audience:null,
            claims,
            notBefore:null,
            expires:DateTime.Now.AddMinutes(double.Parse(_config["Jwt:ExpiresInMinutes"]))
        );

      Console.WriteLine(payload.Claims);
       

        var header = new JwtHeader(credentials);

        var token = new JwtSecurityToken(header: header,
            payload: payload
        );
        

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    }
}