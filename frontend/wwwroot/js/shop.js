// API Base URL
const API_BASE_URL = 'https://localhost:7062/api';

// Global variables
let productsData = [];
let currentPage = 1;
let totalPages = 1;
let isListView = false;
let pageSize = 6;

// Helper functions
function getProductImageUrl(product) {
    const baseUrl = API_BASE_URL.replace('/api', ''); // Định nghĩa trước
    if (!product || !product.id) {
        return `${baseUrl}/Image/products/default/default.jpg`;
    }
    return `${baseUrl}/Image/products/${product.id}/primary.jpg`;
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
        this.sortBy = 'default';
        this.isLoading = false;
        this.lastSortValue = 'default';

        this.init();
    }

    async init() {
        if (document.readyState !== 'complete') {
            await new Promise(resolve => window.addEventListener('load', resolve));
        }

        this.initDOMElements();

        try {
            await this.loadCategories();
            await this.loadItems();
            this.setupEventListeners();
            this.startSortMonitoring();
        } catch (error) {
            console.error('Error initializing ItemsPage:', error);
        }
    }

    initDOMElements() {
        this.productContainer = document.querySelector('.shop-product-wrap') ||
            document.querySelector('.shop-product-wrapper .row');

        this.paginationContainer = document.querySelector('.pagination-box') ||
            document.querySelector('.paginatoin-area ul') ||
            document.querySelector('.pagination');

        this.productAmountElement = document.querySelector('.product-amount p') ||
            document.querySelector('.product-amount');

        this.sortSelect = document.querySelector('select[name="sortBy"]') ||
            document.querySelector('.product-short select');

        this.categoriesContainer = document.querySelector('.shop-categories') ||
            document.querySelector('.sidebar-body ul');

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
                const newUl = document.createElement('ul');
                newUl.className = 'pagination-box';
                paginationArea.appendChild(newUl);
                this.paginationContainer = newUl;
            }
        }
    }

    async loadCategories() {
        try {
            const response = await fetch(`${API_BASE_URL}/ProductCategory/with-products`);
            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);

            const result = await response.json();
            if (result.succeeded && result.data) {
                this.categories = result.data;
                this.renderCategories(result.data);
            }
        } catch (error) {
            console.error('Error loading categories:', error);
        }
    }

    renderCategories(categories) {
        if (!this.categoriesContainer) return;

        let html = `<li><a href="#" data-category-id="" class="category-link">All <span>-</span></a></li>`;

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

            let apiUrl = `${API_BASE_URL}/Product?pageNumber=${this.currentPage}&pageSize=${this.itemsPerPage}`;

            if (this.currentCategory) {
                apiUrl += `&categoryId=${this.currentCategory}`;
            }
            if (this.sortBy && this.sortBy !== 'default') {
                apiUrl += `&sortBy=${this.sortBy}`;
            }

            const response = await fetch(apiUrl);
            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);

            const result = await response.json();

            if (result.succeeded) {
                this.items = result.data?.items || result.data || [];
                this.totalPages = result.data?.totalPages || Math.ceil((result.data?.totalCount || 0) / this.itemsPerPage);
                this.totalProducts = result.data?.totalCount || this.items.length;

                productsData = this.items;
                totalPages = this.totalPages;
                currentPage = this.currentPage;

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
        if (!this.productContainer || !this.items || this.items.length === 0) {
            if (this.productContainer) {
                this.productContainer.innerHTML = '<div class="col-12"><p class="text-center">Không có sản phẩm nào.</p></div>';
            }
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
    <div class="col-md-4 col-sm-6">
        <div class="product-item">
            <figure class="product-thumb">
                <a href="product-details.html?id=${id}">
                    <img style="width:100%;height:250px;object-fit:cover;" 
                         class="pri-img" src="${imageUrl}" alt="${name}" 
                         onerror="this.src='assets/img/product/default-product.jpg'">
                    <img style="width:100%;height:250px;object-fit:cover;" 
                         class="sec-img" src="${imageUrl}" alt="${name}" 
                         onerror="this.src='assets/img/product/default-product.jpg'">
                </a>
                <div class="product-badge">
                    ${isNew ? '<div class="product-label new"><span>mới</span></div>' : ''}
                    ${discountPercent > 0 ? `<div class="product-label discount"><span>-${discountPercent}%</span></div>` : ''}
                </div>
                <div class="button-group">
                    <a href="wishlist.html" data-bs-toggle="tooltip" data-bs-placement="left" title="Thêm vào yêu thích"><i class="lnr lnr-heart"></i></a>
                    <a href="#" data-bs-toggle="modal" data-bs-target="#quick_view" data-product-id="${id}"><span data-bs-toggle="tooltip" data-bs-placement="left" title="Xem nhanh"><i class="lnr lnr-magnifier"></i></span></a>
                    <a href="#" class="add-to-cart-btn" data-product-id="${id}" data-bs-toggle="tooltip" data-bs-placement="left" title="Thêm vào giỏ hàng"><i class="lnr lnr-cart"></i></a>
                </div>
            </figure>
            <div class="product-caption">
                <p class="product-name">
                    <a href="product-details.html?id=${id}">${name}</a>
                </p>
                <div class="price-box">
                    <span class="price-regular">${formatPrice(price)}</span>
                    ${originalPrice && originalPrice > price ? `<span class="price-old"><del>${formatPrice(originalPrice)}</del></span>` : ''}
                </div>
            </div>
        </div>

        <div class="product-list-item" style="display: none;">
            <figure class="product-thumb">
                <a href="product-details.html?id=${id}">
                    <img style="width:120px;height:120px;object-fit:cover;" 
                         class="pri-img" src="${imageUrl}" alt="${name}" 
                         onerror="this.src='assets/img/product/default-product.jpg'">
                    <img style="width:120px;height:120px;object-fit:cover;" 
                         class="sec-img" src="${imageUrl}" alt="${name}" 
                         onerror="this.src='assets/img/product/default-product.jpg'">
                </a>
                <div class="product-badge">
                    ${isNew ? '<div class="product-label new"><span>mới</span></div>' : ''}
                    ${discountPercent > 0 ? `<div class="product-label discount"><span>-${discountPercent}%</span></div>` : ''}
                </div>
            </figure>
            <div class="product-content-list">
                <h5 class="product-name"><a href="product-details.html?id=${id}">${name}</a></h5>
                <div class="price-box">
                    <span class="price-regular">${formatPrice(price)}</span>
                    ${originalPrice && originalPrice > price ? `<span class="price-old"><del>${formatPrice(originalPrice)}</del></span>` : ''}
                </div>
                <p>${description || 'Sản phẩm chất lượng cao, được chọn lọc kỹ càng.'}</p>
                <div class="button-group-list">
                    <a class="btn-big add-to-cart-btn" href="#" data-product-id="${id}" data-bs-toggle="tooltip" title="Thêm vào giỏ hàng"><i class="lnr lnr-cart"></i>Thêm vào giỏ</a>
                    <a href="#" data-bs-toggle="modal" data-bs-target="#quick_view" data-product-id="${id}"><span data-bs-toggle="tooltip" title="Xem nhanh"><i class="lnr lnr-magnifier"></i></span></a>
                </div>
            </div>
        </div>
    </div>
            `;
        });

        this.productContainer.innerHTML = html;
    }

    renderPagination() {
        if (!this.paginationContainer || this.totalPages <= 1) {
            if (this.paginationContainer) this.paginationContainer.innerHTML = '';
            return;
        }

        let html = '';

        if (this.currentPage > 1) {
            html += `<li><a class="previous pagination-btn" href="#" data-page="${this.currentPage - 1}"><i class="lnr lnr-chevron-left"></i></a></li>`;
        }

        const startPage = Math.max(1, this.currentPage - 2);
        const endPage = Math.min(this.totalPages, this.currentPage + 2);

        if (startPage > 1) {
            html += `<li><a class="pagination-btn" href="#" data-page="1">1</a></li>`;
            if (startPage > 2) html += `<li><span>...</span></li>`;
        }

        for (let i = startPage; i <= endPage; i++) {
            const isActive = i === this.currentPage;
            html += `<li class="${isActive ? 'active' : ''}"><a class="pagination-btn" href="#" data-page="${i}">${i}</a></li>`;
        }

        if (endPage < this.totalPages) {
            if (endPage < this.totalPages - 1) html += `<li><span>...</span></li>`;
            html += `<li><a class="pagination-btn" href="#" data-page="${this.totalPages}">${this.totalPages}</a></li>`;
        }

        if (this.currentPage < this.totalPages) {
            html += `<li><a class="next pagination-btn" href="#" data-page="${this.currentPage + 1}"><i class="lnr lnr-chevron-right"></i></a></li>`;
        }

        this.paginationContainer.innerHTML = html;
    }

    setupEventListeners() {
        if (this.categoriesContainer) {
            this.categoriesContainer.addEventListener('click', async (e) => {
                e.preventDefault();
                const categoryLink = e.target.closest('.category-link');
                if (!categoryLink) return;

                const categoryId = categoryLink.getAttribute('data-category-id');

                this.categoriesContainer.querySelectorAll('.category-link').forEach(link => {
                    link.classList.remove('active');
                });
                categoryLink.classList.add('active');

                this.currentCategory = categoryId || '';
                this.currentPage = 1;
                await this.loadItems();
            });
        }

        document.addEventListener('click', async (e) => {
            if (e.target.closest('.pagination-btn')) {
                e.preventDefault();
                const paginationBtn = e.target.closest('.pagination-btn');
                const newPage = parseInt(paginationBtn.getAttribute('data-page'));

                if (newPage && newPage !== this.currentPage && newPage >= 1 && newPage <= this.totalPages) {
                    this.currentPage = newPage;
                    await this.loadItems();
                    window.scrollTo({ top: 0, behavior: 'smooth' });
                }
            }

            if (e.target.closest('.add-to-cart-btn')) {
                e.preventDefault();
                const productId = e.target.closest('.add-to-cart-btn').getAttribute('data-product-id');
                this.addToCart(productId);
            }
        });

        const viewModeButtons = document.querySelectorAll('.product-view-mode a');
        viewModeButtons.forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                viewModeButtons.forEach(b => b.classList.remove('active'));
                btn.classList.add('active');
                const target = btn.getAttribute('data-target');
                this.toggleViewMode(target);
            });
        });
    }

    startSortMonitoring() {
        setTimeout(() => {
            const sortSelect = document.querySelector('select[name="sortBy"]');
            if (sortSelect) {
                sortSelect.onchange = () => this.handleSortChange(sortSelect.value);
            }
        }, 1000);

        setInterval(() => {
            const sortSelect = document.querySelector('select[name="sortBy"]');
            if (sortSelect && sortSelect.value !== this.lastSortValue) {
                this.lastSortValue = sortSelect.value;
                this.handleSortChange(sortSelect.value);
            }
        }, 300);

        const sortSelect = document.querySelector('select[name="sortBy"]');
        if (sortSelect) {
            const observer = new MutationObserver(() => {
                if (sortSelect.value !== this.lastSortValue) {
                    this.lastSortValue = sortSelect.value;
                    this.handleSortChange(sortSelect.value);
                }
            });

            observer.observe(sortSelect, {
                attributes: true,
                attributeFilter: ['value'],
                childList: true,
                subtree: true
            });
        }
    }

    async handleSortChange(newValue) {
        if (newValue !== this.sortBy) {
            this.sortBy = newValue;
            this.currentPage = 1;
            await this.loadItems();
        }
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
        const product = this.items.find(item => item.id == productId);
        const productName = product ? product.name : 'Sản phẩm';
        alert(`${productName} đã được thêm vào giỏ hàng!`);
    }

    updateProductCount() {
        if (this.productAmountElement && this.items) {
            const startIndex = (this.currentPage - 1) * this.itemsPerPage + 1;
            const endIndex = Math.min(this.currentPage * this.itemsPerPage, this.totalProducts);
            this.productAmountElement.textContent = `Showing ${startIndex}–${endIndex} of ${this.totalProducts} results`;
        }
    }

    showLoading(show) {
        if (this.productContainer && show) {
            this.productContainer.innerHTML = '<div class="col-12"><div class="text-center p-5"><i class="fa fa-spinner fa-spin fa-2x"></i><p class="mt-2">Đang tải sản phẩm...</p></div></div>';
        }
    }

    showError(message) {
        if (this.productContainer) {
            this.productContainer.innerHTML = `<div class="col-12"><div class="text-center p-5 text-danger"><i class="fa fa-exclamation-triangle fa-2x"></i><p class="mt-2">${message}</p></div></div>`;
        }
    }

    forceSort(value) {
        this.sortBy = value;
        this.lastSortValue = value;
        this.currentPage = 1;
        this.loadItems();
    }
}

// Initialize
document.addEventListener('DOMContentLoaded', () => {
    setTimeout(() => {
        window.itemsPageInstance = new ItemsPage();
        window.forceSort = (value) => window.itemsPageInstance.forceSort(value);
    }, 100);
});

window.addEventListener('load', () => {
    if (!window.itemsPageInstance) {
        window.itemsPageInstance = new ItemsPage();
    }
});

// jQuery backup for sort functionality
setTimeout(() => {
    if (window.itemsPageInstance) {
        $(document).on('change', 'select[name="sortBy"]', function () {
            window.itemsPageInstance.forceSort($(this).val());
        });

        $(document).on('click', '.nice-select .option', function () {
            setTimeout(() => {
                const sortSelect = document.querySelector('select[name="sortBy"]');
                if (sortSelect) {
                    window.itemsPageInstance.forceSort(sortSelect.value);
                }
            }, 50);
        });
    }
}, 3000);


