using LibraryManagement.Models;

namespace LibraryManagement.ViewModels
{
    public class StatisticsViewModel
    {
        public int TotalBooks { get; set; }
        public int TotalCategories { get; set; }
        public int TotalReaders { get; set; }
        public int TotalStaff { get; set; }
        public int TotalBorrowing { get; set; }
        public int TotalReturned { get; set; }
        public int TotalOverdue { get; set; }

        public List<MonthlyStats> MonthlyBorrowStats { get; set; } = new();
        public List<MonthlyStats> MonthlyReturnStats { get; set; } = new();
        public List<CategoryStats> CategoryStats { get; set; } = new();
        public List<TopBook> TopBorrowedBooks { get; set; } = new();
    }

    public class MonthlyStats
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Count { get; set; }
        public string Label => $"{Month:D2}/{Year}";
    }

    public class CategoryStats
    {
        public string CategoryName { get; set; } = string.Empty;
        public int BookCount { get; set; }
        public int BorrowCount { get; set; }
    }

    public class TopBook
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int BorrowCount { get; set; }
    }

    public class PersonalStatisticsViewModel
    {
        public int TotalBorrowed { get; set; }
        public int CurrentlyBorrowing { get; set; }
        public int TotalReturned { get; set; }
        public int Overdue { get; set; }
        public List<BorrowRecord> CurrentBorrows { get; set; } = new();
        public List<BorrowRecord> BorrowHistory { get; set; } = new();
    }
}
