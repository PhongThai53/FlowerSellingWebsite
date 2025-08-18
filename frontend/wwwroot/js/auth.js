// Authentication JavaScript Module
class AuthManager {
  constructor() {
    this.apiBaseUrl = "http://localhost:5062/api";
    this.token = localStorage.getItem("auth_token");
    this.user = JSON.parse(localStorage.getItem("user_data") || "null");

    this.init();
  }

  init() {
    // This function will be called both on initial load and after HTMX swaps.
    const setupUI = () => {
      this.updateUserInterface();
      this.setupFormHandlers();
      this.setupPasswordValidation();
      this.setupForgotPasswordFormToggle();
    };

    // Run on initial page load
    if (document.readyState === "loading") {
      document.addEventListener("DOMContentLoaded", setupUI);
    } else {
      setupUI();
    }

    // Listen for HTMX content swaps to re-initialize UI components
    document.body.addEventListener("htmx:afterSwap", setupUI);

    // This listener is for handling API responses, not for UI setup
    this.setupHTMXEventListeners();
  }

  setupHTMXEventListeners() {
    document.body.addEventListener("htmx:beforeRequest", (event) => {
      const form = event.target;
      if (form.tagName === "FORM") {
        if (!this.validateForm(form)) {
          event.preventDefault(); // Stop the request if validation fails
          return;
        }
        this.showLoadingState(form);
      }
    });

    document.body.addEventListener("htmx:afterRequest", (event) => {
      // Only handle responses from the login or register forms
      if (
        event.target.id === "login-form" ||
        event.target.id === "register-form" ||
        event.target.id === "forgot-password-form" ||
        event.target.id === "reset-password-form"
      ) {
        this.hideLoadingState(event.target);
        this.handleAuthResponse(event);
      }
    });
  }

  handleAuthResponse(event) {
    try {
      const contentType = event.detail.xhr.getResponseHeader("Content-Type");
      if (contentType && contentType.includes("application/json")) {
        const response = JSON.parse(event.detail.xhr.responseText);
        const formType =
          event.target.id === "login-form" ? "login" : "register";
        const responseContainer =
          formType === "login" ? "#login-response" : "#register-response";

        if (event.detail.failed) {
          // This handles server-side validation errors (e.g., 400 Bad Request)
          let errorMessage = "An error occurred. Please check your input.";
          if (response && response.errors) {
            errorMessage = "<ul>";
            // response.errors is an object like {"Email": ["error1"], "Password": ["error2"]}
            for (const key in response.errors) {
              if (response.errors.hasOwnProperty(key)) {
                response.errors[key].forEach((error) => {
                  errorMessage += `<li>${error}</li>`;
                });
              }
            }
            errorMessage += "</ul>";
          } else if (response && response.message) {
            errorMessage = response.message;
          }
          this.showErrorMessage(responseContainer, errorMessage);
        } else if (response.succeeded && response.data) {
          // This handles successful responses
          if (formType === "login") {
            this.setAuthData(response.data.token, response.data.user);
          }

          this.showSuccessMessage(responseContainer, response.message);

          if (formType === "login") {
            setTimeout(() => {
              window.location.href = "/html/common/homepage.html";
            }, 1500);
          } else {
            // Registration successful - show email verification message
            const verificationMessage = `
              <div class="alert alert-info">
                <h5><i class="fas fa-envelope"></i> Registration Successful!</h5>
                <p><strong>Please check your email inbox to verify your account.</strong></p>
                <p>We've sent a verification link to your email address. Click the link to activate your account and complete the registration process.</p>
                <p class="mb-0"><small>Didn't receive the email? Check your spam folder or <a href="/html/auth/verification-failed.html">request a new verification email</a>.</small></p>
              </div>
            `;
            this.showSuccessMessage(responseContainer, verificationMessage);

            // Clear the form fields on successful registration
            event.target.reset();
          }
        } else {
          // This handles cases where Succeeded is false but not a 4xx/5xx error
          let errorMessage = response.message || "Authentication failed.";
          if (response.errors && response.errors.length > 0) {
            errorMessage += "<ul>";
            response.errors.forEach((error) => {
              errorMessage += `<li>${error}</li>`;
            });
            errorMessage += "</ul>";
          }
          this.showErrorMessage(responseContainer, errorMessage);
        }
      }
      // If the response is HTML, do nothing and let HTMX handle the swap.
    } catch (error) {
      console.error("Error parsing auth response:", error);
      this.showErrorMessage(
        event.target.id === "login-form"
          ? "#login-response"
          : "#register-response",
        "An unexpected error occurred. Please try again."
      );
    }
  }

