// My Account Management JavaScript
class MyAccountManager {
  constructor() {
    this.apiBaseUrl = "https://localhost:7062/api";
    this.token = localStorage.getItem("auth_token"); // Get token at instantiation
    this.currentUser = null;
    this.init();
  }

  async init() {
    if (!this.isAuthenticated()) {
      this.showAlert(
        "Authentication Required",
        "Please log in to access your account.",
        "warning"
      );
      setTimeout(() => {
        window.location.href = "/html/auth/login-register.html";
      }, 2000);
      return; // Stop initialization if not authenticated
    }

    // Bind events immediately
    this.bindEvents();

    // Load user profile and update UI
    await this.loadUserProfile();
  }

  isAuthenticated() {
    // A simple check for the token's existence.
    return !!this.token;
  }

  bindEvents() {
    // Account form submission
    const accountForm = document.getElementById("account-form");
    if (accountForm) {
      accountForm.addEventListener("submit", (e) =>
        this.handleAccountUpdate(e)
      );
    }

    // Enhanced validation for form fields
    this.setupFieldValidation();

    // Password form submission
    const passwordForm = document.getElementById("password-form");
    if (passwordForm) {
      passwordForm.addEventListener("submit", (e) =>
        this.handlePasswordChange(e)
      );
    }

    // Logout button
    const logoutBtn = document.getElementById("logout-btn");
    if (logoutBtn) {
      logoutBtn.addEventListener("click", (e) => this.handleLogout(e));
    }

    // Tab switching
    const tabButtons = document.querySelectorAll(
      '.myaccount-tab-menu a[data-bs-toggle="tab"]'
    );
    tabButtons.forEach((tab) => {
      tab.addEventListener("click", (e) => this.handleTabSwitch(e));
    });

    // Password confirmation validation
    const confirmPasswordField = document.getElementById("confirm-password");
    if (confirmPasswordField) {
      confirmPasswordField.addEventListener("input", () =>
        this.validatePasswordMatch()
      );
    }

    const newPasswordField = document.getElementById("new-password");
    if (newPasswordField) {
      newPasswordField.addEventListener("input", () =>
        this.validatePasswordStrength()
      );
    }
  }

  async loadUserProfile() {
    try {
      this.showLoading("dashboard-username");

      const response = await this.makeAuthenticatedRequest(
        "/MyAccount/profile",
        "GET"
      );

      if (response.succeeded && response.data) {
        this.currentUser = response.data;
        this.populateUserData();
        // Address functionality removed - no longer needed
      } else {
        throw new Error(response.message || "Failed to load profile data.");
      }
    } catch (error) {
      console.error("Error loading user profile:", error);
      this.showAlert(
        "Error",
        error.message || "Failed to load user profile. Please try again.",
        "danger"
      );
      // If profile fails to load with an auth error, the token is likely invalid. Redirect to login.
      if (error.isAuthError) {
        setTimeout(() => this.performLogout(true), 2000);
      }
    }
  }

  populateUserData() {
    if (!this.currentUser) return;

    // Update dashboard
    const dashboardUsername = document.getElementById("dashboard-username");
    if (dashboardUsername) {
      dashboardUsername.textContent =
        this.currentUser.fullName || this.currentUser.userName;
    }

    // Populate account form
    const fullnameField = document.getElementById("fullname");
    const usernameField = document.getElementById("username");
    const phoneField = document.getElementById("phone");
    const addressField = document.getElementById("address");

    if (fullnameField) fullnameField.value = this.currentUser.fullName || "";
    if (usernameField) usernameField.value = this.currentUser.userName || "";
    if (phoneField) phoneField.value = this.currentUser.phoneNumber || "";
    if (addressField) addressField.value = this.currentUser.address || "";
  }

  // Refresh dashboard with current user data
  refreshDashboard() {
    if (this.currentUser) {
      this.populateUserData();
    }
  }

  // Address functionality removed - no longer needed

  switchToTab(tabId) {
    // Remove active class from all tabs
    document.querySelectorAll(".myaccount-tab-menu a").forEach((tab) => {
      tab.classList.remove("active");
    });
    document.querySelectorAll(".tab-pane").forEach((pane) => {
      pane.classList.remove("show", "active");
    });

    // Activate target tab
    const targetTab = document.querySelector(`[href="#${tabId}"]`);
    const targetPane = document.getElementById(tabId);

    if (targetTab && targetPane) {
      targetTab.classList.add("active");
      targetPane.classList.add("show", "active");
    }
  }

