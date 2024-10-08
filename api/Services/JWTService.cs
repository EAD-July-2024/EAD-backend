/*
 * File: JWTService.cs
 * Author: [Thilakarathne S.P.]

 * Description: 
 *     This file contains the JWTService class, which is responsible for generating 
 *     JSON Web Tokens (JWT) for user authentication in the API. It utilizes the 
 *     Microsoft IdentityModel library to create secure tokens that can be used 
 *     for user validation and role-based access control.
 * 
 * Dependencies:
 *     - Microsoft.IdentityModel.Tokens: Provides the necessary classes for creating 
 *       and validating JWT tokens.
 *     - IConfiguration: Used to retrieve configuration settings from the app settings.
 *     - ApplicationUser: Represents the user model with properties like Email and Role.
 * 
 * Methods:
 *     - JWTService: Constructor that initializes the service with configuration settings.
 *     - GenerateJwtToken: 
 *         Generates a JWT token for the provided ApplicationUser. The token includes
 *         claims such as the user's email, role, and a unique identifier (JTI).
 *         The token is signed with a symmetric security key and can be configured 
 *         for expiration time and issuer details from the configuration.
 * 

 */

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