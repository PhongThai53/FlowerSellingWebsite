// ================================
// js/auto-init.js - Main Entry Point
// ================================


import { ProductPage } from '../component/pages/ProductPage.js';
import { PageFactory } from '../component/pages/PageFactory.js';
import { CONFIG, MESSAGES } from '../../js/config/config.js';
class AutoProductInitializer {
    constructor() {
        this.productPage = null;
        this.isInitialized = false;
    }

    async init() {
        if (this.isInitialized) return;

        try {
            // Đợi DOM ready
            await this.waitForDOM();

            // Tự động phát hiện loại trang
            const pageType = this.detectPageType();

            // Tạo và khởi tạo trang phù hợp
            this.productPage = this.createPage(pageType);
            await this.productPage.init();

            this.isInitialized = true;
            console.log('Product page initialized successfully:', pageType);

            // Dispatch event để thông báo đã ready
            this.dispatchReadyEvent();

        } catch (error) {
            console.error('Failed to initialize product page:', error);
            this.handleInitializationError(error);
        }
    }

    async waitForDOM() {
        // Nếu DOM đã ready
        if (document.readyState === 'complete' || document.readyState === 'interactive') {
            return Promise.resolve();
        }

        // Đợi DOMContentLoaded
        return new Promise(resolve => {
            if (document.readyState === 'loading') {
                document.addEventListener('DOMContentLoaded', resolve);
            } else {
                resolve();
            }
        });
    }

    detectPageType() {
        const url = window.location.href;
        const pathname = window.location.pathname;
        const params = new URLSearchParams(window.location.search);

        // Kiểm tra các element để xác định loại trang
        const hasProductGrid = document.querySelector('.shop-container');
        const hasPagination = document.querySelector('.pagination-box, .pagination-area, .pagination');
        const hasCategories = document.querySelector('.shop-categories, .sidebar-body ul');
        const hasSort = document.querySelector('select[name="sortBy"], .product-short select');

        // Detect based on URL patterns
        if (pathname.includes('search') || params.has('search')) {
            return 'search';
        }

        if (pathname.includes('category') || params.has('categoryId')) {
            return 'category';
        }

        if (pathname.includes('mobile') || window.innerWidth <= 768) {
            return 'mobile';
        }

        if (pathname.includes('admin')) {
            return 'admin';
        }

        // Default product list page
        if (hasProductGrid && hasPagination) {
            return 'product-list';
        }

        return 'default';
    }

    createPage(pageType) {
        const searchParams = new URLSearchParams(window.location.search);

        switch (pageType) {
            case 'search':
                return PageFactory.createSearchPage(searchParams.get('search'));

            case 'category':
                return PageFactory.createCategoryPage(searchParams.get('categoryId'));

            case 'mobile':
                return PageFactory.createMobilePage();

            case 'admin':
                return PageFactory.createAdminProductPage();

            case 'product-list':
                return PageFactory.createProductListPage();

            default:
                return PageFactory.createProductPage();
        }
    }

    dispatchReadyEvent() {
        const event = new CustomEvent('productPageReady', {
            detail: {
                productPage: this.productPage,
                timestamp: Date.now()
            }
        });
        document.dispatchEvent(event);
    }

    handleInitializationError(error) {
        // Hiển thị lỗi trên UI nếu có container
        const container = document.querySelector('.shop-product-wrap, .shop-product-wrapper .row');
        if (container) {
            container.innerHTML = `
                <div class="col-12">
                    <div class="text-center p-5 text-danger">
                        <i class="fa fa-exclamation-triangle fa-2x"></i>
                        <p class="mt-2">Không thể tải trang sản phẩm</p>
                        <button class="btn btn-primary mt-3" onclick="location.reload()">Thử lại</button>
                    </div>
                </div>
            `;
        }

        // Dispatch error event
        const event = new CustomEvent('productPageError', {
            detail: { error, timestamp: Date.now() }
        });
        document.dispatchEvent(event);
    }

    // Public API methods
    getProductPage() {
        return this.productPage;
    }

    refresh() {
        return this.productPage?.refresh();
    }

    destroy() {
        if (this.productPage) {
            this.productPage.destroy();
            this.productPage = null;
        }
        this.isInitialized = false;
    }
}

// ================================
// Global Auto-Initialization
// ================================

// Tạo instance global
const autoInitializer = new AutoProductInitializer();

// Auto-initialize khi script được load
(async function autoInit() {
    try {
        await autoInitializer.init();
    } catch (error) {
        console.error('Auto initialization failed:', error);
    }
})();

