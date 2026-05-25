using Core.Shared.Data;
using Core.Shared.Interfaces;
using Core.Shared.Repositories;
using Core.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Đăng ký Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Đăng ký DbContext
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Đăng ký Repository
builder.Services.AddScoped<BookRepository>();
builder.Services.AddScoped<BorrowRepository>();
builder.Services.AddScoped<ReaderRepository>();
builder.Services.AddScoped<AccountRepository>();
builder.Services.AddScoped<CategoryRepository>();

// Đăng ký Service
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IBorrowService, BorrowService>();
builder.Services.AddScoped<IReaderService, ReaderService>();
builder.Services.AddScoped<IUnifiedAuthService, UnifiedAuthService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

// ── AI Search ────────────────────────────────────────────────────────────────
// Dùng typed HttpClient — DI tự inject HttpClient vào constructor AiSearchService
builder.Services.AddHttpClient<AiSearchService>();
builder.Services.AddScoped<IAiSearchService, AiSearchService>();
// ─────────────────────────────────────────────────────────────────────────────

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

var frontendPath = Path.GetFullPath(Path.Combine(
    Directory.GetCurrentDirectory(), "../../Fontend/Client"));
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

var accountFrontendPath = Path.GetFullPath(Path.Combine(
    Directory.GetCurrentDirectory(), "../../Fontend/Acount"));
if (Directory.Exists(accountFrontendPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(accountFrontendPath),
        RequestPath = "/account"
    });
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(accountFrontendPath),
        RequestPath = "/Account"
    });
}

// Serve ảnh sách
var bookImagesPath = Path.GetFullPath(Path.Combine(
    Directory.GetCurrentDirectory(), "../Core.Shared/Uploads/books"));
Directory.CreateDirectory(bookImagesPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(bookImagesPath),
    RequestPath = "/book-images"
});

// Serve ảnh đại diện bạn đọc
var readerAvatarsPath = Path.GetFullPath(Path.Combine(
    Directory.GetCurrentDirectory(), "../Core.Shared/Uploads/reader-avatars"));
Directory.CreateDirectory(readerAvatarsPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(readerAvatarsPath),
    RequestPath = "/reader-avatars"
});

app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllers();

if (Directory.Exists(frontendPath))
    app.MapGet("/", () => Results.Redirect("/index.html"));

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
