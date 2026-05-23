/**
 * SmartLibrary Admin Dashboard Logic
 */
document.addEventListener("DOMContentLoaded", () => {
  const app = new AdminDashboard();
  window.app = app; // Xuất ra global để gọi từ onclick trong HTML
  app.init();
});

class AdminDashboard {
  constructor() {
    // Sidebar link update logic
    this.updateSidebarLinks();

    this.recentBorrows = [
      {
        id: "MT-001",
        reader: "Nguyễn Văn A",
        books: ["Lập trình C# cơ bản"],
        date: "2024-05-15",
        dueDate: "2024-05-25",
        status: "Đang mượn",
      },
      {
        id: "MT-002",
        reader: "Trần Thị B",
        books: ["Cấu trúc dữ liệu"],
        date: "2024-05-16",
        dueDate: "2024-05-26",
        status: "Trả đúng hạn",
      },
      {
        id: "MT-003",
        reader: "Lê Văn C",
        books: ["Thiết kế Web hiện đại"],
        date: "2024-05-17",
        dueDate: "2024-05-27",
        status: "Đang mượn",
      },
      {
        id: "MT-004",
        reader: "Phạm Minh D",
        books: ["Học máy với Python"],
        date: "2024-05-18",
        dueDate: "2024-05-28",
        status: "Đang mượn",
      },
    ];

    this.books = [
      {
        id: "S001",
        title: "Lập trình C# nâng cao",
        author: "Phạm Duy Lễ",
        category: "CNTT",
        stock: 15,
        status: "Còn sách", // Derived from stock
        img: "https://via.placeholder.com/45x60",
        publisher: "NXB Giáo dục",
        publishYear: 2023,
        description:
          "Cuốn sách hướng dẫn chuyên sâu về các kỹ thuật lập trình C# hiện đại.",
      },
      {
        id: "S002",
        title: "Thiết kế thuật toán",
        author: "Nguyễn Văn B",
        category: "CNTT",
        stock: 0, // Derived from stock
        status: "Hết sách", // Derived from stock
        img: "https://via.placeholder.com/45x60",
        publisher: "NXB Khoa học",
        publishYear: 2022,
        description:
          "Phân tích và thiết kế các thuật toán tối ưu cho bài toán thực tế.",
      },
      {
        id: "S003",
        title: "Kinh tế vi mô",
        author: "Trần Thị C",
        category: "Kinh tế",
        stock: 8, // Derived from stock
        status: "Còn sách", // Derived from stock
        img: "https://via.placeholder.com/45x60",
        publisher: "NXB Thống kê",
        publishYear: 2024,
        description:
          "Kiến thức cơ bản về quy luật cung cầu và hành vi người tiêu dùng.",
      },
      {
        id: "S004",
        title: "Tiếng Anh chuyên ngành",
        author: "John Doe",
        category: "Ngoại ngữ",
        stock: 20, // Derived from stock
        status: "Còn sách", // Derived from stock
        img: "https://via.placeholder.com/45x60",
        publisher: "NXB Tổng hợp",
        publishYear: 2021,
        description: "Từ vựng và ngữ pháp chuyên ngành Công nghệ thông tin.",
      },
    ];

    this.readers = [
      {
        id: "R001",
        name: "Nguyễn Văn A",
        email: "vana@gmail.com",
        phone: "0912345678",
        address: "Hà Nội",
        status: "Hoạt động", // Status is still relevant for active/inactive
        img: "https://ui-avatars.com/api/?name=Nguyen+Van+A&background=random",
        dob: "1990-01-15",
        gender: "Nam",
        borrowingCount: 2,
        overdueCount: 0,
      },
      {
        id: "R002",
        name: "Trần Thị B",
        email: "thib@gmail.com",
        phone: "0987654321",
        address: "Đà Nẵng",
        status: "Hoạt động",
        img: "https://ui-avatars.com/api/?name=Tran+Thi+B&background=random",
        dob: "1992-03-20",
        gender: "Nữ",
        borrowingCount: 1,
        overdueCount: 1,
      },
      {
        id: "R003",
        name: "Lê Văn C",
        email: "vanc@gmail.com",
        phone: "0905123456",
        address: "TP.HCM",
        status: "Đang khóa",
        img: "https://ui-avatars.com/api/?name=Le+Van+C&background=random",
        dob: "1988-11-05",
        gender: "Nam",
        borrowingCount: 0,
        overdueCount: 0,
      },
    ];

    this.categories = [
      { id: 1, name: "CNTT", bookCount: 25 },
      { id: 2, name: "Kinh tế", bookCount: 12 },
      { id: 3, name: "Ngoại ngữ", bookCount: 18 },
      { id: 4, name: "Văn học", bookCount: 30 },
    ];

    this.staffs = [
      {
        id: 1,
        name: "Lê Minh Admin",
        email: "minhlee@library.com",
        role: "Admin",
        createdAt: "2023-10-01",
        img: "https://ui-avatars.com/api/?name=Admin+Lee&background=0D8ABC&color=fff",
      },
      {
        id: 2,
        name: "Nguyễn Thị Hoa",
        email: "hoant@library.com",
        role: "Staff",
        createdAt: "2024-01-15",
        img: "https://ui-avatars.com/api/?name=Hoa+Nguyen&background=green&color=fff",
      },
    ];

    this.borrowTickets = [
      {
        id: 101,
        readerId: "R001",
        readerName: "Nguyễn Văn A",
        borrowDate: "2024-05-15",
        dueDate: "2024-05-25",
        returnDate: null,
        status: "Chờ duyệt",
        staff: null,
        books: [
          {
            id: "S001",
            title: "Lập trình C# nâng cao",
            author: "Phạm Duy Lễ",
            status: "Có thể mượn",
          },
        ],
      },
      {
        id: 102,
        readerId: "R002",
        readerName: "Trần Thị B",
        borrowDate: "2024-05-10",
        dueDate: "2024-05-20",
        returnDate: "2024-05-19",
        status: "Đã trả",
        staff: "admin",
        books: [
          {
            id: "S003",
            title: "Kinh tế vi mô",
            author: "Trần Thị C",
            status: "Có thể mượn",
          },
        ],
      },
    ];
  }

  updateSidebarLinks() {
    // Đồng bộ các link cũ sang link ql-*.html
    document.querySelectorAll(".sidebar-nav a").forEach((link) => {
      const href = link.getAttribute("href");
      if (href === "ban-doc.html") link.setAttribute("href", "ql-ban-doc.html");
      if (href === "the-loai.html")
        link.setAttribute("href", "ql-danh-muc.html");
    });
  }

