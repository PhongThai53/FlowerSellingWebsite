import { ApiService } from "./services/ApiService.js";

class CheckoutManager {
  constructor() {
    this.cartItems = [];
    this.cartSummary = { totalItems: 0, total_amount: 0 };
    this.calculatedPrices = null; // { subtotal, serviceFee, totalAmount, cartItems: [] }
    this.init();
  }

  async init() {
    try {
      await this.waitForComponents();
      await Promise.race([
        this.loadCartData(),
        new Promise((_, reject) =>
          setTimeout(() => reject(new Error("Cart loading timeout")), 10000)
        ),
      ]);
      this.setupEventListeners();
      await this.loadCalculatedPrices();
      this.populateOrderSummary();
      this.checkAuthentication();
    } catch (error) {
      console.error("Error initializing checkout:", error);
      this.showCartEmptyState();
    }
  }

  async waitForComponents() {
    return new Promise((resolve) => setTimeout(resolve, 200));
  }

  async loadCartData() {
    try {
      const result = await ApiService.getCartItems(1, 100);
      if (result && result.succeeded && result.data) {
        this.cartItems = result.data.cart_items || result.data.cartItems || [];
        this.cartSummary = result.data.cart_summary ||
          result.data.cartSummary || { totalItems: 0, total_amount: 0 };
      } else {
        this.showCartEmptyState();
      }
    } catch (error) {
      console.error("Error loading cart data:", error);
      this.showCartEmptyState();
    }
  }

  async loadCalculatedPrices() {
    try {
      const calc = await ApiService.calculateCartPrice();
      if (calc && calc.succeeded && calc.data) {
        // Normalize keys: serviceFee from backend is "serviceFee"
        this.calculatedPrices = {
          subtotal: calc.data.subtotal || 0,
          serviceFee: calc.data.serviceFee || 0,
          totalAmount: calc.data.totalAmount || 0,
          cartItems: calc.data.cartItems || [],
        };
      }
    } catch (e) {
      console.error("Failed to load calculated prices:", e);
    }
  }

  showCartEmptyState() {
    const orderSummaryLoading = document.getElementById("orderSummaryLoading");
    const cartEmptyState = document.getElementById("cartEmptyState");
    const placeOrderBtn = document.getElementById("placeOrderBtn");
    if (orderSummaryLoading) orderSummaryLoading.style.display = "none";
    if (cartEmptyState) cartEmptyState.style.display = "block";
    if (placeOrderBtn) placeOrderBtn.disabled = true;
  }

  setupEventListeners() {
    this.setupFormValidation();
    this.setupPaymentMethodSelection();
    this.setupPlaceOrderButton();
    this.setupRefreshCartButton();
  }

  setupFormValidation() {
    const form = document.getElementById("checkoutForm");
    if (!form) return;
    const inputs = form.querySelectorAll(
      "input[required], select[required], textarea[required]"
    );
    inputs.forEach((input) => {
      input.addEventListener("blur", () => this.validateField(input));
      input.addEventListener("input", () => this.clearFieldError(input));
    });
  }

  setupPaymentMethodSelection() {
    const paymentMethods = document.querySelectorAll(
      'input[name="paymentMethod"]'
    );
    const paymentDetails = document.querySelectorAll(".payment-method-details");
    paymentMethods.forEach((method) => {
      method.addEventListener("change", (e) => {
        const selectedMethod = e.target.value;
        paymentDetails.forEach((detail) => detail.classList.remove("show"));
        const selectedDetail = document.querySelector(
          `[data-method="${selectedMethod}"]`
        );
        if (selectedDetail) selectedDetail.classList.add("show");
        this.updatePlaceOrderButtonText(selectedMethod);
      });
    });
  }

  setupPlaceOrderButton() {
    const placeOrderBtn = document.getElementById("placeOrderBtn");
    if (placeOrderBtn)
      placeOrderBtn.addEventListener("click", () => this.placeOrder());
  }

