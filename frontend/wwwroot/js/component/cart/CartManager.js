import { ApiService } from '../../services/ApiService.js';
import { MESSAGES } from '../../config/config.js';

export class CartManager {
    constructor() {
        this.cartItems = [];
        this.setupEventListeners();
    }

    setupEventListeners() {
        document.addEventListener('click', (e) => {
            if (e.target.closest('.add-to-cart-btn')) {
                e.preventDefault();
                const btn = e.target.closest('.add-to-cart-btn');
                const productId = btn.getAttribute('data-product-id');
                const quantity = parseInt(btn.getAttribute('data-quantity')) || 1;
                this.addToCart(productId, quantity);
            }
        });
    }

    async addToCart(productId, quantity = 1) {
        try {
            // Show loading state on button
            this.setButtonLoading(productId, true);

            // Call API to add to cart
            const result = await ApiService.addToCart(productId, quantity);

            if (result.succeeded) {
                const product = this.findProductById(productId);
                const productName = product ? product.name : 'Sản phẩm';

                this.showSuccessNotification(`${productName} ${MESSAGES.CART_ADDED}`);
                this.updateCartCount();
                this.dispatchCartUpdateEvent(productId, quantity);
            } else {
                throw new Error(result.message || 'Không thể thêm vào giỏ hàng');
            }
        } catch (error) {
            console.error('Error adding to cart:', error);
            this.showErrorNotification('Có lỗi xảy ra khi thêm vào giỏ hàng');
        } finally {
            this.setButtonLoading(productId, false);
        }
    }

    setButtonLoading(productId, isLoading) {
        const btn = document.querySelector(`[data-product-id="${productId}"]`);
        if (btn) {
            if (isLoading) {
                btn.disabled = true;
                btn.innerHTML = '<i class="fa fa-spinner fa-spin"></i>';
            } else {
                btn.disabled = false;
                btn.innerHTML = '<i class="lnr lnr-cart"></i>';
            }
        }
    }

    findProductById(productId) {
        return window.productsData?.find(item => item.id == productId);
    }

    showSuccessNotification(message) {
        // Có thể thay thế bằng toast notification
        this.createToast(message, 'success');
    }

    showErrorNotification(message) {
        this.createToast(message, 'error');
    }

    createToast(message, type) {
        // Simple toast implementation
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.innerHTML = `
            <div class="toast-content">
                <i class="fa fa-${type === 'success' ? 'check' : 'exclamation-triangle'}"></i>
                <span>${message}</span>
            </div>
        `;

        document.body.appendChild(toast);

        setTimeout(() => {
            toast.classList.add('show');
        }, 100);

        setTimeout(() => {
            toast.remove();
        }, 3000);
    }

    updateCartCount() {
        // Update cart icon count in header
        const cartCountElement = document.querySelector('.cart-count');
        if (cartCountElement) {
            const currentCount = parseInt(cartCountElement.textContent) || 0;
            cartCountElement.textContent = currentCount + 1;
        }
    }

    dispatchCartUpdateEvent(productId, quantity) {
        const event = new CustomEvent('cartUpdated', {
            detail: { productId, quantity, timestamp: Date.now() }
        });
        document.dispatchEvent(event);
    }
}