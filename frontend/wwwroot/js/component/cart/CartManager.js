import { ApiService } from "../../services/ApiService.js";
import { MESSAGES } from "../../config/config.js";

export class CartManager {
  constructor() {
    this.cartItems = [];
    this.cartSummary = { totalItems: 0, totalAmount: 0, cartId: 0 };
    this.setupEventListeners();
    this.loadCartData();
    // Prevent concurrent adds per product to avoid race conditions
    this.inFlightByProduct = new Map();
  }

  setupEventListeners() {
    // Only handle cart-specific buttons, not general add-to-cart buttons
    // General add-to-cart buttons are handled by individual page scripts (shop.js, homepage.js) to prevent conflicts
    document.addEventListener("click", (e) => {
      // Handle only cart page specific buttons or product detail pages
      const isCartPage = window.location.pathname.includes("cart");
      const isProductDetailPage =
        window.location.pathname.includes("product-details");

      if (
        e.target.closest(".add-to-cart-btn") &&
        (isCartPage || isProductDetailPage)
      ) {
        e.preventDefault();
        const btn = e.target.closest(".add-to-cart-btn");
        const productId = btn.getAttribute("data-product-id");
        const quantity = parseInt(btn.getAttribute("data-quantity")) || 1;
        this.addToCart(productId, quantity);
      }

      if (e.target.closest(".update-cart-item")) {
        e.preventDefault();
        const btn = e.target.closest(".update-cart-item");
        const cartItemId = btn.getAttribute("data-cart-item-id");
        const quantityInput = btn
          .closest("tr")
          .querySelector(".cart-quantity-input");
        const quantity = parseInt(quantityInput.value) || 1;
        this.updateCartItem(cartItemId, quantity);
      }

      if (e.target.closest(".remove-cart-item")) {
        e.preventDefault();
        const btn = e.target.closest(".remove-cart-item");
        const cartItemId = btn.getAttribute("data-cart-item-id");
        this.removeCartItem(cartItemId);
      }

      if (e.target.closest(".clear-cart-btn")) {
        e.preventDefault();
        this.clearCart();
      }

      // Checkout button
      const checkoutButton = e.target.closest(".checkout-btn");
      if (checkoutButton) {
        e.preventDefault();
        this.handleCheckoutClick();
      }
    });
  }

  async loadCartData() {
    try {
      // Check if user is authenticated
      if (!ApiService.isAuthenticated()) {
        // User not logged in or token expired, just update count to 0
        this.cartSummary = { totalItems: 0, totalAmount: 0, cartId: 0 };
        this.updateCartCount();
        return;
      }

      const result = await ApiService.getCartSummary();
      if (result.succeeded) {
        this.cartSummary = result.data;
        this.updateCartCount();
      }
    } catch (error) {
      console.error("Error loading cart data:", error);

      // Handle authentication errors
      if (
        error.message.includes("401") ||
        error.message.includes("Unauthorized")
      ) {
        // Clear invalid token and reset cart
        localStorage.removeItem("auth_token");
        this.cartSummary = { totalItems: 0, totalAmount: 0, cartId: 0 };
        this.updateCartCount();
      }
    }
  }

  // Check if adding to cart would exceed stock
  // Đã bỏ stock validation - function này không còn cần thiết
  // async checkStockLimit(productId, quantityToAdd = 1) { ... }

