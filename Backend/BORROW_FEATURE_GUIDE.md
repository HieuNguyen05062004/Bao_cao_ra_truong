# 📚 Chức Năng Mượn Sách - Tài Liệu Hướng Dẫn

## 🎯 Tổng Quan

Chức năng mượn sách cho phép bạn đọc (Client) gửi yêu cầu mượn sách, và nhân viên thư viện (Admin/Staff) có thể duyệt/từ chối yêu cầu và quản lý việc trả sách. Khi yêu cầu được duyệt, số lượng sách sẽ tự động cập nhật.

---

## 📋 Quy Trình Mượn Sách

### 1. **Bạn Đọc Gửi Yêu Cầu Mượn (Client)**

#### Các Trường Tự Động Điền:
- ✅ **Mã Bạn Đọc** → Lấy từ Session/User Identity
- ✅ **Tên Bạn Đọc** → Lấy từ Database (Reader table)
- ✅ **Mã Sách** → Từ trang chi tiết sách
- ✅ **Tên Sách** → Hiển thị danh sách sách được chọn

#### Các Trường Bạn Đọc Nhập:
- 📅 **Ngày Mượn** → Ngày bắt đầu (không được là ngày quá khứ)
- 📅 **Ngày Trả Dự Kiến** → Ngày trả (phải sau ngày mượn)

#### Flow:
```
1. Bạn đọc vào trang chi tiết sách
2. Nhấn nút "Gửi yêu cầu mượn"
3. Điền ngày mượn/trả
4. Xác nhận gửi
5. Yêu cầu được tạo với status = "Pending"
```

---

### 2. **Nhân Viên Duyệt Yêu Cầu (Admin)**

#### Menu: **Quản Lý → Mượn Sách**

#### Các Thao Tác:
| Hành Động | Điều Kiện | Kết Quả |
|-----------|-----------|---------|
| **Duyệt** | Status = "Pending" | ✅ Status → "Approved" <br> ✅ Gán nhân viên <br> ✅ Giảm số lượng sách |
| **Từ Chối** | Status = "Pending" | ❌ Status → "Rejected" <br> ❌ Số lượng không thay đổi |
| **Xác Nhận Trả Sách** | Status = "Approved" | ✅ Status → "Returned" <br> ✅ Ghi nhận ngày trả <br> ✅ Tăng số lượng sách |

---

## 🏗️ Cấu Trúc Code

### Core.Shared

#### Interface & Service:
- 📄 **IBorrowService** - Interface quản lý mượn sách
- 📄 **BorrowService** - Implementation, validation logic
- 📄 **BorrowRepository** - Truy vấn DB, transaction handling

#### Repositories & Services:
```
IBorrowService → BorrowService → BorrowRepository → LibraryDbContext
```

### Client

#### Controller:
- 📄 **Client/Controllers/BorrowController.cs**
  - `CreateBorrowRequest()` - Form mượn sách
  - `SubmitBorrowRequest()` - Gửi yêu cầu
  - `BorrowHistory()` - Lịch sử mượn
  - `BorrowDetail()` - Chi tiết yêu cầu

#### Views:
- 📄 **Client/Views/Borrow/CreateBorrowRequest.cshtml** - Form mượn
- 📄 **Client/Views/Borrow/BorrowHistory.cshtml** - Lịch sử
- 📄 **Client/Views/Borrow/BorrowDetail.cshtml** - Chi tiết

#### ViewModels:
- 📄 **BorrowRequestViewModel** - Data binding form
- 📄 **BorrowTicketApprovalViewModel** - Data approval admin
- 📄 **BorrowBookItemViewModel** - Item sách trong danh sách

### Admin

#### Controller:
- 📄 **Admin/Controllers/BorrowController.cs**
  - `Index()` - Danh sách yêu cầu (filter by status)
  - `Details()` - Chi tiết yêu cầu
  - `Approve()` - Duyệt yêu cầu
  - `Reject()` - Từ chối yêu cầu
  - `ReturnBooks()` - Xác nhận trả sách

#### Views:
- 📄 **Admin/Views/Borrow/Index.cshtml** - Danh sách
- 📄 **Admin/Views/Borrow/Details.cshtml** - Chi tiết + Hành động

---

## 💾 Database & BorrowTicket Entity

### Entity Hiện Tại:
```csharp
public class BorrowTicket
{
    public int TicketId { get; set; }              // Mã yêu cầu mượn
    public string? ReaderId { get; set; }          // Mã bạn đọc
    public string? StaffUsername { get; set; }     // Nhân viên duyệt
    public DateTime? BorrowDate { get; set; }      // Ngày mượn
    public DateTime? DueDate { get; set; }         // Ngày trả dự kiến
    public DateTime? ReturnDate { get; set; }      // Ngày trả thực tế
    public string? Status { get; set; }            // Pending/Approved/Returned/Rejected

    public virtual Reader? Reader { get; set; }
    public virtual Account? StaffUsernameNavigation { get; set; }
    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
```

