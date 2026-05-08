# 🧪 Hướng dẫn Kiểm tra Hệ thống

## 📋 Test Scenarios

### **Part 1: Admin Portal Testing**

---

## ✅ Test Case 1: Xem Danh sách Sách

**Mục đích**: Kiểm tra hiển thị danh sách sách

**Bước thực hiện**:
1. Khởi động Admin project: `dotnet run`
2. Truy cập: `https://localhost:7001/Book/Index`

**Kết quả mong đợi**:
- ✅ Trang tải thành công
- ✅ Hiển thị bảng danh sách sách
- ✅ Cột: Mã sách, Tên, Tác giả, Thể loại, Số lượng, Tình trạng
- ✅ Nút thao tác: 👁️ Xem, ✏️ Sửa, 🗑️ Xóa
- ✅ Nút thêm sách: ➕ Thêm sách

---

## ✅ Test Case 2: Thêm Sách Mới

**Mục đích**: Kiểm tra chức năng thêm sách

**Bước thực hiện**:
1. Click nút "➕ Thêm sách"
2. Điền form:
   ```
   Mã sách:           SACH001
   Tên sách:          Lập trình C#
   Tác giả:           Tác giả X
   Nhà xuất bản:      NXB Y
   Năm xuất bản:      2024
   Thể loại:          (Chọn từ dropdown)
   Số lượng:          5
   Tình trạng:        Có thể mượn
   ```
3. Click "💾 Thêm sách"

**Kết quả mong đợi**:
- ✅ Form validate không báo lỗi
- ✅ Redirect về danh sách
- ✅ Thông báo: "Thêm sách thành công"
- ✅ Sách mới có trong danh sách

**Test Error Cases**:
- Mã sách trống: "Mã sách không được để trống"
- Tên sách trống: "Tên sách không được để trống"
- Số lượng âm: "Số lượng không được âm"
- Mã sách trùng: "Mã sách này đã tồn tại"

---

## ✅ Test Case 3: Xem Chi tiết Sách

**Mục đích**: Kiểm tra xem chi tiết sách

**Bước thực hiện**:
1. Từ danh sách, click 👁️ (xem chi tiết)
2. Hoặc truy cập: `/Book/Details/SACH001`

**Kết quả mong đợi**:
- ✅ Hiển thị toàn bộ thông tin sách
- ✅ Có nút: "Quay lại", "Sửa", "Xóa"
- ✅ Format rõ ràng, dễ đọc

---

## ✅ Test Case 4: Sửa Thông tin Sách

**Mục đích**: Kiểm tra chức năng sửa sách

**Bước thực hiện**:
1. Click ✏️ (sửa) trên danh sách
2. Hoặc từ chi tiết, click "Sửa"
3. Chỉnh sửa: Tên sách, Tác giả, Số lượng
4. Click "💾 Lưu thay đổi"

**Kết quả mong đợi**:
- ✅ Mã sách không thể chỉnh sửa (readonly)
- ✅ Form validate thành công
- ✅ Redirect về chi tiết sách
- ✅ Thông báo: "Cập nhật sách thành công"
- ✅ Dữ liệu được cập nhật trong database

---

## ✅ Test Case 5: Xóa Sách

**Mục đích**: Kiểm tra chức năng xóa sách

**Bước thực hiện - Trường hợp 1 (Xóa thành công)**:
1. Từ danh sách, click 🗑️ (xóa)
2. Trang xác nhận hiển thị thông tin sách
3. Click "🗑️ Xác nhận xóa"

**Kết quả mong đợi**:
- ✅ Hiển thị trang xác nhận
- ✅ Xóa thành công, hiển thị "Xóa sách thành công"
- ✅ Redirect về danh sách
- ✅ Sách biến mất khỏi danh sách

**Bước thực hiện - Trường hợp 2 (Xóa thất bại)**:
1. Thêm sách vào một BorrowTicket (nếu có)
2. Cố gắng xóa sách đó

**Kết quả mong đợi**:
- ✅ Hiển thị lỗi: "Không thể xóa sách đang được mượn"
- ✅ Quay lại danh sách, sách vẫn tồn tại

