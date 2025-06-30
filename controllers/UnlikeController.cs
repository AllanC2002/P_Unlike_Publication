using Microsoft.AspNetCore.Mvc;
using UnlikeService.Services;
using UnlikeService.Connection;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using MongoDB.Driver;

namespace UnlikeService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UnlikeController : ControllerBase
    {
        private readonly IMongoDatabase _db;
        private readonly string _secretKey;

        public UnlikeController()
        {
            _db = Mongo.MongoConnection();
            _secretKey = Environment.GetEnvironmentVariable("SECRET_KEY");
        }

        [HttpPost]
        public async Task<IActionResult> Unlike([FromBody] PublicationRequest request)
        {
            var authHeader = Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { error = "Token missing or invalid" });
            }

            var tokenStr = authHeader.Replace("Bearer ", "");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            try
            {
                var token = tokenHandler.ValidateToken(tokenStr, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out _);

                var claimsIdentity = token.Identity as ClaimsIdentity;
                var userIdClaim = claimsIdentity?.FindFirst("user_id")?.Value;

                if (userIdClaim == null)
                    return Unauthorized(new { error = "user_id not found in token" });

                int userId = int.Parse(userIdClaim);

                var (message, code, err) = await Functions.UnlikePublication(_db, request.IdPublication, userId);

                if (err != null)
                {
                    return StatusCode(code, new { error = err });
                }

                return StatusCode(code, new { message = message });
            }
            catch
            {
                return Unauthorized(new { error = "Invalid or expired token" });
            }
        }
    }

    public class PublicationRequest
    {
        public string IdPublication { get; set; }
    }
}
