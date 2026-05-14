namespace Core.Shared.Utilities;

/// <summary>
/// Helper class để format ID cho display (UI friendly)
/// Ví dụ: Category ID 1 → "CY0001", BorrowTicket ID 42 → "BT0042"
/// </summary>
public static class IdFormatter
{
    /// <summary>
    /// Format Category ID từ int sang string "CY" + PadLeft(6, '0')
    /// </summary>
    public static string FormatCategoryId(int categoryId)
    {
        return $"CY{categoryId:D6}";
    }

    /// <summary>
    /// Format BorrowTicket ID từ int sang string "BT" + PadLeft(6, '0')
    /// </summary>
    public static string FormatBorrowTicketId(int ticketId)
    {
        return $"BT{ticketId:D6}";
    }

    /// <summary>
    /// Parse Category ID từ string "CY0001" → 1
    /// </summary>
    public static int? ParseCategoryId(string formattedId)
    {
        if (string.IsNullOrWhiteSpace(formattedId) || !formattedId.StartsWith("CY"))
            return null;

        return int.TryParse(formattedId.Substring(2), out var id) ? id : null;
    }

    /// <summary>
    /// Parse BorrowTicket ID từ string "BT0042" → 42
    /// </summary>
    public static int? ParseBorrowTicketId(string formattedId)
    {
        if (string.IsNullOrWhiteSpace(formattedId) || !formattedId.StartsWith("BT"))
            return null;

        return int.TryParse(formattedId.Substring(2), out var id) ? id : null;
    }
}
