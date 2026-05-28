using Core.Shared.Entities;
using Core.Shared.Interfaces;
using Core.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthApiController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthApiController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AdminLoginRequest request)
    {
        var account = await _authService.LoginAsync(request.Username ?? string.Empty, request.Password ?? string.Empty);
        if (account == null)
            return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu." });

        HttpContext.Session.SetString("AdminUsername", account.Username);
        HttpContext.Session.SetString("AdminName", account.FullName ?? account.Username);
        HttpContext.Session.SetString("AdminRole", account.Role ?? "Admin");

        return Ok(new
        {
            userType = account.Role,
            userId = account.Username,
            userName = account.FullName
        });
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        var username = HttpContext.Session.GetString("AdminUsername");
        if (string.IsNullOrWhiteSpace(username))
            return Ok(new { isAuthenticated = false });

        var role = HttpContext.Session.GetString("AdminRole") ?? "Admin";
        return Ok(new
        {
            isAuthenticated = true,
            role = role,
            userType = role,
            userId = username,
            userName = HttpContext.Session.GetString("AdminName")
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok();
    }

    [HttpGet("password/{username}")]
    public async Task<IActionResult> GetPassword(string username)
    {
        // Chỉ Admin mới được xem mật khẩu
        var currentRole = HttpContext.Session.GetString("AdminRole");
        if (currentRole != "Admin")
            return Unauthorized(new { message = "Bạn không có quyền xem mật khẩu." });

        var account = await _authService.GetByUsernameAsync(username);
        if (account == null)
            return NotFound(new { message = "Tài khoản không tồn tại." });

        return Ok(new { password = account.Password, username = account.Username });
    }
}

[ApiController]
[Route("api/Book")]
public class BookApiController : ControllerBase
{
    private readonly IBookService _bookService;

    public BookApiController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet("All")]
    public async Task<IActionResult> All()
    {
        var books = await _bookService.GetAllBooksAsync();
        return Ok(books.Select(ToDto));
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] BookRequest request)
    {
        var (book, categoryIds) = await BuildBookAsync(request);
        book.BookId = null!;
        var (success, message) = await _bookService.AddBookAsync(book, categoryIds);
        if (!success) return BadRequest(new { message });

        var saved = await _bookService.GetBookByIdAsync(book.BookId);
        return Ok(ToDto(saved ?? book));
    }

    [HttpPut("Update")]
    public async Task<IActionResult> Update([FromBody] BookRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Id))
            return BadRequest(new { message = "Ma sach khong hop le." });

        var existing = await _bookService.GetBookByIdAsync(request.Id);
        if (existing == null)
            return NotFound(new { message = "Khong tim thay sach." });

        var (book, categoryIds) = await BuildBookAsync(request, existing.ImageUrl);
        var (success, message) = await _bookService.UpdateBookAsync(book, categoryIds);
        if (!success) return BadRequest(new { message });

        var saved = await _bookService.GetBookByIdAsync(book.BookId);
        return Ok(ToDto(saved ?? book));
    }

    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var (success, message) = await _bookService.DeleteBookAsync(id);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }

    private async Task<(Book Book, List<int> CategoryIds)> BuildBookAsync(BookRequest request, string? fallbackImage = null)
    {
        var categories = await _bookService.GetAllCategoriesAsync();
        var categoryIds = categories
            .Where(c => string.Equals(c.CategoryName, request.Category, StringComparison.OrdinalIgnoreCase))
            .Select(c => c.CategoryId)
            .ToList();

        if (!categoryIds.Any() && int.TryParse(request.Category, out var categoryId))
            categoryIds.Add(categoryId);

        return (new Book
        {
            BookId = request.Id,
            Title = request.Title?.Trim() ?? string.Empty,
            Author = request.Author?.Trim(),
            Publisher = request.Publisher?.Trim(),
            PublishYear = request.PublishYear,
            Quantity = request.Stock,
            Status = request.Stock > 0 ? "Có thể mượn" : "Hết sách",
            Description = request.Description?.Trim(),
            ImageUrl = SaveDataUrl(request.Img, "books", "/book-images") ?? fallbackImage ?? string.Empty,
            CreatedAt = DateTime.Now
        }, categoryIds);
    }

    private static object ToDto(Book book)
    {
        var categoryNames = book.BookCategories?
            .Select(bc => bc.Category?.CategoryName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Cast<string>()
            .ToList() ?? new List<string>();

        return new
        {
            id = book.BookId,
            title = book.Title,
            author = book.Author,
            publisher = book.Publisher,
            publishYear = book.PublishYear,
            stock = book.Quantity ?? 0,
            status = (book.Quantity ?? 0) > 0 ? "Còn sách" : "Hết sách",
            img = book.ImageUrl,
            description = book.Description,
            category = string.Join(", ", categoryNames),
            categoryIds = book.BookCategories?.Select(bc => bc.CategoryId).ToList() ?? new List<int>()
        };
    }

    private static string? SaveDataUrl(string? value, string folder, string requestPath)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            return value;

        var commaIndex = value.IndexOf(',');
        if (commaIndex < 0) return null;

        var header = value[..commaIndex];
        var extension = header.Contains("png", StringComparison.OrdinalIgnoreCase) ? ".png" :
            header.Contains("gif", StringComparison.OrdinalIgnoreCase) ? ".gif" : ".jpg";

        var uploadDir = Path.GetFullPath(Path.Combine(
            Directory.GetCurrentDirectory(), "..", "Core.Shared", "Uploads", folder));
        Directory.CreateDirectory(uploadDir);

        var fileName = $"{Guid.NewGuid()}{extension}";
        System.IO.File.WriteAllBytes(Path.Combine(uploadDir, fileName), Convert.FromBase64String(value[(commaIndex + 1)..]));
        return $"{requestPath}/{fileName}";
    }
}

