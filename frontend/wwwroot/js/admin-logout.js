class AdminLogoutManager {
    constructor() {
        this.init();
    }

    init() {
        this.setupEventListeners();
        this.displayUserInfo();
    }

    setupEventListeners() {
        // Có thể thêm các event listeners khác nếu cần
        console.log('AdminLogoutManager initialized');
    }

    displayUserInfo() {
        // Hiển thị thông tin user từ localStorage
        const userInfoElement = document.getElementById('admin-user-email');
        const userRoleElement = document.getElementById('admin-user-role');
        
        if (userInfoElement && userRoleElement) {
            const userData = this.getCurrentUser();
            if (userData) {
                // Hiển thị email hoặc tên user
                const displayName = userData.email || userData.userName || userData.fullName || 'Admin';
                userInfoElement.textContent = displayName;
                
                // Hiển thị role
                const displayRole = userData.roleName || 'Administrator';
                userRoleElement.textContent = displayRole;
                
                console.log('Admin user info displayed:', { displayName, displayRole });
            } else {
                userInfoElement.textContent = 'Admin';
                userRoleElement.textContent = 'Administrator';
                console.log('No user data found, using default values');
            }
        } else {
            console.warn('User info elements not found');
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

    showProfile() {
        // Hiển thị thông tin profile của admin
        const userData = this.getCurrentUser();
        if (userData) {
            const profileInfo = `
Thông tin Profile:
• Tên: ${userData.fullName || userData.userName || 'Admin'}
• Email: ${userData.email || 'N/A'}
• Vai trò: ${userData.roleName || 'Administrator'}
• Ngày tham gia: ${userData.createdAt || 'N/A'}
• ID: ${userData.id || 'N/A'}
            `;
            alert(profileInfo);
        } else {
            alert('Không thể lấy thông tin profile. Vui lòng đăng nhập lại.');
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
        
        console.log('All authentication data cleared for admin');
    }
}

// Khởi tạo khi DOM loaded
document.addEventListener('DOMContentLoaded', () => {
    window.adminLogoutManager = new AdminLogoutManager();
});
