// Supplier Authentication Check Script
// Sử dụng cho tất cả các trang supplier

class SupplierAuthChecker {
  constructor() {
    this.init();
  }

  init() {
    document.addEventListener('DOMContentLoaded', () => {
      this.checkAuth();
    });
  }

  checkAuth() {
    // Đợi authManager được khởi tạo
    setTimeout(() => {
      if (window.authManager) {
        this.checkWithAuthManager();
      } else {
        this.checkWithLocalStorage();
      }
    }, 100);
  }

  checkWithAuthManager() {
    // Kiểm tra xác thực
    if (!window.authManager.isAuthenticated()) {
      this.redirectToLogin('Bạn cần đăng nhập để truy cập trang supplier!');
      return;
    }
    
    // Kiểm tra vai trò supplier
    const currentUser = window.authManager.getCurrentUser();
    if (currentUser && currentUser.roleName !== 'Supplier') {
      this.redirectToHomepage('Bạn không có quyền truy cập trang supplier!');
      return;
    }
    
    console.log('Supplier access granted for user:', currentUser.fullName);
  }

  checkWithLocalStorage() {
    console.error('AuthManager not found, using localStorage fallback');
    
    const token = localStorage.getItem('auth_token');
    const userData = localStorage.getItem('user_data');
    
    if (!token || !userData) {
      this.redirectToLogin('Bạn cần đăng nhập để truy cập trang supplier!');
      return;
    }
    
    try {
      const user = JSON.parse(userData);
      if (user.roleName !== 'Supplier') {
        this.redirectToHomepage('Bạn không có quyền truy cập trang supplier!');
        return;
      }
      console.log('Supplier access granted for user:', user.fullName);
    } catch (error) {
      console.error('Error parsing user data:', error);
      this.redirectToLogin('Lỗi xác thực! Vui lòng đăng nhập lại.');
    }
  }

  redirectToLogin(message) {
    alert(message);
    window.location.href = '/html/auth/login-register.html';
  }

  redirectToHomepage(message) {
    alert(message);
    window.location.href = '/html/common/homepage.html';
  }
}

// Khởi tạo kiểm tra xác thực
new SupplierAuthChecker();
