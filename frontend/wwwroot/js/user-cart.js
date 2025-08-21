import { ApiService } from "./services/ApiService.js";
import { CartManager } from "./component/cart/CartManager.js";
import { Pagination } from "./component/pagination/Pagination.js";

class CartPageManager {
  constructor() {
    this.currentPage = 1;
    this.pageSize = 10;
    this.cartItems = [];
    this.cartSummary = { totalItems: 0, totalAmount: 0, cartId: 0 };

    // Simple initialization
    this.init();
  }

  async init() {
    // Load header and footer
    await this.loadComponents();

    // Check authentication before loading cart data
    await this.checkAuthenticationAndLoadCart();
    this.setupEventListeners();
  }

  async checkAuthenticationAndLoadCart() {
    try {
      // Simple token check with debug
      const token = localStorage.getItem("auth_token");
      console.log("CART DEBUG: Token exists:", !!token);
      console.log("CART DEBUG: Token length:", token ? token.length : 0);

      if (!token) {
        console.log("CART DEBUG: No token found, showing login prompt");
        this.showAuthenticationError();
        return;
      }

      console.log("CART DEBUG: Token found, loading cart data");
      // User has token, load cart data
      await this.loadCartData();
    } catch (error) {
      console.error("Error checking authentication:", error);
      this.showAuthenticationError();
    }
  }

  showAuthenticationError() {
    // Hide all cart content and show authentication error
    this.hideCartContent();
    this.hideLoading();

    // Show error in empty state container
    const emptyContainer = document.getElementById("cart-empty");
    if (emptyContainer) {
      emptyContainer.innerHTML = `
        <div class="text-center p-5">
          <i class="fa fa-user-slash fa-3x text-danger mb-3"></i>
          <h4>Vui lòng đăng nhập</h4>
          <p>Bạn cần đăng nhập để xem giỏ hàng của mình.</p>
          <a href="/html/auth/login-register.html" class="btn btn-primary">Đăng nhập</a>
        </div>
      `;
      emptyContainer.style.display = "block";
    }
  }

  async loadComponents() {
    try {
      // Load header
      const headerResponse = await fetch("../components/header.html");
      const headerHTML = await headerResponse.text();
      document.getElementById("header-container").innerHTML = headerHTML;

      // Load footer
      const footerResponse = await fetch("../components/footer.html");
      const footerHTML = await footerResponse.text();
      document.getElementById("footer-container").innerHTML = footerHTML;
    } catch (error) {
      console.error("Error loading components:", error);
    }
  }

  setupEventListeners() {
    // Quantity update handlers
    document.addEventListener("change", (e) => {
      if (e.target.classList.contains("cart-quantity-input")) {
        const cartItemId = e.target.getAttribute("data-cart-item-id");
        const quantity = parseInt(e.target.value) || 1;
        this.updateCartItemQuantity(cartItemId, quantity);
      }
    });

    // Quantity control buttons
    document.addEventListener("click", (e) => {
      if (e.target.classList.contains("quantity-minus")) {
        e.preventDefault();
        const input = e.target.nextElementSibling;
        const currentValue = parseInt(input.value) || 1;
        if (currentValue > 1) {
          input.value = currentValue - 1;
          input.dispatchEvent(new Event("change"));
        }
      }

      if (e.target.classList.contains("quantity-plus")) {
        e.preventDefault();
        const input = e.target.previousElementSibling;
        const currentValue = parseInt(input.value) || 1;
        input.value = currentValue + 1;
        input.dispatchEvent(new Event("change"));
      }

      // Remove item buttons
      const removeButton = e.target.closest(".remove-cart-item");
      if (removeButton) {
        e.preventDefault();
        const cartItemId = removeButton.getAttribute("data-cart-item-id");
        this.removeCartItem(cartItemId);
      }

      // Clear cart button
      const clearCartButton = e.target.closest(".clear-cart-btn");
      if (clearCartButton) {
        e.preventDefault();
        this.clearCart();
      }
    });

    // Pagination
    document.addEventListener("click", (e) => {
      if (e.target.classList.contains("page-link")) {
        e.preventDefault();
        const page = parseInt(e.target.getAttribute("data-page"));
        if (page && page !== this.currentPage) {
          this.currentPage = page;
          this.loadCartData();
        }
      }
    });
  }

