# Bao_cao_ra_truong

hệ thống quản lý thư viện tích hợp tìm kiếm nâng cao(tích hợp AI cho tìm kiếm)

# 📚 MÔ TẢ CHI TIẾT LUỒNG NGHIỆP VỤ HỆ THỐNG

## Hệ thống quản lý thư viện tích hợp tìm kiếm nâng cao (AI Search)

Hệ thống quản lý thư viện được xây dựng theo mô hình phân tầng sử dụng ASP.NET Core MVC kết hợp Service Layer và Repository Pattern. Kiến trúc này giúp tách biệt giữa giao diện người dùng, xử lý nghiệp vụ và truy cập dữ liệu nhằm tăng khả năng bảo trì, mở rộng và kiểm thử hệ thống. Trong mô hình này, Controller chịu trách nhiệm nhận request từ người dùng, Service xử lý nghiệp vụ và Repository thực hiện thao tác với cơ sở dữ liệu SQL Server thông qua Entity Framework Core. ([Infragistics][1])

---

# 🧑‍💼 LUỒNG NGHIỆP VỤ PHÍA ADMIN

Admin là người có quyền quản lý toàn bộ hệ thống, bao gồm quản lý sách, danh mục, bạn đọc, nhân viên, mượn trả sách và thống kê dữ liệu.

---

# 📖 LUỒNG NGHIỆP VỤ QUẢN LÝ SÁCH

Khi admin truy cập chức năng quản lý sách, hệ thống sẽ hiển thị danh sách toàn bộ dữ liệu từ bảng `Books`. Dữ liệu được lấy thông qua `BookController`, sau đó controller gọi `IBookService` để xử lý nghiệp vụ và truy xuất dữ liệu từ `BookRepository`.

Khi admin thực hiện thêm sách, hệ thống sẽ điều hướng đến giao diện tạo sách mới. Form nhập liệu bao gồm các trường:

- mã sách (`BookID`)
- tên sách (`Title`)
- tác giả (`Author`)
- nhà xuất bản (`Publisher`)
- năm xuất bản (`PublishYear`)
- danh mục (`CategoryID`)
- số lượng (`Quantity`)
- ảnh bìa (`ImageURL`)

Sau khi admin nhập dữ liệu và nhấn nút thêm, `BookController` sẽ nhận dữ liệu từ `BookViewModel` và gửi sang `BookService`. Tại đây hệ thống sẽ:

- kiểm tra dữ liệu hợp lệ
- kiểm tra mã sách đã tồn tại hay chưa
- xử lý upload ảnh bằng `FileHelper`
- chuẩn hóa dữ liệu trước khi lưu

Sau khi xử lý thành công, `BookRepository` sẽ thêm dữ liệu vào bảng `Books`. Nếu có lỗi, Service sẽ trả thông báo lỗi về Controller để hiển thị cho người dùng.

Trong chức năng sửa sách, hệ thống sẽ lấy dữ liệu hiện tại của sách từ database và hiển thị lên form chỉnh sửa. Sau khi admin cập nhật dữ liệu, Service sẽ thực hiện kiểm tra và cập nhật lại thông tin trong bảng `Books`.

Trong chức năng xóa sách, admin có thể chọn một hoặc nhiều sách để xóa. Trước khi xóa, hệ thống hiển thị hộp thoại xác nhận nhằm tránh thao tác nhầm. Sau khi xác nhận, `BookService` kiểm tra xem sách có đang nằm trong phiếu mượn hay không. Nếu không có ràng buộc, `BookRepository` sẽ thực hiện xóa dữ liệu khỏi database.

Ngoài ra, số lượng sách không được chỉnh sửa trực tiếp mà được cập nhật tự động thông qua nghiệp vụ mượn và trả sách:

- khi mượn → giảm số lượng
- khi trả → tăng số lượng

Nếu số lượng bằng 0, trạng thái sách sẽ chuyển thành:

