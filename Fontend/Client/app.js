class SmartLibraryApp {
  constructor() {
    this.init();
  }

  init() {
    console.log("SmartLibrary AI App initialized...");
    this.createToastContainer();
    this.setupEventListeners();
    if (document.getElementById("latest-books-grid")) {
      this.loadHomepageData();
    }
    if (document.getElementById("all-books-grid")) {
      this.loadAllBooksPage();
    }
    if (document.getElementById("book-detail-page")) {
      this.loadBookDetailPage();
    }
    if (document.getElementById("history-page")) {
      this.loadBorrowHistoryPage();
    }
    if (document.getElementById("ticket-detail-page")) {
      this.loadTicketDetailPage();
    }
  }

  createToastContainer() {
    if (!document.getElementById("toast-container")) {
      const container = document.createElement("div");
      container.id = "toast-container";
      document.body.appendChild(container);
    }
  }

  showToast(message, type = "success") {
    const container = document.getElementById("toast-container");
    const toast = document.createElement("div");
    toast.className = `toast ${type}`;

    const icon =
      type === "success"
        ? "fa-check-circle"
        : type === "danger"
          ? "fa-exclamation-circle"
          : "fa-info-circle";

    toast.innerHTML = `
      <i class="fas ${icon}"></i>
      <span>${message}</span>
    `;

    container.appendChild(toast);
    setTimeout(() => toast.classList.add("active"), 10);
    setTimeout(() => {
      toast.classList.remove("active");
      setTimeout(() => toast.remove(), 300);
    }, 3000);
  }

  async loadHomepageData() {
    // Trong thực tế, bạn sẽ fetch từ API của ASP.NET Core:
    // const latest = await fetch('/api/books/latest?count=5').then(res => res.json());
    // const trending = await fetch('/api/books/trending?count=5').then(res => res.json());

    // Giả lập dữ liệu để hiển thị
    const mockLatest = Array(5).fill({
      id: "L1",
      title: "Sách mới nhất 2024",
      author: "Admin",
      category: "Công nghệ",
      status: "Sẵn có",
    });
    const mockTrending = Array(5).fill({
      id: "T1",
      title: "Sách mượn nhiều nhất",
      author: "Tác giả Hot",
      category: "Kinh tế",
      status: "Sẵn có",
    });

    this.renderBookGrid("latest-books-grid", mockLatest);
    this.renderBookGrid("trending-books-grid", mockTrending);
  }

  async loadBorrowHistoryPage() {
    // Giả lập dữ liệu từ BorrowTickets
    const mockHistory = [
      {
        id: "TKT-001",
        date: "10/05/2026",
        due: "17/05/2026",
        count: 2,
        status: "Approved",
        statusText: "Đã duyệt",
      },
      {
        id: "TKT-002",
        date: "15/05/2026",
        due: "22/05/2026",
        count: 1,
        status: "Pending",
        statusText: "Chờ duyệt",
      },
      {
        id: "TKT-003",
        date: "01/05/2026",
        due: "08/05/2026",
        count: 3,
        status: "Returned",
        statusText: "Đã trả",
      },
      {
        id: "TKT-005",
        date: "12/05/2026",
        due: "19/05/2026",
        count: 1,
        status: "Borrowing",
        statusText: "Đang mượn",
      },
      {
        id: "TKT-004",
        date: "20/04/2026",
        due: "27/04/2026",
        count: 1,
        status: "Rejected",
        statusText: "Bị từ chối",
      },
    ];

    const tableBody = document.getElementById("borrow-history-table");
    const searchInput = document.getElementById("history-search");
    const statusFilter = document.getElementById("status-filter");
    if (!tableBody) return;

    const renderTable = (data) => {
      if (data.length === 0) {
        tableBody.innerHTML = `<tr><td colspan="6" class="text-center p-xl">Không tìm thấy phiếu mượn nào.</td></tr>`;
        return;
      }
      tableBody.innerHTML = data
        .map(
          (item) => `
        <tr>
          <td class="fw-bold text-accent">${item.id}</td>
          <td>${item.date}</td>
          <td>${item.due}</td>
          <td>${item.count} cuốn</td>
          <td>
            <span class="status-badge status-${item.status.toLowerCase()}">
              ${item.statusText}
            </span>
          </td>
          <td class="text-center">
            <button class="btn btn-outline btn-sm" onclick="app.viewTicketDetails('${item.id}')">
              <i class="fas fa-eye"></i> Chi tiết
            </button>
            ${
              item.status === "Pending"
                ? `
            <button class="btn btn-danger btn-sm" style="margin-left: 8px" onclick="app.deleteBorrowTicket('${item.id}')">
              <i class="fas fa-trash"></i> Xóa
            </button>
            `
                : ""
            }
          </td>
        </tr>
      `,
        )
        .join("");
    };

    const filterData = () => {
      const searchTerm = searchInput.value.toLowerCase();
      const statusValue = statusFilter.value.toLowerCase();

      const filtered = mockHistory.filter((item) => {
        const matchesSearch = item.id.toLowerCase().includes(searchTerm);
        const matchesStatus =
          statusValue === "all" || item.status.toLowerCase() === statusValue;
        return matchesSearch && matchesStatus;
      });
      renderTable(filtered);
    };

    // Sử dụng property assignment để tránh tích tụ event listener khi load lại dữ liệu
    searchInput.oninput = filterData;
    statusFilter.onchange = filterData;

    // Initial render
    setTimeout(() => renderTable(mockHistory), 300);
  }

  deleteBorrowTicket(id) {
    this.openConfirmModal(
      "Xác nhận hủy yêu cầu",
      `Bạn có chắc chắn muốn xóa và hủy yêu cầu mượn sách <strong>${id}</strong> không? Hành động này không thể hoàn tác.`,
      () => {
        // Trong thực tế, bạn sẽ gọi API DELETE tại đây
        this.showToast(`Đã xóa yêu cầu mượn sách ${id} thành công!`);
        this.loadBorrowHistoryPage();
      },
    );
  }

  openConfirmModal(title, message, onConfirm) {
    const modal = document.getElementById("confirm-modal");
    if (!modal) return;

    document.getElementById("confirm-modal-title").innerText = title;
    document.getElementById("confirm-modal-message").innerHTML = message;

    modal.classList.add("active");

    const closeModal = () => modal.classList.remove("active");

    document.getElementById("btn-confirm-action").onclick = () => {
      onConfirm();
      closeModal();
    };
    document.getElementById("btn-confirm-cancel").onclick = closeModal;
    document.getElementById("close-confirm-modal").onclick = closeModal;
  }

  viewTicketDetails(id) {
    window.location.href = `ct-phieu-muon.html?id=${id}`;
  }

  async loadTicketDetailPage() {
    const urlParams = new URLSearchParams(window.location.search);
    const ticketId = urlParams.get("id") || "TKT-001";

    // Giả lập dữ liệu phiếu mượn chi tiết
    const mockTicket = {
      id: ticketId,
      readerName: "hieu (RR66520)",
      borrowDate: "12/05/2026",
      dueDate: "19/05/2026",
      returnDate: ticketId === "TKT-003" ? "08/05/2026" : "Chưa trả",
      status: ticketId === "TKT-003" ? "Returned" : "Borrowing",
      statusText: ticketId === "TKT-003" ? "Đã trả" : "Đang mượn",
      books: [
        { title: "Lập trình .NET 10 & AI", quantity: 1 },
        { title: "Cấu trúc dữ liệu và Giải thuật", quantity: 1 },
      ],
    };

    // Hiển thị thông tin header
    document.getElementById("display-ticket-id").innerText = mockTicket.id;
    const statusEl = document.getElementById("display-status");
    statusEl.innerText = mockTicket.statusText;
    statusEl.className = `status-badge status-${mockTicket.status.toLowerCase()}`;

    // Hiển thị thông tin chi tiết
    document.getElementById("display-reader-name").innerText =
      mockTicket.readerName;
    document.getElementById("display-borrow-date").innerText =
      mockTicket.borrowDate;
    document.getElementById("display-due-date").innerText = mockTicket.dueDate;
    document.getElementById("display-return-date").innerText =
      mockTicket.returnDate;

    // Hiển thị danh sách sách
    const bookTable = document.getElementById("ticket-books-table");
    bookTable.innerHTML = mockTicket.books
      .map(
        (b) => `
      <tr>
        <td class="fw-bold">${b.title}</td>
        <td class="text-center">${b.quantity} cuốn</td>
      </tr>
    `,
      )
      .join("");
  }

  async loadAllBooksPage() {
    // Giả lập load danh mục
    const categories = [
      "Văn học",
      "Kinh tế",
      "Công nghệ",
      "Khoa học",
      "Kỹ năng",
    ];
    const categoryContainer = document.getElementById("category-filter-list");
    if (categoryContainer) {
      categoryContainer.innerHTML =
        '<li><label class="filter-item"><input type="checkbox" name="category" value="all" checked> <span>Tất cả thể loại</span></label></li>' +
        categories
          .map(
            (cat) => `
          <li>
            <label class="filter-item">
              <input type="checkbox" name="category" value="${cat}"> <span>${cat}</span>
            </label>
          </li>
        `,
          )
          .join("");
    }

    // Giả lập load toàn bộ sách (ví dụ 12 cuốn)
    const mockAllBooks = Array(12)
      .fill(0)
      .map((_, i) => ({
        id: `B${i}`,
        title: `Sách Kiến Thức Số ${i + 1}`,
        author: "Tác giả Tuyển Chọn",
        category: categories[i % categories.length],
        status: i % 4 === 0 ? "Hết hàng" : "Sẵn có",
        statusClass: i % 4 === 0 ? "danger" : "success",
      }));

    this.renderBookGrid("all-books-grid", mockAllBooks);
    document.getElementById("results-count").innerText =
      `Hiển thị ${mockAllBooks.length} kết quả`;
  }

  loadBookDetailPage() {
    const urlParams = new URLSearchParams(window.location.search);
    const bookId = urlParams.get("id") || "B001";
    const bookTitle = "Lập trình .NET 10 & AI";

    // Hiển thị thông tin lên trang chi tiết
    document.getElementById("detail-title").innerText = bookTitle;
    document.getElementById("detail-id").innerText = `Mã: ${bookId}`;

    // Xử lý Modal
    const modal = document.getElementById("borrow-modal");
    const openModalBtn = document.getElementById("borrow-btn");
    const closeModalBtn = document.getElementById("close-modal-btn");
    const cancelBtn = document.getElementById("cancel-borrow");
    const confirmBtn = document.getElementById("confirm-borrow");

    if (openModalBtn && modal) {
      openModalBtn.addEventListener("click", () => {
        // Tự động điền tên sách vào modal
        document.getElementById("modal-book-title").value = bookTitle;

        // Thiết lập ngày mặc định (Ngày mượn là hôm nay, ngày trả là 7 ngày sau)
        const today = new Date().toISOString().split("T")[0];
        const nextWeek = new Date();
        nextWeek.setDate(nextWeek.getDate() + 7);
        const dueDate = nextWeek.toISOString().split("T")[0];

        document.getElementById("modal-borrow-date").value = today;
        document.getElementById("modal-due-date").value = dueDate;

        modal.classList.add("active");
      });
    }

    const closeModal = () => modal.classList.remove("active");

    if (closeModalBtn) closeModalBtn.onclick = closeModal;
    if (cancelBtn) cancelBtn.onclick = closeModal;

    if (confirmBtn) {
      confirmBtn.addEventListener("click", () => {
        const borrowDate = document.getElementById("modal-borrow-date").value;
        const dueDate = document.getElementById("modal-due-date").value;

        this.showToast(`Đã gửi yêu cầu mượn sách "${bookTitle}" thành công!`);
        closeModal();
      });
    }
  }

  renderBookGrid(containerId, books) {
    const container = document.getElementById(containerId);
    if (!container) return;

    container.innerHTML = books
      .map(
        (book) => `
        <div class="card book-card">
            <div class="book-cover">
                <img src="https://via.placeholder.com/200x300?text=Cover" alt="${book.title}">
                <span class="book-tag ${book.statusClass || "success"}">${book.status}</span>
            </div>
            <div class="card-body">
                <div class="book-category">${book.category}</div>
                <h3 class="book-title">${book.title}</h3>
                <p class="book-meta">${book.author}</p>
                <a href="ct-sach.html?id=${book.id}" class="btn btn-primary btn-sm mt-md w-full">Chi tiết</a>
            </div>
        </div>
    `,
      )
      .join("");
  }

  setupEventListeners() {
    const searchBtn = document.getElementById("search-btn");
    const searchInput = document.getElementById("search-input");
    const modeToggle = document.getElementById("search-mode-toggle");
    const searchBox = document.getElementById("search-box");

    let isAiMode = true;

    if (modeToggle) {
      modeToggle.addEventListener("click", () => {
        isAiMode = !isAiMode;

        if (isAiMode) {
          searchBox.classList.remove("basic-mode");
          modeToggle.innerHTML = '<i class="fas fa-magic"></i>';
          modeToggle.title = "Chuyển sang Tìm kiếm Cơ bản";
          searchInput.placeholder =
            "Hỏi AI: 'Tìm cho tôi sách về lập trình Python cho người mới'...";
          searchBtn.innerText = "Tìm kiếm AI";
          searchBtn.className = "btn btn-accent";
        } else {
          searchBox.classList.add("basic-mode");
          modeToggle.innerHTML = '<i class="fas fa-search"></i>';
          modeToggle.title = "Chuyển sang Tìm kiếm AI";
          searchInput.placeholder = "Tìm theo tên sách, tác giả...";
          searchBtn.innerText = "Tìm kiếm";
          searchBtn.className = "btn btn-primary";
        }
      });
    }

    if (searchBtn) {
      searchBtn.addEventListener("click", () => {
        const query = searchInput.value;
        if (query) {
          const mode = isAiMode ? "AI" : "Cơ bản";
          this.showToast(`Đang thực hiện tìm kiếm ${mode}: ${query}`, "info");
        }
      });
    }

    // Mobile Menu Toggle logic
    const mobileMenuBtn = document.getElementById("mobile-menu-btn");
    const navMenu = document.getElementById("nav-menu");

    if (mobileMenuBtn && navMenu) {
      mobileMenuBtn.addEventListener("click", () => {
        navMenu.classList.toggle("active");
        // Đổi biểu tượng bars <-> times
        const icon = mobileMenuBtn.querySelector("i");
        icon.classList.toggle("fa-bars");
        icon.classList.toggle("fa-times");
      });
    }
  }
}

let app;
document.addEventListener("DOMContentLoaded", () => {
  app = new SmartLibraryApp();
});
