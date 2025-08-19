import { CONFIG, SELECTORS, MESSAGES } from '../../config/config.js';
import { ApiService } from '../../services/ApiService.js';
import { LoadingManager } from '../loading/LoadingManager.js';
import { Pagination } from '../pagination/Pagination.js';
import { ProductCountDisplay } from '../product/ProductCountDisplay.js';
import { CategoryManager } from '../category/CategoryManager.js';
import { ViewModeManager } from '../viewMode/ViewMode.js';
import { CartManager } from '../cart/CartManager.js';
import { SortManager } from '../sort/SortManager.js';
import { ProductRenderer } from '../product/ProductRender.js';
import { Utils } from '../../utils/Utils.js';

export class ProductPage {
    constructor(options = {}) {
        // State
        this.currentPage = 1;
        this.currentCategory = '';
        this.itemsPerPage = options.itemsPerPage || CONFIG.DEFAULT_PAGE_SIZE;
        this.totalPages = 0;
        this.totalProducts = 0;
        this.min = 0;
        this.max = 0;
        this.dbmax = CONFIG.DBMAX;
        this.items = [];
        this.sortBy = 'default';
        this.isLoading = false;

        // Options
        this.options = {
            enableViewMode: true,
            enableSort: true,
            enableCategories: true,
            enablePriceFilter: true,
            ...options
        };
    }

    async init() {
        await this.waitForDOMReady();
        this.initDOMElements();
        this.initComponents();
        await this.loadInitialData();
        this.setupGlobalEventListeners();
    }

    async waitForDOMReady() {
        if (document.readyState !== 'complete') {
            await new Promise(resolve => window.addEventListener('load', resolve));
        }
    }

    initDOMElements() {
        this.productContainer = document.querySelector(SELECTORS.PRODUCT_CONTAINER);
        this.paginationContainer = document.querySelector(SELECTORS.PAGINATION_CONTAINER);
        this.productAmountElement = document.querySelector(SELECTORS.PRODUCT_AMOUNT);
        this.categoriesContainer = document.querySelector(SELECTORS.CATEGORIES_CONTAINER);


        // Create pagination container if not exists
        if (!this.paginationContainer) {
            this.createPaginationContainer();
        }
    }

    createPaginationContainer() {
        const paginationArea = document.querySelector('.paginatoin-area');
        if (paginationArea) {
            const ul = paginationArea.querySelector('ul') || Utils.createElement('ul', { className: 'pagination-box' });
            if (!paginationArea.contains(ul)) {
                paginationArea.appendChild(ul);
            }
            this.paginationContainer = ul;
        }
    }

    initComponents() {
        // Core components
        this.loadingManager = new LoadingManager(this.productContainer);
        this.productCountDisplay = new ProductCountDisplay(this.productAmountElement);

        // Pagination
        this.pagination = new Pagination(
            this.paginationContainer,
            (page) => this.handlePageChange(page)
        );

        //Price Range

        if (this.options.enablePriceFilter) {
            const priceContainer = document.querySelector(CONFIG.PRICE_RANCE_SLIDER);
            if (priceContainer) {
                this.priceRangeManager = new PriceRangeManager(
                    priceContainer,
                    (priceRange) => this.handlePriceChange(priceRange)
                );
            }
        }

        // Optional components based on options
        if (this.options.enableCategories && this.categoriesContainer) {
            this.categoryManager = new CategoryManager(
                this.categoriesContainer,
                (categoryId) => this.handleCategoryChange(categoryId)
            );
        }

        if (this.options.enableViewMode) {
            this.viewModeManager = new ViewModeManager();
        }

        if (this.options.enableSort) {
            this.sortManager = new SortManager(
                (sortValue) => this.handleSortChange(sortValue)
            );
        }

        // Cart manager
        this.cartManager = new CartManager();
    }

