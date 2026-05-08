using Core.Shared.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Shared.Interfaces
{
    /// <summary>
    /// Interface cho dịch vụ quản lý sách
    /// </summary>
    public interface IBookService
    {
        /// <summary>
        /// Lấy tất cả sách
        /// </summary>
        Task<List<Book>> GetAllBooksAsync();

        /// <summary>
        /// Lấy sách theo ID
        /// </summary>
        Task<Book?> GetBookByIdAsync(string bookId);

        /// <summary>
        /// Tìm kiếm sách theo tên hoặc tác giả
        /// </summary>
        Task<List<Book>> SearchBooksAsync(string searchTerm);

        /// <summary>
        /// Lấy sách theo thể loại
        /// </summary>
        Task<List<Book>> GetBooksByCategoryAsync(int categoryId);

        /// <summary>
        /// Lấy sách còn hàng
        /// </summary>
        Task<List<Book>> GetAvailableBooksAsync();

        /// <summary>
        /// Thêm sách mới
        /// </summary>
        Task<(bool Success, string Message)> AddBookAsync(Book book);

        /// <summary>
        /// Cập nhật thông tin sách
        /// </summary>
        Task<(bool Success, string Message)> UpdateBookAsync(Book book);

        /// <summary>
        /// Xóa sách
        /// </summary>
        Task<(bool Success, string Message)> DeleteBookAsync(string bookId);

        /// <summary>
        /// Kiểm tra mã sách có tồn tại
        /// </summary>
        Task<bool> BookIdExistsAsync(string bookId);

        /// <summary>
        /// Lấy danh sách thể loại
        /// </summary>
        Task<List<Category>> GetAllCategoriesAsync();

        /// <summary>
        /// Cập nhật số lượng sách
        /// </summary>
        Task<(bool Success, string Message)> UpdateBookQuantityAsync(string bookId, int quantityChange);
    }
}
