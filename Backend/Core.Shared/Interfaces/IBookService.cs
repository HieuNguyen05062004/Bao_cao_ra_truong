using Core.Shared.Entities;

namespace Core.Shared.Interfaces;

public interface IBookService
{
    Task<List<Book>> GetAllBooksAsync();
    Task<Book?> GetBookByIdAsync(string bookId);
    Task<List<Book>> SearchBooksAsync(string searchTerm);
    Task<List<Book>> GetBooksByCategoryAsync(int categoryId);
    Task<List<Book>> GetAvailableBooksAsync();
    Task<List<Book>> GetFeaturedBooksAsync(int count = 5);
    Task<List<Book>> GetTrendingBooksAsync(int count = 5);
    Task<List<Category>> GetAllCategoriesAsync();

    /// <summary>
    /// Thêm sách mới kèm danh sách danh mục (many-to-many).
    /// categoryIds: danh sách CategoryID người dùng đã chọn qua chips.
    /// </summary>
    Task<(bool Success, string Message)> AddBookAsync(Book book, List<int> categoryIds);

    /// <summary>
    /// Cập nhật sách và đồng bộ lại danh sách danh mục.
    /// </summary>
    Task<(bool Success, string Message)> UpdateBookAsync(Book book, List<int> categoryIds);

    Task<(bool Success, string Message)> DeleteBookAsync(string bookId);
    Task<bool> BookIdExistsAsync(string bookId);
    Task<(bool Success, string Message)> UpdateBookQuantityAsync(string bookId, int quantityChange);
}
