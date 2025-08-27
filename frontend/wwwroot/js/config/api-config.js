// API Configuration for the entire application
export const API_CONFIG = {
  // Base URLs
  BACKEND_HTTP: "https://localhost:7062",
  BACKEND_HTTPS: "https://localhost:7062",

  // Current active backend URL (use HTTPS for production-like environment)
  CURRENT_BACKEND: "https://localhost:7062",

  // API endpoints
  API_BASE: "/api",

  // Full API URL
  get API_URL() {
    return `${this.CURRENT_BACKEND}${this.API_BASE}`;
  },

  // Specific endpoints
  AUTH: {
    LOGIN: "/Auth/login",
    REGISTER: "/Auth/register",
    FORGOT_PASSWORD: "/Auth/forgot-password",
    RESET_PASSWORD: "/Auth/reset-password",
    REFRESH: "/Auth/refresh",
    LOGOUT: "/Auth/logout",
  },

  PRODUCTS: {
    LIST: "/Product",
    DETAIL: "/Product/{id}",
    CATEGORIES: "/ProductCategory/with-products",
    PRICE_RANGE: "/Product/price-range",
    CHECK_AVAILABILITY: "/Product/check-availability/{id}",
  },

  CART: {
    BASE: "/Cart",
    ITEMS: "/Cart/items",
    SUMMARY: "/Cart/summary",
    ADD: "/Cart/add",
    UPDATE: "/Cart/items/{id}",
    REMOVE: "/Cart/items/{id}",
    CLEAR: "/Cart/clear",
    CALCULATE: "/Cart/calculate-price",
  },

  USER: {
    PROFILE: "/User/profile",
    UPDATE: "/User/update",
  },

  ORDERS: {
    LIST: "/Order",
    DETAIL: "/Order/{id}",
    CREATE: "/Order/create",
  },
};

// Helper function to build full API URLs
export const buildApiUrl = (endpoint, params = {}) => {
  let url = `${API_CONFIG.API_URL}${endpoint}`;

  // Replace path parameters
  Object.keys(params).forEach((key) => {
    url = url.replace(`{${key}}`, params[key]);
  });

  return url;
};

// Helper function to get current backend status
export const getBackendStatus = async () => {
  try {
    const response = await fetch(`${API_CONFIG.CURRENT_BACKEND}/health`, {
      method: "GET",
      mode: "cors",
    });
    return response.ok;
  } catch (error) {
    console.error("Backend connection failed:", error);
    return false;
  }
};
