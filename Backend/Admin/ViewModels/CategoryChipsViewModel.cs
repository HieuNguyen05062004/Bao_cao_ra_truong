using Core.Shared.Entities;

namespace Admin.ViewModels;

/// <summary>
/// ViewModel truyền vào Partial View _CategoryChips.cshtml.
/// Hỗ trợ chọn nhiều danh mục (multi-select).
/// </summary>
public class CategoryChipsViewModel
{
    /// <summary>Danh sách tất cả danh mục để render chips.</summary>
    public List<Category> Categories { get; set; } = new();

    /// <summary>
    /// Danh sách ID đang được chọn.
    /// Rỗng khi Create mới, có giá trị khi Edit.
    /// </summary>
    public List<int> SelectedIds { get; set; } = new();

    /// <summary>
    /// Tên của hidden input gửi lên Controller.
    /// Phải khớp với tên property trong ViewModel (mặc định: "CategoryIds").
    /// </summary>
    public string InputName { get; set; } = "CategoryIds";
}
