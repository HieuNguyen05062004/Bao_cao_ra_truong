using System.Text.RegularExpressions;
using Core.Shared.Entities;
using Core.Shared.Interfaces;
using Core.Shared.Repositories;
using Core.Shared.Utilities;

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

    public async Task<bool> EmailExistsAsync(string email, string? exceptReaderId = null)
        => await _repo.EmailExistsAsync(email, exceptReaderId);

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
        if (string.IsNullOrWhiteSpace(reader.FullName))
            return "Vui lòng điền họ tên.";

        // Luôn tự sinh ID — không cho nhập tay
        reader.ReaderId = await GenerateUniqueIdAsync(reader.FullName);

        if (!string.IsNullOrWhiteSpace(reader.Email) && !IsValidEmail(reader.Email))
            return "Email không hợp lệ.";

        if (!string.IsNullOrWhiteSpace(reader.Email) && await _repo.EmailExistsAsync(reader.Email))
            return "Gmail không được để trùng.";

        if (!string.IsNullOrWhiteSpace(reader.Phone) && !IsValidPhone(reader.Phone))
            return "Số điện thoại không hợp lệ.";

        reader.CreatedAt ??= DateTime.Now;

        await _repo.AddAsync(reader);
        return null;
    }

    // ------------------------------------------------------------------ //
    //  UPDATE — Admin cập nhật bạn đọc
    // ------------------------------------------------------------------ //
    public async Task<string?> UpdateAsync(Reader reader)
    {
        var existing = await _repo.GetByIdAsync(reader.ReaderId);
        if (existing is null) return "Bạn đọc không tồn tại.";

        if (string.IsNullOrWhiteSpace(reader.FullName))
            return "Vui lòng nhập họ tên.";

        if (!string.IsNullOrWhiteSpace(reader.Email) && !IsValidEmail(reader.Email))
            return "Email không hợp lệ.";

        if (!string.IsNullOrWhiteSpace(reader.Email) && await _repo.EmailExistsAsync(reader.Email, reader.ReaderId))
            return "Gmail không được để trùng.";

        if (!string.IsNullOrWhiteSpace(reader.Phone) && !IsValidPhone(reader.Phone))
            return "Số điện thoại không hợp lệ.";

        existing.FullName = reader.FullName;
        existing.DoB = reader.DoB;
        existing.Gender = reader.Gender;
        existing.Address = reader.Address;
        existing.Phone = reader.Phone;
        existing.Email = reader.Email;
        existing.AvatarUrl = reader.AvatarUrl;

        // Chỉ cập nhật PasswordHash nếu có giá trị mới (đã hash từ controller)
        if (!string.IsNullOrWhiteSpace(reader.PasswordHash))
            existing.PasswordHash = reader.PasswordHash;

        await _repo.UpdateAsync(existing);
        return null;
    }

    // ------------------------------------------------------------------ //
    //  UPDATE PROFILE — Client tự sửa thông tin cá nhân
    // ------------------------------------------------------------------ //
    public async Task<string?> UpdateProfileAsync(Reader reader)
    {
        var existing = await _repo.GetByIdAsync(reader.ReaderId);
        if (existing is null) return "Tài khoản không tồn tại.";

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

        // Chỉ cập nhật avatar nếu có ảnh mới
        if (reader.AvatarUrl != null)
            existing.AvatarUrl = reader.AvatarUrl;

        // Chỉ cập nhật mật khẩu nếu có giá trị mới (đã hash từ controller)
        if (!string.IsNullOrWhiteSpace(reader.PasswordHash))
            existing.PasswordHash = reader.PasswordHash;

        await _repo.UpdateAsync(existing);
        return null;
    }

    // ------------------------------------------------------------------ //
    //  DELETE — Admin xóa bạn đọc
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
    //  DELETE SELF — Client tự xóa tài khoản
    // ------------------------------------------------------------------ //
    public async Task<string?> DeleteSelfAsync(string readerId)
    {
        var reader = await _repo.GetByIdAsync(readerId);
        if (reader is null) return "Tài khoản không tồn tại.";

        if (await _repo.HasActiveBorrowAsync(readerId))
            return "Bạn đang có sách chưa trả. Vui lòng trả sách trước khi xóa tài khoản.";

        await _repo.DeleteAsync(reader);
        return null;
    }

    // ------------------------------------------------------------------ //
    //  HELPERS
    // ------------------------------------------------------------------ //
    private async Task<string> GenerateUniqueIdAsync(string fullName)
    {
        for (int i = 0; i < 10; i++)
        {
            var id = IdGenerator.GenerateReaderId(fullName);
            if (!await _repo.ExistsAsync(id))
                return id;
        }
        // Fallback — dùng RR prefix nhất quán
        return $"RR{DateTime.Now:yyMMddHHmmss}";
    }

    private static bool IsValidEmail(string email) =>
        Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);

    private static bool IsValidPhone(string phone) =>
        Regex.IsMatch(phone, @"^[0-9]{9,15}$");
}
