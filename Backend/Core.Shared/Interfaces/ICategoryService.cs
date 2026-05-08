using Core.Shared.Entities;

namespace Core.Shared.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAllAsync(string? keyword = null);
    Task<Category?> GetByIdAsync(int categoryId);
    Task<(bool Success, string Message, Category? Data)> CreateAsync(Category category);
    Task<(bool Success, string Message, Category? Data)> UpdateAsync(int categoryId, Category category);
    Task<(bool Success, string Message)> DeleteAsync(int categoryId);
}
