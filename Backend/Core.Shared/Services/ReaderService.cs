using System.Text.RegularExpressions;
using Core.Shared.Entities;
using Core.Shared.Interfaces;
using Core.Shared.Repositories;

namespace Core.Shared.Services;

public class ReaderService : IReaderService
{
    private readonly ReaderRepository _repo;

    public ReaderService(ReaderRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<Reader>> GetAllAsync()
        => await _repo.GetAllAsync();

    public async Task<Reader?> GetByIdAsync(string readerId)
        => await _repo.GetByIdAsync(readerId);

    public async Task<bool> ReaderIdExistsAsync(string readerId)
        => await _repo.ExistsAsync(readerId);

    public async Task<IEnumerable<Reader>> SearchAsync(string keyword)
        => await _repo.SearchAsync(keyword);

    public async Task<int> CountBorrowingAsync(string readerId)
        => await _repo.CountBorrowingAsync(readerId);

    public async Task<int> CountOverdueAsync(string readerId)
        => await _repo.CountOverdueAsync(readerId);

    // ------------------------------------------------------------------ //
    //  CREATE
    // ------------------------------------------------------------------ //
    public async Task<string?> CreateAsync(Reader reader)
    {
        if (string.IsNullOrWhiteSpace(reader.ReaderId) ||
            string.IsNullOrWhiteSpace(reader.FullName))
            return "Vui lòng điền đầy đủ thông tin bắt buộc.";

        if (await _repo.ExistsAsync(reader.ReaderId))
            return "Mã bạn đọc đã tồn tại.";

        if (!string.IsNullOrWhiteSpace(reader.Email) && !IsValidEmail(reader.Email))
            return "Email không hợp lệ.";

        if (!string.IsNullOrWhiteSpace(reader.Phone) && !IsValidPhone(reader.Phone))
            return "Số điện thoại không hợp lệ.";

        await _repo.AddAsync(reader);
        return null;
    }

    // ------------------------------------------------------------------ //
    //  UPDATE
    // ------------------------------------------------------------------ //
    public async Task<string?> UpdateAsync(Reader reader)
    {
        var existing = await _repo.GetByIdAsync(reader.ReaderId);
        if (existing is null) return "Bạn đọc không tồn tại.";

        if (string.IsNullOrWhiteSpace(reader.FullName))
            return "Vui lòng nhập họ tên.";

        if (!string.IsNullOrWhiteSpace(reader.Email) && !IsValidEmail(reader.Email))
            return "Email không hợp lệ.";

        if (!string.IsNullOrWhiteSpace(reader.Phone) && !IsValidPhone(reader.Phone))
            return "Số điện thoại không hợp lệ.";

        existing.FullName = reader.FullName;
        existing.DoB = reader.DoB;
        existing.Gender = reader.Gender;
        existing.Address = reader.Address;
        existing.Phone = reader.Phone;
        existing.Email = reader.Email;
        existing.AvatarUrl = reader.AvatarUrl;

        await _repo.UpdateAsync(existing);
        return null;
    }

    // ------------------------------------------------------------------ //
    //  DELETE
    // ------------------------------------------------------------------ //
    public async Task<string?> DeleteAsync(string readerId)
    {
        var reader = await _repo.GetByIdAsync(readerId);
        if (reader is null) return "Bạn đọc không tồn tại.";

        if (await _repo.HasActiveBorrowAsync(readerId))
            return "Không thể xóa: bạn đọc đang có sách chưa trả.";

        await _repo.DeleteAsync(reader);
        return null;
    }

    // ------------------------------------------------------------------ //
    //  HELPERS
    // ------------------------------------------------------------------ //
    private static bool IsValidEmail(string email) =>
        Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);

    private static bool IsValidPhone(string phone) =>
        Regex.IsMatch(phone, @"^[0-9]{9,15}$");
}