  async handleAccountUpdate(e) {
    e.preventDefault();

    const form = e.target;
    const formData = new FormData(form);

    const updateData = {
      fullName: formData.get("fullname"),
      userName: formData.get("username"),
      phone: formData.get("phone"),
      address: formData.get("address"),
    };

    if (!this.validateAccountData(updateData)) {
      return;
    }

    try {
      this.setButtonLoading("save-account-btn", true);

      const response = await this.makeAuthenticatedRequest(
        "/MyAccount/profile",
        "PUT",
        updateData
      );

      if (response.succeeded) {
        this.currentUser = response.data;
        this.showAlert("Success", "Account updated successfully!", "success");

        // Refresh dashboard to show updated fullname
        this.refreshDashboard();

        // Address functionality removed - no longer needed
      } else {
        throw new Error(response.message || "Failed to update account");
      }
    } catch (error) {
      console.error("Error updating account:", error);
      this.showAlert(
        "Error",
        error.message || "Failed to update account. Please try again.",
        "danger"
      );
    } finally {
      this.setButtonLoading("save-account-btn", false);
    }
  }

  async handlePasswordChange(e) {
    e.preventDefault();

    const form = e.target;
    const formData = new FormData(form);

    const passwordData = {
      currentPassword: formData.get("currentPassword"),
      newPassword: formData.get("newPassword"),
      confirmPassword: formData.get("confirmPassword"),
    };

    if (!this.validatePasswordData(passwordData)) {
      return;
    }

    try {
      this.setButtonLoading("change-password-btn", true);

      const response = await this.makeAuthenticatedRequest(
        "/MyAccount/change-password",
        "POST",
        passwordData
      );

      if (response.succeeded) {
        this.showAlert(
          "Success",
          "Password changed successfully! Please log in again.",
          "success"
        );
        form.reset();

        // Logout after successful password change
        setTimeout(() => {
          this.performLogout();
        }, 2000);
      } else {
        throw new Error(response.message || "Failed to change password");
      }
    } catch (error) {
      console.error("Error changing password:", error);
      this.showAlert(
        "Error",
        error.message || "Failed to change password. Please try again.",
        "danger"
      );
    } finally {
      this.setButtonLoading("change-password-btn", false);
    }
  }

  handleLogout(e) {
    e.preventDefault();

    if (confirm("Are you sure you want to logout?")) {
      this.performLogout();
    }
  }

  async performLogout(isError = false) {
    try {
      if (!isError) {
        await this.makeAuthenticatedRequest("/Auth/logout", "POST");
      }
    } catch (error) {
      console.error("Error during server logout:", error);
    } finally {
      localStorage.removeItem("auth_token");
      localStorage.removeItem("user_data");

      if (!isError) {
        this.showAlert("Success", "You have been logged out.", "success");
      }

      setTimeout(() => {
        window.location.href = "/html/auth/login-register.html";
      }, 1500);
    }
  }

  handleTabSwitch(e) {
    const targetTab = e.target.getAttribute("href");

    // Load specific data when switching to certain tabs
    // Address functionality removed - no longer needed
  }

  validateAccountData(data) {
    if (!data.fullName || data.fullName.trim().length < 2) {
      this.showAlert(
        "Validation Error",
        "Full name must be at least 2 characters long.",
        "warning"
      );
      return false;
    }

    if (!data.userName || data.userName.trim().length < 3) {
      this.showAlert(
        "Validation Error",
        "Username must be at least 3 characters long.",
        "warning"
      );
      return false;
    }

    if (!data.phone || !this.isValidPhone(data.phone)) {
      this.showAlert(
        "Validation Error",
        "Please enter a valid phone number.",
        "warning"
      );
      return false;
    }

    return true;
  }

  validatePasswordData(data) {
    if (!data.currentPassword) {
      this.showAlert(
        "Validation Error",
        "Current password is required.",
        "warning"
      );
      return false;
    }

    if (!data.newPassword) {
      this.showAlert(
        "Validation Error",
        "New password is required.",
        "warning"
      );
      return false;
    }

    if (!this.isValidPassword(data.newPassword)) {
      this.showAlert(
        "Validation Error",
        "Password must contain at least 8 characters, including 1 lowercase letter, 1 uppercase letter, 1 number, and 1 special character.",
        "warning"
      );
      return false;
    }

    if (data.newPassword !== data.confirmPassword) {
      this.showAlert(
        "Validation Error",
        "New password and confirm password do not match.",
        "warning"
      );
      return false;
    }

    return true;
  }

