using Client.ViewModels;
using Core.Shared.Entities;
using Core.Shared.Interfaces;
using Core.Shared.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers;

public class AccountController : ClientBaseController
{
    private readonly IReaderService _readerService;
    private readonly IUnifiedAuthService _unifiedAuthService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IReaderService readerService,
        IUnifiedAuthService unifiedAuthService,
        IWebHostEnvironment webHostEnvironment,
        ILogger<AccountController> logger)
    {
        _readerService = readerService;
        _unifiedAuthService = unifiedAuthService;
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
    }

    // ------------------------------------------------------------------ //
    //  REGISTER
    // ------------------------------------------------------------------ //
    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        try
        {
            if (!ModelState.IsValid) return View(model);

            if (await _unifiedAuthService.EmailExistsAsync(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "Email này đã được đăng ký. Vui lòng dùng email khác.");
                return View(model);
            }

            string? avatarUrl = null;
            if (model.AvatarFile != null && model.AvatarFile.Length > 0)
                avatarUrl = await SaveAvatarAsync(model.AvatarFile, $"tmp_{DateTime.Now:yyMMddHHmmss}");

            var reader = new Reader
            {
                ReaderId = string.Empty, // service tự sinh RR + 5 số
                FullName = model.FullName.Trim(),
                Email = model.Email.Trim(),
                Phone = model.Phone?.Trim(),
                DoB = model.DoB.HasValue ? DateOnly.FromDateTime(model.DoB.Value) : null,
                Gender = model.Gender ?? "Nam",
                Address = model.Address?.Trim(),
                AvatarUrl = avatarUrl,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                CreatedAt = DateTime.Now
            };

            var error = await _readerService.CreateAsync(reader);
            if (error != null)
            {
                ModelState.AddModelError(string.Empty, error);
                return View(model);
            }

            _logger.LogInformation("Bạn đọc {ReaderId} đã đăng ký thành công", reader.ReaderId);
            TempData["SuccessMessage"] = $"Đăng ký thành công! Mã bạn đọc của bạn là: {reader.ReaderId}";
            return RedirectToAction(nameof(RegisterSuccess), new { readerId = reader.ReaderId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đăng ký bạn đọc");
            ModelState.AddModelError(string.Empty, $"Lỗi khi đăng ký: {ex.Message}");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult RegisterSuccess(string readerId)
    {
        ViewBag.ReaderId = readerId;
        return View();
    }

    // ------------------------------------------------------------------ //
    //  LOGIN
    // ------------------------------------------------------------------ //
    [HttpGet]
    public IActionResult Login()
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("ReaderId")))
            return RedirectToAction("Index", "Home");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(UnifiedLoginViewModel model)
    {
        try
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _unifiedAuthService.LoginAsync(model.Email, model.Password);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            if (result.UserType == "Reader")
            {
                HttpContext.Session.SetString("ReaderId", result.UserId);
                HttpContext.Session.SetString("ReaderName", result.UserName);
                HttpContext.Session.SetString("ReaderEmail", model.Email);
                if (!string.IsNullOrEmpty(result.AvatarUrl))
                    HttpContext.Session.SetString("ReaderAvatar", result.AvatarUrl);

                _logger.LogInformation("Reader {UserId} đã đăng nhập", result.UserId);
                return RedirectToAction("Index", "Home");
            }

            // Admin/Staff → redirect sang Admin app
            HttpContext.Session.SetString("AdminUsername", result.UserId);
            HttpContext.Session.SetString("AdminName", result.UserName);
            HttpContext.Session.SetString("AdminRole", result.UserType);
            return Redirect("https://localhost:5001/Home/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đăng nhập");
            ModelState.AddModelError(string.Empty, $"Lỗi khi đăng nhập: {ex.Message}");
            return View(model);
        }
    }

    // ------------------------------------------------------------------ //
    //  LOGOUT
    // ------------------------------------------------------------------ //
    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

    // ------------------------------------------------------------------ //
    //  PROFILE
    // ------------------------------------------------------------------ //
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var readerId = HttpContext.Session.GetString("ReaderId");
        if (string.IsNullOrEmpty(readerId)) return RedirectToAction(nameof(Login));

        var reader = await _readerService.GetByIdAsync(readerId);
        if (reader == null) return RedirectToAction("Index", "Home");

        var model = new ProfileViewModel
        {
            ReaderId = reader.ReaderId,
            FullName = reader.FullName,
            Email = reader.Email,
            Phone = reader.Phone,
            Gender = reader.Gender,
            DoB = reader.DoB,
            Address = reader.Address,
            AvatarUrl = reader.AvatarUrl,
            CreatedAt = reader.CreatedAt,
            TotalBorrow = reader.BorrowTickets?.Count ?? 0,
            BorrowingCount = reader.BorrowTickets?.Count(bt => bt.ReturnDate == null) ?? 0,
            OverdueCount = reader.BorrowTickets?.Count(bt => bt.ReturnDate == null && bt.DueDate < DateTime.Now) ?? 0,
            ReturnedCount = reader.BorrowTickets?.Count(bt => bt.ReturnDate != null) ?? 0,
            WishlistCount = 0
        };

        return View(model);
    }

    // ------------------------------------------------------------------ //
    //  EDIT PROFILE
    // ------------------------------------------------------------------ //
    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var readerId = HttpContext.Session.GetString("ReaderId");
        if (string.IsNullOrEmpty(readerId)) return RedirectToAction(nameof(Login));

        var reader = await _readerService.GetByIdAsync(readerId);
        if (reader == null) return RedirectToAction("Index", "Home");

        var model = new EditProfileViewModel
        {
            ReaderId = reader.ReaderId,
            FullName = reader.FullName,
            Email = reader.Email,
            Phone = reader.Phone,
            Gender = reader.Gender,
            DoB = reader.DoB.HasValue ? reader.DoB.Value.ToDateTime(TimeOnly.MinValue) : null,
            Address = reader.Address,
            AvatarUrl = reader.AvatarUrl
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditProfileViewModel model)
    {
        var readerId = HttpContext.Session.GetString("ReaderId");
        if (string.IsNullOrEmpty(readerId)) return RedirectToAction(nameof(Login));

        // Mật khẩu không bắt buộc khi edit
        ModelState.Remove(nameof(model.NewPassword));
        ModelState.Remove(nameof(model.ConfirmPassword));
        if (!ModelState.IsValid) return View(model);

        string? avatarUrl = model.AvatarUrl;
        if (model.AvatarFile != null && model.AvatarFile.Length > 0)
            avatarUrl = await SaveAvatarAsync(model.AvatarFile, readerId);

        var reader = new Reader
        {
            ReaderId = readerId,
            FullName = model.FullName.Trim(),
            Email = model.Email?.Trim(),
            Phone = model.Phone?.Trim(),
            Gender = model.Gender,
            DoB = model.DoB.HasValue ? DateOnly.FromDateTime(model.DoB.Value) : null,
            Address = model.Address?.Trim(),
            AvatarUrl = avatarUrl,
            // Hash mật khẩu mới nếu có nhập
            PasswordHash = string.IsNullOrWhiteSpace(model.NewPassword)
                ? null
                : BCrypt.Net.BCrypt.HashPassword(model.NewPassword)
        };

        var error = await _readerService.UpdateProfileAsync(reader);
        if (error != null)
        {
            ModelState.AddModelError(string.Empty, error);
            return View(model);
        }

        // Cập nhật lại session
        HttpContext.Session.SetString("ReaderName", reader.FullName);
        if (avatarUrl != null)
            HttpContext.Session.SetString("ReaderAvatar", avatarUrl);

        TempData["Success"] = "Cập nhật thông tin thành công.";
        return RedirectToAction(nameof(Profile));
    }

    // ------------------------------------------------------------------ //
    //  DELETE ACCOUNT
    // ------------------------------------------------------------------ //
    [HttpGet]
    public async Task<IActionResult> DeleteAccount()
    {
        var readerId = HttpContext.Session.GetString("ReaderId");
        if (string.IsNullOrEmpty(readerId)) return RedirectToAction(nameof(Login));

        var reader = await _readerService.GetByIdAsync(readerId);
        if (reader == null) return RedirectToAction("Index", "Home");

        ViewBag.FullName = reader.FullName;
        ViewBag.ReaderId = reader.ReaderId;
        ViewBag.HasActive = await _readerService.CountBorrowingAsync(readerId) > 0;
        return View();
    }

    [HttpPost, ActionName("DeleteAccount")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccountConfirmed()
    {
        var readerId = HttpContext.Session.GetString("ReaderId");
        if (string.IsNullOrEmpty(readerId)) return RedirectToAction(nameof(Login));

        var error = await _readerService.DeleteSelfAsync(readerId);
        if (error != null)
        {
            TempData["Error"] = error;
            return RedirectToAction(nameof(Profile));
        }

        HttpContext.Session.Clear();
        TempData["AccountDeleted"] = "Tài khoản của bạn đã được xóa thành công.";
        return RedirectToAction("Index", "Home");
    }

    // ------------------------------------------------------------------ //
    //  HELPER
    // ------------------------------------------------------------------ //
    private async Task<string?> SaveAvatarAsync(IFormFile file, string readerId)
    {
        try
        {
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!allowed.Contains(ext) || file.Length > 5 * 1024 * 1024) return null;

            var folder = Path.GetFullPath(Path.Combine(
                Directory.GetCurrentDirectory(), "../Core.Shared/Uploads/reader-avatars"));
            Directory.CreateDirectory(folder);

            var fileName = $"{readerId}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
            var filePath = Path.Combine(folder, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/reader-avatars/{fileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lưu ảnh đại diện");
            return null;
        }
    }
}
