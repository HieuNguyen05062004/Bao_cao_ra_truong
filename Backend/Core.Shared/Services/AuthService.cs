using System.Security.Cryptography;
using System.Text;
using Core.Shared.Constants;
using Core.Shared.Entities;
using Core.Shared.Interfaces;
using Core.Shared.Models;
using Core.Shared.Repositories;

namespace Core.Shared.Services;

public class AuthService : IAuthService
{
    private readonly AccountRepository _accountRepository;

    public AuthService(AccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<IEnumerable<Account>> GetStaffAccountsAsync()
    {
        return await _accountRepository.GetStaffAccountsAsync();
    }

    public async Task<Account?> GetStaffByUsernameAsync(string username)
    {
        return await _accountRepository.GetByUsernameAsync(username);
    }

    public async Task<(bool Success, string Message, Account? Data)> CreateStaffAsync(StaffUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Role))
        {
            return (false, MessageConstants.InvalidData, null);
        }

        if (request.Role != RoleConstants.Admin && request.Role != RoleConstants.Staff)
        {
            return (false, MessageConstants.InvalidData, null);
        }

        if (await _accountRepository.ExistsAsync(request.Username))
        {
            return (false, MessageConstants.DuplicateUsername, null);
        }

        var account = new Account
        {
            Username = request.Username.Trim(),
            Password = HashPassword(request.Password),
            FullName = request.FullName?.Trim(),
            Email = request.Email?.Trim(),
            Role = request.Role,
            AvatarUrl = request.AvatarUrl?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _accountRepository.AddAsync(account);
        await _accountRepository.SaveChangesAsync();

        return (true, "Tạo nhân viên thành công.", account);
    }

    public async Task<(bool Success, string Message, Account? Data)> UpdateStaffAsync(string username, StaffUpsertRequest request)
    {
        var account = await _accountRepository.GetByUsernameAsync(username);
        if (account is null)
        {
            return (false, MessageConstants.NotFound, null);
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            account.Password = HashPassword(request.Password);
        }

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            if (request.Role != RoleConstants.Admin && request.Role != RoleConstants.Staff)
            {
                return (false, MessageConstants.InvalidData, null);
            }

            account.Role = request.Role;
        }

        account.FullName = request.FullName?.Trim();
        account.Email = request.Email?.Trim();
        account.AvatarUrl = request.AvatarUrl?.Trim();

        await _accountRepository.UpdateAsync(account);
        await _accountRepository.SaveChangesAsync();

        return (true, "Cập nhật nhân viên thành công.", account);
    }

    public async Task<(bool Success, string Message)> DeleteStaffAsync(string username)
    {
        var account = await _accountRepository.GetByUsernameAsync(username);
        if (account is null)
        {
            return (false, MessageConstants.NotFound);
        }

        if (await _accountRepository.IsUsedInBorrowAsync(username))
        {
            return (false, MessageConstants.AccountInUse);
        }

        await _accountRepository.DeleteAsync(account);
        await _accountRepository.SaveChangesAsync();

        return (true, "Xóa nhân viên thành công.");
    }

    private static string HashPassword(string rawPassword)
    {
        const int iterations = 100000;
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(rawPassword),
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            32);

        return $"PBKDF2${iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }
}
