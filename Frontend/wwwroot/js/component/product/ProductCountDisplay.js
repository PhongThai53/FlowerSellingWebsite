export class ProductCountDisplay {
    constructor(element) {
        this.element = element;
    }

    update(currentPage, itemsPerPage, totalProducts) {
        if (!this.element) return;

        const startIndex = (currentPage - 1) * itemsPerPage + 1;
        const endIndex = Math.min(currentPage * itemsPerPage, totalProducts);

        const text = totalProducts > 0
            ? `Showing ${startIndex}–${endIndex} of ${totalProducts} results`
            : 'No results found';

        this.element.textContent = text;
    }

    hide() {
        if (this.element) {
            this.element.style.display = 'none';
        }
    }

    show() {
        if (this.element) {
            this.element.style.display = 'block';
        }
    }
}