  setupForgotPasswordFormToggle() {
    const forgotPasswordLink = document.getElementById("forgot-password-link");
    const backToLoginLink = document.getElementById("back-to-login-link");
    const loginFormContainer = document.getElementById("login-form-container");
    const forgotPasswordContainer = document.getElementById(
      "forgot-password-container"
    );

    if (
      forgotPasswordLink &&
      backToLoginLink &&
      loginFormContainer &&
      forgotPasswordContainer
    ) {
      forgotPasswordLink.addEventListener("click", (e) => {
        e.preventDefault();
        loginFormContainer.style.display = "none";
        forgotPasswordContainer.style.display = "block";
      });

      backToLoginLink.addEventListener("click", (e) => {
        e.preventDefault();
        loginFormContainer.style.display = "block";
        forgotPasswordContainer.style.display = "none";
      });
    }
  }

  // Show loading state on form submission
  showLoadingState(form) {
    const submitBtn = form.querySelector('button[type="submit"]');
    const textSpan = submitBtn.querySelector(".btn-text");
    const loadingSpan = submitBtn.querySelector(".btn-loading");

    if (textSpan && loadingSpan) {
      textSpan.style.display = "none";
      loadingSpan.style.display = "inline";
      submitBtn.disabled = true;
    }
  }

  // Hide loading state after form submission
  hideLoadingState(form) {
    const submitButton = form.querySelector('button[type="submit"]');
    if (submitButton) {
      submitButton.disabled = false;
      const textSpan = submitButton.querySelector(".btn-text");
      const loadingSpan = submitButton.querySelector(".btn-loading");
      if (textSpan && loadingSpan) {
        textSpan.style.display = "inline";
        loadingSpan.style.display = "none";
      }
    }
  }

  // Setup form validation and handlers
  setupFormHandlers() {
    // Password confirmation validation
    const confirmPasswordField = document.getElementById("confirm-password");
    const passwordField = document.getElementById("register-password");

    if (confirmPasswordField && passwordField) {
      confirmPasswordField.addEventListener("input", () => {
        // This is a minimal, non-intrusive check that can stay,
        // as it only validates one field against another.
        const passwordValue = passwordField.value;
        if (passwordValue) {
          this.validatePasswordMatch(passwordValue, confirmPasswordField.value);
        }
      });

      passwordField.addEventListener("input", () => {
        if (confirmPasswordField.value) {
          this.validatePasswordMatch(
            passwordField.value,
            confirmPasswordField.value
          );
        }
      });
    }
  }

  // Setup password strength validation
  setupPasswordValidation() {
    const passwordField = document.getElementById("register-password");
    if (passwordField) {
      passwordField.addEventListener("input", (e) => {
        this.showPasswordStrength(e.target.value, e.target);
      });
    }
  }

  // Validate password match
  validatePasswordMatch(password, confirmPassword) {
    const confirmField = document.getElementById("confirm-password");
    const errorElement = confirmField.parentNode.querySelector(".field-error");

    if (password !== confirmPassword) {
      confirmField.classList.add("invalid");
      confirmField.classList.remove("valid");
      this.showFieldError(confirmField, "Passwords do not match");
    } else {
      confirmField.classList.remove("invalid");
      confirmField.classList.add("valid");
      this.hideFieldError(confirmField);
    }
  }

  // Show password strength indicator
  showPasswordStrength(password, field) {
    let strengthElement = field.parentNode.querySelector(".password-strength");

    if (!strengthElement) {
      strengthElement = document.createElement("div");
      strengthElement.className = "password-strength";
      field.parentNode.appendChild(strengthElement);
    }

    const strength = this.calculatePasswordStrength(password);
    strengthElement.textContent = `Password strength: ${strength.text}`;
    strengthElement.className = `password-strength ${strength.class}`;
  }

