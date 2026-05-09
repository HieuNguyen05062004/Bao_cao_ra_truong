using Core.Shared.Data;
using Core.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Shared.Repositories
{
    /// <summary>
    /// Repository cho quản lý sách
    /// </summary>
    public class BookRepository
    {
        private readonly LibraryDbContext _context;

        public BookRepository(LibraryDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy tất cả sách
        /// </summary>
        public async Task<List<Book>> GetAllAsync()
        {
            return await _context.Books
                .Include(b => b.Category)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Lấy sách theo ID
        /// </summary>
        public async Task<Book?> GetByIdAsync(string bookId)
        {
            return await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.BookId == bookId);
        }

        /// <summary>
        /// Tìm kiếm sách theo tên hoặc tác giả
        /// </summary>
        public async Task<List<Book>> SearchAsync(string searchTerm)
        {
            searchTerm = searchTerm?.ToLower() ?? "";
            return await _context.Books
                .Include(b => b.Category)
                .Where(b => b.Title.ToLower().Contains(searchTerm) ||
                           b.Author!.ToLower().Contains(searchTerm))
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Lấy sách theo thể loại
        /// </summary>
        public async Task<List<Book>> GetByCategoryAsync(int categoryId)
        {
            return await _context.Books
                .Include(b => b.Category)
                .Where(b => b.CategoryId == categoryId)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Lấy sách còn hàng
        /// </summary>
        public async Task<List<Book>> GetAvailableAsync()
        {
            return await _context.Books
                .Include(b => b.Category)
                .Where(b => b.Quantity > 0)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Kiểm tra mã sách có tồn tại
        /// </summary>
        public async Task<bool> ExistsAsync(string bookId)
        {
            return await _context.Books.AnyAsync(b => b.BookId == bookId);
        }

        /// <summary>
        /// Thêm sách mới
        /// </summary>
        public async Task<bool> AddAsync(Book book)
        {
            try
            {
                _context.Books.Add(book);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Cập nhật sách
        /// </summary>
        public async Task<bool> UpdateAsync(Book book)
        {
            try
            {
                var existing = await _context.Books.FindAsync(book.BookId);
                if (existing == null) return false;

                existing.Title = book.Title;
                existing.Author = book.Author;
                existing.Publisher = book.Publisher;
                existing.PublishYear = book.PublishYear;
                existing.CategoryId = book.CategoryId;
                existing.Quantity = book.Quantity;
                existing.Status = book.Status;

                // Chỉ cập nhật ImageUrl nếu có ảnh mới
                if (!string.IsNullOrEmpty(book.ImageUrl))
                    existing.ImageUrl = book.ImageUrl;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Xóa sách
        /// </summary>
        public async Task<bool> DeleteAsync(string bookId)
        {
            try
            {
                var book = await _context.Books.FindAsync(bookId);
                if (book == null) return false;

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra sách có đang được mượn không
        /// </summary>
        public async Task<bool> IsBookBorrowedAsync(string bookId)
        {
            return await _context.BorrowTickets
                .Where(bt => bt.Status != "Trả hàng")
                .SelectMany(bt => bt.Books)
                .AnyAsync(b => b.BookId == bookId);
        }

        /// <summary>
        /// Cập nhật số lượng sách
        /// </summary>
        public async Task<bool> UpdateQuantityAsync(string bookId, int quantityChange)
        {
            try
            {
                var book = await _context.Books.FindAsync(bookId);
                if (book == null) return false;

                book.Quantity = (book.Quantity ?? 0) + quantityChange;
                if (book.Quantity < 0) book.Quantity = 0;

                _context.Books.Update(book);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Lấy danh sách thể loại
        /// </summary>
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
