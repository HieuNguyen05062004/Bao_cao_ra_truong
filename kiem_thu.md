-Xác định điều kiện
STT Mã nội dung Nội dung Giá trị
1 C1 Họ và tên T,B
2 C2 Email T,B,F
3 C3 Số điện thoại T,B,F
4 C4 Mật khẩu đăng nhập T,B,F
5 C5 Địa chỉ T,B
6 C6 Giới tính T,B
7 C7 Ngày sinh T,B
8 C8 Ảnh đại diện T,B

- T: Nhập đúng
- B: Để trống, không chọn
- F: Nhập sai, chọn sai
  -Xác định hoạt đông hệ thống
  STT Mã nội dung Nội dung
  1 A1 Thêm danh Bạn đọc thành công

-Bảng quyết định thêm bạn đọc
Điều kiện đầu vào TH1 TH2 TH3 TH4 TH5 TH6 TH7 TH8 TH9 TH10 TH11 TH12
C1 T T T T T T T T T T T B
C2 T T T T T T T T T B F -
C3 T T T T T T T B F - - -
C4 T T T T T B F - - - - -
C5 T T T T B - - - - - - -
C6 T T T B T - - - - - - -
C7 T T B T T - - - - - - -
C8 T B - T T - - - - - - -
Hành động hệ thống
A1 T F F T T F F F F F F F

-Kịch bản test case
| Id | Tiêu đề | Mô tả kịch bản | Dữ liệu thử | Kết quả mong đợi |
|---|---|---|---|---|
| TC01 | Thêm bạn đọc hợp lệ | Nhập đầy đủ thông tin và tải ảnh đại diện hợp lệ | Họ tên: "Nguyễn Văn A"; Email: "nguyenvana@gmail.com"; SĐT: "0912345678"; Mật khẩu: "123456"; Địa chỉ: "Hà Nội"; Giới tính: "Nam"; Ngày sinh: 2000-01-01; Ảnh đại diện: file ảnh hợp lệ | Hệ thống thêm bạn đọc thành công, sinh mã bạn đọc tự động và hiển thị thông báo thành công |
| TC02 | Thiếu họ và tên | Không nhập họ tên khi tạo mới bạn đọc | Họ tên: trống; các trường khác hợp lệ | Hệ thống báo lỗi bắt buộc nhập họ tên, không lưu dữ liệu |
| TC03 | Email không hợp lệ | Nhập email sai định dạng | Email: "abc@"; các trường khác hợp lệ | Hệ thống báo lỗi email không hợp lệ, không lưu dữ liệu |
| TC04 | Email bị trùng | Tạo bạn đọc với email đã tồn tại | Email: trùng với bạn đọc khác; các trường khác hợp lệ | Hệ thống báo lỗi email không được để trùng, không lưu dữ liệu |
| TC05 | Số điện thoại không hợp lệ | Nhập số điện thoại sai định dạng | SĐT: "12345abc"; các trường khác hợp lệ | Hệ thống báo lỗi số điện thoại không hợp lệ, không lưu dữ liệu |
| TC06 | Thiếu ảnh đại diện | Không tải ảnh đại diện khi thêm mới | Ảnh đại diện: trống; các trường khác hợp lệ | Hệ thống báo bắt buộc tải ảnh đại diện, không lưu dữ liệu |
| TC07 | Thiếu mật khẩu | Để trống mật khẩu khi tạo mới | Mật khẩu: trống; các trường khác hợp lệ | Hệ thống vẫn cho phép tạo tài khoản bạn đọc và không gán mật khẩu đăng nhập |
| TC08 | Thiếu ngày sinh | Không chọn ngày sinh | Ngày sinh: trống; các trường khác hợp lệ | Hệ thống báo lỗi nếu ngày sinh bắt buộc, hoặc cho phép lưu nếu không bắt buộc theo nghiệp vụ |
| TC09 | Thiếu địa chỉ | Không nhập địa chỉ | Địa chỉ: trống; các trường khác hợp lệ | Hệ thống vẫn cho phép lưu nếu địa chỉ không bắt buộc |
| TC10 | Thiếu giới tính | Không chọn giới tính | Giới tính: trống; các trường khác hợp lệ | Hệ thống báo lỗi nếu giới tính bắt buộc, hoặc cho phép lưu nếu không bắt buộc theo nghiệp vụ |
| TC11 | Mật khẩu không đủ độ dài | Nhập mật khẩu quá ngắn | Mật khẩu: "123"; các trường khác hợp lệ | Hệ thống báo lỗi mật khẩu không đạt yêu cầu, không lưu dữ liệu |
| TC12 | Ngày sinh không hợp lệ | Chọn ngày sinh ở tương lai | Ngày sinh: 2030-01-01 | Hệ thống báo lỗi ngày sinh không hợp lệ, không lưu dữ liệu |
| TC13 | Số điện thoại đã tồn tại | Thêm bạn đọc với số điện thoại trùng | SĐT trùng với bạn đọc khác; các trường khác hợp lệ | Hệ thống báo lỗi số điện thoại không được để trùng, không lưu dữ liệu |
| TC14 | Ảnh đại diện không hợp lệ | Tải file sai định dạng | Ảnh đại diện: file .exe hoặc .txt | Hệ thống báo lỗi định dạng ảnh, không lưu dữ liệu |
| TC15 | Dữ liệu quá dài | Nhập họ tên hoặc địa chỉ vượt giới hạn cho phép | Họ tên > 255 ký tự; Địa chỉ > 255 ký tự | Hệ thống cảnh báo vượt giới hạn dữ liệu, không lưu hoặc tự động chặn nhập |
| TC16 | Hủy thao tác thêm bạn đọc | Nhập dữ liệu nhưng không bấm lưu | Chưa bấm nút Lưu | Dữ liệu không được tạo mới trong hệ thống |

-Ghi chú kiểm thử

- Ưu tiên kiểm thử các trường bắt buộc và ràng buộc nghiệp vụ: họ tên, email, số điện thoại, ảnh đại diện.
- Kiểm thử riêng chức năng thêm mới bạn đọc theo các tình huống hợp lệ và không hợp lệ.
- Nếu nghiệp vụ có quy định riêng về mật khẩu, ngày sinh hoặc giới tính, cần cập nhật lại expected result cho phù hợp.
