import { ApiService } from "../../services/ApiService.js";
import { MESSAGES } from "../../config/config.js";

export class CartManager {
  constructor() {
    this.cartItems = [];
    this.cartSummary = { totalItems: 0, totalAmount: 0, cartId: 0 };
    this.setupEventListeners();
    this.loadCartData();
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
  async checkStockLimit(productId, quantityToAdd = 1) {
    try {
      const stockResult = await ApiService.getProductStock(productId);
      if (stockResult && stockResult.succeeded) {
        const currentStock = stockResult.data.stock || 0;

        // Check if requested quantity exceeds stock
        if (quantityToAdd > currentStock) {
          return {
            allowed: false,
            message: `Không thể thêm ${quantityToAdd} sản phẩm. Chỉ còn ${currentStock} sản phẩm trong kho.`,
            currentStock,
          };
        }

        // Check if adding this quantity would exceed stock when combined with existing cart items
        const existingCartItem = this.cartItems.find(
          (item) =>
            item.productId && item.productId.toString() === productId.toString()
        );

        if (existingCartItem) {
          const totalQuantity = existingCartItem.quantity + quantityToAdd;
          if (totalQuantity > currentStock) {
            return {
              allowed: false,
              message: `Không thể thêm ${quantityToAdd} sản phẩm. Tổng số lượng sẽ vượt quá tồn kho (${currentStock}).`,
              currentStock,
              existingQuantity: existingCartItem.quantity,
            };
          }
        }

        return {
          allowed: true,
          currentStock,
          existingQuantity: existingCartItem ? existingCartItem.quantity : 0,
        };
      }

      return {
        allowed: true,
        currentStock: 0,
        message: "Không thể kiểm tra tồn kho, cho phép thêm vào giỏ hàng",
      };
    } catch (error) {
      console.warn("Error checking stock limit:", error);
      return {
        allowed: true,
        currentStock: 0,
        message: "Không thể kiểm tra tồn kho, cho phép thêm vào giỏ hàng",
      };
    }
  }

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
        return;
      }

      // Check current stock before adding to cart
      try {
        const stockCheck = await this.checkStockLimit(productId, quantity);
        if (!stockCheck.allowed) {
          this.showErrorNotification(stockCheck.message);
          return;
        }
      } catch (stockError) {
        console.warn(
          "Could not verify stock, proceeding with cart addition:",
          stockError
        );
        // Continue with cart addition if stock check fails
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
      } else {
        throw new Error(result.message || "Không thể thêm vào giỏ hàng");
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
    } finally {
      this.setButtonLoading(productId, false);
    }
  }

  // Check if updating cart item would exceed stock
  async checkCartUpdateStockLimit(cartItemId, newQuantity) {
    try {
      // Find the cart item
      const cartItem = this.cartItems.find(
        (item) => item.id.toString() === cartItemId.toString()
      );

      if (!cartItem || !cartItem.productId) {
        return {
          allowed: false,
          message: "Không thể tìm thấy sản phẩm trong giỏ hàng",
        };
      }

      const stockResult = await ApiService.getProductStock(cartItem.productId);
      if (stockResult && stockResult.succeeded) {
        const currentStock = stockResult.data.stock || 0;

        if (newQuantity > currentStock) {
          return {
            allowed: false,
            message: `Không thể cập nhật số lượng ${newQuantity}. Chỉ còn ${currentStock} sản phẩm trong kho.`,
            currentStock,
          };
        }

        return {
          allowed: true,
          currentStock,
        };
      }

      return {
        allowed: true,
        currentStock: 0,
        message: "Không thể kiểm tra tồn kho, cho phép cập nhật",
      };
    } catch (error) {
      console.warn("Error checking cart update stock limit:", error);
      return {
        allowed: true,
        currentStock: 0,
        message: "Không thể kiểm tra tồn kho, cho phép cập nhật",
      };
    }
  }

  async updateCartItem(cartItemId, quantity) {
    try {
      // Check stock limit before updating
      const stockCheck = await this.checkCartUpdateStockLimit(
        cartItemId,
        quantity
      );
      if (!stockCheck.allowed) {
        this.showErrorNotification(stockCheck.message);
        return;
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

    const validationErrors = [];
    let hasStockIssues = false;

    // Check each cart item against current stock
    for (const cartItem of this.cartItems) {
      try {
        const stockResult = await ApiService.getProductStock(
          cartItem.productId
        );
        if (stockResult && stockResult.succeeded) {
          const currentStock = stockResult.data.stock || 0;
          const cartQuantity = cartItem.quantity || 0;

          if (cartQuantity > currentStock) {
            hasStockIssues = true;
            validationErrors.push({
              productName: cartItem.productName || "Sản phẩm",
              requestedQuantity: cartQuantity,
              currentStock: currentStock,
              message: `${cartItem.productName}: Yêu cầu ${cartQuantity} nhưng chỉ còn ${currentStock} trong kho`,
            });
          }
        }
      } catch (error) {
        console.warn(
          `Could not validate stock for product ${cartItem.productId}:`,
          error
        );
        // Continue validation for other items
      }
    }

    // If there are stock issues, show detailed error message
    if (hasStockIssues) {
      const errorMessage = validationErrors
        .map((error) => error.message)
        .join("\n");

      this.showErrorNotification(
        `Không thể tiến hành thanh toán vì một số sản phẩm vượt quá tồn kho:\n${errorMessage}`
      );

      // Also show a more detailed alert for better visibility
      alert(
        `Không thể tiến hành thanh toán vì một số sản phẩm vượt quá tồn kho:\n\n${errorMessage}\n\nVui lòng cập nhật số lượng trong giỏ hàng.`
      );

      return false;
    }

    // All validations passed
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
