/**
 * HeaderManager - Manages header functionality including cart count updates
 */
import { ApiService } from "../../services/ApiService.js";

export class HeaderManager {
  constructor() {
    this.init();
  }

  async init() {
    console.log(
      "HeaderManager init() called on page:",
      window.location.pathname
    );
    // Wait for DOM to be ready
    if (document.readyState === "loading") {
      document.addEventListener("DOMContentLoaded", () => this.setupHeader());
    } else {
      this.setupHeader();
    }

    // Listen for cart update events
    this.setupCartEventListeners();

    // Listen for HTMX events to handle dynamically loaded headers
    this.setupHtmxEventListeners();

    // Add fallback polling mechanism
    this.setupFallbackPolling();
  }

  async setupHeader() {
    console.log("setupHeader() called on page:", window.location.pathname);
    await this.updateCartCount();
    this.setupUserMenu();
  }

  setupCartEventListeners() {
    // Listen for cart updates from any page
    document.addEventListener("cartUpdated", async (event) => {
      await this.updateCartCount();
    });

    // Listen for custom cart count update events
    document.addEventListener("updateCartCount", async () => {
      await this.updateCartCount();
    });

    // Listen for authentication changes to update user menu
    document.addEventListener("authChanged", () => {
      this.setupUserMenu();
      this.updateCartCount(); // Also update cart count after auth change
    });

    // Listen for storage changes (for when user logs in/out in another tab)
    window.addEventListener("storage", (e) => {
      if (e.key === "auth_token" || e.key === "user_data") {
        this.setupUserMenu();
        this.updateCartCount();
      }
    });
  }

  setupHtmxEventListeners() {
    // Listen for HTMX afterSwap events
    document.addEventListener("htmx:afterSwap", (event) => {
      try {
        // Check if event and target exist
        if (!event || !event.detail || !event.detail.target) {
          return;
        }

        const target = event.detail.target;

        // Check if target has the required properties before accessing them
        if (
          (target.id && target.id === "header-placeholder") ||
          target.closest("header") ||
          target.querySelector("header")
        ) {
          setTimeout(() => {
            this.setupUserMenu();
            this.updateCartCount();
          }, 100); // Small delay to ensure DOM is fully updated
        }
      } catch (error) {
        console.error("Error in HTMX afterSwap event handler:", error);
      }
    });

    // Listen for HTMX load events
    document.addEventListener("htmx:load", (event) => {
      try {
        // Check if event and target exist
        if (!event || !event.detail || !event.detail.target) {
          return;
        }

        const eventTarget = event.detail.target;

        // Check if target has the required properties before accessing them
        if (
          (eventTarget.id && eventTarget.id === "header-placeholder") ||
          eventTarget.closest("header") ||
          eventTarget.querySelector("header")
        ) {
          setTimeout(() => {
            this.setupUserMenu();
            this.updateCartCount();
          }, 100);
        }
      } catch (error) {
        console.error("Error in HTMX load event handler:", error);
      }
    });

    // Add MutationObserver to watch for header elements being added
    this.setupMutationObserver();
  }

  setupMutationObserver() {
    const observer = new MutationObserver((mutations) => {
      mutations.forEach((mutation) => {
        if (mutation.type === "childList") {
          mutation.addedNodes.forEach((node) => {
            if (node.nodeType === Node.ELEMENT_NODE) {
              // Check if the added node is a header or contains header elements
              if (
                node.tagName === "HEADER" ||
                (node.querySelector && node.querySelector("header")) ||
                (node.querySelector && node.querySelector("#user-menu"))
              ) {
                setTimeout(() => {
                  this.setupUserMenu();
                  this.updateCartCount();
                }, 100);
              }
            }
          });
        }
      });
    });

    // Start observing the document body for changes
    observer.observe(document.body, {
      childList: true,
      subtree: true,
    });
  }

