using Core.Shared.Entities;
using Core.Shared.Interfaces;
using Core.Shared.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Shared.Services
{
    /// <summary>
    /// Service xử lý business logic quản lý sách
    /// </summary>
    public class BookService : IBookService
    {
        private readonly BookRepository _repository;

        public BookService(BookRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Book>> GetAllBooksAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Book?> GetBookByIdAsync(string bookId)
        {
            if (string.IsNullOrWhiteSpace(bookId))
                return null;

            return await _repository.GetByIdAsync(bookId);
        }

        public async Task<List<Book>> SearchBooksAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await _repository.GetAllAsync();

            return await _repository.SearchAsync(searchTerm.Trim());
        }

        public async Task<List<Book>> GetBooksByCategoryAsync(int categoryId)
        {
            if (categoryId <= 0)
                return new List<Book>();

            return await _repository.GetByCategoryAsync(categoryId);
        }

        public async Task<List<Book>> GetAvailableBooksAsync()
        {
            return await _repository.GetAvailableAsync();
        }

        public async Task<(bool Success, string Message)> AddBookAsync(Book book)
        {
            // Validate đầu vào
            if (book == null)
                return (false, "Thông tin sách không hợp lệ");

            if (string.IsNullOrWhiteSpace(book.BookId))
                return (false, "Mã sách không được để trống");

            if (string.IsNullOrWhiteSpace(book.Title))
                return (false, "Tên sách không được để trống");

            if (book.BookId.Length > 20)
                return (false, "Mã sách không được quá 20 ký tự");

            if (book.Title.Length > 255)
                return (false, "Tên sách không được quá 255 ký tự");

            // Kiểm tra mã sách trùng
            if (await _repository.ExistsAsync(book.BookId))
                return (false, "Mã sách này đã tồn tại trong hệ thống");

            // Validate số lượng
            if (book.Quantity == null)
                book.Quantity = 0;

            if (book.Quantity < 0)
                return (false, "Số lượng sách không được âm");

            // Set status mặc định
            if (string.IsNullOrWhiteSpace(book.Status))
                book.Status = "Có thể mượn";

            var result = await _repository.AddAsync(book);
            if (result)
                return (true, "Thêm sách thành công");
            else
                return (false, "Thêm sách thất bại. Vui lòng thử lại");
        }

        public async Task<(bool Success, string Message)> UpdateBookAsync(Book book)
        {
            if (book == null)
                return (false, "Thông tin sách không hợp lệ");

            if (string.IsNullOrWhiteSpace(book.BookId))
                return (false, "Mã sách không được để trống");

            if (string.IsNullOrWhiteSpace(book.Title))
                return (false, "Tên sách không được để trống");

            // Kiểm tra sách có tồn tại
            var existingBook = await _repository.GetByIdAsync(book.BookId);
            if (existingBook == null)
                return (false, "Không tìm thấy sách cần cập nhật");

            // Validate số lượng
            if (book.Quantity == null)
                book.Quantity = 0;

            if (book.Quantity < 0)
                return (false, "Số lượng sách không được âm");

            var result = await _repository.UpdateAsync(book);
            if (result)
                return (true, "Cập nhật sách thành công");
            else
                return (false, "Cập nhật sách thất bại. Vui lòng thử lại");
        }

        public async Task<(bool Success, string Message)> DeleteBookAsync(string bookId)
        {
            if (string.IsNullOrWhiteSpace(bookId))
                return (false, "Mã sách không hợp lệ");

            // Kiểm tra sách có tồn tại
            var book = await _repository.GetByIdAsync(bookId);
            if (book == null)
                return (false, "Không tìm thấy sách cần xóa");

            // Kiểm tra sách có đang được mượn không
            if (await _repository.IsBookBorrowedAsync(bookId))
                return (false, "Không thể xóa sách đang được mượn. Vui lòng kiểm tra lại");

            var result = await _repository.DeleteAsync(bookId);
            if (result)
                return (true, "Xóa sách thành công");
            else
                return (false, "Xóa sách thất bại. Vui lòng thử lại");
        }

        public async Task<bool> BookIdExistsAsync(string bookId)
        {
            if (string.IsNullOrWhiteSpace(bookId))
                return false;

            return await _repository.ExistsAsync(bookId);
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _repository.GetAllCategoriesAsync();
        }

        public async Task<(bool Success, string Message)> UpdateBookQuantityAsync(string bookId, int quantityChange)
        {
            if (string.IsNullOrWhiteSpace(bookId))
                return (false, "Mã sách không hợp lệ");

            var book = await _repository.GetByIdAsync(bookId);
            if (book == null)
                return (false, "Không tìm thấy sách");

            int newQuantity = (book.Quantity ?? 0) + quantityChange;
            if (newQuantity < 0)
                return (false, "Số lượng sách không được âm");

            var result = await _repository.UpdateQuantityAsync(bookId, quantityChange);
            if (result)
                return (true, "Cập nhật số lượng sách thành công");
            else
                return (false, "Cập nhật số lượng sách thất bại");
        }
    }
}
