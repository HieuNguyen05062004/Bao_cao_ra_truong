using System;
using System.Collections.Generic;
using System.Text;
namespace Core.Shared.Constants;

public static class MessageConstants
{
    // Thành công
    public const string CreateSuccess = "Thêm tài khoản thành công.";
    public const string UpdateSuccess = "Cập nhật tài khoản thành công.";
    public const string DeleteSuccess = "Xóa tài khoản thành công.";

    // Lỗi dữ liệu
    public const string UsernameExists = "Tên đăng nhập đã tồn tại.";
    public const string EmailInvalid = "Email không hợp lệ.";
    public const string PasswordWeak = "Mật khẩu phải có ít nhất 8 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt.";
    public const string RoleInvalid = "Quyền hạn không hợp lệ.";
    public const string DataEmpty = "Vui lòng điền đầy đủ thông tin.";
    public const string AccountNotFound = "Tài khoản không tồn tại.";

    // Lỗi xóa
    public const string DeleteHasBorrow = "Không thể xóa: nhân viên này đang có phiếu mượn liên kết.";
    public const string DeleteAdminRoot = "Không thể xóa tài khoản Admin gốc.";

    // Lỗi đăng nhập
    public const string LoginFailed = "Tên đăng nhập hoặc mật khẩu không đúng.";
    public const string LoginNoPermission = "Tài khoản không có quyền truy cập hệ thống này.";
}

