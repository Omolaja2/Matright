using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PharMarket.Data;
using PharMarket.Models.Entities;

namespace PharMarket.Services;

public interface IAuthService
{
    Task<User?> ValidateUserAsync(string email, string password);
    string GenerateToken(User user);
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> CreateUserAsync(string fullName, string email, string password, string role = "Apprentice", int? storeId = null);
    Task SeedAdminAsync();
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IPasswordEncryptionService _encryption;

    public AuthService(AppDbContext context, IConfiguration configuration, IPasswordEncryptionService encryption)
    {
        _context = context;
        _configuration = configuration;
        _encryption = encryption;
    }

    public async Task<User?> ValidateUserAsync(string email, string password)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive);
        if (user == null) return null;

        return VerifyPasswordHash(password, user.PasswordHash) ? user : null;
    }

    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetJwtKey()));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        if (user.StoreId.HasValue)
            claims.Add(new Claim("StoreId", user.StoreId.Value.ToString()));

        var token = new JwtSecurityToken(
            issuer: "PharMarket",
            audience: "PharMarket",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<User?> GetUserByIdAsync(int id)
        => await _context.Users.FindAsync(id);

    public async Task<User?> GetUserByEmailAsync(string email)
        => await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

    public async Task<User> CreateUserAsync(string fullName, string email, string password, string role = "Apprentice", int? storeId = null)
    {
        var user = new User
        {
            FullName = fullName,
            Email = email,
            PasswordHash = HashPassword(password),
            EncryptedPassword = _encryption.Encrypt(password),
            Role = role,
            StoreId = storeId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task SeedAdminAsync()
    {
        if (await _context.Users.AnyAsync()) return;

        var admin = new User
        {
            FullName = "Admin",
            Email = "admin@pharmarket.com",
            PasswordHash = HashPassword("Admin@123"),
            EncryptedPassword = _encryption.Encrypt("Admin@123"),
            Role = "Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(admin);
        await _context.SaveChangesAsync();
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "PharMarket_Salt_2026"));
        return Convert.ToBase64String(bytes);
    }

    private static bool VerifyPasswordHash(string password, string storedHash)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "PharMarket_Salt_2026"));
        return Convert.ToBase64String(bytes) == storedHash;
    }

    private string GetJwtKey()
    {
        var key = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(key))
            key = "PharMarket_SuperSecret_JWT_Key_2026_!@#$%^&*()_+PharmacyERP";
        return key;
    }
}
