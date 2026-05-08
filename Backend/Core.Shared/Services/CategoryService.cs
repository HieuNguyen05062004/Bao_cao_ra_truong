using Core.Shared.Constants;
using Core.Shared.Entities;
using Core.Shared.Interfaces;
using Core.Shared.Repositories;

namespace Core.Shared.Services;

public class CategoryService : ICategoryService
{
    private readonly CategoryRepository _categoryRepository;

    public CategoryService(CategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<Category>> GetAllAsync(string? keyword = null)
    {
        var categories = await _categoryRepository.GetAllAsync();

        if (string.IsNullOrWhiteSpace(keyword))
        {
            return categories;
        }

        return categories.Where(x => x.CategoryName.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<Category?> GetByIdAsync(int categoryId)
    {
        return await _categoryRepository.GetByIdAsync(categoryId);
    }

    public async Task<(bool Success, string Message, Category? Data)> CreateAsync(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.CategoryName))
        {
            return (false, MessageConstants.InvalidData, null);
        }

        category.CategoryName = category.CategoryName.Trim();
        await _categoryRepository.AddAsync(category);
        await _categoryRepository.SaveChangesAsync();

        return (true, "Tạo danh mục thành công.", category);
    }

    public async Task<(bool Success, string Message, Category? Data)> UpdateAsync(int categoryId, Category category)
    {
        var existing = await _categoryRepository.GetByIdAsync(categoryId);
        if (existing is null)
        {
            return (false, MessageConstants.NotFound, null);
        }

        if (string.IsNullOrWhiteSpace(category.CategoryName))
        {
            return (false, MessageConstants.InvalidData, null);
        }

        existing.CategoryName = category.CategoryName.Trim();
        await _categoryRepository.UpdateAsync(existing);
        await _categoryRepository.SaveChangesAsync();

        return (true, "Cập nhật danh mục thành công.", existing);
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int categoryId)
    {
        var existing = await _categoryRepository.GetByIdAsync(categoryId);
        if (existing is null)
        {
            return (false, MessageConstants.NotFound);
        }

        if (await _categoryRepository.IsUsedAsync(categoryId))
        {
            return (false, MessageConstants.CategoryInUse);
        }

        await _categoryRepository.DeleteAsync(existing);
        await _categoryRepository.SaveChangesAsync();

        return (true, "Xóa danh mục thành công.");
    }
}