  async addToCart(productId, quantity = 1) {
    try {
      // Check if user is authenticated
      if (!ApiService.isAuthenticated()) {
        this.showErrorNotification(
          "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng"
        );
        // Clear invalid token
        localStorage.removeItem("auth_token");
        // Redirect to login page
        setTimeout(() => {
          window.location.href = "/html/auth/login-register.html";
        }, 2000);
        return false;
      }

      // Debounce concurrent adds for this product
      if (this.inFlightByProduct.get(productId)) {
        return false;
      }
      this.inFlightByProduct.set(productId, true);

      // Check supplier availability before adding to cart
      try {
        // Determine current quantity in cart for this product
        let currentQtyInCart = 0;
        try {
          const itemsRes = await ApiService.getCartItems(1, 100);
          const items =
            itemsRes?.data?.cart_items || itemsRes?.data?.cartItems || [];
          const existing = items.find(
            (x) => x.product_id === parseInt(productId)
          );
          currentQtyInCart = existing ? parseInt(existing.quantity) || 0 : 0;
        } catch (_) {}

        const desiredTotalQty = currentQtyInCart + quantity;

        const availabilityResponse = await fetch(
          `https://localhost:7062/api/Product/check-availability/${productId}?quantity=${desiredTotalQty}`
        );

        if (!availabilityResponse.ok) {
          this.showErrorNotification(
            "Không thể kiểm tra khả năng cung cấp sản phẩm"
          );
          return false;
        }

        const availabilityResult = await availabilityResponse.json();
        if (!availabilityResult.succeeded) {
          this.showErrorNotification("Lỗi kiểm tra khả năng cung cấp sản phẩm");
          return false;
        }

        const availability = availabilityResult.data;
        if (!availability.isAvailable) {
          this.showErrorNotification(availability.message);
          return false;
        }
      } catch (error) {
        console.error("Error checking availability:", error);
        this.showErrorNotification(
          "Không thể kiểm tra khả năng cung cấp sản phẩm"
        );
        return false;
      }

      // Show loading state on button
      this.setButtonLoading(productId, true);

      // Call API to add to cart
      const result = await ApiService.addToCart(productId, quantity);

      if (result.succeeded) {
        const product = this.findProductById(productId);
        const productName = product ? product.name : "Sản phẩm";

        // Only show notification if we're on cart page or product details page
        // Homepage and shop pages handle their own notifications
        const isCartPage = window.location.pathname.includes("cart");
        const isProductDetailPage =
          window.location.pathname.includes("product-details");

        if (isCartPage || isProductDetailPage) {
          this.showSuccessNotification(
            `${productName} đã được thêm vào giỏ hàng`
          );
        }

        await this.loadCartData(); // Reload cart data
        this.dispatchCartUpdateEvent(productId, quantity);

        // Update header cart count immediately
        this.updateHeaderCartCount();

        // Trigger product re-render to prevent disappearing
        this.triggerProductRerender();
        return true;
      } else {
        this.showErrorNotification(
          result.message || "Không thể thêm vào giỏ hàng"
        );
        return false;
      }
    } catch (error) {
      console.error("Error adding to cart:", error);

      // Handle authentication errors
      if (
        error.message.includes("401") ||
        error.message.includes("Unauthorized")
      ) {
        this.showErrorNotification(
          "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại"
        );
        // Clear invalid token
        localStorage.removeItem("auth_token");
        setTimeout(() => {
          window.location.href = "/html/auth/login-register.html";
        }, 2000);
      } else {
        this.showErrorNotification("Có lỗi xảy ra khi thêm vào giỏ hàng");
      }
      return false;
    } finally {
      this.setButtonLoading(productId, false);
      this.inFlightByProduct.delete(productId);
    }
  }

  // Check if updating cart item would exceed stock
  // Đã bỏ stock validation - function này không còn cần thiết
  // async checkCartUpdateStockLimit(cartItemId, newQuantity) { ... }

  async updateCartItem(cartItemId, quantity) {
    try {
      // Check supplier availability before updating cart
      const cartItem = this.cartItems.find(
        (item) => item.id === parseInt(cartItemId)
      );
      if (cartItem) {
        try {
          const availabilityResponse = await fetch(
            `https://localhost:7062/api/Product/check-availability/${cartItem.product_id}?quantity=${quantity}`
          );

          if (!availabilityResponse.ok) {
            this.showErrorNotification(
              "Không thể kiểm tra khả năng cung cấp sản phẩm"
            );
            return;
          }

          const availabilityResult = await availabilityResponse.json();
          if (!availabilityResult.succeeded) {
            this.showErrorNotification(
              "Lỗi kiểm tra khả năng cung cấp sản phẩm"
            );
            return;
          }

          const availability = availabilityResult.data;
          if (!availability.isAvailable) {
            this.showErrorNotification(availability.message);
            return;
          }
        } catch (error) {
          console.error("Error checking availability:", error);
          this.showErrorNotification(
            "Không thể kiểm tra khả năng cung cấp sản phẩm"
          );
          return;
        }
      }

      const result = await ApiService.updateCartItem(cartItemId, quantity);

      if (result.succeeded) {
        this.showSuccessNotification("Cập nhật giỏ hàng thành công");
        await this.loadCartData();
        this.refreshCartPage();
      } else {
        throw new Error(result.message || "Không thể cập nhật giỏ hàng");
      }
    } catch (error) {
      console.error("Error updating cart item:", error);
      this.showErrorNotification("Có lỗi xảy ra khi cập nhật giỏ hàng");
    }
  }

  async removeCartItem(cartItemId) {
    try {
      const result = await ApiService.removeCartItem(cartItemId);

      if (result.succeeded) {
        this.showSuccessNotification("Đã xóa sản phẩm khỏi giỏ hàng");
        await this.loadCartData();
        this.refreshCartPage();
      } else {
        throw new Error(result.message || "Không thể xóa sản phẩm");
      }
    } catch (error) {
      console.error("Error removing cart item:", error);
      this.showErrorNotification("Có lỗi xảy ra khi xóa sản phẩm");
    }
  }

