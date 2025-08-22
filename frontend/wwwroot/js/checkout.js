import { ApiService } from "./services/ApiService.js";

class CheckoutManager {
  constructor() {
    this.cartItems = [];
    this.cartSummary = { totalItems: 0, totalAmount: 0 };
    this.init();
  }

  async init() {
    try {
      // Wait for HTMX to load components
      await this.waitForComponents();

      // Load cart data with timeout
      await Promise.race([
        this.loadCartData(),
        new Promise((_, reject) =>
          setTimeout(() => reject(new Error("Cart loading timeout")), 10000)
        ),
      ]);

      // Setup event listeners
      this.setupEventListeners();

      // Populate order summary
      this.populateOrderSummary();

      // Check authentication
      this.checkAuthentication();
    } catch (error) {
      console.error("Error initializing checkout:", error);
      this.showCartEmptyState();
    }
  }

  async waitForComponents() {
    return new Promise((resolve) => {
      setTimeout(resolve, 200);
    });
  }

  async loadCartData() {
    try {
      console.log("=== LOADING CART DATA ===");

      const token = localStorage.getItem("auth_token");
      console.log("Auth token found:", !!token);

      if (!token) {
        console.log("No authentication token found");
        this.showCartEmptyState();
        return;
      }

      console.log("Loading cart data...");

      // Test the API directly first
      console.log("Testing API directly...");
      try {
        const testResponse = await fetch(
          "https://localhost:7062/api/Cart/items?page=1&pageSize=100",
          {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          }
        );
        console.log("Direct API response status:", testResponse.status);
        const testData = await testResponse.text();
        console.log("Direct API response text:", testData);
      } catch (testError) {
        console.error("Direct API test failed:", testError);
      }

      // Get cart items (not just summary)
      console.log("Calling ApiService.getCartItems...");
      const result = await ApiService.getCartItems(1, 100); // Get all items
      console.log("Cart API response:", result);
      console.log("Result succeeded:", result?.succeeded);
      console.log("Result data:", result?.data);

      if (result && result.succeeded && result.data) {
        this.cartItems = result.data.cart_items || result.data.cartItems || [];
        this.cartSummary = result.data.cart_summary ||
          result.data.cartSummary || {
            totalItems: 0,
            total_amount: 0,
            totalAmount: 0,
          };
        console.log("Cart data loaded successfully:", this.cartItems);
        console.log("Cart summary:", this.cartSummary);
        console.log("Cart items length:", this.cartItems.length);

        // Force update the order summary
        this.populateOrderSummary();
      } else {
        console.error(
          "Failed to load cart data:",
          result?.message || "Unknown error"
        );
        console.error("Result structure:", {
          succeeded: result?.succeeded,
          data: result?.data,
          message: result?.message,
        });
        this.showCartEmptyState();
      }
    } catch (error) {
      console.error("Error loading cart data:", error);
      console.error("Error details:", {
        name: error.name,
        message: error.message,
        stack: error.stack,
      });

      // Try to get cart summary as fallback
      try {
        console.log("Trying cart summary as fallback...");
        const summaryResult = await ApiService.getCartSummary();
        console.log("Fallback summary result:", summaryResult);

        if (summaryResult && summaryResult.succeeded && summaryResult.data) {
          this.cartSummary = summaryResult.data;
          // Ensure consistent property names
          if (
            this.cartSummary &&
            !this.cartSummary.total_amount &&
            this.cartSummary.totalAmount
          ) {
            this.cartSummary.total_amount = this.cartSummary.totalAmount;
          }
          this.cartItems = []; // Empty items but we have summary
          console.log("Cart summary loaded as fallback:", this.cartSummary);
          this.populateOrderSummary();
          return;
        }
      } catch (summaryError) {
        console.error("Fallback cart summary also failed:", summaryError);
      }

      this.showCartEmptyState();
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
    // Form validation
    this.setupFormValidation();

    // Payment method selection
    this.setupPaymentMethodSelection();

    // Place order button
    this.setupPlaceOrderButton();

    // Refresh cart button
    this.setupRefreshCartButton();
  }

  setupFormValidation() {
    const form = document.getElementById("checkoutForm");
    if (!form) return;

    // Real-time validation
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

        // Hide all payment details
        paymentDetails.forEach((detail) => {
          detail.classList.remove("show");
        });

        // Show selected payment method details
        const selectedDetail = document.querySelector(
          `[data-method="${selectedMethod}"]`
        );
        if (selectedDetail) {
          selectedDetail.classList.add("show");
        }

        // Update place order button text
        this.updatePlaceOrderButtonText(selectedMethod);
      });
    });
  }

  setupPlaceOrderButton() {
    const placeOrderBtn = document.getElementById("placeOrderBtn");
    if (placeOrderBtn) {
      placeOrderBtn.addEventListener("click", () => this.placeOrder());
    }
  }

  setupRefreshCartButton() {
    const refreshCartBtn = document.getElementById("refreshCartBtn");
    if (refreshCartBtn) {
      refreshCartBtn.addEventListener("click", async () => {
        console.log("Manual cart refresh requested...");
        refreshCartBtn.disabled = true;
        refreshCartBtn.textContent = "Refreshing...";

        try {
          await this.loadCartData();
        } finally {
          refreshCartBtn.disabled = false;
          refreshCartBtn.textContent = "Refresh Cart";
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

    // Check if field is empty
    if (!field.value.trim()) {
      isValid = false;
      errorMessage = "This field is required";
    } else {
      // Specific validation for different field types
      switch (field.type) {
        case "email":
          if (!this.isValidEmail(field.value)) {
            isValid = false;
            errorMessage = "Please enter a valid email address";
          }
          break;
        case "tel":
          if (!this.isValidPhone(field.value)) {
            isValid = false;
            errorMessage = "Please enter a valid phone number";
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
    // Remove spaces and common separators
    const cleanPhone = phone.replace(/[\s\-\(\)\.]/g, "");

    // Vietnamese phone number patterns:
    // - 09xxxxxxxx (10 digits, mobile)
    // - 01xxxxxxxx (10 digits, mobile)
    // - 02xxxxxxxx (10 digits, landline)
    // - 03xxxxxxxx (10 digits, landline)
    // - 04xxxxxxxx (10 digits, landline)
    // - 05xxxxxxxx (10 digits, landline)
    // - 06xxxxxxxx (10 digits, landline)
    // - 07xxxxxxxx (10 digits, landline)
    // - 08xxxxxxxx (10 digits, landline)
    // - +84xxxxxxxxx (international format)

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
        placeOrderBtn.textContent = "Proceed to VNPay";
      } else {
        placeOrderBtn.textContent = "Place Order";
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

    // Hide loading state
    if (orderSummaryLoading) orderSummaryLoading.style.display = "none";

    if (!this.cartItems || this.cartItems.length === 0) {
      // Show empty state
      if (cartEmptyState) cartEmptyState.style.display = "block";
      if (placeOrderBtn) placeOrderBtn.disabled = true;
      if (orderSummaryTable) orderSummaryTable.style.display = "none";
      return;
    }

    // Hide empty state, show table, and enable button
    if (cartEmptyState) cartEmptyState.style.display = "none";
    if (placeOrderBtn) placeOrderBtn.disabled = false;
    if (orderSummaryTable) orderSummaryTable.style.display = "table";

    const summaryHTML = this.cartItems
      .map(
        (item) => `
            <tr>
                <td>
                    <a href="../common/product-details.html?id=${
                      item.product_id
                    }">
                        ${item.product_name || "Product"} <strong> × ${
          item.quantity
        }</strong>
                    </a>
                </td>
                <td>${this.formatCurrency(
                  item.line_total || item.unit_price * item.quantity
                )}</td>
            </tr>
        `
      )
      .join("");

    orderSummaryBody.innerHTML = summaryHTML;
    this.updateOrderSummary();
  }

  updateOrderSummary() {
    const subtotal = this.cartSummary.total_amount || 0;
    const total = subtotal; // Total equals subtotal (no shipping fee or tax)

    // Update display
    const subtotalElement = document.getElementById("subtotalAmount");
    const totalElement = document.getElementById("totalAmount");

    if (subtotalElement) {
      subtotalElement.textContent = this.formatCurrency(subtotal);
    }
    if (totalElement) {
      totalElement.textContent = this.formatCurrency(total);
    }
  }

  formatCurrency(amount) {
    return new Intl.NumberFormat("vi-VN", {
      style: "currency",
      currency: "VND",
    }).format(amount);
  }

  checkAuthentication() {
    const token = localStorage.getItem("auth_token");
    if (!token) {
      // Redirect to login if not authenticated
      window.location.href = "../auth/login-register.html";
      return;
    }
  }

  async placeOrder() {
    try {
      console.log("=== PLACING ORDER ===");
      console.log("Cart items:", this.cartItems);
      console.log("Cart summary:", this.cartSummary);

      // Check if cart has items
      if (!this.cartItems || this.cartItems.length === 0) {
        console.log("Cart is empty, showing error");
        this.showToast(
          "Your cart is empty. Please add items before checkout.",
          "error"
        );
        return;
      }

      // Validate form
      console.log("Validating form...");
      if (!this.validateForm()) {
        console.log("Form validation failed");
        this.showToast("Please fill in all required fields correctly", "error");
        return;
      }

      console.log("Form validation passed, collecting order data...");
      // Get form data
      const orderData = this.collectOrderData();
      console.log("Order data collected:", orderData);

      // Show loading state
      console.log("Setting loading state...");
      this.setLoadingState(true);

      // Submit order
      console.log("Submitting order to backend...");
      const result = await this.submitOrder(orderData);
      console.log("Order submission result:", result);

      if (result.succeeded) {
        console.log("Order placed successfully!");
        this.showToast("Order placed successfully!", "success");

        // Handle different payment methods
        if (orderData.paymentMethod === "vnpay") {
          // Redirect to VNPay
          console.log("Redirecting to VNPay...");
          if (result.data.paymentUrl) {
            window.location.href = result.data.paymentUrl;
          } else {
            console.error("VNPay payment URL not received");
            this.showToast("VNPay payment URL not received", "error");
          }
        } else {
          // Redirect to order confirmation for COD
          console.log("Redirecting to order confirmation...");
          if (result.data.redirectUrl) {
            window.location.href = result.data.redirectUrl;
          } else {
            window.location.href = `../user/order-confirmation.html?orderId=${result.data.orderId}&orderNumber=${result.data.orderNumber}`;
          }
        }
      } else {
        console.log("Order placement failed:", result.message);
        this.showToast(result.message || "Failed to place order", "error");
      }
    } catch (error) {
      console.error("Error placing order:", error);
      this.showToast("An error occurred while placing your order", "error");
    } finally {
      console.log("Clearing loading state...");
      this.setLoadingState(false);
    }
  }

  validateForm() {
    const requiredFields = document.querySelectorAll(
      "#checkoutForm [required]"
    );
    let isValid = true;

    requiredFields.forEach((field) => {
      if (!this.validateField(field)) {
        isValid = false;
      }
    });

    return isValid;
  }

  collectOrderData() {
    const form = document.getElementById("checkoutForm");
    const formData = new FormData(form);

    const orderData = {
      // Customer information
      customerFirstName: formData.get("firstName"),
      customerLastName: formData.get("lastName"),
      customerEmail: formData.get("email"),
      customerPhone: formData.get("phone"),
      companyName: formData.get("companyName"),

      // Billing address
      country: formData.get("country"),
      city: formData.get("city"),
      state: formData.get("state"),
      postcode: formData.get("postcode"),
      streetAddress: formData.get("streetAddress"),
      streetAddress2: formData.get("streetAddress2"),

      // Order details
      orderNotes: formData.get("orderNotes"),

      // Payment and shipping
      paymentMethod: document.querySelector(
        'input[name="paymentMethod"]:checked'
      ).value,
      shippingMethod: "flat", // Default to flat rate shipping

      // Cart data
      cartItems: this.cartItems.map((item) => ({
        productId: item.product_id,
        quantity: item.quantity,
        unitPrice: item.unit_price,
        lineTotal: item.line_total || item.unit_price * item.quantity,
        productName: item.product_name,
      })),
      subtotal:
        this.cartSummary?.total_amount || this.cartSummary?.totalAmount || 0,
      totalAmount:
        this.cartSummary?.total_amount || this.cartSummary?.totalAmount || 0, // Total equals subtotal
    };

    // Debug logging
    console.log("Cart summary in collectOrderData:", this.cartSummary);
    console.log("Cart items in collectOrderData:", this.cartItems);
    console.log("Calculated values:", {
      subtotal: orderData.subtotal,
      totalAmount: orderData.totalAmount,
    });

    return orderData;
  }

  async submitOrder(orderData) {
    try {
      const token = localStorage.getItem("auth_token");
      if (!token) {
        throw new Error("Authentication required");
      }

      // Submit order to backend
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

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || "Failed to create order");
      }

      return await response.json();
    } catch (error) {
      console.error("Error submitting order:", error);
      throw error;
    }
  }

  setLoadingState(loading) {
    const placeOrderBtn = document.getElementById("placeOrderBtn");
    const loadingSpinner = document.getElementById("orderLoadingSpinner");

    if (placeOrderBtn) {
      placeOrderBtn.disabled = loading;
    }

    if (loadingSpinner) {
      if (loading) {
        loadingSpinner.classList.add("show");
      } else {
        loadingSpinner.classList.remove("show");
      }
    }
  }

  showToast(message, type = "info") {
    // Create toast element
    const toast = document.createElement("div");
    toast.className = `toast toast-${type}`;
    toast.innerHTML = `
            <div class="toast-content">
                <i class="fa fa-${this.getToastIcon(type)}"></i>
                <span>${message}</span>
            </div>
        `;

    // Add to page
    document.body.appendChild(toast);

    // Show toast
    setTimeout(() => {
      toast.classList.add("show");
    }, 100);

    // Remove toast after 3 seconds
    setTimeout(() => {
      toast.remove();
    }, 3000);
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

// Export for use in HTML
export { CheckoutManager };
