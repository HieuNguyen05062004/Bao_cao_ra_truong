# Giải thích luồng nghiệp vụ, luồng sự kiện và luồng hoạt động của dự án Quản lý thư viện

## 1. Tổng quan kiến trúc

Dự án này được tổ chức theo hướng ASP.NET Core MVC tách thành 2 ứng dụng chính:

- **Admin**: khu vực quản trị cho thủ thư, admin, staff.
- **Client**: khu vực cho bạn đọc tra cứu sách, xem chi tiết, đăng nhập và gửi yêu cầu mượn.

Hai ứng dụng này dùng chung một thư viện lõi là **Core.Shared**. Đây là nơi đặt toàn bộ phần dùng chung như entity, DbContext, repository, service, hằng số, tiện ích sinh ID và thư mục lưu file upload.

Luồng chuẩn của hệ thống là:

```text
View / Form / UI
	-> Controller
	-> Service
	-> Repository
	-> LibraryDbContext
	-> SQL Server
	-> Entity
	-> Controller
	-> View
```

Nói ngắn gọn, **Controller không đi thẳng xuống database**, mà đi qua **Service** để xử lý nghiệp vụ, rồi mới xuống **Repository** để đọc ghi dữ liệu.

---

## 2. Core.Shared là gì

`Core.Shared` là lớp chia sẻ trung tâm của toàn bộ hệ thống. Mục tiêu của nó là:

- Tách logic nghiệp vụ ra khỏi Controller.
- Cho Admin và Client dùng chung một bộ entity và truy cập dữ liệu.
- Giảm trùng lặp code giữa 2 ứng dụng MVC.
- Giúp việc bảo trì và mở rộng dễ hơn.

### 2.1 Constants

Thư mục `Constants` chứa các hằng số dùng chung trên toàn hệ thống.

- `RoleConstants` quy định các vai trò hợp lệ như `Admin`, `Staff`.
- `MessageConstants` gom các chuỗi thông báo như lỗi đăng nhập, lỗi dữ liệu, lỗi xóa tài khoản, thông báo thành công.

Vai trò của folder này là chuẩn hóa thông điệp và tránh rải chuỗi cứng khắp code.

### 2.2 Data

Thư mục `Data` chứa `LibraryDbContext`, là cầu nối giữa Entity Framework Core và SQL Server.

`LibraryDbContext` khai báo các bảng chính:

- `Accounts`
- `Books`
- `BorrowTickets`
- `Categories`
- `Readers`
- `BookCategories`

Ngoài việc khai báo `DbSet`, `LibraryDbContext` còn cấu hình:

- khóa chính cho từng bảng
- kiểu dữ liệu và độ dài chuỗi
- tên cột trong database
- quan hệ giữa các bảng
- cascade delete hoặc client set null ở các quan hệ liên quan

Hiểu đơn giản, `DbContext` là bản đồ chuyển đổi giữa code C# và database thật.

### 2.3 Entities

Thư mục `Entities` chứa các class mô hình dữ liệu cốt lõi:

- `Book`
- `Category`
- `BookCategory`
- `Reader`
- `Account`
- `BorrowTicket`

Đây là các đối tượng phản ánh trực tiếp các bảng dữ liệu và quan hệ giữa chúng.

Ý nghĩa thực tế của từng entity:

- `Book`: thông tin sách, số lượng, trạng thái, ảnh, mô tả, ngày tạo.
- `Category`: danh mục sách.
- `BookCategory`: bảng nối để một sách có thể thuộc nhiều danh mục.
- `Reader`: thông tin bạn đọc, email, số điện thoại, ảnh đại diện, mật khẩu hash.
- `Account`: tài khoản cán bộ hệ thống như admin hoặc staff.
- `BorrowTicket`: phiếu mượn, ngày mượn, hạn trả, ngày trả, trạng thái, danh sách sách trong phiếu.

### 2.4 Interfaces

Thư mục `Interfaces` là nơi định nghĩa hợp đồng cho các service.

Các interface chính gồm:

- `IBookService`
- `IBorrowService`
- `ICategoryService`
- `IReaderService`
- `IAuthService`
- `IUnifiedAuthService`
- `IAiSearchService`

`ISearchService` và `SearchService` hiện đang là lớp/contract để trống, không phải luồng chính của hệ thống.