  init() {
    this.renderRecentActivity();
    this.renderBooks();
    this.renderReaders();
    this.renderCategories();
    this.renderStaffs();
    this.renderBorrowTickets();
    this.renderBorrowTicketDetails();
    this.renderStaffDetailView();
    this.setupEventListeners();
    this.setupValidationListeners();
    this.setupSidebarToggle();
    this.renderBookDetailView(); // Thêm hàm này để chạy khi ở trang ct-sach.html
    this.renderReaderDetailView(); // Thêm cho trang chi tiết bạn đọc
    this.renderCategoryDetailView(); // Thêm cho trang chi tiết danh mục
    console.log("SmartLib Admin initialized.");
  }

  setupSidebarToggle() {
    const toggleBtns = document.querySelectorAll(".sidebar-toggle");
    const sidebar = document.querySelector(".sidebar");

    // Tạo overlay nếu chưa có
    let overlay = document.querySelector(".sidebar-overlay");
    if (!overlay) {
      overlay = document.createElement("div");
      overlay.className = "sidebar-overlay";
      document.body.appendChild(overlay);
    }

    toggleBtns.forEach((btn) => {
      btn.addEventListener("click", () => {
        sidebar.classList.toggle("active");
        overlay.classList.toggle("active");
      });
    });

    overlay.addEventListener("click", () => {
      sidebar.classList.remove("active");
      overlay.classList.remove("active");
    });
  }

  // --- Validation Helpers ---
  showError(input, message) {
    const parent = input.closest(".form-group");
    if (!parent) return;
    let error = parent.querySelector(".error-msg");
    if (!error) {
      error = document.createElement("span");
      error.className = "error-msg";
      error.style.color = "red";
      error.style.fontSize = "0.8rem";
      error.style.marginTop = "5px";
      error.style.display = "block";
      parent.appendChild(error);
    }
    error.textContent = message;
    input.style.borderColor = "red";
  }

  clearError(input) {
    const parent = input.closest(".form-group");
    if (!parent) return;
    const error = parent.querySelector(".error-msg");
    if (error) error.remove();
    input.style.borderColor = "";
  }

  // --- Validation Logic ---
  validateBookForm() {
    const fields = {
      title: document.getElementById("form-title"),
      author: document.getElementById("form-author"),
      publisher: document.getElementById("form-publisher"),
      img: document.getElementById("form-img"),
      description: document.getElementById("form-description"),
    };
    let isValid = true;

    if (
      !fields.title ||
      fields.title.value.trim().length < 5 ||
      fields.title.value.trim().length > 20
    ) {
      this.showError(
        fields.title,
        "Tên sách không được để trống và phải từ 5 - 20 ký tự.",
      );
      isValid = false;
    } else this.clearError(fields.title);

    if (
      !fields.author ||
      fields.author.value.trim().length < 5 ||
      fields.author.value.trim().length > 20
    ) {
      this.showError(
        fields.author,
        "Tên tác giả không được để trống và phải từ 5 - 20 ký tự.",
      );
      isValid = false;
    } else this.clearError(fields.author);

    if (
      !fields.publisher ||
      fields.publisher.value.trim().length < 5 ||
      fields.publisher.value.trim().length > 20
    ) {
      this.showError(
        fields.publisher,
        "Nhà xuất bản không được để trống và phải từ 5 - 20 ký tự.",
      );
      isValid = false;
    } else this.clearError(fields.publisher);

    const categoryContainer = document.getElementById(
      "category-fields-container",
    );
    if (categoryContainer) {
      const hasCategory = Array.from(
        categoryContainer.querySelectorAll("select"),
      ).some((s) => s.value !== "");
      if (!hasCategory) {
        this.showError(
          categoryContainer,
          "Vui lòng chọn ít nhất một thể loại sách.",
        );
        isValid = false;
      } else this.clearError(categoryContainer);
    }

    if (
      !document.getElementById("edit-id")?.value &&
      fields.img?.files.length === 0
    ) {
      this.showError(fields.img, "Vui lòng tải lên hình ảnh bìa sách.");
      isValid = false;
    } else if (fields.img) this.clearError(fields.img);

    if (
      !fields.description ||
      fields.description.value.trim().length < 10 ||
      fields.description.value.trim().length > 900
    ) {
      this.showError(
        fields.description,
        "Mô tả không được để trống và phải từ 10 - 900 ký tự.",
      );
      isValid = false;
    } else this.clearError(fields.description);

    return isValid;
  }

  validateReaderForm() {
    const rFields = {
      name: document.getElementById("form-reader-name"),
      phone: document.getElementById("form-reader-phone"),
      gender: document.getElementById("form-reader-gender"),
      dob: document.getElementById("form-reader-dob"),
      email: document.getElementById("form-reader-email"),
      address: document.getElementById("form-reader-address"),
      img: document.getElementById("form-reader-img"),
    };
    let isValid = true;

    if (
      !rFields.name ||
      rFields.name.value.trim().length < 5 ||
      rFields.name.value.trim().length > 20
    ) {
      this.showError(
        rFields.name,
        "Họ và tên không được để trống và phải từ 5 - 20 ký tự.",
      );
      isValid = false;
    } else this.clearError(rFields.name);

    if (!rFields.phone || !/^\d{10}$/.test(rFields.phone.value.trim())) {
      this.showError(
        rFields.phone,
        "Số điện thoại phải nhập đúng đủ 10 chữ số.",
      );
      isValid = false;
    } else this.clearError(rFields.phone);

    if (!rFields.gender || !rFields.gender.value) {
      this.showError(rFields.gender, "Vui lòng chọn giới tính.");
      isValid = false;
    } else this.clearError(rFields.gender);

    if (!rFields.dob || !rFields.dob.value) {
      this.showError(rFields.dob, "Vui lòng chọn ngày sinh.");
      isValid = false;
    } else this.clearError(rFields.dob);

    if (
      !rFields.email ||
      !/^[^\s@]+@gmail\.com$/.test(rFields.email.value.trim())
    ) {
      this.showError(
        rFields.email,
        "Email không hợp lệ (Phải đúng định dạng @gmail.com).",
      );
      isValid = false;
    } else this.clearError(rFields.email);

    if (
      !rFields.address ||
      rFields.address.value.trim().length < 5 ||
      rFields.address.value.trim().length > 100
    ) {
      this.showError(
        rFields.address,
        "Địa chỉ không được để trống và phải từ 5 - 100 ký tự.",
      );
      isValid = false;
    } else this.clearError(rFields.address);

    if (
      !document.getElementById("edit-reader-flag")?.value &&
      rFields.img?.files.length === 0
    ) {
      this.showError(rFields.img, "Vui lòng tải lên ảnh đại diện bạn đọc.");
      isValid = false;
    } else if (rFields.img) this.clearError(rFields.img);

    return isValid;
  }

  validateCategoryForm() {
    const catNameField = document.getElementById("form-category-name");
    if (!catNameField) return true;
    const val = catNameField.value.trim();
    if (val.length < 5 || val.length > 20) {
      this.showError(
        catNameField,
        "Tên danh mục không được để trống và phải từ 5 - 20 ký tự.",
      );
      return false;
    }
    this.clearError(catNameField);
    return true;
  }

