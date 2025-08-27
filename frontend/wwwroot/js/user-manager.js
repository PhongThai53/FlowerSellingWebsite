// User Management JavaScript Module
class UserManager {
  constructor() {
    this.apiBaseUrl = "https://localhost:7062/api";
    this.currentPage = 1;
    this.pageSize = 5;
    this.totalPages = 0;
    this.totalItems = 0;
    this.currentUserId = null;
    this.loadingTimeout = null;
    this.isEditMode = false;
    this.isCreateMode = false;
    this.userToDelete = null;
    this.availableRoles = [];
    this.currentUsers = [];

    this.init();
  }

  init() {
    // If AuthManager is not yet available, allow deferred initialization
    if (!window.authManager) {
      document.addEventListener("DOMContentLoaded", () => {
        // no-op; initializeUserList will be called from the page script
      });
      return;
    }
  }

  // Initialize User List Page
  initializeUserList() {
    console.log("UserList initialization debug:", {
      authManager: window.authManager,
      isAuthenticated: window.authManager?.isAuthenticated(),
      currentUser: window.authManager?.getCurrentUser(),
      token: window.authManager?.getAuthToken() ? "Present" : "Missing",
    });

    // Load roles first (before permission check) so dropdown is always populated
    this.loadAvailableRoles();

    // Check permission after loading roles
    if (!this.checkAdminPermission()) {
      return; // Stop initialization if not admin
    }

    this.showUserViewInfo();

    // Wire search input for new UI if present
    const newSearchInput = document.getElementById("exampleInputSearch");
    if (newSearchInput) {
      const debouncedSearch = this.debounce(() => {
        this.currentPage = 1;
        // Silent reload on typing to avoid showing the loading overlay
        this.loadUsers(false);
      }, 300);
      newSearchInput.addEventListener("input", debouncedSearch);
    }

    // Wire filter and sort dropdowns for auto-submit
    const roleFilter = document.getElementById("role-filter");
    if (roleFilter) {
      roleFilter.addEventListener("change", () => {
        this.currentPage = 1;
        this.loadUsers();
      });
    }

    const sortBy = document.getElementById("sort-by");
    if (sortBy) {
      sortBy.addEventListener("change", () => {
        this.currentPage = 1;
        this.loadUsers();
      });
    }

    const sortDirection = document.getElementById("sort-direction");
    if (sortDirection) {
      sortDirection.addEventListener("change", () => {
        this.currentPage = 1;
        this.loadUsers();
      });
    }

    // Prepare pagination container for new UI
    const paginationContainer = document.querySelector("ul.pagination");
    if (paginationContainer) {
      // Nothing to bind now; we will rebuild on each update
    }
    this.loadUsers();
  }

  // Public method to manually refresh role dropdowns
  refreshRoleDropdowns() {
    console.log("Manually refreshing role dropdowns...");
    if (this.availableRoles && this.availableRoles.length > 0) {
      this.populateRoleDropdowns();
    } else {
      console.warn("No roles available. Try loading roles first.");
      this.loadAvailableRoles();
    }
  }

  // Initialize User Detail Page
  initializeUserDetail() {
    console.log("UserDetail initialization debug:", {
      authManager: window.authManager,
      isAuthenticated: window.authManager?.isAuthenticated(),
      currentUser: window.authManager?.getCurrentUser(),
      token: window.authManager?.getAuthToken() ? "Present" : "Missing",
    });

    // Load roles first (before permission check) so dropdown is always populated
    this.loadAvailableRoles();

    // Check permission after loading roles
    if (!this.checkAdminPermission()) {
      return; // Stop initialization if not admin
    }

    const urlParams = new URLSearchParams(window.location.search);
    const userId = urlParams.get("id");

    if (userId) {
      this.currentUserId = userId;
      this.loadUserDetail(userId);
    } else {
      // Create mode
      this.enterCreateMode();
    }
  }

  // Check if user has admin permission
  checkAdminPermission() {
    // Check if user is authenticated
    if (!window.authManager?.isAuthenticated()) {
      this.showAccessDenied(
        "Bạn cần đăng nhập để truy cập trang này.",
        "Chưa đăng nhập"
      );
      setTimeout(() => {
        window.location.href = "/html/auth/login-register.html";
      }, 3000);
      return false;
    }

    // Check if user is admin
    const currentUser = window.authManager.getCurrentUser();
    if (!currentUser || currentUser.roleName !== "Admin") {
      this.showAccessDenied(
        "Bạn không có quyền truy cập trang quản lý người dùng. Chỉ có Admin mới có thể truy cập.",
        `Vai trò hiện tại: ${currentUser?.roleName || "Không xác định"}`
      );
      setTimeout(() => {
        window.location.href = "/html/common/homepage.html";
      }, 3000);
      return false;
    }

    return true;
  }

  // Show access denied message
  showAccessDenied(message, subtitle) {
    const container = document.querySelector(".container") || document.body;
    container.innerHTML = `
            <div class="access-denied-container" style="text-align: center; padding: 60px 20px; min-height: 60vh;">
                <div class="access-denied-content" style="max-width: 600px; margin: 0 auto;">
                    <i class="fa fa-shield-alt" style="font-size: 72px; color: #dc3545; margin-bottom: 30px;"></i>
                    <h1 style="color: #dc3545; margin-bottom: 20px;">Truy cập bị từ chối</h1>
                    <h3 style="color: #6c757d; margin-bottom: 30px;">${subtitle}</h3>
                    <div class="alert alert-danger" style="margin-bottom: 30px;">
                        <p style="margin: 0; font-size: 16px;">${message}</p>
                    </div>
                    <div class="countdown" style="margin-bottom: 30px;">
                        <p style="color: #6c757d;">Bạn sẽ được chuyển hướng trong <span id="countdown">3</span> giây...</p>
                    </div>
                    <div class="action-buttons">
                        <a href="/html/common/homepage.html" class="btn btn-primary" style="margin-right: 10px;">
                            <i class="fa fa-home"></i> Về trang chủ
                        </a>
                        <a href="/html/auth/login-register.html" class="btn btn-secondary">
                            <i class="fa fa-sign-in-alt"></i> Đăng nhập
                        </a>
                    </div>
                </div>
            </div>
        `;

    // Countdown timer
    let count = 3;
    const countdownElement = document.getElementById("countdown");
    const timer = setInterval(() => {
      count--;
      if (countdownElement) {
        countdownElement.textContent = count;
      }
      if (count <= 0) {
        clearInterval(timer);
      }
    }, 1000);
  }

  // Show info about current view mode
  showUserViewInfo() {
    const currentUser = window.authManager?.getCurrentUser();
    const isAdmin = currentUser?.roleName === "Admin";
    const infoContainer = document.getElementById("view-info-container");

    console.log("showUserViewInfo Debug:", {
      currentUser: currentUser,
      roleName: currentUser?.roleName,
      isAdmin: isAdmin,
    });

    if (infoContainer && isAdmin) {
      infoContainer.innerHTML = `
                <div class="alert alert-info mb-3">
                    <i class="fa fa-shield-alt"></i> <strong>Admin View:</strong> Bạn có thể xem và quản lý tất cả người dùng trong hệ thống.
                    <br><small>Vai trò: ${currentUser?.roleName}</small>
                </div>
            `;
    }
  }