  async loadCartData() {
    this.showLoading();

    try {
      // Load cart items with pagination
      const itemsResult = await ApiService.getCartItems(
        this.currentPage,
        this.pageSize
      );

      if (itemsResult.succeeded) {
        this.cartItems = itemsResult.data.cart_items;
        this.cartSummary = itemsResult.data.cart_summary;

        if (this.cartItems.length === 0 && this.currentPage === 1) {
          this.showEmptyState();
        } else {
          this.renderCartItems();
          this.renderPagination(itemsResult.data);
          this.updateCartSummary();
          this.showCartContent();
        }
      } else {
        throw new Error(itemsResult.message || "Không thể tải giỏ hàng");
      }
    } catch (error) {
      console.error("Error loading cart:", error);

      // Handle authentication errors specifically
      if (
        error.message.includes("401") ||
        error.message.includes("Unauthorized")
      ) {
        console.log("Authentication error in loadCartData, clearing token");
        localStorage.removeItem("auth_token");
        this.showAuthenticationError();
      } else {
        this.showError("Có lỗi xảy ra khi tải giỏ hàng");
      }
    }
  }

  renderCartItems() {
    const container = document.getElementById("cart-items-container");

    if (this.cartItems.length === 0) {
      container.innerHTML =
        '<tr><td colspan="6" class="text-center">Không có sản phẩm nào trong giỏ hàng</td></tr>';
      return;
    }

    container.innerHTML = this.cartItems
      .map(
        (item) => `
            <tr data-cart-item-id="${item.id}">
                <td class="pro-thumbnail">
                    <a href="#">
                        <img class="img-fluid" 
                             src="${
                               item.product_image ||
                               "../../images/product/default-product.jpg"
                             }" 
                             alt="${item.product_name}" 
                             style="width: 80px; height: 80px; object-fit: cover;" />
                    </a>
                </td>
                <td class="pro-title">
                    <a href="${item.product_url || "#"}">${
          item.product_name
        }</a>
                </td>
                <td class="pro-price">
                    <span>${this.formatCurrency(item.unit_price)}</span>
                </td>
                <td class="pro-quantity">
                    <div class="quantity-controls">
                        <button type="button" class="quantity-minus">-</button>
                        <input type="number" 
                               class="cart-quantity-input" 
                               value="${item.quantity}" 
                               min="1"
                               data-cart-item-id="${item.id}">
                        <button type="button" class="quantity-plus">+</button>
                    </div>
                </td>
                <td class="pro-subtotal">
                    <span>${this.formatCurrency(item.line_total)}</span>
                </td>
                <td class="pro-remove">
                    <button class="btn-remove-item remove-cart-item" 
                            data-cart-item-id="${item.id}">
                        <i class="fa fa-trash-o"></i>
                    </button>
                </td>
            </tr>
        `
      )
      .join("");
  }

  renderPagination(data) {
    const paginationContainer = document.getElementById("cart-pagination");

    if (data.total_pages <= 1) {
      paginationContainer.innerHTML = "";
      return;
    }

    const pagination = new Pagination({
      currentPage: data.page,
      totalPages: data.total_pages,
      onPageChange: (page) => {
        this.currentPage = page;
        this.loadCartData();
      },
    });

    paginationContainer.innerHTML = pagination.render();
  }

  updateCartSummary() {
    // Update header elements
    document.getElementById("cart-total-items").textContent =
      this.cartSummary.total_items;
    document.getElementById("cart-total-amount").textContent =
      this.formatCurrency(this.cartSummary.total_amount);

    // Update sidebar summary
    const subtotal = this.cartSummary.total_amount;
    const total = subtotal; // Total is just the product total, no shipping

    document.getElementById("subtotal-amount").textContent =
      this.formatCurrency(subtotal);
    document.getElementById("final-total-amount").textContent =
      this.formatCurrency(total);
  }

