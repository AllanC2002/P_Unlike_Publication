using Microsoft.AspNetCore.Mvc;
using UnlikeService.services;
using UnlikeService.connection;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace UnlikeService.controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UnlikeController : ControllerBase
    {
        private readonly IMongoDatabase _db;
        private readonly string _secretKey;

        public UnlikeController()
        {
            Console.WriteLine("Initializing UnlikeController");

            var mongoHost = Environment.GetEnvironmentVariable("MONGO_HOSTIP");
            var mongoPort = Environment.GetEnvironmentVariable("MONGO_PORT");
            var mongoUser = Environment.GetEnvironmentVariable("MONGO_USER");
            var mongoPass = Environment.GetEnvironmentVariable("MONGO_PASSWORD");
            var mongoDb = Environment.GetEnvironmentVariable("MONGO_DB");
            _secretKey = Environment.GetEnvironmentVariable("SECRET_KEY");

            Console.WriteLine($"MONGO_HOSTIP: {mongoHost}");
            Console.WriteLine($"MONGO_PORT: {mongoPort}");
            Console.WriteLine($"MONGO_USER: {mongoUser}");
            Console.WriteLine($"MONGO_PASSWORD: {mongoPass}");
            Console.WriteLine($"MONGO_DB: {mongoDb}");
            Console.WriteLine($"SECRET_KEY: {_secretKey}");

            if (string.IsNullOrEmpty(_secretKey))
            {
                throw new Exception("SECRET_KEY is not defined");
            }

            _db = Mongo.MongoConnection();
        }

        [HttpPost]
        public async Task<IActionResult> Unlike([FromBody] PublicationRequest request)
        {
            Console.WriteLine(">> endpoint /unlike called");

            var authHeader = Request.Headers["Authorization"].ToString();
            Console.WriteLine("authorization header: " + authHeader);

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                Console.WriteLine("Token missing or invalid");
                return Unauthorized(new { error = "token missing or invalid" });
            }

            var tokenStr = authHeader.Replace("Bearer ", "");
            Console.WriteLine("token extracted");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            try
            {
                
                Console.WriteLine($"Token completo: {tokenStr}");
                var jwt = new JwtSecurityToken(tokenStr);
                Console.WriteLine($"Token kid: {jwt.Header.Kid ?? "null"}");

                var token = tokenHandler.ValidateToken(tokenStr, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKeyResolver = (token, securityToken, kid, validationParameters) =>
                    {
                        // Ignore the kid in the secret
                        return new List<SecurityKey> { new SymmetricSecurityKey(key) };
                    }
                }, out _);

                var claimsIdentity = token.Identity as ClaimsIdentity;
                var userIdClaim = claimsIdentity?.FindFirst("user_id")?.Value;

                if (userIdClaim == null)
                {
                    Console.WriteLine("Failed to extract user_id");
                    return Unauthorized(new { error = "user_id not found in token" });
                }

                int userId = int.Parse(userIdClaim);
                Console.WriteLine("user_id: " + userId);

                var (message, code, err) = await Functions.UnlikePublication(_db, request.IdPublication, userId);

                if (err != null)
                {
                    Console.WriteLine("Error in unlike: " + err);
                    return StatusCode(code, new { error = err });
                }

                Console.WriteLine(message);
                return StatusCode(code, new { message = message });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error to validate token: " + ex.Message);
                return Unauthorized(new { error = "invalid or expired token" });
            }
        }
    }

    public class PublicationRequest
    {
        public required string IdPublication { get; set; }
    }
}
