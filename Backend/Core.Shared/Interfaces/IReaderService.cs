using Core.Shared.Entities;

namespace Core.Shared.Interfaces;

public interface IReaderService
{
    Task<IEnumerable<Reader>> GetAllAsync();
    Task<Reader?> GetByIdAsync(string readerId);
    Task<bool> ReaderIdExistsAsync(string readerId);
    Task<bool> EmailExistsAsync(string email, string? exceptReaderId = null);
    Task<IEnumerable<Reader>> SearchAsync(string keyword);
    Task<int> CountBorrowingAsync(string readerId);
    Task<int> CountOverdueAsync(string readerId);

    /// <summary>Trả về null nếu thành công, chuỗi lỗi nếu thất bại.</summary>
    Task<string?> CreateAsync(Reader reader);

    /// <summary>Admin cập nhật bạn đọc.</summary>
    Task<string?> UpdateAsync(Reader reader);

    /// <summary>Admin xóa bạn đọc.</summary>
    Task<string?> DeleteAsync(string readerId);

    /// <summary>Client tự sửa thông tin cá nhân.</summary>
    Task<string?> UpdateProfileAsync(Reader reader);

    /// <summary>Client tự xóa tài khoản của mình.</summary>
    Task<string?> DeleteSelfAsync(string readerId);
}
