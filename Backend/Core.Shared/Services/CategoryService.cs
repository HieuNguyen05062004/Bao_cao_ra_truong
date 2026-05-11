using Core.Shared.Entities;
using Core.Shared.Interfaces;
using Core.Shared.Repositories;

namespace Core.Shared.Services;

/// <summary>
/// Service xử lý toàn bộ business logic (nghiệp vụ) của quản lý danh mục.
/// Implement (triển khai) ICategoryService.
/// Giao tiếp với tầng dưới thông qua CategoryRepository.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly CategoryRepository _repository;

    public CategoryService(CategoryRepository repository)
    {
        _repository = repository;
    }

    // ─── READ ────────────────────────────────────────────────────────────────

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        if (id <= 0) return null;
        return await _repository.GetByIdAsync(id);
    }

    public async Task<List<Category>> SearchCategoriesAsync(string keyword)
    {
        // Nếu keyword rỗng → trả về toàn bộ, giống BookService
        if (string.IsNullOrWhiteSpace(keyword))
            return await _repository.GetAllAsync();

        return await _repository.SearchAsync(keyword.Trim());
    }

    // ─── WRITE ───────────────────────────────────────────────────────────────

    public async Task<(bool Success, string Message)> AddCategoryAsync(Category category)
    {
        // Validate dữ liệu đầu vào
        if (category == null)
            return (false, "Thông tin danh mục không hợp lệ.");

        if (string.IsNullOrWhiteSpace(category.CategoryName))
            return (false, "Tên danh mục không được để trống.");

        if (category.CategoryName.Trim().Length > 100)
            return (false, "Tên danh mục không được vượt quá 100 ký tự.");

        // Chuẩn hóa (normalize) tên trước khi lưu
        category.CategoryName = category.CategoryName.Trim();

        // Kiểm tra trùng tên
        if (await _repository.NameExistsAsync(category.CategoryName))
            return (false, $"Danh mục \"{category.CategoryName}\" đã tồn tại trong hệ thống.");

        var result = await _repository.AddAsync(category);

        return result
            ? (true, "Thêm danh mục thành công.")
            : (false, "Thêm danh mục thất bại. Vui lòng thử lại.");
    }

    public async Task<(bool Success, string Message)> UpdateCategoryAsync(Category category)
    {
        if (category == null)
            return (false, "Thông tin danh mục không hợp lệ.");

        if (category.CategoryId <= 0)
            return (false, "ID danh mục không hợp lệ.");

        if (string.IsNullOrWhiteSpace(category.CategoryName))
            return (false, "Tên danh mục không được để trống.");

        if (category.CategoryName.Trim().Length > 100)
            return (false, "Tên danh mục không được vượt quá 100 ký tự.");

        category.CategoryName = category.CategoryName.Trim();

        // Kiểm tra danh mục cần sửa có tồn tại không
        var existing = await _repository.GetByIdAsync(category.CategoryId);
        if (existing == null)
            return (false, "Không tìm thấy danh mục cần cập nhật.");

        // Kiểm tra trùng tên với danh mục KHÁC (excludeId = ID hiện tại)
        if (await _repository.NameExistsAsync(category.CategoryName, category.CategoryId))
            return (false, $"Danh mục \"{category.CategoryName}\" đã tồn tại trong hệ thống.");

        var result = await _repository.UpdateAsync(category);

        return result
            ? (true, "Cập nhật danh mục thành công.")
            : (false, "Cập nhật danh mục thất bại. Vui lòng thử lại.");
    }

    public async Task<(bool Success, string Message)> DeleteCategoryAsync(int id)
    {
        if (id <= 0)
            return (false, "ID danh mục không hợp lệ.");

        // Kiểm tra danh mục có tồn tại không
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            return (false, "Không tìm thấy danh mục cần xóa.");

        // Kiểm tra ràng buộc: còn sách thuộc danh mục này không?
        if (await _repository.HasBooksAsync(id))
            return (false, $"Không thể xóa danh mục \"{existing.CategoryName}\" vì vẫn còn sách thuộc danh mục này.");

        var result = await _repository.DeleteAsync(id);

        return result
            ? (true, "Xóa danh mục thành công.")
            : (false, "Xóa danh mục thất bại. Vui lòng thử lại.");
    }

    public async Task<Category?> GetCategoryWithBooksAsync(int id)
    {
        if (id <= 0) return null;
        return await _repository.GetWithBooksAsync(id);
    }
}
