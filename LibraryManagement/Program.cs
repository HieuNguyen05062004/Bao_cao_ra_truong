using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=library.db"));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Register application services
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBorrowService, BorrowService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<ISearchService, SearchService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Seed database and create roles/admin
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles = { "Admin", "Staff", "Reader" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Create default admin account
        if (await userManager.FindByEmailAsync("admin@library.com") == null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin@library.com",
                Email = "admin@library.com",
                FullName = "Quản trị viên",
                IsActive = true,
                CreatedAt = DateTime.Now,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(admin, "Admin@123");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Create default staff account
        if (await userManager.FindByEmailAsync("staff@library.com") == null)
        {
            var staff = new ApplicationUser
            {
                UserName = "staff@library.com",
                Email = "staff@library.com",
                FullName = "Nhân viên thư viện",
                IsActive = true,
                CreatedAt = DateTime.Now,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(staff, "Staff@123");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(staff, "Staff");
        }

        // Create default reader account
        if (await userManager.FindByEmailAsync("reader@library.com") == null)
        {
            var reader = new ApplicationUser
            {
                UserName = "reader@library.com",
                Email = "reader@library.com",
                FullName = "Bạn đọc mẫu",
                IsActive = true,
                CreatedAt = DateTime.Now,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(reader, "Reader@123");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(reader, "Reader");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi khi khởi tạo database");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

