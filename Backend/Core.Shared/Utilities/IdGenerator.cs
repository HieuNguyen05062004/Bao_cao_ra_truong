namespace Core.Shared.Utilities;

public static class IdGenerator
{
    private static readonly Random _rng = new();

    /// <summary>
    /// Sinh ReaderId theo format: RR + 5 số ngẫu nhiên
    /// Ví dụ: RR01234
    /// (RR = chữ đầu và chữ cuối của "Reader")
    /// </summary>
    public static string GenerateReaderId(string fullName = "")
    {
        string numbers = _rng.Next(10000, 99999).ToString();
        return $"RR{numbers}";
    }

    /// <summary>
    /// Sinh BookId theo format: BK + 5 số ngẫu nhiên
    /// Ví dụ: BK01234
    /// </summary>
    public static string GenerateBookId()
    {
        string numbers = _rng.Next(10000, 99999).ToString();
        return $"BK{numbers}";
    }

    /// <summary>
    /// Sinh Id tổng quát theo prefix + 5 số ngẫu nhiên
    /// Ví dụ: GenerateId("CAT") → "CAT01234"
    /// </summary>
    public static string GenerateId(string prefix)
    {
        string numbers = _rng.Next(10000, 99999).ToString();
        return $"{prefix}{numbers}";
    }
}