  validatePasswordMatch() {
    const newPassword = document.getElementById("new-password").value;
    const confirmPassword = document.getElementById("confirm-password").value;
    const confirmField = document.getElementById("confirm-password");

    if (confirmPassword && newPassword !== confirmPassword) {
      confirmField.setCustomValidity("Passwords do not match");
      confirmField.classList.add("is-invalid");
    } else {
      confirmField.setCustomValidity("");
      confirmField.classList.remove("is-invalid");
    }
  }

  validatePasswordStrength() {
    const password = document.getElementById("new-password").value;
    const field = document.getElementById("new-password");

    if (password && !this.isValidPassword(password)) {
      field.setCustomValidity(
        "Password must contain at least 8 characters, including 1 lowercase letter, 1 uppercase letter, 1 number, and 1 special character."
      );
      field.classList.add("is-invalid");
    } else {
      field.setCustomValidity("");
      field.classList.remove("is-invalid");
    }
  }

  isValidPhone(phone) {
    // Remove spaces, dashes, parentheses, and other formatting
    const cleanPhone = phone.replace(/[\s\-\(\)\+]/g, "");
    // Allow phone numbers that start with 0 (like Vietnamese numbers) or country codes
    // Must be 10-15 digits long
    const phoneRegex = /^[0-9]{10,15}$/;
    return phoneRegex.test(cleanPhone);
  }

  isValidPassword(password) {
    // This regex enforces:
    // - at least 8 characters
    // - at least one lowercase letter
    // - at least one uppercase letter
    // - at least one number
    // - at least one special character from a wide set
    const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$/;
    return passwordRegex.test(password);
  }

  async makeAuthenticatedRequest(endpoint, method = "GET", data = null) {
    const token = localStorage.getItem("auth_token");
    if (!token) {
      throw new Error("No authentication token found");
    }

    const config = {
      method: method,
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
    };

    if (data && (method === "POST" || method === "PUT")) {
      config.body = JSON.stringify(data);
    }

    console.log(`Making ${method} request to: ${this.apiBaseUrl}${endpoint}`);
    console.log("Request config:", config);

    const response = await fetch(`${this.apiBaseUrl}${endpoint}`, config);

    console.log("Response status:", response.status);
    console.log("Response headers:", response.headers);

    // Any non-ok response needs to be handled as an error.
    if (!response.ok) {
      let errorMessage = `HTTP ${response.status}: An unexpected error occurred.`;
      const isAuthError = response.status === 401;

      try {
        const errorJson = await response.json();
        console.error("Error JSON:", errorJson);

        // Handle custom ApiResponse format: { message: "..." }
        if (errorJson.message) {
          errorMessage = errorJson.message;
        }
        // Handle ASP.NET ValidationProblemDetails format: { errors: { Field: ["message"] } }
        else if (errorJson.errors) {
          const firstErrorKey = Object.keys(errorJson.errors)[0];
          errorMessage = errorJson.errors[firstErrorKey][0];
        }
        // Handle simple string error
        else if (typeof errorJson === "string") {
          errorMessage = errorJson;
        } else {
          errorMessage = `HTTP ${response.status}: ${response.statusText}`;
        }
      } catch (e) {
        // The response body was not JSON or was empty.
        console.error("Could not parse error response as JSON.", e);
        errorMessage = `HTTP ${response.status}: ${
          response.statusText || "Server returned an error"
        }`;
      }

      const error = new Error(errorMessage);
      error.isAuthError = isAuthError; // Add a flag for auth errors
      throw error;
    }

    // If response is ok, but content is empty (e.g., 204 No Content)
    if (response.status === 204) {
      return null;
    }

    const result = await response.json();
    console.log("Response data:", result);
    return result;
  }