  setupRefreshCartButton() {
    const refreshCartBtn = document.getElementById("refreshCartBtn");
    if (refreshCartBtn) {
      refreshCartBtn.addEventListener("click", async () => {
        refreshCartBtn.disabled = true;
        refreshCartBtn.textContent = "Đang tải...";
        try {
          await this.loadCartData();
          await this.loadCalculatedPrices();
          this.populateOrderSummary();
        } finally {
          refreshCartBtn.disabled = false;
          refreshCartBtn.textContent = "Tải lại giỏ hàng";
        }
      });
    }
  }

  validateField(field) {
    const fieldId = field.id;
    const errorElement = document.getElementById(fieldId + "Error");
    if (!errorElement) return true;
    let isValid = true;
    let errorMessage = "";
    if (!field.value.trim()) {
      isValid = false;
      errorMessage = "Trường này là bắt buộc";
    } else {
      switch (field.type) {
        case "email":
          if (!this.isValidEmail(field.value)) {
            isValid = false;
            errorMessage = "Vui lòng nhập email hợp lệ";
          }
          break;
        case "tel":
          if (!this.isValidPhone(field.value)) {
            isValid = false;
            errorMessage = "Vui lòng nhập số điện thoại hợp lệ";
          }
          break;
      }
    }
    if (!isValid) {
      this.showFieldError(field, errorMessage);
    } else {
      this.clearFieldError(field);
    }
    return isValid;
  }

  isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  isValidPhone(phone) {
    const cleanPhone = phone.replace(/[\s\-\(\)\.]/g, "");
    const vietnamPhoneRegex = /^(0|\+84)[0-9]{9}$/;
    const internationalPhoneRegex = /^\+[1-9][0-9]{7,14}$/;
    return (
      vietnamPhoneRegex.test(cleanPhone) ||
      internationalPhoneRegex.test(cleanPhone)
    );
  }

  showFieldError(field, message) {
    const errorElement = document.getElementById(field.id + "Error");
    if (errorElement) {
      errorElement.textContent = message;
      errorElement.classList.add("show");
      field.style.borderColor = "#dc3545";
    }
  }

  clearFieldError(field) {
    const errorElement = document.getElementById(field.id + "Error");
    if (errorElement) {
      errorElement.classList.remove("show");
      field.style.borderColor = "#ddd";
    }
  }

  updatePlaceOrderButtonText(paymentMethod) {
    const placeOrderBtn = document.getElementById("placeOrderBtn");
    if (placeOrderBtn) {
      if (paymentMethod === "vnpay") {
        placeOrderBtn.textContent = "Thanh toán VNPay";
      } else {
        placeOrderBtn.textContent = "Đặt hàng";
      }
    }
  }

  populateOrderSummary() {
    const orderSummaryBody = document.getElementById("orderSummaryBody");
    const cartEmptyState = document.getElementById("cartEmptyState");
    const placeOrderBtn = document.getElementById("placeOrderBtn");
    const orderSummaryLoading = document.getElementById("orderSummaryLoading");
    const orderSummaryTable = document.getElementById("orderSummaryTable");
    if (!orderSummaryBody) return;
    if (orderSummaryLoading) orderSummaryLoading.style.display = "none";
    if (!this.cartItems || this.cartItems.length === 0) {
      if (cartEmptyState) cartEmptyState.style.display = "block";
      if (placeOrderBtn) placeOrderBtn.disabled = true;
      if (orderSummaryTable) orderSummaryTable.style.display = "none";
      return;
    }
    if (cartEmptyState) cartEmptyState.style.display = "none";
    if (placeOrderBtn) placeOrderBtn.disabled = false;
    if (orderSummaryTable) orderSummaryTable.style.display = "table";

    const lineTotalsByProduct = new Map();
    if (
      this.calculatedPrices &&
      Array.isArray(this.calculatedPrices.cartItems)
    ) {
      for (const ci of this.calculatedPrices.cartItems) {
        // Expect keys: productId, quantity, calculatedUnitPrice, calculatedLineTotal
        const pid = ci.productId || ci.product_id;
        if (pid != null) {
          lineTotalsByProduct.set(pid, {
            unit: ci.calculatedUnitPrice ?? ci.unitPrice ?? 0,
            line: ci.calculatedLineTotal ?? ci.lineTotal ?? 0,
          });
        }
      }
    }

    const summaryHTML = this.cartItems
      .map((item) => {
        const priceInfo = lineTotalsByProduct.get(item.product_id) || {
          unit: item.unit_price || 0,
          line:
            item.line_total || (item.unit_price || 0) * (item.quantity || 1),
        };
        return `
            <tr>
                <td>
                    ${item.product_name || "Sản phẩm"} <strong> × ${
          item.quantity
        }</strong>
                </td>
                <td>${this.formatCurrency(priceInfo.line)}</td>
            </tr>
        `;
      })
      .join("");

    orderSummaryBody.innerHTML = summaryHTML;
    this.updateOrderSummary();
  }

