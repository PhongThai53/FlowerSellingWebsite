// Dữ liệu giả cho hoa
let flowers = [
    { id: 1, name: 'Hoa Bouquet Hồng', price: 40000, image: '../images/flower1.jpg', description: 'Bó hoa hồng đẹp' },
    { id: 2, name: 'Hoa Jasmine Trắng', price: 50000, image: '../images/flower2.jpg', description: 'Hoa jasmine trắng tinh khôi' },
];

// Dữ liệu giả cho đơn hàng
let orders = [
    { id: 1, customer: 'Nguyễn Văn A', total: 100000, status: 'Chờ xử lý', items: ['Hoa Bouquet Hồng'] },
    { id: 2, customer: 'Trần Thị B', total: 150000, status: 'Đã chấp nhận', items: ['Hoa Jasmine Trắng'] },
];

// Dữ liệu giả cho phản hồi
let feedback = [
    { orderId: 1, rating: 5, comment: 'Hoa rất đẹp!' },
];

// Dữ liệu giả cho báo cáo
let reports = {
    totalSales: 250000,
    topFlower: 'Hoa Bouquet Hồng',
};

// Hiển thị danh sách hoa
function populateFlowerList() {
    const grid = document.querySelector('.grid');
    if (grid) {
        grid.innerHTML = '';
        flowers.forEach(flower => {
            const card = document.createElement('div');
            card.classList.add('card');
            card.innerHTML = `
                <img src="${flower.image}" alt="${flower.name}">
                <h3>${flower.name}</h3>
                <p>${flower.price.toLocaleString('vi-VN')} VNĐ</p>
                <a href="view-flower-detail.html?id=${flower.id}" class="btn">Xem Chi Tiết</a>
                <a href="update-flower-detail.html?id=${flower.id}" class="btn">Cập Nhật</a>
            `;
            grid.appendChild(card);
        });
    }
}

// Thêm hoa (giả lập)
function addFlower(event) {
    event.preventDefault();
    const name = document.getElementById('name').value;
    const price = parseFloat(document.getElementById('price').value);
    const image = document.getElementById('image').value;
    const description = document.getElementById('description').value;
    flowers.push({ id: flowers.length + 1, name, price, image, description });
    alert('Đã thêm hoa thành công!');
    window.location.href = 'view-flower-list.html';
}

// Cập nhật hoa
function updateFlower(event) {
    event.preventDefault();
    const params = new URLSearchParams(window.location.search);
    const id = parseInt(params.get('id'));
    const flower = flowers.find(f => f.id === id);
    if (flower) {
        flower.name = document.getElementById('name').value;
        flower.price = parseFloat(document.getElementById('price').value);
        flower.image = document.getElementById('image').value;
        flower.description = document.getElementById('description').value;
        alert('Đã cập nhật hoa thành công!');
        window.location.href = 'view-flower-list.html';
    }
}

// Hiển thị chi tiết hoa
function populateFlowerDetail() {
    const params = new URLSearchParams(window.location.search);
    const id = parseInt(params.get('id'));
    const flower = flowers.find(f => f.id === id);
    if (flower) {
        document.querySelector('.detail').innerHTML = `
            <img src="${flower.image}" alt="${flower.name}">
            <h2>${flower.name}</h2>
            <p>Giá: ${flower.price.toLocaleString('vi-VN')} VNĐ</p>
            <p>${flower.description}</p>
        `;
    }
}

