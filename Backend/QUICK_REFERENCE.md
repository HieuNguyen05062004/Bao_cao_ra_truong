# ⚡ Quick Reference Guide

## 🚀 Quick Start

### **Step 1: Configure Connection String**
Update `appsettings.json` in both Admin and Client projects:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=LOCALHOST\\SQLEXPRESS;Database=ThuVien;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### **Step 2: Run Admin Project**
```bash
cd Admin
dotnet run
# Access: https://localhost:7001/Book/Index
```

### **Step 3: Run Client Project**
```bash
cd Client
dotnet run
# Access: https://localhost:7002
```

---

## 📋 File Checklist

### **Core.Shared**
- ✅ `Interfaces/IBookService.cs` - Service interface
- ✅ `Services/BookService.cs` - Business logic
- ✅ `Repositories/BookRepository.cs` - Data access
- ✅ `Program.cs` - DI configuration (updated)

### **Admin**
- ✅ `Controllers/BookController.cs` - Book management
- ✅ `ViewModels/BookViewModel.cs` - Form model
- ✅ `Views/Book/Index.cshtml` - List view
- ✅ `Views/Book/Create.cshtml` - Add form
- ✅ `Views/Book/Edit.cshtml` - Edit form
- ✅ `Views/Book/Details.cshtml` - Details view
- ✅ `Views/Book/Delete.cshtml` - Delete confirmation
- ✅ `Program.cs` - DI configuration (updated)

### **Client**
- ✅ `Controllers/SearchController.cs` - Search & browse
- ✅ `Controllers/HomeController.cs` - Homepage (updated)
- ✅ `Views/Home/Index.cshtml` - Homepage (updated)
- ✅ `Views/Search/Index.cshtml` - Search & list
- ✅ `Views/Search/Details.cshtml` - Details
- ✅ `Program.cs` - DI configuration (updated)

---

## 🎯 Admin Features

| Feature | URL | Method |
|---------|-----|--------|
| View Books | `/Book/Index` | GET |
| Search Books | `/Book/Index?searchTerm=xxx` | GET |
| Book Details | `/Book/Details/SACH001` | GET |
| Add Book | `/Book/Create` | GET/POST |
| Edit Book | `/Book/Edit/SACH001` | GET/POST |
| Delete Book | `/Book/Delete/SACH001` | GET/POST |

---

## 🎯 Client Features

| Feature | URL | Method |
|---------|-----|--------|
| Homepage | `/Home` | GET |
| Search Books | `/Search/Index?searchTerm=xxx` | GET |
| Filter by Category | `/Search/Index?categoryId=1` | GET |
| Book Details | `/Search/Details/SACH001` | GET |

---

## 🔧 Key Methods

### **IBookService Interface**
```csharp
Task<List<Book>> GetAllBooksAsync()
Task<Book?> GetBookByIdAsync(string bookId)
Task<List<Book>> SearchBooksAsync(string searchTerm)
Task<List<Book>> GetBooksByCategoryAsync(int categoryId)
Task<List<Book>> GetAvailableBooksAsync()
Task<(bool Success, string Message)> AddBookAsync(Book book)
Task<(bool Success, string Message)> UpdateBookAsync(Book book)
Task<(bool Success, string Message)> DeleteBookAsync(string bookId)
Task<bool> BookIdExistsAsync(string bookId)
Task<List<Category>> GetAllCategoriesAsync()
Task<(bool Success, string Message)> UpdateBookQuantityAsync(string bookId, int quantityChange)
```

---

## 📊 Validation Rules

```
BookId:
  - Required
  - Unique
  - Max 20 characters

Title:
  - Required
  - Max 255 characters

Author:
  - Optional
  - Max 100 characters

Publisher:
  - Optional
  - Max 100 characters

PublishYear:
  - Optional
  - Range: 1000-2100

Quantity:
  - Required
  - Min: 0

Status:
  - Default: "Có thể mượn"

CategoryId:
  - Optional
  - Foreign key to Category

ImageUrl:
  - Optional
```

---

## 💾 Response Patterns

### **Success Response**
```csharp
(bool Success = true, string Message = "Thêm sách thành công")
```

### **Error Response**
```csharp
(bool Success = false, string Message = "Mã sách không được để trống")
```

---

