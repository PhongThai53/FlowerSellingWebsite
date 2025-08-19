import { MESSAGES } from '../../config/config.js';
export class LoadingManager {
    constructor(container) {
        this.container = container;
    }

    show(message = MESSAGES.LOADING) {
        if (!this.container) return;

        this.container.innerHTML = `
            <div class="col-12">
                <div class="text-center p-5">
                    <i class="fa fa-spinner fa-spin fa-2x"></i>
                    <p class="mt-2">${message}</p>
                </div>
            </div>
        `;
    }

    hide() {
        // Loading sẽ được ẩn khi render nội dung mới
    }

    showError(message = MESSAGES.ERROR) {
        if (!this.container) return;

        this.container.innerHTML = `
            <div class="col-12">
                <div class="text-center p-5 text-danger">
                    <i class="fa fa-exclamation-triangle fa-2x"></i>
                    <p class="mt-2">${message}</p>
                    <button class="btn btn-primary mt-3" onclick="location.reload()">Thử lại</button>
                </div>
            </div>
        `;
    }

    showNoResults(message = MESSAGES.NO_PRODUCTS) {
        if (!this.container) return;

        this.container.innerHTML = `
            <div class="col-12">
                <div class="text-center p-5">
                    <i class="fa fa-inbox fa-2x text-muted"></i>
                    <p class="mt-2 text-muted">${message}</p>
                </div>
            </div>
        `;
    }
}