  validateStaffForm() {
    const fields = {
      password: document.getElementById("form-staff-password"),
      role: document.getElementById("form-staff-role"),
      img: document.getElementById("form-staff-img"),
    };
    const PASS_REGEX = /^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*(),.?":{}|<>]).{9,}$/;
    let isValid = true;

    if (fields.password) {
      const passVal = fields.password.value.trim();
      if (passVal === "" || !PASS_REGEX.test(passVal)) {
        this.showError(
          fields.password,
          "Mật khẩu phải dài hơn 8 ký tự, bao gồm chữ hoa, số và ký tự đặc biệt.",
        );
        isValid = false;
      } else this.clearError(fields.password);
    }

    if (fields.role && fields.role.value === "") {
      this.showError(fields.role, "Vui lòng chọn quyền hạn cho tài khoản.");
      isValid = false;
    } else if (fields.role) this.clearError(fields.role);

    if (
      fields.img &&
      fields.img.files.length === 0 &&
      !document.getElementById("edit-staff-id")?.value
    ) {
      this.showError(fields.img, "Vui lòng tải lên ảnh đại diện nhân viên.");
      isValid = false;
    } else if (fields.img) this.clearError(fields.img);

    return isValid;
  }

  setupValidationListeners() {
    // Real-time Staff Validation
    const staffPass = document.getElementById("form-staff-password");
    const PASS_REGEX = /^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*(),.?":{}|<>]).{9,}$/;
    staffPass?.addEventListener("input", () => {
      if (PASS_REGEX.test(staffPass.value.trim())) this.clearError(staffPass);
      else
        this.showError(
          staffPass,
          "Mật khẩu phải dài hơn 8 ký tự, bao gồm chữ hoa, số và ký tự đặc biệt.",
        );
    });

    const staffRole = document.getElementById("form-staff-role");
    staffRole?.addEventListener("change", () => {
      if (staffRole.value !== "") this.clearError(staffRole);
    });

    // Real-time Category Validation
    const catName = document.getElementById("form-category-name");
    catName?.addEventListener("input", () => {
      if (catName.value.trim().length >= 5 && catName.value.trim().length <= 20)
        this.clearError(catName);
    });

    // Real-time Reader Validation
    const readerName = document.getElementById("form-reader-name");
    readerName?.addEventListener("input", () => {
      if (
        readerName.value.trim().length >= 5 &&
        readerName.value.trim().length <= 20
      )
        this.clearError(readerName);
    });
    const readerPhone = document.getElementById("form-reader-phone");
    readerPhone?.addEventListener("input", () => {
      if (/^\d{10}$/.test(readerPhone.value.trim()))
        this.clearError(readerPhone);
    });
    const readerEmail = document.getElementById("form-reader-email");
    readerEmail?.addEventListener("input", () => {
      if (/^[^\s@]+@gmail\.com$/.test(readerEmail.value.trim()))
        this.clearError(readerEmail);
    });

    // Real-time Book Validation
    const bookTitle = document.getElementById("form-title");
    bookTitle?.addEventListener("input", () => {
      if (
        bookTitle.value.trim().length >= 5 &&
        bookTitle.value.trim().length <= 20
      )
        this.clearError(bookTitle);
    });
  }

  renderStaffDetailView() {
    const urlParams = new URLSearchParams(window.location.search);
    const staffId = urlParams.get("id");
    const headerTitle = document.getElementById("display-header-title");
    if (!staffId || !headerTitle) return;

    const staff = this.staffs.find((s) => s.id == staffId);
    if (staff) {
      document.title = `Chi tiết nhân viên: ${staff.name} - SmartLibrary`;
      headerTitle.innerText = `👤 Chi tiết nhân viên: ${staff.name}`;
      const img = document.getElementById("staff-detail-img");
      if (img) img.src = staff.img;

      document.getElementById("detail-staff-name").innerText = staff.name;
      document.getElementById("detail-staff-email").innerText = staff.email;

      const roleSpan = document.getElementById("detail-staff-role");
      if (roleSpan) {
        roleSpan.innerText =
          staff.role === "Admin" ? "Quản trị viên" : "Thủ thư";
        roleSpan.className = `badge ${staff.role === "Admin" ? "badge-primary" : "badge-info"}`;
      }

      const createdSpan = document.getElementById("detail-staff-created");
      if (createdSpan && staff.createdAt) {
        createdSpan.innerText = new Date(staff.createdAt).toLocaleString(
          "vi-VN",
        );
      }
    }
  }

  renderRecentActivity() {
    const tableBody = document.getElementById("recent-borrows");
    if (!tableBody) return;

    tableBody.innerHTML = this.recentBorrows
      .map(
        (item) => `
            <tr>
                <td><strong>#${item.id}</strong></td>
                <td>${item.reader}</td>
                <td>${item.books.join(", ")}</td>
                <td>${new Date(item.date).toLocaleDateString("vi-VN")}</td>
                <td>${new Date(item.dueDate).toLocaleDateString("vi-VN")}</td>
                <td>
                    <span class="badge ${item.status === "Trả đúng hạn" ? "badge-success" : "badge-warning"}">
                        ${item.status}
                    </span>
                </td>
                <td>
                    <a href="muon-tra.html" class="btn btn-sm btn-view" title="Chi tiết">
                        <i class="fas fa-eye"></i>
                    </a>
                </td>
            </tr>
        `,
      )
      .join("");
  }

  renderBooks() {
    const bookTable = document.getElementById("book-list");
    if (!bookTable) return;

    bookTable.innerHTML = this.books
      .map(
        (book) => `
      <tr>
        <td><strong>${book.id}</strong></td>
        <td><img src="${book.img}" class="table-img" alt="cover"></td>
        <td>${book.title}</td>
        <td>${book.author}</td>
        <td>${book.category}</td>
        <td>${book.stock}</td>
        <td>
          <span class="badge ${book.stock > 0 ? "badge-success" : "badge-danger"}">
            ${book.status}
          </span>
        </td>
        <td>
          <div class="actions">
            <a href="ct-sach.html?id=${book.id}" class="btn btn-sm btn-view" title="Xem chi tiết">
              <i class="fas fa-eye"></i>
            </a>
            <button class="btn btn-sm btn-edit" onclick="app.openModal('${book.id}')" title="Sửa">
              <i class="fas fa-edit"></i>
            </button>
            <button class="btn btn-sm btn-delete" onclick="app.deleteBook('${book.id}')" title="Xóa">
              <i class="fas fa-trash"></i>
            </button>
          </div>
        </td>
      </tr>
    `,
      )
      .join("");
  }