  // Load available roles for filters and form
  async loadAvailableRoles() {
    try {
      console.log("Loading roles from API...");

      // Fetch roles from API
      const response = await this.makeAuthenticatedRequest(
        `${this.apiBaseUrl}/user/roles`
      );

      if (response && response.ok) {
        const raw = await response.json();
        // Support common envelope shapes: { data: [...] }, { items: [...] }, { result: [...] }
        const roles = raw?.data || raw?.items || raw?.result || raw;
        console.log("Roles loaded from API:", roles);

        // Transform API data to our format (robust mapping for varying shapes)
        this.availableRoles = (Array.isArray(roles) ? roles : [])
          .map((role) => {
            const roleName =
              role.roleName ||
              role.RoleName ||
              role.name ||
              role.role ||
              role.value ||
              "";
            const id =
              role.id || role.Id || role.roleId || role.roleID || role.ID;
            const description =
              role.description || role.Description || role.desc || "";
            return {
              value: roleName,
              label: this.getRoleDisplayName(roleName),
              id,
              description,
            };
          })
          .filter((r) => !!r.value);
      } else {
        console.warn("Failed to load roles from API, using fallback");
        // Fallback to static roles if API fails
        this.availableRoles = [
          { value: "Admin", label: "Quản trị viên" },
          { value: "Users", label: "Người dùng" },
          { value: "Staff", label: "Nhân viên" },
          { value: "Supplier", label: "Nhà cung cấp" },
        ];
      }

      // Do not force populate here; specific pages/modals will call when needed
      // this.populateRoleDropdowns();

      console.log("Available roles populated:", this.availableRoles);
    } catch (error) {
      console.error("Error loading roles:", error);
      // Fallback to static roles on error
      this.availableRoles = [
        { value: "Admin", label: "Quản trị viên" },
        { value: "Users", label: "Người dùng" },
        { value: "Staff", label: "Nhân viên" },
        { value: "Supplier", label: "Nhà cung cấp" },
      ];

      // Still populate the dropdowns with fallback data
      const roleFilter = document.getElementById("role-filter");
      if (roleFilter) {
        roleFilter.innerHTML = '<option value="">Tất cả vai trò</option>';
        this.availableRoles.forEach((role) => {
          roleFilter.innerHTML += `<option value="${role.value}">${role.label}</option>`;
        });
      }

      const roleSelect = document.getElementById("roleName");
      if (roleSelect) {
        roleSelect.innerHTML = '<option value="">Chọn vai trò</option>';
        this.availableRoles.forEach((role) => {
          roleSelect.innerHTML += `<option value="${role.value}">${role.label}</option>`;
        });
      }
    }
  }

  // Get display name for role
  getRoleDisplayName(roleName) {
    const roleDisplayNames = {
      Admin: "Quản trị viên",
      User: "Người dùng",
      Users: "Người dùng",
      Staff: "Nhân viên",
      Supplier: "Nhà cung cấp",
      Moderator: "Kiểm duyệt viên",
      Guest: "Khách",
    };
    return roleDisplayNames[roleName] || roleName;
  }

  // Populate role dropdowns with retry mechanism
  populateRoleDropdowns(retryCount = 0) {
    const maxRetries = 5;
    const retryDelay = 100; // 100ms delay between retries

    // Check if roles are available
    if (!this.availableRoles || this.availableRoles.length === 0) {
      console.warn("No roles available to populate dropdowns");
      return;
    }

    let populated = false;

    // Populate role select in add user modal (main target for this page)
    const addRoleSelect = document.getElementById("add-roleName");
    if (addRoleSelect) {
      console.log("Populating add-roleName dropdown...");
      addRoleSelect.innerHTML = '<option value="">Chọn vai trò</option>';
      this.availableRoles.forEach((role) => {
        addRoleSelect.innerHTML += `<option value="${role.value}">${role.label}</option>`;
      });
      console.log(
        "Add role select populated with",
        this.availableRoles.length,
        "roles"
      );
      populated = true;
    } else {
      console.log("add-roleName element not found yet, will retry...");
    }

    // Try to populate role select in detail form (for detail page)
    const roleSelect = document.getElementById("roleName");
    if (roleSelect) {
      console.log("Populating roleName dropdown...");
      roleSelect.innerHTML = '<option value="">Chọn vai trò</option>';
      this.availableRoles.forEach((role) => {
        roleSelect.innerHTML += `<option value="${role.value}">${role.label}</option>`;
      });
      console.log(
        "Role select populated with",
        this.availableRoles.length,
        "roles"
      );
      populated = true;
    }

    // Try to populate ALL role filter elements (in case there are duplicates)
    const roleFilters = document.querySelectorAll(
      '#role-filter, select[id="role-filter"]'
    );
    if (roleFilters.length > 0) {
      roleFilters.forEach((roleFilter, index) => {
        roleFilter.innerHTML = '<option value="">Tất cả vai trò</option>';
        this.availableRoles.forEach((role) => {
          const option = document.createElement("option");
          option.value = role.value;
          option.textContent = role.label;
          roleFilter.appendChild(option);
        });
      });
      console.log(
        "Role filter dropdowns populated with",
        this.availableRoles.length,
        "roles"
      );
      populated = true;
    }

    // If no elements found and we haven't exceeded retry limit, try again
    if (!populated && retryCount < maxRetries) {
      console.log(
        `Dropdown elements not ready, retrying in ${retryDelay}ms... (attempt ${
          retryCount + 1
        }/${maxRetries})`
      );
      setTimeout(() => {
        this.populateRoleDropdowns(retryCount + 1);
      }, retryDelay);
    } else if (!populated) {
      console.log("No role dropdowns found to populate on this page");
    } else {
      console.log("Role dropdowns populated successfully");
    }
  }

  // Specific function to populate modal role dropdown
  populateModalRoleDropdown() {
    console.log("populateModalRoleDropdown() called");
    console.log("Available roles:", this.availableRoles);

    if (!this.availableRoles || this.availableRoles.length === 0) {
      console.warn("No roles available to populate modal dropdown");
      return false;
    }

    const addRoleSelect = document.getElementById("add-roleName");
    console.log("Looking for element with ID 'add-roleName':", addRoleSelect);

    if (addRoleSelect) {
      console.log(
        "Populating modal role dropdown with",
        this.availableRoles.length,
        "roles"
      );
      console.log("Current dropdown HTML before:", addRoleSelect.innerHTML);
      console.log("Dropdown element properties:", {
        disabled: addRoleSelect.disabled,
        readonly: addRoleSelect.readOnly,
        style: addRoleSelect.style.cssText,
        computedStyle: window.getComputedStyle(addRoleSelect),
      });

      // Clear existing options first
      addRoleSelect.innerHTML = "";

      // Add default option
      const defaultOption = document.createElement("option");
      defaultOption.value = "";
      defaultOption.textContent = "Chọn vai trò";
      addRoleSelect.appendChild(defaultOption);

      // Add role options
      this.availableRoles.forEach((role) => {
        const option = document.createElement("option");
        option.value = role.value;
        option.textContent = role.label;
        addRoleSelect.appendChild(option);
      });

      console.log("Current dropdown HTML after:", addRoleSelect.innerHTML);
      console.log(
        "Number of options after population:",
        addRoleSelect.options.length
      );
      console.log("Modal role dropdown populated successfully");

      // Force a re-render
      addRoleSelect.style.display = "none";
      addRoleSelect.offsetHeight; // Trigger reflow
      addRoleSelect.style.display = "";

      return true;
    } else {
      console.error("Modal role dropdown element not found");
      console.log(
        "All elements with 'add-roleName' in their ID:",
        document.querySelectorAll('[id*="add-roleName"]')
      );
      return false;
    }
  }