  async updateCartItemQuantity(cartItemId, quantity) {
    try {
      const result = await ApiService.updateCartItem(cartItemId, quantity);

      if (result.succeeded) {
        this.showToast("Cập nhật số lượng thành công", "success");
        await this.loadCartData(); // Reload to get updated totals
      } else {
        throw new Error(result.message || "Không thể cập nhật số lượng");
      }
    } catch (error) {
      console.error("Error updating quantity:", error);

      // Handle authentication errors
      if (
        error.message.includes("401") ||
        error.message.includes("Unauthorized")
      ) {
        localStorage.removeItem("auth_token");
        this.showAuthenticationError();
      } else {
        this.showToast("Có lỗi xảy ra khi cập nhật số lượng", "error");
        // Reload to revert changes
        await this.loadCartData();
      }
    }
  }

  async removeCartItem(cartItemId) {
    if (!confirm("Bạn có chắc chắn muốn xóa sản phẩm này khỏi giỏ hàng?")) {
      return;
    }

    try {
      const result = await ApiService.removeCartItem(cartItemId);

      if (result.succeeded) {
        this.showToast("Đã xóa sản phẩm khỏi giỏ hàng", "success");
        await this.loadCartData();
      } else {
        throw new Error(result.message || "Không thể xóa sản phẩm");
      }
    } catch (error) {
      console.error("Error removing item:", error);

      // Handle authentication errors
      if (
        error.message.includes("401") ||
        error.message.includes("Unauthorized")
      ) {
        localStorage.removeItem("auth_token");
        this.showAuthenticationError();
      } else {
        this.showToast("Có lỗi xảy ra khi xóa sản phẩm", "error");
      }
    }
  }

  async clearCart() {
    if (!confirm("Bạn có chắc chắn muốn xóa tất cả sản phẩm khỏi giỏ hàng?")) {
      return;
    }

    try {
      const result = await ApiService.clearCart();

      if (result.succeeded) {
        this.showToast("Đã xóa tất cả sản phẩm khỏi giỏ hàng", "success");
        await this.loadCartData();
      } else {
        throw new Error(result.message || "Không thể xóa giỏ hàng");
      }
    } catch (error) {
      console.error("Error clearing cart:", error);

      // Handle authentication errors
      if (
        error.message.includes("401") ||
        error.message.includes("Unauthorized")
      ) {
        localStorage.removeItem("auth_token");
        this.showAuthenticationError();
      } else {
        this.showToast("Có lỗi xảy ra khi xóa giỏ hàng", "error");
      }
    }
  }

  showLoading() {
    document.getElementById("cart-loading").style.display = "block";
    document.getElementById("cart-empty").style.display = "none";
    document.getElementById("cart-content").style.display = "none";
  }

  showEmptyState() {
    document.getElementById("cart-loading").style.display = "none";
    document.getElementById("cart-empty").style.display = "block";
    document.getElementById("cart-content").style.display = "none";
  }

  showCartContent() {
    document.getElementById("cart-loading").style.display = "none";
    document.getElementById("cart-empty").style.display = "none";
    document.getElementById("cart-content").style.display = "block";
  }

  hideCartContent() {
    document.getElementById("cart-content").style.display = "none";
  }

  hideLoading() {
    document.getElementById("cart-loading").style.display = "none";
  }

  showError(message) {
    this.showToast(message, "error");
    this.showEmptyState();
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

  formatCurrency(amount) {
    return new Intl.NumberFormat("vi-VN", {
      style: "currency",
      currency: "VND",
    }).format(amount);
  }
}

// Initialize cart page when DOM is loaded
document.addEventListener("DOMContentLoaded", () => {
  new CartPageManager();
});
