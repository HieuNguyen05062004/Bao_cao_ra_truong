using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Services
{
    public interface ISearchService
    {
        Task<SearchViewModel> BasicSearchAsync(string? title, string? author, string? category);
        Task<SearchViewModel> AiSearchAsync(string query);
    }

    public class SearchService : ISearchService
    {
        private readonly ApplicationDbContext _context;

        public SearchService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SearchViewModel> BasicSearchAsync(string? title, string? author, string? category)
        {
            var query = _context.Books.Include(b => b.Category).AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(b => b.Title.ToLower().Contains(title.ToLower()));
            if (!string.IsNullOrWhiteSpace(author))
                query = query.Where(b => b.Author.ToLower().Contains(author.ToLower()));
            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(b => b.Category != null && b.Category.Name.ToLower().Contains(category.ToLower()));

            var results = await query.OrderBy(b => b.Title).ToListAsync();

            return new SearchViewModel
            {
                Query = title ?? author ?? category,
                SearchType = "basic",
                FilterCategory = category,
                FilterAuthor = author,
                Results = results,
                TotalResults = results.Count
            };
        }

        public async Task<SearchViewModel> AiSearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new SearchViewModel { Query = query, SearchType = "ai" };

            var interpretation = InterpretQuery(query);
            var keywords = ExtractKeywords(query);
            var suggestions = GenerateSuggestions(query, keywords);

            var dbQuery = _context.Books.Include(b => b.Category).AsQueryable();

            // Build multi-field search with all extracted keywords
            foreach (var keyword in keywords)
            {
                var kw = keyword.ToLower();
                dbQuery = dbQuery.Where(b =>
                    b.Title.ToLower().Contains(kw) ||
                    b.Author.ToLower().Contains(kw) ||
                    (b.Category != null && b.Category.Name.ToLower().Contains(kw)) ||
                    (b.Description != null && b.Description.ToLower().Contains(kw)) ||
                    (b.Publisher != null && b.Publisher.ToLower().Contains(kw)));
            }

            var results = await dbQuery.OrderBy(b => b.Title).ToListAsync();

            // If no results with AND logic, try OR logic
            if (!results.Any() && keywords.Count > 1)
            {
                var orQuery = _context.Books.Include(b => b.Category).AsQueryable();
                var combinedResults = new List<Book>();
                foreach (var keyword in keywords)
                {
                    var kw = keyword.ToLower();
                    var partial = await orQuery.Where(b =>
                        b.Title.ToLower().Contains(kw) ||
                        b.Author.ToLower().Contains(kw) ||
                        (b.Category != null && b.Category.Name.ToLower().Contains(kw)) ||
                        (b.Description != null && b.Description.ToLower().Contains(kw)))
                        .ToListAsync();
                    combinedResults.AddRange(partial);
                }
                results = combinedResults.DistinctBy(b => b.Id).OrderBy(b => b.Title).ToList();
            }

            return new SearchViewModel
            {
                Query = query,
                SearchType = "ai",
                Results = results,
                TotalResults = results.Count,
                AiInterpretation = interpretation,
                AiSuggestions = suggestions
            };
        }

        private string InterpretQuery(string query)
        {
            var q = query.ToLower();
            var parts = new List<string>();

            if (q.Contains("java")) parts.Add("lập trình Java");
            else if (q.Contains("python")) parts.Add("lập trình Python");
            else if (q.Contains("c#") || q.Contains("csharp") || q.Contains(".net")) parts.Add("lập trình C# / .NET");
            else if (q.Contains("javascript") || q.Contains("js")) parts.Add("lập trình JavaScript");
            else if (q.Contains("lập trình") || q.Contains("code") || q.Contains("programming")) parts.Add("sách lập trình");
            else if (q.Contains("văn học") || q.Contains("tiểu thuyết") || q.Contains("truyện")) parts.Add("sách văn học");
            else if (q.Contains("toán") || q.Contains("vật lý") || q.Contains("hóa")) parts.Add("sách khoa học tự nhiên");
            else if (q.Contains("kinh tế") || q.Contains("kinh doanh") || q.Contains("quản trị")) parts.Add("sách kinh tế");
            else if (q.Contains("lịch sử") || q.Contains("địa lý")) parts.Add("sách lịch sử - địa lý");

            if (q.Contains("người mới") || q.Contains("cơ bản") || q.Contains("nhập môn") || q.Contains("beginner"))
                parts.Add("dành cho người mới bắt đầu");
            else if (q.Contains("nâng cao") || q.Contains("chuyên sâu") || q.Contains("advanced"))
                parts.Add("dành cho người nâng cao");

            if (!parts.Any()) parts.Add($"tìm kiếm cho: '{query}'");

            return $"AI đang tìm kiếm {string.Join(", ", parts)}";
        }

        private List<string> ExtractKeywords(string query)
        {
            // Remove common Vietnamese stopwords and extract meaningful keywords
            var stopwords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "sách", "cho", "và", "về", "với", "của", "trong", "là", "có", "được",
                "người", "mới", "học", "cuốn", "quyển", "tôi", "muốn", "tìm", "kiếm",
                "the", "a", "an", "for", "and", "or", "in", "of", "to", "on"
            };

            var keywords = query
                .Split(new[] { ' ', ',', '.', '!', '?', ';', ':', '-' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length >= 2 && !stopwords.Contains(w))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // Add full query as a keyword too for exact phrase matching
            if (keywords.Count > 1)
                keywords.Insert(0, query.Trim());

            return keywords.Take(5).ToList();
        }

        private List<string> GenerateSuggestions(string query, List<string> keywords)
        {
            var suggestions = new List<string>();
            var q = query.ToLower();

            if (q.Contains("java"))
            {
                suggestions.Add("Lập trình Java cơ bản");
                suggestions.Add("Java nâng cao và Design Patterns");
                suggestions.Add("Spring Boot cho người mới");
            }
            else if (q.Contains("python"))
            {
                suggestions.Add("Python cho người mới học");
                suggestions.Add("Machine Learning với Python");
                suggestions.Add("Data Science với Python");
            }
            else if (q.Contains("c#") || q.Contains(".net"))
            {
                suggestions.Add("C# và .NET Framework");
                suggestions.Add("ASP.NET Core MVC");
            }
            else if (keywords.Any())
            {
                suggestions.Add($"Tìm kiếm theo tên sách: {keywords.First()}");
                suggestions.Add($"Tìm kiếm theo tác giả: {keywords.First()}");
            }

            return suggestions.Take(3).ToList();
        }
    }
}
