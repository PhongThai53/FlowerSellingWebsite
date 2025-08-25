// API Base URL - thay đổi theo URL API của bạn
const API_BASE_URL = 'https://localhost:7062/api';

// Biến global để lưu trữ dữ liệu
let productsData = [];
let currentPage = 1;
let totalPages = 1;
let isListView = false;
let pageSize = 6;

// Helper functions
function getProductImageUrl(product) {
    if (!product.id) return 'assets/img/product/default-product.jpg';
    const baseUrl = API_BASE_URL.replace('/api', '');
    const imageUrl = `/images/products/${product.id}/primary.jpg`;
    return `${baseUrl}${imageUrl}`;
}

function formatPrice(price) {
    if (!price) return 'Liên hệ';
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(price);
}

class ItemsPage {
    constructor() {
        this.currentPage = 1;
        this.currentCategory = '';
        this.itemsPerPage = 6;
        this.totalPages = 0;
        this.totalProducts = 0;
        this.items = [];
        this.categories = [];
        this.sortBy = "default";
        this.isLoading = false;

        // DOM Elements
        this.productContainer = null;
        this.paginationContainer = null;
        this.productAmountElement = null;
        this.sortSelect = null;
        this.categoriesContainer = null;

        this.init();
    }

    async init() {
        console.log('Initializing ItemsPage...');

        // Đợi DOM ready
        if (document.readyState !== 'complete') {
            await new Promise(resolve => {
                window.addEventListener('load', resolve);
            });
        }

        // Initialize DOM elements
        this.initDOMElements();

        // Load data và setup events
        try {
            await this.loadCategories();
            await this.loadItems();
            this.setupEventListeners();
            console.log('ItemsPage initialized successfully');
        } catch (error) {
            console.error('Error initializing ItemsPage:', error);
        }
    }

    initDOMElements() {
        // Tìm các elements với nhiều selectors khác nhau
        this.productContainer = document.querySelector('.shop-product-wrap') ||
            document.querySelector('.shop-product-wrapper .row');

        this.paginationContainer = document.querySelector('.pagination-box') ||
            document.querySelector('.paginatoin-area ul') ||
            document.querySelector('.pagination');

        this.productAmountElement = document.querySelector('.product-amount p') ||
            document.querySelector('.product-amount');

        this.sortSelect = document.querySelector('select[name="sortby"]') ||
            document.querySelector('.product-short select');

        this.categoriesContainer = document.querySelector('.shop-categories') ||
            document.querySelector('.sidebar-body ul');

        // Debug log
        console.log('DOM Elements initialized:');
        console.log('- Product container:', !!this.productContainer);
        console.log('- Pagination container:', !!this.paginationContainer);
        console.log('- Categories container:', !!this.categoriesContainer);
        console.log('- Sort select:', !!this.sortSelect);
        console.log('- Product amount element:', !!this.productAmountElement);

        // Tạo pagination container nếu không tồn tại
        if (!this.paginationContainer) {
            this.createPaginationContainer();
        }
    }

    createPaginationContainer() {
        const paginationArea = document.querySelector('.paginatoin-area');
        if (paginationArea) {
            const ul = paginationArea.querySelector('ul');
            if (ul) {
                this.paginationContainer = ul;
            } else {
                // Tạo ul mới
                const newUl = document.createElement('ul');
                newUl.className = 'pagination-box';
                paginationArea.appendChild(newUl);
                this.paginationContainer = newUl;
            }
        }
        console.log('Pagination container created/found:', !!this.paginationContainer);
    }

    async loadCategories() {
        try {
            console.log('Loading categories...');
            const response = await fetch(`${API_BASE_URL}/ProductCategory/with-products`);

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result = await response.json();

            if (result.succeeded && result.data) {
                this.categories = result.data;
                this.renderCategories(result.data);
                console.log('Categories loaded successfully:', this.categories.length);
            } else {
                console.error('API Error:', result.message);
            }
        } catch (error) {
            console.error('Error loading categories:', error);
        }
    }



