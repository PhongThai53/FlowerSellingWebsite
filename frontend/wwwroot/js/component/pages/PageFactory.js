import { ProductPage } from '../pages/ProductPage.js';
import { CONFIG } from '../../config/config.js';

export class PageFactory {
    static createProductPage(options = {}) {
        return new ProductPage(options);
    }

    static createProductListPage(options = {}) {
        return new ProductPage({
            itemsPerPage: 6,
            enableViewMode: true,
            enableSort: true,
            enableCategories: true,
            ...options
        });
    }

    static createSearchPage(searchQuery, options = {}) {
        const page = new ProductPage({
            itemsPerPage: 9,
            enableViewMode: false,
            enableCategories: false,
            ...options
        });
        page.searchQuery = searchQuery;
        return page;
    }

    static createCategoryPage(categoryId, options = {}) {
        const page = new ProductPage({
            enableCategories: false,
            ...options
        });
        page.currentCategory = categoryId;
        return page;
    }

    static createMobilePage(options = {}) {
        return new ProductPage({
            itemsPerPage: 4,
            enableViewMode: false,
            ...options
        });
    }

    static createAdminProductPage(options = {}) {
        return new ProductPage({
            itemsPerPage: 20,
            enableViewMode: false,
            enableSort: true,
            ...options
        });
    }
}