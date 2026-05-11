using Core.Shared.Data;
using Core.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.Shared.Repositories;

public class CategoryRepository
{
    private readonly LibraryDbContext _context;

    public CategoryRepository(LibraryDbContext context)
    {
        _context = context;
    }

    // ─── READ ────────────────────────────────────────────────────────────────

    public async Task<List<Category>> GetAllAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.CategoryName)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == id);
    }

    public async Task<List<Category>> SearchAsync(string keyword)
    {
        return await _context.Categories
            .Where(c => c.CategoryName.Contains(keyword))
            .OrderBy(c => c.CategoryName)
            .ToListAsync();
    }

    public async Task<bool> NameExistsAsync(string name, int excludeId = 0)
    {
        return await _context.Categories
            .AnyAsync(c => c.CategoryName == name && c.CategoryId != excludeId);
    }

    /// <summary>
    /// Kiểm tra danh mục còn sách hay không qua bảng BookCategories.
    /// </summary>
    public async Task<bool> HasBooksAsync(int categoryId)
    {
        return await _context.BookCategories
            .AnyAsync(bc => bc.CategoryId == categoryId);
    }

    /// <summary>
    /// Lấy danh mục kèm danh sách sách thuộc danh mục đó.
    /// Dùng cho trang Details và Delete.
    /// </summary>
    public async Task<Category?> GetWithBooksAsync(int id)
    {
        return await _context.Categories
            .Include(c => c.BookCategories)
                .ThenInclude(bc => bc.Book)
            .FirstOrDefaultAsync(c => c.CategoryId == id);
    }

    // ─── WRITE ───────────────────────────────────────────────────────────────

    public async Task<bool> AddAsync(Category category)
    {
        try
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return true;
        }
        catch { return false; }
    }

    public async Task<bool> UpdateAsync(Category category)
    {
        try
        {
            var existing = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == category.CategoryId);

            if (existing == null) return false;

            existing.CategoryName = category.CategoryName;
            await _context.SaveChangesAsync();
            return true;
        }
        catch { return false; }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null) return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }
        catch { return false; }
    }
}
