namespace Core.Shared.Interfaces;

/// <summary>
/// Service phân tích câu tìm kiếm tự nhiên bằng AI,
/// trả về các tham số có cấu trúc để truy vấn database.
/// </summary>
public interface IAiSearchService
{
    /// <summary>
    /// Phân tích câu hỏi tự nhiên (VD: "sách lập trình Python cho người mới")
    /// thành các tham số tìm kiếm có cấu trúc.
    /// </summary>
    Task<AiSearchResult> ParseSearchQueryAsync(string naturalLanguageQuery);
}

/// <summary>
/// Kết quả phân tích từ AI — chứa các trường để truy vấn database
/// và thông tin hiển thị cho người dùng.
/// </summary>
public class AiSearchResult
{
    /// <summary>Từ khóa chính để tìm trong Title + Author</summary>
    public string Keyword { get; set; } = string.Empty;

    /// <summary>AI diễn giải ý định tìm kiếm — hiển thị trên UI</summary>
    public string InterpretedQuery { get; set; } = string.Empty;

    /// <summary>True nếu AI parse thành công; False nếu có lỗi</summary>
    public bool IsSuccess { get; set; } = true;

    /// <summary>Thông báo lỗi nếu IsSuccess = false</summary>
    public string? ErrorMessage { get; set; }
}
