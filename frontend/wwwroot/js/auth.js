// Authentication JavaScript Module
class AuthManager {
  constructor() {
    this.apiBaseUrl = "https://localhost:7062/api";
    this.token = localStorage.getItem("auth_token");
    this.user = JSON.parse(localStorage.getItem("user_data") || "null");

    this.init();
  }

  init() {
    // This function will be called both on initial load and after HTMX swaps.
    const setupUI = () => {
      this.setupFormHandlers();
      this.setupPasswordValidation();
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
            // response.errors can be either an object {"Email": ["error1"]} or a List<string> ["error1", "error2"]
            if (Array.isArray(response.errors)) {
              // Handle List<string> format
              response.errors.forEach((error) => {
                errorMessage += `<li>${error}</li>`;
              });
            } else {
              // Handle object format {"Email": ["error1"]}
              for (const key in response.errors) {
                if (response.errors.hasOwnProperty(key)) {
                  if (Array.isArray(response.errors[key])) {
                    response.errors[key].forEach((error) => {
                      errorMessage += `<li>${error}</li>`;
                    });
                  } else {
                    errorMessage += `<li>${response.errors[key]}</li>`;
                  }
                }
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
              // Kiểm tra vai trò của người dùng để chuyển hướng phù hợp
              const userRole = response.data.user.roleName;
              if (userRole === "Admin") {
                window.location.href = "/html/common/homepage.html";
              } else if (userRole === "Supplier") {
                window.location.href = "/html/supplier/view-flower-list.html";
              } else {
                // Người dùng thường và các vai trò khác
                window.location.href = "/html/common/homepage.html";
              }
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
          if (response.errors) {
            errorMessage += "<ul>";
            if (Array.isArray(response.errors)) {
              response.errors.forEach((error) => {
                errorMessage += `<li>${error}</li>`;
              });
            } else {
              // Handle object format if needed
              for (const key in response.errors) {
                if (response.errors.hasOwnProperty(key)) {
                  if (Array.isArray(response.errors[key])) {
                    response.errors[key].forEach((error) => {
                      errorMessage += `<li>${error}</li>`;
                    });
                  } else {
                    errorMessage += `<li>${response.errors[key]}</li>`;
                  }
                }
              }
            }
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

  // Show register form (for email not found scenario)
  showRegisterForm() {
    // Scroll to register form
    const registerForm = document.getElementById("register-form");
    if (registerForm) {
      registerForm.scrollIntoView({ behavior: "smooth" });

      // Highlight the register form briefly
      registerForm.style.boxShadow = "0 0 0 0.2rem rgba(0, 123, 255, 0.25)";
      setTimeout(() => {
        registerForm.style.boxShadow = "";
      }, 2000);
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

    // Real-time validation for all form fields
    this.setupRealTimeValidation();
  }

  // Setup real-time validation for form fields
  setupRealTimeValidation() {
    const forms = ["login-form", "register-form"];

    forms.forEach((formId) => {
      const form = document.getElementById(formId);
      if (form) {
        const inputs = form.querySelectorAll("input, textarea");

        inputs.forEach((input) => {
          // Validate on input change
          input.addEventListener("input", () => {
            this.validateField(input);
          });

          // Validate on blur (when user leaves the field)
          input.addEventListener("blur", () => {
            this.validateField(input);
          });

          // Clear validation on focus
          input.addEventListener("focus", () => {
            this.clearFieldValidation(input);
          });
        });
      }
    });
  }

  // Validate individual field
  validateField(field) {
    const value = field.value.trim();
    const fieldName = field.name;

    // Clear previous validation
    this.clearFieldValidation(field);

    // Skip validation for empty optional fields
    if (!field.hasAttribute("required") && !value) {
      return;
    }

    let isValid = true;
    let errorMessage = "";

    // Required field validation
    if (field.hasAttribute("required") && (!value || value.length === 0)) {
      isValid = false;
      errorMessage = `${field.placeholder || fieldName} is required.`;
    }
    // Whitespace validation
    else if (value && /^\s+$/.test(field.value)) {
      isValid = false;
      errorMessage = `${
        field.placeholder || fieldName
      } cannot contain only spaces.`;
    }
    // Leading/trailing spaces validation
    else if (value && field.value !== field.value.trim()) {
      isValid = false;
      errorMessage = `${
        field.placeholder || fieldName
      } cannot have leading or trailing spaces.`;
    }
    // Field-specific validation
    else if (value) {
      switch (fieldName) {
        case "email":
          const emailRegex = /^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$/;
          if (!emailRegex.test(value)) {
            isValid = false;
            errorMessage = "Please enter a valid email address.";
          } else if (value.length > 200) {
            isValid = false;
            errorMessage = "Email cannot exceed 200 characters.";
          }
          break;

        case "password":
          if (value.length < 6) {
            isValid = false;
            errorMessage = "Password must be at least 6 characters long.";
          } else if (value.length > 100) {
            isValid = false;
            errorMessage = "Password cannot exceed 100 characters.";
          } else {
            const hasLower = /[a-z]/.test(value);
            const hasUpper = /[A-Z]/.test(value);
            const hasNumber = /\d/.test(value);

            if (!hasLower || !hasUpper || !hasNumber) {
              isValid = false;
              errorMessage =
                "Password must contain at least one uppercase letter, one lowercase letter, and one number.";
            }
          }
          break;

        case "userName":
          const usernameRegex = /^[a-zA-Z0-9_]+$/;
          if (!usernameRegex.test(value)) {
            isValid = false;
            errorMessage =
              "Username can only contain letters, numbers, and underscores.";
          } else if (value.length > 100) {
            isValid = false;
            errorMessage = "Username cannot exceed 100 characters.";
          }
          break;

        case "fullName":
          if (value.length > 200) {
            isValid = false;
            errorMessage = "Full name cannot exceed 200 characters.";
          }
          break;

        case "phoneNumber":
          if (value) {
            // Vietnamese phone number validation
            const phoneRegex =
              /^(0|\+84)(3[2-9]|5[689]|7[06-9]|8[1-689]|9[0-46-9])[0-9]{7}$/;
            if (!phoneRegex.test(value.replace(/\s/g, ""))) {
              isValid = false;
              errorMessage =
                "Please enter a valid Vietnamese phone number (e.g., 0964901308, +84964901308)";
            } else if (value.length > 20) {
              isValid = false;
              errorMessage = "Phone number cannot exceed 20 characters.";
            }
          }
          break;

        case "address":
          if (value && value.length > 500) {
            isValid = false;
            errorMessage = "Address cannot exceed 500 characters.";
          }
          break;
      }
    }

    // Apply validation result
    if (!isValid) {
      this.showFieldError(field, errorMessage);
      field.classList.add("invalid");
    } else if (value) {
      field.classList.add("valid");
    }
  }

  // Clear field validation
  clearFieldValidation(field) {
    field.classList.remove("invalid", "valid");
    this.hideFieldError(field);
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
    const errorMessages = [];
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
      const inputs = form.querySelectorAll(
        "input[required], textarea[required]"
      );
      inputs.forEach((input) => {
        const value = input.value.trim();

        // Check for blank/whitespace-only inputs
        if (!value || value.length === 0) {
          isValid = false;
          errorMessages.push(`${input.placeholder || input.name} is required.`);
          return;
        }

        // Check for whitespace-only inputs
        if (/^\s+$/.test(input.value)) {
          isValid = false;
          errorMessages.push(
            `${input.placeholder || input.name} cannot contain only spaces.`
          );
          return;
        }

        // Check for leading/trailing spaces
        if (input.value !== input.value.trim()) {
          isValid = false;
          errorMessages.push(
            `${
              input.placeholder || input.name
            } cannot have leading or trailing spaces.`
          );
          return;
        }

        // Specific validations for login/register
        if (input.name === "email" && value) {
          const emailRegex = /^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$/;
          if (!emailRegex.test(value)) {
            isValid = false;
            errorMessages.push("Please enter a valid email address.");
          }

          // Check email length against database constraint
          if (value.length > 200) {
            isValid = false;
            errorMessages.push("Email cannot exceed 200 characters.");
          }
        }

        if (input.name === "password" && value) {
          if (value.length < 6) {
            isValid = false;
            errorMessages.push("Password must be at least 6 characters long.");
          }

          // Check password length against database constraint
          if (value.length > 100) {
            isValid = false;
            errorMessages.push("Password cannot exceed 100 characters.");
          }

          // Check password complexity
          const hasLower = /[a-z]/.test(value);
          const hasUpper = /[A-Z]/.test(value);
          const hasNumber = /\d/.test(value);

          if (!hasLower || !hasUpper || !hasNumber) {
            isValid = false;
            errorMessages.push(
              "Password must contain at least one uppercase letter, one lowercase letter, and one number."
            );
          }
        }

        if (input.name === "userName" && value) {
          // Check username format
          const usernameRegex = /^[a-zA-Z0-9_]+$/;
          if (!usernameRegex.test(value)) {
            isValid = false;
            errorMessages.push(
              "Username can only contain letters, numbers, and underscores."
            );
          }

          // Check username length against database constraint
          if (value.length > 100) {
            isValid = false;
            errorMessages.push("Username cannot exceed 100 characters.");
          }
        }

        if (input.name === "fullName" && value) {
          // Check full name length against database constraint
          if (value.length > 200) {
            isValid = false;
            errorMessages.push("Full name cannot exceed 200 characters.");
          }
        }

        if (input.name === "phoneNumber" && value) {
          // Check phone number format for Vietnamese numbers
          const phoneRegex =
            /^(0|\+84)(3[2-9]|5[689]|7[06-9]|8[1-689]|9[0-46-9])[0-9]{7}$/;
          if (!phoneRegex.test(value.replace(/\s/g, ""))) {
            isValid = false;
            errorMessages.push(
              "Please enter a valid phone number (e.g., 0964901308, +84964901308)"
            );
          }

          // Check phone number length against database constraint
          if (value.length > 20) {
            isValid = false;
            errorMessages.push("Phone number cannot exceed 20 characters.");
          }
        }

        if (input.name === "address" && value) {
          // Check address length against database constraint
          if (value.length > 500) {
            isValid = false;
            errorMessages.push("Address cannot exceed 500 characters.");
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
  }

  // Clear authentication data
  clearAuthData() {
    this.token = null;
    this.user = null;

    localStorage.removeItem("auth_token");
    localStorage.removeItem("user_data");
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
        await fetch(`${this.apiBaseUrl}/Auth/logout`, {
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

  // Make helper functions globally accessible
  window.showRegisterForm = function () {
    window.authManager.showRegisterForm();
  };
});

// Export for use in other modules
if (typeof module !== "undefined" && module.exports) {
  module.exports = AuthManager;
}
