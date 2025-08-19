import { Utils } from '../../utils/Utils.js';
import { CONFIG , MESSAGES } from '../../config/config.js';

export class ProductRenderer {
    static renderProductGrid(items) {
        if (!items || items.length === 0) {
            return '<div class="col-12"><p class="text-center">Không có sản phẩm nào.</p></div>';
        }

        return items.map(item => this.renderProductCard(item)).join('');
    }

    static renderProductCard(item) {
        return `
            <div class="col-md-4 col-sm-6 product-col" style="padding-top:15px" data-product-id="${item.id}">
                <div class="product-item">
                    ${this.renderProductThumb(item)}
                    ${this.renderProductCaption(item)}
                </div>
                ${this.renderListViewItem(item)}
            </div>
        `;
    }

    static renderProductThumb(item) {
        const imageUrl = Utils.getProductImageUrl(item);
        const name = item.name || 'Sản phẩm';
        const id = item.id;
        const badges = this.renderProductBadges(item);
        const actionButtons = this.renderActionButtons(item);

        return `
            <figure class="product-thumb">
                <a href="product-details.html?id=${id}" class="product-link">
                    <img class="pri-img" src="${imageUrl}" alt="${name}" 
                         onerror="this.src='${CONFIG.DEFAULT_IMAGE}'"
                         loading="lazy">
                    <img class="sec-img" src="${imageUrl}" alt="${name}" 
                         onerror="this.src='${CONFIG.DEFAULT_IMAGE}'"
                         loading="lazy">
                </a>
                ${badges}
                ${actionButtons}
            </figure>
        `;
    }

    static renderProductBadges(item) {
        const isNew = item.isNew || false;
        const discountPercent = item.discountPercent || 0;

        if (!isNew && !discountPercent) return '';

        return `
            <div class="product-badge">
                ${isNew ? '<div class="product-label new"><span>mới</span></div>' : ''}
                ${discountPercent > 0 ? `<div class="product-label discount"><span>-${discountPercent}%</span></div>` : ''}
            </div>
        `;
    }

    static renderActionButtons(item) {
        const id = item.id;

        return `
            <div class="button-group">
                <a href="wishlist.html" 
                   data-bs-toggle="tooltip" 
                   data-bs-placement="left" 
                   title="Thêm vào yêu thích"
                   class="wishlist-btn"
                   data-product-id="${id}">
                    <i class="lnr lnr-heart"></i>
                </a>
                <a href="#" 
                   data-bs-toggle="modal" 
                   data-bs-target="#quick_view" 
                   data-product-id="${id}"
                   class="quick-view-btn">
                    <span data-bs-toggle="tooltip" 
                          data-bs-placement="left" 
                          title="Xem nhanh">
                        <i class="lnr lnr-magnifier"></i>
                    </span>
                </a>
                <a href="#" 
                   class="add-to-cart-btn" 
                   data-product-id="${id}" 
                   data-bs-toggle="tooltip" 
                   data-bs-placement="left" 
                   title="Thêm vào giỏ hàng">
                    <i class="lnr lnr-cart"></i>
                </a>
            </div>
        `;
    }

    static renderProductCaption(item) {
        const name = item.name || 'Sản phẩm';
        const id = item.id;
        const priceBox = this.renderPriceBox(item);

        return `
            <div class="product-caption">
                <p class="product-name">
                    <a href="product-details.html?id=${id}">${name}</a>
                </p>
                ${priceBox}
            </div>
        `;
    }

    static renderPriceBox(item) {
        const price = item.price || 0;
        const originalPrice = item.originalPrice || item.oldPrice;

        return `
            <div class="price-box">
                <span class="price-regular">${Utils.formatPrice(price)}</span>
                ${originalPrice && originalPrice > price ?
                `<span class="price-old"><del>${Utils.formatPrice(originalPrice)}</del></span>` : ''
            }
            </div>
        `;
    }

    static renderListViewItem(item) {
        const imageUrl = Utils.getProductImageUrl(item);
        const name = item.name || 'Sản phẩm';
        const description = item.description || 'Sản phẩm chất lượng cao, được chọn lọc kỹ càng.';
        const id = item.id;
        const badges = this.renderProductBadges(item);
        const priceBox = this.renderPriceBox(item);

        return `
            <div class="product-list-item" style="display: none;">
                <figure class="product-thumb">
                    <a href="product-details.html?id=${id}">
                        <img class="pri-img" src="${imageUrl}" alt="${name}" 
                             onerror="this.src='${CONFIG.DEFAULT_IMAGE}'"
                             loading="lazy">
                        <img class="sec-img" src="${imageUrl}" alt="${name}" 
                             onerror="this.src='${CONFIG.DEFAULT_IMAGE}'"
                             loading="lazy">
                    </a>
                    ${badges}
                </figure>
                <div class="product-content-list">
                    <h5 class="product-name">
                        <a href="product-details.html?id=${id}">${name}</a>
                    </h5>
                    ${priceBox}
                    <p class="product-description">${description}</p>
                    ${this.renderListActionButtons(item)}
                </div>
            </div>
        `;
    }

    static renderListActionButtons(item) {
        const id = item.id;

        return `
            <div class="button-group-list">
                <a class="btn-big add-to-cart-btn" 
                   href="#" 
                   data-product-id="${id}" 
                   data-bs-toggle="tooltip" 
                   title="Thêm vào giỏ hàng">
                    <i class="lnr lnr-cart"></i>Thêm vào giỏ
                </a>
                <a href="#" 
                   data-bs-toggle="modal" 
                   data-bs-target="#quick_view" 
                   data-product-id="${id}"
                   class="quick-view-btn">
                    <span data-bs-toggle="tooltip" title="Xem nhanh">
                        <i class="lnr lnr-magnifier"></i>
                    </span>
                </a>
                <a href="wishlist.html" 
                   data-bs-toggle="tooltip" 
                   title="Thêm vào yêu thích"
                   class="wishlist-btn"
                   data-product-id="${id}">
                    <i class="lnr lnr-heart"></i>
                </a>
            </div>
        `;
    }
}