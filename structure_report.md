# Project Directory Structure

```
Bao_cao_ra_truong/
├─ .gitignore
├─ README.md
├─ Fontend/
│  ├─ .gitignore
│  ├─ README.md
│  ├─ Acount/
│  │  ├─ index.html
│  │  └─ sign-up.html
│  ├─ Admin/
│  │  ├─ app.js
│  │  ├─ Creat-bandoc.html
│  │  ├─ Creat-danhmuc.html
│  │  ├─ Creat-nhanvien.html
│  │  ├─ Creat-sach.html
│  │  ├─ ct-ban-doc.html
│  │  ├─ ct-danh-muc.html
│  │  ├─ ct-muon-tra.html
│  │  ├─ ct-nhan-vien.html
│  │  ├─ ct-sach.html
│  │  ├─ Delete-bandoc.html
│  │  ├─ Delete-danhmuc.html
│  │  ├─ Delete-nhanvien.html
│  │  ├─ Delete-sach.html
│  │  ├─ Edit-bandoc.html
│  │  ├─ Edit-danhmuc.html
│  │  ├─ Edit-nhanvien.html
│  │  ├─ Edit-sach.html
│  │  ├─ index.html
│  │  ├─ login.html
│  │  ├─ muon-tra.html
│  │  ├─ ql-ban-doc.html
│  │  ├─ ql-danh-muc.html
│  │  ├─ ql-nhan-vien.html
│  │  ├─ ql-sach.html
│  │  └─ styles.css
│  └─ Client/
│     ├─ app.js
│     ├─ styles.css
│     ├─ ct-phieu-muon.html
│     ├─ ct-sach.html
│     ├─ index.html
│     ├─ kho-sach.html
│     └─ lich-su-muon.html
└─ Backend/
   ├─ Backend.slnx
   ├─ Admin/
   │  ├─ Admin.csproj
   │  ├─ Program.cs
   │  ├─ appsettings.json
   │  ├─ Controllers/
   │  │  ├─ AccountController.cs
   │  │  ├─ BookController.cs
   │  │  ├─ BorrowController.cs
   │  │  ├─ CategoryController.cs
   │  │  ├─ HomeController.cs
   │  │  ├─ ReaderController.cs
   │  │  └─ StaffController.cs
   │  ├─ ViewModels/
   │  │  ├─ BookViewModel.cs
   │  │  ├─ CategoryChipsViewModel.cs
   │  │  ├─ CategoryViewModel.cs
   │  │  ├─ LoginViewModel.cs
   │  │  ├─ ReaderViewModel.cs
   │  │  └─ StaffViewModel.cs
   │  ├─ Views/
   │  │  ├─ Account/
   │  │  │  └─ Login.cshtml
   │  │  ├─ Book/
   │  │  │  ├─ Create.cshtml
   │  │  │  ├─ Delete.cshtml
   │  │  │  ├─ Details.cshtml
   │  │  │  ├─ Edit.cshtml
   │  │  │  └─ Index.cshtml
   │  │  ├─ Borrow/
   │  │  │  ├─ Details.cshtml
   │  │  │  └─ Index.cshtml
   │  │  ├─ Category/
   │  │  │  ├─ Create.cshtml
   │  │  │  ├─ Delete.cshtml
   │  │  │  ├─ Details.cshtml
   │  │  │  ├─ Edit.cshtml
   │  │  │  └─ Index.cshtml
   │  │  ├─ Home/
   │  │  │  ├─ Index.cshtml
   │  │  │  └─ Privacy.cshtml
   │  │  ├─ Reader/
   │  │  │  ├─ Create.cshtml
   │  │  │  ├─ Delete.cshtml
   │  │  │  ├─ Details.cshtml
   │  │  │  ├─ Edit.cshtml
   │  │  │  └─ Index.cshtml
   │  │  ├─ Shared/
   │  │  │  ├─ _Layout.cshtml
   │  │  │  ├─ _ValidationScriptsPartial.cshtml
   │  │  │  └─ Error.cshtml
   │  │  ├─ Staff/
   │  │  │  ├─ Create.cshtml
   │  │  │  ├─ Details.cshtml
   │  │  │  ├─ Edit.cshtml
   │  │  │  └─ Index.cshtml
   │  │  ├─ _ViewImports.cshtml
   │  │  └─ _ViewStart.cshtml
   │  └─ wwwroot/
   ├─ Client/
   │  ├─ Client.csproj
   │  ├─ Program.cs
   │  ├─ appsettings.json
   │  ├─ Controllers/
   │  │  ├─ AccountController.cs
   │  │  ├─ BorrowController.cs
   │  │  ├─ ClientBaseController.cs
   │  │  ├─ HomeController.cs
   │  │  └─ SearchController.cs
   │  ├─ Extensions/
   │  │  └─ SessionExtensions.cs
   │  ├─ Models/
   │  ├─ ViewModels/
   │  │  ├─ BorrowRequestViewModel.cs
   │  │  ├─ LoginViewModel.cs
   │  │  ├─ ProfileViewModel.cs
   │  │  └─ RegisterViewModel.cs
   │  ├─ Views/
   │  │  ├─ Account/
   │  │  │  ├─ DeleteAccount.cshtml
   │  │  │  ├─ EditProfile.cshtml
   │  │  │  ├─ Login.cshtml
   │  │  │  ├─ Profile.cshtml
   │  │  │  ├─ Register.cshtml
   │  │  │  └─ RegisterSuccess.cshtml
   │  │  ├─ Book/
   │  │  │  ├─ Details.cshtml
   │  │  │  └─ Index.cshtml
   │  │  ├─ Borrow/
   │  │  │  ├─ BorrowDetail.cshtml
   │  │  │  ├─ BorrowHistory.cshtml
   │  │  │  └─ CreateBorrowRequest.cshtml
   │  │  ├─ Home/
   │  │  │  ├─ Index.cshtml
   │  │  │  └─ Privacy.cshtml
   │  │  ├─ Search/
   │  │  │  ├─ Details.cshtml
   │  │  │  └─ Index.cshtml
   │  │  ├─ Shared/
   │  │  ├─ _ViewImports.cshtml
   │  │  └─ _ViewStart.cshtml
   │  └─ wwwroot/
   └─ Core.Shared/
      ├─ Core.Shared.csproj
      ├─ Class1.cs
      ├─ Constants/
      │  ├─ MessageConstants.cs
      │  └─ RoleConstants.cs
      ├─ Data/
      │  └─ LibraryDbContext.cs
      ├─ Entities/
      │  ├─ Account.cs
      │  ├─ Book.cs
      │  ├─ BookCategory.cs
      │  ├─ BorrowTicket.cs
      │  ├─ Category.cs
      │  └─ Reader.cs
      ├─ Interfaces/
      │  ├─ IAiSearchService.cs
      │  ├─ IAuthService.cs
      │  ├─ IBookService.cs
      │  ├─ IBorrowService.cs
      │  ├─ ICategoryService.cs
      │  ├─ IReaderService.cs
      │  ├─ ISearchService.cs
      │  └─ IUnifiedAuthService.cs
      ├─ Repositories/
      │  ├─ AccountRepository.cs
      │  ├─ BookRepository.cs
      │  ├─ BorrowRepository.cs
      │  ├─ CategoryRepository.cs
      │  └─ ReaderRepository.cs
      ├─ Services/
      │  ├─ AiSearchService.cs
      │  ├─ AuthService.cs
      │  ├─ BookService.cs
      │  ├─ BorrowService.cs
      │  ├─ CategoryService.cs
      │  ├─ ReaderService.cs
      │  ├─ SearchService.cs
      │  └─ UnifiedAuthService.cs
      ├─ Utilities/
      │  ├─ IdFormatter.cs
      │  └─ IdGenerator.cs
      ├─ ViewModels/
      │  └─ UnifiedLoginViewModel.cs
      └─ Uploads/
         ├─ books/
         └─ reader-avatars/
```