[ApiController]
[Route("api/Category")]
public class CategoryApiController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryApiController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet("All")]
    public async Task<IActionResult> All()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        var result = new List<object>();
        foreach (var category in categories)
        {
            var detail = await _categoryService.GetCategoryWithBooksAsync(category.CategoryId);
            result.Add(new
            {
                id = category.CategoryId,
                name = category.CategoryName,
                bookCount = detail?.BookCategories.Count ?? 0
            });
        }

        return Ok(result);
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] CategoryRequest request)
    {
        var (success, message) = await _categoryService.AddCategoryAsync(new Category { CategoryName = request.Name?.Trim() ?? string.Empty });
        return success ? Ok(new { message }) : BadRequest(new { message });
    }

    [HttpPut("Update")]
    public async Task<IActionResult> Update([FromBody] CategoryRequest request)
    {
        var (success, message) = await _categoryService.UpdateCategoryAsync(new Category
        {
            CategoryId = request.Id,
            CategoryName = request.Name?.Trim() ?? string.Empty
        });
        return success ? Ok(new { message }) : BadRequest(new { message });
    }

    [HttpDelete("Delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _categoryService.DeleteCategoryAsync(id);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }
}

[ApiController]
[Route("api/Reader")]
public class ReaderApiController : ControllerBase
{
    private readonly IReaderService _readerService;

    public ReaderApiController(IReaderService readerService)
    {
        _readerService = readerService;
    }

    [HttpGet("All")]
    public async Task<IActionResult> All()
    {
        var readers = await _readerService.GetAllAsync();
        var result = new List<object>();
        foreach (var reader in readers)
        {
            result.Add(new
            {
                id = reader.ReaderId,
                name = reader.FullName,
                email = reader.Email,
                phone = reader.Phone,
                address = reader.Address,
                dob = reader.DoB?.ToString("yyyy-MM-dd"),
                gender = reader.Gender,
                img = reader.AvatarUrl,
                status = "Hoat dong",
                borrowingCount = await _readerService.CountBorrowingAsync(reader.ReaderId),
                overdueCount = await _readerService.CountOverdueAsync(reader.ReaderId)
            });
        }

        return Ok(result);
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] ReaderRequest request)
    {
        var reader = BuildReader(request);
        var error = await _readerService.CreateAsync(reader);
        return error == null ? Ok(new { id = reader.ReaderId }) : BadRequest(new { message = error });
    }

    [HttpPut("Update")]
    public async Task<IActionResult> Update([FromBody] ReaderRequest request)
    {
        var reader = BuildReader(request);
        var error = await _readerService.UpdateAsync(reader);
        return error == null ? Ok(new { id = reader.ReaderId }) : BadRequest(new { message = error });
    }

    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var error = await _readerService.DeleteAsync(id);
        return error == null ? Ok() : BadRequest(new { message = error });
    }

    private static Reader BuildReader(ReaderRequest request) => new()
    {
        ReaderId = request.Id ?? string.Empty,
        FullName = request.Name?.Trim() ?? string.Empty,
        Email = request.Email?.Trim(),
        Phone = request.Phone?.Trim(),
        Address = request.Address?.Trim(),
        Gender = request.Gender,
        DoB = DateOnly.TryParse(request.Dob, out var dob) ? dob : null,
        AvatarUrl = BookApiController_SaveDataUrl(request.Img, "reader-avatars", "/reader-avatars"),
        PasswordHash = string.IsNullOrWhiteSpace(request.Password) ? null : BCrypt.Net.BCrypt.HashPassword(request.Password)
    };

    private static string? BookApiController_SaveDataUrl(string? value, string folder, string requestPath)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            return value;

        var commaIndex = value.IndexOf(',');
        if (commaIndex < 0) return null;

        var header = value[..commaIndex];
        var extension = header.Contains("png", StringComparison.OrdinalIgnoreCase) ? ".png" :
            header.Contains("gif", StringComparison.OrdinalIgnoreCase) ? ".gif" : ".jpg";
        var uploadDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Core.Shared", "Uploads", folder));
        Directory.CreateDirectory(uploadDir);
        var fileName = $"{Guid.NewGuid()}{extension}";
        System.IO.File.WriteAllBytes(Path.Combine(uploadDir, fileName), Convert.FromBase64String(value[(commaIndex + 1)..]));
        return $"{requestPath}/{fileName}";
    }
}

