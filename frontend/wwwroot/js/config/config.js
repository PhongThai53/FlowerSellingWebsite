import { API_CONFIG } from "./api-config.js";

// Configuration constants for the application
export const CONFIG = {
  // API Base URL - imported from api-config.js
  API_BASE_URL: API_CONFIG.API_URL,
  // Pagination
  DEFAULT_PAGE_SIZE: 10,
  MAX_PAGE_SIZE: 50,

  // Cart settings
  CART_STORAGE_KEY: "cart_items",

  // Authentication
  AUTH_TOKEN_KEY: "auth_token",
  TOKEN_REFRESH_THRESHOLD: 300, // 5 minutes in seconds

  // UI Settings
  TOAST_DURATION: 3000,
  LOADING_DELAY: 200,

  // Product settings
  DEFAULT_PRODUCT_IMAGE: "/images/product/default-product.jpg",

  // Search settings
  SEARCH_DEBOUNCE_DELAY: 300,

  // Currency
  CURRENCY: "VND",
  CURRENCY_LOCALE: "vi-VN",
};

// Messages for user feedback
export const MESSAGES = {
  // Success messages
  SUCCESS: {
    ADD_TO_CART: "Sản phẩm đã được thêm vào giỏ hàng",
    UPDATE_CART: "Cập nhật giỏ hàng thành công",
    REMOVE_FROM_CART: "Đã xóa sản phẩm khỏi giỏ hàng",
    CLEAR_CART: "Đã xóa tất cả sản phẩm khỏi giỏ hàng",
    LOGIN: "Đăng nhập thành công",
    LOGOUT: "Đăng xuất thành công",
    REGISTER: "Đăng ký thành công",
  },

  // Error messages
  ERROR: {
    GENERIC: "Có lỗi xảy ra, vui lòng thử lại",
    NETWORK: "Lỗi kết nối mạng",
    AUTH_REQUIRED: "Vui lòng đăng nhập để thực hiện thao tác này",
    AUTH_EXPIRED: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại",
    INVALID_INPUT: "Dữ liệu nhập không hợp lệ",
    NOT_FOUND: "Không tìm thấy dữ liệu",
    PERMISSION_DENIED: "Bạn không có quyền thực hiện thao tác này",
    ADD_TO_CART_FAILED: "Không thể thêm sản phẩm vào giỏ hàng",
    UPDATE_CART_FAILED: "Không thể cập nhật giỏ hàng",
    REMOVE_FROM_CART_FAILED: "Không thể xóa sản phẩm khỏi giỏ hàng",
    CLEAR_CART_FAILED: "Không thể xóa giỏ hàng",
    LOGIN_FAILED: "Đăng nhập thất bại",
    REGISTER_FAILED: "Đăng ký thất bại",
  },

  // Warning messages
  WARNING: {
    CONFIRM_REMOVE: "Bạn có chắc chắn muốn xóa sản phẩm này?",
    CONFIRM_CLEAR_CART:
      "Bạn có chắc chắn muốn xóa tất cả sản phẩm trong giỏ hàng?",
    LOGOUT_CONFIRM: "Bạn có chắc chắn muốn đăng xuất?",
  },

  // Info messages
  INFO: {
    EMPTY_CART: "Giỏ hàng của bạn đang trống",
    LOADING: "Đang tải...",
    NO_PRODUCTS: "Không có sản phẩm nào",
    NO_RESULTS: "Không tìm thấy kết quả",
  },
};

// API Endpoints
export const ENDPOINTS = {
  // Authentication
  LOGIN: "/Auth/login",
  REGISTER: "/Auth/register",
  REFRESH_TOKEN: "/Auth/refresh",
  LOGOUT: "/Auth/logout",

  // Products
  PRODUCTS: "/Product",
  PRODUCT_DETAIL: "/Product/{id}",
  PRODUCT_CATEGORIES: "/ProductCategory/with-products",
  PRODUCT_PRICE_RANGE: "/Product/price-range",

  // Cart
  CART: "/Cart",
  CART_ITEMS: "/Cart/items",
  CART_SUMMARY: "/Cart/summary",
  CART_ADD: "/Cart/add",
  CART_UPDATE: "/Cart/items/{id}",
  CART_REMOVE: "/Cart/items/{id}",
  CART_CLEAR: "/Cart/clear",

  // User
  USER_PROFILE: "/User/profile",
  USER_UPDATE: "/User/update",

  // Orders
  ORDERS: "/Order",
  ORDER_DETAIL: "/Order/{id}",
  ORDER_CREATE: "/Order/create",
};

// Application Routes (frontend)
export const ROUTES = {
  HOME: "/html/common/homepage.html",
  SHOP: "/html/common/shop.html",
  CART: "/html/user/cart.html",
  CHECKOUT: "/html/checkout.html",
  LOGIN: "/html/auth/login-register.html",
  PROFILE: "/html/user/my-account.html",
  CONTACT: "/html/common/contact-us.html",
};

// Utility functions
export const UTILS = {
  // Format currency for Vietnamese market
  formatCurrency: (amount) => {
    return new Intl.NumberFormat("vi-VN", {
      style: "currency",
      currency: "VND",
    }).format(amount);
  },

  // Debounce function for search
  debounce: (func, wait) => {
    let timeout;
    return function executedFunction(...args) {
      const later = () => {
        clearTimeout(timeout);
        func(...args);
      };
      clearTimeout(timeout);
      timeout = setTimeout(later, wait);
    };
  },

  // Generate unique ID
  generateId: () => {
    return Date.now().toString(36) + Math.random().toString(36).substr(2);
  },

  // Validate email
  isValidEmail: (email) => {
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(email);
  },

  // Sanitize HTML
  sanitizeHtml: (str) => {
    const temp = document.createElement("div");
    temp.textContent = str;
    return temp.innerHTML;
  },
};
