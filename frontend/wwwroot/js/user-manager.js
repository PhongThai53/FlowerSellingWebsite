// User Management JavaScript Module
class UserManager {
    constructor() {
        this.apiBaseUrl = "http://localhost:5062/api";
        this.currentPage = 1;
        this.pageSize = 10;
        this.totalPages = 0;
        this.currentUserId = null;
        this.loadingTimeout = null;
        this.isEditMode = false;
        this.isCreateMode = false;
        this.userToDelete = null;
        this.availableRoles = [];
        
        this.init();
    }

    init() {
        // Ensure auth manager is available
        if (!window.authManager) {
            console.error('AuthManager not found. Please ensure auth.js is loaded first.');
            return;
        }
    }

    // Initialize User List Page
    initializeUserList() {
        console.log('UserList initialization debug:', {
            authManager: window.authManager,
            isAuthenticated: window.authManager?.isAuthenticated(),
            currentUser: window.authManager?.getCurrentUser(),
            token: window.authManager?.getAuthToken() ? 'Present' : 'Missing'
        });
        
        // Load roles first (before permission check) so dropdown is always populated
        this.loadAvailableRoles();
        
        // Check permission after loading roles
        if (!this.checkAdminPermission()) {
            return; // Stop initialization if not admin
        }
        
        this.showUserViewInfo();
        this.loadUsers();
    }

    // Public method to manually refresh role dropdowns
    refreshRoleDropdowns() {
        console.log('Manually refreshing role dropdowns...');
        if (this.availableRoles && this.availableRoles.length > 0) {
            this.populateRoleDropdowns();
        } else {
            console.warn('No roles available. Try loading roles first.');
            this.loadAvailableRoles();
        }
    }

    // Initialize User Detail Page
    initializeUserDetail() {
        console.log('UserDetail initialization debug:', {
            authManager: window.authManager,
            isAuthenticated: window.authManager?.isAuthenticated(),
            currentUser: window.authManager?.getCurrentUser(),
            token: window.authManager?.getAuthToken() ? 'Present' : 'Missing'
        });

        // Load roles first (before permission check) so dropdown is always populated
        this.loadAvailableRoles();

        // Check permission after loading roles
        if (!this.checkAdminPermission()) {
            return; // Stop initialization if not admin
        }
        
        const urlParams = new URLSearchParams(window.location.search);
        const userId = urlParams.get('id');
        
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
            this.showAccessDenied('Bạn cần đăng nhập để truy cập trang này.', 'Chưa đăng nhập');
            setTimeout(() => {
                window.location.href = '/html/auth/login-register.html';
            }, 3000);
            return false;
        }

        // Check if user is admin
        const currentUser = window.authManager.getCurrentUser();
        if (!currentUser || currentUser.roleName !== 'Admin') {
            this.showAccessDenied(
                'Bạn không có quyền truy cập trang quản lý người dùng. Chỉ có Admin mới có thể truy cập.',
                `Vai trò hiện tại: ${currentUser?.roleName || 'Không xác định'}`
            );
            setTimeout(() => {
                window.location.href = '/html/common/homepage.html';
            }, 3000);
            return false;
        }

