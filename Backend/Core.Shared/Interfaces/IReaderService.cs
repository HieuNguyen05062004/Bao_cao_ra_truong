using Core.Shared.Entities;

namespace Core.Shared.Interfaces;

public interface IReaderService
{
    Task<IEnumerable<Reader>> GetAllAsync();
    Task<Reader?> GetByIdAsync(string readerId);

    /// <summary>Trả về null nếu thành công, chuỗi lỗi nếu thất bại.</summary>
    Task<string?> CreateAsync(Reader reader);
    Task<string?> UpdateAsync(Reader reader);
    Task<string?> DeleteAsync(string readerId);

    Task<IEnumerable<Reader>> SearchAsync(string keyword);

    Task<bool> ReaderIdExistsAsync(string readerId);
    Task<int> CountBorrowingAsync(string readerId);
    Task<int> CountOverdueAsync(string readerId);
}
