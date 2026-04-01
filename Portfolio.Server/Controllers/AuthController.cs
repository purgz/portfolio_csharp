using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Portfolio.Server.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Portfolio.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _db.AdminUsers
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        var hash = user?.PasswordHash ?? BCrypt.Net.BCrypt.HashPassword("dummy");
        var validPassword = BCrypt.Net.BCrypt.Verify(request.Password, hash);

        if (user == null || !validPassword)
            return Unauthorized(new { message = "Invalid credentials" });

        return Ok(new { token = GenerateJwt() });
    }

    private string GenerateJwt()
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));

        var token = new JwtSecurityToken(
            claims: new[] { new Claim("role", "admin") },
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record LoginRequest(string Username, string Password);