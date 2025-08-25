// API Base URL - thay đổi theo URL API của bạn
const API_BASE_URL = 'https://localhost:7062/api';

// Helper function để lấy URL ảnh đầy đủ
function getProductImageUrl(product) {
    if (!product.id) return 'assets/img/product/default-product.jpg';

    const baseUrl = API_BASE_URL.replace('/api', ''); // https://localhost:7062

    const imageUrl = `/images/products/${product.id}/primary.jpg`;

    return `${baseUrl}${imageUrl}`;
}

// Helper function để format giá tiền VND
function formatPrice(price) {
    if (!price) return 'Liên hệ';
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(price);
}

// Function để fetch danh sách sản phẩm
async function fetchProducts() {
    try {
        const response = await fetch(`${API_BASE_URL}/product?pageSize=12`);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const result = await response.json();
        console.log('API Response:', result);

        // Xử lý response format: result.data.items
        if (result.succeeded && result.data && result.data.items) {
            return result.data.items;
        }

        return result.data?.items || result.items || result.data || result || [];
    } catch (error) {
        console.error('Error fetching products:', error);
        return [];
    }
}

// Function để tạo HTML cho một sản phẩm
function createProductHTML(product) {
    const imageUrl = getProductImageUrl(product);
    const formattedPrice = formatPrice(product.price);

    return `
        <div class="col-lg-3 col-md-4 col-sm-6">
            <div class="product-item mt-40">
                <figure class="product-thumb">
                    <a href="product-details.html?id=${product.id}">
                        <img class="pri-img" src="${imageUrl}" alt="${product.name}" ">
                        <img class="sec-img" src="${imageUrl}" alt="${product.name}" ">
                    </a>
                    <div class="product-badge">
                        <div class="product-label new">
                            <span>Mới</span>
                        </div>
                    </div>
                    <div class="button-group">
                        <a href="wishlist.html" data-bs-toggle="tooltip" data-bs-placement="left" title="Thêm vào yêu thích">
                            <i class="lnr lnr-heart"></i>
                        </a>
                        <a href="#" data-bs-toggle="modal" data-bs-target="#quick_view" onclick="loadQuickView(${product.id})">
                            <span data-bs-toggle="tooltip" data-bs-placement="left" title="Xem nhanh">
                                <i class="lnr lnr-magnifier"></i>
                            </span>
                        </a>
                        <a href="#" data-bs-toggle="tooltip" data-bs-placement="left" title="Thêm vào giỏ hàng" onclick="addToCart(${product.id})">
                            <i class="lnr lnr-cart"></i>
                        </a>
                    </div>
                </figure>
                <div class="product-caption">
                    <p class="product-name">
                        <a href="product-details.html?id=${product.id}">${product.name}</a>
                    </p>
                    <div class="price-box">
                        <span class="price-regular">${formattedPrice}</span>
                    </div>
                </div>
            </div>
        </div>
    `;
}

// Function để hiển thị sản phẩm trong section "New Products"
async function loadNewProducts() {
    const products = await fetchProducts();
    const newProductsContainer = document.querySelector('.our-product .row.mtn-40');

    console.log('Products received:', products);

    if (!newProductsContainer) {
        console.error('New products container not found');
        return;
    }

    if (!products || !Array.isArray(products) || products.length === 0) {
        console.warn('No products available');
        return;
    }

    // Lưu lại nút "view more products" nếu có
    const viewMoreBtn = newProductsContainer.querySelector('.view-more-btn');

    // Xóa nội dung cũ
    newProductsContainer.innerHTML = '';

    // Hiển thị tối đa 8 sản phẩm
    const displayProducts = products.slice(0, 8);

    displayProducts.forEach(product => {
        newProductsContainer.innerHTML += createProductHTML(product);
    });

    // Thêm lại nút "view more products"
    if (viewMoreBtn) {
        newProductsContainer.appendChild(viewMoreBtn);
    } else {
        newProductsContainer.innerHTML += `
            <div class="col-12">
                <div class="view-more-btn">
                    <a class="btn-hero btn-load-more" href="shop.html">Xem thêm sản phẩm</a>
                </div>
            </div>
        `;
    }
}