```text id="b9l3tf"
Đã mượn hết
```

Ngược lại:

```text id="m0d5ve"
Có thể mượn
```

---

# 👨‍💻 LUỒNG NGHIỆP VỤ QUẢN LÝ NHÂN VIÊN

Chức năng quản lý nhân viên được thực hiện thông qua `StaffController`. Dữ liệu nhân viên được lưu trong bảng `Accounts`.

Khi admin thêm nhân viên mới, hệ thống hiển thị form nhập:

- tên đăng nhập
- mật khẩu
- họ tên
- email
- vai trò
- ảnh đại diện

Sau khi nhấn thêm, `AuthService` sẽ:

- kiểm tra username trùng lặp
- kiểm tra dữ liệu hợp lệ
- mã hóa mật khẩu
- gán quyền (`Admin` hoặc `Staff`)

Sau đó dữ liệu được lưu xuống bảng `Accounts`.

Khi sửa nhân viên, hệ thống lấy thông tin hiện tại từ database và hiển thị lên form. Sau khi cập nhật, dữ liệu được kiểm tra lại trước khi lưu.

Khi xóa nhân viên, hệ thống kiểm tra xem tài khoản có đang tham gia xử lý phiếu mượn hay không. Nếu không có ràng buộc, dữ liệu sẽ được xóa khỏi database.

---

# 📥 LUỒNG NGHIỆP VỤ MƯỢN SÁCH

Đây là một trong những nghiệp vụ quan trọng nhất của hệ thống.

Khi bạn đọc chọn sách và gửi yêu cầu mượn, request sẽ được gửi từ `Client/BorrowController` đến `BorrowService`.

`BorrowService` sẽ thực hiện các bước:

- kiểm tra người dùng tồn tại
- kiểm tra sách có tồn tại
- kiểm tra số lượng sách còn hay không
- kiểm tra người dùng có đang nợ sách quá hạn không

Nếu hợp lệ, hệ thống sẽ:

1. tạo dữ liệu trong bảng `BorrowTickets`
2. tạo dữ liệu trong bảng `BorrowDetails`
3. giảm số lượng sách trong bảng `Books`
4. cập nhật trạng thái phiếu mượn:

```text id="zexv1h"
Đang mượn
```

Trong trường hợp số lượng sách bằng 0 hoặc người dùng vi phạm điều kiện mượn, hệ thống sẽ trả thông báo lỗi và không tạo phiếu mượn.

Toàn bộ nghiệp vụ này được xử lý trong Service nhằm đảm bảo tính nhất quán dữ liệu và tách biệt nghiệp vụ khỏi Controller. ([Exception Not Found][2])

---

# 📤 LUỒNG NGHIỆP VỤ TRẢ SÁCH

Khi bạn đọc trả sách, nhân viên hoặc admin sẽ xác nhận thao tác trả sách trên hệ thống.

`BorrowService` sẽ thực hiện:

- cập nhật ngày trả (`ReturnDate`)
- so sánh với hạn trả (`DueDate`)

Nếu:

```text id="wnk2ik"
ReturnDate <= DueDate
```

thì trạng thái:

```text id="zafn4s"
Đã trả
```

Nếu:

```text id="j73g4x"
ReturnDate > DueDate
```

thì trạng thái:

```text id="v4fjg9"
Quá hạn
```

Sau đó hệ thống:

- tăng lại số lượng sách trong bảng `Books`
- cập nhật trạng thái sách nếu còn hàng
- lưu lịch sử trả sách

Thông tin này sẽ được hiển thị trong thống kê và lịch sử mượn trả của bạn đọc.

---

# 🗂️ LUỒNG NGHIỆP VỤ QUẢN LÝ DANH MỤC

Danh mục sách được quản lý thông qua bảng `Categories`.

Khi thêm danh mục:

- admin nhập tên danh mục
- `CategoryController` gửi dữ liệu sang Service
- Service kiểm tra dữ liệu hợp lệ
- Repository thêm dữ liệu vào bảng `Categories`

Khi sửa:

- hệ thống lấy dữ liệu hiện tại
- admin cập nhật
- Service xử lý và cập nhật database

Khi xóa:

- hệ thống kiểm tra xem danh mục có đang được sử dụng bởi sách nào không
- nếu không có liên kết → cho phép xóa

---

# 👤 LUỒNG NGHIỆP VỤ QUẢN LÝ BẠN ĐỌC

Thông tin bạn đọc được lưu trong bảng `Readers`.

Khi thêm bạn đọc:

- admin hoặc nhân viên nhập thông tin cá nhân
- Controller nhận request
- `ReaderService` kiểm tra:
  - email hợp lệ
  - dữ liệu không trống
  - mã bạn đọc không trùng

Sau khi hợp lệ, dữ liệu được lưu xuống database.

Khi sửa:

- hệ thống hiển thị dữ liệu hiện tại
- người quản lý cập nhật thông tin
- Service cập nhật database

Khi xóa:

- hệ thống kiểm tra xem bạn đọc có đang mượn sách không
- nếu không có ràng buộc → cho phép xóa

---

# 📊 LUỒNG NGHIỆP VỤ THỐNG KÊ

Hệ thống thống kê dữ liệu dựa trên:

- bảng `Books`
- bảng `BorrowTickets`
- bảng `Readers`

Dashboard sẽ hiển thị:

- tổng số sách
- số lượng sách đang mượn
- số sách quá hạn
- số lượng bạn đọc
- số lượt mượn theo ngày/tháng/năm

Dữ liệu được hiển thị dưới dạng biểu đồ nhằm hỗ trợ admin theo dõi hoạt động thư viện.

---

# 🔍 LUỒNG NGHIỆP VỤ TÌM KIẾM

Hệ thống hỗ trợ hai loại tìm kiếm:

## Tìm kiếm cơ bản

Người dùng có thể tìm theo:

- tên sách
- tác giả
- danh mục

`SearchController` nhận từ khóa và gửi đến `SearchService`. Service sẽ xử lý truy vấn và trả về danh sách sách phù hợp.

---

## 🤖 Tìm kiếm nâng cao bằng AI

Đây là chức năng mở rộng của hệ thống.

Người dùng có thể nhập truy vấn tự nhiên như:

```text id="f6p4nc"
Sách Java cho người mới
```

hoặc:

```text id="v1zk8x"
Sách của tác giả Nguyễn Văn A thuộc thể loại CNTT
```

`SearchService` sẽ:

- phân tích từ khóa
- xác định ý định tìm kiếm
- truy xuất dữ liệu phù hợp từ database
- sắp xếp kết quả theo mức độ liên quan

Chức năng này giúp tăng trải nghiệm người dùng và hỗ trợ tìm kiếm thông minh hơn so với tìm kiếm truyền thống.

---

# 👤 LUỒNG NGHIỆP VỤ PHÍA BẠN ĐỌC

Bạn đọc sử dụng hệ thống thông qua phần `Client`.

Sau khi đăng nhập, người dùng có thể:

- tìm kiếm sách
- mượn sách
- xem lịch sử mượn trả
- cập nhật thông tin cá nhân
- đổi mật khẩu

Thông tin cá nhân và lịch sử mượn được lấy từ:

- `Readers`
- `BorrowTickets`
- `BorrowDetails`

Hệ thống sẽ hiển thị:

- sách đang mượn
- sách đã trả
- sách quá hạn

---

# 🏗️ KẾT LUẬN

Hệ thống quản lý thư viện được xây dựng theo mô hình:

```text id="ygdu1g"
Controller → Service → Repository → Database
```

Kiến trúc này giúp:

- tách biệt xử lý nghiệp vụ
- dễ bảo trì
- dễ mở rộng
- dễ kiểm thử