  async clearCart() {
    if (!confirm("Bạn có chắc chắn muốn xóa tất cả sản phẩm trong giỏ hàng?")) {
      return;
    }

    try {
      const result = await ApiService.clearCart();

      if (result.succeeded) {
        this.showSuccessNotification("Đã xóa tất cả sản phẩm trong giỏ hàng");
        await this.loadCartData();
        this.refreshCartPage();
      } else {
        throw new Error(result.message || "Không thể xóa giỏ hàng");
      }
    } catch (error) {
      console.error("Error clearing cart:", error);
      this.showErrorNotification("Có lỗi xảy ra khi xóa giỏ hàng");
    }
  }

  setButtonLoading(productId, isLoading) {
    // Only affect buttons on cart-related pages or product detail pages to avoid conflicts with shop.js/homepage.js
    const isCartPage = window.location.pathname.includes("cart");
    const isProductDetailPage =
      window.location.pathname.includes("product-details");

    if (isCartPage || isProductDetailPage) {
      const btn = document.querySelector(`[data-product-id="${productId}"]`);
      if (btn) {
        if (isLoading) {
          btn.disabled = true;
          btn.innerHTML = '<i class="fa fa-spinner fa-spin"></i>';
        } else {
          btn.disabled = false;
          btn.innerHTML = '<i class="lnr lnr-cart"></i>';
        }
      }
    }
  }

  findProductById(productId) {
    return window.productsData?.find((item) => item.id == productId);
  }

  showSuccessNotification(message) {
    // Có thể thay thế bằng toast notification
    this.createToast(message, "success");
  }

  showErrorNotification(message) {
    this.createToast(message, "error");
  }

  createToast(message, type) {
    // Simple toast implementation
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

  refreshCartPage() {
    // If we're on the cart page, refresh the cart items
    if (window.location.pathname.includes("cart")) {
      window.location.reload();
    }
  }

  updateCartCount() {
    // Update cart icon count in header
    const cartCountElements = document.querySelectorAll(
      ".cart-count, .notification"
    );
    cartCountElements.forEach((element) => {
      element.textContent = this.cartSummary.totalItems || 0;
    });
  }

  dispatchCartUpdateEvent(productId, quantity) {
    const event = new CustomEvent("cartUpdated", {
      detail: { productId, quantity, timestamp: Date.now() },
    });
    document.dispatchEvent(event);
  }

  triggerProductRerender() {
    // Check if there's a global ProductPage instance via ProductPageAPI
    const productPage = window.ProductPageAPI?.getProductPage();
    if (productPage && typeof productPage.renderProducts === "function") {
      console.log("DEBUG: Triggering product re-render via ProductPageAPI");
      setTimeout(() => {
        productPage.renderProducts();
      }, 100);
    } else {
      // Fallback: dispatch custom event for product re-render
      console.log(
        "DEBUG: ProductPageAPI not available, dispatching productRerender event"
      );
      const event = new CustomEvent("productRerender", {
        detail: { timestamp: Date.now() },
      });
      document.dispatchEvent(event);
    }
  }

  updateHeaderCartCount() {
    // Update header cart count via HeaderManager if available
    if (
      window.HeaderManager &&
      typeof window.HeaderManager.forceUpdateCartCount === "function"
    ) {
      window.HeaderManager.forceUpdateCartCount();
    } else {
      // Fallback: dispatch event for header to listen to
      const event = new CustomEvent("updateCartCount", {
        detail: {
          totalItems:
            this.cartSummary.total_items || this.cartSummary.totalItems || 0,
          timestamp: Date.now(),
        },
      });
      document.dispatchEvent(event);
    }
  }

  // Validate cart items before checkout
  async validateCartForCheckout() {
    if (!this.cartItems || this.cartItems.length === 0) {
      this.showErrorNotification(
        "Giỏ hàng trống, không thể tiến hành thanh toán"
      );
      return false;
    }

    // Bỏ stock validation - cho phép checkout bất kỳ số lượng nào
    // Tất cả validation đã pass
    return true;
  }

  // Handle checkout button click
  handleCheckoutClick() {
    const checkoutBtn = document.querySelector(".checkout-btn");
    if (checkoutBtn) {
      // Show loading state
      checkoutBtn.disabled = true;
      checkoutBtn.innerHTML =
        '<i class="fa fa-spinner fa-spin"></i> Đang kiểm tra...';
    }

    // Validate cart before proceeding to checkout
    this.validateCartForCheckout()
      .then((isValid) => {
        if (isValid) {
          // Proceed to checkout
          window.location.href = "/html/Payment/checkout.html";
        } else {
          // Reset button state if validation failed
          if (checkoutBtn) {
            checkoutBtn.disabled = false;
            checkoutBtn.innerHTML = "Proceed To Checkout";
          }
        }
      })
      .catch((error) => {
        console.error("Checkout validation error:", error);
        this.showErrorNotification("Có lỗi xảy ra khi kiểm tra giỏ hàng");
        // Reset button state on error
        if (checkoutBtn) {
          checkoutBtn.disabled = false;
          checkoutBtn.innerHTML = "Proceed To Checkout";
        }
      });
  }
}
