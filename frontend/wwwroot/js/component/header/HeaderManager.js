/**
 * HeaderManager - Manages header functionality including cart count updates
 */
import { ApiService } from "../../services/ApiService.js";

export class HeaderManager {
  constructor() {
    this.init();
  }

  async init() {
    // Wait for DOM to be ready
    if (document.readyState === "loading") {
      document.addEventListener("DOMContentLoaded", () => this.setupHeader());
    } else {
      this.setupHeader();
    }

    // Listen for cart update events
    this.setupCartEventListeners();
  }

  async setupHeader() {
    await this.updateCartCount();
    this.setupUserMenu();
  }

  setupCartEventListeners() {
    // Listen for cart updates from any page
    document.addEventListener("cartUpdated", async (event) => {
      console.log("Cart updated event received:", event.detail);
      await this.updateCartCount();
    });

    // Listen for custom cart count update events
    document.addEventListener("updateCartCount", async () => {
      await this.updateCartCount();
    });

    // Listen for authentication changes to update user menu
    document.addEventListener("authChanged", () => {
      console.log("Auth changed event received, updating user menu");
      this.setupUserMenu();
      this.updateCartCount(); // Also update cart count after auth change
    });

    // Listen for storage changes (for when user logs in/out in another tab)
    window.addEventListener("storage", (e) => {
      if (e.key === "auth_token" || e.key === "user_data") {
        console.log("Auth data changed in another tab, updating user menu");
        this.setupUserMenu();
        this.updateCartCount();
      }
    });
  }

  async updateCartCount() {
    try {
      // Check if user is authenticated
      if (!ApiService.isAuthenticated()) {
        this.setCartCount(0);
        return;
      }

      // Get cart summary from API
      const result = await ApiService.getCartSummary();

      if (result && result.succeeded) {
        const totalItems = result.data?.total_items || 0;
        this.setCartCount(totalItems);
      } else {
        this.setCartCount(0);
      }
    } catch (error) {
      console.error("Error updating cart count:", error);

      // Handle authentication errors
      if (
        error.message.includes("401") ||
        error.message.includes("Unauthorized")
      ) {
        localStorage.removeItem("auth_token");
        this.setCartCount(0);
      }
    }
  }

  setCartCount(count) {
    // Find all cart count elements and update them
    const cartCountElements = document.querySelectorAll(
      ".notification, .cart-count, .minicart-count, [data-cart-count]"
    );

    cartCountElements.forEach((element) => {
      element.textContent = count || 0;

      // Add animation class for visual feedback
      element.classList.add("cart-count-updated");
      setTimeout(() => {
        element.classList.remove("cart-count-updated");
      }, 300);
    });

    // Update any data attributes
    document.querySelectorAll("[data-cart-count]").forEach((element) => {
      element.setAttribute("data-cart-count", count || 0);
    });

    console.log(`Cart count updated to: ${count}`);
  }

  setupUserMenu() {
    const userMenu = document.getElementById("user-menu");
    const userDropdown = document.getElementById("user-dropdown");
    const adminUserManagement = document.getElementById(
      "admin-user-management"
    );

    if (!userMenu || !userDropdown) return;

    // Get authentication data from localStorage (same as homepage-manager.js)
    const token = localStorage.getItem("auth_token");
    const userData = JSON.parse(localStorage.getItem("user_data") || "null");

    // Check if user is admin
    const isAdmin = userData && userData.roleName === "Admin";

    // Show/hide admin menu
    if (adminUserManagement) {
      adminUserManagement.style.display = isAdmin ? "block" : "none";
    }

    // Update user dropdown based on authentication status
    if (token && userData) {
      this.showAuthenticatedUserMenu(userDropdown, userData, isAdmin);
    } else {
      this.showGuestUserMenu(userDropdown);
    }
  }

  showAuthenticatedUserMenu(dropdown, user, isAdmin) {
    let userMenuItems = `
      <li><a href="/html/user/my-account.html"><i class="lnr lnr-user"></i> My Account</a></li>
    `;

    if (isAdmin) {
      userMenuItems += `
        <li><a href="/html/user/user-list.html"><i class="fa fa-users"></i> User Management</a></li>
      `;
    }

    userMenuItems += `
      <li><a href="#" id="logout-btn"><i class="lnr lnr-exit"></i> Logout</a></li>
    `;

    dropdown.innerHTML = userMenuItems;

    // Add logout event listener
    const logoutBtn = dropdown.querySelector("#logout-btn");
    if (logoutBtn) {
      logoutBtn.addEventListener("click", (e) => {
        e.preventDefault();
        this.handleLogout();
      });
    }
  }

  showGuestUserMenu(dropdown) {
    dropdown.innerHTML = `
      <li><a href="/html/auth/login-register.html">Login</a></li>
      <li><a href="/html/auth/login-register.html">Register</a></li>
    `;
  }

  async handleLogout() {
    try {
      const token = localStorage.getItem("auth_token");

      // Call server logout if token exists
      if (token) {
        await fetch("https://localhost:7062/api/Auth/logout", {
          method: "POST",
          headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "application/json",
          },
        });
      }
    } catch (error) {
      console.error("Error during server logout:", error);
    } finally {
      // Clear authentication data regardless of server response
      localStorage.removeItem("auth_token");
      localStorage.removeItem("user_data");

      // Reset cart count
      this.setCartCount(0);

      // Update user menu
      this.setupUserMenu();

      // Show success message
      this.showToast("You have been logged out.", "success");

      // Redirect to home if on user pages
      setTimeout(() => {
        if (window.location.pathname.includes("/user/")) {
          window.location.href = "/html/common/homepage.html";
        }
      }, 1000);
    }
  }

  showToast(message, type = "success") {
    const toast = document.createElement("div");
    toast.className = `toast toast-${type}`;
    toast.innerHTML = `
      <div class="toast-content">
        <i class="fa fa-${
          type === "success" ? "check" : "exclamation-triangle"
        }"></i>
        <span>${message}</span>
      </div>
    `;

    document.body.appendChild(toast);

    setTimeout(() => {
      toast.classList.add("show");
    }, 100);

    setTimeout(() => {
      toast.remove();
    }, 3000);
  }

  // Public method to force cart count update
  async forceUpdateCartCount() {
    await this.updateCartCount();
  }

  // Public method to increment cart count locally (for immediate feedback)
  incrementCartCount(amount = 1) {
    const cartCountElements = document.querySelectorAll(
      ".notification, .cart-count, .minicart-count"
    );

    cartCountElements.forEach((element) => {
      const currentCount = parseInt(element.textContent) || 0;
      const newCount = currentCount + amount;
      element.textContent = newCount;

      // Add animation
      element.classList.add("cart-count-updated");
      setTimeout(() => {
        element.classList.remove("cart-count-updated");
      }, 300);
    });
  }
}

// Create global instance
let headerManagerInstance = null;

// Initialize header manager when DOM is ready
document.addEventListener("DOMContentLoaded", () => {
  if (!headerManagerInstance) {
    headerManagerInstance = new HeaderManager();

    // Make it globally accessible
    window.HeaderManager = headerManagerInstance;
  }
});

// Also initialize on window load as fallback
window.addEventListener("load", () => {
  if (!headerManagerInstance) {
    headerManagerInstance = new HeaderManager();
    window.HeaderManager = headerManagerInstance;
  }
});

export default HeaderManager;
