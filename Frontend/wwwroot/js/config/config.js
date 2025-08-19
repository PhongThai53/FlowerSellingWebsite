export const CONFIG = {
    API_BASE_URL: 'https://localhost:7062/api',
    DEFAULT_PAGE_SIZE: 6,
    DEFAULT_IMAGE: 'imgages/product/default-product.jpg',
    CURRENCY: 'VND',
    LOCALE: 'vi-VN',
    DEBOUNCE_DELAY: 300,
    SCROLL_DELAY: 100,
    DBMAX: 650000
};

export const MESSAGES = {
    LOADING: 'Đang tải sản phẩm...',
    ERROR: 'Không thể tải sản phẩm. Vui lòng thử lại sau.',
    NO_PRODUCTS: 'Không có sản phẩm nào.',
    CART_ADDED: 'đã được thêm vào giỏ hàng!',
    CONTACT_PRICE: 'Liên hệ'
};

export const SELECTORS = {
    PRICE_RANCE_SLIDER: '.price-range-slider',
    PRODUCT_CONTAINER: '.shop-container',
    PAGINATION_CONTAINER: '.pagination-box, .paginatoin-area ul, .pagination',
    PRODUCT_AMOUNT: '.product-amount p, .product-amount',
    SORT_SELECT: 'select[name="sortBy"], .product-short select',
    CATEGORIES_CONTAINER: '.shop-categories, .sidebar-body ul',
    VIEW_MODE_BUTTONS: '.product-view-mode a'
};