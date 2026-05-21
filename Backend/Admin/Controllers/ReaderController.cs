using Admin.ViewModels;
using Core.Shared.Constants;
using Core.Shared.Entities;
using Core.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers;

public class ReaderController : Controller
{
    private readonly IReaderService _readerService;
    private readonly IWebHostEnvironment _env;

    public ReaderController(IReaderService readerService, IWebHostEnvironment env)
    {
        _readerService = readerService;
        _env = env;
    }

    // ------------------------------------------------------------------ //
    //  Kiểm tra đăng nhập
    // ------------------------------------------------------------------ //
    private bool IsLoggedIn() =>
        HttpContext.Session.GetString("Username") != null;

    private IActionResult RequireLogin()
    {
        if (!IsLoggedIn())
            return RedirectToAction("Login", "Account");
        return null!;
    }

    // ------------------------------------------------------------------ //
    //  INDEX
    // ------------------------------------------------------------------ //
    [HttpGet]
    public async Task<IActionResult> Index(string? keyword)
    {
        var guard = RequireLogin(); if (guard != null) return guard;

        var readers = string.IsNullOrWhiteSpace(keyword)
            ? await _readerService.GetAllAsync()
            : await _readerService.SearchAsync(keyword);

        var viewModels = new List<ReaderViewModel>();
        foreach (var r in readers)
        {
            viewModels.Add(new ReaderViewModel
            {
                ReaderId = r.ReaderId,
                FullName = r.FullName,
                DoB = r.DoB,
                Gender = r.Gender,
                Phone = r.Phone,
                Email = r.Email,
                AvatarUrl = r.AvatarUrl,
                BorrowingCount = await _readerService.CountBorrowingAsync(r.ReaderId),
                OverdueCount = await _readerService.CountOverdueAsync(r.ReaderId)
            });
        }

        ViewBag.Keyword = keyword;
        return View(viewModels);
    }

    // ------------------------------------------------------------------ //
    //  DETAILS
    // ------------------------------------------------------------------ //
    [HttpGet]
    public async Task<IActionResult> Details(string id, string filter = "all")
    {
        var guard = RequireLogin(); if (guard != null) return guard;

        var reader = await _readerService.GetByIdAsync(id);
        if (reader is null)
        {
            TempData["Error"] = "Bạn đọc không tồn tại.";
            return RedirectToAction(nameof(Index));
        }

        var now = DateTime.Now;
        var tickets = reader.BorrowTickets.AsEnumerable();

        tickets = filter switch
        {
            "pending" => tickets.Where(bt => bt.Status == "Chờ duyệt"),
            "approved" => tickets.Where(bt => bt.Status == "Đã duyệt"),
            "borrowing" => tickets.Where(bt => bt.Status == "Đang mượn"),
            "rejected" => tickets.Where(bt => bt.Status == "Bị từ chối"),
            "overdue" => tickets.Where(bt => bt.Status == "Đang mượn" && bt.DueDate < now),
            "returned" => tickets.Where(bt => bt.Status == "Đã trả"),
            _ => tickets
        };

        var model = new ReaderDetailViewModel
        {
            ReaderId = reader.ReaderId,
            FullName = reader.FullName,
            DoB = reader.DoB,
            Gender = reader.Gender,
            Address = reader.Address,
            Phone = reader.Phone,
            Email = reader.Email,
            AvatarUrl = reader.AvatarUrl,
            TotalBorrow = reader.BorrowTickets.Count,
            PendingCount = reader.BorrowTickets.Count(bt => bt.Status == "Chờ duyệt"),
            ApprovedCount = reader.BorrowTickets.Count(bt => bt.Status == "Đã duyệt"),
            BorrowingCount = reader.BorrowTickets.Count(bt => bt.Status == "Đang mượn"),
            RejectedCount = reader.BorrowTickets.Count(bt => bt.Status == "Bị từ chối"),
            OverdueCount = reader.BorrowTickets.Count(bt => bt.Status == "Đang mượn" && bt.DueDate < now),
            ReturnedCount = reader.BorrowTickets.Count(bt => bt.Status == "Đã trả"),
            BorrowTickets = tickets.OrderByDescending(bt => bt.BorrowDate).ToList(),
            Filter = filter
        };

        return View(model);
    }