  updateOrderSummary() {
    // Prefer backend calculated prices
    const subtotal =
      this.calculatedPrices?.subtotal ?? (this.cartSummary.total_amount || 0);
    const serviceFee =
      this.calculatedPrices?.serviceFee ?? Math.round(subtotal * 0.5);
    const total = this.calculatedPrices?.totalAmount ?? subtotal + serviceFee;

    const subtotalElement = document.getElementById("subtotalAmount");
    const serviceFeeElement = document.getElementById("serviceFeeAmount");
    const totalElement = document.getElementById("totalAmount");

    if (subtotalElement)
      subtotalElement.textContent = this.formatCurrency(subtotal);
    if (serviceFeeElement)
      serviceFeeElement.textContent = this.formatCurrency(serviceFee);
    if (totalElement) totalElement.textContent = this.formatCurrency(total);
  }

  formatCurrency(amount) {
    return new Intl.NumberFormat("vi-VN", {
      style: "currency",
      currency: "VND",
    }).format(amount || 0);
  }

  checkAuthentication() {
    const token = localStorage.getItem("auth_token");
    if (!token) {
      window.location.href = "../auth/login-register.html";
      return;
    }
  }

  async placeOrder() {
    try {
      if (!this.cartItems || this.cartItems.length === 0) {
        this.showToast(
          "Giỏ hàng trống. Vui lòng thêm sản phẩm trước khi thanh toán.",
          "error"
        );
        return;
      }
      if (!this.validateForm()) {
        this.showToast("Vui lòng điền đúng các trường bắt buộc", "error");
        return;
      }
      const orderData = this.collectOrderData();
      this.setLoadingState(true);
      const result = await this.submitOrder(orderData);
      if (result.succeeded) {
        this.showToast("Đặt hàng thành công!", "success");
        if (orderData.paymentMethod === "vnpay") {
          if (result.data.paymentUrl) {
            window.location.href = result.data.paymentUrl;
          } else {
            this.showToast("Không nhận được liên kết VNPay", "error");
          }
        } else {
          if (result.data.redirectUrl) {
            window.location.href = result.data.redirectUrl;
          } else {
            window.location.href = `../user/order-confirmation.html?orderId=${
              result.data.orderId
            }&orderNumber=${
              result.data.orderNumber
            }&paymentMethod=${encodeURIComponent(
              orderData.paymentMethod
            )}&status=Created`;
          }
        }
      } else {
        this.showToast(result.message || "Đặt hàng thất bại", "error");
      }
    } catch (error) {
      console.error("Error placing order:", error);
      this.showToast("Có lỗi xảy ra khi đặt hàng", "error");
    } finally {
      this.setLoadingState(false);
    }
  }

  validateForm() {
    const requiredFields = document.querySelectorAll(
      "#checkoutForm [required]"
    );
    let isValid = true;
    requiredFields.forEach((field) => {
      if (!this.validateField(field)) isValid = false;
    });
    return isValid;
  }

