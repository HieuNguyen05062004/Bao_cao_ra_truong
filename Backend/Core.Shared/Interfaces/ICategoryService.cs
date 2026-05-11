using Core.Shared.Entities;

namespace Core.Shared.Interfaces;

/// <summary>
/// Interface (hợp đồng) định nghĩa các nghiệp vụ quản lý danh mục sách.
/// Controller chỉ phụ thuộc vào Interface này, không phụ thuộc trực tiếp vào Service.
/// → Dễ thay thế, dễ viết Unit Test (kiểm thử đơn vị).
/// </summary>
public interface ICategoryService
{
    /// <summary>Lấy toàn bộ danh sách danh mục.</summary>
    Task<List<Category>> GetAllCategoriesAsync();

    /// <summary>Lấy một danh mục theo ID. Trả về null nếu không tồn tại.</summary>
    Task<Category?> GetCategoryByIdAsync(int id);

    /// <summary>
    /// Tìm kiếm danh mục theo từ khóa (keyword) trong tên.
    /// Nếu keyword rỗng, trả về toàn bộ danh sách.
    /// </summary>
    Task<List<Category>> SearchCategoriesAsync(string keyword);

    /// <summary>
    /// Thêm danh mục mới.
    /// Trả về (Success, Message) để Controller xử lý thông báo.
    /// </summary>
    Task<(bool Success, string Message)> AddCategoryAsync(Category category);

    /// <summary>
    /// Cập nhật danh mục đã có.
    /// Trả về (Success, Message) để Controller xử lý thông báo.
    /// </summary>
    Task<(bool Success, string Message)> UpdateCategoryAsync(Category category);

    /// <summary>
    /// Xóa danh mục theo ID.
    /// Trả về (Success, Message) để Controller xử lý thông báo.
    /// Nghiệp vụ: không cho xóa nếu còn sách thuộc danh mục này.
    /// </summary>
    Task<(bool Success, string Message)> DeleteCategoryAsync(int id);

    /// <summary>
    /// Lấy danh mục kèm theo toàn bộ danh sách sách (Books) thuộc danh mục đó.
    /// Dùng cho trang Details và trang Delete (hiển thị cảnh báo còn sách).
    /// </summary>
    Task<Category?> GetCategoryWithBooksAsync(int id);
}
