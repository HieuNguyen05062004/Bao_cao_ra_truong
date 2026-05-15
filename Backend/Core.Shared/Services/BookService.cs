using Core.Shared.Entities;
using Core.Shared.Interfaces;
using Core.Shared.Repositories;
using Core.Shared.Utilities;

namespace Core.Shared.Services;

public class BookService : IBookService
{
    private readonly BookRepository _repository;

    public BookService(BookRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Book>> GetAllBooksAsync()
        => await _repository.GetAllAsync();

    public async Task<Book?> GetBookByIdAsync(string bookId)
    {
        if (string.IsNullOrWhiteSpace(bookId)) return null;
        return await _repository.GetByIdAsync(bookId);
    }

    public async Task<List<Book>> SearchBooksAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm)) return await _repository.GetAllAsync();
        return await _repository.SearchAsync(searchTerm.Trim());
    }

    public async Task<List<Book>> GetBooksByCategoryAsync(int categoryId)
    {
        if (categoryId <= 0) return new List<Book>();
        return await _repository.GetByCategoryAsync(categoryId);
    }

    /// <summary>
    /// Lọc sách theo nhiều danh mục (OR logic).
    /// Nếu list rỗng → trả về tất cả sách.
    /// </summary>
    public async Task<List<Book>> GetBooksByMultipleCategoriesAsync(List<int> categoryIds)
    {
        if (categoryIds == null || !categoryIds.Any())
            return await _repository.GetAllAsync();

        return await _repository.GetByMultipleCategoriesAsync(categoryIds);
    }

    public async Task<List<Book>> GetAvailableBooksAsync()
        => await _repository.GetAvailableAsync();

    public async Task<List<Book>> GetFeaturedBooksAsync(int count = 5)
        => await _repository.GetFeaturedBooksAsync(count);

    public async Task<List<Book>> GetTrendingBooksAsync(int count = 5)
        => await _repository.GetTrendingBooksAsync(count);

    public async Task<List<Category>> GetAllCategoriesAsync()
        => await _repository.GetAllCategoriesAsync();

    public async Task<bool> BookIdExistsAsync(string bookId)
    {
        if (string.IsNullOrWhiteSpace(bookId)) return false;
        return await _repository.ExistsAsync(bookId);
    }

    // ─── ADD ─────────────────────────────────────────────────────────────────

    public async Task<(bool Success, string Message)> AddBookAsync(Book book, List<int> categoryIds)
    {
        if (book == null)
            return (false, "Thông tin sách không hợp lệ.");

        if (string.IsNullOrWhiteSpace(book.Title))
            return (false, "Tên sách không được để trống.");

        if (book.Title.Length > 255)
            return (false, "Tên sách không được quá 255 ký tự.");

        if (string.IsNullOrWhiteSpace(book.BookId))
        {
            string newId;
            do { newId = IdGenerator.GenerateBookId(); }
            while (await _repository.ExistsAsync(newId));
            book.BookId = newId;
        }
        else
        {
            if (await _repository.ExistsAsync(book.BookId))
                return (false, $"Mã sách '{book.BookId}' đã tồn tại trong hệ thống.");
        }

        if (book.Quantity == null) book.Quantity = 0;
        if (book.Quantity < 0)
            return (false, "Số lượng sách không được âm.");

        if (string.IsNullOrWhiteSpace(book.Status))
            book.Status = "Có thể mượn";

        var result = await _repository.AddAsync(book, categoryIds ?? new List<int>());
        return result
            ? (true, "Thêm sách thành công.")
            : (false, "Thêm sách thất bại. Vui lòng thử lại.");
    }

    // ─── UPDATE ──────────────────────────────────────────────────────────────

    public async Task<(bool Success, string Message)> UpdateBookAsync(Book book, List<int> categoryIds)
    {
        if (book == null)
            return (false, "Thông tin sách không hợp lệ.");

        if (string.IsNullOrWhiteSpace(book.BookId))
            return (false, "Mã sách không được để trống.");

        if (string.IsNullOrWhiteSpace(book.Title))
            return (false, "Tên sách không được để trống.");

        var existingBook = await _repository.GetByIdAsync(book.BookId);
        if (existingBook == null)
            return (false, "Không tìm thấy sách cần cập nhật.");

        if (book.Quantity == null) book.Quantity = 0;
        if (book.Quantity < 0)
            return (false, "Số lượng sách không được âm.");

        var result = await _repository.UpdateAsync(book, categoryIds ?? new List<int>());
        return result
            ? (true, "Cập nhật sách thành công.")
            : (false, "Cập nhật sách thất bại. Vui lòng thử lại.");
    }

    // ─── DELETE ──────────────────────────────────────────────────────────────

    public async Task<(bool Success, string Message)> DeleteBookAsync(string bookId)
    {
        if (string.IsNullOrWhiteSpace(bookId))
            return (false, "Mã sách không hợp lệ.");

        var book = await _repository.GetByIdAsync(bookId);
        if (book == null)
            return (false, "Không tìm thấy sách cần xóa.");

        if (await _repository.IsBookBorrowedAsync(bookId))
            return (false, "Không thể xóa sách đang được mượn.");

        var result = await _repository.DeleteAsync(bookId);
        return result
            ? (true, "Xóa sách thành công.")
            : (false, "Xóa sách thất bại. Vui lòng thử lại.");
    }

    public async Task<(bool Success, string Message)> UpdateBookQuantityAsync(string bookId, int quantityChange)
    {
        if (string.IsNullOrWhiteSpace(bookId))
            return (false, "Mã sách không hợp lệ.");

        var book = await _repository.GetByIdAsync(bookId);
        if (book == null) return (false, "Không tìm thấy sách.");

        int newQty = (book.Quantity ?? 0) + quantityChange;
        if (newQty < 0) return (false, "Số lượng sách không được âm.");

        var result = await _repository.UpdateQuantityAsync(bookId, quantityChange);
        return result
            ? (true, "Cập nhật số lượng thành công.")
            : (false, "Cập nhật số lượng thất bại.");
    }
}
