export class Pagination {
    constructor(container, onPageChange) {
        this.container = container;
        this.onPageChange = onPageChange;
        this.currentPage = 1;
        this.totalPages = 1;
        this.maxVisiblePages = 5;
    }

    render(currentPage, totalPages) {
        this.currentPage = currentPage;
        this.totalPages = totalPages;

        if (!this.container || totalPages <= 1) {
            if (this.container) this.container.innerHTML = '';
            return;
        }

        const html = this.generatePaginationHTML();
        this.container.innerHTML = html;
        this.setupEventListeners();
    }

    generatePaginationHTML() {
        let html = '';

        // Previous button
        if (this.currentPage > 1) {
            html += this.createPageButton(this.currentPage - 1, '<i class="lnr lnr-chevron-left"></i>', 'previous');
        }

        // Page numbers
        const { startPage, endPage } = this.calculatePageRange();

        if (startPage > 1) {
            html += this.createPageButton(1, '1');
            if (startPage > 2) html += '<li><span>...</span></li>';
        }

        for (let i = startPage; i <= endPage; i++) {
            const isActive = i === this.currentPage;
            html += this.createPageButton(i, i.toString(), isActive ? 'active' : '');
        }

        if (endPage < this.totalPages) {
            if (endPage < this.totalPages - 1) html += '<li><span>...</span></li>';
            html += this.createPageButton(this.totalPages, this.totalPages.toString());
        }

        // Next button
        if (this.currentPage < this.totalPages) {
            html += this.createPageButton(this.currentPage + 1, '<i class="lnr lnr-chevron-right"></i>', 'next');
        }

        return html;
    }

    createPageButton(page, content, cssClass = '') {
        return `<li class="${cssClass}"><a class="pagination-btn" href="#" data-page="${page}">${content}</a></li>`;
    }

    calculatePageRange() {
        const halfVisible = Math.floor(this.maxVisiblePages / 2);
        let startPage = Math.max(1, this.currentPage - halfVisible);
        let endPage = Math.min(this.totalPages, this.currentPage + halfVisible);

        // Adjust if we're near the start or end
        if (endPage - startPage + 1 < this.maxVisiblePages) {
            if (startPage === 1) {
                endPage = Math.min(this.totalPages, startPage + this.maxVisiblePages - 1);
            } else if (endPage === this.totalPages) {
                startPage = Math.max(1, endPage - this.maxVisiblePages + 1);
            }
        }

        return { startPage, endPage };
    }

    setupEventListeners() {
        this.container.addEventListener('click', (e) => {
            e.preventDefault();
            const paginationBtn = e.target.closest('.pagination-btn');
            if (paginationBtn) {
                const newPage = parseInt(paginationBtn.getAttribute('data-page'));
                if (this.isValidPage(newPage)) {
                    this.onPageChange(newPage);
                }
            }
        });
    }

    isValidPage(page) {
        return page && page !== this.currentPage && page >= 1 && page <= this.totalPages;
    }
}