# 📁 Project Structure Guide

## Solution: Backend

```
Backend/
│
├── Core.Shared/                          (Shared Layer)
│   ├── Interfaces/
│   │   ├── IBookService.cs              ✅ Interface cho dịch vụ sách
│   │   ├── IReaderService.cs
│   │   ├── IBorrowService.cs
│   │   └── IAuthService.cs
│   │
│   ├── Services/
│   │   ├── BookService.cs               ✅ Xử lý business logic sách
│   │   ├── ReaderService.cs
│   │   ├── BorrowService.cs
│   │   └── AuthService.cs
│   │
│   ├── Repositories/
│   │   ├── BookRepository.cs            ✅ Truy vấn dữ liệu sách
│   │   ├── ReaderRepository.cs
│   │   └── BorrowRepository.cs
│   │
│   ├── Data/
│   │   └── LibraryDbContext.cs          ✅ Entity Framework DbContext
│   │
│   ├── Entities/
│   │   ├── Book.cs                      ✅ Entity sách
│   │   ├── Category.cs                  ✅ Entity thể loại
│   │   ├── Reader.cs
│   │   ├── BorrowTicket.cs
│   │   ├── Account.cs
│   │
│   ├── Constants/
│   │   ├── MessageConstants.cs
│   │   └── RoleConstants.cs
│   │
│   └── Helpers/
│       └── (Các helper methods)
│
├── Admin/                                (Admin/Staff Project)
│   ├── Controllers/
│   │   ├── BookController.cs            ✅ Quản lý sách
│   │   └── HomeController.cs
│   │
│   ├── ViewModels/
│   │   └── BookViewModel.cs             ✅ ViewModel cho form sách
│   │
│   ├── Views/
│   │   ├── Book/
│   │   │   ├── Index.cshtml             ✅ Danh sách sách
│   │   │   ├── Create.cshtml            ✅ Form thêm sách
│   │   │   ├── Edit.cshtml              ✅ Form sửa sách
│   │   │   ├── Details.cshtml           ✅ Chi tiết sách
│   │   │   └── Delete.cshtml            ✅ Xác nhận xóa
│   │   ├── Home/
│   │   ├── Shared/
│   │   └── _ViewStart.cshtml
│   │
│   ├── Program.cs                       ✅ DI Configuration
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Admin.csproj
│
├── Client/                               (Client/Reader Project)
│   ├── Controllers/
│   │   ├── HomeController.cs            ✅ Trang chủ
│   │   ├── SearchController.cs          ✅ Tìm kiếm sách
│   │   └── ErrorController.cs
│   │
│   ├── Views/
│   │   ├── Home/
│   │   │   └── Index.cshtml             ✅ Trang chủ
│   │   ├── Search/
│   │   │   ├── Index.cshtml             ✅ Tìm kiếm & danh sách
│   │   │   └── Details.cshtml           ✅ Chi tiết sách
│   │   ├── Shared/
│   │   ├── ErrorViewModel.cs
│   │   └── _ViewStart.cshtml
│   │
│   ├── Models/
│   │   └── ErrorViewModel.cs
│   │
│   ├── Program.cs                       ✅ DI Configuration
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Client.csproj
│
├── HUONG_DAN_SU_DUNG.md                 ✅ Hướng dẫn tiếng Việt
├── IMPLEMENTATION_SUMMARY.md            ✅ Tài liệu tiếng Anh
└── PROJECT_STRUCTURE.md                 ✅ File này

```

---

## 📝 File mới được tạo

### **Core.Shared** (Layers)

| File | Trạng thái | Mô tả |
|------|-----------|-------|
| `Interfaces/IBookService.cs` | ✅ | Interface định nghĩa các phương thức quản lý sách |
| `Services/BookService.cs` | ✅ | Thực thi business logic quản lý sách |
| `Repositories/BookRepository.cs` | ✅ | Truy vấn cơ sở dữ liệu cho sách |

---

### **Admin** (Quản trị viên)

| File | Trạng thái | Mô tả |
|------|-----------|-------|
| `Controllers/BookController.cs` | ✅ | Controller quản lý sách |
| `ViewModels/BookViewModel.cs` | ✅ | ViewModel cho form thêm/sửa sách |
| `Views/Book/Index.cshtml` | ✅ | Danh sách sách + tìm kiếm |
| `Views/Book/Create.cshtml` | ✅ | Form thêm sách mới |
| `Views/Book/Edit.cshtml` | ✅ | Form sửa thông tin sách |
| `Views/Book/Details.cshtml` | ✅ | Xem chi tiết sách |
| `Views/Book/Delete.cshtml` | ✅ | Xác nhận xóa sách |
| `Program.cs` | ✅ | Cấu hình Dependency Injection |

---

### **Client** (Bạn đọc)

| File | Trạng thái | Mô tả |
|------|-----------|-------|
| `Controllers/SearchController.cs` | ✅ | Controller tìm kiếm sách |
| `Controllers/HomeController.cs` | ✅ | Controller trang chủ (cập nhật) |
| `Views/Home/Index.cshtml` | ✅ | Trang chủ hiển thị sách nổi bật |
| `Views/Search/Index.cshtml` | ✅ | Tìm kiếm + danh sách sách |
| `Views/Search/Details.cshtml` | ✅ | Chi tiết sách |
| `Program.cs` | ✅ | Cấu hình Dependency Injection |

---

## 🔄 Data Flow

### **Xem danh sách sách (Admin)**