// Expose global API
window.ProductPageAPI = {
    // Lấy product page instance
    getProductPage: () => autoInitializer.getProductPage(),

    // Refresh trang
    refresh: () => autoInitializer.refresh(),

    // Reinitialize
    reinit: async () => {
        autoInitializer.destroy();
        await autoInitializer.init();
    },

    // Manual initialization với options
    initWithOptions: async (options) => {
        autoInitializer.destroy();
        const productPage = new ProductPage(options);
        await productPage.init();
        autoInitializer.productPage = productPage;
        autoInitializer.isInitialized = true;
        return productPage;
    }
};

// ================================
// Backward Compatibility & Utilities
// ================================

// Expose utilities globally cho backward compatibility
window.ProductUtils = {
    // Format price
    formatPrice: (price) => {
        const Utils = autoInitializer.productPage?.constructor?.Utils;
        return Utils ? Utils.formatPrice(price) : price;
    },

    // Get product image URL
    getProductImageUrl: (product) => {
        const Utils = autoInitializer.productPage?.constructor?.Utils;
        return Utils ? Utils.getProductImageUrl(product) : '';
    },

    // Smooth scroll
    scrollToTop: () => {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }
};

// ================================
// Event Listeners cho Integration
// ================================

// Listen for ready event
document.addEventListener('productPageReady', (e) => {
    console.log('Product page is ready!', e.detail);

    // Có thể thêm custom logic ở đây
    // Ví dụ: trigger Google Analytics, thông báo cho parent app, etc.
});

// Listen for error event
document.addEventListener('productPageError', (e) => {
    console.error('Product page error:', e.detail.error);

    // Có thể gửi error tracking ở đây
});

// Handle page visibility change
document.addEventListener('visibilitychange', () => {
    if (document.visibilityState === 'visible') {
        // Refresh data khi user quay lại tab
        setTimeout(() => {
            autoInitializer.refresh();
        }, 1000);
    }
});

// Handle back/forward navigation
window.addEventListener('popstate', () => {
    // Reinitialize khi user dùng back/forward button
    setTimeout(() => {
        autoInitializer.reinit();
    }, 100);
});

// ================================
// CSS Injection cho Loading States
// ================================

// Inject CSS cho loading và toast notifications
(function injectCSS() {
    const css = `
        .toast {
            position: fixed;
            top: 20px;
            right: 20px;
            z-index: 9999;
            background: white;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            padding: 16px 20px;
            transform: translateX(400px);
            transition: transform 0.3s ease;
            border-left: 4px solid #28a745;
        }
        
        .toast.toast-error {
            border-left-color: #dc3545;
        }
        
        .toast.show {
            transform: translateX(0);
        }
        
        .toast-content {
            display: flex;
            align-items: center;
            gap: 10px;
        }
        
        .toast-content i {
            color: #28a745;
        }
        
        .toast-error .toast-content i {
            color: #dc3545;
        }
        
        /* Loading spinner enhancement */
        .fa-spinner {
            animation: spin 1s linear infinite;
        }
        
        @keyframes spin {
            from { transform: rotate(0deg); }
            to { transform: rotate(360deg); }
        }
        
        /* Smooth transitions cho product items */
        .product-item, .product-list-item {
            transition: opacity 0.3s ease, transform 0.3s ease;
        }
        
        .product-item:hover, .product-list-item:hover {
            transform: translateY(-2px);
        }
    `;

    const style = document.createElement('style');
    style.textContent = css;
    document.head.appendChild(style);



    document.addEventListener('productPageReady', () => {
        const page = window.ProductPageAPI.getProductPage();
        const sm = page?.sortManager;

        const checkSortReady = () => {
            const select = document.querySelector('select[name="sortBy"]');
            const niceSelect = document.querySelector('.nice-select');

            if (sm && select) {
                console.log('SortManager on ready:', {
                    sortManager: sm,
                    currentSort: sm.currentSort,
                    lastSortValue: sm.lastSortValue,
                    isMonitoring: sm.isMonitoring,
                    selectElement: select,
                    selectValue: select.value,
                    selectDisplay: window.getComputedStyle(select).display,
                    niceSelectRendered: !!niceSelect
                });
            } else {
                // Nếu chưa render, thử lại sau 500ms
                setTimeout(checkSortReady, 500);
            }
        };

        checkSortReady();
    });
})();

// Export for module environments
export { autoInitializer as default, AutoProductInitializer };