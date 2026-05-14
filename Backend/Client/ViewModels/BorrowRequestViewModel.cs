namespace Client.ViewModels;

/// <summary>
/// View Model cho mượn sách - Bước 1: Chọn sách và ngày mượn/trả
/// </summary>
public class BorrowRequestViewModel
{
    /// <summary>Mã bạn đọc (tự động điền)</summary>
    public string ReaderId { get; set; } = string.Empty;

    /// <summary>Tên bạn đọc (tự động điền)</summary>
    public string ReaderName { get; set; } = string.Empty;

    /// <summary>Danh sách mã sách được chọn</summary>
    public List<string> SelectedBookIds { get; set; } = new();

    /// <summary>Danh sách tên sách được chọn</summary>
    public List<string> SelectedBookTitles { get; set; } = new();

    /// <summary>Ngày mượn</summary>
    public DateTime BorrowDate { get; set; } = DateTime.Today;

    /// <summary>Ngày trả dự kiến</summary>
    public DateTime DueDate { get; set; } = DateTime.Today.AddDays(7);
}

/// <summary>
/// View Model cho Admin/Nhân viên duyệt yêu cầu mượn
/// </summary>
public class BorrowTicketApprovalViewModel
{
    public int TicketId { get; set; }

    /// <summary>Mã bạn đọc</summary>
    public string ReaderId { get; set; } = string.Empty;

    /// <summary>Tên bạn đọc</summary>
    public string ReaderName { get; set; } = string.Empty;

    /// <summary>Danh sách sách mượn</summary>
    public List<BorrowBookItemViewModel> Books { get; set; } = new();

    /// <summary>Ngày mượn</summary>
    public DateTime BorrowDate { get; set; }

    /// <summary>Ngày trả dự kiến</summary>
    public DateTime DueDate { get; set; }

    /// <summary>Trạng thái (Pending, Approved, Rejected, Returned)</summary>
    public string Status { get; set; } = "Pending";
}

/// <summary>
/// Item sách trong danh sách mượn
/// </summary>
public class BorrowBookItemViewModel
{
    public string BookId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;
}
