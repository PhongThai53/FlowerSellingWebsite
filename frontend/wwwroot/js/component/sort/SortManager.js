import { Utils } from '../../utils/Utils.js';
import { CONFIG, SELECTORS } from '../../config/config.js';

export class SortManager {
    constructor(onSortChange) {
        this.onSortChange = onSortChange;
        this.currentSort = 'default';
        this.lastSortValue = 'default';
        this.isMonitoring = false;
        this.observers = [];
    }

    startMonitoring() {
        if (this.isMonitoring) return;
        this.isMonitoring = true;

        const debouncedHandler = Utils.debounce((value) => {
            this.handleSortChange(value);
        });

        this.setupMultipleListeners(debouncedHandler);
    }

    setupMultipleListeners(handler) {
        this.setupChangeListener(handler);
        this.setupMutationObserver(handler);
        this.setupIntervalCheck(handler);
        this.setupJQueryBackup(handler);
    }

    setupChangeListener(handler) {
        setTimeout(() => {
            const sortSelect = document.querySelector(SELECTORS.SORT_SELECT)
            if (sortSelect) {
                sortSelect.addEventListener('change', (e) => {
                    handler(e.target.value)
                });
            }
        }, 1000);
    }

    setupMutationObserver(handler) {
        const sortSelect = document.querySelector(SELECTORS.SORT_SELECT);
        if (sortSelect) {
            const observer = new MutationObserver(() => {
                if (sortSelect.value !== this.lastSortValue) {
                    this.lastSortValue = sortSelect.value;
                    handler(sortSelect.value);
                }
            });

            observer.observe(sortSelect, {
                attributes: true,
                attributeFilter: ['value'],
                childList: true,
                subtree: true
            });

            this.observers.push(observer);
        }
    }

    setupIntervalCheck(handler) {
        const intervalId = setInterval(() => {
            const sortSelect = document.querySelector(SELECTORS.SORT_SELECT);
            if (sortSelect && sortSelect.value !== this.lastSortValue) {
                this.lastSortValue = sortSelect.value;
                handler(sortSelect.value);
            }
        }, CONFIG.DEBOUNCE_DELAY);

        // Store interval ID for cleanup
        this.intervalId = intervalId;
    }

    setupJQueryBackup(handler) {
        setTimeout(() => {
            if (typeof $ !== 'undefined') {
                $(document).on('change', SELECTORS.SORT_SELECT, function () {
                    handler($(this).val());
                });

                $(document).on('click', '.nice-select .option', function () {
                    setTimeout(() => {
                        const sortSelect = document.querySelector(SELECTORS.SORT_SELECT);
                        if (sortSelect) {
                            handler(sortSelect.value);
                        }
                    }, 50);
                });
            }
        }, 3000);
    }

  

    handleSortChange(newValue) {
        if (newValue !== this.currentSort) {
            this.currentSort = newValue;
            this.onSortChange(newValue);
        }
    }

    forceSort(value) {
        this.currentSort = value;
        this.lastSortValue = value;
        this.onSortChange(value);
    }

    destroy() {
        this.isMonitoring = false;
        this.observers.forEach(observer => observer.disconnect());
        if (this.intervalId) clearInterval(this.intervalId);
    }
}