Vai trò của interface là:

- Controller chỉ phụ thuộc vào hợp đồng, không phụ thuộc cứng vào class cụ thể.
- Dễ thay thế implementation.
- Dễ viết unit test.

### 2.5 Migrations

Thư mục `Migrations` là lịch sử tiến hóa schema của database do EF Core tạo ra.

Trong project hiện có các migration chính:

- `InitialCreate`
- `AddBookDescription`
- `FixCascadeDeleteBorrowDetail`

Migrations giúp:

- Tạo database đồng bộ từ entity.
- Ghi lại từng lần thay đổi schema.
- Tránh phải tự viết SQL thủ công cho mỗi lần chỉnh model.

### 2.6 Repositories

Thư mục `Repositories` là tầng truy cập dữ liệu trực tiếp.

Các repository chính gồm:

- `BookRepository`
- `BorrowRepository`
- `CategoryRepository`
- `ReaderRepository`
- `AccountRepository`

Repository chịu trách nhiệm:

- Query EF Core.
- `Include` / `ThenInclude` dữ liệu liên quan.
- `Where`, `AnyAsync`, `ToListAsync`, `SaveChangesAsync`.
- Không xử lý luật nghiệp vụ phức tạp, chỉ lo dữ liệu.

Ví dụ thực tế:

- `BookRepository` đọc sách kèm danh mục.
- `BorrowRepository` đọc phiếu mượn kèm bạn đọc, staff và sách.
- `ReaderRepository` kiểm tra còn phiếu mượn đang mở hay không.
- `AccountRepository` kiểm tra tài khoản có phiếu mượn liên kết hay không.

### 2.7 Services

Thư mục `Services` là nơi đặt logic nghiệp vụ thật sự.

Các service chính gồm:

- `BookService`
- `BorrowService`
- `CategoryService`
- `ReaderService`
- `AuthService`
- `UnifiedAuthService`
- `AiSearchService`
- `SearchService`

Service thường làm các việc sau:

- Kiểm tra dữ liệu đầu vào.
- Áp rule nghiệp vụ.
- Gọi repository phù hợp.
- Trả về kết quả thành công/thất bại kèm thông báo rõ ràng.

#### BookService

`BookService` xử lý các nghiệp vụ của sách:

- Lấy danh sách sách.
- Lấy theo ID.
- Tìm kiếm.
- Lọc theo danh mục.
- Lấy sách khả dụng, sách nổi bật, sách đang hot.
- Thêm, sửa, xóa, cập nhật số lượng.

Điểm đáng chú ý là `SearchBooksAsync` không chỉ tìm đơn giản theo database. Nó lấy toàn bộ sách rồi tìm trong bộ nhớ, chuẩn hóa tiếng Việt, tách token và chấm điểm ưu tiên theo tiêu đề, tác giả, danh mục, mô tả, mã sách. Vì vậy kết quả tìm kiếm thường sát ý hơn tìm `Contains` đơn thuần.

#### BorrowService

`BorrowService` điều khiển luồng mượn trả:

- Bạn đọc gửi yêu cầu mượn.
- Admin duyệt yêu cầu.
- Admin xác nhận giao sách.
- Admin từ chối.
- Admin xác nhận trả sách.
- Xóa phiếu mượn khi hợp lệ.

Các trạng thái chính gồm:

- Chờ duyệt
- Đã duyệt
- Đang mượn
- Đã trả
- Bị từ chối

Service này cũng cập nhật số lượng sách khi duyệt và khi trả.

#### CategoryService

`CategoryService` quản lý danh mục:

- Thêm, sửa, xóa danh mục.
- Tìm kiếm danh mục.
- Không cho xóa danh mục nếu còn sách liên kết.

#### ReaderService

`ReaderService` quản lý bạn đọc:

- Tạo bạn đọc mới.
- Sinh ID tự động.
- Kiểm tra email, số điện thoại.
- Sửa thông tin bạn đọc.
- Xóa bạn đọc.
- Cho client tự sửa profile hoặc tự xóa tài khoản nếu đủ điều kiện.

#### AuthService

`AuthService` xử lý đăng nhập và CRUD staff/admin:

- Đăng nhập bằng username hoặc email.
- Verify mật khẩu bằng BCrypt.
- Tạo, sửa, xóa tài khoản cán bộ.
- Không cho xóa admin gốc.
- Không cho xóa tài khoản nếu còn phiếu mượn liên kết.

#### UnifiedAuthService

`UnifiedAuthService` là luồng đăng nhập chung cho cả Reader và Admin/Staff trong app Client:

- Tìm reader theo email trước.
- Nếu không có thì tìm account theo email.
- Verify mật khẩu hash.
- Trả về kiểu tài khoản để client biết người dùng là Reader hay Admin.

#### AiSearchService

`AiSearchService` gọi Google Gemini API để phân tích câu tìm kiếm tự nhiên như:

- “sách lập trình Python cho người mới”
- “sách của Nguyễn Nhật Ánh”

Service này biến câu tự nhiên thành:

- `Keyword`
- `InterpretedQuery`

Sau đó Controller dùng `Keyword` đó để tìm sách trong database.

Nếu API lỗi hoặc chưa cấu hình key, hệ thống sẽ fallback về tìm kiếm thường.

### 2.8 Uploads

Thư mục `Uploads` là nơi lưu file vật lý.

Các nhóm chính đang dùng:

- `books` cho ảnh bìa sách.
- `reader-avatars` cho ảnh đại diện bạn đọc.
- `staff-avatars` cho ảnh đại diện nhân viên.

File ảnh không chỉ nằm trong database, mà được lưu ra thư mục thật và được map thành static file để trình duyệt truy cập được qua URL như `/book-images/...`.

### 2.9 Utilities

Thư mục `Utilities` chứa helper dùng chung, đặc biệt là `IdGenerator`.

`IdGenerator` sinh mã tự động cho:

- `BookId` dạng `BKxxxxx`
- `ReaderId` dạng `RRxxxxx`
- ID chung theo prefix nếu cần

Mục đích là giảm nhập tay và đồng nhất format mã định danh trong hệ thống.

---

## 3. Core.Shared tác động qua lại với MVC như thế nào

MVC trong dự án này không làm việc trực tiếp với database. Nó làm việc theo chuỗi:

```text
Controller -> Service -> Repository -> DbContext -> Database
```

Ngược chiều lại:

```text
Database -> DbContext -> Repository -> Service -> Controller -> View
```

### 3.1 Controller nhận dữ liệu từ View

Trong Admin và Client, các form nhập liệu thường đi qua `ViewModel`.

Ví dụ:

- `BookViewModel`
- `LoginViewModel`
- `ReaderViewModel`
- `BorrowRequestViewModel`

ViewModel là lớp trung gian giữa View và Controller, giúp:

- Bind dữ liệu từ form.
- Gắn validation.
- Tránh đưa entity thô ra giao diện khi không cần thiết.

### 3.2 Controller chuyển ViewModel sang Entity

Controller thường nhận dữ liệu form, rồi tạo entity để gửi xuống service.

Ví dụ với sách:

- View gửi `BookViewModel`.
- Controller kiểm tra file ảnh, danh mục chọn, dữ liệu bắt buộc.
- Controller tạo `Book` entity.
- Controller gọi `IBookService.AddBookAsync(...)` hoặc `UpdateBookAsync(...)`.

### 3.3 Service xử lý nghiệp vụ

Service là lớp quyết định đúng sai theo luật hệ thống.

Ví dụ:

- `BookService` kiểm tra tên sách, mã sách, số lượng, trạng thái, danh mục.
- `BorrowService` kiểm tra ngày mượn, ngày trả, tồn kho, trạng thái phiếu.
- `ReaderService` kiểm tra email, số điện thoại, trùng dữ liệu.
- `AuthService` kiểm tra hash password và role.

### 3.4 Repository đọc ghi dữ liệu

Repository chỉ lo dữ liệu:

- Lấy sách kèm danh mục.
- Lấy phiếu mượn kèm reader, staff, books.
- Thêm, cập nhật, xóa entity.
- Kiểm tra tồn tại, kiểm tra ràng buộc liên kết.

### 3.5 View nhận kết quả từ Controller

Sau khi service hoàn thành, Controller sẽ trả về:

- `View(...)` để render HTML.
- `RedirectToAction(...)` để chuyển trang.
- `Json(...)` cho AJAX.
- `TempData` để hiển thị message thành công hoặc lỗi sau redirect.
- `ViewBag` để truyền dữ liệu phụ như danh mục, từ khóa tìm kiếm.