---

## ✅ Test Case 6: Tìm kiếm Sách

**Mục đích**: Kiểm tra chức năng tìm kiếm

**Bước thực hiện**:
1. Nhập vào ô tìm kiếm: "Lập trình"
2. Click "🔍 Tìm kiếm"

**Kết quả mong đợi**:
- ✅ Hiển thị sách chứa "Lập trình" trong tên hoặc tác giả
- ✅ URL: `/Book/Index?searchTerm=Lập+trình`
- ✅ Có nút "🔍 Tìm kiếm" để reset

**Test Cases Bổ sung**:
- Tìm theo tác giả: "Tác giả X" → Hiển thị sách của tác giả
- Tìm không phân biệt hoa/thường: "lập TRÌNH" → Hoạt động
- Tìm rỗng: Reset về danh sách đầy đủ

---

## ✅ Test Case 7: Lỗi Validation

**Mục đích**: Kiểm tra validation dữ liệu

**Test Scenarios**:

| Test | Input | Expected Result |
|------|-------|-----------------|
| Mã sách trống | (trống) | "Mã sách không được để trống" |
| Tên trống | (trống) | "Tên sách không được để trống" |
| Mã sách quá dài | SACH0000000000000000000 | "Tối đa 20 ký tự" |
| Tên quá dài | (255+ ký tự) | "Tối đa 255 ký tự" |
| Số lượng âm | -5 | "Số lượng không được âm" |
| Năm không hợp lệ | 999 | "Năm từ 1000-2100" |
| Mã trùng | SACH001 (existing) | "Mã sách này đã tồn tại" |

---

### **Part 2: Client Portal Testing**

---

## ✅ Test Case 8: Xem Trang Chủ

**Mục đích**: Kiểm tra trang chủ client

**Bước thực hiện**:
1. Khởi động Client project: `dotnet run`
2. Truy cập: `https://localhost:7002`

**Kết quả mong đợi**:
- ✅ Trang tải thành công
- ✅ Hiển thị Hero section
- ✅ Hiển thị ô tìm kiếm
- ✅ Hiển thị nút lọc thể loại
- ✅ Hiển thị 6 sách nổi bật (dạng thẻ)
- ✅ Mỗi thẻ sách hiển thị: hình ảnh, tên, tác giả, thể loại, số lượng
- ✅ Nút "Xem chi tiết" hoạt động

---

## ✅ Test Case 9: Tìm kiếm Sách (Client)

**Mục đích**: Kiểm tra tìm kiếm từ phía client

**Bước thực hiện**:
1. Nhập vào ô tìm kiếm: "Lập trình"
2. Click "🔍 Tìm" hoặc Enter

**Kết quả mong đợi**:
- ✅ Redirect đến `/Search/Index?searchTerm=Lập+trình`
- ✅ Hiển thị kết quả tìm kiếm
- ✅ Format thẻ card
- ✅ Có nút "Xem chi tiết" cho mỗi sách

---

## ✅ Test Case 10: Lọc Theo Thể loại

**Mục đích**: Kiểm tra lọc sách theo thể loại

**Bước thực hiện**:
1. Click nút thể loại: "Văn học" (hoặc thể loại có sách)
2. Hoặc truy cập: `/Search/Index?categoryId=1`

**Kết quả mong đợi**:
- ✅ Hiển thị sách thuộc thể loại đó
- ✅ Nút filter được highlight (active)
- ✅ Nút "Tất cả" để xem toàn bộ

---

## ✅ Test Case 11: Xem Chi tiết Sách (Client)

**Mục đích**: Kiểm tra xem chi tiết sách client

**Bước thực hiện**:
1. Click "👁️ Xem chi tiết" trên thẻ sách
2. Hoặc truy cập: `/Search/Details/SACH001`

**Kết quả mong đợi**:
- ✅ Hiển thị toàn bộ thông tin sách
- ✅ Hình ảnh bìa (lớn)
- ✅ Tên, Mã, Tác giả, Nhà xuất bản, Năm
- ✅ Thể loại (badge)
- ✅ Tình trạng: "Còn hàng" hoặc "Hết hàng"
- ✅ Nút: "⬅️ Quay lại"
- ✅ Nếu còn hàng: Nút "📖 Gửi yêu cầu mượn" (chức năng tương lai)

