import { SELECTORS } from '../../config/config.js';

export class ViewModeManager {
    constructor() {
        this.isListView = false;
        this.currentMode = 'grid';
        this.setupEventListeners();
    }

    setupEventListeners() {
        const viewModeButtons = document.querySelectorAll(SELECTORS.VIEW_MODE_BUTTONS);
        viewModeButtons.forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                this.setActiveButton(btn, viewModeButtons);
                const target = btn.getAttribute('data-target');
                this.toggleViewMode(target);
            });
        });
    }

    setActiveButton(activeBtn, allButtons) {
        allButtons.forEach(btn => btn.classList.remove('active'));
        activeBtn.classList.add('active');
    }

    toggleViewMode(mode) {
        const productItems = document.querySelectorAll('.product-item');
        const productListItems = document.querySelectorAll('.product-list-item');

        if (mode === 'list-view') {
            this.showListView(productItems, productListItems);
        } else {
            this.showGridView(productItems, productListItems);
        }

        this.currentMode = mode;
        this.dispatchViewModeChangeEvent(mode);
    }

    showListView(gridItems, listItems) {
        gridItems.forEach(item => item.style.display = 'none');
        listItems.forEach(item => item.style.display = 'flex');
        this.isListView = true;
    }

    showGridView(gridItems, listItems) {
        gridItems.forEach(item => item.style.display = 'block');
        listItems.forEach(item => item.style.display = 'none');
        this.isListView = false;
    }

    dispatchViewModeChangeEvent(mode) {
        const event = new CustomEvent('viewModeChanged', {
            detail: { mode, isListView: this.isListView }
        });
        document.dispatchEvent(event);
    }

    getCurrentMode() {
        return this.currentMode;
    }
}
