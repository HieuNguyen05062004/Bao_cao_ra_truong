using Core.Shared.Entities;

namespace Core.Shared.Interfaces;

public interface IReaderService
{
    Task<IEnumerable<Reader>> GetAllAsync(string? keyword = null);
    Task<Reader?> GetByIdAsync(string readerId);
    Task<(bool Success, string Message, Reader? Data)> CreateAsync(Reader reader);
    Task<(bool Success, string Message, Reader? Data)> UpdateAsync(string readerId, Reader reader);
    Task<(bool Success, string Message)> DeleteAsync(string readerId);
}