  renderReaders() {
    const readerTable = document.getElementById("reader-list");
    if (!readerTable) return;

    readerTable.innerHTML = this.readers
      .map(
        (reader) => `
      <tr>
        <td><strong>${reader.id}</strong></td>
        <td>${reader.name}</td>
        <td>${reader.gender || "Nam"}</td>
        <td>${reader.phone}</td>
        <td>${reader.email}</td>
        <td><span class="badge badge-warning">${reader.borrowingCount || 0}</span></td>
        <td><span class="badge badge-danger">${reader.overdueCount || 0}</span></td>
        <td>
          <div class="actions">
            <a href="ct-ban-doc.html?id=${reader.id}" class="btn btn-sm btn-view" title="Xem chi tiết">
              <i class="fas fa-eye"></i>
            </a>
            <button class="btn btn-sm btn-edit" onclick="app.openReaderModal('${reader.id}')" title="Sửa">
              <i class="fas fa-edit"></i>
            </button>
            <button class="btn btn-sm btn-delete" onclick="app.deleteReader('${reader.id}')" title="Xóa">
              <i class="fas fa-trash"></i>
            </button>
          </div>
        </td>
      </tr>
    `,
      )
      .join("");
  }

  renderStaffs() {
    const staffTable = document.getElementById("staff-list");
    if (!staffTable) return;

    staffTable.innerHTML = this.staffs
      .map(
        (s) => `
      <tr>
        <td><img src="${s.img}" class="table-img" style="border-radius: 50%" alt="avatar"></td>
        <td>${s.name}</td>
        <td>${s.email}</td>
        <td><span class="badge ${s.role === "Admin" ? "badge-primary" : "badge-info"}">${s.role}</span></td>
        <td>${new Date(s.createdAt).toLocaleDateString("vi-VN")}</td>
        <td>
          <div class="actions">
            <a href="ct-nhan-vien.html?id=${s.id}" class="btn btn-sm btn-view" title="Xem chi tiết">
              <i class="fas fa-eye"></i>
            </a>
            <button class="btn btn-sm btn-edit" onclick="app.openStaffModal(${s.id})" title="Sửa"><i class="fas fa-edit"></i></button>
            <button class="btn btn-sm btn-delete" onclick="app.deleteStaff(${s.id})" title="Xóa"><i class="fas fa-trash"></i></button>
          </div>
        </td>
      </tr>
    `,
      )
      .join("");
  }

  openStaffModal(staffId = null) {
    const modal = document.getElementById("staff-modal");
    const form = document.getElementById("staff-form");
    const title = document.getElementById("staff-modal-title");
    const passHelp = document.getElementById("password-help");

    if (!modal || !form) return;
    form.reset();
    document.getElementById("edit-staff-id").value = "";
    passHelp.style.display = "none";

    if (staffId) {
      const staff = this.staffs.find((s) => s.id === staffId);
      if (staff) {
        title.innerText = "Chỉnh sửa nhân viên";
        document.getElementById("edit-staff-id").value = staff.id;
        document.getElementById("form-staff-name").value = staff.name;
        document.getElementById("form-staff-email").value = staff.email;
        document.getElementById("form-staff-role").value = staff.role;
        passHelp.style.display = "block";
      }
    } else {
      title.innerText = "Thêm nhân viên mới";
    }
    modal.classList.add("active");
  }

  closeStaffModal() {
    document.getElementById("staff-modal")?.classList.remove("active");
  }

  async handleStaffFormSubmit(e) {
    const editId = document.getElementById("edit-staff-id").value;
    const formImgInput = document.getElementById("form-staff-img");
    let imageUrl = "https://ui-avatars.com/api/?name=Staff&background=random";

    if (formImgInput.files && formImgInput.files.length > 0) {
      imageUrl = await new Promise((resolve) => {
        const reader = new FileReader();
        reader.onloadend = () => resolve(reader.result);
        reader.readAsDataURL(formImgInput.files[0]);
      });
    } else if (editId) {
      imageUrl = this.staffs.find((s) => s.id == editId)?.img;
    }

    const staffData = {
      id: editId ? parseInt(editId) : this.staffs.length + 1,
      name: document.getElementById("form-staff-name").value,
      email: document.getElementById("form-staff-email").value,
      role: document.getElementById("form-staff-role").value,
      img: imageUrl,
      createdAt: editId
        ? this.staffs.find((s) => s.id == editId).createdAt
        : new Date().toISOString().split("T")[0],
    };

    const isAdding = !editId;
    this.openConfirmationModal(
      isAdding ? "Xác nhận thêm" : "Xác nhận sửa",
      `Lưu thông tin nhân viên <strong>${staffData.name}</strong>?`,
      () => {
        if (isAdding) {
          this.staffs.push(staffData);
          this.showToast("Thêm nhân viên thành công!", "success");
        } else {
          const idx = this.staffs.findIndex((s) => s.id == editId);
          if (idx !== -1) this.staffs[idx] = staffData;
          this.showToast("Cập nhật thành công!", "success");
        }
        this.renderStaffs();
        this.closeStaffModal();
      },
    );
  }

  deleteStaff(id) {
    const staff = this.staffs.find((s) => s.id === id);
    if (staff && staff.id === 1) {
      this.showToast("Không thể xóa tài khoản Admin hệ thống!", "error");
      return;
    }

    this.openConfirmationModal(
      "Xác nhận xóa",
      `Bạn có chắc muốn xóa nhân viên <strong>${staff.name}</strong>?`,
      () => {
        this.staffs = this.staffs.filter((s) => s.id !== id);
        this.renderStaffs();
        this.showToast("Đã xóa nhân viên", "success");
      },
    );
  }

  renderCategories() {
    const categoryTable = document.getElementById("category-list");
    if (!categoryTable) return;

    categoryTable.innerHTML = this.categories
      .map(
        (cat) => `
      <tr>
        <td><strong>#${cat.id}</strong></td>
        <td>${cat.name}</td>
        <td>${cat.bookCount}</td>
        <td>
          <div class="actions">
            <a href="ct-danh-muc.html?id=${cat.id}" class="btn btn-sm btn-view" title="Xem chi tiết">
              <i class="fas fa-eye"></i>
            </a>
            <button class="btn btn-sm btn-edit" onclick="app.openCategoryModal(${cat.id})" title="Sửa">
              <i class="fas fa-edit"></i>
            </button>
            <button class="btn btn-sm btn-delete" onclick="app.deleteCategory(${cat.id})" title="Xóa">
              <i class="fas fa-trash"></i>
            </button>
          </div>
        </td>
      </tr>
    `,
      )
      .join("");
  }