---

## 4. Luồng nghiệp vụ chính trong dự án

### 4.1 Luồng đăng nhập Admin

1. Người dùng vào trang login của Admin.
2. `AccountController.Login` nhận username và password.
3. Controller gọi `IAuthService.LoginAsync(...)`.
4. `AuthService` tìm tài khoản theo username hoặc email.
5. Password được xác thực bằng BCrypt.
6. Nếu hợp lệ và role đúng, Controller kiểm tra role bằng `RoleConstants`.
7. Nếu role hợp lệ, thông tin người dùng được lưu vào Session.
8. Người dùng được chuyển sang dashboard.

Đây là luồng cho cán bộ hệ thống, không phải bạn đọc.

### 4.2 Luồng quản lý sách

1. Admin mở danh sách sách.
2. `BookController.Index` gọi `IBookService.GetAllBooksAsync()` hoặc `SearchBooksAsync()`.
3. `BookService` gọi `BookRepository`.
4. Repository đọc sách, kèm danh mục, trả về entity.
5. Controller đẩy dữ liệu sang View để hiển thị.

Khi thêm sách:

1. Form gửi `BookViewModel` và file ảnh.
2. Controller lưu ảnh vào thư mục upload.
3. Controller convert sang `Book`.
4. `BookService.AddBookAsync(...)` kiểm tra dữ liệu và sinh mã sách nếu cần.
5. `BookRepository.AddAsync(...)` lưu xuống database.
6. Quan hệ sách-danh mục được lưu vào bảng nối `BookCategories`.

Khi sửa hoặc xóa sách, luồng cũng tương tự nhưng sẽ có thêm kiểm tra ràng buộc như không xóa sách đang được mượn.

### 4.3 Luồng quản lý danh mục

1. Admin tạo hoặc chỉnh sửa danh mục.
2. Controller gọi `ICategoryService`.
3. Service kiểm tra tên danh mục không rỗng, không quá dài, không trùng.
4. Repository lưu vào database.
5. Khi xóa danh mục, service kiểm tra danh mục còn sách liên kết hay không.
6. Nếu còn sách, hệ thống từ chối xóa để tránh mất dữ liệu quan hệ.

### 4.4 Luồng quản lý bạn đọc

1. Admin tạo bạn đọc mới hoặc bạn đọc tự đăng ký.
2. `ReaderService` sinh `ReaderId` tự động bằng `IdGenerator`.
3. Service kiểm tra email, số điện thoại, dữ liệu bắt buộc.
4. Password của bạn đọc được hash trước khi lưu.
5. Repository ghi xuống bảng `Readers`.

Khi sửa hoặc xóa, `ReaderService` kiểm tra xem bạn đọc có phiếu mượn chưa trả hay không để chặn thao tác nguy hiểm.

### 4.5 Luồng mượn trả

Đây là phần nghiệp vụ quan trọng nhất của hệ thống.

#### Bạn đọc gửi yêu cầu mượn

1. Client chọn sách và gửi yêu cầu.
2. `BorrowService.CreateBorrowRequestAsync(...)` kiểm tra reader, danh sách sách, ngày mượn, ngày trả.
3. Service kiểm tra sách còn tồn kho hay không.
4. Nếu hợp lệ, hệ thống tạo `BorrowTicket` với trạng thái `Chờ duyệt`.
5. Ticket và danh sách sách được lưu qua repository.

#### Admin duyệt phiếu

1. Admin mở danh sách phiếu mượn.
2. `BorrowService.ApproveBorrowRequestAsync(...)` kiểm tra trạng thái hiện tại.
3. Hệ thống kiểm tra lại tồn kho ngay tại thời điểm duyệt.
4. Nếu sách vẫn còn, service giảm `Quantity` của từng sách.
5. Ticket chuyển sang trạng thái `Đã duyệt` và lưu staff xử lý.

#### Admin xác nhận giao sách

1. Ticket đang ở trạng thái `Đã duyệt`.
2. `BorrowService.ConfirmBorrowingAsync(...)` chuyển phiếu sang `Đang mượn`.
3. Staff xử lý được ghi nhận.

#### Admin xác nhận trả sách