  // Debug function to check dropdown state
  debugDropdown() {
    const addRoleSelect = document.getElementById("add-roleName");
    if (addRoleSelect) {
      console.log("=== DROPDOWN DEBUG INFO ===");
      console.log("Element found:", addRoleSelect);
      console.log("Element ID:", addRoleSelect.id);
      console.log("Element tagName:", addRoleSelect.tagName);
      console.log("Element className:", addRoleSelect.className);
      console.log("Element disabled:", addRoleSelect.disabled);
      console.log("Element readonly:", addRoleSelect.readOnly);
      console.log("Element style:", addRoleSelect.style.cssText);
      console.log(
        "Element computed style:",
        window.getComputedStyle(addRoleSelect)
      );
      console.log("Element offsetHeight:", addRoleSelect.offsetHeight);
      console.log("Element offsetWidth:", addRoleSelect.offsetWidth);
      console.log("Element innerHTML:", addRoleSelect.innerHTML);
      console.log("Element options length:", addRoleSelect.options.length);
      console.log("Element selectedIndex:", addRoleSelect.selectedIndex);
      console.log("Element value:", addRoleSelect.value);

      // Check if element is visible
      const rect = addRoleSelect.getBoundingClientRect();
      console.log("Element bounding rect:", rect);
      console.log("Element is visible:", rect.width > 0 && rect.height > 0);

      // Check parent elements
      let parent = addRoleSelect.parentElement;
      let level = 1;
      while (parent && level <= 5) {
        console.log(`Parent level ${level}:`, {
          tagName: parent.tagName,
          id: parent.id,
          className: parent.className,
          style: parent.style.cssText,
          display: window.getComputedStyle(parent).display,
          visibility: window.getComputedStyle(parent).visibility,
          zIndex: window.getComputedStyle(parent).zIndex,
        });
        parent = parent.parentElement;
        level++;
      }

      console.log("=== END DEBUG INFO ===");
    } else {
      console.error("Dropdown element not found for debugging");
    }
  }