// Function để tạo HTML cho sản phẩm trending (carousel)
function createTrendingProductHTML(product) {
    const imageUrl = getProductImageUrl(product);
    const formattedPrice = formatPrice(product.price);

    return `
        <div class="product-item">
            <figure class="product-thumb">
                <a href="product-details.html?id=${product.id}">
                    <img class="pri-img" src="${imageUrl}" alt="${product.name}" ">
                    <img class="sec-img" src="${imageUrl}" alt="${product.name}" ">
                </a>
                <div class="product-badge">
                    <div class="product-label new">
                        <span>Hot</span>
                    </div>
                </div>
                <div class="button-group">
                    <a href="wishlist.html" data-bs-toggle="tooltip" data-bs-placement="left" title="Thêm vào yêu thích">
                        <i class="lnr lnr-heart"></i>
                    </a>
                    <a href="#" data-bs-toggle="modal" data-bs-target="#quick_view" onclick="loadQuickView(${product.id})">
                        <span data-bs-toggle="tooltip" data-bs-placement="left" title="Xem nhanh">
                            <i class="lnr lnr-magnifier"></i>
                        </span>
                    </a>
                    <a href="#" data-bs-toggle="tooltip" data-bs-placement="left" title="Thêm vào giỏ hàng" onclick="addToCart(${product.id})">
                        <i class="lnr lnr-cart"></i>
                    </a>
                </div>
            </figure>
            <div class="product-caption">
                <p class="product-name">
                    <a href="product-details.html?id=${product.id}">${product.name}</a>
                </p>
                <div class="price-box">
                    <span class="price-regular">${formattedPrice}</span>
                </div>
            </div>
        </div>
    `;
}

// Function để hiển thị sản phẩm trending
async function loadTrendingProducts() {
    const products = await fetchProducts();
    const trendingContainer = document.querySelector('.top-sellers .product-carousel--4');

    if (!trendingContainer) {
        console.error('Trending products container not found');
        return;
    }

    if (!products || !Array.isArray(products) || products.length === 0) {
        console.warn('No products available for trending');
        return;
    }

    // Xóa nội dung cũ
    trendingContainer.innerHTML = '';

    // Hiển thị tối đa 6 sản phẩm cho trending
    const displayProducts = products.slice(0, 6);

    displayProducts.forEach(product => {
        trendingContainer.innerHTML += createTrendingProductHTML(product);
    });

    // Khởi tạo lại Slick carousel nếu cần
    if (typeof $ !== 'undefined' && $.fn.slick) {
        setTimeout(() => {
            $(trendingContainer).slick('refresh');
        }, 100);
    }
}