  openCategoryModal(catId = null) {
    const modal = document.getElementById("category-modal");
    const form = document.getElementById("category-form");
    const title = document.getElementById("category-modal-title");

    if (!modal || !form) return;
    form.reset();
    document.getElementById("edit-category-id").value = "";

    if (catId) {
      const cat = this.categories.find((c) => c.id === catId);
      if (cat) {
        title.innerText = "Chỉnh sửa danh mục";
        document.getElementById("edit-category-id").value = cat.id;
        document.getElementById("form-category-name").value = cat.name;
      }
    } else {
      title.innerText = "Thêm danh mục mới";
    }
    modal.classList.add("active");
  }

  viewCategoryDetails(id) {
    const cat = this.categories.find((c) => c.id === id);
    if (cat) {
      document.getElementById("detail-category-id").innerText = `#${cat.id}`;
      document.getElementById("detail-category-name").innerText = cat.name;
      document.getElementById("detail-category-count").innerText =
        `${cat.bookCount} cuốn sách`;

      const modal = document.getElementById("category-details-modal");
      if (modal) modal.classList.add("active");
    }
  }

  closeCategoryDetailsModal() {
    document
      .getElementById("category-details-modal")
      ?.classList.remove("active");
  }

  closeCategoryModal() {
    document.getElementById("category-modal")?.classList.remove("active");
  }

  deleteCategory(id) {
    const cat = this.categories.find((c) => c.id === id);
    if (!cat) return;

    // Giả lập logic backend: Không cho xóa nếu danh mục có sách
    if (cat.bookCount > 0) {
      this.showToast(
        `Không thể xóa danh mục "${cat.name}" vì đang có ${cat.bookCount} sách thuộc thể loại này!`,
        "error",
      );
      return;
    }

    this.openConfirmationModal(
      "Xác nhận xóa danh mục",
      `Bạn có chắc chắn muốn xóa danh mục <strong>${cat.name}</strong> không?`,
      () => {
        this.categories = this.categories.filter((c) => c.id !== id);
        this.renderCategories();
        this.showToast("Đã xóa danh mục thành công", "success");
      },
    );
  }

  handleCategoryFormSubmit(e) {
    e.preventDefault();
    const editId = document.getElementById("edit-category-id").value;
    const catName = document.getElementById("form-category-name").value;

    const isAdding = !editId;
    this.openConfirmationModal(
      isAdding ? "Xác nhận thêm danh mục" : "Xác nhận lưu thay đổi",
      `Lưu danh mục <strong>${catName}</strong>?`,
      () => {
        if (isAdding) {
          const newId =
            this.categories.length > 0
              ? Math.max(...this.categories.map((c) => c.id)) + 1
              : 1;
          this.categories.push({ id: newId, name: catName, bookCount: 0 });
          this.showToast("Thêm danh mục thành công!", "success");
        } else {
          const idx = this.categories.findIndex((c) => c.id == editId);
          if (idx !== -1) this.categories[idx].name = catName;
          this.showToast("Cập nhật danh mục thành công!", "success");
        }
        this.renderCategories();
        this.closeCategoryModal();
      },
    );
  }

  renderBorrowTickets() {
    const list = document.getElementById("borrow-ticket-list");
    if (!list) return;

    list.innerHTML = this.borrowTickets
      .map((t) => {
        let badgeClass = "badge-warning";
        if (t.status === "Đã trả") badgeClass = "badge-success";
        if (t.status === "Bị từ chối") badgeClass = "badge-danger";
        if (t.status === "Đang mượn") badgeClass = "badge-primary";

        return `
        <tr>
          <td><strong>#${t.id}</strong></td>
          <td>${t.readerName}</td>
          <td>${t.books.map((b) => b.title).join(", ")}</td>
          <td>${new Date(t.borrowDate).toLocaleDateString("vi-VN")}</td>
          <td>${new Date(t.dueDate).toLocaleDateString("vi-VN")}</td>
          <td><span class="badge ${badgeClass}">${t.status}</span></td>
          <td>
            <div class="actions">
              <a href="ct-muon-tra.html?id=${t.id}" class="btn btn-sm btn-view" title="Chi tiết">
                <i class="fas fa-eye"></i>
              </a>
            </div>
          </td>
        </tr>
      `;
      })
      .join("");
  }

  renderBorrowTicketDetails() {
    const ticketIdSpan = document.getElementById("detail-ticket-id");
    if (!ticketIdSpan) return;

    const urlParams = new URLSearchParams(window.location.search);
    const id = parseInt(urlParams.get("id"));
    const ticket = this.borrowTickets.find((t) => t.id === id);

    if (!ticket) {
      this.showToast("Không tìm thấy phiếu mượn", "error");
      return;
    }

    // Fill data
    ticketIdSpan.innerText = ticket.id;
    document.getElementById("detail-ticket-status").innerText = ticket.status;
    document.getElementById("detail-borrow-date").innerText = new Date(
      ticket.borrowDate,
    ).toLocaleDateString("vi-VN");
    document.getElementById("detail-due-date").innerText = new Date(
      ticket.dueDate,
    ).toLocaleDateString("vi-VN");
    document.getElementById("detail-return-date").innerText = ticket.returnDate
      ? new Date(ticket.returnDate).toLocaleDateString("vi-VN")
      : "—";
    document.getElementById("detail-staff").innerText = ticket.staff || "—";

    document.getElementById("detail-reader-id").innerText = ticket.readerId;
    document.getElementById("detail-reader-name").innerText = ticket.readerName;
    // Mock data for demo
    document.getElementById("detail-reader-email").innerText = "vana@gmail.com";
    document.getElementById("detail-reader-phone").innerText = "0912345678";

    // Render actions
    const actionsContainer = document.getElementById("ticket-actions");
    let actionHtml = "";
    if (ticket.status === "Chờ duyệt") {
      actionHtml = `
        <button class="btn btn-primary" onclick="app.updateTicketStatus(${ticket.id}, 'Đã duyệt')">Duyệt phiếu</button>
        <button class="btn btn-danger" onclick="app.showRejectModal(${ticket.id})">Từ chối</button>
      `;
    } else if (ticket.status === "Đã duyệt") {
      actionHtml = `<button class="btn btn-primary" onclick="app.updateTicketStatus(${ticket.id}, 'Đang mượn')">Xác nhận giao sách</button>`;
    } else if (ticket.status === "Đang mượn") {
      actionHtml = `<button class="btn btn-success" onclick="app.updateTicketStatus(${ticket.id}, 'Đã trả')">Xác nhận trả sách</button>`;
    }
    actionsContainer.innerHTML = actionHtml;

    // Render books
    document.getElementById("detail-books-list").innerHTML = ticket.books
      .map(
        (b) => `
      <tr>
        <td><code>${b.id}</code></td>
        <td>${b.title}</td>
        <td>${b.author}</td>
        <td><span class="badge badge-success">${b.status}</span></td>
      </tr>
    `,
      )
      .join("");
  }

