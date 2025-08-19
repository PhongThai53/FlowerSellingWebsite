import { Utils } from '../../utils/Utils.js';
import { CONFIG } from '../../config/config.js';

export class PriceRangeManager {
    constructor(container, onPriceChange) {
        this.container = container;
        this.onPriceChange = onPriceChange;
        // Giá mặc định từ DB
        this.dbMin = 0;
        this.dbMax = 650000;
        // Giá hiện tại do người dùng chọn
        this.currentMin = this.dbMin;
        this.currentMax = this.dbMax;
        this.slider = null;
    }

    init(dbMin = 0, dbMax = 1000000) {
        this.dbMin = dbMin;
        this.dbMax = dbMax;
        this.currentMin = this.dbMin;
        this.currentMax = this.dbMax;
        this.initSlider();
        this.setupEventListeners();
        this.updateDisplay();
    }

    initSlider() {
        // Sửa selector - sử dụng class trực tiếp thay vì config
        const sliderElement = this.container?.querySelector('.price-range-slider');

        // Kiểm tra jQuery và jQuery UI có sẵn không
        if (!sliderElement || typeof $ === 'undefined' || !$.ui?.slider) {
            console.warn('jQuery UI slider not available');
            return;
        }

        try {
            $(sliderElement).slider({
                range: true,
                min: this.dbMin,
                max: this.dbMax,
                values: [this.currentMin, this.currentMax],
                slide: (event, ui) => {
                    this.currentMin = ui.values[0];
                    this.currentMax = ui.values[1];
                    this.updateDisplay(); // realtime update UI
                },
                stop: (event, ui) => {
                    this.currentMin = ui.values[0];
                    this.currentMax = ui.values[1];
                    this.triggerPriceChange(); // trigger callback khi thả slider
                }
            });
            this.slider = $(sliderElement);
        } catch (error) {
            console.error('Error initializing price slider:', error);
        }
    }

    setupEventListeners() {
        const filterBtn = this.container?.querySelector('.filter-btn');
        const amountInput = this.container?.querySelector('#amount');

        if (filterBtn) {
            filterBtn.addEventListener('click', (e) => {
                e.preventDefault();
                this.parseManualInput();
                this.triggerPriceChange();
            });
        }

        if (amountInput) {
            amountInput.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    this.parseManualInput();
                    this.triggerPriceChange();
                }
            });
        }
    }

    parseManualInput() {
        const amountInput = this.container?.querySelector('#amount');
        if (!amountInput) return;

        try {
            const inputValue = amountInput.value.trim();
            if (!inputValue) return;

            // Parse input format "min - max" hoặc "min-max"
            const parts = inputValue.split('-').map(s => s.replace(/[^\d]/g, '').trim());

            if (parts.length >= 2) {
                const min = parseInt(parts[0], 10);
                const max = parseInt(parts[1], 10);

                if (!isNaN(min)) {
                    this.currentMin = Math.max(this.dbMin, Math.min(min, this.dbMax));
                }
                if (!isNaN(max)) {
                    this.currentMax = Math.max(this.currentMin, Math.min(max, this.dbMax));
                }
            } else if (parts.length === 1) {
                // Nếu chỉ có 1 giá trị, set làm max
                const value = parseInt(parts[0], 10);
                if (!isNaN(value)) {
                    this.currentMax = Math.max(this.currentMin, Math.min(value, this.dbMax));
                }
            }

            this.updateSliderValues();
            this.updateDisplay();
        } catch (error) {
            console.error('Error parsing manual input:', error);
        }
    }

    updateSliderValues() {
        if (this.slider && this.slider.slider) {
            try {
                this.slider.slider('values', [this.currentMin, this.currentMax]);
            } catch (error) {
                console.error('Error updating slider values:', error);
            }
        }
    }

    updateDisplay() {
        const amountInput = this.container?.querySelector('#amount');
        if (amountInput) {
            try {
                const minFormatted = Utils.formatPrice ? Utils.formatPrice(this.currentMin) : this.currentMin;
                const maxFormatted = Utils.formatPrice ? Utils.formatPrice(this.currentMax) : this.currentMax;
                amountInput.value = `${minFormatted} - ${maxFormatted}`;
            } catch (error) {
                console.error('Error updating display:', error);
                amountInput.value = `${this.currentMin} - ${this.currentMax}`;
            }
        }
    }

    triggerPriceChange() {
        try {
            if (this.onPriceChange && typeof this.onPriceChange === 'function') {
                this.onPriceChange({
                    min: this.currentMin,
                    max: this.currentMax
                });
            }
        } catch (error) {
            console.error('Error triggering price change:', error);
        }
    }

    reset() {
        this.currentMin = this.dbMin;
        this.currentMax = this.dbMax;
        this.updateSliderValues();
        this.updateDisplay();
        this.triggerPriceChange();
    }

    getCurrentRange() {
        return {
            min: this.currentMin,
            max: this.currentMax
        };
    }

    // Thêm method để update range từ bên ngoài
    updateRange(min, max) {
        this.currentMin = Math.max(this.dbMin, Math.min(min, this.dbMax));
        this.currentMax = Math.max(this.currentMin, Math.min(max, this.dbMax));
        this.updateSliderValues();
        this.updateDisplay();
    }

    // Thêm method để set DB range mới
    setDBRange(dbMin, dbMax) {
        this.dbMin = dbMin;
        this.dbMax = dbMax;

        // Reset current values nếu vượt quá range mới
        this.currentMin = Math.max(this.dbMin, this.currentMin);
        this.currentMax = Math.min(this.dbMax, this.currentMax);

        // Reinit slider với range mới
        if (this.slider && this.slider.slider) {
            try {
                this.slider.slider('option', 'min', this.dbMin);
                this.slider.slider('option', 'max', this.dbMax);
                this.slider.slider('values', [this.currentMin, this.currentMax]);
            } catch (error) {
                console.error('Error updating slider range:', error);
            }
        }

        this.updateDisplay();
    }
}