[ApiController]
[Route("api/Staff")]
public class StaffApiController : ControllerBase
{
    private readonly IAuthService _authService;

    public StaffApiController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet("All")]
    public async Task<IActionResult> All()
    {
        var staff = await _authService.GetAllStaffAsync();
        return Ok(staff.Select(a => new
        {
            id = a.Username,
            name = a.FullName,
            email = a.Email,
            role = a.Role,
            img = a.AvatarUrl,
            createdAt = a.CreatedAt
        }));
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] StaffRequest request)
    {
        var username = string.IsNullOrWhiteSpace(request.Id)
            ? (request.Email ?? string.Empty).Split('@')[0]
            : request.Id;
        var error = await _authService.CreateAccountAsync(new Account
        {
            Username = username,
            FullName = request.Name?.Trim(),
            Email = request.Email?.Trim(),
            Role = request.Role,
            AvatarUrl = SaveDataUrl(request.Img, "staff-avatars", "/staff-avatars"),
            CreatedAt = DateTime.Now
        }, string.IsNullOrWhiteSpace(request.Password) ? "Admin@123" : request.Password);

        return error == null ? Ok(new { id = username }) : BadRequest(new { message = error });
    }

    [HttpPut("Update")]
    public async Task<IActionResult> Update([FromBody] StaffRequest request)
    {
        var error = await _authService.UpdateAccountAsync(new Account
        {
            Username = request.Id ?? string.Empty,
            FullName = request.Name?.Trim(),
            Email = request.Email?.Trim(),
            Role = request.Role,
            AvatarUrl = SaveDataUrl(request.Img, "staff-avatars", "/staff-avatars")
        }, request.Password);

        return error == null ? Ok() : BadRequest(new { message = error });
    }

    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var error = await _authService.DeleteAccountAsync(id);
        return error == null ? Ok() : BadRequest(new { message = error });
    }

    private static string? SaveDataUrl(string? value, string folder, string requestPath)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            return value;

        var commaIndex = value.IndexOf(',');
        if (commaIndex < 0) return null;

        var header = value[..commaIndex];
        var extension = header.Contains("png", StringComparison.OrdinalIgnoreCase) ? ".png" :
            header.Contains("gif", StringComparison.OrdinalIgnoreCase) ? ".gif" : ".jpg";
        var uploadDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Core.Shared", "Uploads", folder));
        Directory.CreateDirectory(uploadDir);
        var fileName = $"{Guid.NewGuid()}{extension}";
        System.IO.File.WriteAllBytes(Path.Combine(uploadDir, fileName), Convert.FromBase64String(value[(commaIndex + 1)..]));
        return $"{requestPath}/{fileName}";
    }
}

