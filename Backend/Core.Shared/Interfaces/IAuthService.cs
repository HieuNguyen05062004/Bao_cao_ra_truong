using Core.Shared.Entities;
using Core.Shared.Models;

namespace Core.Shared.Interfaces;

public interface IAuthService
{
    Task<IEnumerable<Account>> GetStaffAccountsAsync();
    Task<Account?> GetStaffByUsernameAsync(string username);
    Task<(bool Success, string Message, Account? Data)> CreateStaffAsync(StaffUpsertRequest request);
    Task<(bool Success, string Message, Account? Data)> UpdateStaffAsync(string username, StaffUpsertRequest request);
    Task<(bool Success, string Message)> DeleteStaffAsync(string username);
}