## 🎨 UI Components Used

- **Bootstrap 5** - CSS Framework
- **Font Awesome** - Icons
- **Razor** - View engine
- **HTML5** - Markup
- **CSS3** - Styling
- **jQuery** - JavaScript (if needed)

---

## 🔐 Exception Handling

### **Try-Catch Pattern**
```csharp
try {
    var data = await service.GetDataAsync();
    return View(data);
} catch (Exception ex) {
    TempData["ErrorMessage"] = "Lỗi khi tải dữ liệu: " + ex.Message;
    return View(new List<Model>());
}
```

---

## 📝 Logging Considerations

Current implementation logs to:
- Standard console output
- Visual Studio debug window
- Event log (if configured)

Recommended for production:
- Structured logging (Serilog)
- Cloud logging (Application Insights)
- File logging

---

## 🧪 Testing URLs

### **Admin Project (Port 7001)**
```
Home: https://localhost:7001
Books: https://localhost:7001/Book/Index
Add Book: https://localhost:7001/Book/Create
Book Details: https://localhost:7001/Book/Details/SACH001
Edit Book: https://localhost:7001/Book/Edit/SACH001
Delete Book: https://localhost:7001/Book/Delete/SACH001
```

### **Client Project (Port 7002)**
```
Home: https://localhost:7002
Search: https://localhost:7002/Search/Index
Book Details: https://localhost:7002/Search/Details/SACH001
Category Filter: https://localhost:7002/Search/Index?categoryId=1
```

---

## ⚠️ Common Issues & Solutions

### **Issue: Connection refused**
- Check SQL Server is running
- Verify connection string
- Check database exists

### **Issue: DbContext not found**
- Ensure DbContext registered in Program.cs
- Check Core.Shared package is referenced

### **Issue: Service not injected**
- Verify AddScoped in Program.cs
- Check interface vs implementation

### **Issue: Views not found**
- Verify View path matches folder structure
- Check file extensions (.cshtml)
- Verify controller name matches folder

---

## 🔄 Dependency Injection Flow

```
Program.cs
  ├── Services.AddDbContext<LibraryDbContext>()
  ├── Services.AddScoped<BookRepository>()
  └── Services.AddScoped<IBookService, BookService>()
        ↓
   BookController
     ├── IBookService injected
     └── Methods use service
          ├── GetAllBooksAsync()
          ├── SearchBooksAsync()
          ├── AddBookAsync()
          ├── UpdateBookAsync()
          └── DeleteBookAsync()
```

---

## 📚 Database Tables

### **Books Table**
```sql
SELECT * FROM Books;
-- Columns: BookID, Title, Author, Publisher, PublishYear, 
--          CategoryID, Quantity, Status, ImageURL
```

### **Categories Table**
```sql
SELECT * FROM Categories;
-- Columns: CategoryID, CategoryName
```

### **Query Sample**
```sql
-- Join books with categories
SELECT b.BookID, b.Title, b.Author, c.CategoryName, b.Quantity
FROM Books b
LEFT JOIN Categories c ON b.CategoryID = c.CategoryID
WHERE b.Quantity > 0;
```

---

## 🎓 Learning Resources

### **Key Concepts**
- Entity Framework Core (Database First)
- ASP.NET Core MVC
- Dependency Injection
- Repository Pattern
- Service Layer Pattern
- Async/Await

### **Documentation**
- Microsoft Learn: https://learn.microsoft.com/aspnet/core
- EF Core: https://learn.microsoft.com/ef/core
- Bootstrap: https://getbootstrap.com

---

## ✨ Best Practices Implemented

- ✅ Separation of concerns
- ✅ DI pattern
- ✅ Async operations
- ✅ Validation
- ✅ Error handling
- ✅ Anti-forgery tokens
- ✅ Responsive design
- ✅ Code documentation
- ✅ Meaningful naming
- ✅ Clean architecture

---

## 📞 Support Files

- **HUONG_DAN_SU_DUNG.md** - Vietnamese guide
- **IMPLEMENTATION_SUMMARY.md** - Technical docs
- **PROJECT_STRUCTURE.md** - Folder structure
- **QUICK_REFERENCE.md** - This file

---

## ✅ Build Status

**Status**: ✅ SUCCESSFUL

All projects compile without errors!

