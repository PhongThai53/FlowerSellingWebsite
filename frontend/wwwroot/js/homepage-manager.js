class HomepageManager {
    constructor() {
        this.apiBaseUrl = "https://localhost:7062/api";
        this.currentUser = null;
        // The init() method is called by event listeners after the header is loaded, not here.
    }

    init() {
        // This method is now called externally after the header is in the DOM.
        this.initializeUI();
        this.bindEvents();
    }

    initializeUI() {
        this.updateUserInterface();
    }

    bindEvents() {
        // Handle navigation links for my-account
        document.body.addEventListener("click", (e) => {
            const myAccountLink = e.target.closest('a[href*="my-account"]');
            if (myAccountLink) {
                e.preventDefault();
                this.handleMyAccountClick();
            }
        });
    }

    updateUserInterface() {
        const token = localStorage.getItem("auth_token");
        const userData = JSON.parse(localStorage.getItem("user_data") || "null");
        this.updateUserDropdown(token, userData);
    }

    updateUserDropdown(token, userData) {
        const userDropdown = document.getElementById("user-dropdown");
        const adminUserManagement = document.getElementById(
            "admin-user-management"
        );

        if (!userDropdown) {
            console.error(
                "#user-dropdown element not found. Header might not be loaded yet."
            );
            return;
        }

        const isAdmin = userData && userData.roleName === "Admin";

        if (adminUserManagement) {
            adminUserManagement.style.display = isAdmin ? "block" : "none";
        }

        if (token && userData) {
            // User is logged in
            let userMenuItems = `
        <li><a href="/html/user/my-account.html"><i class="lnr lnr-user"></i> My Account</a></li>
      `;
            if (isAdmin) {
                userMenuItems += `
          <li><a href="/html/user/user-list.html"><i class="fa fa-users"></i> User Management</a></li>
        `;
            }
            userMenuItems += `
        <li><a href="#" onclick="window.homepageManager.logout()"><i class="lnr lnr-exit"></i> Logout</a></li>
      `;
            userDropdown.innerHTML = userMenuItems;
        } else {
            // User is not logged in
            userDropdown.innerHTML = `
        <li><a href="/html/auth/login-register.html">Login</a></li>
        <li><a href="/html/auth/login-register.html">Register</a></li>
      `;
        }
    }

    handleMyAccountClick() {
        if (!this.isAuthenticated()) {
            this.showAlert(
                "Authentication Required",
                "Please log in to access your account.",
                "warning"
            );
            setTimeout(() => {
                window.location.href = "/html/auth/login-register.html";
            }, 1500);
        } else {
            window.location.href = "/html/user/my-account.html";
        }
    }

    async logout() {
        try {
            const token = localStorage.getItem("auth_token");
            if (token) {
                await fetch(`${this.apiBaseUrl}/Auth/logout`, {
                    method: "POST",
                    headers: {
                        Authorization: `Bearer ${token}`,
                        "Content-Type": "application/json",
                    },
                });
            }
        } catch (error) {
            console.error("Error during server logout:", error);
        } finally {
            localStorage.removeItem("auth_token");
            localStorage.removeItem("user_data");
            this.showAlert("Success", "You have been logged out.", "success");
            this.updateUserInterface(); // Refresh UI after logout
            setTimeout(() => {
                // Optional: redirect to home or login page after a short delay
                if (window.location.pathname.includes("/user/")) {
                    window.location.href = "/html/common/homepage.html";
                }
            }, 1000);
        }
    }

    showAlert(title, message, type = "info") {
        const alertContainer = document.createElement("div");
        alertContainer.style.position = "fixed";
        alertContainer.style.top = "20px";
        alertContainer.style.right = "20px";
        alertContainer.style.zIndex = "10000";
        alertContainer.innerHTML = `
      <div class="alert alert-${type} alert-dismissible fade show" role="alert">
        <strong>${title}:</strong> ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
      </div>
    `;
        document.body.appendChild(alertContainer);
        setTimeout(() => {
            const alertNode = alertContainer.querySelector(".alert");
            if (alertNode) {
                const bsAlert = new bootstrap.Alert(alertNode);
                bsAlert.close();
            }
        }, 3000);
    }

    isAuthenticated() {
        return !!localStorage.getItem("auth_token");
    }
}

// --- SCRIPT INITIALIZATION ---

// Create a single, global instance of the manager immediately.
// The init() method will be called at the appropriate time by the event listeners below.
if (!window.homepageManager) {
    window.homepageManager = new HomepageManager();
}

// The primary initialization trigger for pages using HTMX to load the header.
document.body.addEventListener("htmx:afterSwap", function (event) {
    // Check if the swapped content includes the header placeholder.
    if (event.detail.target.id === "header-placeholder") {
        console.log("Header loaded via HTMX. Initializing HomepageManager...");
        window.homepageManager.init();
    }
});

// Fallback for pages where the header is static or for cached scenarios.
document.addEventListener("DOMContentLoaded", function () {
    // Check if the header is already in the DOM.
    if (document.getElementById("user-dropdown")) {
        console.log(
            "Header found on DOMContentLoaded. Initializing HomepageManager..."
        );
        window.homepageManager.init();
    }
});