  updateTicketStatus(id, newStatus) {
    this.openConfirmationModal(
      "Xác nhận thay đổi trạng thái",
      `Bạn có muốn chuyển trạng thái phiếu #${id} sang <strong>${newStatus}</strong>?`,
      () => {
        const ticket = this.borrowTickets.find((t) => t.id === id);
        if (ticket) {
          ticket.status = newStatus;
          ticket.staff = "admin";
          if (newStatus === "Đã trả")
            ticket.returnDate = new Date().toISOString().split("T")[0];
          this.showToast(`Cập nhật trạng thái thành công!`, "success");
          this.renderBorrowTicketDetails();
        }
      },
    );
  }

  showRejectModal(id) {
    const container = document.getElementById("reject-reason-container");
    if (container) container.style.display = "block";
    this.openConfirmationModal(
      "Từ chối phiếu",
      `Xác nhận từ chối phiếu #${id}?`,
      () => {
        const ticket = this.borrowTickets.find((t) => t.id === id);
        if (ticket) {
          ticket.status = "Bị từ chối";
          this.showToast("Đã từ chối phiếu mượn", "warning");
          this.renderBorrowTicketDetails();
        }
      },
    );
  }

  openModal(bookId = null) {
    const modal = document.getElementById("book-modal");
    const form = document.getElementById("book-form");
    const title = document.getElementById("modal-title");

    form.reset();
    document.getElementById("edit-id").value = "";

    if (bookId) {
      const book = this.books.find((b) => b.id === bookId);
      if (book) {
        title.innerText = "Chỉnh sửa sách";
        document.getElementById("edit-id").value = book.id;
        document.getElementById("form-title").value = book.title;
        document.getElementById("form-author").value = book.author;
        document.getElementById("form-category").value = book.category;
        document.getElementById("form-stock").value = book.stock;
        document.getElementById("form-img").value = book.img;
        document.getElementById("form-publisher").value = book.publisher;
        document.getElementById("form-publish-year").value = book.publishYear;
        document.getElementById("form-description").value =
          book.description || "";
        this.setSubmitButtonText("Lưu thay đổi");
      }
    } else {
      title.innerText = "Thêm sách mới";
      this.setSubmitButtonText("Thêm sách");
    }

    modal.classList.add("active");
  }

  openReaderModal(readerId = null) {
    const modal = document.getElementById("reader-modal");
    const form = document.getElementById("reader-form");
    const title = document.getElementById("reader-modal-title");
    const submitBtn = document.getElementById("reader-submit-button");

    if (!modal || !form) return;
    form.reset();
    document.getElementById("edit-reader-flag").value = "";
    document.getElementById("form-reader-id").readOnly = false;
    document.getElementById("password-group").style.display = "block";

    if (readerId) {
      const reader = this.readers.find((r) => r.id === readerId);
      if (reader) {
        title.innerText = "Chỉnh sửa bạn đọc";
        document.getElementById("edit-reader-flag").value = "edit";
        document.getElementById("form-reader-id").value = reader.id;
        document.getElementById("form-reader-id").readOnly = true;
        document.getElementById("form-reader-name").value = reader.name;
        document.getElementById("form-reader-email").value = reader.email;
        document.getElementById("form-reader-phone").value = reader.phone;
        document.getElementById("form-reader-address").value = reader.address;
        document.getElementById("form-reader-status").value = reader.status;
        document.getElementById("form-reader-dob").value = reader.dob;
        document.getElementById("form-reader-gender").value =
          reader.gender || "Nam";
        document.getElementById("password-group").style.display = "none"; // Ẩn mật khẩu khi sửa (có trang riêng để đổi)
        submitBtn.innerText = "Lưu thay đổi";
      }
    } else {
      title.innerText = "Thêm bạn đọc mới";
      submitBtn.innerText = "Thêm bạn đọc";
    }
    modal.classList.add("active");
  }

  closeReaderModal() {
    const modal = document.getElementById("reader-modal");
    if (modal) modal.classList.remove("active");
  }

  setSubmitButtonText(text) {
    const btn = document.getElementById("submit-button");
    if (btn) btn.innerText = text;
  }

  viewBookDetails(id) {
    const book = this.books.find((b) => b.id === id);
    if (book) {
      document.getElementById("detail-id").innerText = book.id;
      document.getElementById("detail-title").innerText = book.title;
      document.getElementById("detail-author").innerText =
        book.author || "Chưa rõ";
      document.getElementById("detail-publisher").innerText =
        book.publisher || "Chưa rõ";
      document.getElementById("detail-year").innerText =
        book.publishYear || "Chưa rõ";
      document.getElementById("detail-category").innerText = book.category;
      document.getElementById("detail-stock").innerText = book.stock;
      document.getElementById("detail-img").src = book.img;

      const statusBadge = document.getElementById("detail-status");
      statusBadge.innerText = book.status;
      statusBadge.className = `badge ${book.stock > 0 ? "badge-success" : "badge-danger"}`;

      document.getElementById("details-modal").classList.add("active");
    }
  }

  renderReaderDetailView() {
    const urlParams = new URLSearchParams(window.location.search);
    const readerId = urlParams.get("id");
    if (!readerId || !document.getElementById("detail-reader-id")) return;

    const reader = this.readers.find((r) => r.id === readerId);
    if (reader) {
      document.getElementById("detail-reader-id").innerText = reader.id;
      document.getElementById("detail-reader-name").innerText = reader.name;
      document.getElementById("detail-reader-gender").innerText =
        reader.gender || "Nam";
      document.getElementById("detail-reader-dob").innerText = new Date(
        reader.dob,
      ).toLocaleDateString("vi-VN");
      document.getElementById("detail-reader-email").innerText = reader.email;
      document.getElementById("detail-reader-phone").innerText = reader.phone;
      document.getElementById("detail-reader-address").innerText =
        reader.address || "Chưa cập nhật";
      document.getElementById("detail-reader-img").src = reader.img;

      const statusBadge = document.getElementById("detail-reader-status");
      statusBadge.innerText = reader.status;
      statusBadge.className = `badge ${reader.status === "Hoạt động" ? "badge-success" : "badge-danger"}`;

      // Đổ dữ liệu thống kê giả lập
      document.getElementById("reader-total-borrowed").innerText =
        reader.borrowingCount + 5;
      document.getElementById("reader-current-borrowing").innerText =
        reader.borrowingCount;
      document.getElementById("reader-overdue").innerText = reader.overdueCount;
    }
  }