// Function để tạo HTML cho deals products
function createDealsProductHTML(product) {
    const imageUrl = getProductImageUrl(product);
    const originalPrice = product.price ? product.price * 1.3 : null;
    const salePrice = formatPrice(product.price);
    const originalPriceHTML = originalPrice ? `<span class="price-old"><del>${formatPrice(originalPrice)}</del></span>` : '';

    return `
        <div class="deal-slide">
            <div class="product-item deal-item">
                <figure class="product-thumb">
                    <a href="product-details.html?id=${product.id}">
                        <img class="pri-img" src="${imageUrl}" alt="${product.name}" ">
                        <img class="sec-img" src="${imageUrl}" alt="${product.name}" ">
                    </a>
                    <div class="product-badge">
                        <div class="product-label sale">
                            <span>Sale</span>
                        </div>
                    </div>
                    <div class="button-group">
                        <a href="wishlist.html" data-bs-toggle="tooltip" data-bs-placement="left" title="Thêm vào yêu thích">
                            <i class="lnr lnr-heart"></i>
                        </a>
                        <a href="#" data-bs-toggle="modal" data-bs-target="#quick_view" onclick="loadQuickView(${product.id})">
                            <span data-bs-toggle="tooltip" data-bs-placement="left" title="Xem nhanh">
                                <i class="lnr lnr-magnifier"></i>
                            </span>
                        </a>
                        <a href="#" data-bs-toggle="tooltip" data-bs-placement="left" title="Thêm vào giỏ hàng" onclick="addToCart(${product.id})">
                            <i class="lnr lnr-cart"></i>
                        </a>
                    </div>
                </figure>
                <div class="product-caption product-deal-content">
                    <p class="product-name">
                        <a href="product-details.html?id=${product.id}">${product.name}</a>
                    </p>
                    <div class="ratings d-flex mb-1">
                        <span><i class="lnr lnr-star"></i></span>
                        <span><i class="lnr lnr-star"></i></span>
                        <span><i class="lnr lnr-star"></i></span>
                        <span><i class="lnr lnr-star"></i></span>
                        <span><i class="lnr lnr-star"></i></span>
                    </div>
                    <div class="price-box">
                        <span class="price-regular">${salePrice}</span>
                        ${originalPriceHTML}
                    </div>
                    <div class="countdown-titmer mt-3">
                        <h5 class="offer-text"><strong class="text-danger">Nhanh tay</strong>! Ưu đãi kết thúc trong:</h5>
                        <div class="product-countdown" data-countdown="2024/12/25"></div>
                    </div>
                </div>
            </div>
        </div>
    `;
}

// Function để hiển thị deals products
async function loadDealsProducts() {
    const products = await fetchProducts();
    const dealsContainer = document.querySelector('.deals-area .product-deal-carousel--2');

    if (!dealsContainer) {
        console.log('Deals products container not found - skipping deals section');
        return;
    }

    if (!products || !Array.isArray(products) || products.length === 0) {
        console.warn('No products available for deals');
        return;
    }

    // Xóa nội dung cũ
    dealsContainer.innerHTML = '';

    // Hiển thị tối đa 3 sản phẩm cho deals (lấy những sản phẩm có giá cao nhất)
    const sortedProducts = [...products].sort((a, b) => (b.price || 0) - (a.price || 0));
    const displayProducts = sortedProducts.slice(0, 3);

    displayProducts.forEach(product => {
        dealsContainer.innerHTML += createDealsProductHTML(product);
    });

    // Khởi tạo lại Slick carousel nếu cần
    if (typeof $ !== 'undefined' && $.fn.slick) {
        setTimeout(() => {
            $(dealsContainer).slick('refresh');
        }, 100);
    }
}

// Function xử lý quick view
function loadQuickView(productId) {
    console.log('Loading quick view for product:', productId);
    // Có thể fetch thêm chi tiết sản phẩm và hiển thị trong modal
    // Implement quick view functionality here
}

// Function xử lý add to cart
function addToCart(productId) {
    console.log('Adding to cart product:', productId);
    // Implement add to cart functionality here
    // Có thể lưu vào localStorage hoặc gửi request lên server
    alert('Đã thêm sản phẩm vào giỏ hàng!');
}

// Function để load tất cả sản phẩm khi trang được tải
async function initializeHomepage() {
    try {
        console.log('Initializing homepage products...');

        // Load các section sản phẩm song song
        await Promise.all([
            loadNewProducts(),
            loadTrendingProducts(),
            loadDealsProducts()
        ]);

        console.log('Homepage products loaded successfully');
    } catch (error) {
        console.error('Error initializing homepage:', error);
    }
}

// Khởi tạo khi DOM đã sẵn sàng
document.addEventListener('DOMContentLoaded', initializeHomepage);

// Export functions nếu cần sử dụng ở file khác
window.FlowerShop = {
    fetchProducts,
    loadNewProducts,
    loadTrendingProducts,
    loadDealsProducts,
    loadQuickView,
    addToCart,
    formatPrice
};