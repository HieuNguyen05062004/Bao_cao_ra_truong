using Core.Shared.Data;
using Core.Shared.Entities;
using Core.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Core.Shared.Services;

public class SearchService : ISearchService
{
    private readonly LibraryDbContext _dbContext;

    public SearchService(LibraryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Book>> BasicSearchAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return Enumerable.Empty<Book>();
        }

        var normalized = keyword.Trim().ToLowerInvariant();

        return await _dbContext.Books
            .Include(x => x.Category)
            .Where(x =>
                x.Title.ToLower().Contains(normalized) ||
                (x.Author != null && x.Author.ToLower().Contains(normalized)) ||
                (x.Category != null && x.Category.CategoryName.ToLower().Contains(normalized)))
            .OrderBy(x => x.Title)
            .ToListAsync();
    }

    public async Task<IEnumerable<Book>> AdvancedSearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Enumerable.Empty<Book>();
        }

        var tokens = query
            .ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        IQueryable<Book> queryable = _dbContext.Books.Include(x => x.Category);

        foreach (var token in tokens)
        {
            queryable = queryable.Where(x =>
                x.Title.Contains(token) ||
                (x.Author != null && x.Author.Contains(token)) ||
                (x.Category != null && x.Category.CategoryName.Contains(token)) ||
                (x.Publisher != null && x.Publisher.Contains(token)));
        }

        var books = await queryable.ToListAsync();

        return books
            .Select(x => new
            {
                Book = x,
                Score = CalculateScore(x, tokens)
            })
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Book.Title)
            .Select(x => x.Book)
            .ToList();
    }

    private static int CalculateScore(Book book, string[] tokens)
    {
        var score = 0;

        foreach (var token in tokens)
        {
            if (book.Title.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                score += 5;
            }

            if (!string.IsNullOrWhiteSpace(book.Author) && book.Author.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                score += 3;
            }

            if (book.Category?.CategoryName.Contains(token, StringComparison.OrdinalIgnoreCase) == true)
            {
                score += 4;
            }

            if (!string.IsNullOrWhiteSpace(book.Publisher) && book.Publisher.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                score += 1;
            }
        }

        return score;
    }
}