```
1. User: Truy cập /Book/Index
   ↓
2. BookController.Index() - GET
   ├── Gọi IBookService.GetAllBooksAsync()
   │   ↓
3. BookService.GetAllBooksAsync()
   ├── Gọi BookRepository.GetAllAsync()
   │   ↓
4. BookRepository.GetAllAsync()
   ├── Thực hiện: dbContext.Books
   │   .Include(b => b.Category)
   │   .AsNoTracking()
   │   .ToListAsync()
   ├── Trả về: List<Book>
   ↓
5. BookService trả về: List<Book>
   ↓
6. BookController trả về: View(books)
   ↓
7. View/Book/Index.cshtml
   └── Render HTML danh sách sách
```

### **Thêm sách mới (Admin)**

```
1. User: Click "Thêm sách"
   ↓
2. BookController.Create() - GET
   └── Return: View(new BookViewModel)
   ↓
3. View/Book/Create.cshtml - Hiển thị form
   ↓
4. User: Điền thông tin + Submit
   ↓
5. BookController.Create(BookViewModel) - POST
   ├── Validate ModelState
   ├── Convert ViewModel → Entity
   ├── Gọi IBookService.AddBookAsync(book)
   │   ↓
6. BookService.AddBookAsync(Book)
   ├── Validate:
   │   ├── BookId không null
   │   ├── Title không null
   │   ├── BookId không được trùng
   │   ├── Quantity >= 0
   ├── Gọi BookRepository.AddAsync(book)
   │   ↓
7. BookRepository.AddAsync(Book)
   ├── dbContext.Books.Add(book)
   ├── await dbContext.SaveChangesAsync()
   └── Return: bool success
   ↓
8. BookService trả về: (success, message)
   ↓
9. BookController:
   ├── Nếu success: TempData["SuccessMessage"] → Redirect
   ├── Nếu failed: TempData["ErrorMessage"] → Return View
```

### **Tìm kiếm sách (Client)**

```
1. User: Nhập từ khóa + Click "Tìm"
   ↓
2. SearchController.Index(searchTerm)
   ├── Gọi IBookService.SearchBooksAsync(searchTerm)
   │   ↓
3. BookService.SearchBooksAsync(string)
   ├── Gọi BookRepository.SearchAsync(searchTerm)
   │   ↓
4. BookRepository.SearchAsync(string)
   ├── Thực hiện:
   │   dbContext.Books
   │   .Include(b => b.Category)
   │   .Where(b => b.Title.Contains(searchTerm) ||
   │              b.Author.Contains(searchTerm))
   │   .ToListAsync()
   └── Return: List<Book>
   ↓
5. BookService trả về: List<Book>
   ↓
6. SearchController trả về: View(books)
   ↓
7. View/Search/Index.cshtml
   └── Render HTML kết quả tìm kiếm
```

---

## 🗂️ Folder Structure Explanation

### **Core.Shared** (Shared Library)
- **Purpose**: Chứa code dùng chung cho Admin và Client
- **Interfaces**: Định nghĩa contract (interface)
- **Services**: Xử lý business logic
- **Repositories**: Truy vấn database
- **Data**: DbContext (Entity Framework)
- **Entities**: Database models

### **Admin** (Quản trị)
- **Purpose**: Website quản lý sách dành cho admin/nhân viên
- **Controllers**: Xử lý request từ người dùng
- **ViewModels**: Định dạng dữ liệu cho View
- **Views**: Giao diện HTML

### **Client** (Bạn đọc)
- **Purpose**: Website xem sách dành cho bạn đọc
- **Controllers**: Xử lý request từ người dùng
- **Views**: Giao diện HTML

---

## 🔐 Security Considerations

### **Implemented**
- ✅ AntiForgerToken trên POST requests
- ✅ Input validation (Model validation)
- ✅ Server-side validation (Service layer)
- ✅ Error handling (không tiếp lộ thông tin nhạy cảm)

### **Recommended for Production**
- ⚠️ Authentication (Login/Authorization)
- ⚠️ HTTPS/SSL
- ⚠️ SQL Injection prevention (Entity Framework LINQ)
- ⚠️ XSS protection (Razor auto-encoding)
- ⚠️ CORS configuration
- ⚠️ Rate limiting

---

## 📊 Database Relationships

```
┌─────────────────┐
│   Category      │
├─────────────────┤
│ CategoryID (PK) │
│ CategoryName    │
└────────┬────────┘
         │
         │ 1:*
         │
┌────────▼────────────────────────┐
│          Book                   │
├─────────────────────────────────┤
│ BookID (PK)                     │
│ Title                           │
│ Author                          │
│ Publisher                       │
│ PublishYear                     │
│ CategoryID (FK) ───────┘        │
│ Quantity                        │
│ Status                          │
│ ImageURL                        │
└────────┬─────────────────────────┘
         │
         │ M:* (via BorrowDetails)
         │
┌────────▼───────────────────────┐
│    BorrowTicket                │
├────────────────────────────────┤
│ TicketID (PK)                  │
│ ReaderID (FK)                  │
│ StaffUsername (FK)             │
│ BorrowDate                      │
│ DueDate                         │
│ ReturnDate                      │
│ Status                          │
└────────────────────────────────┘
```

---

## 🚀 Deployment Checklist

- [ ] Update `appsettings.Production.json`
- [ ] Update Connection String
- [ ] Disable detailed error messages
- [ ] Enable HTTPS
- [ ] Setup authentication
- [ ] Add logging
- [ ] Optimize database queries
- [ ] Test all endpoints
- [ ] Load testing
- [ ] Security audit
- [ ] Backup strategy

---

## 📚 Related Files

- **HUONG_DAN_SU_DUNG.md** - Vietnamese user guide with screenshots
- **IMPLEMENTATION_SUMMARY.md** - English technical documentation
- **PROJECT_STRUCTURE.md** - This file (Project structure overview)

---

## ✅ Status

**Build Status**: ✅ SUCCESS

All files created and project compiles successfully!

