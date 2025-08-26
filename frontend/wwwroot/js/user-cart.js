import { ApiService } from "./services/ApiService.js";
import { Pagination } from "./component/pagination/Pagination.js";

class CartPageManager {
  constructor() {
    this.currentPage = 1;
    this.pageSize = 10;
    this.cartItems = [];
    this.cartSummary = { totalItems: 0, totalAmount: 0, cartId: 0 };
    this.pendingChanges = new Map(); // Track pending quantity changes
    this.calculatedPrices = null; // Store calculated prices from supplier

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
          <a href="../auth/login-register.html" class="btn" style="background-color: red; color: white;">Đăng nhập</a>
        </div>
      `;
      emptyContainer.style.display = "block";
    }
  }

  async loadComponents() {
    // Components are loaded via HTMX, so we don't need to manually load them
    // Just wait a bit for HTMX to complete loading
    return new Promise((resolve) => {
      setTimeout(resolve, 100);
    });
  }

  setupEventListeners() {
    // Prevent mouse wheel from changing quantity when focusing the input
    document.addEventListener(
      "wheel",
      (e) => {
        const input = e.target.closest(".cart-quantity-input");
        if (input && document.activeElement === input) {
          e.preventDefault();
        }
      },
      { passive: false }
    );

    // Prevent arrow up/down from changing the number directly
    document.addEventListener("keydown", (e) => {
      if (e.target.classList?.contains("cart-quantity-input")) {
        if (e.key === "ArrowUp" || e.key === "ArrowDown") {
          e.preventDefault();
        }
      }
    });

    // Quantity input handlers - just validate, don't update immediately
    document.addEventListener("change", (e) => {
      if (e.target.classList.contains("cart-quantity-input")) {
        const cartItemId = e.target.getAttribute("data-cart-item-id");
        let quantity = parseInt(e.target.value) || 1;

        // Validate quantity
        if (quantity < 0) {
          quantity = 1;
          e.target.value = 1;
          this.showToast("Số lượng không hợp lệ", "warning");
        } else if (quantity === 0) {
          // Quantity 0 means remove item from cart
          e.target.value = 1; // Reset to 1 temporarily
          this.showToast("Số lượng 0 sẽ xóa sản phẩm khỏi giỏ hàng", "info");
        } else if (quantity > 100) {
          quantity = 100;
          e.target.value = 100;
          this.showToast("Số lượng tối đa là 100", "warning");
        }

        // Check stock validation asynchronously
        // Đã bỏ stock validation - không còn cần thiết
        // this.validateQuantityAgainstStock(cartItemId, quantity, e.target);
      }
    });

    // Real-time quantity validation
    document.addEventListener("input", (e) => {
      if (e.target.classList.contains("cart-quantity-input")) {
        const cartItemId = e.target.getAttribute("data-cart-item-id");
        const quantity = parseInt(e.target.value) || 0;

        // Basic validation
        if (quantity < 0) {
          e.target.value = 0;
        } else if (quantity > 100) {
          e.target.value = 100;
        }

        // Mark as changed for real-time feedback (async)
        if (quantity > 0) {
          this.markItemAsChanged(cartItemId, quantity);
        }
      }
    });

    // Quantity control buttons
    document.addEventListener("click", (e) => {
      if (e.target.classList.contains("quantity-minus")) {
        e.preventDefault();
        const input = e.target.nextElementSibling;
        const currentValue = parseInt(input.value) || 1;
        if (currentValue > 1) {
          const newQuantity = currentValue - 1;
          input.value = newQuantity;
          this.markItemAsChanged(
            input.getAttribute("data-cart-item-id"),
            newQuantity
          );
        }
      }

      if (e.target.classList.contains("quantity-plus")) {
        e.preventDefault();
        const input = e.target.previousElementSibling;
        const currentValue = parseInt(input.value) || 1;
        const newQuantity = currentValue + 1;
        input.value = newQuantity;
        this.markItemAsChanged(
          input.getAttribute("data-cart-item-id"),
          newQuantity
        );
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

      // Update cart button
      const updateCartButton = e.target.closest(".update-cart-btn");
      if (updateCartButton) {
        e.preventDefault();
        this.updateCartWithChanges();
      }

      // Checkout button
      const checkoutButton = e.target.closest(".checkout-btn");
      if (checkoutButton) {
        e.preventDefault();
        this.handleCheckoutClick();
      }

      // Debug buttons
      const debugCartButton = e.target.closest("#debug-cart-btn");
      if (debugCartButton) {
        e.preventDefault();
        this.debugCartState();
      }

      const reloadPricesButton = e.target.closest("#reload-prices-btn");
      if (reloadPricesButton) {
        e.preventDefault();
        this.forceReloadCalculatedPrices();
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
      console.log("Loading cart data...");
      const itemsResult = await ApiService.getCartItems(
        this.currentPage,
        this.pageSize
      );
      console.log("Cart data loaded:", itemsResult);

      if (itemsResult.succeeded) {
        this.cartItems = itemsResult.data.cart_items;
        this.cartSummary = itemsResult.data.cart_summary;
        console.log("Cart items loaded:", this.cartItems);
        console.log("Cart summary loaded:", this.cartSummary);

        // Debug: Log cart items for troubleshooting
        console.log("Loaded cart items:", this.cartItems);
        console.log("Cart summary:", this.cartSummary);

        if (this.cartItems.length === 0 && this.currentPage === 1) {
          this.showEmptyState();
        } else {
          this.renderCartItems();
          this.renderPagination(itemsResult.data);
          this.updateCartSummary();
          this.showCartContent();

          // Load calculated prices after rendering cart items
          await this.loadCalculatedPrices();

          // After loading calculated prices, update cart summary with new totals
          if (this.calculatedPrices && this.calculatedPrices.totalAmount > 0) {
            this.updateCartSummaryWithCalculatedPrices();
          }
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

  async loadCalculatedPrices() {
    try {
      console.log("=== LOADING CALCULATED PRICES ===");
      console.log("Calling API endpoint: /Cart/calculate-price");

      const priceResult = await ApiService.calculateCartPrice();
      console.log("API Response received:", priceResult);

      if (priceResult.succeeded && priceResult.data) {
        this.calculatedPrices = priceResult.data;
        console.log("Calculated prices data:", this.calculatedPrices);

        // Check if we have valid data
        if (
          this.calculatedPrices.cartItems &&
          this.calculatedPrices.cartItems.length > 0
        ) {
          console.log(
            `Found ${this.calculatedPrices.cartItems.length} calculated cart items`
          );

          // Verify data structure
          this.calculatedPrices.cartItems.forEach((item, index) => {
            console.log(`Item ${index}:`, {
              cartItemId: item.cartItemId,
              productId: item.productId,
              calculatedUnitPrice: item.calculatedUnitPrice,
              lineTotal: item.lineTotal,
              priceDifference: item.priceDifference,
            });
          });

          // Update cart display with new prices
          console.log("Updating cart display with calculated prices...");
          this.updateCartWithCalculatedPrices();

          // Re-render cart items to show calculated prices immediately
          this.renderCartItems();

          // Show info about sequential allocation
          this.showToast(
            "Giá được tính dựa trên phân bổ hoa tuần tự từ nhà cung cấp rẻ nhất",
            "info"
          );
        } else {
          console.log("No calculated prices available, using original prices");
          console.log(
            "Cart items in response:",
            this.calculatedPrices.cartItems
          );
          // Show info message
          this.showToast("Sử dụng giá gốc sản phẩm", "info");
        }
      } else {
        console.warn("Failed to load calculated prices:", priceResult.message);
        console.warn("Full response:", priceResult);
        // Show user-friendly message
        this.showToast(
          `Không thể tính giá từ nhà cung cấp: ${priceResult.message}`,
          "warning"
        );
      }
    } catch (error) {
      console.error("Error loading calculated prices:", error);
      console.error("Error details:", {
        message: error.message,
        stack: error.stack,
      });
      // Show user-friendly message
      this.showToast(
        "Không thể tính giá từ nhà cung cấp, sử dụng giá gốc",
        "warning"
      );
    }
  }

  updateCartWithCalculatedPrices() {
    if (!this.calculatedPrices || !this.calculatedPrices.cartItems) {
      console.log("No calculated prices available");
      return;
    }

    console.log("Updating cart with calculated prices:", this.calculatedPrices);

    // Update each cart item with calculated prices
    this.calculatedPrices.cartItems.forEach((calculatedItem) => {
      console.log("Processing calculated item:", calculatedItem);

      // Find the corresponding cart item row
      const row = document.querySelector(
        `tr[data-cart-item-id="${calculatedItem.cartItemId}"]`
      );

      if (row) {
        console.log(
          `Found row for cart item ${calculatedItem.cartItemId}, updating prices...`
        );

        // Update unit price - show the calculated unit price
        const priceCell = row.querySelector(".pro-price span");
        if (priceCell) {
          const calculatedPrice = this.formatCurrency(
            calculatedItem.calculatedUnitPrice
          );

          // Always update with calculated price
          priceCell.textContent = calculatedPrice;
          console.log(`Updated unit price to: ${calculatedPrice}`);
        }

        // Update line total
        const subtotalCell = row.querySelector(".pro-subtotal span");
        if (subtotalCell) {
          const calculatedTotal = this.formatCurrency(calculatedItem.lineTotal);
          subtotalCell.textContent = calculatedTotal;
          console.log(`Updated line total to: ${calculatedTotal}`);
        }

        // Add visual indicator that price has been updated
        row.classList.add("price-updated");
      } else {
        console.warn(
          `Could not find row for cart item ${calculatedItem.cartItemId}`
        );
      }
    });

    // Update cart totals
    this.updateCartSummaryWithCalculatedPrices();

    // Show success message
    this.showToast("Đã cập nhật giá theo phân bổ hoa tuần tự", "success");
  }

  updateCartSummaryWithCalculatedPrices() {
    if (!this.calculatedPrices) return;

    // Update subtotal
    const subtotalElement = document.getElementById("subtotal-amount");
    if (subtotalElement) {
      subtotalElement.textContent = this.formatCurrency(
        this.calculatedPrices.subtotal
      );
    }

    // Update service fee
    const serviceFeeElement = document.getElementById("service-fee-amount");
    if (serviceFeeElement) {
      serviceFeeElement.textContent = this.formatCurrency(
        this.calculatedPrices.serviceFee
      );
    }

    // Update total amount
    const totalElement = document.getElementById("final-total-amount");
    if (totalElement) {
      totalElement.textContent = this.formatCurrency(
        this.calculatedPrices.totalAmount
      );
    }

    // Update cart total amount in summary card
    const cartTotalElement = document.getElementById("cart-total-amount");
    if (cartTotalElement) {
      cartTotalElement.textContent = this.formatCurrency(
        this.calculatedPrices.totalAmount
      );
    }
  }

  renderCartItems() {
    const container = document.getElementById("cart-items-container");

    if (this.cartItems.length === 0) {
      container.innerHTML =
        '<tr><td colspan="5" class="text-center">Không có sản phẩm nào trong giỏ hàng</td></tr>';
      return;
    }

    const cartItemsHTML = this.cartItems
      .map((item) => {
        // Debug: Log each item structure
        console.log("Rendering cart item:", item);

        // Try to get calculated price if available
        let displayPrice = this.formatCurrency(item.unit_price);
        let displayTotal = this.formatCurrency(item.line_total);

        if (this.calculatedPrices && this.calculatedPrices.cartItems) {
          const calculatedItem = this.calculatedPrices.cartItems.find(
            (ci) => ci.cartItemId === item.id
          );

          if (calculatedItem) {
            console.log("Found calculated price for item:", calculatedItem);
            // Use calculated prices instead of original prices
            displayPrice = this.formatCurrency(
              calculatedItem.calculatedUnitPrice
            );
            displayTotal = this.formatCurrency(calculatedItem.lineTotal);
          }
        }

        return `
            <tr data-cart-item-id="${item.id}">
                <td class="pro-thumbnail">
                    <img class="img-fluid" 
                             src="${this.getProductImageUrl({
                               id: item.product_id,
                             })}" 
                             alt="${item.product_name}" 
                             style="width: 80px; height: 80px; object-fit: cover;"
                             onerror="this.src='https://localhost:7062/images/products/default/default.jpg'" />
                </td>
                <td class="pro-title">
                    ${item.product_name}
                </td>
                <td class="pro-quantity">
                    <div class="quantity-controls">
                        <button type="button" class="quantity-minus">-</button>
                        <input type="number" 
                               class="cart-quantity-input" 
                               value="${item.quantity}" 
                               min="0"
                               step="1"
                               inputmode="numeric"
                               pattern="[0-9]*"
                               data-cart-item-id="${item.id}"
                               data-original-quantity="${item.quantity}">
                        <button type="button" class="quantity-plus">+</button>
                    </div>
                </td>
                <td class="pro-subtotal">
                    <span>${displayTotal}</span>
                </td>
                <td class="pro-remove">
                    <button class="btn-remove-item remove-cart-item" 
                            data-cart-item-id="${item.id}">
                        <i class="fa fa-trash-o"></i>
                    </button>
                </td>
            </tr>
        `;
      })
      .join("");

    // Add the cart items
    container.innerHTML = cartItemsHTML;

    // Add Update Cart button below the table
    const updateButtonContainer = document.createElement("tr");
    updateButtonContainer.innerHTML = `
      <td colspan="5" class="text-center">
        <div class="update-cart-section">