    async loadInitialData() {
        try {
            // Load categories if enabled
            if (this.categoryManager) {
                await this.categoryManager.loadCategories();
            }

            // Load initial products
            await this.loadProducts();

            // Start monitoring sort changes
            if (this.sortManager) {
                this.sortManager.startMonitoring();
            }
        } catch (error) {
            console.error('Error loading initial data:', error);
            this.loadingManager.showError(MESSAGES.ERROR);
        }
    }

    async loadProducts() {
        if (this.isLoading) return;

        try {
            this.isLoading = true;
            this.loadingManager.show();

            const result = await ApiService.fetchProducts({
                pageNumber: this.currentPage,
                pageSize: this.itemsPerPage,
                categoryId: this.currentCategory,
                sortBy: this.sortBy
            });

            if (result.succeeded) {
                this.items = result.data?.items || result.data || [];
                this.totalPages = result.data?.totalPages || Math.ceil((result.data?.totalCount || 0) / this.itemsPerPage);
                this.totalProducts = result.data?.totalCount || this.items.length;
                this.min = result.data?.min || 0;
                this.DBMAX = result.data?.dbmax || CONFIG.DBMAX;
                this.max = result.data?.max || CONFIG.DBMAX;

                // Update global variables for backward compatibility
                this.updateGlobalState();

                // Render UI
                this.renderProducts();
                this.pagination.render(this.currentPage, this.totalPages);
                this.productCountDisplay.update(this.currentPage, this.itemsPerPage, this.totalProducts);

                // Dispatch event
                this.dispatchProductsLoadedEvent();
            } else {
                throw new Error(result.message || 'Failed to load products');
            }
        } catch (error) {
            console.error('Error loading products:', error);
            this.loadingManager.showError(MESSAGES.ERROR);
        } finally {
            this.isLoading = false;
        }
    }

    updateGlobalState() {
        window.productsData = this.items;
        window.totalPages = this.totalPages;
        window.currentPage = this.currentPage;
    }

    renderProducts() {
        if (!this.productContainer) return;

        const html = ProductRenderer.renderProductGrid(this.items);
        this.productContainer.innerHTML = html;

        // Apply current view mode
        if (this.viewModeManager) {
            this.viewModeManager.toggleViewMode(this.viewModeManager.getCurrentMode());
        }
    }

    setupGlobalEventListeners() {
        // Listen for custom events
        document.addEventListener('cartUpdated', (e) => {
            console.log('Cart updated:', e.detail);
        });

        document.addEventListener('viewModeChanged', (e) => {
            console.log('View mode changed:', e.detail);
        });
    }

    // Event handlers
    async handlePriceChange(priceRange) {
        this.minPrice = priceRange.min;
        this.maxPrice = priceRange.max;
        this.currentPage = 1;
        await this.loadProducts();
    }

    async handlePageChange(page) {
        this.currentPage = page;
        await this.loadProducts();
        Utils.smoothScrollToTop();
    }

    async handleCategoryChange(categoryId) {
        this.currentCategory = categoryId;
        this.currentPage = 1;
        await this.loadProducts();
    }

    async handleSortChange(sortValue) {
        this.sortBy = sortValue;
        this.currentPage = 1;
        await this.loadProducts();
    }

    dispatchProductsLoadedEvent() {
        const event = new CustomEvent('productsLoaded', {
            detail: {
                items: this.items,
                currentPage: this.currentPage,
                totalPages: this.totalPages,
                totalProducts: this.totalProducts
            }
        });
        document.dispatchEvent(event);
    }

    // Public API
    refresh() {
        return this.loadProducts();
    }

    setPageSize(newSize) {
        this.itemsPerPage = newSize;
        this.currentPage = 1;
        return this.loadProducts();
    }

    goToCategory(categoryId) {
        if (this.categoryManager) {
            return this.handleCategoryChange(categoryId);
        }
    }

    search(query) {
        this.searchQuery = query;
        this.currentPage = 1;
        return this.loadProducts();
    }

    destroy() {
        if (this.sortManager) {
            this.sortManager.destroy();
        }
    }
}