    renderCategories(categories) {
        if (!this.categoriesContainer) {
            console.warn('Categories container not found');
            return;
        }

        let html = '';
        html += `<li><a href="#" data-category-id="" class="category-link">Tất cả <span>-</span></a></li>`;

        categories.forEach(category => {
            const productCount = category.totalProducts || 0;
            html += `<li>
                        <a href="#" data-category-id="${category.id}" class="category-link">
                            ${category.name} <span>${productCount}</span>
                        </a>
                     </li>`;
        });

        this.categoriesContainer.innerHTML = html;
    }

    async loadItems() {
        if (this.isLoading) return;

        try {
            this.isLoading = true;
            this.showLoading(true);

            console.log(`Loading items - Page: ${this.currentPage}, Category: ${this.currentCategory}`);

            // Build API URL với parameters
            let apiUrl = `${API_BASE_URL}/Product?pageNumber=${this.currentPage}&pageSize=${this.itemsPerPage}`;

            if (this.currentCategory) {
                apiUrl += `&categoryId=${this.currentCategory}`;
            }

            if (this.sortBy && this.sortBy !== 'default') {
                apiUrl += `&sortBy=${this.sortBy}`;
            }

            console.log('API URL:', apiUrl);

            const response = await fetch(apiUrl);

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result = await response.json();
            console.log('API Response:', result);

            if (result.succeeded) {
                this.items = result.data?.items || result.data || [];
                this.totalPages = result.data?.totalPages || Math.ceil((result.data?.totalCount || 0) / this.itemsPerPage);
                this.totalProducts = result.data?.totalCount || this.items.length;

                // Update global variables
                productsData = this.items;
                totalPages = this.totalPages;
                currentPage = this.currentPage;

                console.log(`Loaded ${this.items.length} items, Total pages: ${this.totalPages}`);

                this.renderItems();
                this.renderPagination();
                this.updateProductCount();
            } else {
                throw new Error(result.message || 'Failed to load products');
            }
        } catch (error) {
            console.error('Error loading items:', error);
            this.showError('Không thể tải sản phẩm. Vui lòng thử lại sau.');
        } finally {
            this.isLoading = false;
            this.showLoading(false);
        }
    }

    renderItems() {
        if (!this.productContainer) {
            console.warn('Product container not found');
            return;
        }

        if (!this.items || this.items.length === 0) {
            this.productContainer.innerHTML = '<div class="col-12"><p class="text-center">Không có sản phẩm nào.</p></div>';
            return;
        }

        let html = '';

        this.items.forEach(item => {
            const imageUrl = getProductImageUrl(item);
            const name = item.name || 'Sản phẩm';
            const price = item.price || 0;
            const originalPrice = item.originalPrice || item.oldPrice;
            const description = item.description || '';
            const id = item.id;
            const isNew = item.isNew || false;
            const discountPercent = item.discountPercent || 0;

            html += `
  <div class="col-md-4 col-sm-6 mb-4" style="display:flex">
    <!-- product grid start -->
    <div class="product-item" style="flex:1; display:flex; flex-direction:column; height:100%; padding:10px; border-radius:8px; text-align:center;">

      <figure class="product-thumb" style="margin:0; flex-shrink:0;">
        <a href="product-details.html?id=${id}">
          <img class="pri-img" src="${imageUrl}" alt="${name}" style="width:100%; height:250px; object-fit:cover; border-radius:6px;">
          <img class="sec-img" src="${imageUrl}" alt="${name}" style="width:100%; height:250px; object-fit:cover; border-radius:6px;">
        </a>
        <div class="product-badge" style="position:absolute; top:10px; left:10px;">
          ${isNew ? '<div class="product-label new" style="background:#28a745;color:#fff;padding:2px 6px;border-radius:4px;font-size:12px;"><span>mới</span></div>' : ''}
          ${discountPercent > 0 ? `<div class="product-label discount" style="background:#dc3545;color:#fff;padding:2px 6px;border-radius:4px;font-size:12px;margin-top:5px;"><span>-${discountPercent}%</span></div>` : ''}
        </div>
        <div class="button-group" style="margin-top:8px; display:flex; justify-content:center; gap:8px;">
          <a href="wishlist.html" title="Thêm vào yêu thích"><i class="lnr lnr-heart"></i></a>
          <a href="#" data-bs-toggle="modal" data-bs-target="#quick_view" data-product-id="${id}" title="Xem nhanh"><i class="lnr lnr-magnifier"></i></a>
          <a href="#" class="add-to-cart-btn" data-product-id="${id}" title="Thêm vào giỏ hàng"><i class="lnr lnr-cart"></i></a>
        </div>
      </figure>

      <div class="product-caption" style="margin-top:auto;">
        <p class="product-name" style="margin:0; font-weight:600;">
          <a href="product-details.html?id=${id}" style="text-decoration:none; color:#333;">${name}</a>
        </p>
        <div class="price-box" style="margin-top:5px;">
          <span class="price-regular" style="color:#28a745; font-weight:700;">${formatPrice(price)}</span>
          ${originalPrice && originalPrice > price ? `<span class="price-old" style="color:#999; margin-left:5px;"><del>${formatPrice(originalPrice)}</del></span>` : ''}
        </div>
      </div>
    </div>
    <!-- product grid end -->
  </div>
`;
        });

        this.productContainer.innerHTML = html;
        console.log('Items rendered successfully');
    }

