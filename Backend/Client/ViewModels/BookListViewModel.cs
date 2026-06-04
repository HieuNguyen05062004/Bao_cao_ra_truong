using Core.Shared.Entities;

namespace Client.ViewModels;

/// <summary>
/// ViewModel cho trang danh sách / tìm kiếm sách (Search/Index).
/// Gộp dữ liệu sách, danh mục, bộ lọc và phân trang vào một object duy nhất
/// thay vì dùng ViewBag rải rác.
/// </summary>
public class BookListViewModel
{
    // ── Dữ liệu hiển thị ─────────────────────────────────────────────────────

    /// <summary>Danh sách sách của trang hiện tại (đã phân trang).</summary>
    public List<Book> Books { get; set; } = new();

    /// <summary>Toàn bộ danh mục để render chip bộ lọc.</summary>
    public List<Category> Categories { get; set; } = new();

    // ── Bộ lọc / tìm kiếm ────────────────────────────────────────────────────

    /// <summary>Danh sách ID danh mục đang được chọn (đa chọn).</summary>
    public List<int> SelectedCategoryIds { get; set; } = new();

    /// <summary>Từ khoá tìm kiếm theo tên sách hoặc tác giả.</summary>
    public string Keyword { get; set; } = string.Empty;

    /// <summary>Thứ tự sắp xếp: "newest" hoặc "oldest".</summary>
    public string Sort { get; set; } = "newest";

    // ── Phân trang ────────────────────────────────────────────────────────────

    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalCount { get; set; } = 0;
    public int PageSize { get; set; } = 9;

    /// <summary>Có trang trước không.</summary>
    public bool HasPrev => CurrentPage > 1;

    /// <summary>Có trang sau không.</summary>
    public bool HasNext => CurrentPage < TotalPages;

    // ── Tích hợp AI tìm kiếm nâng cao (giữ tương thích với AiSearch action cũ)

    /// <summary>Câu AI đã diễn giải (hiển thị badge xanh trên view).</summary>
    public string? AiInterpretedQuery { get; set; }

    /// <summary>Câu gốc người dùng nhập vào ô tìm kiếm nâng cao.</summary>
    public string? OriginalAiQuery { get; set; }

    /// <summary>True khi kết quả đến từ tìm kiếm nâng cao, false khi tìm kiếm thường.</summary>
    public bool IsAiSearch { get; set; } = false;
}