  collectOrderData() {
    const form = document.getElementById("checkoutForm");
    const formData = new FormData(form);
    const orderData = {
      customerFirstName: formData.get("firstName"),
      customerLastName: formData.get("lastName"),
      customerEmail: formData.get("email"),
      customerPhone: formData.get("phone"),
      companyName: formData.get("companyName") || undefined,
      country: formData.get("country") || undefined,
      city: formData.get("city"),
      state: formData.get("state"),
      postcode: formData.get("postcode") || undefined,
      streetAddress: formData.get("streetAddress"),
      streetAddress2: formData.get("streetAddress2"),
      orderNotes: formData.get("orderNotes"),
      paymentMethod: document.querySelector(
        'input[name="paymentMethod"]:checked'
      ).value,
      shippingMethod: "flat",
      cartItems: this.cartItems.map((item) => ({
        productId: item.product_id,
        quantity: item.quantity,
        unitPrice: item.unit_price,
        lineTotal: item.line_total || item.unit_price * item.quantity,
        productName: item.product_name,
      })),
      subtotal:
        this.calculatedPrices?.subtotal ??
        (this.cartSummary?.total_amount || this.cartSummary?.totalAmount || 0),
      totalAmount:
        this.calculatedPrices?.totalAmount ??
        (this.cartSummary?.total_amount || this.cartSummary?.totalAmount || 0),
    };
    return orderData;
  }

  async submitOrder(orderData) {
    try {
      const token = localStorage.getItem("auth_token");
      if (!token) throw new Error("Authentication required");
      const response = await fetch(
        "https://localhost:7062/api/Order/checkout",
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${token}`,
          },
          body: JSON.stringify(orderData),
        }
      );

      const status = response.status;
      const contentType = response.headers.get("content-type") || "";
      let rawText = "";
      let parsedJson = null;

      try {
        rawText = await response.text();
      } catch (readErr) {
        console.warn("Failed reading response body:", readErr);
      }

      if (rawText) {
        if (contentType.includes("application/json")) {
          try {
            parsedJson = JSON.parse(rawText);
          } catch (parseErr) {
            console.warn("Failed to parse JSON response:", parseErr, rawText);
          }
        }
      }

      if (!response.ok) {
        const serverMessage =
          parsedJson?.message || rawText || `HTTP ${status}`;
        console.error("Checkout API error:", {
          status,
          serverMessage,
          rawText,
        });
        // More friendly message for common cases
        if (status === 404) {
          throw new Error(
            "Không tìm thấy API thanh toán (404). Vui lòng kiểm tra server."
          );
        }
        if (status === 401 || status === 403) {
          throw new Error(
            "Bạn cần đăng nhập để thanh toán. Vui lòng đăng nhập lại."
          );
        }
        throw new Error(serverMessage);
      }

      // Success path
      if (parsedJson) {
        return parsedJson;
      }

      // If no JSON returned but request succeeded, fabricate a basic success object
      console.warn("Checkout API returned empty/non-JSON success body.");
      return { succeeded: true, data: {} };
    } catch (error) {
      console.error("Error submitting order:", error);
      throw error;
    }
  }

  setLoadingState(loading) {
    const placeOrderBtn = document.getElementById("placeOrderBtn");
    const loadingSpinner = document.getElementById("orderLoadingSpinner");
    if (placeOrderBtn) placeOrderBtn.disabled = loading;
    if (loadingSpinner) {
      if (loading) loadingSpinner.classList.add("show");
      else loadingSpinner.classList.remove("show");
    }
  }

  showToast(message, type = "info") {
    const toast = document.createElement("div");
    toast.className = `toast toast-${type}`;
    toast.innerHTML = `
            <div class="toast-content">
                <i class="fa fa-${this.getToastIcon(type)}"></i>
                <span>${message}</span>
            </div>
        `;
    document.body.appendChild(toast);
    setTimeout(() => toast.classList.add("show"), 100);
    setTimeout(() => toast.remove(), 3000);
  }

  getToastIcon(type) {
    switch (type) {
      case "success":
        return "check";
      case "error":
        return "times-circle";
      case "warning":
        return "exclamation-triangle";
      default:
        return "info";
    }
  }
}

export { CheckoutManager };