    renderPagination() {
        if (!this.paginationContainer) {
            console.warn('Pagination container not found');
            return;
        }

        console.log(`Rendering pagination - Current: ${this.currentPage}, Total: ${this.totalPages}`);

        if (this.totalPages <= 1) {
            this.paginationContainer.innerHTML = '';
            return;
        }

        let html = '';

        // Previous button
        if (this.currentPage > 1) {
            html += `<li><a class="previous pagination-btn" href="#" data-page="${this.currentPage - 1}"><i class="lnr lnr-chevron-left"></i></a></li>`;
        }

        // Page numbers
        const startPage = Math.max(1, this.currentPage - 2);
        const endPage = Math.min(this.totalPages, this.currentPage + 2);

        // Add first page if not in range
        if (startPage > 1) {
            html += `<li><a class="pagination-btn" href="#" data-page="1">1</a></li>`;
            if (startPage > 2) {
                html += `<li><span>...</span></li>`;
            }
        }

        // Add page numbers
        for (let i = startPage; i <= endPage; i++) {
            const isActive = i === this.currentPage;
            html += `<li class="${isActive ? 'active' : ''}"><a class="pagination-btn" href="#" data-page="${i}">${i}</a></li>`;
        }

        // Add last page if not in range
        if (endPage < this.totalPages) {
            if (endPage < this.totalPages - 1) {
                html += `<li><span>...</span></li>`;
            }
            html += `<li><a class="pagination-btn" href="#" data-page="${this.totalPages}">${this.totalPages}</a></li>`;
        }

        // Next button
        if (this.currentPage < this.totalPages) {
            html += `<li><a class="next pagination-btn" href="#" data-page="${this.currentPage + 1}"><i class="lnr lnr-chevron-right"></i></a></li>`;
        }

        this.paginationContainer.innerHTML = html;
        console.log('Pagination rendered');
    }

