using Core.Shared.Data;
using Core.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.Shared.Repositories;

public class CategoryRepository
{
    private readonly LibraryDbContext _dbContext;

    public CategoryRepository(LibraryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        return await _dbContext.Categories.OrderBy(x => x.CategoryName).ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int categoryId)
    {
        return await _dbContext.Categories.FirstOrDefaultAsync(x => x.CategoryId == categoryId);
    }

    public async Task<bool> IsUsedAsync(int categoryId)
    {
        return await _dbContext.Books.AnyAsync(x => x.CategoryId == categoryId);
    }

    public async Task AddAsync(Category category)
    {
        await _dbContext.Categories.AddAsync(category);
    }

    public Task UpdateAsync(Category category)
    {
        _dbContext.Categories.Update(category);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Category category)
    {
        _dbContext.Categories.Remove(category);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}