    // ------------------------------------------------------------------ //
    //  CREATE
    // ------------------------------------------------------------------ //
    [HttpGet]
    public IActionResult Create()
    {
        var guard = RequireLogin(); if (guard != null) return guard;
        return View(new ReaderViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReaderViewModel model)
    {
        var guard = RequireLogin(); if (guard != null) return guard;

        // ReaderId không nhập tay nên không validate
        ModelState.Remove(nameof(model.ReaderId));
        if (!ModelState.IsValid) return View(model);

        string? avatarUrl = await SaveAvatarAsync(model.AvatarFile);

        var reader = new Reader
        {
            ReaderId = string.Empty,   // service sẽ tự sinh
            FullName = model.FullName.Trim(),
            DoB = model.DoB,
            Gender = model.Gender,
            Address = model.Address?.Trim(),
            Phone = model.Phone?.Trim(),
            Email = model.Email?.Trim(),
            AvatarUrl = avatarUrl,
            // Hash mật khẩu nếu admin nhập
            PasswordHash = string.IsNullOrWhiteSpace(model.Password)
                ? null
                : BCrypt.Net.BCrypt.HashPassword(model.Password)
        };

        var error = await _readerService.CreateAsync(reader);
        if (error != null)
        {
            ModelState.AddModelError(string.Empty, error);
            return View(model);
        }

        TempData["Success"] = $"Thêm bạn đọc thành công. Mã bạn đọc: {reader.ReaderId}";
        return RedirectToAction(nameof(Index));
    }

    // ------------------------------------------------------------------ //
    //  EDIT
    // ------------------------------------------------------------------ //
    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var guard = RequireLogin(); if (guard != null) return guard;

        var reader = await _readerService.GetByIdAsync(id);
        if (reader is null)
        {
            TempData["Error"] = "Bạn đọc không tồn tại.";
            return RedirectToAction(nameof(Index));
        }

        var model = new ReaderViewModel
        {
            ReaderId = reader.ReaderId,
            FullName = reader.FullName,
            DoB = reader.DoB,
            Gender = reader.Gender,
            Address = reader.Address,
            Phone = reader.Phone,
            Email = reader.Email,
            AvatarUrl = reader.AvatarUrl
            // Password để trống — chỉ cập nhật nếu admin nhập mới
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, ReaderViewModel model)
    {
        var guard = RequireLogin(); if (guard != null) return guard;

        ModelState.Remove(nameof(model.ReaderId));
        ModelState.Remove(nameof(model.Password)); // Password không bắt buộc khi edit
        if (!ModelState.IsValid) return View(model);

        string? avatarUrl = model.AvatarUrl;
        if (model.AvatarFile != null)
            avatarUrl = await SaveAvatarAsync(model.AvatarFile);

        var reader = new Reader
        {
            ReaderId = id,
            FullName = model.FullName.Trim(),
            DoB = model.DoB,
            Gender = model.Gender,
            Address = model.Address?.Trim(),
            Phone = model.Phone?.Trim(),
            Email = model.Email?.Trim(),
            AvatarUrl = avatarUrl,
            // Chỉ hash và cập nhật nếu admin nhập mật khẩu mới
            PasswordHash = string.IsNullOrWhiteSpace(model.Password)
                ? null
                : BCrypt.Net.BCrypt.HashPassword(model.Password)
        };

        var error = await _readerService.UpdateAsync(reader);
        if (error != null)
        {
            ModelState.AddModelError(string.Empty, error);
            return View(model);
        }

        TempData["Success"] = "Cập nhật bạn đọc thành công.";
        return RedirectToAction(nameof(Index));
    }

    // ------------------------------------------------------------------ //
    //  DELETE
    // ------------------------------------------------------------------ //
    [HttpGet]
    public async Task<IActionResult> Delete(string id)
    {
        var guard = RequireLogin(); if (guard != null) return guard;

        var reader = await _readerService.GetByIdAsync(id);
        if (reader is null)
        {
            TempData["Error"] = "Bạn đọc không tồn tại.";
            return RedirectToAction(nameof(Index));
        }

        var model = new ReaderViewModel
        {
            ReaderId = reader.ReaderId,
            FullName = reader.FullName,
            Gender = reader.Gender,
            Phone = reader.Phone,
            Email = reader.Email,
            AvatarUrl = reader.AvatarUrl,
            BorrowingCount = await _readerService.CountBorrowingAsync(reader.ReaderId)
        };

        return View(model);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var guard = RequireLogin(); if (guard != null) return guard;

        var error = await _readerService.DeleteAsync(id);

        if (error != null) TempData["Error"] = error;
        else TempData["Success"] = "Xóa bạn đọc thành công.";

        return RedirectToAction(nameof(Index));
    }

    // ------------------------------------------------------------------ //
    //  HELPER
    // ------------------------------------------------------------------ //
    private async Task<string?> SaveAvatarAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0) return null;

        var dir = Path.Combine(_env.WebRootPath, "uploads", "readers");
        Directory.CreateDirectory(dir);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(dir, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/readers/{fileName}";
    }
}
