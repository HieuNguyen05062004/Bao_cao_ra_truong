using Core.Shared.Entities;

namespace Core.Shared.Interfaces;

public interface IBookService
{
    Task<IEnumerable<Book>> GetAllAsync(string? keyword = null, int? categoryId = null);
    Task<Book?> GetByIdAsync(string bookId);
    Task<(bool Success, string Message, Book? Data)> CreateAsync(Book book);
    Task<(bool Success, string Message, Book? Data)> UpdateAsync(string bookId, Book book);
    Task<(bool Success, string Message)> DeleteAsync(string bookId);
}
