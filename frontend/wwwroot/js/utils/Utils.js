
import { CONFIG, MESSAGES } from '../config/config.js';
export class Utils {
    static getProductImageUrl(product) {
        if (!product.id) {
            return CONFIG.DEFAULT_IMAGE;
        }
        const baseUrl = CONFIG.API_BASE_URL.replace('/api', '');
        return `${baseUrl}/Image/products/${product.id}/product-${product.id}.jpg`;
    }
    static formatPrice(price) {
        if (!price) return MESSAGES.CONTACT_PRICE;
        return new Intl.NumberFormat(CONFIG.LOCALE, {
            style: 'currency',
            currency: CONFIG.CURRENCY
        }).format(price);
    }
    static debounce(func, wait = CONFIG.DEBOUNCE_DELAY) {
        let timeout;
        return function executedFunction(...arg) {
            const later = () => {
                clearTimeout(timeout);
                func(...arg);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        }
    }
    static smoothScrollToTop() {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }
}