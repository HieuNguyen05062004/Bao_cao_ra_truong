using Core.Shared.Constants;
using Core.Shared.Entities;
using Core.Shared.Interfaces;
using Core.Shared.Repositories;

namespace Core.Shared.Services;

public class ReaderService : IReaderService
{
    private readonly ReaderRepository _readerRepository;

    public ReaderService(ReaderRepository readerRepository)
    {
        _readerRepository = readerRepository;
    }

    public async Task<IEnumerable<Reader>> GetAllAsync(string? keyword = null)
    {
        var readers = await _readerRepository.GetAllAsync();

        if (string.IsNullOrWhiteSpace(keyword))
        {
            return readers;
        }

        return readers.Where(x =>
            x.ReaderId.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            x.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            (!string.IsNullOrWhiteSpace(x.Email) && x.Email.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
    }

    public async Task<Reader?> GetByIdAsync(string readerId)
    {
        return await _readerRepository.GetByIdAsync(readerId);
    }

    public async Task<(bool Success, string Message, Reader? Data)> CreateAsync(Reader reader)
    {
        if (string.IsNullOrWhiteSpace(reader.ReaderId) || string.IsNullOrWhiteSpace(reader.FullName))
        {
            return (false, MessageConstants.InvalidData, null);
        }

        if (await _readerRepository.ExistsAsync(reader.ReaderId))
        {
            return (false, MessageConstants.DuplicateReaderId, null);
        }

        NormalizeReader(reader);
        await _readerRepository.AddAsync(reader);
        await _readerRepository.SaveChangesAsync();

        return (true, "Tạo bạn đọc thành công.", reader);
    }

    public async Task<(bool Success, string Message, Reader? Data)> UpdateAsync(string readerId, Reader reader)
    {
        var existing = await _readerRepository.GetByIdAsync(readerId);
        if (existing is null)
        {
            return (false, MessageConstants.NotFound, null);
        }

        existing.FullName = reader.FullName;
        existing.DoB = reader.DoB;
        existing.Gender = reader.Gender;
        existing.Address = reader.Address;
        existing.Phone = reader.Phone;
        existing.Email = reader.Email;
        existing.AvatarUrl = reader.AvatarUrl;

        NormalizeReader(existing);
        await _readerRepository.UpdateAsync(existing);
        await _readerRepository.SaveChangesAsync();

        return (true, "Cập nhật bạn đọc thành công.", existing);
    }

    public async Task<(bool Success, string Message)> DeleteAsync(string readerId)
    {
        var existing = await _readerRepository.GetByIdAsync(readerId);
        if (existing is null)
        {
            return (false, MessageConstants.NotFound);
        }

        if (await _readerRepository.HasActiveBorrowAsync(readerId))
        {
            return (false, MessageConstants.ReaderHasActiveBorrow);
        }

        await _readerRepository.DeleteAsync(existing);
        await _readerRepository.SaveChangesAsync();

        return (true, "Xóa bạn đọc thành công.");
    }

    private static void NormalizeReader(Reader reader)
    {
        reader.FullName = reader.FullName.Trim();
        reader.Email = reader.Email?.Trim();
        reader.Address = reader.Address?.Trim();
        reader.Phone = reader.Phone?.Trim();
        reader.Gender = reader.Gender?.Trim();
        reader.AvatarUrl = reader.AvatarUrl?.Trim();
    }
}