// Hiển thị danh sách đơn hàng với tìm kiếm
function populateOrderList() {
    const tableBody = document.querySelector('tbody');
    const searchInput = document.getElementById('search-status');
    if (tableBody) {
        tableBody.innerHTML = '';
        let filteredOrders = orders;
        if (searchInput) {
            const status = searchInput.value.toLowerCase();
            filteredOrders = orders.filter(o => o.status.toLowerCase().includes(status));
        }
        filteredOrders.forEach(order => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${order.id}</td>
                <td>${order.customer}</td>
                <td>${order.total.toLocaleString('vi-VN')} VNĐ</td>
                <td>${order.status}</td>
                <td><a href="view-order-information.html?id=${order.id}" class="btn">Xem Thông Tin</a></td>
            `;
            tableBody.appendChild(row);
        });
    }
}

// Xử lý hành động đơn hàng (chấp nhận, hủy, giao)
function handleOrderAction(action) {
    const params = new URLSearchParams(window.location.search);
    const id = parseInt(params.get('id'));
    const order = orders.find(o => o.id === id);
    if (order) {
        if (action === 'accept') order.status = 'Đã chấp nhận';
        else if (action === 'cancel') order.status = 'Đã hủy';
        else if (action === 'deliver') order.status = 'Đã giao';
        alert(`Đơn hàng đã được ${action}!`);
        populateOrderInfo();
    }
}

// Hiển thị thông tin đơn hàng
function populateOrderInfo() {
    const params = new URLSearchParams(window.location.search);
    const id = parseInt(params.get('id'));
    const order = orders.find(o => o.id === id);
    if (order) {
        document.querySelector('.order-detail').innerHTML = `
            <h2>Đơn Hàng #${order.id}</h2>
            <p>Khách hàng: ${order.customer}</p>
            <p>Tổng: ${order.total.toLocaleString('vi-VN')} VNĐ</p>
            <p>Trạng thái: ${order.status}</p>
            <p>Sản phẩm: ${order.items.join(', ')}</p>
            <button onclick="handleOrderAction('accept')" class="btn">Chấp Nhận</button>
            <button onclick="handleOrderAction('cancel')" class="btn">Hủy</button>
            <button onclick="handleOrderAction('deliver')" class="btn">Đánh Dấu Đã Giao</button>
        `;
    }
}

// Hiển thị phản hồi
function populateFeedback() {
    const list = document.querySelector('.feedback-list');
    if (list) {
        list.innerHTML = '';
        feedback.forEach(f => {
            const item = document.createElement('div');
            item.classList.add('card');
            item.innerHTML = `
                <h3>Đơn Hàng #${f.orderId}</h3>
                <p>Đánh giá: ${f.rating}/5</p>
                <p>${f.comment}</p>
            `;
            list.appendChild(item);
        });
    }
}

// Hiển thị báo cáo
function populateReport() {
    const reportDiv = document.querySelector('.report');
    if (reportDiv) {
        reportDiv.innerHTML = `
            <h2>Báo Cáo Doanh Thu</h2>
            <p>Tổng Doanh Thu: ${reports.totalSales.toLocaleString('vi-VN')} VNĐ</p>
            <p>Sản phẩm bán chạy nhất: ${reports.topFlower}</p>
        `;
    }
}

// Gán sự kiện khi trang tải
document.addEventListener('DOMContentLoaded', () => {
    if (window.location.pathname.includes('view-flower-list.html')) populateFlowerList();
    if (window.location.pathname.includes('add-flower.html')) document.querySelector('form').addEventListener('submit', addFlower);
    if (window.location.pathname.includes('update-flower-detail.html')) {
        const params = new URLSearchParams(window.location.search);
        const id = parseInt(params.get('id'));
        const flower = flowers.find(f => f.id === id);
        if (flower) {
            document.getElementById('name').value = flower.name;
            document.getElementById('price').value = flower.price;
            document.getElementById('image').value = flower.image;
            document.getElementById('description').value = flower.description;
        }
        document.querySelector('form').addEventListener('submit', updateFlower);
    }
    if (window.location.pathname.includes('view-flower-detail.html')) populateFlowerDetail();
    if (window.location.pathname.includes('view-order-list.html')) {
        populateOrderList();
        document.getElementById('search-status').addEventListener('input', populateOrderList);
    }
    if (window.location.pathname.includes('view-order-information.html')) populateOrderInfo();
    if (window.location.pathname.includes('view-feedback-order.html')) populateFeedback();
    if (window.location.pathname.includes('view-report.html')) populateReport();
});