    setupEventListeners() {
        console.log('Setting up event listeners...');

        // Category filter
        if (this.categoriesContainer) {
            this.categoriesContainer.addEventListener('click', async (e) => {
                e.preventDefault();

                const categoryLink = e.target.closest('.category-link');
                if (!categoryLink) return;

                const categoryId = categoryLink.getAttribute('data-category-id');

                // Remove active class from all categories
                this.categoriesContainer.querySelectorAll('.category-link').forEach(link => {
                    link.classList.remove('active');
                });

                // Add active class to clicked category
                categoryLink.classList.add('active');

                // Update current category and reset page
                this.currentCategory = categoryId || '';
                this.currentPage = 1;

                console.log('Category changed:', this.currentCategory);
                await this.loadItems();
            });
        }

        // Global event delegation for dynamic content
        document.addEventListener('click', async (e) => {
            // Handle pagination clicks
            if (e.target.closest('.pagination-btn')) {
                e.preventDefault();

                const paginationBtn = e.target.closest('.pagination-btn');
                const newPage = parseInt(paginationBtn.getAttribute('data-page'));

                console.log('Pagination clicked:', newPage);

                if (newPage && newPage !== this.currentPage && newPage >= 1 && newPage <= this.totalPages) {
                    this.currentPage = newPage;
                    await this.loadItems();

                    // Scroll to top
                    window.scrollTo({ top: 0, behavior: 'smooth' });
                }
            }

            // Handle add to cart clicks
            if (e.target.closest('.add-to-cart-btn')) {
                e.preventDefault();
                const productId = e.target.closest('.add-to-cart-btn').getAttribute('data-product-id');
                this.addToCart(productId);
            }
        });

        // Sort dropdown
        if (this.sortSelect) {
            this.sortSelect.addEventListener('change', async (e) => {
                this.sortBy = e.target.value;
                this.currentPage = 1; // Reset to first page
                console.log('Sort changed:', this.sortBy);
                await this.loadItems();
            });
        }

        // View mode toggle (Grid/List)
        const viewModeButtons = document.querySelectorAll('.product-view-mode a');
        viewModeButtons.forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();

                // Remove active from all buttons
                viewModeButtons.forEach(b => b.classList.remove('active'));

                // Add active to clicked button
                btn.classList.add('active');

                const target = btn.getAttribute('data-target');
                this.toggleViewMode(target);
            });
        });

        console.log('Event listeners set up successfully');
    }

    toggleViewMode(mode) {
        const productItems = document.querySelectorAll('.product-item');
        const productListItems = document.querySelectorAll('.product-list-item');

        if (mode === 'list-view') {
            productItems.forEach(item => item.style.display = 'none');
            productListItems.forEach(item => item.style.display = 'flex');
            isListView = true;
        } else {
            productItems.forEach(item => item.style.display = 'block');
            productListItems.forEach(item => item.style.display = 'none');
            isListView = false;
        }
    }

    addToCart(productId) {
        console.log('Adding product to cart:', productId);

        // TODO: Implement actual add to cart functionality
        // For now, just show an alert
        const product = this.items.find(item => item.id == productId);
        const productName = product ? product.name : 'Sản phẩm';

        alert(`${productName} đã được thêm vào giỏ hàng!`);

        // You can implement actual cart logic here
        // Example: update cart counter, send to API, etc.
    }

    updateProductCount() {
        if (this.productAmountElement && this.items) {
            const startIndex = (this.currentPage - 1) * this.itemsPerPage + 1;
            const endIndex = Math.min(this.currentPage * this.itemsPerPage, this.totalProducts);

            this.productAmountElement.textContent = `Showing ${startIndex}–${endIndex} of ${this.totalProducts} results`;
            console.log('Product count updated');
        }
    }

    showLoading(show) {
        if (this.productContainer) {
            if (show) {
                this.productContainer.innerHTML = '<div class="col-12"><div class="text-center p-5"><i class="fa fa-spinner fa-spin fa-2x"></i><p class="mt-2">Đang tải sản phẩm...</p></div></div>';
            }
        }
    }

    showError(message) {
        if (this.productContainer) {
            this.productContainer.innerHTML = `<div class="col-12"><div class="text-center p-5 text-danger"><i class="fa fa-exclamation-triangle fa-2x"></i><p class="mt-2">${message}</p></div></div>`;
        }
    }
}

// Initialize app khi DOM ready
document.addEventListener('DOMContentLoaded', () => {
    console.log('DOM Content Loaded');

    // Đợi một chút để đảm bảo tất cả elements đã render
    setTimeout(() => {
        console.log('Initializing ItemsPage...');
        window.itemsPageInstance = new ItemsPage();
    }, 100);
});

// Backup initialization với window.onload
window.addEventListener('load', () => {
    console.log('Window loaded');

    // Chỉ khởi tạo nếu chưa có instance
    if (!window.itemsPageInstance) {
        console.log('Creating backup ItemsPage instance...');
        window.itemsPageInstance = new ItemsPage();
    }
});