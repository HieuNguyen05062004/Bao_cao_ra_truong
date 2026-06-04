using Core.Shared.Entities;
using Core.Shared.Interfaces;
using Core.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers;

[ApiController]
[Route("api/books")]
public class BooksApiController : ControllerBase
{
    private readonly IBookService _bookService;
    private readonly ICategoryService _categoryService;
    private readonly IAiSearchService _aiSearchService;

    public BooksApiController(
        IBookService bookService,
        ICategoryService categoryService,
        IAiSearchService aiSearchService)
    {
        _bookService = bookService;
        _categoryService = categoryService;
        _aiSearchService = aiSearchService;
    }

    [HttpGet]
    public async Task<IActionResult> All([FromQuery] string keyword = "", [FromQuery] int categoryId = 0)
    {
        var books = !string.IsNullOrWhiteSpace(keyword)
            ? await _bookService.SearchBooksAsync(keyword.Trim())
            : categoryId > 0
                ? await _bookService.GetBooksByCategoryAsync(categoryId)
                : await _bookService.GetAllBooksAsync();

        return Ok(books.Select(ToDto));
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q = "", [FromQuery] string mode = "basic")
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(Array.Empty<object>());

        var keyword = q.Trim();
        var isAiMode = mode.Equals("ai", StringComparison.OrdinalIgnoreCase);
        if (mode.Equals("ai", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var ai = await _aiSearchService.ParseSearchQueryAsync(q);
                if (!string.IsNullOrWhiteSpace(ai.Keyword))
                    keyword = ai.Keyword;
            }
            catch
            {
                keyword = q.Trim();
            }
        }

        var books = await _bookService.SearchBooksAsync(keyword);
        if (isAiMode && books.Count == 0 && !keyword.Equals(q.Trim(), StringComparison.OrdinalIgnoreCase))
            books = await _bookService.SearchBooksAsync(q.Trim());

        return Ok(books.Select(ToDto));
    }

    [HttpGet("latest")]
    public async Task<IActionResult> Latest([FromQuery] int count = 5)
    {
        var books = await _bookService.GetFeaturedBooksAsync(count);
        if (books.Count == 0)
            books = (await _bookService.GetAllBooksAsync())
                .OrderByDescending(b => b.CreatedAt ?? DateTime.MinValue)
                .Take(count)
                .ToList();
        return Ok(books.Select(ToDto));
    }

    [HttpGet("trending")]
    public async Task<IActionResult> Trending([FromQuery] int count = 5)
    {
        var books = await _bookService.GetTrendingBooksAsync(count);
        if (books.Count == 0)
            books = (await _bookService.GetAllBooksAsync())
                .OrderByDescending(b => b.Tickets.Count)
                .ThenBy(b => b.Title)
                .Take(count)
                .ToList();
        return Ok(books.Select(ToDto));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Details(string id)
    {
        var book = await _bookService.GetBookByIdAsync(id);
        return book == null ? NotFound() : Ok(ToDto(book));
    }

    [HttpGet("categories")]
    public async Task<IActionResult> Categories()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        return Ok(categories.Select(c => new { id = c.CategoryId, name = c.CategoryName }));
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
            status = (book.Quantity ?? 0) > 0 ? "Sẵn có" : "Hết hàng",
            statusClass = (book.Quantity ?? 0) > 0 ? "success" : "danger",
            img = book.ImageUrl,
            description = book.Description,
            category = string.Join(", ", categoryNames),
            categoryIds = book.BookCategories?.Select(bc => bc.CategoryId).ToList() ?? new List<int>()
        };
    }
}

[ApiController]
[Route("api/borrow")]
public class BorrowApiController : ControllerBase
{
    private readonly IBorrowService _borrowService;

    public BorrowApiController(IBorrowService borrowService)
    {
        _borrowService = borrowService;
    }

    [HttpGet("history")]
    public async Task<IActionResult> History()
    {
        var readerId = HttpContext.Session.GetString("ReaderId");
        if (string.IsNullOrWhiteSpace(readerId))
            return Unauthorized(new { message = "Chua dang nhap." });

        var tickets = await _borrowService.GetBorrowTicketsByReaderIdAsync(readerId);
        return Ok(tickets.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var ticket = await _borrowService.GetBorrowTicketByIdAsync(id);
        if (ticket == null) return NotFound();

        var readerId = HttpContext.Session.GetString("ReaderId");
        if (!string.IsNullOrWhiteSpace(readerId) && ticket.ReaderId != readerId)
            return Forbid();

        return Ok(ToDto(ticket));
    }

    [HttpPost("request")]
    public async Task<IActionResult> Create([FromBody] BorrowRequest request)
    {
        var readerId = HttpContext.Session.GetString("ReaderId");
        if (string.IsNullOrWhiteSpace(readerId))
            return Unauthorized(new { message = "Vui long dang nhap de muon sach." });

        if (!request.BorrowDate.HasValue)
            return BadRequest(new { message = "Vui lòng chọn ngày mượn." });

        if (!request.DueDate.HasValue)
            return BadRequest(new { message = "Vui lòng chọn ngày trả." });

        var (success, message, ticketId) = await _borrowService.CreateBorrowRequestAsync(
            readerId,
            request.BookIds ?? new List<string>(),
            request.BorrowDate.Value,
            request.DueDate.Value);

        return success ? Ok(new { message, ticketId }) : BadRequest(new { message });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var error = await _borrowService.DeleteAsync(id);
        return error == null ? Ok() : BadRequest(new { message = error });
    }

    private static object ToDto(BorrowTicket ticket) => new
    {
        id = $"TKT-{ticket.TicketId:000}",
        rawId = ticket.TicketId,
        readerName = ticket.Reader?.FullName,
        date = ticket.BorrowDate?.ToString("dd/MM/yyyy"),
        due = ticket.DueDate?.ToString("dd/MM/yyyy"),
        borrowDate = ticket.BorrowDate,
        dueDate = ticket.DueDate,
        returnDate = ticket.ReturnDate,
        count = ticket.Books.Count,
        status = MapStatus(ticket.Status),
        statusText = ticket.Status,
        books = ticket.Books.Select(b => new { id = b.BookId, title = b.Title, quantity = 1 })
    };

    private static string MapStatus(string? status) => status switch
    {
        var s when s == BorrowService.StatusApproved => "Approved",
        var s when s == BorrowService.StatusBorrowing => "Borrowing",
        var s when s == BorrowService.StatusReturned => "Returned",
        var s when s == BorrowService.StatusRejected => "Rejected",
        _ => "Pending"
    };
}

[ApiController]
[Route("api/auth")]
public class AuthApiController : ControllerBase
{
    private readonly IReaderService _readerService;
    private readonly IUnifiedAuthService _authService;

