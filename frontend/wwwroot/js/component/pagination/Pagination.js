/**
 * Pagination Component
 * Generates pagination HTML for paginated content
 */
export class Pagination {
  constructor(options = {}) {
    this.currentPage = options.currentPage || 1;
    this.totalPages = options.totalPages || 1;
    this.maxVisiblePages = options.maxVisiblePages || 5;
    this.onPageChange = options.onPageChange || function () {};
    this.showFirstLast = options.showFirstLast !== false; // Default true
    this.showPrevNext = options.showPrevNext !== false; // Default true
    this.className = options.className || "pagination-container";
  }

  /**
   * Render the pagination HTML
   * @returns {string} HTML string for pagination
   */
  render() {
    if (this.totalPages <= 1) {
      return ""; // No pagination needed
    }

    const pages = this.getVisiblePages();
    let html = `<div class="${this.className}">`;
    html += '<nav aria-label="Page navigation">';
    html += '<ul class="pagination justify-content-center">';

    // First page button
    if (this.showFirstLast && this.currentPage > 1) {
      html += this.createPageButton(1, "First", "lnr lnr-chevron-left-circle");
    }

    // Previous page button
    if (this.showPrevNext && this.currentPage > 1) {
      html += this.createPageButton(
        this.currentPage - 1,
        "Previous",
        "lnr lnr-chevron-left"
      );
    }

    // Page number buttons
    pages.forEach((page) => {
      if (page === "...") {
        html +=
          '<li class="page-item disabled"><span class="page-link">...</span></li>';
      } else {
        const isActive = page === this.currentPage;
        html += this.createPageButton(page, page.toString(), null, isActive);
      }
    });

    // Next page button
    if (this.showPrevNext && this.currentPage < this.totalPages) {
      html += this.createPageButton(
        this.currentPage + 1,
        "Next",
        "lnr lnr-chevron-right"
      );
    }

    // Last page button
    if (this.showFirstLast && this.currentPage < this.totalPages) {
      html += this.createPageButton(
        this.totalPages,
        "Last",
        "lnr lnr-chevron-right-circle"
      );
    }

    html += "</ul>";
    html += "</nav>";
    html += "</div>";

    return html;
  }

  /**
   * Create a page button
   * @param {number} page - Page number
   * @param {string} text - Button text
   * @param {string} icon - Icon class (optional)
   * @param {boolean} isActive - Whether this is the active page
   * @returns {string} HTML for the page button
   */
  createPageButton(page, text, icon = null, isActive = false) {
    const activeClass = isActive ? " active" : "";
    const disabledClass = isActive ? " disabled" : "";

    let buttonContent = "";
    if (icon) {
      buttonContent = `<i class="${icon}"></i>`;
      if (text !== page.toString()) {
        buttonContent += ` <span class="sr-only">${text}</span>`;
      }
    } else {
      buttonContent = text;
    }

    return `
      <li class="page-item${activeClass}${disabledClass}">
        <a class="page-link" href="#" data-page="${page}" ${
      isActive ? 'aria-current="page"' : ""
    }>
          ${buttonContent}
        </a>
      </li>
    `;
  }

  /**
   * Calculate which page numbers to show
   * @returns {Array} Array of page numbers and ellipsis
   */
  getVisiblePages() {
    const pages = [];
    const half = Math.floor(this.maxVisiblePages / 2);

    let start = Math.max(1, this.currentPage - half);
    let end = Math.min(this.totalPages, start + this.maxVisiblePages - 1);

    // Adjust start if we're near the end
    if (end - start + 1 < this.maxVisiblePages) {
      start = Math.max(1, end - this.maxVisiblePages + 1);
    }

    // Add ellipsis at the beginning if needed
    if (start > 1) {
      pages.push(1);
      if (start > 2) {
        pages.push("...");
      }
    }

    // Add visible page numbers
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }

    // Add ellipsis at the end if needed
    if (end < this.totalPages) {
      if (end < this.totalPages - 1) {
        pages.push("...");
      }
      pages.push(this.totalPages);
    }

    return pages;
  }

  /**
   * Update pagination settings
   * @param {Object} options - New options
   */
  update(options = {}) {
    this.currentPage = options.currentPage || this.currentPage;
    this.totalPages = options.totalPages || this.totalPages;
    this.maxVisiblePages = options.maxVisiblePages || this.maxVisiblePages;
    this.onPageChange = options.onPageChange || this.onPageChange;
  }

  /**
   * Set up event listeners for pagination clicks
   * @param {HTMLElement} container - Container element to attach listeners to
   */
  attachEventListeners(container) {
    if (!container) return;

    container.addEventListener("click", (e) => {
      const pageLink = e.target.closest(".page-link");
      if (!pageLink) return;

      e.preventDefault();

      const page = parseInt(pageLink.getAttribute("data-page"));
      if (
        page &&
        page !== this.currentPage &&
        page >= 1 &&
        page <= this.totalPages
      ) {
        this.currentPage = page;
        this.onPageChange(page);
      }
    });
  }

  /**
   * Get pagination info object
   * @returns {Object} Pagination information
   */
  getInfo() {
    return {
      currentPage: this.currentPage,
      totalPages: this.totalPages,
      hasNext: this.currentPage < this.totalPages,
      hasPrev: this.currentPage > 1,
      isFirst: this.currentPage === 1,
      isLast: this.currentPage === this.totalPages,
    };
  }
}

/**
 * Simple pagination utility function
 * @param {Object} options - Pagination options
 * @returns {Pagination} New Pagination instance
 */
export function createPagination(options) {
  return new Pagination(options);
}

/**
 * Calculate pagination data from API response
 * @param {Object} apiResponse - API response with pagination data
 * @returns {Object} Normalized pagination data
 */
export function normalizePaginationData(apiResponse) {
  return {
    currentPage: apiResponse.page || apiResponse.currentPage || 1,
    totalPages: apiResponse.total_pages || apiResponse.totalPages || 1,
    totalItems: apiResponse.total_items || apiResponse.totalItems || 0,
    pageSize: apiResponse.page_size || apiResponse.pageSize || 10,
    hasNext: apiResponse.has_next || apiResponse.hasNext || false,
    hasPrev: apiResponse.has_prev || apiResponse.hasPrev || false,
  };
}