1. Khi bạn đọc trả sách, admin gọi `ReturnBooksAsync(...)`.
2. Phiếu chuyển sang `Đã trả`.
3. Số lượng sách được cộng lại vào kho.
4. Trạng thái sách đổi về `Có thể mượn` nếu còn sách.

Đây là luồng thể hiện rõ nhất cách service điều khiển nghiệp vụ, còn repository chỉ lo lưu trữ.

### 4.6 Luồng tìm kiếm

Hệ thống có 2 mức tìm kiếm:

#### Tìm kiếm thường

1. Người dùng nhập từ khóa.
2. `BookService.SearchBooksAsync(...)` lấy sách từ repository.
3. Service chuẩn hóa tiếng Việt, tách token và chấm điểm mức độ khớp.
4. Kết quả được sắp xếp theo điểm tìm kiếm rồi trả về view.

#### Tìm kiếm AI

1. Người dùng nhập câu tự nhiên.
2. `SearchController` gọi `IAiSearchService.ParseSearchQueryAsync(...)`.
3. AI trả về keyword ngắn và câu diễn giải.
4. Controller dùng keyword đó gọi lại `IBookService.SearchBooksAsync(...)`.
5. Kết quả được hiển thị cùng câu diễn giải đã hiểu từ AI.

Nếu AI lỗi, hệ thống vẫn chạy bằng cách tìm kiếm thường, không làm gãy luồng.

---

## 5. Vai trò của Model, View, Controller trong dự án này

### Model

Trong dự án này, “model” có 2 lớp nghĩa:

- **Entity** trong `Core.Shared/Entities`: đại diện dữ liệu thật trong database.
- **ViewModel** trong `Admin/ViewModels`, `Client/ViewModels`, và `Core.Shared/ViewModels`: đại diện dữ liệu phục vụ giao diện.

Entity dùng cho logic và database, còn ViewModel dùng cho form và màn hình.

### View

View là các file `.cshtml` trong Admin và Client.

View chỉ hiển thị dữ liệu, không tự xử lý nghiệp vụ quan trọng.

### Controller

Controller là lớp nhận request từ trình duyệt, gọi service, và quyết định trả về gì cho người dùng.

Controller thường dùng các thứ sau:

- `ModelState` để kiểm tra dữ liệu form.
- `ViewBag` để truyền dữ liệu phụ.
- `TempData` để báo lỗi hoặc thành công sau redirect.
- `Session` để lưu thông tin đăng nhập.

### Mối quan hệ thực tế

1. View gửi dữ liệu.
2. Controller nhận dữ liệu qua ViewModel.
3. Controller chuyển dữ liệu sang Entity nếu cần.
4. Controller gọi Service qua interface.
5. Service gọi Repository.
6. Repository làm việc với DbContext.
7. DbContext thao tác SQL Server.
8. Kết quả trả ngược về View.

---

## 6. Cách Admin và Client dùng chung Core.Shared

### Admin

Admin đăng ký và dùng chủ yếu các service sau:

- `IBookService`
- `ICategoryService`
- `IAuthService`
- `IReaderService`
- `IBorrowService`

Admin Program còn map ảnh sách và ảnh đại diện từ `Core.Shared/Uploads` ra static file để trình duyệt truy cập được.

### Client

Client dùng chung `IBookService`, `IReaderService`, `IBorrowService` và thêm:

- `IAiSearchService`
- `IUnifiedAuthService`

Client Program cũng map static file cho frontend HTML tĩnh và thư mục ảnh upload.

Điểm quan trọng là cùng một database, cùng một entity, cùng một core layer nhưng 2 ứng dụng có nhiệm vụ khác nhau.

---

## 7. Kết luận ngắn

Toàn bộ dự án được tổ chức theo mô hình:

```text
MVC UI -> Controller -> Service -> Repository -> EF Core -> SQL Server
```

Trong đó:

- `Entities` mô tả dữ liệu.
- `DbContext` kết nối database.
- `Repository` đọc ghi dữ liệu.
- `Service` giữ nghiệp vụ.
- `Controller` điều phối request.
- `View` hiển thị kết quả.

Nhờ cách tách này, hệ thống có thể mở rộng thêm tính năng như tìm kiếm nâng cao, quản lý mượn trả, phân quyền, upload ảnh và đăng nhập chung mà không làm rối Controller.
