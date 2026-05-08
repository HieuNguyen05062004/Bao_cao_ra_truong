namespace Core.Shared.Constants;

public static class MessageConstants
{
    public const string NotFound = "Không tìm thấy dữ liệu.";
    public const string InvalidData = "Dữ liệu không hợp lệ.";
    public const string DuplicateBookId = "Mã sách đã tồn tại.";
    public const string DuplicateReaderId = "Mã bạn đọc đã tồn tại.";
    public const string DuplicateUsername = "Tên đăng nhập đã tồn tại.";
    public const string BookInUse = "Sách đang nằm trong phiếu mượn, không thể xóa.";
    public const string ReaderHasActiveBorrow = "Bạn đọc đang mượn sách, không thể xóa.";
    public const string CategoryInUse = "Danh mục đang được sử dụng bởi sách, không thể xóa.";
    public const string AccountInUse = "Tài khoản đang tham gia xử lý phiếu mượn, không thể xóa.";
    public const string BookOutOfStock = "Sách đã hết số lượng có thể mượn.";
    public const string ReaderHasOverdue = "Bạn đọc đang có sách quá hạn.";
}
