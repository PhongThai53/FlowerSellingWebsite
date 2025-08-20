import { CONFIG } from '../config/config.js';

export class ApiService {
    static async request(endpoint, options = {}) {
        const url = `${CONFIG.API_BASE_URL}${endpoint}`;
        try {
            const response = await fetch(url, {
                headers: {
                    'Content-Type': 'application/json',
                    ...options.headers
                },
                ...options
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            return response.json();
        } catch (error) {
            console.error(`API request failed for ${endpoint}:`, error);
            throw error;
        }
    }

    static async fetchProducts(params = {}) {
        const {
            pageNumber = 1,
            pageSize = CONFIG.DEFAULT_PAGE_SIZE,
            categoryId = '',
            sortBy = '',
            search = '',
            minPrice = null,
            maxPrice = null
        } = params;

        let queryParams = new URLSearchParams({
            pageNumber: pageNumber.toString(),
            pageSize: pageSize.toString(),
        });

        // Add optional parameters
        if (categoryId) queryParams.append('CategoryId', categoryId);
        if (sortBy && sortBy !== 'default') queryParams.append('sortBy', sortBy);
        if (search) queryParams.append('search', search);

        if (minPrice !== null && minPrice !== undefined) {
            queryParams.append('min', minPrice.toString());
        }
        if (maxPrice !== null && maxPrice !== undefined) {
            queryParams.append('max', maxPrice.toString());
        }

        return this.request(`/Product?${queryParams.toString()}`);
    }

    static async fetchCategories() {
        return this.request('/ProductCategory/with-products');
    }

    static async fetchProductById(id) {
        return this.request(`/Product/${id}`);
    }

    static async addToCart(productId, quantity = 1) {
        return this.request('/Cart', {
            method: 'POST',
            body: JSON.stringify({ productId, quantity })
        });
    }

    // ← New: Add method to get price range from server
    static async fetchPriceRange() {
        return this.request('/Product/price-range');
    }
}