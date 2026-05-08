# 📚 Hướng dẫn sử dụng - Hệ thống Quản lý Thư viện

## 📖 Mục lục

1. [Kiến trúc hệ thống](#kiến-trúc-hệ-thống)
2. [Các thành phần được tạo](#các-thành-phần-được-tạo)
3. [Hướng dẫn sử dụng](#hướng-dẫn-sử-dụng)
4. [API Endpoints](#api-endpoints)
5. [Thông tin cơ sở dữ liệu](#thông-tin-cơ-sở-dữ-liệu)

---

## 🏗️ Kiến trúc hệ thống

### Mô hình Layer

```
Controller (Admin/Client)
    ↓
Service Layer (IBookService/BookService)
    ↓
Repository Layer (BookRepository)
    ↓
DbContext (LibraryDbContext)
    ↓
SQL Server Database
```

### Dependency Injection (DI)

Cấu hình trong `Program.cs`:

```csharp
// Đăng ký Repository
builder.Services.AddScoped<BookRepository>();

// Đăng ký Service
builder.Services.AddScoped<IBookService, BookService>();
```

---

## 📦 Các thành phần được tạo

### **Core.Shared Project**

#### 1. **Interface: IBookService** (`Core.Shared/Interfaces/IBookService.cs`)
- `GetAllBooksAsync()` - Lấy tất cả sách
- `GetBookByIdAsync(string bookId)` - Lấy sách theo ID
- `SearchBooksAsync(string searchTerm)` - Tìm kiếm sách
- `GetBooksByCategoryAsync(int categoryId)` - Lấy sách theo thể loại
- `GetAvailableBooksAsync()` - Lấy sách còn hàng
- `AddBookAsync(Book book)` - Thêm sách mới
- `UpdateBookAsync(Book book)` - Cập nhật sách
- `DeleteBookAsync(string bookId)` - Xóa sách
- `BookIdExistsAsync(string bookId)` - Kiểm tra mã sách tồn tại
- `GetAllCategoriesAsync()` - Lấy danh sách thể loại
- `UpdateBookQuantityAsync(string bookId, int quantityChange)` - Cập nhật số lượng

#### 2. **Repository: BookRepository** (`Core.Shared/Repositories/BookRepository.cs`)
- Xử lý tất cả truy vấn cơ sở dữ liệu
- Sử dụng Entity Framework Core async/await
- Include relationship (Category)
- Error handling

#### 3. **Service: BookService** (`Core.Shared/Services/BookService.cs`)
- Xử lý business logic
- Validate dữ liệu đầu vào
- Kiểm tra mã sách trùng
- Kiểm tra sách đang được mượn trước khi xóa

---

### **Admin Project**

#### 1. **ViewModel: BookViewModel** (`Admin/ViewModels/BookViewModel.cs`)
- Dùng cho form thêm/sửa sách
- Validation attributes
- Display attributes

#### 2. **Controller: BookController** (`Admin/Controllers/BookController.cs`)
- `Index()` - Danh sách sách + tìm kiếm
- `Details(string id)` - Chi tiết sách
- `Create()` / `Create(BookViewModel)` - Thêm sách
- `Edit(string id)` / `Edit(string id, BookViewModel)` - Sửa sách
- `Delete(string id)` / `DeleteConfirmed(string id)` - Xóa sách
- `Search(string term)` - API tìm kiếm

#### 3. **Views (Razor)**
- `Views/Book/Index.cshtml` - Danh sách sách
- `Views/Book/Create.cshtml` - Form thêm sách
- `Views/Book/Edit.cshtml` - Form sửa sách
- `Views/Book/Details.cshtml` - Chi tiết sách
- `Views/Book/Delete.cshtml` - Xác nhận xóa sách

---

### **Client Project**

#### 1. **Controller: SearchController** (`Client/Controllers/SearchController.cs`)
- `Index()` - Danh sách sách + tìm kiếm + lọc
- `Details(string id)` - Chi tiết sách
- `FilterByCategory(int categoryId)` - API lọc theo thể loại
- `Search(string term)` - API tìm kiếm

#### 2. **Controller: HomeController** (Cập nhật)
- `Index()` - Trang chủ hiển thị sách còn hàng

#### 3. **Views (Razor)**
- `Views/Home/Index.cshtml` - Trang chủ
- `Views/Search/Index.cshtml` - Tìm kiếm + danh sách sách
- `Views/Search/Details.cshtml` - Chi tiết sách

---

## 🎯 Hướng dẫn sử dụng

### **Cho Admin/Nhân viên thư viện**

#### 1. **Xem danh sách sách**
- Truy cập: `https://localhost:xxxx/Book/Index`
- Hiển thị: Mã sách, Tên sách, Tác giả, Thể loại, Số lượng, Tình trạng

#### 2. **Thêm sách mới**
- Click: "Thêm sách"
- Điền thông tin: Mã sách*, Tên sách*, Tác giả, Nhà xuất bản, Năm xuất bản, Thể loại, Số lượng*, Tình trạng, URL hình ảnh
- Validation:
  - Mã sách không được trùng
  - Mã sách tối đa 20 ký tự
  - Tên sách không được để trống (tối đa 255 ký tự)
  - Số lượng >= 0
- Click: "Thêm sách"

#### 3. **Cập nhật thông tin sách**
- Click: "Sửa" (icon bút chì)
- Chỉnh sửa thông tin (Mã sách không thể thay đổi)
- Click: "Lưu thay đổi"

#### 4. **Xóa sách**
- Click: "Xóa" (icon thùng rác)
- Xác nhận xóa
- **Lưu ý**: Không thể xóa sách đang được mượn

#### 5. **Tìm kiếm sách**
- Nhập: Tên sách hoặc tác giả
- Click: "Tìm kiếm"

---

### **Cho Bạn đọc (Client)**

#### 1. **Xem trang chủ**
- Truy cập: `https://localhost:xxxx/Home`
- Hiển thị: 6 sách nổi bật (có còn hàng)
- Có thể lọc theo thể loại

#### 2. **Tìm kiếm sách**
- Nhập: Tên sách hoặc tác giả
- Click: "Tìm" hoặc "Tìm kiếm"
- Kết quả: Danh sách sách phù hợp

#### 3. **Lọc theo thể loại**
- Click: Tên thể loại
- Hiển thị: Sách thuộc thể loại đó

#### 4. **Xem chi tiết sách**
- Click: "Xem chi tiết"
- Hiển thị: Đầy đủ thông tin sách
- Kiểm tra: Còn hàng hay hết

#### 5. **Gửi yêu cầu mươn sách**
- (Chức năng đang phát triển)
- Click: "Gửi yêu cầu mượn" (nếu còn hàng)

---

## 🔗 API Endpoints

### **Admin Endpoints**

| Method | URL | Mô tả |
|--------|-----|-------|
| GET | `/Book/Index` | Danh sách sách |
| GET | `/Book/Index?searchTerm=xxx` | Tìm kiếm sách |
| GET | `/Book/Details/{id}` | Chi tiết sách |
| GET | `/Book/Create` | Form thêm sách |
| POST | `/Book/Create` | Xử lý thêm sách |
| GET | `/Book/Edit/{id}` | Form sửa sách |
| POST | `/Book/Edit/{id}` | Xử lý cập nhật sách |
| GET | `/Book/Delete/{id}` | Xác nhận xóa |
| POST | `/Book/Delete/{id}` | Xử lý xóa sách |
| GET | `/Book/Search?term=xxx` | API tìm kiếm (JSON) |

### **Client Endpoints**

| Method | URL | Mô tả |
|--------|-----|-------|
| GET | `/Home/Index` | Trang chủ |
| GET | `/Search/Index` | Danh sách sách |
| GET | `/Search/Index?searchTerm=xxx` | Tìm kiếm sách |
| GET | `/Search/Index?categoryId=x` | Lọc theo thể loại |
| GET | `/Search/Details/{id}` | Chi tiết sách |
| GET | `/Search/FilterByCategory?categoryId=x` | API lọc (JSON) |
| GET | `/Search/Search?term=xxx` | API tìm kiếm (JSON) |

---

## 🗄️ Thông tin Cơ sở dữ liệu

### **Entity: Book**
```
BookID (PK)       : string (20)
Title             : string (255) - NOT NULL
Author            : string (100)
Publisher         : string (100)
PublishYear       : int
CategoryID (FK)   : int
Quantity          : int (Default: 0)
Status            : string (50) (Default: "Có thể mượn")
ImageURL          : string
```

### **Entity: Category**
```
CategoryID (PK)   : int
CategoryName      : string (100) - NOT NULL
```

### **Relationships**
- **Book.CategoryID** → Category.CategoryID (One-to-Many)
- **BorrowTicket.Books** → Book (Many-to-Many)

---

## 📝 Validate Rules

### **Khi thêm/sửa sách:**

```csharp
- Mã sách: Bắt buộc, duy nhất, tối đa 20 ký tự
- Tên sách: Bắt buộc, tối đa 255 ký tự
- Tác giả: Tối đa 100 ký tự
- Nhà xuất bản: Tối đa 100 ký tự
- Năm xuất bản: 1000-2100
- Số lượng: Bắt buộc, >= 0
- Thể loại: Không bắt buộc
- Tình trạng: Mặc định "Có thể mươn"
```

### **Khi xóa sách:**
```csharp
- Sách không được đang được mượn (Status != "Trả hàng")
- Nếu đang mượn: Hiển thị thông báo lỗi
```

---

## 🚀 Cách chạy hệ thống

### **1. Cấu hình Connection String**

Trong `appsettings.json` (Admin/Client):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=LOCALHOST\\SQLEXPRESS;Database=ThuVien;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### **2. Chạy Admin Project**
```bash
cd Admin
dotnet run
# Truy cập: https://localhost:7001
```

### **3. Chạy Client Project**
```bash
cd Client
dotnet run
# Truy cập: https://localhost:7002
```

---

## 🔐 Exception Handling

Tất cả Controller action đều có try-catch:
- Catch lỗi và hiển thị thông báo lỗi
- Log lỗi (nếu cần)
- Redirect về trang phù hợp

---

## 💾 Async/Await Usage

Tất cả database operations:
- Sử dụng `async/await`
- `.ToListAsync()`, `.FirstOrDefaultAsync()`, `.AnyAsync()`, `.FindAsync()`
- `SaveChangesAsync()`

---

## 📊 Tìm kiếm

### **Tìm kiếm theo:**
- Tên sách (Title)
- Tác giả (Author)
- Case-insensitive

### **Lọc theo:**
- Thể loại (CategoryId)
- Sách còn hàng (Quantity > 0)

---

## ✅ Kiểm tra Build

Build successful ✓

---

## 📞 Support

Nếu có vấn đề, kiểm tra:
1. Connection string trong `appsettings.json`
2. Database đã tồn tại và có bảng Books, Categories
3. DbContext được đăng ký trong `Program.cs`
4. Repository và Service được đăng ký trong `Program.cs`

