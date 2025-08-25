import { CONFIG } from "../config/config.js";

export class ApiService {
  // Helper method to check if user is authenticated
  static isAuthenticated() {
    const token = localStorage.getItem("auth_token");
    if (!token) return false;

    try {
      // Basic JWT structure check
      const parts = token.split(".");
      if (parts.length !== 3) return false;

      // Decode payload to check expiration
      const payload = JSON.parse(atob(parts[1]));
      const now = Date.now() / 1000;

      return payload.exp > now;
    } catch (error) {
      console.error("Token validation error:", error);
      return false;
    }
  }

  // Helper method to get user info from token
  static getCurrentUser() {
    const token = localStorage.getItem("auth_token");
    if (!token) return null;

    try {
      const payload = JSON.parse(atob(token.split(".")[1]));
      return {
        userId: payload.nameid || payload.sub,
        username: payload.unique_name || payload.name,
        role: payload.role,
        exp: payload.exp,
      };
    } catch (error) {
      console.error("Token decode error:", error);
      return null;
    }
  }

  static async request(endpoint, options = {}) {
    const url = `${CONFIG.API_BASE_URL}${endpoint}`;

    const fetchOptions = {
      ...options,
      headers: {
        "Content-Type": "application/json",
        ...options.headers,
      },
    };

    try {
      const response = await fetch(url, fetchOptions);

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      // Handle responses with no content
      const contentType = response.headers.get("content-type");
      if (contentType && contentType.indexOf("application/json") !== -1) {
        return response.json();
      } else {
        return; // Return undefined for non-JSON responses
      }
    } catch (error) {
      console.error(`API request failed for ${endpoint}:`, error);
      throw error;
    }
  }

  static async fetchProducts(params = {}) {
    const {
      pageNumber = 1,
      pageSize = CONFIG.DEFAULT_PAGE_SIZE,
      categoryId = "",
      sortBy = "",
      search = "",
      minPrice = null,
      maxPrice = null,
    } = params;

    let queryParams = new URLSearchParams({
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString(),
    });

    // Add optional parameters
    if (categoryId) queryParams.append("CategoryId", categoryId);
    if (sortBy && sortBy !== "default") queryParams.append("sortBy", sortBy);
    if (search) queryParams.append("search", search);

    if (minPrice !== null && minPrice !== undefined) {
      queryParams.append("min", minPrice.toString());
    }
    if (maxPrice !== null && maxPrice !== undefined) {
      queryParams.append("max", maxPrice.toString());
    }

    return this.request(`/Product?${queryParams.toString()}`);
  }

  static async fetchCategories() {
    return this.request("/ProductCategory/with-products");
  }

  static async fetchProductById(id) {
    return this.request(`/Product/${id}`);
  }

  static async getProductStock(productId) {
    return this.request(`/Product/${productId}`);
  }

  // ← New: Add method to get price range from server
  static async fetchPriceRange() {
    return this.request("/Product/price-range");
  }

  // Cart API methods
  static async getCart() {
    const token = localStorage.getItem("auth_token");
    return this.request("/Cart", {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });
  }

  static async getCartItems(page = 1, pageSize = 10) {
    const token = localStorage.getItem("auth_token");
    return this.request(`/Cart/items?page=${page}&pageSize=${pageSize}`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });
  }

  static async getCartSummary() {
    const token = localStorage.getItem("auth_token");
    return this.request("/Cart/summary", {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });
  }

  static async addToCart(productId, quantity) {
    const token = localStorage.getItem("auth_token");

    // Debug: Log token information
    console.log("Debug - Adding to cart:");
    console.log("Token exists:", !!token);
    console.log("Token length:", token ? token.length : 0);
    console.log("Product ID:", productId);
    console.log("Quantity:", quantity);

    if (!token) {
      throw new Error("No authentication token found");
    }

    return this.request("/Cart/add", {
      method: "POST",
      headers: {
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify({
        product_id: productId,
        quantity: quantity,
      }),
    });
  }

  static async updateCartItem(cartItemId, quantity) {
    const token = localStorage.getItem("auth_token");
    return this.request(`/Cart/items/${cartItemId}`, {
      method: "PUT",
      headers: {
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify({
        quantity: quantity,
      }),
    });
  }

  static async removeCartItem(cartItemId) {
    const token = localStorage.getItem("auth_token");
    return this.request(`/Cart/items/${cartItemId}`, {
      method: "DELETE",
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });
  }

  static async clearCart() {
    const token = localStorage.getItem("auth_token");
    return this.request("/Cart/clear", {
      method: "DELETE",
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });
  }

  static async calculateCartPrice() {
    const token = localStorage.getItem("auth_token");
    return this.request("/Cart/calculate-price", {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });
  }

  static async checkProductAvailability(productId, quantity = 1) {
    return this.request(
      `/Product/check-availability/${productId}?quantity=${quantity}`
    );
  }
}
