using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharMarket.Helpers;
using PharMarket.Services;
using PharMarket.ViewModels.Store;

namespace PharMarket.Controllers;

[Authorize]
public class StoreController : BaseController
{
    private readonly IStoreService _storeService;

    public StoreController(IStoreService storeService)
    {
        _storeService = storeService;
    }

    public IActionResult Setup()
    {
        if (User.GetStoreId().HasValue)
            return RedirectToAction("Index", "Home");

        return View(new StoreViewModel
        {
            OwnerName = User.GetUserName()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Setup(StoreViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = User.GetUserId();
        var store = await _storeService.CreateStoreAsync(model, userId);

        var authService = HttpContext.RequestServices.GetRequiredService<IAuthService>();
        var user = await authService.GetUserByIdAsync(userId);

        if (user != null)
        {
            var token = authService.GenerateToken(user);
            Response.Cookies.Append("jwt_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddHours(8)
            });

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role),
                new("StoreId", store.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
                    AllowRefresh = true
                });
        }

        SetSuccessMessage($"Store '{store.Name}' created successfully!");
        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Settings()
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue)
            return RedirectToAction(nameof(Setup));

        var store = await _storeService.GetStoreByIdAsync(storeId.Value);
        if (store == null) return RedirectToAction(nameof(Setup));

        var model = new StoreViewModel
        {
            Id = store.Id,
            Name = store.Name,
            Address = store.Address,
            Description = store.Description,
            OwnerName = store.OwnerName,
            OwnerPosition = store.OwnerPosition,
            Phone = store.Phone,
            Email = store.Email
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(StoreViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        await _storeService.UpdateStoreAsync(model);
        SetSuccessMessage("Store settings updated.");
        return RedirectToAction(nameof(Settings));
    }
}
