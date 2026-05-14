using Core.Shared.Data;
using Core.Shared.Interfaces;
using Core.Shared.Repositories;
using Core.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// ── MVC ──────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── DbContext ─────────────────────────────────────────────────────────────
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Session ───────────────────────────────────────────────────────────────
builder.Services.AddDistributedMemoryCache();   // ← BẮT BUỘC phải có
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ── Repositories ─────────────────────────────────────────────────────────
builder.Services.AddScoped<BookRepository>();
builder.Services.AddScoped<CategoryRepository>();
builder.Services.AddScoped<AccountRepository>();   // ← mới
builder.Services.AddScoped<ReaderRepository>();
builder.Services.AddScoped<BorrowRepository>();
// ── Services ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAuthService, AuthService>();  // ← mới
builder.Services.AddScoped<IReaderService, ReaderService>();
builder.Services.AddScoped<IBorrowService, BorrowService>();
// ─────────────────────────────────────────────────────────────────────────
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Serve ảnh sách
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.GetFullPath(Path.Combine(
            Directory.GetCurrentDirectory(), "../Core.Shared/Uploads/books"))),
    RequestPath = "/book-images"
});

app.UseRouting();

app.UseSession();          // ← phải đặt TRƯỚC UseAuthorization
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")  // Mặc định vào Login
    .WithStaticAssets();

// Seed tài khoản admin mặc định nếu chưa có
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
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

app.Run();
app.Run();
