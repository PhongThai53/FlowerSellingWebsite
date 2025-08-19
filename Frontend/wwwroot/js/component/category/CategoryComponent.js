

// components/CategoryComponent.js
class CategoryComponent {
    constructor(container, apiService, onCategoryChange) {
        this.container = container;
        this.apiService = apiService;
        this.onCategoryChange = onCategoryChange;
        this.categories = [];
        this.currentCategory = '';
        this.init();
    }

    async init() {
        if (!this.container) {
            console.error('Container not found for CategoryComponent');
            return;
        }
        try {
            await this.loadCategories();
            this.setupEventListeners();
        } catch (error) {
            console.error('Error initializing CategoryComponent:', error);
            this.showError('Không thể khởi tạo danh mục');
        }
    }

    async loadCategories() {
        try {
            // Giả sử apiService.fetchCategories() trả về response với structure như JSON bạn cung cấp
            const response = await this.apiService.fetchCategories();

            // Nếu API trả về object với data property
            if (response && response.data) {
                this.categories = response.data;
            }
            // Nếu API trả về trực tiếp array
            else if (Array.isArray(response)) {
                this.categories = response;
            } else {
                throw new Error('Invalid API response format');
            }

            this.render();
        } catch (error) {
            console.error('Error loading categories:', error);
            this.showError('Không thể tải danh mục sản phẩm');
        }
    }

    render() {
        if (!this.container || !this.categories) return;

        let html = `<li>
            <a href="#" data-category-id="" class="category-link ${this.currentCategory === '' ? 'active' : ''}">
                Tất cả <span>-</span>
            </a>
        </li>`;

        this.categories.forEach(category => {
            // Sử dụng products.length thay vì productCount
            const productCount = category.products ? category.products.length : 0;
            const isActive = this.currentCategory === category.id.toString();

            html += `<li>
                <a href="#" data-category-id="${category.id}" class="category-link ${isActive ? 'active' : ''}">
                    ${category.name} <span>${productCount}</span>
                </a>
            </li>`;
        });

        this.container.innerHTML = html;
    }

    setupEventListeners() {
        if (!this.container) return;

        this.container.addEventListener('click', (e) => {
            e.preventDefault();
            const categoryLink = e.target.closest('.category-link');
            if (!categoryLink) return;

            const categoryId = categoryLink.getAttribute('data-category-id');
            this.selectCategory(categoryId);
        });
    }

    selectCategory(categoryId) {
        // Update active state
        this.container.querySelectorAll('.category-link').forEach(link => {
            link.classList.remove('active');
        });

        const selectedLink = this.container.querySelector(`[data-category-id="${categoryId}"]`);
        if (selectedLink) {
            selectedLink.classList.add('active');
        }

        this.currentCategory = categoryId || '';

        // Trigger callback
        if (this.onCategoryChange) {
            this.onCategoryChange(this.currentCategory);
        }
    }

    getCurrentCategory() {
        return this.currentCategory;
    }

    showError(message) {
        if (this.container) {
            this.container.innerHTML = `<li><p class="text-danger">${message}</p></li>`;
        }
    }

    // Thêm method để refresh data
    async refresh() {
        await this.loadCategories();
    }

    // Thêm method để set category từ bên ngoài
    setCategoryById(categoryId) {
        this.selectCategory(categoryId);
    }
}

// API Service example
class ApiService {
    constructor(baseUrl) {
        this.baseUrl = baseUrl;
    }

    async fetchCategories() {
        try {
            const response = await fetch(`${this.baseUrl}/ProductCategory`);
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return await response.json();
        } catch (error) {
            console.error('API Error:', error);
            throw error;
        }
    }
}

// Cách sử dụng:
document.addEventListener('DOMContentLoaded', function () {
    // Khởi tạo API Service
    const apiService = new ApiService('https://localhost:7062/api');

    // Lấy container element
    const categoryContainer = document.querySelector('.shop-categories');

    if (categoryContainer) {
        // Khởi tạo CategoryComponent
        const categoryComponent = new CategoryComponent(
            categoryContainer,
            apiService,
            (selectedCategoryId) => {
                console.log('Category changed:', selectedCategoryId);
                // Xử lý khi category thay đổi (load products theo category)
                // Ví dụ: loadProductsByCategory(selectedCategoryId);
            }
        );

        // Có thể lưu reference để sử dụng sau
        window.categoryComponent = categoryComponent;
    } else {
        console.error('Category container not found!');
    }
});