[ApiController]
[Route("api/Borrow")]
public class BorrowApiController : ControllerBase
{
    private readonly IBorrowService _borrowService;

    public BorrowApiController(IBorrowService borrowService)
    {
        _borrowService = borrowService;
    }

    [HttpGet("All")]
    public async Task<IActionResult> All()
    {
        var tickets = await _borrowService.GetAllBorrowTicketsAsync();
        return Ok(tickets.Select(ToDto));
    }

    [HttpPost("{id:int}/Approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var (success, message) = await _borrowService.ApproveBorrowRequestAsync(id, "admin");
        return success ? Ok(new { message }) : BadRequest(new { message });
    }

    [HttpPost("{id:int}/ConfirmBorrowing")]
    public async Task<IActionResult> ConfirmBorrowing(int id)
    {
        var (success, message) = await _borrowService.ConfirmBorrowingAsync(id, "admin");
        return success ? Ok(new { message }) : BadRequest(new { message });
    }

    [HttpPost("{id:int}/Return")]
    public async Task<IActionResult> Return(int id)
    {
        var (success, message) = await _borrowService.ReturnBooksAsync(id);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }

    [HttpPost("{id:int}/Reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectRequest? request)
    {
        var (success, message) = await _borrowService.RejectBorrowRequestAsync(id, request?.Reason ?? string.Empty);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }

    [HttpDelete("Delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var error = await _borrowService.DeleteAsync(id);
        return error == null ? Ok() : BadRequest(new { message = error });
    }

    private static object ToDto(BorrowTicket ticket) => new
    {
        id = ticket.TicketId,
        readerId = ticket.ReaderId,
        readerName = ticket.Reader?.FullName,
        borrowDate = ticket.BorrowDate,
        dueDate = ticket.DueDate,
        returnDate = ticket.ReturnDate,
        status = ticket.Status,
        staff = ticket.StaffUsername,
        books = ticket.Books.Select(b => new
        {
            id = b.BookId,
            title = b.Title,
            author = b.Author,
            status = b.Status
        })
    };
}

public record BookRequest(string? Id, string? Title, string? Author, string? Publisher, int? PublishYear, int Stock, string? Status, string? Description, string? Img, string? Category);
public record CategoryRequest(int Id, string? Name);
public record ReaderRequest(string? Id, string? Name, string? Email, string? Phone, string? Address, string? Status, string? Dob, string? Gender, string? Img, string? Password);
public record StaffRequest(string? Id, string? Name, string? Email, string? Role, string? Img, string? Password);
public record RejectRequest(string? Reason);
public record AdminLoginRequest(string? Username, string? Password);