  // Calculate password strength
  calculatePasswordStrength(password) {
    let score = 0;

    if (password.length >= 8) score++;
    if (/[a-z]/.test(password)) score++;
    if (/[A-Z]/.test(password)) score++;
    if (/[0-9]/.test(password)) score++;
    if (/[^A-Za-z0-9]/.test(password)) score++;

    if (score < 3) return { text: "Weak", class: "weak" };
    if (score < 4) return { text: "Medium", class: "medium" };
    return { text: "Strong", class: "strong" };
  }

  // Validate username format
  validateUsername(username) {
    const usernameField = document.querySelector('input[name="userName"]');
    const pattern = /^[a-zA-Z0-9_]+$/;

    if (!pattern.test(username)) {
      usernameField.classList.add("invalid");
      this.showFieldError(
        usernameField,
        "Username can only contain letters, numbers, and underscores"
      );
    } else {
      usernameField.classList.remove("invalid");
      usernameField.classList.add("valid");
      this.hideFieldError(usernameField);
    }
  }

  // Validate email format
  validateEmail(email) {
    const emailField = document.querySelector('input[name="email"]');
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    if (!emailPattern.test(email)) {
      emailField.classList.add("invalid");
      this.showFieldError(emailField, "Please enter a valid email address");
    } else {
      emailField.classList.remove("invalid");
      emailField.classList.add("valid");
      this.hideFieldError(emailField);
    }
  }

  validateForm(form) {
    let isValid = true;
    const responseContainerId = `#${form.id.replace("form", "response")}`;
    const responseContainer = form.querySelector(responseContainerId);

    // Clear previous errors
    if (responseContainer) {
      responseContainer.style.display = "none";
      responseContainer.innerHTML = "";
    }

    if (form.id === "reset-password-form") {
      const newPassword = form.querySelector('input[name="NewPassword"]').value;
      const confirmPassword = form.querySelector(
        'input[name="ConfirmPassword"]'
      ).value;

      if (newPassword.length < 6) {
        isValid = false;
        errorMessages.push("Password must be at least 6 characters long.");
      }
      if (newPassword !== confirmPassword) {
        isValid = false;
        errorMessages.push("Passwords do not match.");
      }
    } else {
      const inputs = form.querySelectorAll("input[required]");
      inputs.forEach((input) => {
        const value = input.value.trim();
        if (!value) {
          isValid = false;
          errorMessages.push(`${input.placeholder} is required.`);
        }

        // Specific validations for login/register
        if (input.name === "email" && value) {
          const emailRegex = /^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$/;
          if (!emailRegex.test(value)) {
            isValid = false;
            errorMessages.push("Please enter a valid email address.");
          }
        }

        if (input.name === "password" && value) {
          if (value.length < 6) {
            isValid = false;
            errorMessages.push("Password must be at least 6 characters long.");
          }
        }
      });

      // Password confirmation check for register form
      if (form.id === "register-form") {
        const password = form.querySelector('input[name="password"]').value;
        const confirmPassword = form.querySelector(
          'input[name="confirmPassword"]'
        ).value;
        if (password !== confirmPassword) {
          isValid = false;
          errorMessages.push("Passwords do not match.");
        }
      }
    }

    if (!isValid) {
      let errorMessage = "<ul>";
      errorMessages.forEach((error) => {
        errorMessage += `<li>${error}</li>`;
      });
      errorMessage += "</ul>";
      this.showErrorMessage(responseContainerId, errorMessage);
    }

    return isValid;
  }

  // Show field-specific error
  showFieldError(field, message) {
    this.hideFieldError(field); // Remove existing error first

    const errorElement = document.createElement("span");
    errorElement.className = "field-error";
    errorElement.textContent = message;
    field.parentNode.appendChild(errorElement);
  }

  // Hide field-specific error
  hideFieldError(field) {
    const errorElement = field.parentNode.querySelector(".field-error");
    if (errorElement) {
      errorElement.remove();
    }
  }

  // Show success message
  showSuccessMessage(container, message) {
    const responseContainer = document.querySelector(container);
    if (responseContainer) {
      responseContainer.innerHTML = `<div class="alert alert-success">${message}</div>`;
      responseContainer.style.display = "block";
    }
  }

  // Show error message
  showErrorMessage(container, message) {
    const responseContainer = document.querySelector(container);
    if (responseContainer) {
      responseContainer.innerHTML = `<div class="alert alert-danger">${message}</div>`;
      responseContainer.style.display = "block";
    }
  }