  renderCategoryDetailView() {
    const urlParams = new URLSearchParams(window.location.search);
    const catId = parseInt(urlParams.get("id"));
    if (!catId || !document.getElementById("detail-category-id")) return;

    const cat = this.categories.find((c) => c.id === catId);
    if (cat) {
      document.getElementById("detail-category-id").innerText = `#${cat.id}`;
      document.getElementById("detail-category-name").innerText = cat.name;
      document.getElementById("detail-category-total").innerText =
        cat.bookCount;

      const bookList = document.getElementById("category-books-list");
      if (bookList) {
        const filteredBooks = this.books.filter((b) => b.category === cat.name);
        bookList.innerHTML = filteredBooks
          .map(
            (b) => `
          <tr>
            <td><strong>${b.id}</strong></td>
            <td>${b.title}</td>
            <td>${b.author}</td>
            <td><span class="badge ${b.stock > 0 ? "badge-success" : "badge-danger"}">${b.status}</span></td>
            <td><a href="ct-sach.html?id=${b.id}" class="btn btn-sm btn-view"><i class="fas fa-eye"></i></a></td>
          </tr>
        `,
          )
          .join("");
      }
    }
  }

  viewReaderDetails(id) {
    const reader = this.readers.find((r) => r.id === id);
    if (reader) {
      document.getElementById("detail-reader-id").innerText = reader.id;
      document.getElementById("detail-reader-name").innerText = reader.name;
      document.getElementById("detail-reader-gender").innerText =
        reader.gender || "Nam";
      document.getElementById("detail-reader-email").innerText = reader.email;
      document.getElementById("detail-reader-phone").innerText = reader.phone;
      document.getElementById("detail-reader-address").innerText =
        reader.address || "Chưa cập nhật";
      document.getElementById("reader-detail-img").src = reader.img;

      const statusBadge = document.getElementById("detail-reader-status");
      statusBadge.innerText = reader.status;
      statusBadge.className = `badge ${reader.status === "Hoạt động" ? "badge-success" : "badge-danger"}`;
      document.getElementById("detail-reader-dob").innerText = new Date(
        reader.dob,
      ).toLocaleDateString("vi-VN");
      document.getElementById("reader-details-modal").classList.add("active");
    }
  }

  closeDetailsModal() {
    document.getElementById("details-modal").classList.remove("active");
  }

  closeReaderDetailsModal() {
    document.getElementById("reader-details-modal").classList.remove("active");
  }

  closeBookModal() {
    document.getElementById("book-modal").classList.remove("active");
  }

  showToast(message, type = "success") {
    const container = document.getElementById("toast-container");
    if (!container) return;

    const toast = document.createElement("div");
    toast.className = `toast ${type}`;

    const icon =
      type === "success"
        ? "fa-check-circle"
        : type === "error"
          ? "fa-times-circle"
          : "fa-exclamation-circle";

    toast.innerHTML = `
      <i class="fas ${icon}"></i>
      <span class="toast-message">${message}</span>
    `;

    container.appendChild(toast);

    // Xóa toast sau khi hiệu ứng kết thúc (3s delay + 0.5s fade out)
    setTimeout(() => {
      toast.remove();
    }, 3600);
  }

  openConfirmationModal(title, message, onConfirmCallback) {
    const confirmModal = document.getElementById("confirm-modal");
    document.getElementById("confirm-modal-title").innerText = title;
    document.getElementById("confirm-modal-message").innerText = message;

    const confirmActionBtn = document.getElementById("confirm-action-btn");
    // Remove previous event listener to prevent multiple calls
    confirmActionBtn.replaceWith(confirmActionBtn.cloneNode(true));
    document
      .getElementById("confirm-action-btn")
      .addEventListener("click", () => {
        onConfirmCallback();
        this.closeConfirmationModal();
      });

    confirmModal.classList.add("active");
  }

  closeConfirmationModal() {
    document.getElementById("confirm-modal").classList.remove("active");
  }

  deleteBook(id) {
    this.openConfirmationModal(
      "Xác nhận xóa sách",
      `Bạn có chắc chắn muốn xóa sách có mã <strong>${id}</strong> không? Hành động này không thể hoàn tác.`,
      () => {
        this.books = this.books.filter((b) => b.id !== id);
        this.renderBooks();
        this.showToast(`Đã xóa thành công sách có mã: ${id}`, "success");
      },
    );
  }

  deleteReader(id) {
    this.openConfirmationModal(
      "Xác nhận xóa bạn đọc",
      `Bạn có chắc chắn muốn xóa bạn đọc <strong>${id}</strong>? Dữ liệu này không thể khôi phục.`,
      () => {
        this.readers = this.readers.filter((r) => r.id !== id);
        this.renderReaders();
        this.showToast(`Đã xóa bạn đọc: ${id}`, "success");
      },
    );
  }

  async handleFormSubmit(e) {
    e.preventDefault();
    const editId = document.getElementById("edit-id").value;
    const formImgInput = document.getElementById("form-img");
    let imageUrl = "https://via.placeholder.com/45x60";

    // Xử lý đọc file ảnh nếu có
    if (formImgInput.files && formImgInput.files.length > 0) {
      imageUrl = await new Promise((resolve) => {
        const reader = new FileReader();
        reader.onloadend = () => resolve(reader.result);
        reader.readAsDataURL(formImgInput.files[0]);
      });
    } else if (editId) {
      const existingBook = this.books.find((b) => b.id === editId);
      if (existingBook) imageUrl = existingBook.img;
    }

    const bookData = {
      id: editId || `S00${this.books.length + 1}`,
      title: document.getElementById("form-title").value,
      author: document.getElementById("form-author").value,
      category: document.getElementById("form-category").value,
      stock: parseInt(document.getElementById("form-stock").value),
      img: imageUrl,
      publisher: document.getElementById("form-publisher").value,
      publishYear: parseInt(document.getElementById("form-publish-year").value),
      description: document.getElementById("form-description").value,
      status:
        parseInt(document.getElementById("form-stock").value) > 0
          ? "Còn sách"
          : "Hết sách", // Status derived from stock
    };

    const isAdding = !editId;
    const confirmTitle = isAdding
      ? "Xác nhận thêm sách"
      : "Xác nhận lưu thay đổi";
    const confirmMessage = isAdding
      ? `Bạn có muốn thêm sách <strong>"${bookData.title}"</strong> vào thư viện không?`
      : `Bạn có muốn lưu các thay đổi cho sách <strong>"${bookData.title}"</strong> không?`;

    this.openConfirmationModal(confirmTitle, confirmMessage, () => {
      if (isAdding) {
        this.books.push(bookData);
        this.showToast(
          `Đã thêm sách "${bookData.title}" thành công!`,
          "success",
        );
      } else {
        const index = this.books.findIndex((b) => b.id === editId);
        if (index !== -1) {
          this.books[index] = bookData;
          this.showToast(
            `Cập nhật sách "${bookData.title}" thành công!`,
            "success",
          );
        }
      }
      this.renderBooks();
      this.closeBookModal(); // Close the input modal after confirmation and save
    });
  }

