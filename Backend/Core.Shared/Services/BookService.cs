using Core.Shared.Entities;
using Core.Shared.Interfaces;
using Core.Shared.Repositories;
using Core.Shared.Utilities;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

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

        var books = await _repository.GetAllAsync();
        return SearchInMemory(books, searchTerm.Trim());
    }

    private static List<Book> SearchInMemory(IEnumerable<Book> books, string searchTerm)
    {
        var normalizedSearch = NormalizeForSearch(searchTerm);
        // Tách các cụm từ theo dấu phẩy (nếu AI trả về nhiều chủ đề)
        var phrases = searchTerm.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => NormalizeForSearch(p.Trim()))
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToList();

        var tokens = ExtractSearchTokens(normalizedSearch);

        if (string.IsNullOrWhiteSpace(normalizedSearch))
            return books.OrderBy(b => b.Title).ToList();

        var scoredBooks = books
            .Select(book => new
            {
                Book = book,
                Score = CalculateSearchScore(book, normalizedSearch, phrases, tokens)
            })
            .Where(item => item.Score >= 15) // Ngưỡng điểm tối thiểu để giảm nhiễu
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Book.Title)
            .Select(item => item.Book)
            .ToList();

        if (scoredBooks.Count > 0)
            return scoredBooks;

        // Fallback: nếu AI/truy vấn trả về cụm từ ít token hoặc quá chung,
        // vẫn thử khớp theo cụm từ đầy đủ trên các trường chính thay vì trả về tất cả sách.
        return books
            .Where(book =>
            {
                var title = NormalizeForSearch(book.Title);
                var author = NormalizeForSearch(book.Author);
                var publisher = NormalizeForSearch(book.Publisher);
                var description = NormalizeForSearch(book.Description);
                var bookId = NormalizeForSearch(book.BookId);
                var categories = NormalizeForSearch(string.Join(" ", book.BookCategories
                    .Select(bc => bc.Category?.CategoryName)
                    .Where(name => !string.IsNullOrWhiteSpace(name))));

                return title.Contains(normalizedSearch)
                    || author.Contains(normalizedSearch)
                    || publisher.Contains(normalizedSearch)
                    || description.Contains(normalizedSearch)
                    || bookId.Contains(normalizedSearch)
                    || categories.Contains(normalizedSearch);
            })
            .OrderByDescending(b => NormalizeForSearch(b.Title).Contains(normalizedSearch))
            .ThenBy(b => b.Title)
            .ToList();
    }

    private static int CalculateSearchScore(Book book, string normalizedSearch, List<string> phrases, List<string> tokens)
    {
        var title = NormalizeForSearch(book.Title);
        var author = NormalizeForSearch(book.Author);
        var publisher = NormalizeForSearch(book.Publisher);
        var description = NormalizeForSearch(book.Description);
        var bookId = NormalizeForSearch(book.BookId);
        var categoryNames = book.BookCategories
            .Select(bc => bc.Category?.CategoryName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(NormalizeForSearch)
            .ToList();
        var allCategoriesStr = string.Join(" ", categoryNames);

        int score = 0;

        // 1. Khớp cụm từ (Phrases) - Rất quan trọng để giảm nhiễu
        foreach (var phrase in phrases)
        {
            bool matched = false;
            // Khớp chính xác danh mục là điểm cao nhất
            if (categoryNames.Any(c => c == phrase || c.Contains(phrase)))
            {
                score += 100;
                matched = true;
            }

            if (title.Contains(phrase)) { score += 80; matched = true; }
            if (author.Contains(phrase)) { score += 50; matched = true; }
            if (description.Contains(phrase)) { score += 30; matched = true; }
            if (publisher.Contains(phrase)) { score += 20; matched = true; }
            
            // Nếu khớp cả cụm từ dài (như "công nghệ thông tin")
            if (matched && phrase.Split(' ').Length > 1) score += 40;
        }

        // 2. Khớp token lẻ (Dùng whole-word matching)
        var matchedTokens = tokens.Count(token =>
               ContainsWholeWord(title, token)
            || ContainsWholeWord(author, token)
            || ContainsWholeWord(allCategoriesStr, token)
            || ContainsWholeWord(description, token));

        if (matchedTokens > 0)
        {
            score += matchedTokens * 5;
            if (matchedTokens == tokens.Count) score += 20;
        }

        // 3. Exact match cho toàn bộ chuỗi search
        if (!string.IsNullOrWhiteSpace(normalizedSearch))
        {
            if (title == normalizedSearch) score += 200;
            else if (title.Contains(normalizedSearch)) score += 50;
        }

        return score;
    }

    /// <summary>
    /// Kiểm tra token xuất hiện như một từ nguyên (không phải một phần của từ khác).
    /// Ví dụ: "an" không khớp với "ban", "can", chỉ khớp với từ "an" đứng độc lập.
    /// </summary>
    private static bool ContainsWholeWord(string text, string token)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(token))
            return false;

        return Regex.IsMatch(text, $@"(?<![a-z0-9]){Regex.Escape(token)}(?![a-z0-9])");
    }

    private static List<string> ExtractSearchTokens(string normalizedSearch)
    {
        // Stop words: các từ phổ biến không mang ý nghĩa tìm kiếm
        var stopWords = new HashSet<string>
        {
            "tim", "kiem", "sach", "cuon", "quyen", "cho", "toi", "minh", "can",
            "muon", "doc", "ve", "thuoc", "the", "loai", "tac", "gia", "cua",
            "nhung", "cac", "mot", "nguoi", "moi", "bat", "dau", "co", "khong",
            "hay", "gioi", "thieu", "ai",
            "an", "ma", "la", "va", "da", "de", "bi", "bo", "ca",
            "tu", "di", "gi", "no", "ta", "em", "ha"
        };

        return Regex.Matches(normalizedSearch, @"[a-z0-9]+")
            .Select(match => match.Value)
            .Where(token => token.Length >= 2 && !stopWords.Contains(token))
            .Distinct()
            .ToList();
    }

    private static string NormalizeForSearch(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var normalized = value.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category != UnicodeCategory.NonSpacingMark)
                builder.Append(character == 'đ' ? 'd' : character);
        }

        return Regex.Replace(builder.ToString().Normalize(NormalizationForm.FormC), @"\s+", " ").Trim();
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