  // Load users with filters and pagination
  async loadUsers(showOverlay = true) {
    if (showOverlay) {
      this.showLoading();
    }

    try {
      const filters = this.getFilters();
      const queryParams = this.buildQueryParams(filters);

      console.log("Loading users with filters:", filters);
      console.log("Query params:", queryParams);

      const response = await this.makeAuthenticatedRequest(
        `${this.apiBaseUrl}/user/list`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(queryParams),
        }
      );

      if (response && response.ok) {
        const result = await response.json();
        console.log("Users loaded:", result);

        this.totalPages = result.total_pages || result.totalPages || 1;
        this.totalItems =
          result.total_items ||
          result.totalItems ||
          result.total ||
          result.count ||
          (Array.isArray(result.items) ? result.items.length : 0);
        if (result.page_size || result.pageSize) {
          this.pageSize = result.page_size || result.pageSize;
        }
        this.currentUsers = result.items || [];
        this.renderUsers(this.currentUsers);
        this.updatePaginationControls();
      } else {
        console.error(
          "Failed to load users:",
          response?.status,
          response?.statusText
        );
        this.showError("Không thể tải danh sách người dùng");
      }
    } catch (error) {
      console.error("Error loading users:", error);
      this.showError("Lỗi khi tải danh sách người dùng");
    } finally {
      if (showOverlay) {
        this.hideLoading();
      }
    }
  }

  // Build query parameters for API request
  buildQueryParams(filters) {
    const params = {
      page_index: this.currentPage,
      page_size: this.pageSize,
      sort_by: filters.sortBy || "fullname",
      sort_direction: filters.sortDirection === "Desc" ? 1 : 0, // Enum: Asc = 0, Desc = 1
    };

    // Add search parameters
    if (filters.search && filters.search.trim()) {
      // Support multiple search fields - search in fullname by default
      // but can be extended to search in multiple fields
      params.search_by = "fullname";
      params.search_value = filters.search.trim();
    }

    // Add filter parameters
    const filterParams = {};
    if (filters.role && filters.role.trim()) {
      filterParams.role = filters.role.trim();
    }

    if (Object.keys(filterParams).length > 0) {
      params.filers = filterParams; // Note: typo in backend DTO "filers" instead of "filters"
    }

    return params;
  }

  // Get current filter values
  getFilters() {
    return {
      // Support both old UI (#search-input) and new UI (#exampleInputSearch)
      search:
        document.getElementById("exampleInputSearch")?.value ||
        document.getElementById("search-input")?.value ||
        "",
      role: document.getElementById("role-filter")?.value || "",
      sortBy: document.getElementById("sort-by")?.value || "fullName",
      sortDirection: document.getElementById("sort-direction")?.value || "Desc",
    };
  }

  // Render users list
  renderUsers(users) {
    console.log("Rendering users:", users);

    // If new table-based UI exists, render into table body
    const tableBody = document.querySelector("#user-list-table tbody");
    if (tableBody) {
      console.log("Table body found:", tableBody);

      if (!users || users.length === 0) {
        tableBody.innerHTML = `
                    <tr>
                        <td colspan="8" class="text-center">Không tìm thấy người dùng</td>
                    </tr>
                `;
        return;
      }

      const rowsHtml = users
        .map((user) => this.getUserTableRowHTML(user))
        .join("");

      console.log("Generated HTML:", rowsHtml);
      tableBody.innerHTML = rowsHtml;

      // Debug: Check if content was actually inserted
      console.log("Table body after insert:", tableBody.innerHTML);
      return;
    }

    // Fallback to old card-based UI
    const container = document.getElementById("users-container");
    if (!container) return;

    if (users.length === 0) {
      container.innerHTML = this.getEmptyStateHTML();
      return;
    }

    const usersHTML = users.map((user) => this.getUserCardHTML(user)).join("");
    container.innerHTML = `
            <div class="row">
                ${usersHTML}
            </div>
        `;
  }

  // Generate HTML for a user row in the new table UI
  getUserTableRowHTML(user) {
    const statusClass = !user.isActive ? "bg-secondary" : "bg-success";
    const statusText = !user.isActive ? "Inactive" : "Active";

    // Determine action button based on user status
    const actionButton = !user.isActive
      ? `<button class="badge bg-primary" onclick="userManager.activateUser('${user.publicId}')" title="Activate User">
                <i class="las la-check-circle"></i> Activate
            </button>`
      : `<button class="badge bg-danger" onclick="userManager.deactivateUser('${user.publicId}')" title="Deactivate User">
                <i class="las la-ban"></i> Deactivate
            </button>`;

    return `
        <tr>
            <td>${user.fullName || "N/A"}</td>
            <td>${user.userName || "N/A"}</td>
            <td>${user.email || "N/A"}</td>
            <td>${user.phoneNumber || "N/A"}</td>
            <td>${user.roleName || "N/A"}</td>
            <td>${user.address || "N/A"}</td>
            <td><span class="badge ${statusClass}">${statusText}</span></td>
            <td>
                <div class="list-user-action">
                    <button class="badge bg-primary" onclick="userManager.openEditUserModal('${
                      user.publicId
                    }')" title="Edit User">
                        <i class="las la-edit"></i> Edit
                    </button>
                    ${actionButton}
                </div>
            </td>
        </tr>
    `;
  }

  // Generate HTML for a user card
  getUserCardHTML(user) {
    const initials = this.getInitials(user.fullName);
    const roleClass = this.getRoleClass(user.roleName);
    const createdDate = new Date(user.createdAt).toLocaleDateString("vi-VN");

    return `
            <div class="col-md-6 col-lg-4 mb-4">
                <div class="user-card">
                    <div class="user-content">
                        <div class="d-flex align-items-center mb-3">
                            <div class="user-avatar">
                                ${initials}
                            </div>
                            <div class="ml-3 flex-grow-1">
                                <h5 class="mb-1">${user.fullName}</h5>
                                <span class="role-badge ${roleClass}">${
      user.roleName
    }</span>
                            </div>
                        </div>
                        
                        <div class="user-info">
                            <p class="mb-2">
                                <i class="fa fa-envelope text-muted mr-2"></i>
                                ${user.email}
                            </p>
                            ${
                              user.phoneNumber
                                ? `
                                <p class="mb-2">
                                    <i class="fa fa-phone text-muted mr-2"></i>
                                    ${user.phoneNumber}
                                </p>
                            `
                                : ""
                            }

                            <p class="mb-3 text-muted small">
                                <i class="fa fa-calendar text-muted mr-2"></i>
                                Tạo: ${createdDate}
                            </p>
                        </div>

                        <div class="user-actions">
                            <button class="btn-action btn-view" onclick="userManager.viewUser('${
                              user.publicId
                            }')" title="Xem chi tiết">
                                <i class="fa fa-eye"></i>
                            </button>
                            <button class="btn-action btn-edit" onclick="userManager.editUser('${
                              user.publicId
                            }')" title="Chỉnh sửa">
                                <i class="fa fa-edit"></i>
                            </button>
                            <button class="btn-action btn-delete" onclick="userManager.openDeleteModal('${
                              user.publicId
                            }', '${user.fullName}')" title="Xóa">
                                <i class="fa fa-trash"></i>
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;
  }

  // Get empty state HTML
  getEmptyStateHTML() {
    return `
            <div class="empty-state">
                <i class="fa fa-users"></i>
                <h3>Không tìm thấy người dùng</h3>
                <p>Không có người dùng nào phù hợp với bộ lọc hiện tại.</p>
                <button class="btn btn-primary" onclick="userManager.clearFilters()">
                    <i class="fa fa-refresh"></i> Xóa bộ lọc
                </button>
            </div>
        `;
  }

  // Get user initials
  getInitials(fullName) {
    if (!fullName) return "U";
    const names = fullName.trim().split(" ");
    if (names.length === 1) return names[0][0].toUpperCase();
    return (names[0][0] + names[names.length - 1][0]).toUpperCase();
  }

  // Get role CSS class
  getRoleClass(roleName) {
    switch (roleName?.toLowerCase()) {
      case "admin":
        return "role-admin";
      case "user":
        return "role-user";
      case "moderator":
        return "role-moderator";
      default:
        return "role-user";
    }
  }

  // Load user detail
  async loadUserDetail(userId) {
    this.showLoading();

    try {
      const response = await this.makeAuthenticatedRequest(
        `${this.apiBaseUrl}/user/${userId}`
      );

      if (response && response.ok) {
        const user = await response.json();
        console.log("User detail loaded:", user);
        this.populateUserForm(user);
        this.updateBreadcrumb(user.fullName);
      } else {
        console.error(
          "Failed to load user detail:",
          response?.status,
          response?.statusText
        );
        this.showError("Không thể tải thông tin người dùng");
      }
    } catch (error) {
      console.error("Error loading user detail:", error);
      this.showError("Lỗi khi tải thông tin người dùng");
    } finally {
      this.hideLoading();
    }
  }

  // Populate user form with data
  populateUserForm(user) {
    // Basic information
    document.getElementById("fullName").value = user.fullName || "";
    document.getElementById("email").value = user.email || "";
    document.getElementById("phoneNumber").value = user.phoneNumber || "";
    document.getElementById("address").value = user.address || "";
    document.getElementById("roleName").value = user.roleName || "";

    // Additional information
    if (user.createdAt) {
      const createdDate = new Date(user.createdAt).toLocaleString("vi-VN");
      document.getElementById("createdAt").value = createdDate;
    }

    // Status (default to Active if not provided)
    document.getElementById("status").value = user.status || "Active";

    // User IDs
    document.getElementById("publicId").value = user.publicId || "";
    document.getElementById("id").value = user.id || "";

    // Update avatar
    const avatar = document.getElementById("user-avatar");
    if (avatar) {
      avatar.innerHTML = this.getInitials(user.fullName);
    }

    // Update card title and breadcrumb
    document.getElementById(
      "card-title"
    ).textContent = `Chi tiết: ${user.fullName}`;
    this.updateBreadcrumb(user.fullName);
  }

  // Update breadcrumb
  updateBreadcrumb(userName) {
    const breadcrumb = document.getElementById("breadcrumb-current");
    if (breadcrumb) {
      breadcrumb.textContent = `Chi tiết: ${userName}`;
    }
  }

  // Enter create mode
  enterCreateMode() {
    this.isCreateMode = true;
    this.isEditMode = true;

    // Update UI for create mode
    document.getElementById("card-title").textContent = "Tạo người dùng mới";
    document.getElementById("breadcrumb-current").textContent =
      "Tạo người dùng mới";

    // Show password field
    document.getElementById("password-group").style.display = "block";

    // Show create mode buttons
    document.getElementById("view-mode-buttons").style.display = "none";
    document.getElementById("edit-mode-buttons").style.display = "none";
    document.getElementById("create-mode-buttons").style.display = "flex";

    this.toggleFormEditMode(true);
  }

  // Toggle edit mode
  toggleEditMode() {
    if (this.isCreateMode) return;

    this.isEditMode = !this.isEditMode;
    this.toggleFormEditMode(this.isEditMode);

    // Update buttons
    if (this.isEditMode) {
      document.getElementById("view-mode-buttons").style.display = "none";
      document.getElementById("edit-mode-buttons").style.display = "flex";
    } else {
      document.getElementById("view-mode-buttons").style.display = "flex";
      document.getElementById("edit-mode-buttons").style.display = "none";
    }
  }

  // Toggle form edit mode
  toggleFormEditMode(isEdit) {
    const form = document.getElementById("user-form");
    const inputs = form.querySelectorAll("input, select, textarea");

    if (isEdit) {
      form.classList.remove("view-mode");
      form.classList.add("edit-mode");

      // Only allow editing of specific fields
      inputs.forEach((input) => {
        if (
          ["fullName", "phoneNumber", "address", "roleName"].includes(input.id)
        ) {
          input.removeAttribute("readonly");
          input.removeAttribute("disabled");
          input.classList.remove("form-control-readonly");
        }
      });
    } else {
      form.classList.remove("edit-mode");
      form.classList.add("view-mode");

      // Make all fields read-only
      inputs.forEach((input) => {
        if (input.tagName === "SELECT") {
          input.setAttribute("disabled", "");
        } else {
          input.setAttribute("readonly", "");
        }
        input.classList.add("form-control-readonly");
      });
    }
  }

  // Cancel edit
  cancelEdit() {
    if (this.isCreateMode) {
      window.location.href = "user-list.html";
      return;
    }

    this.isEditMode = false;
    this.toggleFormEditMode(false);

    // Reload user data
    if (this.currentUserId) {
      this.loadUserDetail(this.currentUserId);
    }

    // Update buttons
    document.getElementById("view-mode-buttons").style.display = "flex";
    document.getElementById("edit-mode-buttons").style.display = "none";
  }

  // Save user (update)
  async saveUser() {
    if (!this.validateForm()) return;

    this.showLoading();

    try {
      const formData = this.getFormData();

      // Only send editable fields
      const updateData = {
        fullName: formData.fullName,
        phoneNumber: formData.phoneNumber,
        address: formData.address,
        roleName: formData.roleName,
      };

      console.log("Updating user with data:", updateData);

      const response = await this.makeAuthenticatedRequest(
        `${this.apiBaseUrl}/user/${this.currentUserId}`,
        {
          method: "PATCH",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(updateData),
        }
      );

      if (response && response.ok) {
        this.showSuccess("Cập nhật người dùng thành công");
        this.toggleEditMode(); // Exit edit mode
        this.loadUserDetail(this.currentUserId); // Reload data
      } else {
        const errorData = await response.json().catch(() => null);
        const errorMessage =
          errorData?.message || "Không thể cập nhật người dùng";
        this.showError(errorMessage);
      }
    } catch (error) {
      console.error("Error saving user:", error);
      this.showError("Lỗi khi lưu thông tin người dùng");
    } finally {
      this.hideLoading();
    }
  }

  // Create user from modal
  async createUserFromModal() {
    if (!this.validateAddUserForm()) return;

    this.showLoading();

    try {
      const formData = this.getAddUserFormData();
      console.log("Creating user with data:", formData);

      const response = await this.makeAuthenticatedRequest(
        `${this.apiBaseUrl}/user`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(formData),
        }
      );

      if (response && response.ok) {
        this.showSuccess("Tạo người dùng thành công");

        // Close modal using jQuery
        const modal = document.getElementById("addUserModal");
        if (modal) {
          $(modal).modal("hide");
        }

        // Reload page after a short delay to show the success message
        setTimeout(() => {
          window.location.reload();
        }, 1500);
      } else {
        const errorData = await response.json().catch(() => null);
        const errorMessage = errorData?.message || "Không thể tạo người dùng";
        this.showError(errorMessage);
      }
    } catch (error) {
      console.error("Error creating user:", error);
      this.showError("Lỗi khi tạo người dùng");
    } finally {
      this.hideLoading();
    }
  }

  // Create user (legacy method)
  async createUser() {
    if (!this.validateForm()) return;

    this.showLoading();

    try {
      const formData = this.getFormData();

      const response = await this.makeAuthenticatedRequest(
        `${this.apiBaseUrl}/user`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(formData),
        }
      );

      if (response && response.ok) {
        this.showSuccess("Tạo người dùng thành công");
        setTimeout(() => {
          window.location.href = "user-list.html";
        }, 1500);
      } else {
        const errorData = await response.json().catch(() => null);
        const errorMessage = errorData?.message || "Không thể tạo người dùng";
        this.showError(errorMessage);
      }
    } catch (error) {
      console.error("Error creating user:", error);
      this.showError("Lỗi khi tạo người dùng");
    } finally {
      this.hideLoading();
    }
  }

  // Get add user form data
  getAddUserFormData() {
    const formData = {};
    const form = document.getElementById("add-user-form");
    if (!form) return formData;

    const inputs = form.querySelectorAll("input, select, textarea");

    inputs.forEach((input) => {
      if (input.name && input.value.trim()) {
        if (input.type === "checkbox") {
          formData[input.name] = input.checked;
        } else {
          formData[input.name] = input.value.trim();
        }
      }
    });

    // Ensure required fields are always included (even if empty)
    const requiredFields = [
      "fullName",
      "username",
      "email",
      "password",
      "roleName",
    ];
    requiredFields.forEach((field) => {
      if (!formData[field]) {
        formData[field] = "";
      }
    });

    return formData;
  }

  // Get form data
  getFormData() {
    const formData = {};
    const form = document.getElementById("user-form");
    const inputs = form.querySelectorAll("input, select, textarea");

    // Only get data from editable fields
    inputs.forEach((input) => {
      if (
        input.name &&
        ["fullName", "phoneNumber", "address", "roleName"].includes(input.id)
      ) {
        if (input.type === "checkbox") {
          formData[input.name] = input.checked;
        } else {
          formData[input.name] = input.value.trim();
        }
      }
    });

    return formData;
  }

  // Validate add user form
  validateAddUserForm() {
    const form = document.getElementById("add-user-form");
    if (!form) return false;

    const requiredFields = [
      { id: "add-fullName", name: "Họ và tên" },
      { id: "add-username", name: "Username" },
      { id: "add-email", name: "Email" },
      { id: "add-password", name: "Mật khẩu" },
      { id: "add-roleName", name: "Vai trò" },
    ];

    let isValid = true;

    requiredFields.forEach((field) => {
      const input = document.getElementById(field.id);
      if (input) {
        this.clearValidation(input);

        if (!input.value.trim()) {
          this.showValidationError(input, `${field.name} là bắt buộc`);
          isValid = false;
        } else if (
          field.id === "add-username" &&
          !this.isValidUsername(input.value.trim())
        ) {
          this.showValidationError(
            input,
            "Tên đăng nhập chỉ được chứa chữ cái, số và dấu gạch dưới"
          );
          isValid = false;
        } else if (
          field.id === "add-email" &&
          !this.isValidEmail(input.value.trim())
        ) {
          this.showValidationError(input, "Email không hợp lệ");
          isValid = false;
        } else if (
          field.id === "add-password" &&
          input.value.trim().length < 6
        ) {
          this.showValidationError(input, "Mật khẩu phải có ít nhất 6 ký tự");
          isValid = false;
        }
      }
    });

    // Validate phone number if provided
    const phoneInput = document.getElementById("add-phoneNumber");
    if (
      phoneInput &&
      phoneInput.value.trim() &&
      !this.isValidPhone(phoneInput.value.trim())
    ) {
      this.showValidationError(phoneInput, "Số điện thoại không hợp lệ");
      isValid = false;
    }

    return isValid;
  }

  // Validate form
  validateForm() {
    const form = document.getElementById("user-form");
    // Only validate editable fields
    const editableFields = ["fullName", "phoneNumber", "address", "roleName"];
    let isValid = true;

    editableFields.forEach((fieldId) => {
      const input = document.getElementById(fieldId);
      if (input) {
        this.clearValidation(input);

        if (fieldId === "fullName" && !input.value.trim()) {
          this.showValidationError(input, "Họ và tên là bắt buộc");
          isValid = false;
        } else if (
          fieldId === "phoneNumber" &&
          input.value.trim() &&
          !this.isValidPhone(input.value.trim())
        ) {
          this.showValidationError(input, "Số điện thoại không hợp lệ");
          isValid = false;
        } else if (fieldId === "roleName" && !input.value.trim()) {
          this.showValidationError(input, "Vai trò là bắt buộc");
          isValid = false;
        }
      }
    });

    return isValid;
  }

  // Show validation error
  showValidationError(input, message) {
    input.classList.add("is-invalid");
    const feedback = input.parentElement.querySelector(".invalid-feedback");
    if (feedback) {
      feedback.textContent = message;
    }
  }

  // Clear validation
  clearValidation(input) {
    input.classList.remove("is-invalid", "is-valid");
    const feedback = input.parentElement.querySelector(".invalid-feedback");
    if (feedback) {
      feedback.textContent = "";
    }
  }

  // Validate email
  isValidEmail(email) {
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(email);
  }

  // Validate username
  isValidUsername(username) {
    // Username: 3-20 characters, alphanumeric and underscore only
    const re = /^[a-zA-Z0-9_]{3,20}$/;
    return re.test(username);
  }

  // Validate phone number
  isValidPhone(phone) {
    // Allow Vietnamese phone numbers: +84, 84, 0 followed by 9-10 digits
    const re = /^(\+84|84|0)?[0-9]{9,10}$/;
    return re.test(phone);
  }

  // Delete user
  async deleteUser() {
    if (!this.currentUserId) return;

    this.showLoading();

    try {
      const response = await this.makeAuthenticatedRequest(
        `${this.apiBaseUrl}/user/${this.currentUserId}`,
        { method: "DELETE" }
      );

      if (response && response.ok) {
        this.showSuccess("Xóa người dùng thành công");
        setTimeout(() => {
          window.location.href = "user-list.html";
        }, 1500);
      } else {
        this.showError("Không thể xóa người dùng");
      }
    } catch (error) {
      console.error("Error deleting user:", error);
      this.showError("Lỗi khi xóa người dùng");
    } finally {
      this.hideLoading();
    }
  }

  // Navigation methods
  viewUser(userId) {
    window.location.href = `user-detail.html?id=${userId}`;
  }

  editUser(userId) {
    window.location.href = `user-detail.html?id=${userId}&edit=true`;
  }

  openCreateUserModal() {
    window.location.href = "user-detail.html";
  }

  // Add User Modal methods
  async openAddUserModal() {
    const modal = document.getElementById("addUserModal");
    if (modal) {
      // Ensure roles are available before showing the modal
      if (!this.availableRoles || this.availableRoles.length === 0) {
        try {
          await this.loadAvailableRoles();
        } catch (error) {
          console.error("Error loading roles for modal:", error);
          this.showError("Không thể tải danh sách vai trò.");
          return; // Abort if roles can't be loaded
        }
      }

      // Reset form first
      this.resetAddUserForm();

      // Show modal using Bootstrap 4/jQuery
      $(modal).modal("show");

      // Populate the role dropdown AFTER the modal is shown
      // This ensures the element is accessible in the DOM
      setTimeout(() => {
        if (!this.populateModalRoleDropdown()) {
          // If direct population fails, try the general method with retry
          this.populateRoleDropdowns();
        }
      }, 100);
    }
  }

  resetAddUserForm() {
    const form = document.getElementById("add-user-form");
    if (form) {
      form.reset();
      // Clear validation states
      const inputs = form.querySelectorAll("input, select, textarea");
      inputs.forEach((input) => {
        input.classList.remove("is-invalid", "is-valid");
        const feedback = input.parentElement.querySelector(".invalid-feedback");
        if (feedback) feedback.textContent = "";
      });
    }
  }

  // Delete modal methods
  openDeleteModal(userId, userName) {
    this.currentDeleteUserId = userId;
    this.currentDeleteUserName = userName;

    // Update modal content based on user status
    const user = this.currentUsers.find((u) => u.publicId === userId);
    if (user && !user.isActive) {
      // User is inactive, show activate confirmation
      document.getElementById("deleteModalTitle").textContent =
        "Xác nhận kích hoạt";
      document.getElementById("deleteModalBody").innerHTML = `
            <p>Bạn có chắc chắn muốn kích hoạt người dùng <strong>${userName}</strong> không?</p>
            <p>Người dùng này sẽ có thể đăng nhập và sử dụng hệ thống.</p>
        `;
      document.getElementById("confirmDeleteBtn").textContent = "Activate";
      document.getElementById("confirmDeleteBtn").className = "btn btn-success";
    } else {
      // User is active, show deactivate confirmation
      document.getElementById("deleteModalTitle").textContent =
        "Xác nhận vô hiệu hóa";
      document.getElementById("deleteModalBody").innerHTML = `
            <p>Bạn có chắc chắn muốn vô hiệu hóa người dùng <strong>${userName}</strong> không?</p>
            <p>Người dùng này sẽ không thể đăng nhập vào hệ thống.</p>
        `;
      document.getElementById("confirmDeleteBtn").textContent = "Deactivate";
      document.getElementById("confirmDeleteBtn").className = "btn btn-warning";
    }

    $("#deleteUserModal").modal("show");
  }

  async activateUser(userId) {
    try {
      this.showLoading();

      const response = await fetch(
        `${this.apiBaseUrl}/user/${userId}/activate`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${localStorage.getItem("auth_token")}`,
          },
        }
      );

      if (response.ok) {
        this.showSuccess("User activated successfully");
        await this.loadUsers(false);
      } else {
        const errorData = await response.json();
        this.showError(errorData.message || "Failed to activate user");
      }
    } catch (error) {
      console.error("Error activating user:", error);
      this.showError("An error occurred while activating the user");
    } finally {
      this.hideLoading();
    }
  }

  async deactivateUser(userId) {
    try {
      this.showLoading();

      const response = await fetch(
        `${this.apiBaseUrl}/user/${userId}/deactivate`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${localStorage.getItem("auth_token")}`,
          },
        }
      );

      if (response.ok) {
        this.showSuccess("User deactivated successfully");
        await this.loadUsers(false);
      } else {
        const errorData = await response.json();
        this.showError(errorData.message || "Failed to deactivate user");
      }
    } catch (error) {
      console.error("Error deactivating user:", error);
      this.showError("An error occurred while deactivating the user");
    } finally {
      this.hideLoading();
    }
  }

  closeDeleteModal() {
    this.currentDeleteUserId = null;
    this.currentDeleteUserName = null;
    $("#deleteUserModal").modal("hide");
  }

  async confirmDelete() {
    if (!this.currentDeleteUserId) {
      console.error("No user to delete");
      return;
    }

    const user = this.currentUsers.find(
      (u) => u.publicId === this.currentDeleteUserId
    );
    if (!user) {
      console.error("User not found");
      return;
    }

    try {
      if (!user.isActive) {
        // User is inactive, activate them
        await this.activateUser(this.currentDeleteUserId);
      } else {
        // User is active, deactivate them
        await this.deactivateUser(this.currentDeleteUserId);
      }
    } catch (error) {
      console.error("Error in confirmDelete:", error);
    }
  }

  // Search and filter methods
  searchUsers() {
    this.currentPage = 1;
    this.loadUsers();
  }

  applyFilters() {
    this.currentPage = 1;
    this.loadUsers();
  }

  clearFilters() {
    const oldSearch = document.getElementById("search-input");
    if (oldSearch) oldSearch.value = "";
    const newSearch = document.getElementById("exampleInputSearch");
    if (newSearch) newSearch.value = "";

    const roleFilter = document.getElementById("role-filter");
    if (roleFilter) roleFilter.value = "";

    const sortBy = document.getElementById("sort-by");
    if (sortBy) sortBy.value = "fullName";

    const sortDirection = document.getElementById("sort-direction");
    if (sortDirection) sortDirection.value = "Desc";

    this.currentPage = 1;
    this.loadUsers();
  }

  // Pagination methods
  previousPage() {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadUsers();
    }
  }

  nextPage() {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadUsers();
    }
  }

  updatePaginationControls() {
    // New UI: Update "Showing ..." and pagination list if present
    const showingInfoContainer = document.getElementById("user-list-page-info");
    if (showingInfoContainer) {
      const infoSpan =
        showingInfoContainer.querySelector("span") || showingInfoContainer;
      if (this.totalItems && this.totalItems > 0) {
        const start = (this.currentPage - 1) * this.pageSize + 1;
        const end = Math.min(this.currentPage * this.pageSize, this.totalItems);
        infoSpan.textContent = `Hiển thị ${start} đến ${end} trong tổng số ${this.totalItems} người dùng`;
      } else {
        infoSpan.textContent = `Trang ${this.currentPage} của ${this.totalPages}`;
      }
    }

    const paginationUl = document.querySelector("ul.pagination");
    if (paginationUl) {
      paginationUl.innerHTML = this.buildPaginationHTML(
        this.currentPage,
        this.totalPages
      );
    }

    // Old UI controls fallback
    const prevBtn = document.getElementById("prev-btn");
    const nextBtn = document.getElementById("next-btn");
    const paginationInfo = document.getElementById("pagination-info");
    if (prevBtn) prevBtn.disabled = this.currentPage <= 1;
    if (nextBtn) nextBtn.disabled = this.currentPage >= this.totalPages;
    if (paginationInfo)
      paginationInfo.textContent = `Trang ${this.currentPage} của ${this.totalPages}`;
  }

  buildPaginationHTML(currentPage, totalPages) {
    if (!totalPages || totalPages < 1) return "";

    const createPageItem = (page, isActive = false) => `
            <li class="page-item ${isActive ? "active" : ""}">
                <a class="page-link" href="javascript:void(0)" onclick="userManager.gotoPage(${page})">${page}</a>
            </li>
        `;

    const prevClass = currentPage <= 1 ? "disabled" : "";
    const nextClass = currentPage >= totalPages ? "disabled" : "";

    const pages = [];
    const maxPagesToShow = 5;
    let startPage = Math.max(1, currentPage - Math.floor(maxPagesToShow / 2));
    let endPage = Math.min(totalPages, startPage + maxPagesToShow - 1);
    if (endPage - startPage + 1 < maxPagesToShow) {
      startPage = Math.max(1, endPage - maxPagesToShow + 1);
    }
    for (let p = startPage; p <= endPage; p++) {
      pages.push(createPageItem(p, p === currentPage));
    }

    return `
            <li class="page-item ${prevClass}">
                <a class="page-link" href="javascript:void(0)" onclick="userManager.previousPage()">Trước</a>
            </li>
            ${pages.join("")}
            <li class="page-item ${nextClass}">
                <a class="page-link" href="javascript:void(0)" onclick="userManager.nextPage()">Tiếp</a>
            </li>
        `;
  }

  gotoPage(page) {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.loadUsers();
  }

  // Utils
  debounce(fn, wait) {
    let t;
    return (...args) => {
      clearTimeout(t);
      t = setTimeout(() => fn.apply(this, args), wait);
    };
  }

  // Utility methods
  togglePassword(fieldId) {
    const field = document.getElementById(fieldId);
    const button = field.parentElement.querySelector(".password-toggle i");

    if (field.type === "password") {
      field.type = "text";
      button.className = "fa fa-eye-slash";
    } else {
      field.type = "password";
      button.className = "fa fa-eye";
    }
  }

  // HTTP request methods
  async makeAuthenticatedRequest(url, options = {}) {
    const token = window.authManager?.getAuthToken();
    if (!token) {
      throw new Error("No authentication token available");
    }

    const defaultOptions = {
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
      },
    };

    const mergedOptions = {
      ...defaultOptions,
      ...options,
      headers: {
        ...defaultOptions.headers,
        ...options.headers,
      },
    };

    return fetch(url, mergedOptions);
  }

  // UI feedback methods
  showLoading() {
    const overlay = document.getElementById("loading-overlay");
    if (overlay) {
      overlay.style.display = "flex";
    }
  }

  hideLoading() {
    const overlay = document.getElementById("loading-overlay");
    if (overlay) {
      overlay.style.display = "none";
    }
  }

  showAlert(message, type = "info") {
    const container = document.getElementById("alert-container");
    if (!container) {
      // Fallback if alert container doesn't exist in current UI
      try {
        window.alert(message);
      } catch (_) {}
      return;
    }

    const alertHTML = `
            <div class="alert alert-${type} alert-dismissible">
                ${message}
                <button type="button" class="close" onclick="this.parentElement.remove()">
                    <span>&times;</span>
                </button>
            </div>
        `;

    container.innerHTML = alertHTML;

    // Auto remove after 5 seconds
    setTimeout(() => {
      const alert = container.querySelector(".alert");
      if (alert) alert.remove();
    }, 5000);
  }

  showSuccess(message) {
    this.showAlert(message, "success");
  }

  showError(message) {
    this.showAlert(message, "danger");
  }

  showWarning(message) {
    this.showAlert(message, "warning");
  }

  // Edit User Methods
  async openEditUserModal(userId) {
    console.log("Opening edit modal for user:", userId);

    const user = this.findUserById(userId);
    if (!user) {
      this.showError("Không tìm thấy thông tin người dùng");
      return;
    }

    // Ensure roles are loaded before populating dropdown
    if (!this.availableRoles || this.availableRoles.length === 0) {
      try {
        await this.loadAvailableRoles();
      } catch (_) {}
    }

    // Populate the edit form fields
    await this.populateEditForm(user);

    // Populate and select current role robustly
    this.populateEditModalRoleDropdown();
    const roleSelect = document.getElementById("edit-roleName");
    if (roleSelect) {
      roleSelect.disabled = false;
      roleSelect.dataset.currentRole = user.roleName || "";
      this.setRoleSelectValue(roleSelect, user.roleName);
    }

    // Show the modal
    const editModal = new bootstrap.Modal(
      document.getElementById("editUserModal")
    );
    editModal.show();
  }

  findUserById(userId) {
    // Find user in the current users list by publicId
    if (this.currentUsers && Array.isArray(this.currentUsers)) {
      return this.currentUsers.find((user) => user.publicId === userId);
    }
    return null;
  }

  async populateEditForm(user) {
    try {
      console.log("Populating edit form with user data:", user);

      // Populate form fields directly from user object
      document.getElementById("edit-publicId").value = user.publicId;
      document.getElementById("edit-fullName").value = user.fullName || "";
      document.getElementById("edit-username").value = user.userName || "";
      document.getElementById("edit-email").value = user.email || "";
      document.getElementById("edit-phoneNumber").value =
        user.phoneNumber || "";
      document.getElementById("edit-address").value = user.address || "";

      // Role dropdown selection handled in openEditUserModal to guarantee timing
    } catch (error) {
      console.error("Error populating edit form:", error);
      this.showError("Không thể tải thông tin người dùng");
    }
  }

  // Robustly set selected option by role name (matches value or label, case-insensitive)
  setRoleSelectValue(selectElement, roleName) {
    if (!selectElement) return;
    const target = (roleName || "").toString().trim();
    if (!target) return;

    // Try exact value match first
    for (let i = 0; i < selectElement.options.length; i++) {
      if (selectElement.options[i].value === target) {
        selectElement.selectedIndex = i;
        return;
      }
    }

    // Try case-insensitive match on value or text
    const norm = (s) => (s || "").toString().trim().toLowerCase();
    for (let i = 0; i < selectElement.options.length; i++) {
      const opt = selectElement.options[i];
      if (
        norm(opt.value) === norm(target) ||
        norm(opt.textContent) === norm(target)
      ) {
        selectElement.selectedIndex = i;
        return;
      }
    }

    // Fallback: append the current role if not present
    const fallback = document.createElement("option");
    fallback.value = target;
    fallback.textContent = target;
    selectElement.appendChild(fallback);
    selectElement.value = target;
  }

  populateEditModalRoleDropdown() {
    console.log(
      "Populating edit modal role dropdown with",
      this.availableRoles.length,
      "roles"
    );

    const roleSelect = document.getElementById("edit-roleName");
    if (!roleSelect) {
      console.error("Edit role dropdown not found");
      return;
    }

    // Clear existing options except the first one
    while (roleSelect.options.length > 1) {
      roleSelect.remove(1);
    }

    // Add role options
    this.availableRoles.forEach((role) => {
      const option = document.createElement("option");
      option.value = role.value;
      option.textContent = role.label;
      roleSelect.appendChild(option);
    });

    // If we have a current role stored, try to select it now as well
    if (roleSelect.dataset.currentRole) {
      this.setRoleSelectValue(roleSelect, roleSelect.dataset.currentRole);
    }

    console.log("Edit modal role dropdown populated successfully");
  }

  async updateUserFromModal() {
    const form = document.getElementById("edit-user-form");
    if (!form.checkValidity()) {
      form.reportValidity();
      return;
    }

    const formData = new FormData(form);
    const selectedRole = formData.get("roleName");
    if (!selectedRole || selectedRole === "") {
      console.warn(
        "No role selected in edit modal; defaulting to current value if present"
      );
    }
    const updateData = {
      fullName: formData.get("fullName"),
      userName: formData.get("username"),
      phoneNumber: formData.get("phoneNumber"),
      address: formData.get("address"),
      roleName: formData.get("roleName"),
    };
    console.log("Submitting update with role:", updateData.roleName);

    const publicId = formData.get("publicId");
    if (!publicId) {
      this.showError("Thiếu thông tin người dùng");
      return;
    }

    try {
      this.showLoading();

      const response = await fetch(`${this.apiBaseUrl}/user/${publicId}`, {
        method: "PATCH",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${window.authManager.getAuthToken()}`,
        },
        body: JSON.stringify(updateData),
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(
          errorData.message || `HTTP error! status: ${response.status}`
        );
      }

      const result = await response.json();
      console.log("User updated successfully:", result);

      this.showSuccess("Cập nhật người dùng thành công");

      // Close modal
      const editModal = bootstrap.Modal.getInstance(
        document.getElementById("editUserModal")
      );
      if (editModal) {
        editModal.hide();
      }

      // Refresh user list
      this.loadUsers();
    } catch (error) {
      console.error("Error updating user:", error);
      this.showError(`Không thể cập nhật người dùng: ${error.message}`);
    } finally {
      this.hideLoading();
    }
  }

  resetEditUserForm() {
    const form = document.getElementById("edit-user-form");
    if (form) {
      form.reset();
    }

    // Clear any validation messages
    const feedbacks = form.querySelectorAll(".invalid-feedback");
    feedbacks.forEach((feedback) => {
      feedback.textContent = "";
    });
  }

  // Debug function to check dropdown state
  debugDropdown() {
    const addRoleSelect = document.getElementById("add-roleName");
    if (addRoleSelect) {
      console.log("=== DROPDOWN DEBUG INFO ===");
      console.log("Element found:", addRoleSelect);
      console.log("Element ID:", addRoleSelect.id);
      console.log("Element tagName:", addRoleSelect.tagName);
      console.log("Element className:", addRoleSelect.className);
      console.log("Element disabled:", addRoleSelect.disabled);
      console.log("Element readonly:", addRoleSelect.readOnly);
      console.log("Element style:", addRoleSelect.style.cssText);
      console.log(
        "Element computed style:",
        window.getComputedStyle(addRoleSelect)
      );
      console.log("Element offsetHeight:", addRoleSelect.offsetHeight);
      console.log("Element offsetWidth:", addRoleSelect.offsetWidth);
      console.log("Element innerHTML:", addRoleSelect.innerHTML);
      console.log("Element options length:", addRoleSelect.options.length);
      console.log("Element selectedIndex:", addRoleSelect.selectedIndex);
      console.log("Element value:", addRoleSelect.value);

      // Check if element is visible
      const rect = addRoleSelect.getBoundingClientRect();
      console.log("Element bounding rect:", rect);
      console.log("Element is visible:", rect.width > 0 && rect.height > 0);

      // Check parent elements
      let parent = addRoleSelect.parentElement;
      let level = 1;
      while (parent && level <= 5) {
        console.log(`Parent level ${level}:`, {
          tagName: parent.tagName,
          id: parent.id,
          className: parent.className,
          style: parent.style.cssText,
          display: window.getComputedStyle(parent).display,
          visibility: window.getComputedStyle(parent).visibility,
          zIndex: window.getComputedStyle(parent).zIndex,
        });
        parent = parent.parentElement;
        level++;
      }

      console.log("=== END DEBUG INFO ===");
    } else {
      console.error("Dropdown element not found for debugging");
    }
  }
}

// Initialize UserManager
window.userManager = new UserManager();
