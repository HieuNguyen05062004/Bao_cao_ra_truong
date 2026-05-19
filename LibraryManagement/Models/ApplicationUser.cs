using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(200)]
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Còn hoạt động")]
        public bool IsActive { get; set; } = true;

        public ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();
    }
}