  setupFallbackPolling() {
    let attempts = 0;
    const maxAttempts = 20; // Try for 20 seconds

    const pollForHeader = () => {
      attempts++;

      const userMenu = document.getElementById("user-menu");
      const userDropdown = document.getElementById("user-dropdown");

      if (userMenu && userDropdown) {
        this.setupUserMenu();
        this.updateCartCount();
        return; // Stop polling
      }

      if (attempts < maxAttempts) {
        setTimeout(pollForHeader, 500); // Try again in 0.5 seconds
      }
    };

    // Start polling after a longer delay to ensure HTMX has loaded the header
    setTimeout(pollForHeader, 2000);
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

      if (result && result.succeeded && result.data) {
        const totalItems =
          result.data.total_items || result.data.totalItems || 0;
        this.setCartCount(totalItems);
      } else {
        this.setCartCount(0);
      }
    } catch (error) {
      console.error("Error updating cart count:", error);

      // Handle authentication errors
      if (
        error.message &&
        (error.message.includes("401") ||
          error.message.includes("Unauthorized"))
      ) {
        localStorage.removeItem("auth_token");
        this.setCartCount(0);
      } else {
        // For other errors, just set count to 0
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
  }

  setupUserMenu() {
    console.log("setupUserMenu() called on page:", window.location.pathname);
    const userMenu = document.getElementById("user-menu");
    const userDropdown = document.getElementById("user-dropdown");
    const adminUserManagement = document.getElementById(
      "admin-user-management"
    );
    const adminProductManagement = document.getElementById(
      "admin-product-management"
    );
    const mainMenuRoot = document.querySelector("nav.desktop-menu > ul");
    const shopMenuItem = document
      .querySelector('nav.desktop-menu a[href$="/html/common/shop.html"]')
      ?.closest("li");
    const contactMenuItem = document
      .querySelector('nav.desktop-menu a[href$="/html/common/contact-us.html"]')
      ?.closest("li");
    const cartIconItem = document
      .querySelector("a.minicart-btn")
      ?.closest("li");

    console.log("User menu elements found:", {
      userMenu: !!userMenu,
      userDropdown: !!userDropdown,
      adminUserManagement: !!adminUserManagement,
      adminProductManagement: !!adminProductManagement,
    });

    if (!userMenu || !userDropdown) {
      console.error("User menu or dropdown not found!");
      return;
    }

    // Get authentication data from localStorage (same as homepage-manager.js)
    const token = localStorage.getItem("auth_token");
    const userData = JSON.parse(localStorage.getItem("user_data") || "null");

    // Check if user is admin
    const isAdmin = userData && userData.roleName === "Admin";

    // Show/hide admin menu
    if (adminUserManagement) {
      adminUserManagement.style.display = isAdmin ? "block" : "none";
    }
    if (adminProductManagement) {
      adminProductManagement.style.display = isAdmin ? "block" : "none";
    }

    // If Admin: hide all other main menu items except admin menus and user dropdown
    if (isAdmin && mainMenuRoot) {
      // Hide Home, Shop, Contact, and any other items not admin*
      Array.from(mainMenuRoot.children).forEach((li) => {
        const isAdminItem =
          li.id === "admin-user-management" ||
          li.id === "admin-product-management";
        if (!isAdminItem) {
          li.style.display = "none";
        }
      });
      // Also hide cart icon if present
      if (cartIconItem) {
        cartIconItem.style.display = "none";
      }
    } else if (mainMenuRoot) {
      // Non-admin: ensure normal items visible and admin items hidden (already handled above)
      Array.from(mainMenuRoot.children).forEach((li) => {
        if (
          li.id !== "admin-user-management" &&
          li.id !== "admin-product-management"
        ) {
          li.style.display = "";
        }
      });
      if (cartIconItem) {
        cartIconItem.style.display = "";
      }
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
        <li><a href="/html/admin/user-list.html"><i class="fa fa-users"></i> User Management</a></li>
        <li><a href="/html/product/page-list-product.html"><i class="fa fa-box"></i> Product Management</a></li>
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
    const guestMenuItems = `
      <li><a href="/html/auth/login-register.html">Login</a></li>
      <li><a href="/html/auth/login-register.html">Register</a></li>
    `;
    dropdown.innerHTML = guestMenuItems;
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

      // Notify app about auth change
      document.dispatchEvent(new Event("authChanged"));

      // Force refresh header markup to default (non-admin) state
      this.refreshHeader();

      // Show success message
      this.showToast("You have been logged out.", "success");

      // Redirect to login page after logout
      setTimeout(() => {
        window.location.href = "/html/auth/login-register.html";
      }, 1000);
    }
  }

  // Reload the header component and re-run setup
  refreshHeader() {
    try {
      const headerElement =
        document.querySelector("header.header-area") ||
        document.getElementById("header-placeholder");
      if (!headerElement) {
        return;
      }

      // Always fetch absolute header path to avoid relative issues
      fetch("/html/components/header.html")
        .then((res) => res.text())
        .then((html) => {
          const container = document.createElement("div");
          container.innerHTML = html;
          const newHeader = container.querySelector("header");
          if (newHeader && headerElement.parentNode) {
            headerElement.parentNode.replaceChild(newHeader, headerElement);
            // Ensure scripts/styles dependent behavior is restored
            this.setupHeader();
          }
        })
        .catch((e) => console.error("Failed to refresh header:", e));
    } catch (e) {
      console.error("Error refreshing header:", e);
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

// Initialize when HTMX content is loaded
document.addEventListener("htmx:afterSwap", (event) => {
  if (
    event.detail &&
    event.detail.target &&
    event.detail.target.id === "header-placeholder"
  ) {
    if (!headerManagerInstance) {
      headerManagerInstance = new HeaderManager();
      window.HeaderManager = headerManagerInstance;
    } else {
      // Force re-initialization of the header
      headerManagerInstance.setupHeader();
    }
  }
});

// Initialize when HTMX content is loaded (load event)
document.addEventListener("htmx:load", (event) => {
  if (
    event.detail &&
    event.detail.target &&
    event.detail.target.id === "header-placeholder"
  ) {
    if (!headerManagerInstance) {
      headerManagerInstance = new HeaderManager();
      window.HeaderManager = headerManagerInstance;
    } else {
      // Force re-initialization of the header
      headerManagerInstance.setupHeader();
    }
  }
});

export default HeaderManager;
