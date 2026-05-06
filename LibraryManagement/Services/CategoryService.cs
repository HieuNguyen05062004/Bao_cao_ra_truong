using LibraryManagement.Data;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Services
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task<Category> CreateAsync(Category category);
        Task<Category> UpdateAsync(Category category);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteMultipleAsync(IEnumerable<int> ids);
    }

    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await _context.Categories
                .Include(c => c.Books)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.Books)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category> CreateAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<Category> UpdateAsync(Category category)
        {
            var existing = await _context.Categories.FindAsync(category.Id);
            if (existing == null) throw new InvalidOperationException("Không tìm thấy danh mục");
            existing.Name = category.Name;
            existing.Description = category.Description;
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Books)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) return false;
            if (category.Books.Any())
                throw new InvalidOperationException("Không thể xóa danh mục đang có sách");
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMultipleAsync(IEnumerable<int> ids)
        {
            var categories = await _context.Categories
                .Include(c => c.Books)
                .Where(c => ids.Contains(c.Id))
                .ToListAsync();

            foreach (var cat in categories)
            {
                if (cat.Books.Any())
                    throw new InvalidOperationException($"Không thể xóa danh mục '{cat.Name}' đang có sách");
            }

            _context.Categories.RemoveRange(categories);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