---

## ✅ Test Case 12: Responsive Design

**Mục đích**: Kiểm tra hiển thị trên các kích thước màn hình

**Bước thực hiện**:
1. Xem trên Desktop (1920x1080)
2. Xem trên Tablet (768x1024)
3. Xem trên Mobile (375x667)
4. Sử dụng Chrome DevTools F12 → Device Toggle

**Kết quả mong đợi**:
- ✅ Layout responsive
- ✅ Danh sách: Desktop 3 cột → Mobile 1 cột
- ✅ Navigation hoạt động trên mobile
- ✅ Không có horizontal scroll trên mobile
- ✅ Text đủ lớn để đọc trên mobile

---

### **Part 3: Error Handling Testing**

---

## ✅ Test Case 13: Lỗi Connection

**Mục đích**: Kiểm tra xử lý khi mất kết nối database

**Bước thực hiện**:
1. Tắt SQL Server
2. Truy cập: `/Book/Index`

**Kết quả mong đợi**:
- ✅ Hiển thị thông báo lỗi
- ✅ Không bị crash
- ✅ Có thông báo rõ ràng

---

## ✅ Test Case 14: URL Không Tồn Tại

**Mục đích**: Kiểm tra xử lý 404

**Bước thực hiện**:
1. Truy cập: `/Book/Details/KHONGTONAI`
2. Hoặc: `/Book/Edit/KHONGTONAI`

**Kết quả mong đợi**:
- ✅ Redirect về danh sách
- ✅ Hiển thị thông báo lỗi: "Không tìm thấy sách"
- ✅ Có nút quay lại

---

## ✅ Test Case 15: Exception Handling

**Mục đích**: Kiểm tra xử lý ngoại lệ

**Bước thực hiện**:
1. Modify code để simulate exception
2. Hoặc trigger edge cases

**Kết quả mong đợi**:
- ✅ Không bị unhandled exception
- ✅ Hiển thị user-friendly error message
- ✅ Log exception (nếu có)

---

## 📊 Test Summary Checklist

```
Admin Portal (15 Tests):
☐ Xem danh sách sách
☐ Thêm sách mới
☐ Xem chi tiết sách
☐ Sửa thông tin sách
☐ Xóa sách (thành công)
☐ Xóa sách (thất bại - đang mượn)
☐ Tìm kiếm sách
☐ Lỗi validation (7 sub-tests)

Client Portal (7 Tests):
☐ Xem trang chủ
☐ Tìm kiếm sách
☐ Lọc theo thể loại
☐ Xem chi tiết sách
☐ Responsive design (3 sizes)

Error Handling (3 Tests):
☐ Lỗi connection
☐ URL không tồn tại
☐ Exception handling

Total: 25 Test Cases
```

---

## 🎯 Pass/Fail Criteria

**PASS**: Tất cả test cases đều ✅

**FAIL**: Bất kỳ test case nào ❌ hoặc lỗi không được xử lý

---

## 🧪 Tính năng mở rộng (Phase 2)

Những test cases cần thêm sau khi implement:

```
☐ Borrow Management
☐ Authentication/Authorization
☐ Book Ratings & Reviews
☐ Wishlist
☐ Email Notifications
☐ Report Generation
☐ Pagination
☐ Caching
```

---

## 📝 Test Report Template

```
Ngày test: ___/___/____
Người test: _______________
Phiên bản: 1.0

Test Results:
✅ Admin Portal:    15/15 PASS
✅ Client Portal:    7/7 PASS
✅ Error Handling:   3/3 PASS

Total Score: 25/25 (100%)
Status: ✅ PASSED

Ghi chú:
_________________________________
_________________________________

Người duyệt:
_______________ (Ký tên)
```

---

## ✅ Kết luận

Nếu tất cả test cases đều **PASS**, hệ thống sẵn sàng:
- ✅ Triển khai
- ✅ Bàn giao
- ✅ Sử dụng thực tế

---

*Happy Testing! 🎉*
