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

    public StaffController(IAuthService authService, IWebHostEnvironment env)
    {
        _authService = authService;
        _env = env;
    }

    // ------------------------------------------------------------------ //
    //  Middleware kiểm tra quyền (gọi ở đầu mỗi action cần Admin)
    // ------------------------------------------------------------------ //
    private bool IsAdmin() =>
        HttpContext.Session.GetString("Role") == RoleConstants.Admin;

    private IActionResult RequireAdmin()
    {
        if (!IsAdmin())
            return RedirectToAction("Login", "Account");
        return null!;
    }

    // ------------------------------------------------------------------ //
    //  INDEX - Danh sách tài khoản
    // ------------------------------------------------------------------ //

    // GET /Staff  hoặc  /Staff?keyword=...
    [HttpGet]
    public async Task<IActionResult> Index(string? keyword)
    {
        var guard = RequireAdmin(); if (guard != null) return guard;

        IEnumerable<Core.Shared.Entities.Account> accounts;

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            // Tìm kiếm — dùng GetAllStaffAsync rồi filter (hoặc bạn mở rộng IAuthService thêm SearchAsync)
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

    // ------------------------------------------------------------------ //
    //  DETAILS
    // ------------------------------------------------------------------ //

    // GET /Staff/Details/{username}
    [HttpGet]
    public async Task<IActionResult> Details(string username)
    {
        var guard = RequireAdmin(); if (guard != null) return guard;

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

    // ------------------------------------------------------------------ //
    //  CREATE
    // ------------------------------------------------------------------ //

    // GET /Staff/Create
    [HttpGet]
    public IActionResult Create()
    {
        var guard = RequireAdmin(); if (guard != null) return guard;
        return View(new StaffViewModel { Role = RoleConstants.Staff });
    }

    // POST /Staff/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StaffViewModel model)
    {
        var guard = RequireAdmin(); if (guard != null) return guard;

        // Bắt buộc nhập mật khẩu khi tạo mới
        if (string.IsNullOrWhiteSpace(model.Password))
            ModelState.AddModelError(nameof(model.Password), "Vui lòng nhập mật khẩu.");

        if (!ModelState.IsValid)
            return View(model);

        // Xử lý upload avatar
        string? avatarUrl = await SaveAvatarAsync(model.AvatarFile);

        // Tự động tạo Username từ Email (lấy phần trước dấu @)
        string username = model.Email.Trim().Split('@')[0];

        var account = new Core.Shared.Entities.Account
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

    // ------------------------------------------------------------------ //
    //  EDIT
    // ------------------------------------------------------------------ //

    // GET /Staff/Edit/{username}
    [HttpGet]
    public async Task<IActionResult> Edit(string username)
    {
        var guard = RequireAdmin(); if (guard != null) return guard;

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

    // POST /Staff/Edit/{username}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string username, StaffViewModel model)
    {
        var guard = RequireAdmin(); if (guard != null) return guard;

        // Bỏ validate Password (không bắt buộc khi Edit)
        ModelState.Remove(nameof(model.Password));

        if (!ModelState.IsValid)
            return View(model);

        // Xử lý avatar mới (nếu có upload)
        string? avatarUrl = model.AvatarUrl; // giữ ảnh cũ
        if (model.AvatarFile != null)
            avatarUrl = await SaveAvatarAsync(model.AvatarFile);

        var account = new Core.Shared.Entities.Account
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

    // ------------------------------------------------------------------ //
    //  DELETE
    // ------------------------------------------------------------------ //

    // POST /Staff/Delete/{username}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string username)
    {
        var guard = RequireAdmin(); if (guard != null) return guard;

        var error = await _authService.DeleteAccountAsync(username);

        if (error != null)
            TempData["Error"] = error;
        else
            TempData["Success"] = MessageConstants.DeleteSuccess;

        return RedirectToAction(nameof(Index));
    }

    // ------------------------------------------------------------------ //
    //  HELPER - Lưu file ảnh
    // ------------------------------------------------------------------ //

    private async Task<string?> SaveAvatarAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0) return null;

        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "avatars");
        Directory.CreateDirectory(uploadDir);

        // Tên file unique để tránh trùng
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadDir, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/avatars/{fileName}";
    }
}
