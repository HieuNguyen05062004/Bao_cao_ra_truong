using Admin.ViewModels;
using Core.Shared.Constants;
using Core.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    // GET /Account/Login
    [HttpGet]
    public IActionResult Login()
    {
        // Nếu đã đăng nhập → về Dashboard
        if (HttpContext.Session.GetString("Username") != null)
            return RedirectToAction("Index", "Home");

        return View();
    }

    // POST /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var account = await _authService.LoginAsync(model.Username, model.Password);

        if (account is null)
        {
            ModelState.AddModelError(string.Empty, MessageConstants.LoginFailed);
            return View(model);
        }

        // Chỉ cho phép Admin và Staff đăng nhập trang Admin
        if (!RoleConstants.IsValid(account.Role))
        {
            ModelState.AddModelError(string.Empty, MessageConstants.LoginNoPermission);
            return View(model);
        }

        // Lưu Session
        HttpContext.Session.SetString("Username", account.Username);
        HttpContext.Session.SetString("FullName", account.FullName ?? account.Username);
        HttpContext.Session.SetString("Role", account.Role ?? RoleConstants.Staff);
        HttpContext.Session.SetString("AvatarUrl", account.AvatarUrl ?? "");

        return RedirectToAction("Index", "Home");
    }

    // POST /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
