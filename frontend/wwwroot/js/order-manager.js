// Order Management JavaScript Module
class OrderManager {
    constructor() {
        this.apiBaseUrl = "http://localhost:5062/api";
        this.currentPage = 1;
        this.pageSize = 10;
        this.totalPages = 0;
        this.totalItems = 0;
        // No customer ID needed for admin order management
        this.loadingTimeout = null;
        
        this.init();
    }

    init() {
        // Ensure auth manager is available
        if (!window.authManager) {
            console.error('AuthManager not found. Please ensure auth.js is loaded first.');
            return;
        }
    }

    // Initialize Order List Page
    initializeOrderList() {
        console.log('OrderList initialization debug:', {
            authManager: window.authManager,
            isAuthenticated: window.authManager?.isAuthenticated(),
            currentUser: window.authManager?.getCurrentUser(),
            token: window.authManager?.getAuthToken() ? 'Present' : 'Missing'
        });
        
        // Check permission
        if (!this.checkPermission()) {
            return;
        }
        
        this.showOrderViewInfo();
        
        // This is admin order management page, not customer-specific
        // No need for customer ID
        
        // Wire search input
        const searchInput = document.getElementById('order-search');
        if (searchInput) {
            const debouncedSearch = this.debounce(() => {
                this.currentPage = 1;
                this.loadOrders();
            }, 300);
            searchInput.addEventListener('input', debouncedSearch);
        }

        // Wire filter and sort dropdowns for auto-submit
        const statusFilter = document.getElementById('status-filter');
        if (statusFilter) {
            statusFilter.addEventListener('change', () => {
                this.currentPage = 1;
                this.loadOrders();
            });
        }

        const paymentStatusFilter = document.getElementById('payment-status-filter');
        if (paymentStatusFilter) {
            paymentStatusFilter.addEventListener('change', () => {
                this.currentPage = 1;
                this.loadOrders();
            });
        }

        const sortBy = document.getElementById('sort-by');
        if (sortBy) {
            sortBy.addEventListener('change', () => {
                this.currentPage = 1;
                this.loadOrders();
            });
        }

        const sortDirection = document.getElementById('sort-direction');
        if (sortDirection) {
            sortDirection.addEventListener('change', () => {
                this.currentPage = 1;
                this.loadOrders();
            });
        }

        // Load orders
        this.loadOrders();
    }