  showLoading(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
      element.innerHTML = '<i class="fa fa-spinner fa-spin"></i> Loading...';
    }
  }

  setButtonLoading(buttonId, isLoading) {
    const button = document.getElementById(buttonId);
    if (!button) return;

    if (isLoading) {
      button.disabled = true;
      const originalText = button.innerHTML;
      button.setAttribute("data-original-text", originalText);
      button.innerHTML = '<i class="fa fa-spinner fa-spin"></i> Loading...';
    } else {
      button.disabled = false;
      const originalText = button.getAttribute("data-original-text");
      if (originalText) {
        button.innerHTML = originalText;
      }
    }
  }

  showAlert(title, message, type = "info") {
    const alertModal = document.getElementById("alertModal");
    const alertModalLabel = document.getElementById("alertModalLabel");
    const alertModalBody = document.getElementById("alertModalBody");

    if (alertModalLabel) alertModalLabel.textContent = title;
    if (alertModalBody) {
      alertModalBody.innerHTML = `<div class="alert alert-${
        type === "danger"
          ? "danger"
          : type === "warning"
          ? "warning"
          : type === "success"
          ? "success"
          : "info"
      }">${message}</div>`;
    }

    if (alertModal) {
      const modal = new bootstrap.Modal(alertModal);
      modal.show();
    } else {
      // Fallback to browser alert
      alert(`${title}: ${message}`);
    }
  }

  // Enhanced validation methods
  setupFieldValidation() {
    // Setup real-time validation for all form fields
    this.setupRequiredFieldValidation();
    this.setupLengthValidation();
    this.setupFormatValidation();
  }

  setupRequiredFieldValidation() {
    // Required field validation - check for blank and space-only input
    const requiredFields = document.querySelectorAll(
      "input[required], textarea[required]"
    );

    requiredFields.forEach((field) => {
      field.addEventListener("blur", () => this.validateRequiredField(field));
      field.addEventListener("input", () => this.clearFieldError(field));
    });
  }

  setupLengthValidation() {
    // Length validation - check for maximum length to prevent DB overflow
    const textFields = document.querySelectorAll(
      'input[type="text"], input[type="tel"], textarea'
    );

    textFields.forEach((field) => {
      field.addEventListener("input", () => this.validateFieldLength(field));
    });
  }

  setupFormatValidation() {
    // Format validation for specific field types
    const phoneField = document.getElementById("phone");
    if (phoneField) {
      phoneField.addEventListener("blur", () =>
        this.validatePhoneFormat(phoneField)
      );
      phoneField.addEventListener("input", () =>
        this.clearFieldError(phoneField)
      );
    }
  }

  validateRequiredField(field) {
    const value = field.value.trim();

    if (!value || value.length === 0) {
      this.showFieldError(field, "This field is required and cannot be blank");
      return false;
    }

    if (value.replace(/\s/g, "").length === 0) {
      this.showFieldError(field, "This field cannot contain only spaces");
      return false;
    }

    this.clearFieldError(field);
    return true;
  }

  validateFieldLength(field) {
    const value = field.value;
    const maxLength = this.getMaxLengthForField(field);

    if (maxLength && value.length > maxLength) {
      this.showFieldError(field, `Maximum length is ${maxLength} characters`);
      return false;
    }

    this.clearFieldError(field);
    return true;
  }

  validatePhoneFormat(field) {
    const value = field.value.trim();

    if (!value) return true; // Required validation will handle empty

    // Remove all non-digit characters for validation
    const cleanPhone = value.replace(/[\s\-\(\)\+]/g, "");

    // Check if it's a valid phone number (10-15 digits)
    if (!/^[0-9]{10,15}$/.test(cleanPhone)) {
      this.showFieldError(
        field,
        "Please enter a valid phone number (10-15 digits)"
      );
      return false;
    }

    this.clearFieldError(field);
    return true;
  }

  getMaxLengthForField(field) {
    // Define maximum lengths based on field type/name to prevent DB overflow
    const maxLengths = {
      fullname: 100, // Full name max length
      username: 50, // Username max length
      phone: 20, // Phone max length (including formatting)
      address: 500, // Address max length
    };

    return maxLengths[field.name] || 255; // Default max length
  }

  showFieldError(field, message) {
    // Remove existing error
    this.clearFieldError(field);

    // Add error styling
    field.classList.add("is-invalid");

    // Create error message element
    const errorDiv = document.createElement("div");
    errorDiv.className = "invalid-feedback";
    errorDiv.textContent = message;
    errorDiv.id = `${field.id}-error`;

    // Insert error message after the field
    field.parentNode.appendChild(errorDiv);
  }

  clearFieldError(field) {
    field.classList.remove("is-invalid");

    const errorDiv = field.parentNode.querySelector(`#${field.id}-error`);
    if (errorDiv) {
      errorDiv.remove();
    }
  }
}

// Initialize the My Account Manager when the DOM is ready.
document.addEventListener("DOMContentLoaded", function () {
  // Ensure this script only runs on the my-account page by checking for a unique element.
  if (document.getElementById("myaccountContent")) {
    window.myAccountManager = new MyAccountManager();
  }
});
