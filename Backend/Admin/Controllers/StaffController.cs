using System.Text.RegularExpressions;
using Admin.ViewModels;
using Core.Shared.Constants;
using Core.Shared.Entities;
using Core.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers;

public class StaffController : Controller
{
    private readonly IAuthService _authService;
    private readonly IWebHostEnvironment _env;

    // Regex dùng chung: > 8 ký tự, có chữ hoa, có số, có ký tự đặc biệt
    private static readonly Regex PasswordRegex =
        new(@"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{9,}$", RegexOptions.Compiled);

    private const string PasswordErrorMessage =
        "Mật khẩu phải dài hơn 8 ký tự, bao gồm chữ hoa, số và ký tự đặc biệt.";

    public StaffController(IAuthService authService, IWebHostEnvironment env)
    {
        _authService = authService;
        _env = env;
    }

    // ─── HELPER QUYỀN ────────────────────────────────────────────────────────

    private bool IsAdmin() =>
        HttpContext.Session.GetString("Role") == RoleConstants.Admin;

    private bool IsStaff() =>
        HttpContext.Session.GetString("Role") == RoleConstants.Staff;

    private IActionResult RequireAdmin()
    {
        if (!IsAdmin())
            return RedirectToAction("Login", "Account");
        return null!;
    }

    private IActionResult RequireNotStaff()
    {
        if (IsStaff())
            return RedirectToAction("Index", "Home");
        return null!;
    }

    // ─── INDEX ───────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Index(string? keyword)
    {
        var guard = RequireNotStaff(); if (guard != null) return guard;
        var guard2 = RequireAdmin(); if (guard2 != null) return guard2;

        IEnumerable<Account> accounts;

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var all = await _authService.GetAllStaffAsync();
            keyword = keyword.Trim().ToLower();
            accounts = all.Where(a =>
                a.Username.ToLower().Contains(keyword) ||
                (a.FullName?.ToLower().Contains(keyword) ?? false) ||
                (a.Email?.ToLower().Contains(keyword) ?? false));
        }
        else
        {
            accounts = await _authService.GetAllStaffAsync();
        }

        var viewModels = accounts.Select(a => new StaffViewModel
        {
            Username = a.Username,
            FullName = a.FullName ?? "",
            Email = a.Email ?? "",
            Role = a.Role ?? RoleConstants.Staff,
            AvatarUrl = a.AvatarUrl,
            CreatedAt = a.CreatedAt
        }).ToList();

