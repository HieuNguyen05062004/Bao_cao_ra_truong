namespace Core.Shared.Models;

public class BorrowRequest
{
    public string ReaderId { get; set; } = string.Empty;
    public string? StaffUsername { get; set; }
    public List<string> BookIds { get; set; } = new();
    public DateTime? DueDate { get; set; }
}