### Status Flow:
```
Pending → Approved → Returned
   ↓
Rejected
```

---

## 🔄 Transaction Handling

### Approve (Duyệt):
```csharp
using (var transaction = _context.Database.BeginTransactionAsync())
{
    1. Update BorrowTicket (Status = "Approved", Staff)
    2. For each book: book.Quantity--
    3. SaveChanges()
    4. CommitTransaction()
}
```

### Return (Trả Sách):
```csharp
using (var transaction = _context.Database.BeginTransactionAsync())
{
    1. Update BorrowTicket (Status = "Returned", ReturnDate)
    2. For each book: book.Quantity++
    3. SaveChanges()
    4. CommitTransaction()
}
```

---

## 🚀 Cách Sử Dụng

### 📱 Client:

1. **Tìm sách** → Vào trang "Tìm Kiếm"
2. **Chọn sách** → Vào chi tiết sách
3. **Mượn sách** → Nhấn "Gửi yêu cầu mượn"
4. **Điền thông tin** → Ngày mượn, ngày trả
5. **Xác nhận** → Gửi yêu cầu
6. **Theo dõi** → Menu "Lịch sử mượn sách"

### 👨‍💼 Admin/Staff:

1. **Menu** → Quản Lý → Mượn Sách
2. **Lọc** → Chọn status (Chờ Duyệt, Đã Duyệt, Đã Trả, Bị Từ Chối)
3. **Xem chi tiết** → Nhấn "Chi Tiết"
4. **Duyệt** → "Duyệt" hoặc "Từ Chối"
5. **Trả sách** → Khi nhận sách từ bạn đọc, nhấn "Xác Nhận Trả Sách"

---

## ✅ Validation

### Client Side:
- ✅ Ngày mươn ≥ Hôm nay
- ✅ Ngày trả > Ngày mượn
- ✅ Chọn ít nhất 1 sách
- ✅ Sách phải có quantity > 0

### Server Side:
- ✅ Tất cả validation của Client
- ✅ Kiểm tra sách còn hàng
- ✅ Kiểm tra ReaderID hợp lệ

---

## 🔐 TODO: Authentication

Hiện tại, code sử dụng placeholder:
```csharp
// Client
string readerId = "R001";  // TODO: Lấy từ Session/User Identity

// Admin
string staffUsername = User.Identity?.Name ?? "admin";  // TODO: Lấy từ User.Identity
```

### Cần Cập Nhật:

**Client/Controllers/BorrowController.cs:**
```csharp
[HttpPost]
public async Task<IActionResult> SubmitBorrowRequest(BorrowRequestViewModel model)
{
    // TODO: Lấy ReaderId từ Session hoặc User.Identity
    var readerId = HttpContext.Session.GetString("ReaderId") 
                   ?? User.FindFirst("ReaderId")?.Value;
}
```

**Admin/Controllers/BorrowController.cs:**
```csharp
[HttpPost]
public async Task<IActionResult> Approve(int id)
{
    // TODO: Lấy username từ User.Identity
    var staffUsername = User.Identity?.Name ?? User.FindFirst(ClaimTypes.Name)?.Value;
}
```

---

## 📊 Kiểm Tra Kết Quả

### Database Queries:

```sql
-- Xem tất cả yêu cầu mượn
SELECT * FROM BorrowTickets 
ORDER BY BorrowDate DESC;

-- Xem sách còn lại sau khi duyệt
SELECT BookId, Title, Quantity 
FROM Books 
WHERE BookId = 'B001';

-- Xem yêu cầu chờ duyệt
SELECT * FROM BorrowTickets 
WHERE Status = 'Pending';
```

---

## 🎨 UI/UX Features

### Client:
- ✅ Form auto-fill (ReaderId, ReaderName, BookTitle)
- ✅ Date picker cho ngày mượn/trả
- ✅ Status badge (Chờ Duyệt, Đã Duyệt, Đã Trả, Bị Từ Chối)
- ✅ Book card preview với ảnh
- ✅ Success/Error toast notifications

### Admin:
- ✅ Filter by status
- ✅ Table responsive
- ✅ Detail modal với approve/reject buttons
- ✅ Confirm dialog trước approve/reject/return
- ✅ Staff name display
- ✅ Book grid preview

---

## 🐛 Troubleshooting

| Vấn Đề | Nguyên Nhân | Giải Pháp |
|--------|-----------|----------|
| Không thấy danh mục sách | Service chưa được đăng ký | Kiểm tra Program.cs |
| Số lượng sách không cập nhật | Transaction rollback | Kiểm tra DB logs |
| Form mượn không hiển thị | Session chưa set | Implement authentication |
| Validation failed | Request không hợp lệ | Kiểm tra browser console |

---

## 📞 Support

Nếu gặp lỗi:
1. Kiểm tra Output window → Build/Debug logs
2. Kiểm tra SQL Server: `SELECT * FROM BorrowTickets`
3. Clear cache + Refresh browser (Ctrl+Shift+Delete)

---

**Happy Borrowing! 📚✨**