        return true;
    }

    // Show access denied message
    showAccessDenied(message, subtitle) {
        const container = document.querySelector('.container') || document.body;
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
        const countdownElement = document.getElementById('countdown');
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
        const isAdmin = currentUser?.roleName === 'Admin';
        const infoContainer = document.getElementById('view-info-container');
        
        console.log('showUserViewInfo Debug:', {
            currentUser: currentUser,
            roleName: currentUser?.roleName,
            isAdmin: isAdmin
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
            console.log('Loading roles from API...');
            
            // Fetch roles from API
            const response = await this.makeAuthenticatedRequest(`${this.apiBaseUrl}/user/roles`);
            
            if (response && response.ok) {
                const roles = await response.json();
                console.log('Roles loaded from API:', roles);
                
                // Transform API data to our format
                this.availableRoles = roles.map(role => ({
                    value: role.roleName,
                    label: this.getRoleDisplayName(role.roleName),
                    id: role.id,
                    description: role.description
                }));
            } else {
                console.warn('Failed to load roles from API, using fallback');
                // Fallback to static roles if API fails
                this.availableRoles = [
                    { value: 'Admin', label: 'Quản trị viên' },
                    { value: 'Users', label: 'Người dùng' },
                    { value: 'Staff', label: 'Nhân viên' },
                    { value: 'Supplier', label: 'Nhà cung cấp' }
                ];
            }

            // Populate dropdowns with retry mechanism
            this.populateRoleDropdowns();

            console.log('Available roles populated:', this.availableRoles);
        } catch (error) {
            console.error('Error loading roles:', error);
            // Fallback to static roles on error
            this.availableRoles = [
                { value: 'Admin', label: 'Quản trị viên' },
                { value: 'Users', label: 'Người dùng' },
                { value: 'Staff', label: 'Nhân viên' },
                { value: 'Supplier', label: 'Nhà cung cấp' }
            ];
            
            // Still populate the dropdowns with fallback data
            const roleFilter = document.getElementById('role-filter');
            if (roleFilter) {
                roleFilter.innerHTML = '<option value="">Tất cả vai trò</option>';
                this.availableRoles.forEach(role => {
                    roleFilter.innerHTML += `<option value="${role.value}">${role.label}</option>`;
                });
            }

            const roleSelect = document.getElementById('roleName');
            if (roleSelect) {
                roleSelect.innerHTML = '<option value="">Chọn vai trò</option>';
                this.availableRoles.forEach(role => {
                    roleSelect.innerHTML += `<option value="${role.value}">${role.label}</option>`;
                });
            }
        }
    }

    // Get display name for role
    getRoleDisplayName(roleName) {
        const roleDisplayNames = {
            'Admin': 'Quản trị viên',
            'User': 'Người dùng',
            'Users': 'Người dùng',
            'Staff': 'Nhân viên',
            'Supplier': 'Nhà cung cấp',
            'Moderator': 'Kiểm duyệt viên',
            'Guest': 'Khách'
        };
        return roleDisplayNames[roleName] || roleName;
    }

    // Populate role dropdowns with retry mechanism
    populateRoleDropdowns(retryCount = 0) {
        const maxRetries = 5;
        const retryDelay = 100; // 100ms delay between retries

        // Check if roles are available
        if (!this.availableRoles || this.availableRoles.length === 0) {
            console.warn('No roles available to populate dropdowns');
            return;
        }

        let populated = false;

        // Try to populate ALL role filter elements (in case there are duplicates)
        const roleFilters = document.querySelectorAll('#role-filter, select[id="role-filter"]');
        console.log('Found', roleFilters.length, 'role-filter elements');
        
        if (roleFilters.length > 0) {
            roleFilters.forEach((roleFilter, index) => {
                console.log(`Populating role-filter dropdown ${index + 1}...`);
                console.log('Before populate - options count:', roleFilter.options.length);
                console.log('Before populate - innerHTML:', roleFilter.innerHTML);
                
                // Clear and rebuild
                roleFilter.innerHTML = '';
                roleFilter.innerHTML = '<option value="">Tất cả vai trò</option>';
                this.availableRoles.forEach(role => {
                    const option = document.createElement('option');
                    option.value = role.value;
                    option.textContent = role.label;
                    roleFilter.appendChild(option);
                });
                
                console.log('After populate - options count:', roleFilter.options.length);
                console.log('After populate - innerHTML:', roleFilter.innerHTML);
                
                // Force refresh the select element
                roleFilter.style.display = 'none';
                roleFilter.offsetHeight; // trigger reflow
                roleFilter.style.display = '';
            });
            
            console.log('All role filter dropdowns populated with', this.availableRoles.length, 'roles');
            populated = true;
        } else {
            console.warn('No role-filter elements found');
        }

        // Try to populate role select in detail form
        const roleSelect = document.getElementById('roleName');
        if (roleSelect) {
            console.log('Populating roleName dropdown...');
            roleSelect.innerHTML = '<option value="">Chọn vai trò</option>';
            this.availableRoles.forEach(role => {
                roleSelect.innerHTML += `<option value="${role.value}">${role.label}</option>`;
            });
            console.log('Role select populated with', this.availableRoles.length, 'roles');
            console.log('Final role select HTML:', roleSelect.innerHTML);
            populated = true;
        } else {
            console.warn('roleName element not found (expected on detail page)');
        }

        // If no elements found and we haven't exceeded retry limit, try again
        if (!populated && retryCount < maxRetries) {
            console.log(`Dropdown elements not ready, retrying in ${retryDelay}ms... (attempt ${retryCount + 1}/${maxRetries})`);
            setTimeout(() => {
                this.populateRoleDropdowns(retryCount + 1);
            }, retryDelay);
        } else if (!populated) {
            console.error('Failed to populate role dropdowns after maximum retries');
        } else {
            console.log('Role dropdowns populated successfully');
        }
    }

    // Load users with filters and pagination
    async loadUsers() {
        this.showLoading();

        try {
            const filters = this.getFilters();
            const queryParams = this.buildQueryParams(filters);

            console.log('Loading users with filters:', filters);
            console.log('Query params:', queryParams);

            const response = await this.makeAuthenticatedRequest(
                `${this.apiBaseUrl}/user/list`, 
                {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(queryParams)
                }
            );

            if (response && response.ok) {
                const result = await response.json();
                console.log('Users loaded:', result);
                
                this.totalPages = result.total_pages || 1;
                this.renderUsers(result.items || []);
                this.updatePaginationControls();
            } else {
                console.error('Failed to load users:', response?.status, response?.statusText);
                this.showError('Không thể tải danh sách người dùng');
            }
        } catch (error) {
            console.error('Error loading users:', error);
            this.showError('Lỗi khi tải danh sách người dùng');
        } finally {
            this.hideLoading();
        }
    }

    // Build query parameters for API request
    buildQueryParams(filters) {
        const params = {
            page_index: this.currentPage,
            page_size: this.pageSize,
            sort_by: filters.sortBy || 'fullname',
            sort_direction: filters.sortDirection === 'Desc' ? 1 : 0, // Enum: Asc = 0, Desc = 1
        };

        // Add search parameters
        if (filters.search && filters.search.trim()) {
            // Support multiple search fields - search in fullname by default
            // but can be extended to search in multiple fields
            params.search_by = 'fullname';
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
            search: document.getElementById('search-input')?.value || '',
            role: document.getElementById('role-filter')?.value || '',
            sortBy: document.getElementById('sort-by')?.value || 'createdat',
            sortDirection: document.getElementById('sort-direction')?.value || 'Desc'
        };
    }

    // Render users list
    renderUsers(users) {
        const container = document.getElementById('users-container');
        if (!container) return;

        if (users.length === 0) {
            container.innerHTML = this.getEmptyStateHTML();
            return;
        }

        const usersHTML = users.map(user => this.getUserCardHTML(user)).join('');
        container.innerHTML = `
            <div class="row">
                ${usersHTML}
            </div>
        `;
    }

    // Generate HTML for a user card
    getUserCardHTML(user) {
        const initials = this.getInitials(user.fullName);
        const roleClass = this.getRoleClass(user.roleName);
        const createdDate = new Date(user.createdAt).toLocaleDateString('vi-VN');

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
                                <span class="role-badge ${roleClass}">${user.roleName}</span>
                            </div>
                        </div>
                        
                        <div class="user-info">
                            <p class="mb-2">
                                <i class="fa fa-envelope text-muted mr-2"></i>
                                ${user.email}
                            </p>
                            ${user.phoneNumber ? `
                                <p class="mb-2">
                                    <i class="fa fa-phone text-muted mr-2"></i>
                                    ${user.phoneNumber}
                                </p>
                            ` : ''}

                            <p class="mb-3 text-muted small">
                                <i class="fa fa-calendar text-muted mr-2"></i>
                                Tạo: ${createdDate}
                            </p>
                        </div>

                        <div class="user-actions">
                            <button class="btn-action btn-view" onclick="userManager.viewUser('${user.publicId}')" title="Xem chi tiết">
                                <i class="fa fa-eye"></i>
                            </button>
                            <button class="btn-action btn-edit" onclick="userManager.editUser('${user.publicId}')" title="Chỉnh sửa">
                                <i class="fa fa-edit"></i>
                            </button>
                            <button class="btn-action btn-delete" onclick="userManager.openDeleteModal('${user.publicId}', '${user.fullName}')" title="Xóa">
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
        if (!fullName) return 'U';
        const names = fullName.trim().split(' ');
        if (names.length === 1) return names[0][0].toUpperCase();
        return (names[0][0] + names[names.length - 1][0]).toUpperCase();
    }

    // Get role CSS class
    getRoleClass(roleName) {
        switch (roleName?.toLowerCase()) {
            case 'admin': return 'role-admin';
            case 'user': return 'role-user';
            case 'moderator': return 'role-moderator';
            default: return 'role-user';
        }
    }

    // Load user detail
    async loadUserDetail(userId) {
        this.showLoading();

        try {
            const response = await this.makeAuthenticatedRequest(`${this.apiBaseUrl}/user/${userId}`);

            if (response && response.ok) {
                const user = await response.json();
                console.log('User detail loaded:', user);
                this.populateUserForm(user);
                this.updateBreadcrumb(user.fullName);
            } else {
                console.error('Failed to load user detail:', response?.status, response?.statusText);
                this.showError('Không thể tải thông tin người dùng');
            }
        } catch (error) {
            console.error('Error loading user detail:', error);
            this.showError('Lỗi khi tải thông tin người dùng');
        } finally {
            this.hideLoading();
        }
    }

    // Populate user form with data
    populateUserForm(user) {
        document.getElementById('fullName').value = user.fullName || '';
        document.getElementById('email').value = user.email || '';
        document.getElementById('phoneNumber').value = user.phoneNumber || '';
        document.getElementById('address').value = user.address || '';
        document.getElementById('roleName').value = user.roleName || '';
        
        if (user.createdAt) {
            const createdDate = new Date(user.createdAt).toLocaleString('vi-VN');
            document.getElementById('createdAt').value = createdDate;
        }

        // Update avatar
        const avatar = document.getElementById('user-avatar');
        if (avatar) {
            avatar.innerHTML = this.getInitials(user.fullName);
        }

        // Update card title
        document.getElementById('card-title').textContent = `Chi tiết: ${user.fullName}`;
    }

    // Update breadcrumb
    updateBreadcrumb(userName) {
        const breadcrumb = document.getElementById('breadcrumb-current');
        if (breadcrumb) {
            breadcrumb.textContent = `Chi tiết: ${userName}`;
        }
    }

    // Enter create mode
    enterCreateMode() {
        this.isCreateMode = true;
        this.isEditMode = true;

        // Update UI for create mode
        document.getElementById('card-title').textContent = 'Tạo người dùng mới';
        document.getElementById('breadcrumb-current').textContent = 'Tạo người dùng mới';
        
        // Show password field
        document.getElementById('password-group').style.display = 'block';
        
        // Show create mode buttons
        document.getElementById('view-mode-buttons').style.display = 'none';
        document.getElementById('edit-mode-buttons').style.display = 'none';
        document.getElementById('create-mode-buttons').style.display = 'flex';

        this.toggleFormEditMode(true);
    }

    // Toggle edit mode
    toggleEditMode() {
        if (this.isCreateMode) return;

        this.isEditMode = !this.isEditMode;
        this.toggleFormEditMode(this.isEditMode);

        // Update buttons
        if (this.isEditMode) {
            document.getElementById('view-mode-buttons').style.display = 'none';
            document.getElementById('edit-mode-buttons').style.display = 'flex';
        } else {
            document.getElementById('view-mode-buttons').style.display = 'flex';
            document.getElementById('edit-mode-buttons').style.display = 'none';
        }
    }

    // Toggle form edit mode
    toggleFormEditMode(isEdit) {
        const form = document.getElementById('user-form');
        const inputs = form.querySelectorAll('input, select, textarea');
        
        if (isEdit) {
            form.classList.remove('view-mode');
            form.classList.add('edit-mode');
            
            inputs.forEach(input => {
                if (input.id !== 'email' && input.id !== 'createdAt') { // Email is usually not editable
                    input.removeAttribute('readonly');
                    input.removeAttribute('disabled');
                }
            });
        } else {
            form.classList.remove('edit-mode');
            form.classList.add('view-mode');
            
            inputs.forEach(input => {
                if (input.tagName === 'SELECT') {
                    input.setAttribute('disabled', '');
                } else {
                    input.setAttribute('readonly', '');
                }
            });
        }
    }

    // Cancel edit
    cancelEdit() {
        if (this.isCreateMode) {
            window.location.href = 'user-list.html';
            return;
        }

        this.isEditMode = false;
        this.toggleFormEditMode(false);
        
        // Reload user data
        if (this.currentUserId) {
            this.loadUserDetail(this.currentUserId);
        }

        // Update buttons
        document.getElementById('view-mode-buttons').style.display = 'flex';
        document.getElementById('edit-mode-buttons').style.display = 'none';
    }

    // Save user (update)
    async saveUser() {
        if (!this.validateForm()) return;

        this.showLoading();

        try {
            const formData = this.getFormData();
            delete formData.email; // Don't send email in update
            delete formData.password; // Don't send password in update

            const response = await this.makeAuthenticatedRequest(
                `${this.apiBaseUrl}/user/${this.currentUserId}`,
                {
                    method: 'PATCH',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(formData)
                }
            );

            if (response && response.ok) {
                this.showSuccess('Cập nhật người dùng thành công');
                this.toggleEditMode(); // Exit edit mode
                this.loadUserDetail(this.currentUserId); // Reload data
            } else {
                const errorData = await response.json().catch(() => null);
                const errorMessage = errorData?.message || 'Không thể cập nhật người dùng';
                this.showError(errorMessage);
            }
        } catch (error) {
            console.error('Error saving user:', error);
            this.showError('Lỗi khi lưu thông tin người dùng');
        } finally {
            this.hideLoading();
        }
    }

    // Create user
    async createUser() {
        if (!this.validateForm()) return;

        this.showLoading();

        try {
            const formData = this.getFormData();

            const response = await this.makeAuthenticatedRequest(
                `${this.apiBaseUrl}/user`,
                {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(formData)
                }
            );

            if (response && response.ok) {
                this.showSuccess('Tạo người dùng thành công');
                setTimeout(() => {
                    window.location.href = 'user-list.html';
                }, 1500);
            } else {
                const errorData = await response.json().catch(() => null);
                const errorMessage = errorData?.message || 'Không thể tạo người dùng';
                this.showError(errorMessage);
            }
        } catch (error) {
            console.error('Error creating user:', error);
            this.showError('Lỗi khi tạo người dùng');
        } finally {
            this.hideLoading();
        }
    }

    // Get form data
    getFormData() {
        const formData = {};
        const form = document.getElementById('user-form');
        const inputs = form.querySelectorAll('input, select, textarea');
        
                    inputs.forEach(input => {
                if (input.name && input.value !== '') {
                    if (input.type === 'checkbox') {
                        formData[input.name] = input.checked;
                    } else {
                        formData[input.name] = input.value;
                    }
                }
            });

        return formData;
    }

    // Validate form
    validateForm() {
        const form = document.getElementById('user-form');
        const inputs = form.querySelectorAll('input[required], select[required]');
        let isValid = true;

        inputs.forEach(input => {
            this.clearValidation(input);
            
            if (!input.value.trim()) {
                this.showValidationError(input, 'Trường này là bắt buộc');
                isValid = false;
            } else if (input.type === 'email' && !this.isValidEmail(input.value)) {
                this.showValidationError(input, 'Email không hợp lệ');
                isValid = false;
            } else if (input.name === 'password' && input.value.length < 6) {
                this.showValidationError(input, 'Mật khẩu phải có ít nhất 6 ký tự');
                isValid = false;
            }
        });

        return isValid;
    }

    // Show validation error
    showValidationError(input, message) {
        input.classList.add('is-invalid');
        const feedback = input.parentElement.querySelector('.invalid-feedback');
        if (feedback) {
            feedback.textContent = message;
        }
    }

    // Clear validation
    clearValidation(input) {
        input.classList.remove('is-invalid', 'is-valid');
        const feedback = input.parentElement.querySelector('.invalid-feedback');
        if (feedback) {
            feedback.textContent = '';
        }
    }

    // Validate email
    isValidEmail(email) {
        const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return re.test(email);
    }

    // Delete user
    async deleteUser() {
        if (!this.currentUserId) return;

        this.showLoading();

        try {
            const response = await this.makeAuthenticatedRequest(
                `${this.apiBaseUrl}/user/${this.currentUserId}`,
                { method: 'DELETE' }
            );

            if (response && response.ok) {
                this.showSuccess('Xóa người dùng thành công');
                setTimeout(() => {
                    window.location.href = 'user-list.html';
                }, 1500);
            } else {
                this.showError('Không thể xóa người dùng');
            }
        } catch (error) {
            console.error('Error deleting user:', error);
            this.showError('Lỗi khi xóa người dùng');
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
        window.location.href = 'user-detail.html';
    }

    // Delete modal methods
    openDeleteModal(userId, userName) {
        this.userToDelete = { id: userId, name: userName };
        const modal = document.getElementById('deleteModal');
        if (modal) {
            modal.querySelector('p').textContent = `Bạn có chắc chắn muốn xóa người dùng "${userName}" không?`;
            modal.style.display = 'block';
        }
    }

    closeDeleteModal() {
        this.userToDelete = null;
        const modal = document.getElementById('deleteModal');
        if (modal) {
            modal.style.display = 'none';
        }
    }

    async confirmDelete() {
        if (!this.userToDelete) return;

        this.closeDeleteModal();
        this.showLoading();

        try {
            const response = await this.makeAuthenticatedRequest(
                `${this.apiBaseUrl}/user/${this.userToDelete.id}`,
                { method: 'DELETE' }
            );

            if (response && response.ok) {
                this.showSuccess(`Đã xóa người dùng "${this.userToDelete.name}"`);
                this.loadUsers(); // Reload the list
            } else {
                this.showError('Không thể xóa người dùng');
            }
        } catch (error) {
            console.error('Error deleting user:', error);
            this.showError('Lỗi khi xóa người dùng');
        } finally {
            this.hideLoading();
            this.userToDelete = null;
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
        document.getElementById('search-input').value = '';
        document.getElementById('role-filter').value = '';
        document.getElementById('sort-by').value = 'createdat';
        document.getElementById('sort-direction').value = 'Desc';
        
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
        const prevBtn = document.getElementById('prev-btn');
        const nextBtn = document.getElementById('next-btn');
        const paginationInfo = document.getElementById('pagination-info');

        if (prevBtn) prevBtn.disabled = this.currentPage <= 1;
        if (nextBtn) nextBtn.disabled = this.currentPage >= this.totalPages;
        
        if (paginationInfo) {
            paginationInfo.textContent = `Trang ${this.currentPage} của ${this.totalPages}`;
        }
    }

    // Utility methods
    togglePassword(fieldId) {
        const field = document.getElementById(fieldId);
        const button = field.parentElement.querySelector('.password-toggle i');
        
        if (field.type === 'password') {
            field.type = 'text';
            button.className = 'fa fa-eye-slash';
        } else {
            field.type = 'password';
            button.className = 'fa fa-eye';
        }
    }

    // HTTP request methods
    async makeAuthenticatedRequest(url, options = {}) {
        const token = window.authManager?.getAuthToken();
        if (!token) {
            throw new Error('No authentication token available');
        }

        const defaultOptions = {
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json',
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
        const overlay = document.getElementById('loading-overlay');
        if (overlay) {
            overlay.style.display = 'flex';
        }
    }

    hideLoading() {
        const overlay = document.getElementById('loading-overlay');
        if (overlay) {
            overlay.style.display = 'none';
        }
    }

    showAlert(message, type = 'info') {
        const container = document.getElementById('alert-container');
        if (!container) return;

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
            const alert = container.querySelector('.alert');
            if (alert) alert.remove();
        }, 5000);
    }

    showSuccess(message) {
        this.showAlert(message, 'success');
    }

    showError(message) {
        this.showAlert(message, 'danger');
    }

    showWarning(message) {
        this.showAlert(message, 'warning');
    }
}

// Initialize UserManager
window.userManager = new UserManager();