<button type="button" class="btn btn-primary update-cart-btn" id="update-cart-btn" style="background-color: #007bff; color: #ffffff;">
    <i class="fa fa-refresh"></i> Cập nhật giỏ hàng
</button>
          <div class="update-cart-info">
            <small class="text-muted">Thay đổi số lượng và nhấn "Cập nhật giỏ hàng" để áp dụng</small>
          </div>
        </div>
      </td>
    `;
    container.appendChild(updateButtonContainer);
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

    // Use calculated prices if available, otherwise use cart summary
    let totalAmount = this.cartSummary.total_amount;
    if (this.calculatedPrices && this.calculatedPrices.totalAmount > 0) {
      totalAmount = this.calculatedPrices.totalAmount;
      console.log("Using calculated total amount:", totalAmount);
    }

    document.getElementById("cart-total-amount").textContent =
      this.formatCurrency(totalAmount);

    // Update sidebar summary
    let subtotal = this.cartSummary.total_amount;
    let total = subtotal; // Total is just the product total, no shipping

    if (this.calculatedPrices) {
      subtotal = this.calculatedPrices.subtotal;
      total = this.calculatedPrices.totalAmount;
      console.log(
        "Using calculated prices - Subtotal:",
        subtotal,
        "Total:",
        total
      );
    }

    document.getElementById("subtotal-amount").textContent =
      this.formatCurrency(subtotal);
    document.getElementById("final-total-amount").textContent =
      this.formatCurrency(total);
  }

  async updateCartItemQuantity(cartItemId, quantity) {
    // Validate inputs
    if (!cartItemId) {
      console.error("Cart item ID is missing");
      this.showToast("Lỗi: Không tìm thấy ID sản phẩm", "error");
      return;
    }

    console.log(`Updating cart item ${cartItemId} to quantity ${quantity}`);

    // Add visual feedback
    const quantityControl = document
      .querySelector(`[data-cart-item-id="${cartItemId}"]`)
      .closest("tr")
      .querySelector(".quantity-controls");

    if (!quantityControl) {
      console.error(`Quantity control not found for cart item ${cartItemId}`);
      this.showToast("Lỗi: Không tìm thấy điều khiển số lượng", "error");
      return;
    }

    const originalQuantity = quantityControl.querySelector(
      ".cart-quantity-input"
    ).value;

    // Set loading state
    quantityControl.classList.add("updating");
    quantityControl
      .querySelectorAll("button")
      .forEach((btn) => (btn.disabled = true));

    try {
      const result = await ApiService.updateCartItem(cartItemId, quantity);

      if (result.succeeded) {
        this.showToast("Cập nhật số lượng thành công", "success");

        // Update the specific row instead of reloading all data for better UX
        if (result.data) {
          await this.updateCartItemDisplay(cartItemId, result.data);
        }

        // Update totals and header count
        await this.loadCartSummary();
        this.updateHeaderCartCount();

        // Brief success visual feedback
        quantityControl.style.borderColor = "#28a745";
        setTimeout(() => {
          quantityControl.style.borderColor = "";
        }, 1500);
      } else {
        throw new Error(result.message || "Không thể cập nhật số lượng");
      }
    } catch (error) {
      console.error("Error updating quantity:", error);

      // Revert to original quantity
      quantityControl.querySelector(".cart-quantity-input").value =
        originalQuantity;

      // Handle specific error types
      if (
        error.message.includes("401") ||
        error.message.includes("Unauthorized")
      ) {
        localStorage.removeItem("auth_token");
        this.showAuthenticationError();
      } else if (error.message.includes("404")) {
        this.showToast(
          "Sản phẩm không tồn tại trong giỏ hàng. Đang tải lại...",
          "error"
        );
        // Reload cart data to refresh the view and remove stale items
        setTimeout(() => {
          this.loadCartData();
        }, 1500);
      } else if (error.message.includes("400")) {
        this.showToast("Dữ liệu không hợp lệ. Vui lòng thử lại", "error");
      } else {
        // For any other error, try to refresh the cart to get latest data
        this.showToast(
          "Có lỗi xảy ra khi cập nhật số lượng. Đang tải lại giỏ hàng...",
          "error"
        );
        setTimeout(() => {
          this.loadCartData();
        }, 2000);
      }

      // Visual error feedback
      quantityControl.style.borderColor = "#dc3545";
      setTimeout(() => {
        quantityControl.style.borderColor = "";
      }, 3000);
    } finally {
      // Remove loading state
      quantityControl.classList.remove("updating");
      quantityControl
        .querySelectorAll("button")
        .forEach((btn) => (btn.disabled = false));
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
        this.updateHeaderCartCount(); // Update header count
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
        this.updateHeaderCartCount(); // Update header count
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

  getProductImageUrl(product) {
    const baseUrl = "https://localhost:7062"; // Use the same base URL as shop/homepage
    if (!product || !product.id) {
      return `${baseUrl}/images/products/default/default.jpg`;
    }

    // Check if product_url starts with /images/
    if (product.product_url && product.product_url.startsWith("/images/")) {
      // Use the product_url directly
      return `${baseUrl}${product.product_url}`;
    }

    // Fallback to old format
    return `${baseUrl}/images/products/${product.id}/primary.jpg`;
  }

  async updateCartItemDisplay(cartItemId, updatedData) {
    // Update the specific cart item row with new data
    const row = document.querySelector(`tr[data-cart-item-id="${cartItemId}"]`);
    if (row && updatedData) {
      // Update quantity display
      const quantityInput = row.querySelector(".cart-quantity-input");
      if (quantityInput) {
        quantityInput.value = updatedData.quantity;
      }

      // Update line total
      const subtotalCell = row.querySelector(".pro-subtotal span");
      if (subtotalCell && updatedData.line_total) {
        subtotalCell.textContent = this.formatCurrency(updatedData.line_total);
      }
    }
  }

  async loadCartSummary() {
    try {
      // Load just the cart summary for totals
      const summaryResult = await ApiService.getCartSummary();
      if (summaryResult.succeeded) {
        this.cartSummary = summaryResult.data;
        this.updateCartSummary();
      }
    } catch (error) {
      console.error("Error loading cart summary:", error);
      // Fallback to full reload if summary endpoint doesn't exist
      await this.loadCartData();
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

  // Function to validate all pending changes against stock
  async validateAllPendingChanges() {
    // Bỏ stock validation - cho phép tất cả thay đổi
    return this.pendingChanges;
  }

  // Function to validate quantity against stock
  async validateQuantityAgainstStock(cartItemId, quantity, inputElement) {
    // Bỏ stock validation - cho phép thay đổi bất kỳ số lượng nào
    await this.markItemAsChanged(cartItemId, quantity);
    return true;
  }

  // Function to validate quantity change (for +/- buttons)
  async validateQuantityChange(inputElement, newQuantity) {
    const cartItemId = inputElement.getAttribute("data-cart-item-id");

    if (!cartItemId) {
      console.error("Cart item ID is missing for quantity change validation");
      return;
    }

    // Bỏ stock validation - cho phép thay đổi bất kỳ số lượng nào
    inputElement.value = newQuantity;
    await this.markItemAsChanged(cartItemId, newQuantity);
  }

  // Function to reset cart item input to original value
  resetCartItemInput(cartItemId) {
    const input = document.querySelector(
      `[data-cart-item-id="${cartItemId}"] .cart-quantity-input`
    );
    if (input) {
      const originalQuantity = input.getAttribute("data-original-quantity");
      input.value = originalQuantity;
      input.classList.remove("has-changes");

      // Remove from pending changes
      this.pendingChanges.delete(cartItemId);

      // Update button state
      this.updateUpdateCartButton();
    }
  }

  // Function to check if cart item still exists and is valid
  isCartItemValid(cartItemId) {
    return this.cartItems.some(
      (item) => item.id.toString() === cartItemId.toString()
    );
  }

  // Function to mark an item as changed
  async markItemAsChanged(cartItemId, newQuantity) {
    if (!this.pendingChanges) {
      this.pendingChanges = new Map();
    }

    // Check if cart item still exists
    if (!this.isCartItemValid(cartItemId)) {
      console.warn(
        `Cart item ${cartItemId} no longer exists, cannot mark as changed`
      );
      return;
    }

    // Check if quantity is valid
    if (newQuantity < 0) {
      console.warn(
        `Invalid quantity ${newQuantity} for cart item ${cartItemId}`
      );
      return;
    }

    // Bỏ stock validation - cho phép thay đổi bất kỳ số lượng nào

    // Mark as changed
    this.pendingChanges.set(cartItemId, {
      action: newQuantity === 0 ? "remove" : "update",
      newQuantity: newQuantity,
    });

    // Update visual feedback
    const input = document.querySelector(
      `[data-cart-item-id="${cartItemId}"] .cart-quantity-input`
    );
    if (input) {
      input.classList.add("has-changes");
    }

    // Update the update cart button state
    this.updateUpdateCartButton();
  }

  updateUpdateCartButton() {
    const updateBtn = document.getElementById("update-cart-btn");
    if (updateBtn) {
      if (this.pendingChanges && this.pendingChanges.size > 0) {
        updateBtn.textContent = `Cập nhật giỏ hàng (${this.pendingChanges.size} thay đổi)`;
        updateBtn.classList.add("btn-warning");
        updateBtn.classList.remove("btn-primary");
        updateBtn.disabled = false;
      } else {
        updateBtn.innerHTML = '<i class="fa fa-refresh"></i> Cập nhật giỏ hàng';
        updateBtn.classList.remove("btn-warning");
        updateBtn.classList.add("btn-primary");
        updateBtn.disabled = true;
      }
    }
  }

  async updateCartWithChanges() {
    if (this.pendingChanges.size === 0) {
      this.showToast("Không có thay đổi nào để cập nhật", "info");
      return;
    }

    const updateBtn = document.getElementById("update-cart-btn");
    if (updateBtn) {
      updateBtn.disabled = true;
      updateBtn.innerHTML =
        '<i class="fa fa-spinner fa-spin"></i> Đang cập nhật...';
    }

    try {
      // Validate supplier availability before updating
      this.showToast("Đang kiểm tra khả năng cung cấp...", "info");

      for (const [cartItemId, change] of this.pendingChanges) {
        const cartItem = this.cartItems.find(
          (item) => item.id === parseInt(cartItemId)
        );
        if (cartItem) {
          try {
            const availabilityResponse = await fetch(
              `https://localhost:7062/api/Product/check-availability/${cartItem.product_id}?quantity=${change.newQuantity}`
            );

            if (!availabilityResponse.ok) {
              this.showToast(
                `Không thể kiểm tra sản phẩm ${cartItem.product_name}`,
                "error"
              );
              return;
            }

            const availabilityResult = await availabilityResponse.json();
            if (!availabilityResult.succeeded) {
              this.showToast(
                `Lỗi kiểm tra sản phẩm ${cartItem.product_name}`,
                "error"
              );
              return;
            }

            const availability = availabilityResult.data;
            if (!availability.isAvailable) {
              this.showToast(
                `Sản phẩm ${cartItem.product_name}: ${availability.message}`,
                "warning"
              );
              return;
            }
          } catch (error) {
            console.error("Error checking availability:", error);
            this.showToast(
              `Không thể kiểm tra sản phẩm ${cartItem.product_name}`,
              "error"
            );
            return;
          }
        }
      }

      this.showToast("Tất cả sản phẩm đều có thể cung cấp!", "success");

      let successCount = 0;
      let errorCount = 0;

      // Process all changes
      for (const [cartItemId, change] of this.pendingChanges) {
        try {
          console.log(`Processing cart item ${cartItemId}:`, change);

          if (change.action === "remove") {
            // Remove item
            console.log(`Removing cart item ${cartItemId}`);
            const result = await ApiService.removeCartItem(cartItemId);
            if (result.succeeded) {
              console.log(`Successfully removed cart item ${cartItemId}`);
              successCount++;
            } else {
              console.error(
                `Failed to remove cart item ${cartItemId}:`,
                result
              );
              errorCount++;
            }
          } else {
            // Update quantity
            console.log(
              `Updating cart item ${cartItemId} to quantity ${change.newQuantity}`
            );
            const result = await ApiService.updateCartItem(
              cartItemId,
              change.newQuantity
            );
            if (result.succeeded) {
              console.log(`Successfully updated cart item ${cartItemId}`);
              successCount++;
            } else {
              console.error(
                `Failed to update cart item ${cartItemId}:`,
                result
              );
              errorCount++;
            }
          }
        } catch (error) {
          console.error(`Error updating cart item ${cartItemId}:`, error);
          errorCount++;
        }
      }

      // Show results
      if (errorCount === 0) {
        this.showToast(
          `Cập nhật thành công ${successCount} sản phẩm!`,
          "success"
        );
        this.pendingChanges.clear();
        this.updateUpdateCartButton();

        // Clear visual feedback from all inputs
        document
          .querySelectorAll(".cart-quantity-input.has-changes")
          .forEach((input) => {
            input.classList.remove("has-changes");
          });

        // Reload cart data to show updated totals
        await this.loadCartData();
        this.updateHeaderCartCount();
      } else {
        this.showToast(
          `Cập nhật ${successCount} sản phẩm thành công, ${errorCount} lỗi`,
          "warning"
        );

        // Reload cart data if there were errors
        if (errorCount > 0) {
          console.log(
            "Some updates failed, refreshing cart data to get latest state..."
          );
          setTimeout(async () => {
            await this.loadCartData();
          }, 1000);
        }

        // Keep failed changes in pendingChanges for retry
        this.updateUpdateCartButton();
      }
    } catch (error) {
      console.error("Error updating cart:", error);
      this.showToast("Có lỗi xảy ra khi cập nhật giỏ hàng", "error");
    } finally {
      // Reset button
      if (updateBtn) {
        updateBtn.disabled = false;
        this.updateUpdateCartButton();
      }
    }
  }

  // Validate cart items before checkout
  async validateCartForCheckout() {
    // Ensure cart items are loaded
    if (!this.cartItems || this.cartItems.length === 0) {
      // Try to reload cart data first
      try {
        await this.loadCartData();
        if (!this.cartItems || this.cartItems.length === 0) {
          this.showToast(
            "Giỏ hàng trống, không thể tiến hành thanh toán",
            "error"
          );
          return false;
        }
      } catch (error) {
        this.showToast("Không thể tải dữ liệu giỏ hàng", "error");
        return false;
      }
    }

    // Validate supplier availability for each cart item
    try {
      this.showToast("Đang kiểm tra khả năng cung cấp sản phẩm...", "info");

      for (const item of this.cartItems) {
        const availabilityResponse = await fetch(
          `https://localhost:7062/api/Product/check-availability/${item.product_id}?quantity=${item.quantity}`
        );

        if (!availabilityResponse.ok) {
          this.showToast(
            `Không thể kiểm tra sản phẩm ${item.product_name}`,
            "error"
          );
          return false;
        }

        const availabilityResult = await availabilityResponse.json();
        if (!availabilityResult.succeeded) {
          this.showToast(`Lỗi kiểm tra sản phẩm ${item.product_name}`, "error");
          return false;
        }

        const availability = availabilityResult.data;
        if (!availability.isAvailable) {
          this.showToast(
            `Sản phẩm ${item.product_name}: ${availability.message}`,
            "warning"
          );
          return false;
        }
      }

      this.showToast("Tất cả sản phẩm đều có thể cung cấp!", "success");
      return true;
    } catch (error) {
      console.error("Error validating supplier availability:", error);
      this.showToast("Có lỗi xảy ra khi kiểm tra khả năng cung cấp", "error");
      return false;
    }
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
          window.location.href = "../Payment/checkout.html";
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
        this.showToast("Có lỗi xảy ra khi kiểm tra giỏ hàng", "error");
        // Reset button state on error
        if (checkoutBtn) {
          checkoutBtn.disabled = false;
          checkoutBtn.innerHTML = "Proceed To Checkout";
        }
      });
  }

  debugCartState() {
    console.log("Current Cart State:");
    console.log("Cart Items:", this.cartItems);
    console.log("Cart Summary:", this.cartSummary);
    console.log("Calculated Prices:", this.calculatedPrices);
    console.log("Pending Changes:", this.pendingChanges);
    console.log("Current Page:", this.currentPage);
    console.log("Page Size:", this.pageSize);
  }

  forceReloadCalculatedPrices() {
    this.loadCalculatedPrices().then(() => {
      this.showToast("Giá đã được tải lại từ nhà cung cấp.", "success");
    });
  }
}

// Initialize cart page when DOM is loaded
document.addEventListener("DOMContentLoaded", () => {
  const cartManager = new CartPageManager();

  // Make debug method globally accessible
  window.debugCart = () => cartManager.debugCartState();

  // Also store the instance for access from console
  window.cartManager = cartManager;
});
