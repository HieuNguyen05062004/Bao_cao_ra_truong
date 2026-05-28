using Core.Shared.Data;
using Core.Shared.Interfaces;
using Core.Shared.Repositories;
using Core.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// ── MVC ───────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── DbContext ─────────────────────────────────────────────────────────────
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Session ───────────────────────────────────────────────────────────────
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ── Repositories ──────────────────────────────────────────────────────────
builder.Services.AddScoped<BookRepository>();
builder.Services.AddScoped<CategoryRepository>();
builder.Services.AddScoped<AccountRepository>();
builder.Services.AddScoped<ReaderRepository>();
builder.Services.AddScoped<BorrowRepository>();

// ── Services ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IReaderService, ReaderService>();
builder.Services.AddScoped<IBorrowService, BorrowService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

var frontendPath = Path.GetFullPath(Path.Combine(
    Directory.GetCurrentDirectory(), "../../Fontend/Admin"));
if (Directory.Exists(frontendPath))
{
    app.UseDefaultFiles(new DefaultFilesOptions
    {
        FileProvider = new PhysicalFileProvider(frontendPath),
        DefaultFileNames = { "index.html" }
    });
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(frontendPath)
    });
}

// ── Serve ảnh sách ────────────────────────────────────────────────────────
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.GetFullPath(Path.Combine(
            Directory.GetCurrentDirectory(), "../Core.Shared/Uploads/books"))),
    RequestPath = "/book-images"
});

// ── Serve ảnh đại diện bạn đọc ────────────────────────────────────────────
var readerAvatarsPath = Path.GetFullPath(Path.Combine(
    Directory.GetCurrentDirectory(), "../Core.Shared/Uploads/reader-avatars"));
Directory.CreateDirectory(readerAvatarsPath); // tạo folder nếu chưa có
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(readerAvatarsPath),
    RequestPath = "/reader-avatars"
});

var staffAvatarsPath = Path.GetFullPath(Path.Combine(
    Directory.GetCurrentDirectory(), "../Core.Shared/Uploads/staff-avatars"));
Directory.CreateDirectory(staffAvatarsPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(staffAvatarsPath),
    RequestPath = "/staff-avatars"
});

app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllers();

if (Directory.Exists(frontendPath))
    app.MapGet("/", () => Results.Redirect("/login.html"));

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

// ── Seed tài khoản admin mặc định ─────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    context.Database.EnsureCreated();
    if (!context.Accounts.Any(a => a.Username == "admin"))
    {
        context.Accounts.Add(new Core.Shared.Entities.Account
        {
            Username = "admin",
            Password = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            FullName = "Quản trị viên",
            Role = "Admin",
            Email = "admin@thuvien.com",
            CreatedAt = DateTime.Now
        });
        context.SaveChanges();
    }
}

app.Run(); // ← chỉ gọi 1 lần (đã xóa app.Run() thừa ở cuối)
