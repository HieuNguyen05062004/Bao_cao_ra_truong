using Core.Shared.Constants;
using Core.Shared.Entities;
using Core.Shared.Interfaces;
using Core.Shared.Repositories;

namespace Core.Shared.Services;

public class BookService : IBookService
{
    private readonly BookRepository _bookRepository;

    public BookService(BookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<IEnumerable<Book>> GetAllAsync(string? keyword = null, int? categoryId = null)
    {
        var books = await _bookRepository.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            books = books.Where(x =>
                x.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(x.Author) && x.Author.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        if (categoryId.HasValue)
        {
            books = books.Where(x => x.CategoryId == categoryId.Value).ToList();
        }

        return books;
    }

    public async Task<Book?> GetByIdAsync(string bookId)
    {
        return await _bookRepository.GetByIdAsync(bookId);
    }

    public async Task<(bool Success, string Message, Book? Data)> CreateAsync(Book book)
    {
        if (string.IsNullOrWhiteSpace(book.BookId) || string.IsNullOrWhiteSpace(book.Title))
        {
            return (false, MessageConstants.InvalidData, null);
        }

        if (await _bookRepository.ExistsAsync(book.BookId))
        {
            return (false, MessageConstants.DuplicateBookId, null);
        }

        NormalizeBook(book);
        await _bookRepository.AddAsync(book);
        await _bookRepository.SaveChangesAsync();

        return (true, "Tạo sách thành công.", book);
    }

    public async Task<(bool Success, string Message, Book? Data)> UpdateAsync(string bookId, Book book)
    {
        var existing = await _bookRepository.GetByIdAsync(bookId);
        if (existing is null)
        {
            return (false, MessageConstants.NotFound, null);
        }

        existing.Title = book.Title;
        existing.Author = book.Author;
        existing.Publisher = book.Publisher;
        existing.PublishYear = book.PublishYear;
        existing.CategoryId = book.CategoryId;
        existing.ImageUrl = book.ImageUrl;

        if (book.Quantity.HasValue)
        {
            existing.Quantity = book.Quantity;
        }

        NormalizeBook(existing);

        await _bookRepository.UpdateAsync(existing);
        await _bookRepository.SaveChangesAsync();
        return (true, "Cập nhật sách thành công.", existing);
    }

    public async Task<(bool Success, string Message)> DeleteAsync(string bookId)
    {
        var existing = await _bookRepository.GetByIdAsync(bookId);
        if (existing is null)
        {
            return (false, MessageConstants.NotFound);
        }

        if (await _bookRepository.IsUsedInBorrowAsync(bookId))
        {
            return (false, MessageConstants.BookInUse);
        }

        await _bookRepository.DeleteAsync(existing);
        await _bookRepository.SaveChangesAsync();

        return (true, "Xóa sách thành công.");
    }

    private static void NormalizeBook(Book book)
    {
        book.Title = book.Title.Trim();
        book.Author = book.Author?.Trim();
        book.Publisher = book.Publisher?.Trim();
        book.ImageUrl = book.ImageUrl?.Trim();
        book.Quantity ??= 0;
        book.Status = book.Quantity > 0 ? BorrowStatusConstants.Available : BorrowStatusConstants.BorrowedOut;
    }
}
