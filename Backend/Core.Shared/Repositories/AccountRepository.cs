using System;
using System.Collections.Generic;
using System.Text;
using Core.Shared.Data;
using Core.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.Shared.Repositories;

public class AccountRepository
{
    private readonly LibraryDbContext _context;

    public AccountRepository(LibraryDbContext context)
    {
        _context = context;
    }

    // Lấy tất cả tài khoản (trừ Admin gốc nếu muốn lọc ở Service)
    public async Task<IEnumerable<Account>> GetAllAsync()
    {
        return await _context.Accounts
            .OrderBy(a => a.Role)
            .ThenBy(a => a.FullName)
            .ToListAsync();
    }

    public async Task<Account?> GetByUsernameAsync(string username)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.Username == username);
    }

    public async Task<Account?> GetByEmailAsync(string email)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.Email == email);
    }

    public async Task<bool> ExistsAsync(string username)
    {
        return await _context.Accounts
            .AnyAsync(a => a.Username == username);
    }

    // Kiểm tra tài khoản có phiếu mượn liên kết không
    public async Task<bool> HasBorrowTicketsAsync(string username)
    {
        return await _context.BorrowTickets
            .AnyAsync(bt => bt.StaffUsername == username);
    }

    public async Task AddAsync(Account account)
    {
        await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Account account)
    {
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Account account)
    {
        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();
    }

    // Tìm kiếm theo keyword (username / fullname / email)
    public async Task<IEnumerable<Account>> SearchAsync(string keyword)
    {
        keyword = keyword.Trim().ToLower();
        return await _context.Accounts
            .Where(a =>
                a.Username.ToLower().Contains(keyword) ||
                (a.FullName != null && a.FullName.ToLower().Contains(keyword)) ||
                (a.Email != null && a.Email.ToLower().Contains(keyword)))
            .ToListAsync();
    }
}