  async handleReaderFormSubmit(e) {
    e.preventDefault();
    const isEdit = document.getElementById("edit-reader-flag").value === "edit";
    const readerIdInput = document.getElementById("form-reader-id").value;
    const formImgInput = document.getElementById("form-reader-img");
    let imageUrl = "https://ui-avatars.com/api/?name=User&background=random";

    if (formImgInput.files && formImgInput.files.length > 0) {
      imageUrl = await new Promise((resolve) => {
        const reader = new FileReader();
        reader.onloadend = () => resolve(reader.result);
        reader.readAsDataURL(formImgInput.files[0]);
      });
    } else if (isEdit) {
      const existing = this.readers.find((r) => r.id === readerIdInput);
      if (existing) imageUrl = existing.img;
    }

    const readerData = {
      id: readerIdInput,
      name: document.getElementById("form-reader-name").value,
      email: document.getElementById("form-reader-email").value,
      phone: document.getElementById("form-reader-phone").value,
      address: document.getElementById("form-reader-address").value,
      status: document.getElementById("form-reader-status").value,
      dob: document.getElementById("form-reader-dob").value,
      gender: document.getElementById("form-reader-gender").value,
      img: imageUrl,
      borrowingCount: isEdit
        ? this.readers.find((r) => r.id === readerIdInput)?.borrowingCount || 0
        : 0,
      overdueCount: isEdit
        ? this.readers.find((r) => r.id === readerIdInput)?.overdueCount || 0
        : 0,
    };

    const isAdding = !isEdit;

    // Kiểm tra trùng mã khi thêm mới
    if (isAdding && this.readers.some((r) => r.id === readerIdInput)) {
      this.showToast("Mã bạn đọc đã tồn tại!", "error");
      return;
    }

    this.openConfirmationModal(
      isAdding ? "Xác nhận thêm bạn đọc" : "Xác nhận lưu thay đổi",
      `Bạn có muốn lưu thông tin bạn đọc <strong>${readerData.name}</strong> không?`,
      () => {
        if (isAdding) this.readers.push(readerData);
        else {
          const idx = this.readers.findIndex((r) => r.id === readerIdInput);
          if (idx !== -1) this.readers[idx] = readerData;
        }
        this.renderReaders();
        this.closeReaderModal();
        this.showToast(
          isAdding ? "Thêm bạn đọc thành công!" : "Cập nhật thành công!",
          "success",
        );
      },
    );
  }

  setupEventListeners() {
    const searchInput = document.querySelector(".header-search input");
    searchInput?.addEventListener("keyup", (e) => {
      if (e.key === "Enter") {
        const query = e.target.value;
        this.showToast(`Đang tìm kiếm: ${query}`, "warning");
      }
    });

    // Modal events
    document
      .getElementById("btn-add-book")
      ?.addEventListener("click", () => this.openModal());

    // Close book modal
    document
      .querySelector("#book-modal .close-modal-btn")
      ?.addEventListener("click", () => this.closeBookModal());

    // Close details modal
    document
      .getElementById("close-details-x")
      ?.addEventListener("click", () => this.closeDetailsModal());
    document
      .getElementById("close-details-btn")
      ?.addEventListener("click", () => this.closeDetailsModal());

    // Reader modal events
    document
      .getElementById("btn-add-reader")
      ?.addEventListener("click", () => this.openReaderModal());
    document
      .querySelector("#reader-modal .close-modal-btn")
      ?.addEventListener("click", () => this.closeReaderModal());
    document
      .getElementById("close-reader-details-x")
      ?.addEventListener("click", () => this.closeReaderDetailsModal());
    document
      .getElementById("close-reader-details-btn")
      ?.addEventListener("click", () => this.closeReaderDetailsModal());
    document
      .getElementById("reader-form")
      ?.addEventListener("submit", (e) => this.handleReaderFormSubmit(e));

    // Category events
    document
      .getElementById("btn-add-category")
      ?.addEventListener("click", () => this.openCategoryModal());
    document
      .querySelector("#category-modal .close-modal-btn")
      ?.addEventListener("click", () => this.closeCategoryModal());
    document
      .getElementById("category-form")
      ?.addEventListener("submit", (e) => {
        if (this.validateCategoryForm()) this.handleCategoryFormSubmit(e);
        else e.preventDefault();
      });

    // Staff form
    document.getElementById("staff-form")?.addEventListener("submit", (e) => {
      if (this.validateStaffForm()) this.handleStaffFormSubmit(e);
      else e.preventDefault();
    });

    // Dynamic Category fields in Book Modal
    document
      .getElementById("add-category-field")
      ?.addEventListener("click", () => {
        const container = document.getElementById("category-fields-container");
        if (!container) return;
        const firstSelect = container.querySelector("select");
        const newFieldGroup = document.createElement("div");
        newFieldGroup.style.display = "flex";
        newFieldGroup.style.gap = "5px";
        newFieldGroup.style.marginBottom = "8px";
        const newSelect = firstSelect.cloneNode(true);
        newSelect.style.flex = "1";
        newSelect.style.marginBottom = "0";
        const removeBtn = document.createElement("button");
        removeBtn.type = "button";
        removeBtn.className = "btn btn-outline";
        removeBtn.innerHTML = '<i class="fas fa-times"></i>';
        removeBtn.style.padding = "5px 10px";
        removeBtn.onclick = () => newFieldGroup.remove();
        newFieldGroup.appendChild(newSelect);
        newFieldGroup.appendChild(removeBtn);
        container.appendChild(newFieldGroup);
      });

    // Category details events
    document
      .getElementById("close-category-details-x")
      ?.addEventListener("click", () => this.closeCategoryDetailsModal());
    document
      .getElementById("close-category-details-btn")
      ?.addEventListener("click", () => this.closeCategoryDetailsModal());

    // Close confirm modal
    document
      .querySelector("#confirm-modal .close-modal-btn")
      ?.addEventListener("click", () => this.closeConfirmationModal());
    document
      .querySelector("#confirm-modal .btn-outline")
      ?.addEventListener("click", () => this.closeConfirmationModal());

    // Close modals when clicking outside
    window.addEventListener("click", (e) => {
      if (e.target.id === "book-modal") this.closeBookModal();
      if (e.target.id === "details-modal") this.closeDetailsModal();
      if (e.target.id === "reader-modal") this.closeReaderModal();
      if (e.target.id === "category-modal") this.closeCategoryModal();
      if (e.target.id === "category-details-modal")
        this.closeCategoryDetailsModal();
      if (e.target.id === "reader-details-modal")
        this.closeReaderDetailsModal();
      if (e.target.id === "confirm-modal") this.closeConfirmationModal();
      if (e.target.id === "staff-modal") this.closeStaffModal();
    });

    document.getElementById("book-form")?.addEventListener("submit", (e) => {
      if (this.validateBookForm()) this.handleFormSubmit(e);
      else e.preventDefault();
    });
  }
}
