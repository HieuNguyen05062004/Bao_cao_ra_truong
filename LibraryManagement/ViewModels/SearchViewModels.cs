using LibraryManagement.Models;

namespace LibraryManagement.ViewModels
{
    public class SearchViewModel
    {
        public string? Query { get; set; }
        public string? SearchType { get; set; } = "basic"; // "basic", "ai", or "initial"
        public string? FilterCategory { get; set; }
        public string? FilterAuthor { get; set; }
        public List<Book> Results { get; set; } = new();
        public List<string> AiSuggestions { get; set; } = new();
        public string? AiInterpretation { get; set; }
        public int TotalResults { get; set; }
    }

    public class BorrowRequestViewModel
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string BookAuthor { get; set; } = string.Empty;
        public DateTime BorrowDate { get; set; } = DateTime.Today;
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(14);
        public string? Notes { get; set; }
    }

    /// <summary>
    /// ViewModel for Admin/Staff manually creating a borrow record.
    /// </summary>
    public class AdminBorrowCreateViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public int BookId { get; set; }
        public DateTime BorrowDate { get; set; } = DateTime.Today;
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(14);
        public string? Notes { get; set; }
    }
}
