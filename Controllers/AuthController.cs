using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Kanban.Api.Data;
using Kanban.Api.Models;

namespace Kanban.Api.Controllers;

// Les données reçues du front (on ne reçoit pas un User complet)
public record RegisterRequest(string Email, string Password, string Name);
public record LoginRequest(string Email, string Password);

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    // POST /api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        // email déjà pris ?
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest("Cet email est déjà utilisé.");

        var user = new User { Email = request.Email, Name = request.Name };

        // on hache le mot de passe avant de le stocker
        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, request.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Compte créé." });
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user is null)
            return Unauthorized("Email ou mot de passe incorrect.");

        // on compare le mot de passe tapé au haché stocké
        var hasher = new PasswordHasher<User>();
        var resultat = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (resultat == PasswordVerificationResult.Failed)
            return Unauthorized("Email ou mot de passe incorrect.");

        // identité OK → on fabrique le jeton
        var token = GenerateToken(user);
        return Ok(new { token, user = new { user.Id, user.Email, user.Name } });
    }

    private string GenerateToken(User user)
    {
        var jwtKey = _config["Jwt:Key"]!;
        var jwtIssuer = _config["Jwt:Issuer"]!;

        // les "claims" : les infos que le jeton transporte
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.Name),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),   // le jeton expire au bout de 2h
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}