        ViewBag.Keyword = keyword;
        return View(viewModels);
    }

    // ─── DETAILS ─────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Details(string username)
    {
        var guard = RequireNotStaff(); if (guard != null) return guard;
        var guard2 = RequireAdmin(); if (guard2 != null) return guard2;

        var account = await _authService.GetByUsernameAsync(username);
        if (account is null)
        {
            TempData["Error"] = MessageConstants.AccountNotFound;
            return RedirectToAction(nameof(Index));
        }

        var model = new StaffViewModel
        {
            Username = account.Username,
            FullName = account.FullName ?? "",
            Email = account.Email ?? "",
            Role = account.Role ?? RoleConstants.Staff,
            AvatarUrl = account.AvatarUrl,
            CreatedAt = account.CreatedAt
        };

        return View(model);
    }

    // ─── CREATE ──────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Create()
    {
        var guard = RequireNotStaff(); if (guard != null) return guard;
        var guard2 = RequireAdmin(); if (guard2 != null) return guard2;
        return View(new StaffViewModel { Role = RoleConstants.Staff });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StaffViewModel model)
    {
        var guard = RequireNotStaff(); if (guard != null) return guard;
        var guard2 = RequireAdmin(); if (guard2 != null) return guard2;

        // ── Validate Password: bắt buộc khi Create ────────────────────
        if (string.IsNullOrWhiteSpace(model.Password))
            ModelState.AddModelError(nameof(model.Password), PasswordErrorMessage);
        else if (!PasswordRegex.IsMatch(model.Password))
            ModelState.AddModelError(nameof(model.Password), PasswordErrorMessage);
        // ──────────────────────────────────────────────────────────────

        // ── Validate AvatarFile: bắt buộc khi Create ──────────────────
        if (model.AvatarFile == null || model.AvatarFile.Length == 0)
            ModelState.AddModelError(nameof(model.AvatarFile),
                "Vui lòng tải lên ảnh đại diện nhân viên.");
        // ──────────────────────────────────────────────────────────────

        if (!ModelState.IsValid)
            return View(model);

        string? avatarUrl = await SaveAvatarAsync(model.AvatarFile);
        string username = model.Email.Trim().Split('@')[0];

        var account = new Account
        {
            Username = username,
            FullName = model.FullName.Trim(),
            Email = model.Email.Trim(),
            Role = model.Role,
            AvatarUrl = avatarUrl
        };

        var error = await _authService.CreateAccountAsync(account, model.Password!);
        if (error != null)
        {
            ModelState.AddModelError(string.Empty, error);
            return View(model);
        }

        TempData["Success"] = MessageConstants.CreateSuccess;
        return RedirectToAction(nameof(Index));
    }

    // ─── EDIT ────────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Edit(string username)
    {
        var guard = RequireNotStaff(); if (guard != null) return guard;
        var guard2 = RequireAdmin(); if (guard2 != null) return guard2;

        var account = await _authService.GetByUsernameAsync(username);
        if (account is null)
        {
            TempData["Error"] = MessageConstants.AccountNotFound;
            return RedirectToAction(nameof(Index));
        }

        var model = new StaffViewModel
        {
            Username = account.Username,
            FullName = account.FullName ?? "",
            Email = account.Email ?? "",
            Role = account.Role ?? RoleConstants.Staff,
            AvatarUrl = account.AvatarUrl,
            CreatedAt = account.CreatedAt
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string username, StaffViewModel model)
    {
        var guard = RequireNotStaff(); if (guard != null) return guard;
        var guard2 = RequireAdmin(); if (guard2 != null) return guard2;

        // ── Password không bắt buộc khi Edit ──────────────────────────
        // Nếu để trống → giữ nguyên mật khẩu cũ (không validate)
        // Nếu có nhập  → validate đúng format
        if (string.IsNullOrWhiteSpace(model.Password))
            ModelState.Remove(nameof(model.Password));
        else if (!PasswordRegex.IsMatch(model.Password))
            ModelState.AddModelError(nameof(model.Password), PasswordErrorMessage);
        // ──────────────────────────────────────────────────────────────

        // AvatarFile không bắt buộc khi Edit — giữ ảnh cũ nếu không upload mới

        if (!ModelState.IsValid)
            return View(model);

        string? avatarUrl = model.AvatarUrl;
        if (model.AvatarFile != null && model.AvatarFile.Length > 0)
            avatarUrl = await SaveAvatarAsync(model.AvatarFile);

        var account = new Account
        {
            Username = username,
            FullName = model.FullName.Trim(),
            Email = model.Email.Trim(),
            Role = model.Role,
            AvatarUrl = avatarUrl
        };

        var error = await _authService.UpdateAccountAsync(account, model.Password);
        if (error != null)
        {
            ModelState.AddModelError(string.Empty, error);
            return View(model);
        }

        TempData["Success"] = MessageConstants.UpdateSuccess;
        return RedirectToAction(nameof(Index));
    }

    // ─── DELETE ──────────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string username)
    {
        var guard = RequireNotStaff(); if (guard != null) return guard;
        var guard2 = RequireAdmin(); if (guard2 != null) return guard2;

        var error = await _authService.DeleteAccountAsync(username);

        if (error != null) TempData["Error"] = error;
        else TempData["Success"] = MessageConstants.DeleteSuccess;

        return RedirectToAction(nameof(Index));
    }

    // ─── PRIVATE HELPER ──────────────────────────────────────────────────────

    private async Task<string?> SaveAvatarAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0) return null;

        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "avatars");
        Directory.CreateDirectory(uploadDir);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadDir, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/avatars/{fileName}";
    }
}