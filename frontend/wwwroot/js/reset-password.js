/**
 * Reset Password Page JavaScript
 * Handles the reset password form submission and validation
 */

import { API_CONFIG } from "./config/api-config.js";

class ResetPasswordManager {
  constructor() {
    this.initialize();
  }

  initialize() {
    this.setupForm();
    this.setupValidation();
    this.extractTokenFromURL();
  }

  extractTokenFromURL() {
    const urlParams = new URLSearchParams(window.location.search);
    const token = urlParams.get("token");
    if (token) {
      document.getElementById("token").value = token;
    }
  }

  setupForm() {
    const form = document.getElementById("reset-password-form");
    if (form) {
      form.addEventListener("submit", (event) => {
        this.handleSubmit(event);
      });
    }
  }

  setupValidation() {
    const newPasswordField = document.getElementById("newPassword");
    const confirmPasswordField = document.getElementById("confirmPassword");
    const newPasswordValidation = document.getElementById(
      "new-password-validation"
    );
    const confirmPasswordValidation = document.getElementById(
      "confirm-password-validation"
    );
    const passwordMatchError = document.getElementById("password-match-error");

    if (newPasswordField && newPasswordValidation) {
      // Real-time validation for new password
      newPasswordField.addEventListener("input", () => {
        this.validateNewPassword(newPasswordField, newPasswordValidation);
        this.validatePasswordMatch();
      });

      newPasswordField.addEventListener("blur", () => {
        this.validateNewPassword(newPasswordField, newPasswordValidation);
      });
    }

    if (confirmPasswordField && confirmPasswordValidation) {
      // Real-time validation for confirm password
      confirmPasswordField.addEventListener("input", () => {
        this.validateConfirmPassword(
          confirmPasswordField,
          confirmPasswordValidation
        );
        this.validatePasswordMatch();
      });

      confirmPasswordField.addEventListener("blur", () => {
        this.validateConfirmPassword(
          confirmPasswordField,
          confirmPasswordValidation
        );
      });
    }
  }

  validateNewPassword(passwordField, validationElement) {
    const password = passwordField.value.trim();

    // Clear previous validation
    this.clearValidation(passwordField, validationElement);

    if (!password) {
      this.showValidationError(
        passwordField,
        validationElement,
        "New password is required"
      );
      return false;
    }

    if (password.length < 6) {
      this.showValidationError(
        passwordField,
        validationElement,
        "Password must be at least 6 characters long"
      );
      return false;
    }

    if (password.length > 100) {
      this.showValidationError(
        passwordField,
        validationElement,
        "Password cannot exceed 100 characters"
      );
      return false;
    }

    // Check for leading/trailing spaces
    if (password !== passwordField.value) {
      this.showValidationError(
        passwordField,
        validationElement,
        "Password cannot have leading or trailing spaces"
      );
      return false;
    }

    // Check password complexity
    const passwordRegex =
      /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d@$!%*?&]{6,}$/;
    if (!passwordRegex.test(password)) {
      this.showValidationError(
        passwordField,
        validationElement,
        "Password must contain at least one uppercase letter, one lowercase letter, and one number"
      );
      return false;
    }