    public AuthApiController(IReaderService readerService, IUnifiedAuthService authService)
    {
        _readerService = readerService;
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request.Email ?? string.Empty, request.Password ?? string.Empty);
        if (!result.Success) return Unauthorized(new { message = result.Message });

        if (result.UserType == "Reader")
        {
            HttpContext.Session.SetString("ReaderId", result.UserId);
            HttpContext.Session.SetString("ReaderName", result.UserName);
            HttpContext.Session.SetString("ReaderEmail", request.Email ?? string.Empty);
            if (!string.IsNullOrEmpty(result.AvatarUrl))
                HttpContext.Session.SetString("ReaderAvatar", result.AvatarUrl);
        }
        else
        {
            HttpContext.Session.SetString("AdminUsername", result.UserId);
            HttpContext.Session.SetString("AdminName", result.UserName);
            HttpContext.Session.SetString("AdminRole", result.UserType);
        }

        return Ok(new
        {
            userType = result.UserType,
            userId = result.UserId,
            userName = result.UserName,
            avatarUrl = result.AvatarUrl
        });
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        var readerId = HttpContext.Session.GetString("ReaderId");
        if (!string.IsNullOrWhiteSpace(readerId))
        {
            return Ok(new
            {
                isAuthenticated = true,
                userType = "Reader",
                userId = readerId,
                userName = HttpContext.Session.GetString("ReaderName"),
                avatarUrl = HttpContext.Session.GetString("ReaderAvatar")
            });
        }

        var adminId = HttpContext.Session.GetString("AdminUsername");
        if (!string.IsNullOrWhiteSpace(adminId))
        {
            return Ok(new
            {
                isAuthenticated = true,
                userType = HttpContext.Session.GetString("AdminRole") ?? "Admin",
                userId = adminId,
                userName = HttpContext.Session.GetString("AdminName"),
                avatarUrl = ""
            });
        }

        return Ok(new { isAuthenticated = false });
    }

    [HttpGet("password/{readerId}")]
    public async Task<IActionResult> GetPassword(string readerId)
    {
        var currentReaderId = HttpContext.Session.GetString("ReaderId");
        // Chỉ Reader tương ứng mới được xem mật khẩu của chính mình
        if (string.IsNullOrEmpty(currentReaderId) || currentReaderId != readerId)
            return Unauthorized(new { message = "Bạn không có quyền xem mật khẩu này." });

        var reader = await _readerService.GetByIdAsync(readerId);
        if (reader == null)
            return NotFound(new { message = "Bạn đọc không tồn tại." });

        // Trả về password hash để hiển thị
        return Ok(new { password = reader.PasswordHash ?? "Không có mật khẩu", readerId = reader.ReaderId });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromForm] RegisterRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Email) && await _authService.EmailExistsAsync(request.Email))
            return BadRequest(new { message = "Email da duoc dang ky." });

        string? avatarUrl = null;
        if (request.Avatar != null && request.Avatar.Length > 0)
            avatarUrl = await SaveAvatarAsync(request.Avatar);

        var reader = new Reader
        {
            ReaderId = string.Empty,
            FullName = request.FullName?.Trim() ?? string.Empty,
            Email = request.Email?.Trim(),
            Phone = request.Phone?.Trim(),
            DoB = DateOnly.TryParse(request.DoB, out var dob) ? dob : null,
            Gender = request.Gender ?? "Nam",
            Address = request.Address?.Trim(),
            AvatarUrl = avatarUrl,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password ?? string.Empty),
            CreatedAt = DateTime.Now
        };

        var error = await _readerService.CreateAsync(reader);
        return error == null ? Ok(new { readerId = reader.ReaderId }) : BadRequest(new { message = error });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok();
    }

    private static async Task<string?> SaveAvatarAsync(IFormFile file)
    {
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext) || file.Length > 5 * 1024 * 1024) return null;

        var folder = Path.GetFullPath(Path.Combine(
            Directory.GetCurrentDirectory(), "../Core.Shared/Uploads/reader-avatars"));
        Directory.CreateDirectory(folder);

        var fileName = $"tmp_{DateTime.Now:yyMMddHHmmss}_{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(folder, fileName);
        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/reader-avatars/{fileName}";
    }
}

public class BorrowRequest
{
    public List<string>? BookIds { get; set; }
    public DateTime? BorrowDate { get; set; }
    public DateTime? DueDate { get; set; }
}

public record LoginRequest(string? Email, string? Password);

public class RegisterRequest
{
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Gender { get; set; }
    public string? Password { get; set; }
    public string? DoB { get; set; }
    public string? Address { get; set; }
    public IFormFile? Avatar { get; set; }
}
