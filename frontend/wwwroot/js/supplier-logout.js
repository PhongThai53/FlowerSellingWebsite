class SupplierLogoutManager {
    constructor() {
        this.init();
    }

    init() {
        console.log('Initializing SupplierLogoutManager...');
        this.setupEventListeners();
        this.displayUserInfo();
        console.log('SupplierLogoutManager initialization complete');
    }

    // Method để re-init khi header được load
    reinit() {
        console.log('Re-initializing SupplierLogoutManager...');
        this.init();
    }

    setupEventListeners() {
        // Xử lý sự kiện click nút logout
        const logoutBtn = document.getElementById('logout-btn');
        if (logoutBtn) {
            logoutBtn.addEventListener('click', () => {
                console.log('Logout button clicked');
                this.handleLogout();
            });
            console.log('Logout button event listener added');
        } else {
            console.warn('Logout button not found in DOM');
        }
    }

    displayUserInfo() {
        // Hiển thị thông tin user
        const userInfoElement = document.getElementById('user-info');
        if (userInfoElement) {
            const userData = this.getCurrentUser();
            if (userData) {
                const displayName = userData.fullName || userData.userName || 'Supplier';
                userInfoElement.textContent = `Xin chào, ${displayName}`;
                console.log('User info displayed:', displayName);
            } else {
                userInfoElement.textContent = 'Xin chào, Supplier';
                console.log('No user data, using default greeting');
            }
        } else {
            console.warn('User info element not found in DOM');
        }
    }

    getCurrentUser() {
        try {
            const userData = localStorage.getItem('userData') || sessionStorage.getItem('userData');
            return userData ? JSON.parse(userData) : null;
        } catch (error) {
            console.error('Error parsing user data:', error);
            return null;
        }
    }

    async handleLogout() {
        try {
            console.log('Starting logout process...');
            
            // Hiển thị confirm dialog
            const confirmed = confirm('Bạn có chắc chắn muốn đăng xuất?');
            if (!confirmed) {
                console.log('Logout cancelled by user');
                return;
            }

            console.log('User confirmed logout, clearing auth data...');
            
            // Xóa dữ liệu authentication
            this.clearAuthData();

            // Hiển thị thông báo thành công
            alert('Đăng xuất thành công!');

            console.log('Redirecting to login page...');
            
            // Chuyển hướng về trang đăng nhập
            window.location.href = '../auth/login-register.html';

        } catch (error) {
            console.error('Error during logout:', error);
            alert('Có lỗi xảy ra khi đăng xuất: ' + error.message);
        }
    }

    clearAuthData() {
        console.log('Clearing authentication data...');
        
        // Xóa JWT token
        localStorage.removeItem('auth_token');
        sessionStorage.removeItem('auth_token');
        
        // Xóa user data
        localStorage.removeItem('userData');
        sessionStorage.removeItem('userData');
        
        // Xóa các dữ liệu khác nếu có
        localStorage.removeItem('authToken');
        sessionStorage.removeItem('authToken');
        localStorage.removeItem('user');
        sessionStorage.removeItem('user');
        
        // Log những gì còn lại trong storage để debug
        console.log('Remaining localStorage items:', Object.keys(localStorage));
        console.log('Remaining sessionStorage items:', Object.keys(sessionStorage));
        
        console.log('All authentication data cleared for supplier');
    }

    // Method để kiểm tra trạng thái hiện tại
    checkStatus() {
        console.log('=== Supplier Logout Manager Status ===');
        console.log('Manager instance:', this);
        console.log('Window object:', window.supplierLogoutManager);
        console.log('User data in storage:', this.getCurrentUser());
        console.log('Auth token in storage:', localStorage.getItem('auth_token') || sessionStorage.getItem('auth_token'));
        console.log('=====================================');
    }
}

// Khởi tạo khi DOM loaded
document.addEventListener('DOMContentLoaded', () => {
    try {
        window.supplierLogoutManager = new SupplierLogoutManager();
        console.log('✅ SupplierLogoutManager initialized and assigned to window');
        
        // Kiểm tra trạng thái sau khi khởi tạo
        setTimeout(() => {
            if (window.supplierLogoutManager) {
                window.supplierLogoutManager.checkStatus();
            }
        }, 1000);
        
    } catch (error) {
        console.error('❌ Error initializing SupplierLogoutManager:', error);
    }
});

// Cũng khởi tạo ngay lập tức nếu DOM đã sẵn sàng
if (document.readyState === 'loading') {
    console.log('DOM still loading, waiting for DOMContentLoaded...');
} else {
    console.log('DOM already ready, initializing immediately...');
    try {
        window.supplierLogoutManager = new SupplierLogoutManager();
        console.log('✅ SupplierLogoutManager initialized immediately');
    } catch (error) {
        console.error('❌ Error initializing SupplierLogoutManager immediately:', error);
    }
}
