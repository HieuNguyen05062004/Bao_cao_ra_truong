# 📚 Hệ thống Quản lý Thư viện - Hướng dẫn Chi tiết (Tiếng Việt)

## 📖 Mục lục

- [Giới thiệu](#giới-thiệu)
- [Cấu trúc dự án](#cấu-trúc-dự-án)
- [Các tính năng](#các-tính-năng)
- [Hướng dẫn sử dụng](#hướng-dẫn-sử-dụng)
- [Hướng dẫn phát triển](#hướng-dẫn-phát-triển)
- [Xử lý lỗi](#xử-lý-lỗi)

---

## 🎯 Giới thiệu

### **Tổng quan**

Hệ thống Quản lý Thư viện là một ứng dụng ASP.NET Core MVC hoàn chỉnh cho phép:

- **Admin/Nhân viên**: Quản lý sách (thêm, sửa, xóa, tìm kiếm)
- **Bạn đọc**: Duyệt, tìm kiếm, xem chi tiết sách

### **Công nghệ sử dụng**

- **.NET 10** - Framework
- **ASP.NET Core MVC** - Web framework
- **Entity Framework Core** - ORM
- **SQL Server** - Database
- **Bootstrap 5** - CSS framework
- **Razor** - View engine

### **Kiến trúc**

```
Presentation Layer (Views)
    ↓
Controller Layer
    ↓
Service Layer (Business Logic)
    ↓
Repository Layer (Data Access)
    ↓
Database Layer (SQL Server)
```

---

## 📁 Cấu trúc Dự án

```
Backend/
├── Core.Shared/                    # Thư viện dùng chung
│   ├── Interfaces/
│   │   └── IBookService.cs        # Interface dịch vụ sách
│   ├── Services/
│   │   └── BookService.cs         # Xử lý business logic
│   ├── Repositories/
│   │   └── BookRepository.cs      # Truy vấn cơ sở dữ liệu
│   ├── Data/
│   │   └── LibraryDbContext.cs    # EF Core DbContext
│   └── Entities/
│       ├── Book.cs
│       ├── Category.cs
│       ├── Reader.cs
│       ├── BorrowTicket.cs
│       └── Account.cs
│
├── Admin/                          # Website quản trị viên
│   ├── Controllers/
│   │   ├── BookController.cs      # Quản lý sách
│   │   └── HomeController.cs
│   ├── ViewModels/
│   │   └── BookViewModel.cs       # ViewModel cho form
│   ├── Views/
│   │   ├── Book/
│   │   │   ├── Index.cshtml       # Danh sách sách
│   │   │   ├── Create.cshtml      # Form thêm sách
│   │   │   ├── Edit.cshtml        # Form sửa sách
│   │   │   ├── Details.cshtml     # Chi tiết sách
│   │   │   └── Delete.cshtml      # Xác nhận xóa
│   │   └── Home/
│   └── Program.cs                 # Cấu hình ứng dụng
│
└── Client/                         # Website bạn đọc
    ├── Controllers/
    │   ├── SearchController.cs    # Tìm kiếm sách
    │   ├── HomeController.cs      # Trang chủ
    │   └── ErrorController.cs
    ├── Views/
    │   ├── Home/
    │   │   └── Index.cshtml       # Trang chủ
    │   ├── Search/
    │   │   ├── Index.cshtml       # Tìm kiếm
    │   │   └── Details.cshtml     # Chi tiết sách
    │   └── Shared/
    └── Program.cs                 # Cấu hình ứng dụng
```

---

## ✨ Các tính năng

### **Phía Admin (Quản trị viên / Nhân viên thư viện)**

#### 1️⃣ **Xem danh sách sách**
- Hiển thị tất cả sách trong hệ thống
- Thông tin: Mã sách, Tên sách, Tác giả, Thể loại, Số lượng, Tình trạng
- URL: `/Book/Index`

#### 2️⃣ **Thêm sách mới**
- Form nhập đầy đủ thông tin sách
- Validation:
  - Mã sách không được trùng
  - Mã sách tối đa 20 ký tự
  - Tên sách không được để trống
  - Số lượng >= 0
- URL: `/Book/Create`

#### 3️⃣ **Sửa thông tin sách**
- Chỉnh sửa thông tin sách (trừ mã sách)
- Validation tương tự thêm sách
- URL: `/Book/Edit/{id}`

#### 4️⃣ **Xóa sách**
- Xóa sách khỏi hệ thống
- Kiểm tra: Sách không được đang được mượn
- Yêu cầu xác nhận
- URL: `/Book/Delete/{id}`

#### 5️⃣ **Tìm kiếm sách**
- Tìm kiếm theo tên sách hoặc tác giả
- Không phân biệt chữ hoa/thường
- URL: `/Book/Index?searchTerm=xxx`

#### 6️⃣ **Quản lý số lượng**
- Xem số lượng sách hiện có
- Cập nhật số lượng khi sửa sách
- Hiển thị badge: "Còn hàng" / "Hết hàng"

#### 7️⃣ **Quản lý thể loại**
- Lựa chọn thể loại khi thêm/sửa sách
- Hiển thị tên thể loại trong danh sách

---

### **Phía Client (Bạn đọc)**

#### 1️⃣ **Trang chủ**
- Hiển thị 6 sách nổi bật (còn hàng)
- Các nút lọc theo thể loại
- Hình ảnh bìa sách (nếu có)
- URL: `/Home`

#### 2️⃣ **Danh sách sách**
- Xem tất cả sách còn hàng
- Hiển thị dạng thẻ (card)
- Thông tin: Tên, Tác giả, Thể loại, Số lượng còn
- URL: `/Search/Index`

#### 3️⃣ **Tìm kiếm sách**
- Tìm kiếm theo tên sách hoặc tác giả
- Kết quả hiển thị dạng thẻ
- URL: `/Search/Index?searchTerm=xxx`

#### 4️⃣ **Lọc theo thể loại**
- Lọc sách theo thể loại
- Nút lọc ở đầu trang
- URL: `/Search/Index?categoryId=x`

#### 5️⃣ **Xem chi tiết sách**
- Xem toàn bộ thông tin sách
- Hình ảnh bìa sách (nếu có)
- Kiểm tra tình trạng: "Còn hàng" / "Hết hàng"
- URL: `/Search/Details/{id}`

#### 6️⃣ **Yêu cầu mượn sách**
- Nút "Gửi yêu cầu mượn" (nếu còn hàng)
- (Chức năng đang phát triển)

---

## 📖 Hướng dẫn sử dụng

### **Khởi động hệ thống**

#### **Bước 1: Cấu hình Connection String**

**File**: `Admin/appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=LOCALHOST\\SQLEXPRESS;Database=ThuVien;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**File**: `Client/appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=LOCALHOST\\SQLEXPRESS;Database=ThuVien;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

#### **Bước 2: Chạy Admin Project**
```bash
cd Admin
dotnet run
# Truy cập: https://localhost:7001
```

#### **Bước 3: Chạy Client Project**
```bash
cd Client
dotnet run
# Truy cập: https://localhost:7002
```

---

### **Hướng dẫn Admin**

#### **Xem danh sách sách**

1. Truy cập: `https://localhost:7001/Book/Index`
2. Bảng hiển thị:
   - Mã sách
   - Tên sách
   - Tác giả
   - Thể loại
   - Số lượng (badge: xanh=còn, đỏ=hết)
   - Tình trạng
3. Các nút thao tác:
   - 👁️ Xem chi tiết
   - ✏️ Sửa
   - 🗑️ Xóa

#### **Thêm sách mới**

1. Click: "➕ Thêm sách"
2. Điền form:
   - **Mã sách*** (VD: SACH001) - Bắt buộc, duy nhất
   - **Tên sách*** (VD: Lập trình C#) - Bắt buộc
   - **Tác giả** (VD: Tác giả X)
   - **Nhà xuất bản**
   - **Năm xuất bản** (VD: 2024)
   - **Thể loại** - Chọn từ dropdown
   - **Số lượng*** (VD: 5) - Bắt buộc
   - **Tình trạng** (Mặc định: Có thể mượn)
   - **URL hình ảnh** (Optional)

3. Click: "💾 Thêm sách"
4. Kết quả:
   - Thành công: Về danh sách, hiển thị "Thêm sách thành công"
   - Lỗi: Quay lại form, hiển thị thông báo lỗi

**Lỗi có thể gặp:**
- "Mã sách này đã tồn tại trong hệ thống"
- "Mã sách không được để trống"
- "Tên sách không được để trống"
- "Số lượng sách không được âm"

#### **Sửa thông tin sách**

1. Tại danh sách, click ✏️ (icon bút chì)
2. Form sửa:
   - Mã sách: Không thể thay đổi (readonly)
   - Các trường khác: Có thể sửa
3. Click: "💾 Lưu thay đổi"
4. Kết quả: Về chi tiết sách, hiển thị "Cập nhật sách thành công"

#### **Xóa sách**

1. Tại danh sách, click 🗑️ (icon thùng rác)
2. Trang xác nhận:
   - Hiển thị thông tin sách cần xóa
   - Cảnh báo: "Hành động này không thể hoàn tác"
3. Click: "🗑️ Xác nhận xóa"
4. Kết quả:
   - Thành công: Về danh sách, hiển thị "Xóa sách thành công"
   - Lỗi (sách đang mượn): Quay lại danh sách, hiển thị "Không thể xóa sách đang được mượn"

#### **Tìm kiếm sách**

1. Nhập vào ô tìm kiếm:
   - Có thể tìm theo tên sách hoặc tác giả
   - VD: "Lập trình" hoặc "Tác giả X"
2. Click: "🔍 Tìm kiếm"
3. Kết quả: Hiển thị sách phù hợp

---

### **Hướng dẫn Client**

#### **Trang chủ**

1. Truy cập: `https://localhost:7002`
2. Thấy:
   - 🎉 Hero section với tên hệ thống
   - 🔍 Ô tìm kiếm
   - 🏷️ Các nút lọc theo thể loại
   - 📚 6 sách nổi bật (thẻ card)
3. Mỗi sách hiển thị:
   - Hình bìa (nếu có)
   - Tên sách
   - Tác giả
   - Thể loại
   - Số lượng còn (badge xanh)
   - Nút "Xem chi tiết"

#### **Tìm kiếm sách**

1. Tại trang chủ, nhập vào ô tìm kiếm
2. Click: "🔍 Tìm" hoặc Enter
3. Đi đến trang: `/Search/Index?searchTerm=xxx`
4. Kết quả: Danh sách sách phù hợp

#### **Lọc theo thể loại**

**Từ trang chủ:**
1. Click nút thể loại (VD: "Văn học", "Khoa học")
2. Hiển thị sách thuộc thể loại đó

**Từ trang danh sách:**
1. Nhìn thanh lọc ở đầu trang
2. Click thể loại cần lọc
3. Kết quả cập nhật

#### **Xem chi tiết sách**

1. Click "👁️ Xem chi tiết" trên thẻ sách
2. Trang chi tiết hiển thị:
   - Hình bìa sách (lớn)
   - Tên sách
   - Mã sách
   - Tác giả
   - Nhà xuất bản
   - Năm xuất bản
   - Thể loại (badge)
   - Tình trạng: "Còn hàng" hoặc "Hết hàng"
3. Nếu còn hàng: Hiển thị nút "📖 Gửi yêu cầu mượn"
4. Click "⬅️ Quay lại" để về danh sách

---

## 💻 Hướng dẫn Phát triển

### **Code Structure**

#### **IBookService.cs** - Interface
```csharp
public interface IBookService
{
    Task<List<Book>> GetAllBooksAsync();
    Task<Book?> GetBookByIdAsync(string bookId);
    Task<List<Book>> SearchBooksAsync(string searchTerm);
    // ... più metodi
}
```

#### **BookService.cs** - Implementation
```csharp
public class BookService : IBookService
{
    private readonly BookRepository _repository;

    public BookService(BookRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Book>> GetAllBooksAsync()
    {
        return await _repository.GetAllAsync();
    }
    // ... implementation
}
```

#### **BookRepository.cs** - Data Access
```csharp
public class BookRepository
{
    private readonly LibraryDbContext _context;

    public async Task<List<Book>> GetAllAsync()
    {
        return await _context.Books
            .Include(b => b.Category)
            .AsNoTracking()
            .ToListAsync();
    }
    // ... more methods
}
```

### **Dependency Injection** (Program.cs)

```csharp
// Đăng ký Repository
builder.Services.AddScoped<BookRepository>();

// Đăng ký Service
builder.Services.AddScoped<IBookService, BookService>();
```

### **Controller Usage**

```csharp
public class BookController : Controller
{
    private readonly IBookService _bookService;

    public BookController(IBookService bookService)
    {
        _bookService = bookService;
    }

    public async Task<IActionResult> Index()
    {
        var books = await _bookService.GetAllBooksAsync();
        return View(books);
    }
}
```

---

## ⚠️ Xử lý Lỗi

### **Lỗi Connection String**

**Triệu chứng**: "Cannot open database connection"

**Giải pháp**:
1. Kiểm tra SQL Server đang chạy
2. Kiểm tra connection string đúng
3. Kiểm tra database "ThuVien" tồn tại
4. Kiểm tra quyền truy cập

### **Lỗi DbContext**

**Triệu chứng**: "Unable to resolve service for type LibraryDbContext"

**Giải pháp**:
1. Thêm trong Program.cs:
```csharp
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(connectionString));
```

### **Lỗi Service không được inject**

**Triệu chứng**: "Unable to resolve service for type IBookService"

**Giải pháp**:
1. Thêm trong Program.cs:
```csharp
builder.Services.AddScoped<BookRepository>();
builder.Services.AddScoped<IBookService, BookService>();
```

### **Lỗi View không tìm thấy**

**Triệu chứng**: "The view 'Index' was not found"

**Giải pháp**:
1. Kiểm tra đường dẫn folder: `Views/Book/Index.cshtml`
2. Kiểm tra file extension: `.cshtml`
3. Kiểm tra tên controller: `BookController` → `Book/`

### **Lỗi Model Validation**

**Triệu chứng**: Form không submit, hiển thị lỗi

**Giải pháp**:
1. Kiểm tra Form validation messages
2. Điền đầy đủ trường bắt buộc (*)
3. Kiểm tra format dữ liệu

---

## 📊 Kiến thức Cơ bản

### **Entity Framework Core**

```csharp
// Query
var books = await _context.Books
    .Include(b => b.Category)        // Load related data
    .Where(b => b.Quantity > 0)      // Filter
    .AsNoTracking()                  // Read-only
    .ToListAsync();                  // Async execution

// Insert
_context.Books.Add(book);
await _context.SaveChangesAsync();

// Update
_context.Books.Update(book);
await _context.SaveChangesAsync();

// Delete
_context.Books.Remove(book);
await _context.SaveChangesAsync();
```

### **Async/Await**

```csharp
// Async method
public async Task<List<Book>> GetBooksAsync()
{
    return await _context.Books.ToListAsync();
}

// Calling async method
var books = await bookService.GetBooksAsync();
```

### **Repository Pattern**

```
Controller
    ↓
Service (Business Logic)
    ↓
Repository (Data Access)
    ↓
DbContext (EF Core)
    ↓
Database
```

---

## ✅ Checklist Triển khai

- [ ] Cấu hình connection string
- [ ] Kiểm tra database tồn tại
- [ ] Chạy Admin project
- [ ] Chạy Client project
- [ ] Kiểm tra form thêm sách
- [ ] Kiểm tra form sửa sách
- [ ] Kiểm tra xóa sách
- [ ] Kiểm tra tìm kiếm
- [ ] Kiểm tra lọc thể loại
- [ ] Kiểm tra responsive design

---

## 📚 Tài liệu Bổ sung

- **QUICK_REFERENCE.md** - Tham chiếu nhanh
- **PROJECT_STRUCTURE.md** - Cấu trúc dự án
- **IMPLEMENTATION_SUMMARY.md** - Tổng quan kỹ thuật

---

## ✨ Thông tin Bổ sung

**Trạng thái Build**: ✅ THÀNH CÔNG

Tất cả project biên dịch thành công và sẵn sàng chạy!

