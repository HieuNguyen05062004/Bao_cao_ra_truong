namespace Client.ViewModels;

/// <summary>
/// ViewModel cho hồ sơ cá nhân bạn đọc
/// </summary>
public class ProfileViewModel
{
    public string ReaderId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Gender { get; set; }
    public DateOnly? DoB { get; set; }
    public string? Address { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime? CreatedAt { get; set; }

    // Thống kê
    public int TotalBorrow { get; set; }
    public int BorrowingCount { get; set; }
    public int OverdueCount { get; set; }
    public int ReturnedCount { get; set; }
    public int WishlistCount { get; set; }
}