    // Check if user has permission
    checkPermission() {
        // Check if user is authenticated
        if (!window.authManager?.isAuthenticated()) {
            this.showAccessDenied('Bạn cần đăng nhập để truy cập trang này.', 'Chưa đăng nhập');
            setTimeout(() => {
                window.location.href = '/html/auth/login-register.html';
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
    showOrderViewInfo() {
        const currentUser = window.authManager?.getCurrentUser();
        const infoContainer = document.getElementById('view-info-container');
        
        if (infoContainer) {
            infoContainer.innerHTML = `
                <div class="alert alert-info mb-3">
                    <i class="fa fa-shopping-cart"></i> <strong>Quản lý đơn hàng:</strong> Xem và quản lý tất cả đơn hàng trong hệ thống.
                    <br><small>Vai trò: ${currentUser?.roleName || 'Người dùng'}</small>
                </div>
            `;
        }
    }

    // Load orders with filters and pagination
    async loadOrders() {
        this.showLoading();

        try {
            const filters = this.getFilters();
            const queryParams = this.buildQueryParams(filters);

            console.log('Loading orders with filters:', filters);
            console.log('Query params:', queryParams);

            const response = await this.makeAuthenticatedRequest(
                `${this.apiBaseUrl}/order/list`, 
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
                console.log('Orders loaded:', result);
                
                this.totalPages = result.total_pages || 1;
                this.totalItems = result.total_items || 0;
                if (result.page_size) {
                    this.pageSize = result.page_size;
                }
                this.renderOrders(result.items || []);
                this.updatePaginationControls();
            } else {
                console.error('Failed to load orders:', response?.status, response?.statusText);
                this.showError('Không thể tải danh sách đơn hàng');
            }
        } catch (error) {
            console.error('Error loading orders:', error);
            this.showError('Lỗi khi tải danh sách đơn hàng');
        } finally {
            this.hideLoading();
        }
    }

    // Build query parameters for API request
    buildQueryParams(filters) {
        const params = {
            page_index: this.currentPage, // API uses 0-based indexing
            page_size: this.pageSize,
            sort_by: filters.sortBy || 'order_date',
            sort_direction: filters.sortDirection === 'Desc' ? 1 : 0, // Enum: Asc = 0, Desc = 1
        };

        // Add search parameters
        if (filters.search && filters.search.trim()) {
            params.search_by = 'order_number'; // Default search by order number
            params.search_value = filters.search.trim();
        }

        // Add filter parameters
        const filterParams = {};
        if (filters.status && filters.status !== '') {
            filterParams.status = filters.status;
        }
        if (filters.paymentStatus && filters.paymentStatus !== '') {
            filterParams.payment_status = filters.paymentStatus;
        }

        if (Object.keys(filterParams).length > 0) {
            params.filers = filterParams; // Note: typo in backend DTO "filers" instead of "filters"
        }

        return params;
    }

    // Get current filter values
    getFilters() {
        return {
            search: document.getElementById('order-search')?.value || '',
            status: document.getElementById('status-filter')?.value || '',
            paymentStatus: document.getElementById('payment-status-filter')?.value || '',
            sortBy: document.getElementById('sort-by')?.value || 'order_date',
            sortDirection: document.getElementById('sort-direction')?.value || 'Desc'
        };
    }

    // Render orders list
    renderOrders(orders) {
        const tableBody = document.querySelector('#order-list-table tbody');
        if (!tableBody) return;

        if (!orders || orders.length === 0) {
            tableBody.innerHTML = `
                <tr>
                    <td colspan="10" class="text-center">Không tìm thấy đơn hàng</td>
                </tr>
            `;
            return;
        }

        const rowsHtml = orders.map(order => this.getOrderTableRowHTML(order)).join('');
        tableBody.innerHTML = rowsHtml;
    }

    // Generate HTML for an order row
    getOrderTableRowHTML(order) {
        const orderDate = order.order_date ? new Date(order.order_date).toLocaleDateString('vi-VN') : '-';
        const requiredDate = order.required_date ? new Date(order.required_date).toLocaleDateString('vi-VN') : '-';
        const statusBadge = this.getStatusBadge(order.status);
        const paymentStatusBadge = this.getPaymentStatusBadge(order.payment_status);
        const totalAmount = this.formatCurrency(order.total_amount);

        return `
            <tr>
                <td>${order.order_number || '-'}</td>
                <td>${orderDate}</td>
                <td>${requiredDate}</td>
                <td>${statusBadge}</td>
                <td>${totalAmount}</td>
                <td>${paymentStatusBadge}</td>
                <td>${order.shipping_address || '-'}</td>
                <td>${order.notes || '-'}</td>
                <td>
                    <div class="d-flex align-items-center list-action">
                        <a class="badge badge-info mr-2" data-toggle="tooltip" data-placement="top" title="Xem chi tiết" href="javascript:void(0)" onclick="orderManager.viewOrder('${order.order_number}')">
                            <i class="ri-eye-line mr-0"></i>
                        </a>
                        <a class="badge bg-success mr-2" data-toggle="tooltip" data-placement="top" title="Chỉnh sửa" href="javascript:void(0)" onclick="orderManager.editOrder('${order.order_number}')">
                            <i class="ri-pencil-line mr-0"></i>
                        </a>
                        <a class="badge bg-warning mr-2" data-toggle="tooltip" data-placement="top" title="Xóa" href="javascript:void(0)" onclick="orderManager.openDeleteModal('${order.order_number}')">
                            <i class="ri-delete-bin-line mr-0"></i>
                        </a>
                    </div>
                </td>
            </tr>
        `;
    }

    // Get status badge HTML
    getStatusBadge(status) {
        const statusMap = {
            0: { text: 'Chờ xử lý', class: 'badge-warning' },
            1: { text: 'Đang xử lý', class: 'badge-info' },
            2: { text: 'Đã giao hàng', class: 'badge-success' },
            3: { text: 'Đã hủy', class: 'badge-danger' },
            4: { text: 'Hoàn trả', class: 'badge-secondary' }
        };
        
        const statusInfo = statusMap[status] || { text: 'Không xác định', class: 'badge-secondary' };
        return `<div class="badge ${statusInfo.class}">${statusInfo.text}</div>`;
    }

    // Get payment status badge HTML
    getPaymentStatusBadge(paymentStatus) {
        const paymentStatusMap = {
            0: { text: 'Chưa thanh toán', class: 'badge-warning' },
            1: { text: 'Đã thanh toán', class: 'badge-success' },
            2: { text: 'Thanh toán một phần', class: 'badge-info' },
            3: { text: 'Hoàn tiền', class: 'badge-secondary' }
        };
        
        const statusInfo = paymentStatusMap[paymentStatus] || { text: 'Không xác định', class: 'badge-secondary' };
        return `<div class="badge ${statusInfo.class}">${statusInfo.text}</div>`;
    }

    // Format currency
    formatCurrency(amount) {
        if (amount === null || amount === undefined) return '-';
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(amount);
    }

    // Clear filters
    clearFilters() {
        const searchInput = document.getElementById('order-search');
        if (searchInput) searchInput.value = '';
        
        const statusFilter = document.getElementById('status-filter');
        if (statusFilter) statusFilter.value = '';
        
        const paymentStatusFilter = document.getElementById('payment-status-filter');
        if (paymentStatusFilter) paymentStatusFilter.value = '';
        
        const sortBy = document.getElementById('sort-by');
        if (sortBy) sortBy.value = 'order_date';
        
        const sortDirection = document.getElementById('sort-direction');
        if (sortDirection) sortDirection.value = 'Desc';
        
        this.currentPage = 1;
        this.loadOrders();
    }

    // Pagination methods
    previousPage() {
        if (this.currentPage > 1) {
            this.currentPage--;
            this.loadOrders();
        }
    }

    nextPage() {
        if (this.currentPage < this.totalPages) {
            this.currentPage++;
            this.loadOrders();
        }
    }

    updatePaginationControls() {
        // Update "Showing ..." info
        const showingInfoContainer = document.getElementById('order-list-page-info');
        if (showingInfoContainer) {
            if (this.totalItems && this.totalItems > 0) {
                const start = (this.currentPage - 1) * this.pageSize + 1;
                const end = Math.min(this.currentPage * this.pageSize, this.totalItems);
                showingInfoContainer.innerHTML = `<span>Hiển thị ${start} đến ${end} trong tổng số ${this.totalItems} đơn hàng</span>`;
            } else {
                showingInfoContainer.innerHTML = `<span>Trang ${this.currentPage} của ${this.totalPages}</span>`;
            }
        }

        // Update pagination list
        const paginationUl = document.querySelector('ul.pagination');
        if (paginationUl) {
            paginationUl.innerHTML = this.buildPaginationHTML(this.currentPage, this.totalPages);
        }
    }

    buildPaginationHTML(currentPage, totalPages) {
        if (!totalPages || totalPages < 1) return '';

        const createPageItem = (page, isActive = false) => `
            <li class="page-item ${isActive ? 'active' : ''}">
                <a class="page-link" href="javascript:void(0)" onclick="orderManager.gotoPage(${page})">${page}</a>
            </li>
        `;

        const prevClass = currentPage <= 1 ? 'disabled' : '';
        const nextClass = currentPage >= totalPages ? 'disabled' : '';

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
                <a class="page-link" href="javascript:void(0)" onclick="orderManager.previousPage()">Trước</a>
            </li>
            ${pages.join('')}
            <li class="page-item ${nextClass}">
                <a class="page-link" href="javascript:void(0)" onclick="orderManager.nextPage()">Tiếp</a>
            </li>
        `;
    }

    gotoPage(page) {
        if (page < 1 || page > this.totalPages) return;
        this.currentPage = page;
        this.loadOrders();
    }

    // Navigation methods
    viewOrder(orderNumber) {
        // TODO: Implement view order detail
        console.log('View order:', orderNumber);
        alert(`Xem chi tiết đơn hàng: ${orderNumber}`);
    }

    openAddOrderModal() {
        // TODO: Implement add order modal
        console.log('Opening add order modal');
        alert('Chức năng thêm đơn hàng sẽ được phát triển sau');
    }

    editOrder(orderNumber) {
        // TODO: Implement edit order
        console.log('Edit order:', orderNumber);
        alert(`Chỉnh sửa đơn hàng: ${orderNumber}`);
    }

    openDeleteModal(orderNumber) {
        // TODO: Implement delete order modal
        console.log('Delete order:', orderNumber);
        if (confirm(`Bạn có chắc chắn muốn xóa đơn hàng ${orderNumber} không?`)) {
            this.deleteOrder(orderNumber);
        }
    }

    async deleteOrder(orderNumber) {
        // TODO: Implement delete order API call
        console.log('Deleting order:', orderNumber);
        this.showSuccess(`Đã xóa đơn hàng ${orderNumber}`);
    }

    // Utils
    debounce(fn, wait) {
        let t;
        return (...args) => {
            clearTimeout(t);
            t = setTimeout(() => fn.apply(this, args), wait);
        };
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
        if (!container) {
            try { window.alert(message); } catch (_) {}
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

// Initialize OrderManager
window.orderManager = new OrderManager();