  // Set authentication data
  setAuthData(token, user) {
    this.token = token;
    this.user = user;

    localStorage.setItem("auth_token", token);
    localStorage.setItem("user_data", JSON.stringify(user));

    this.updateUserInterface();
  }

  // Clear authentication data
  clearAuthData() {
    this.token = null;
    this.user = null;

    localStorage.removeItem("auth_token");
    localStorage.removeItem("user_data");

    this.updateUserInterface();
  }

  // Update user interface based on authentication state
  updateUserInterface() {
    const userDropdown = document.getElementById("user-dropdown");
    const mobileUserMenu = document.getElementById("mobile-user-menu");
    const adminUserManagement = document.getElementById("admin-user-management");

    // Safety check to ensure the elements exist
    if (!userDropdown && !mobileUserMenu) {
      return;
    }

    if (this.isAuthenticated()) {
      const welcomeMessage = `Hello, ${
        this.user.fullName || this.user.userName
      }`;

      // Check if user is Admin to show User Management menu
      const isAdmin = this.user && this.user.roleName === 'Admin';
      if (adminUserManagement) {
        adminUserManagement.style.display = isAdmin ? 'block' : 'none';
      }

      // Update desktop user menu for authenticated users
      if (userDropdown) {
        let userMenuItems = `
          <li><a href="/html/auth/my-account.html"><i class="lnr lnr-user"></i> My Account</a></li>
        `;
        
        // Add User Management link for Admin users
        if (isAdmin) {
          userMenuItems += `
            <li><a href="/html/user/user-list.html"><i class="fa fa-users"></i> Quản lý người dùng</a></li>
          `;
        }
        
        userMenuItems += `
          <li><a href="#" onclick="authManager.logout()"><i class="lnr lnr-exit"></i> Logout</a></li>
        `;
        
        userDropdown.innerHTML = userMenuItems;
      }

      // Update mobile user menu for authenticated users
      if (mobileUserMenu) {
        let mobileMenuItems = `
          <a class="dropdown-item" href="/html/auth/my-account.html">My Account</a>
        `;
        
        // Add User Management link for Admin users in mobile menu
        if (isAdmin) {
          mobileMenuItems += `
            <a class="dropdown-item" href="/html/user/user-list.html"><i class="fa fa-users"></i> Quản lý người dùng</a>
          `;
        }
        
        mobileMenuItems += `
          <a class="dropdown-item" href="#" onclick="authManager.logout()">Logout</a>
        `;
        
        mobileUserMenu.innerHTML = mobileMenuItems;
      }
    } else {
      // Hide User Management menu for non-authenticated users
      if (adminUserManagement) {
        adminUserManagement.style.display = 'none';
      }

      // Update desktop user menu for non-authenticated users
      if (userDropdown) {
        userDropdown.innerHTML = `
          <li><a href="/html/auth/login-register.html">Login</a></li>
          <li><a href="/html/auth/login-register.html">Register</a></li>
        `;
      }

      // Update mobile user menu for non-authenticated users
      if (mobileUserMenu) {
        mobileUserMenu.innerHTML = `
          <a class="dropdown-item" href="/html/auth/login-register.html">Login</a>
          <a class="dropdown-item" href="/html/auth/login-register.html">Register</a>
        `;
      }
    }
  }

  // Check if user is authenticated
  isAuthenticated() {
    return !!(this.token && this.user);
  }

  // Logout user
  async logout() {
    try {
      // Call logout endpoint if available
      if (this.token) {
        await fetch(`${this.apiBaseUrl}/auth/logout`, {
          method: "POST",
          headers: {
            Authorization: `Bearer ${this.token}`,
            "Content-Type": "application/json",
          },
        });
      }
    } catch (error) {
      console.error("Error during logout:", error);
    } finally {
      // Clear local auth data regardless of API call success
      this.clearAuthData();

      // Redirect to homepage
      window.location.href = "/html/common/homepage.html";
    }
  }

  // Get auth token for API requests
  getAuthToken() {
    return this.token;
  }

  // Get current user data
  getCurrentUser() {
    return this.user;
  }
}

// Initialize authentication manager when DOM is loaded
document.addEventListener("DOMContentLoaded", function () {
  window.authManager = new AuthManager();
});

// Export for use in other modules
if (typeof module !== "undefined" && module.exports) {
  module.exports = AuthManager;
}
