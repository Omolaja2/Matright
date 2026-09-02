using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharMarket.Data;
using PharMarket.Exceptions;
using PharMarket.Helpers;
using PharMarket.Models.Entities;
using PharMarket.Services;
using PharMarket.ViewModels.Users;

namespace PharMarket.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : BaseController
{
    private readonly AppDbContext _context;
    private readonly IAuthService _authService;
    private readonly IEmailService _emailService;
    private readonly IPasswordEncryptionService _encryption;

    public UsersController(AppDbContext context, IAuthService authService, IEmailService emailService, IPasswordEncryptionService encryption)
    {
        _context = context;
        _authService = authService;
        _emailService = emailService;
        _encryption = encryption;
    }

    public async Task<IActionResult> Index()
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var users = await _context.Users
            .Where(u => u.StoreId == storeId.Value)
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserListItem
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                Position = u.Position,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt
            })
            .ToListAsync();

        return View(users);
    }

    public async Task<IActionResult> Details(int id)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var user = await _context.Users
            .Where(u => u.Id == id && u.StoreId == storeId.Value)
            .Select(u => new UserListItem
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                Position = u.Position,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt
            })
            .FirstOrDefaultAsync();

        if (user == null) throw new NotFoundException("User", id);

        return View(user);
    }

    public IActionResult Create()
    {
        return View(new CreateUserViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        if (!ModelState.IsValid) return View(model);

        var existing = await _authService.GetUserByEmailAsync(model.Email);
        if (existing != null)
        {
            ModelState.AddModelError("Email", "An account with this email already exists.");
            return View(model);
        }

        var password = GenerateRandomPassword();
        var user = await _authService.CreateUserAsync(model.FullName, model.Email, password, model.Role, storeId.Value);
        user.Position = model.Position;
        await _context.SaveChangesAsync();

        await _emailService.SendCredentialsAsync(model.Email, model.FullName, model.Email, password, model.Role);

        SetSuccessMessage($"Account created for {model.FullName}. Password: {password}");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var user = await _context.Users.FindAsync(id)
            ?? throw new NotFoundException("User", id);

        if (user.StoreId != storeId.Value) throw new NotFoundException("User", id);

        if (user.Role == "Admin" && user.Id == User.GetUserId())
        {
            SetErrorMessage("Cannot deactivate your own account.");
            return RedirectToAction(nameof(Index));
        }

        user.IsActive = !user.IsActive;
        await _context.SaveChangesAsync();

        SetSuccessMessage($"Account {(user.IsActive ? "activated" : "deactivated")} for {user.FullName}.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var user = await _context.Users.FindAsync(id)
            ?? throw new NotFoundException("User", id);

        if (user.StoreId != storeId.Value) throw new NotFoundException("User", id);

        if (user.Role == "Admin" && user.Id == User.GetUserId())
        {
            SetErrorMessage("Cannot delete your own account.");
            return RedirectToAction(nameof(Index));
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        SetSuccessMessage($"User {user.FullName} deleted.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(int id)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var user = await _context.Users.FindAsync(id)
            ?? throw new NotFoundException("User", id);

        if (user.StoreId != storeId.Value) throw new NotFoundException("User", id);

        var newPassword = GenerateRandomPassword();
        user.PasswordHash = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(newPassword + "PharMarket_Salt_2026")));
        user.EncryptedPassword = _encryption.Encrypt(newPassword);
        await _context.SaveChangesAsync();

        await _emailService.SendCredentialsAsync(user.Email, user.FullName, user.Email, newPassword, user.Role);

        SetSuccessMessage($"Password reset for {user.FullName}. New password: {newPassword}");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> RevealPassword(int id)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.StoreId == storeId.Value)
            ?? throw new NotFoundException("User", id);

        if (string.IsNullOrEmpty(user.EncryptedPassword))
        {
            return Json(new
            {
                success = false,
                message = "Password not available. Use Reset Password to generate a new one."
            });
        }

        return Json(new { success = true, password = _encryption.Decrypt(user.EncryptedPassword) });
    }

    private static string GenerateRandomPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 10).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
