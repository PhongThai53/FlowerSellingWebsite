class SupplierLogoutManager {
    constructor() {
        this.init();
    }

    init() {
        this.setupEventListeners();
        this.displayUserInfo();
    }

    setupEventListeners() {
        // Xử lý sự kiện click nút logout
        const logoutBtn = document.getElementById('logout-btn');
        if (logoutBtn) {
            logoutBtn.addEventListener('click', () => {
                this.handleLogout();
            });
        }
    }

    displayUserInfo() {
        // Hiển thị thông tin user
        const userInfoElement = document.getElementById('user-info');
        if (userInfoElement) {
            const userData = this.getCurrentUser();
            if (userData) {
                userInfoElement.textContent = `Xin chào, ${userData.fullName || userData.userName || 'Supplier'}`;
            } else {
                userInfoElement.textContent = 'Xin chào, Supplier';
            }
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
            // Hiển thị confirm dialog
            const confirmed = confirm('Bạn có chắc chắn muốn đăng xuất?');
            if (!confirmed) {
                return;
            }

            // Xóa dữ liệu authentication
            this.clearAuthData();

            // Hiển thị thông báo thành công
            alert('Đăng xuất thành công!');

            // Chuyển hướng về trang đăng nhập
            window.location.href = '../auth/login-register.html';

        } catch (error) {
            console.error('Error during logout:', error);
            alert('Có lỗi xảy ra khi đăng xuất: ' + error.message);
        }
    }

    clearAuthData() {
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
        
        console.log('All authentication data cleared');
    }
}

// Khởi tạo khi DOM loaded
document.addEventListener('DOMContentLoaded', () => {
    new SupplierLogoutManager();
});
