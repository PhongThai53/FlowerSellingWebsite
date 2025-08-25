/**
 * Forgot Password Page JavaScript
 * Handles the forgot password form submission and validation
 */

class ForgotPasswordManager {
  constructor() {
    this.initialize();
  }

  initialize() {
    this.setupForm();
    this.setupValidation();
  }

  setupForm() {
    const form = document.getElementById("forgot-password-form");
    if (form) {
      form.addEventListener("submit", (event) => {
        this.handleSubmit(event);
      });
    }
  }

  setupValidation() {
    const emailField = document.getElementById("forgot-password-email");
    const validationMessage = document.getElementById(
      "forgot-password-email-validation"
    );

    if (emailField && validationMessage) {
      // Real-time validation
      emailField.addEventListener("input", () => {
        this.validateEmail(emailField, validationMessage);
      });

      emailField.addEventListener("blur", () => {
        this.validateEmail(emailField, validationMessage);
      });
    }
  }

  validateEmail(emailField, validationMessage) {
    const email = emailField.value.trim();

    // Clear previous validation
    this.clearValidation(emailField, validationMessage);

    if (!email) {
      this.showValidationError(
        emailField,
        validationMessage,
        "Email is required"
      );
      return false;
    }

    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    if (!emailRegex.test(email)) {
      this.showValidationError(
        emailField,
        validationMessage,
        "Please enter a valid email address"
      );
      return false;
    }

    this.showValidationSuccess(emailField, validationMessage);
    return true;
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

  async handleSubmit(event) {
    event.preventDefault();

    const form = event.target;
    const email = form.querySelector('input[name="email"]').value.trim();
    const messageArea = document.getElementById("forgot-password-message");
    const submitBtn = form.querySelector("#forgot-password-submit-btn");
    const btnText = submitBtn.querySelector(".btn-text");
    const btnLoading = submitBtn.querySelector(".btn-loading");

    // Validate email
    const emailField = document.getElementById("forgot-password-email");
    const validationMessage = document.getElementById(
      "forgot-password-email-validation"
    );

    if (!this.validateEmail(emailField, validationMessage)) {
      return;
    }

    // Show loading state
    btnText.style.display = "none";
    btnLoading.style.display = "inline-block";
    submitBtn.disabled = true;

    try {
      const response = await fetch(
        "http://localhost:5062/api/auth/forgot-password",
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify({ email: email }),
        }
      );

      const jsonResponse = await response.json();

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
        this.clearValidation(emailField, validationMessage);
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

      // Update message area
      messageArea.innerHTML = messageHtml;
      messageArea.style.display = "block";
    } catch (error) {
      // Handle network/JavaScript errors
      messageArea.innerHTML = `
                <div class="alert alert-danger">
                    <h5><i class="fas fa-exclamation-triangle"></i> Network Error</h5>
                    <p>An error occurred while connecting to the server. Please try again.</p>
                </div>`;
      messageArea.style.display = "block";
    } finally {
      // Reset button state
      btnText.style.display = "inline-block";
      btnLoading.style.display = "none";
      submitBtn.disabled = false;
    }
  }
}

// Initialize when DOM is loaded
document.addEventListener("DOMContentLoaded", () => {
  new ForgotPasswordManager();
});




