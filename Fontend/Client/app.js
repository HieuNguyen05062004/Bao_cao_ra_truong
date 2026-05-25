class SmartLibraryApp {
  constructor() {
    this.apiUrl = "/api";
    this.init();
  }

  init() {
    console.log("SmartLibrary AI App initialized...");
    this.createToastContainer();
    this.renderAuthState();
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
    try {
      const [latest, trending] = await Promise.all([
        fetch(`${this.apiUrl}/books/latest?count=5`).then((res) =>
          res.ok ? res.json() : [],
        ),
        fetch(`${this.apiUrl}/books/trending?count=5`).then((res) =>
          res.ok ? res.json() : [],
        ),
      ]);

      this.renderBookGrid("latest-books-grid", latest);
      this.renderBookGrid("trending-books-grid", trending);
    } catch {
      this.showToast("Không thể tải dữ liệu sách từ backend.", "danger");
    }
  }

  async renderAuthState() {
    const menus = document.querySelectorAll(".user-menu");
    if (!menus.length) return;

    let me = { isAuthenticated: false };
    try {
      const response = await fetch(`${this.apiUrl}/auth/me`);
      if (response.ok) me = await response.json();
    } catch {
      me = { isAuthenticated: false };
    }

    menus.forEach((menu) => {
      if (me.isAuthenticated) {
        menu.innerHTML = `
          <span class="btn btn-outline btn-sm" title="${me.userId || ""}">
            <i class="fas fa-user-circle"></i> ${me.userName || me.userId || "Tài khoản"}
          </span>
          <button class="btn btn-accent btn-sm" data-logout>
            <i class="fas fa-sign-out-alt"></i> Đăng xuất
          </button>
        `;
        menu.querySelector("[data-logout]")?.addEventListener("click", async () => {
          await fetch(`${this.apiUrl}/auth/logout`, { method: "POST" });
          window.location.href = "index.html";
        });
      } else {
        menu.innerHTML = `
          <a class="btn btn-outline btn-sm" href="/account/index.html">Đăng nhập</a>
          <a class="btn btn-accent btn-sm" href="/account/sign-up.html">Đăng ký</a>
        `;
      }
    });
  }

  async loadBorrowHistoryPage() {
    const tableBody = document.getElementById("borrow-history-table");
    const searchInput = document.getElementById("history-search");
    const statusFilter = document.getElementById("status-filter");
    if (!tableBody) return;

    let history = [];
    try {
      const response = await fetch(`${this.apiUrl}/borrow/history`);
      if (response.status === 401) {
        tableBody.innerHTML = `
          <tr>
            <td colspan="6" class="text-center p-xl">
              Vui lòng đăng nhập để xem phiếu mượn.
              <a class="btn btn-primary btn-sm" href="/account/index.html" style="margin-left: 8px">Đăng nhập</a>
            </td>
          </tr>`;
        this.showToast("Vui lòng đăng nhập để xem phiếu mượn.", "danger");
        return;
      }
      history = response.ok ? await response.json() : [];
    } catch {
      this.showToast("Không thể tải lịch sử mượn.", "danger");
    }

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

      const filtered = history.filter((item) => {
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
    renderTable(history);
  }

  deleteBorrowTicket(id) {
    this.openConfirmModal(
      "Xác nhận hủy yêu cầu",
      `Bạn có chắc chắn muốn xóa và hủy yêu cầu mượn sách <strong>${id}</strong> không? Hành động này không thể hoàn tác.`,
      async () => {
        const rawId = String(id).replace("TKT-", "");
        const response = await fetch(`${this.apiUrl}/borrow/${parseInt(rawId)}`, {
          method: "DELETE",
        });
        if (response.ok) {
          this.showToast(`Đã xóa yêu cầu mượn sách ${id} thành công!`);
          this.loadBorrowHistoryPage();
        } else {
          const error = await response.json().catch(() => ({}));
          this.showToast(error.message || "Không thể xóa phiếu mượn.", "danger");
        }
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
    const ticketParam = urlParams.get("id") || "TKT-001";
    const rawId = parseInt(ticketParam.replace("TKT-", ""));

    const response = await fetch(`${this.apiUrl}/borrow/${rawId}`);
    if (!response.ok) {
      this.showToast("Không tìm thấy phiếu mượn.", "danger");
      return;
    }
    const ticket = await response.json();

    // Hiển thị thông tin header
    document.getElementById("display-ticket-id").innerText = ticket.id;
    const statusEl = document.getElementById("display-status");
    statusEl.innerText = ticket.statusText;
    statusEl.className = `status-badge status-${ticket.status.toLowerCase()}`;

    // Hiển thị thông tin chi tiết
    document.getElementById("display-reader-name").innerText =
      ticket.readerName || "";
    document.getElementById("display-borrow-date").innerText =
      ticket.date || "";
    document.getElementById("display-due-date").innerText = ticket.due || "";
    document.getElementById("display-return-date").innerText =
      ticket.returnDate ? new Date(ticket.returnDate).toLocaleDateString("vi-VN") : "Chưa trả";

    // Hiển thị danh sách sách
    const bookTable = document.getElementById("ticket-books-table");
    bookTable.innerHTML = ticket.books
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
    const params = new URLSearchParams(window.location.search);
    const keyword = params.get("q") || "";
    const mode = params.get("mode") || "basic";
    const [categories, books] = await Promise.all([
      fetch(`${this.apiUrl}/books/categories`).then((res) =>
        res.ok ? res.json() : [],
      ),
      fetch(
        keyword
          ? `${this.apiUrl}/books/search?q=${encodeURIComponent(keyword)}&mode=${encodeURIComponent(mode)}`
          : `${this.apiUrl}/books`,
      ).then((res) => (res.ok ? res.json() : [])),
    ]);
    this.allBooks = books;
    const categoryContainer = document.getElementById("category-filter-list");
    if (categoryContainer) {
      categoryContainer.innerHTML =
        '<li><label class="filter-item"><input type="checkbox" name="category" value="all" checked> <span>Tất cả thể loại</span></label></li>' +
        categories
          .map(
            (cat) => `
          <li>
            <label class="filter-item">
              <input type="checkbox" name="category" value="${cat.id}"> <span>${cat.name}</span>
            </label>
          </li>
        `,
          )
          .join("");
    }

    const renderFiltered = () => {
      const checked = Array.from(
        document.querySelectorAll('input[name="category"]:checked'),
      ).map((input) => input.value);
      const sort = document.getElementById("sort-books")?.value || "newest";
      let filtered =
        checked.includes("all") || checked.length === 0
          ? [...this.allBooks]
          : this.allBooks.filter((book) =>
              (book.categoryIds || []).some((id) => checked.includes(String(id))),
            );

      filtered.sort((a, b) => {
        if (sort === "az") return a.title.localeCompare(b.title, "vi");
        if (sort === "trending") return (b.borrowCount || 0) - (a.borrowCount || 0);
        return (b.publishYear || 0) - (a.publishYear || 0);
      });

      this.renderBookGrid("all-books-grid", filtered);
      document.getElementById("results-count").innerText =
        `Hiển thị ${filtered.length} kết quả`;
    };

    document.querySelectorAll('input[name="category"]').forEach((input) => {
      input.addEventListener("change", () => {
        if (input.value === "all" && input.checked) {
          document
            .querySelectorAll('input[name="category"]:not([value="all"])')
            .forEach((item) => (item.checked = false));
        } else if (input.checked) {
          const all = document.querySelector('input[name="category"][value="all"]');
          if (all) all.checked = false;
        }
        renderFiltered();
      });
    });
    document.getElementById("sort-books")?.addEventListener("change", renderFiltered);
    renderFiltered();
  }

  async loadBookDetailPage() {
    const urlParams = new URLSearchParams(window.location.search);
    const bookId = urlParams.get("id") || "B001";
    const response = await fetch(`${this.apiUrl}/books/${bookId}`);
    if (!response.ok) {
      this.showToast("Không tìm thấy sách.", "danger");
      return;
    }
    const book = await response.json();
    const bookTitle = book.title;

    // Hiển thị thông tin lên trang chi tiết
    document.getElementById("detail-title").innerText = bookTitle;
    document.getElementById("detail-id").innerText = `Mã: ${bookId}`;
    document.getElementById("detail-img").src =
      book.img || "https://via.placeholder.com/400x600?text=Book+Cover";
    document.getElementById("detail-status").innerText = book.status || "";
    document.getElementById("detail-status").className =
      `book-tag ${book.statusClass || (book.stock > 0 ? "success" : "danger")}`;
    document.getElementById("detail-category").innerText =
      book.category || "Chưa phân loại";
    document.getElementById("detail-author").innerText =
      `Tác giả: ${book.author || "Chưa cập nhật"}`;
    document.getElementById("detail-desc").innerText =
      book.description || "Chưa có mô tả cho sách này.";
    document.getElementById("detail-pub").innerText =
      `${book.publisher || "Chưa cập nhật"}${book.publishYear ? ` - ${book.publishYear}` : ""}`;

    // Xử lý Modal
    const modal = document.getElementById("borrow-modal");
    const openModalBtn = document.getElementById("borrow-btn");
    const closeModalBtn = document.getElementById("close-modal-btn");
    const cancelBtn = document.getElementById("cancel-borrow");
    const confirmBtn = document.getElementById("confirm-borrow");

    if (openModalBtn && modal) {
      openModalBtn.addEventListener("click", async () => {
        const meResponse = await fetch(`${this.apiUrl}/auth/me`).catch(() => null);
        const me = meResponse?.ok ? await meResponse.json() : { isAuthenticated: false };
        if (!me.isAuthenticated || me.userType !== "Reader") {
          this.showToast("Vui lòng đăng nhập để gửi yêu cầu mượn sách.", "danger");
          return;
        }
        // Tự động điền tên sách vào modal
        document.getElementById("modal-book-title").value = bookTitle;
        document.getElementById("modal-reader-id").value = me.userId || "";
        document.getElementById("modal-reader-name").value = me.userName || "";

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
      confirmBtn.addEventListener("click", async () => {
        const borrowDate = document.getElementById("modal-borrow-date").value;
        const dueDate = document.getElementById("modal-due-date").value;

        const response = await fetch(`${this.apiUrl}/borrow/request`, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            bookIds: [bookId],
            borrowDate,
            dueDate,
          }),
        });

        if (response.ok) {
          this.showToast(`Đã gửi yêu cầu mượn sách "${bookTitle}" thành công!`);
          closeModal();
        } else if (response.status === 401) {
          this.showToast("Vui lòng đăng nhập để gửi yêu cầu mượn sách.", "danger");
        } else {
          const error = await response.json().catch(() => ({}));
          this.showToast(error.message || "Không thể gửi yêu cầu mượn.", "danger");
        }
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
                <img src="${book.img || "https://via.placeholder.com/200x300?text=Cover"}" alt="${book.title}">
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
          window.location.href = `kho-sach.html?q=${encodeURIComponent(query)}&mode=${isAiMode ? "ai" : "basic"}`;
        }
      });
    }

    searchInput?.addEventListener("keydown", (event) => {
      if (event.key === "Enter") searchBtn?.click();
    });

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