    this.showValidationSuccess(passwordField, validationElement);
    return true;
  }

  validateConfirmPassword(confirmField, validationElement) {
    const confirmPassword = confirmField.value.trim();

    // Clear previous validation
    this.clearValidation(confirmField, validationElement);

    if (!confirmPassword) {
      this.showValidationError(
        confirmField,
        validationElement,
        "Password confirmation is required"
      );
      return false;
    }

    if (confirmPassword.length < 6) {
      this.showValidationError(
        confirmField,
        validationElement,
        "Password confirmation must be at least 6 characters long"
      );
      return false;
    }

    if (confirmPassword.length > 100) {
      this.showValidationError(
        confirmField,
        validationElement,
        "Password confirmation cannot exceed 100 characters"
      );
      return false;
    }

    // Check for leading/trailing spaces
    if (confirmPassword !== confirmField.value) {
      this.showValidationError(
        confirmField,
        validationElement,
        "Password confirmation cannot have leading or trailing spaces"
      );
      return false;
    }

    this.showValidationSuccess(confirmField, validationElement);
    return true;
  }

  validatePasswordMatch() {
    const newPassword = document.getElementById("newPassword").value;
    const confirmPassword = document.getElementById("confirmPassword").value;
    const passwordMatchError = document.getElementById("password-match-error");

    if (newPassword && confirmPassword && newPassword !== confirmPassword) {
      passwordMatchError.style.display = "block";
      return false;
    } else {
      passwordMatchError.style.display = "none";
      return true;
    }
  }

  showValidationError(field, messageElement, message) {
    field.classList.add("invalid");
    field.classList.remove("valid");
    messageElement.textContent = message;
    messageElement.style.display = "block";
  }

  showValidationSuccess(field, messageElement) {
    field.classList.add("valid");
    field.classList.remove("invalid");
    messageElement.style.display = "none";
    messageElement.textContent = "";
  }

  clearValidation(field, messageElement) {
    field.classList.remove("invalid", "valid");
    messageElement.style.display = "none";
    messageElement.textContent = "";
  }

  validateForm() {
    const newPasswordField = document.getElementById("newPassword");
    const confirmPasswordField = document.getElementById("confirmPassword");
    const newPasswordValidation = document.getElementById(
      "new-password-validation"
    );
    const confirmPasswordValidation = document.getElementById(
      "confirm-password-validation"
    );

    const newPasswordValid = this.validateNewPassword(
      newPasswordField,
      newPasswordValidation
    );
    const confirmPasswordValid = this.validateConfirmPassword(
      confirmPasswordField,
      confirmPasswordValidation
    );
    const passwordsMatch = this.validatePasswordMatch();

    return newPasswordValid && confirmPasswordValid && passwordsMatch;
  }

  async handleSubmit(event) {
    event.preventDefault();

    if (!this.validateForm()) {
      return;
    }

    const form = event.target;
    const submitBtn = form.querySelector("#reset-submit-btn");
    const btnText = submitBtn.querySelector(".btn-text");
    const btnLoading = submitBtn.querySelector(".btn-loading");

    // Show loading state
    btnText.style.display = "none";
    btnLoading.style.display = "inline-block";
    submitBtn.disabled = true;

    try {
      const formData = new FormData(form);
      const data = {
        Token: formData.get("Token"),
        NewPassword: formData.get("NewPassword").trim(),
        ConfirmPassword: formData.get("ConfirmPassword").trim(),
      };

      const response = await fetch(
        `${API_CONFIG.API_URL}/auth/reset-password`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(data),
        }
      );

      const jsonResponse = await response.json();
      const responseArea = document.getElementById("reset-response");

      // Create message HTML based on response
      let messageHtml = "";
      if (response.ok) {
        // Success case
        messageHtml = `
                     <div class="alert alert-success">
                         <h5><i class="fas fa-check-circle"></i> ${jsonResponse.message}</h5>
                         <p><strong>${jsonResponse.data}</strong></p>
                     </div>`;
        form.reset();
        this.clearAllValidation();
      } else {
        // Error case
        const errorMessage = Array.isArray(jsonResponse.errors)
          ? jsonResponse.errors.join(", ")
          : jsonResponse.errors || jsonResponse.message;

        messageHtml = `
                     <div class="alert alert-danger">
                         <h5><i class="fas fa-exclamation-triangle"></i> ${jsonResponse.message}</h5>
                         <p><strong>${errorMessage}</strong></p>
                     </div>`;
      }

      // Update response area
      responseArea.innerHTML = messageHtml;
      responseArea.style.display = "block";
    } catch (error) {
      // Handle network/JavaScript errors
      const responseArea = document.getElementById("reset-response");
      responseArea.innerHTML = `
                <div class="alert alert-danger">
                    <h5><i class="fas fa-exclamation-triangle"></i> Network Error</h5>
                    <p>An error occurred while connecting to the server. Please try again.</p>
                </div>`;
      responseArea.style.display = "block";
    } finally {
      // Reset button state
      btnText.style.display = "inline-block";
      btnLoading.style.display = "none";
      submitBtn.disabled = false;
    }
  }

  clearAllValidation() {
    const fields = ["newPassword", "confirmPassword"];
    const validations = [
      "new-password-validation",
      "confirm-password-validation",
    ];

    fields.forEach((fieldId, index) => {
      const field = document.getElementById(fieldId);
      const validation = document.getElementById(validations[index]);
      if (field && validation) {
        this.clearValidation(field, validation);
      }
    });

    // Clear password match error
    const passwordMatchError = document.getElementById("password-match-error");
    if (passwordMatchError) {
      passwordMatchError.style.display = "none";
    }
  }
}

// Initialize when DOM is loaded
document.addEventListener("DOMContentLoaded", () => {
  new ResetPasswordManager();
});
