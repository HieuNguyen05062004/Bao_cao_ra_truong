namespace Core.Shared.Models;

public class DashboardStats
{
    public int TotalBooks { get; set; }
    public int BorrowingBooks { get; set; }
    public int OverdueTickets { get; set; }
    public int TotalReaders { get; set; }
    public int BorrowCountToday { get; set; }
    public int BorrowCountThisMonth { get; set; }
    public int BorrowCountThisYear { get; set; }
}
