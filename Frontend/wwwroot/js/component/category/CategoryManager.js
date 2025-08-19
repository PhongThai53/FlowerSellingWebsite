import { ApiService } from '../../services/ApiService.js';

export class CategoryManager {
    constructor(container, onCategoryChange) {
        this.container = container;
        this.onCategoryChange = onCategoryChange;
        this.categories = [];
        this.currentCategory = '';
        this.totalProducts = 0;
    }

    async loadCategories() {
        try {
            const result = await ApiService.fetchCategories();
            if (result.succeeded && result.data) {
                this.categories = result.data;
                this.render();
            }
        } catch (error) {
            console.error('Error loading categories:', error);
            throw error;
        }
    }

    async render() {
        if (!this.container) return;
        const html = this.generateCategoryHTML();
        this.container.innerHTML = html;
        this.setupEventListeners();
    }

    generateCategoryHTML() {
        let html = `<li><a href="#" data-category-id="" class="category-link active">Tất cả <span>-</span></a></li>`;

        this.categories.forEach((category) => {
            const productCount = category.totalProducts || 0;
            html += `
                <li>
                    <a href="#" data-category-id="${category.id}" class="category-link">
                        ${category.name} <span>${productCount}</span>
                    </a>
                </li>`;
        });

        return html;
    }

    setupEventListeners() {
        this.container.addEventListener('click', (e) => {
            e.preventDefault();
            const categoryLink = e.target.closest('.category-link');
            if (!categoryLink) return;

            const category = categoryLink.getAttribute('data-category-id');
            this.setActiveCategory(categoryLink);
            this.currentCategory = category || '';
            this.onCategoryChange(this.currentCategory);
        });
    }

    setActiveCategory(activeLink) {
        this.container.querySelectorAll('.category-link').forEach(link => {
            link.classList.remove('active');
        });
        activeLink.classList.add('active');
    }

    getCurrentCategory() {
        return this.currentCategory;
    }

    resetToAllCategories() {
        const allCategoriesLink = this.container.querySelector('a[data-category-id=""]');
        if (allCategoriesLink) {
            this.setActiveCategory(allCategoriesLink);
            this.currentCategory = '';
        }
    }
}
