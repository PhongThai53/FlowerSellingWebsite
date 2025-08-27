// Blog Management JavaScript Module
import { API_CONFIG } from "./config/api-config.js";

class BlogManager {
    constructor() {
        this.apiBaseUrl = API_CONFIG.API_URL;
        this.currentPage = 1;
        this.pageSize = 6;
        this.totalPages = 0;
        this.currentBlogId = null;
        this.blogOwnerId = null;
        this.uploadedImages = [];
        this.pendingFiles = [];
        this.quillEditor = null;
        this.loadingTimeout = null;
        this.replyingToCommentId = null;
        
        // Blog status enum
        this.BlogStatus = {
            Draft: 0,
            Pending: 1,
            Published: 2,
            Rejected: 3
        };
        
        this.init();
    }

    init() {
        // Ensure auth manager is available
        if (!window.authManager) {
            console.error('AuthManager not found. Please ensure auth.js is loaded first.');
            return;
        }
    }

    // Initialize Blog List Page
    initializeBlogList() {
        // Debug authentication state
        console.log('BlogList initialization debug:', {
            authManager: window.authManager,
            isAuthenticated: window.authManager?.isAuthenticated(),
            currentUser: window.authManager?.getCurrentUser(),
            token: window.authManager?.getAuthToken() ? 'Present' : 'Missing'
        });
        
        // Debug backend user info
        this.debugBackendUser();
        
        this.loadCategories();
        this.setupFilterEventListeners();
        this.showUserViewInfo();
        this.loadBlogs();
    }

    // Show info about current view mode
    showUserViewInfo() {
        const currentUser = window.authManager?.getCurrentUser();
        const isAdmin = currentUser?.roleName === 'Admin';
        const infoContainer = document.getElementById('view-info-container');
        
        // Debug logging for role detection
        console.log('showUserViewInfo Debug:', {
            currentUser: currentUser,
            roleName: currentUser?.roleName,
            isAdmin: isAdmin,
            availableRoles: ['Admin', 'User', 'Moderator'] // Example roles for comparison
        });
        
        if (infoContainer) {
            if (isAdmin) {
                infoContainer.innerHTML = `
                    <div class="alert alert-info mb-3">
                        <i class="fa fa-shield-alt"></i> <strong>Admin View:</strong> You can see all blogs and manage them.
                        <br><small>Role: ${currentUser?.roleName}</small>
                    </div>
                `;
            } else {
                infoContainer.innerHTML = `
                    <div class="alert alert-success mb-3">
                        <i class="fa fa-user"></i> <strong>User View:</strong> You can see published blogs and your own blogs (all statuses). You can only edit your own blogs.
                        <br><small>Role: ${currentUser?.roleName || 'Not detected'}</small>
                    </div>
                `;
            }
        }
    }

    // Debug backend user information
    async debugBackendUser() {
        try {
            const response = await this.makeAuthenticatedRequest(`${this.apiBaseUrl}/blog/debug/user`);
            if (response && response.ok) {
                const result = await response.json();
                console.log('Backend User Debug:', result);
            } else {
                console.error('Debug endpoint failed:', response?.status, response?.statusText);
            }
        } catch (error) {
            console.error('Debug request error:', error);
        }
    }

    // Initialize Blog Detail Page
    initializeBlogDetail() {
        this.loadCategories();
        this.setupQuillEditor();
        this.setupImageUpload();
        this.setupFormEventListeners();
        
        // Check URL parameters
        const urlParams = new URLSearchParams(window.location.search);
        const blogId = urlParams.get('id');
        const mode = urlParams.get('mode'); // 'edit' or default to 'detail'
        
        if (blogId) {
            this.currentBlogId = parseInt(blogId);
            if (mode === 'edit') {
                this.showEditMode();
                this.loadBlogForEdit(this.currentBlogId);
            } else {
                this.showDetailMode();
                this.loadBlogForDetail(this.currentBlogId);
            }
        } else {
            // New blog creation - always edit mode
            this.showEditMode();
            this.showDefaultActionButtons();
        }
    }

    // Show Detail Mode (read-only)
    showDetailMode() {
        document.getElementById('blog-detail-view').style.display = 'block';
        document.getElementById('blog-form-container').style.display = 'none';
        document.getElementById('page-title').textContent = 'Blog Detail';
    }

    // Show Edit Mode (editable form)
    showEditMode() {
        document.getElementById('blog-detail-view').style.display = 'none';
        document.getElementById('blog-form-container').style.display = 'block';
        const urlParams = new URLSearchParams(window.location.search);
        const blogId = urlParams.get('id');
        document.getElementById('page-title').textContent = blogId ? 'Edit Blog' : 'Create New Blog';
    }

    // Show default action buttons for new blog creation
    showDefaultActionButtons() {
        const saveBtn = document.getElementById('save-btn');
        const saveDraftBtn = document.getElementById('save-draft-btn');
        const submitApprovalBtn = document.getElementById('submit-approval-btn');
        const publishBtn = document.getElementById('publish-btn');

        // Show buttons for new blog creation
        if (saveBtn) {
            saveBtn.style.display = 'inline-block';
        }
        
        if (saveDraftBtn) {
            saveDraftBtn.style.display = 'inline-block';
            saveDraftBtn.innerHTML = '<i class="fa fa-edit"></i> Save as Draft';
            saveDraftBtn.title = 'Save as draft';
        }
        
        if (submitApprovalBtn) {
            submitApprovalBtn.style.display = 'inline-block';
        }
        
        if (publishBtn) {
            publishBtn.style.display = 'inline-block';
        }
    }

    // API Helper Methods
    async makeAuthenticatedRequest(url, options = {}) {
        const token = window.authManager?.getAuthToken();
        
        // Check if this is a FormData request (file upload)
        const isFormData = options.body instanceof FormData;
        
        const defaultOptions = {
            headers: {
                // Only set Content-Type for non-FormData requests
                ...((!isFormData) && { 'Content-Type': 'application/json' }),
                ...(token && { 'Authorization': `Bearer ${token}` })
            }
        };

        const mergedOptions = {
            ...defaultOptions,
            ...options,
            headers: {
                ...defaultOptions.headers,
                ...options.headers
            }
        };

        try {
            const response = await fetch(url, mergedOptions);
            
            if (response.status === 401) {
                // Token expired or invalid
                window.authManager?.clearAuthData();
                window.location.href = '/html/auth/login-register.html';
                return null;
            }
            
            return response;
        } catch (error) {
            console.error('Request failed:', error);
            throw error;
        }
    }

    // Load Categories
    async loadCategories() {
        try {
            const response = await this.makeAuthenticatedRequest(`${this.apiBaseUrl}/category`);
            
            if (response && response.ok) {
                const result = await response.json();
                
                if (result && Array.isArray(result)) {
                    // Category API returns direct array
                    this.populateCategoryDropdowns(result);
                } else if (result.succeeded && result.data) {
                    // Wrapped response
                    this.populateCategoryDropdowns(result.data);
                }
            }
        } catch (error) {
            console.error('Error loading categories:', error);
        }
    }

    populateCategoryDropdowns(categories) {
        const categorySelects = document.querySelectorAll('#category-filter, #blog-category');
        
        categorySelects.forEach(select => {
            // Keep the first option (All Categories or Select Category)
            const firstOption = select.querySelector('option:first-child');
            select.innerHTML = '';
            if (firstOption) {
                select.appendChild(firstOption);
            }
            
            categories.forEach(category => {
                const option = document.createElement('option');
                option.value = category.id;
                option.textContent = category.categoryName;
                select.appendChild(option);
            });
        });
    }

    // Setup Filter Event Listeners
    setupFilterEventListeners() {
        const searchInput = document.getElementById('search-input');
        const statusFilter = document.getElementById('status-filter');
        const categoryFilter = document.getElementById('category-filter');
        const sortBy = document.getElementById('sort-by');
        const sortOrder = document.getElementById('sort-order');
        const clearFilters = document.getElementById('clear-filters');

        // Debounced search
        let searchTimeout;
        if (searchInput) {
            searchInput.addEventListener('input', () => {
                clearTimeout(searchTimeout);
                searchTimeout = setTimeout(() => {
                    this.currentPage = 1;
                    this.loadBlogs();
                }, 500);
            });
        }

        // Filter change events
        [statusFilter, categoryFilter, sortBy, sortOrder].forEach(element => {
            if (element) {
                element.addEventListener('change', () => {
                    this.currentPage = 1;
                    this.loadBlogs();
                });
            }
        });

        // Clear filters
        if (clearFilters) {
            clearFilters.addEventListener('click', () => {
                this.clearFilters();
            });
        }
    }

    clearFilters() {
        document.getElementById('search-input').value = '';
        document.getElementById('status-filter').value = '';
        document.getElementById('category-filter').value = '';
        document.getElementById('sort-by').value = 'CreatedAt';
        document.getElementById('sort-order').value = 'desc';
        this.currentPage = 1;
        this.loadBlogs();
    }

    // Load Blogs with Filters
    async loadBlogs() {
        const container = document.getElementById('blog-list-container');
        const summaryContainer = document.getElementById('results-summary');
        
        // Show loading with debounce to prevent multiple rapid calls
        if (this.loadingTimeout) {
            clearTimeout(this.loadingTimeout);
        }
        
        this.loadingTimeout = setTimeout(() => {
            container.innerHTML = `
                <div class="loading-spinner">
                    <i class="fa fa-spinner fa-spin fa-2x"></i>
                    <p>Loading blogs...</p>
                </div>
            `;
        }, 100); // Small delay to prevent flicker on fast responses

        try {
            const filters = this.getFilters();
            // Convert filters to query string
            const queryParams = new URLSearchParams();
            Object.entries(filters).forEach(([key, value]) => {
                if (value !== null && value !== undefined && value !== '') {
                    queryParams.append(key, value);
                }
            });
            
            // Determine which endpoint to use based on user role
            const currentUser = window.authManager?.getCurrentUser();
            const isAdmin = currentUser?.roleName === 'Admin';
            
            // Debug logging
            console.log('loadBlogs Debug:', {
                currentUser: currentUser,
                roleName: currentUser?.roleName,
                isAdmin: isAdmin,
                filters: filters
            });
            
            const endpoint = isAdmin ? 
                `${this.apiBaseUrl}/blog?${queryParams.toString()}` : 
                `${this.apiBaseUrl}/blog/user-view?${queryParams.toString()}`;
            
            console.log('Using endpoint:', endpoint);
            
            const response = await this.makeAuthenticatedRequest(endpoint, {
                method: 'GET'
            });
            
            console.log('API Response status:', response?.status, response?.statusText);

            // Clear loading timeout since we got a response
            if (this.loadingTimeout) {
                clearTimeout(this.loadingTimeout);
                this.loadingTimeout = null;
            }

            if (response && response.ok) {
                const result = await response.json();
                console.log('API Result:', result);
                
                console.log('Result structure:', {
                    succeeded: result.succeeded,
                    hasData: !!result.data,
                    dataType: typeof result.data,
                    hasBlogs: result.data && Array.isArray(result.data.blogs),
                    blogCount: result.data?.blogs?.length,
                    totalCount: result.data?.totalCount,
                    pageCount: result.data?.pageCount,
                    fullData: result.data
                });
                
                // Check if we have valid data structure
                if (result.succeeded && result.data) {
                    // Ensure data.blogs is an array
                    if (!Array.isArray(result.data.blogs)) {
                        console.error('Invalid data structure: blogs is not an array', result.data);
                        if (typeof result.data.blogs === 'object' && result.data.blogs !== null) {
                            console.log('Converting blogs object to array');
                            result.data.blogs = Object.values(result.data.blogs);
                        } else {
                            console.error('Cannot render blogs - invalid data structure');
                            this.showError(container, 'Invalid data structure: blogs is not an array');
                            return;
                        }
                    }
                    
                    this.renderBlogList(result.data);
                    this.renderPagination(result.data);
                    this.updateResultsSummary(result.data);
                } else {
                    console.error('API Result failed:', result);
                    this.showError(container, result.message || 'Failed to load blogs');
                }
            } else {
                const errorText = response ? await response.text() : 'No response';
                console.error('API Error Response:', {
                    status: response?.status,
                    statusText: response?.statusText,
                    errorText: errorText
                });
                this.showError(container, `Failed to load blogs: ${response?.status || 'Unknown error'}`);
            }
        } catch (error) {
            console.error('Error loading blogs:', error);
            this.showError(container, 'An error occurred while loading blogs');
        }
    }

    getFilters() {
        const search = document.getElementById('search-input')?.value || '';
        const status = document.getElementById('status-filter')?.value || '';
        const categoryId = document.getElementById('category-filter')?.value || '';
        const sortBy = document.getElementById('sort-by')?.value || 'CreatedAt';
        const sortOrder = document.getElementById('sort-order')?.value || 'desc';

        return {
            page: this.currentPage,
            pageSize: this.pageSize,
            search: search,
            status: status ? parseInt(status) : null,
            categoryId: categoryId ? parseInt(categoryId) : null,
            sortBy: sortBy,
            sortOrder: sortOrder
        };
    }

    renderBlogList(data) {
        console.log('renderBlogList called with data:', data);
        const container = document.getElementById('blog-list-container');
        
        if (!data || !data.blogs || data.blogs.length === 0) {
            console.log('No blogs to render - showing empty state');
            container.innerHTML = `
                <div class="empty-state">
                    <i class="fa fa-newspaper fa-3x text-muted"></i>
                    <h4>No blogs found</h4>
                    <p>Try adjusting your filters or create a new blog post.</p>
                    <a href="/html/blog/blog-detail.html" class="btn btn-primary">Create New Blog</a>
                </div>
            `;
            return;
        }

        console.log(`Rendering ${data.blogs.length} blogs`);
        
        try {
            // Use DocumentFragment for better performance
            const fragment = document.createDocumentFragment();
            const rowDiv = document.createElement('div');
            rowDiv.className = 'row';
            
            data.blogs.forEach((blog, index) => {
                console.log(`Rendering blog ${index + 1}/${data.blogs.length}:`, blog.id, blog.title);
                try {
                    const cardElement = document.createElement('div');
                    cardElement.innerHTML = this.createBlogCard(blog);
                    if (cardElement.firstElementChild) {
                        rowDiv.appendChild(cardElement.firstElementChild);
                    } else {
                        console.error('Blog card has no firstElementChild', blog.id, blog.title);
                    }
                } catch (err) {
                    console.error(`Error creating card for blog ${blog.id}:`, err);
                }
            });
            
            fragment.appendChild(rowDiv);
            container.innerHTML = '';
            container.appendChild(fragment);
            console.log('Blog list rendered successfully');
        } catch (error) {
            console.error('Error rendering blog list:', error);
            container.innerHTML = `
                <div class="alert alert-danger">
                    <i class="fa fa-exclamation-triangle"></i> Error rendering blogs: ${error.message}
                </div>
            `;
        }
    }

    createBlogCard(blog) {
        const statusClass = this.getStatusClass(blog.status);
        const statusText = this.getStatusText(blog.status);
        const defaultImage = '/images/blog/blog-large-4.jpg'; // Use existing image
        const hasImage = blog.images && blog.images.length > 0;
        const blogImage = hasImage ? blog.images[0] : defaultImage;
        const tags = blog.tags ? blog.tags.split(',').slice(0, 3) : [];
        const createdDate = new Date(blog.createdAt).toLocaleDateString();
        const currentUser = window.authManager?.getCurrentUser();
        const isCurrentUser = currentUser?.id === blog.userId;
        const isAdmin = currentUser?.roleName === 'Admin';
        
        // Debug logging
        console.log('Blog card permission check:', {
            blogId: blog.id,
            currentUserId: currentUser?.id,
            currentUserRole: currentUser?.roleName,
            blogUserId: blog.userId,
            blogTitle: blog.title,
            isCurrentUser: isCurrentUser,
            isAdmin: isAdmin
        });

        // Create image element with better error handling
        const imageElement = hasImage 
            ? `<img src="${blogImage}" alt="${blog.title}" class="blog-image" loading="lazy" onerror="this.src='${defaultImage}'; this.onerror=null;">`
            : `<div class="blog-image blog-no-image" style="background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); display: flex; align-items: center; justify-content: center; color: white; font-size: 48px;"><i class="fa fa-image"></i></div>`;

        return `
            <div class="col-lg-4 col-md-6">
                <div class="blog-card">
                    <div class="blog-image-container" style="position: relative;">
                        ${imageElement}
                        <div style="position: absolute; top: 10px; right: 10px;">
                            <span class="blog-status status-${statusClass}">${statusText}</span>
                        </div>
                    </div>
                    <div class="blog-content">
                        <div class="blog-meta">
                            <i class="fa fa-user"></i> ${blog.userFullName || blog.userName} • 
                            <i class="fa fa-calendar"></i> ${createdDate} • 
                            <i class="fa fa-folder"></i> ${blog.categoryName} •
                            <i class="fa fa-comments"></i> ${blog.commentCount || 0} comments
                        </div>
                        <h5><a href="/html/blog/blog-detail.html?id=${blog.id}">${blog.title}</a></h5>
                        <div class="blog-tags">
                            ${tags.map(tag => `<span class="blog-tag">${tag.trim()}</span>`).join('')}
                        </div>
                        <div class="action-buttons">
                            <a href="/html/blog/blog-detail.html?id=${blog.id}" class="btn btn-primary btn-sm">
                                <i class="fa fa-eye"></i> View
                            </a>
                            ${isCurrentUser ? `
                                <a href="/html/blog/blog-detail.html?id=${blog.id}&mode=edit" class="btn btn-secondary btn-sm">
                                    <i class="fa fa-edit"></i> Edit
                                </a>
                                <button onclick="blogManager.deleteBlog(${blog.id})" class="btn btn-danger btn-sm">
                                    <i class="fa fa-trash"></i> Delete
                                </button>
                            ` : ''}
                            ${isAdmin && !isCurrentUser ? `
                                <button onclick="blogManager.deleteBlog(${blog.id})" class="btn btn-danger btn-sm">
                                    <i class="fa fa-trash"></i> Admin Delete
                                </button>
                            ` : ''}
                            ${isAdmin && blog.status === this.BlogStatus.Pending ? `
                                <button onclick="blogManager.approveBlog(${blog.id})" class="btn btn-success btn-sm">
                                    <i class="fa fa-check"></i> Approve
                                </button>
                                <button onclick="blogManager.rejectBlog(${blog.id})" class="btn btn-warning btn-sm">
                                    <i class="fa fa-times"></i> Reject
                                </button>
                            ` : ''}
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    getStatusClass(status) {
        switch (status) {
            case this.BlogStatus.Draft: return 'draft';
            case this.BlogStatus.Pending: return 'pending';
            case this.BlogStatus.Published: return 'published';
            case this.BlogStatus.Rejected: return 'rejected';
            default: return 'draft';
        }
    }

    getStatusText(status) {
        switch (status) {
            case this.BlogStatus.Draft: return 'Draft';
            case this.BlogStatus.Pending: return 'Pending';
            case this.BlogStatus.Published: return 'Published';
            case this.BlogStatus.Rejected: return 'Rejected';
            default: return 'Draft';
        }
    }

    renderPagination(data) {
        const container = document.getElementById('pagination-container');
        this.totalPages = data.totalPages;
        
        if (this.totalPages <= 1) {
            container.innerHTML = '';
            return;
        }

        let pagination = '<nav><ul class="pagination">';
        
        // Previous button
        if (data.hasPreviousPage) {
            pagination += `<li class="page-item"><a class="page-link" href="#" onclick="blogManager.goToPage(${this.currentPage - 1})">Previous</a></li>`;
        }

        // Page numbers
        const startPage = Math.max(1, this.currentPage - 2);
        const endPage = Math.min(this.totalPages, this.currentPage + 2);

        for (let i = startPage; i <= endPage; i++) {
            const activeClass = i === this.currentPage ? 'active' : '';
            pagination += `<li class="page-item ${activeClass}"><a class="page-link" href="#" onclick="blogManager.goToPage(${i})">${i}</a></li>`;
        }

        // Next button
        if (data.hasNextPage) {
            pagination += `<li class="page-item"><a class="page-link" href="#" onclick="blogManager.goToPage(${this.currentPage + 1})">Next</a></li>`;
        }

        pagination += '</ul></nav>';
        container.innerHTML = pagination;
    }

    updateResultsSummary(data) {
        const container = document.getElementById('results-summary');
        const start = (this.currentPage - 1) * this.pageSize + 1;
        const end = Math.min(this.currentPage * this.pageSize, data.totalCount);
        
        container.innerHTML = `
            <div class="d-flex justify-content-between align-items-center">
                <span>Showing ${start}-${end} of ${data.totalCount} blogs</span>
                <small class="text-muted">Page ${this.currentPage} of ${data.totalPages}</small>
            </div>
        `;
    }

    goToPage(page) {
        this.currentPage = page;
        this.loadBlogs();
        
        // Scroll to top
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }

    // Setup Quill Editor
    setupQuillEditor() {
        const editorContainer = document.getElementById('quill-editor');
        if (!editorContainer) return;

        this.quillEditor = new Quill('#quill-editor', {
            theme: 'snow',
            modules: {
                toolbar: [
                    [{ 'header': [1, 2, 3, false] }],
                    ['bold', 'italic', 'underline', 'strike'],
                    [{ 'color': [] }, { 'background': [] }],
                    [{ 'list': 'ordered'}, { 'list': 'bullet' }],
                    [{ 'align': [] }],
                    ['link', 'blockquote', 'code-block'],
                    ['clean']
                ]
            }
        });

        // Sync content with hidden textarea
        this.quillEditor.on('text-change', () => {
            const content = this.quillEditor.root.innerHTML;
            document.getElementById('blog-content').value = content;
        });
    }

    // Setup Image Upload
    setupImageUpload() {
        const uploadInput = document.getElementById('image-upload');
        const uploadArea = document.querySelector('.image-upload-area');
        
        if (!uploadInput || !uploadArea) return;

        // Handle file selection
        uploadInput.addEventListener('change', (e) => {
            this.handleImageUpload(e.target.files);
        });

        // Handle drag and drop
        uploadArea.addEventListener('dragover', (e) => {
            e.preventDefault();
            uploadArea.style.borderColor = '#007bff';
        });

        uploadArea.addEventListener('dragleave', (e) => {
            e.preventDefault();
            uploadArea.style.borderColor = '#ddd';
        });

        uploadArea.addEventListener('drop', (e) => {
            e.preventDefault();
            uploadArea.style.borderColor = '#ddd';
            this.handleImageUpload(e.dataTransfer.files);
        });
    }

    async handleImageUpload(files) {
        const container = document.getElementById('image-preview-container');
        let validFiles = [];
        let uploadResults = [];
        
        // Validate files first
        for (let file of files) {
            if (!file.type.startsWith('image/')) {
                this.showMessage('error', `"${file.name}" is not a valid image file`);
                continue;
            }

            if (file.size > 10 * 1024 * 1024) { // 10MB limit
                this.showMessage('error', `"${file.name}" is too large (max 10MB)`);
                continue;
            }

            validFiles.push(file);
        }

        if (validFiles.length === 0) {
            return;
        }

        // Process valid files
        for (let file of validFiles) {
            // Show preview immediately with local URL
            const previewId = 'preview-' + Date.now() + Math.random();
            const preview = this.createImagePreview(file, previewId);
            container.appendChild(preview);

            // Store file for later upload (when blog is saved)
            this.pendingFiles = this.pendingFiles || [];
            this.pendingFiles.push({ file, previewId });
            console.log('Added file to pending:', file.name, 'Total pending:', this.pendingFiles.length);

            // If editing existing blog, upload immediately
            if (this.currentBlogId) {
                const result = await this.uploadFileToServer(file, previewId);
                uploadResults.push({ file: file.name, success: result !== null });
            }
        }

        // Show summary message for immediate uploads
        if (this.currentBlogId && uploadResults.length > 0) {
            const successCount = uploadResults.filter(r => r.success).length;
            const failCount = uploadResults.length - successCount;
            
            if (failCount === 0) {
                this.showMessage('success', `Successfully uploaded ${successCount} image(s)`);
            } else if (successCount === 0) {
                this.showMessage('error', `Failed to upload ${failCount} image(s)`);
            } else {
                this.showMessage('warning', `Uploaded ${successCount} image(s), failed ${failCount}`);
            }
        }
    }

    createImagePreview(file, previewId) {
        const preview = document.createElement('div');
        preview.className = 'image-preview';
        preview.id = previewId;

        const img = document.createElement('img');
        img.src = URL.createObjectURL(file);
        
        const removeBtn = document.createElement('button');
        removeBtn.className = 'image-remove-btn';
        removeBtn.innerHTML = '×';
        removeBtn.onclick = () => this.removeImage(previewId);

        preview.appendChild(img);
        preview.appendChild(removeBtn);

        return preview;
    }

    async uploadFileToServer(file, previewId) {
        try {
            console.log('uploadFileToServer called with:', {
                fileName: file.name,
                fileSize: file.size,
                fileType: file.type,
                currentBlogId: this.currentBlogId,
                previewId: previewId
            });

            const formData = new FormData();
            formData.append('files', file);

            const uploadUrl = `${this.apiBaseUrl}/blog/${this.currentBlogId}/upload-images`;
            console.log('Upload URL:', uploadUrl);

            const response = await this.makeAuthenticatedRequest(
                uploadUrl,
                {
                    method: 'POST',
                    body: formData
                    // No headers needed - makeAuthenticatedRequest will handle it correctly for FormData
                }
            );

            console.log('Upload response status:', response?.status);
            console.log('Upload response headers:', response?.headers);

            if (response && response.ok) {
                const result = await response.json();
                console.log('Upload result:', result);
                
                if (result.succeeded && result.data && Array.isArray(result.data) && result.data.length > 0) {
                    console.log('Upload successful, images:', result.data);
                    // Store full URLs in uploadedImages array
                    const fullUrls = result.data.map(url => this.getFullImageUrl(url));
                    this.uploadedImages.push(...fullUrls);
                    // Update preview with actual URL
                    const img = document.querySelector(`#${previewId} img`);
                    if (img) {
                        const fullImageUrl = this.getFullImageUrl(result.data[0]);
                        img.src = fullImageUrl;
                        console.log('Updated preview image src to:', fullImageUrl);
                    }
                    // Don't show message for individual uploads, will show summary later
                    return result.data[0];
                } else {
                    console.error('Upload succeeded but no data:', result);
                    this.showMessage('error', 'Upload succeeded but no images returned');
                    // Remove failed preview
                    const failedPreview = document.getElementById(previewId);
                    if (failedPreview) {
                        failedPreview.remove();
                    }
                    return null;
                }
            } else {
                // Get detailed error info
                let errorText = 'Unknown error';
                try {
                    const errorResponse = await response.text();
                    console.error('Upload failed with response:', errorResponse);
                    errorText = errorResponse;
                } catch (e) {
                    console.error('Could not read error response');
                }
                
                // Remove failed preview
                const failedPreview = document.getElementById(previewId);
                if (failedPreview) {
                    failedPreview.remove();
                }
                this.showMessage('error', `Failed to upload image: ${errorText}`);
                return null;
            }
        } catch (error) {
            console.error('Upload error:', error);
            const failedPreview = document.getElementById(previewId);
            if (failedPreview) {
                failedPreview.remove();
            }
            this.showMessage('error', 'Failed to upload image');
            return null;
        }
    }

    async uploadPendingFiles() {
        console.log('uploadPendingFiles called, pending files:', this.pendingFiles);
        if (!this.pendingFiles || this.pendingFiles.length === 0) {
            console.log('No pending files to upload');
            return;
        }

        console.log(`Uploading ${this.pendingFiles.length} pending files`);
        let uploadResults = [];
        
        for (const { file, previewId } of this.pendingFiles) {
            console.log('Uploading file:', file.name, 'previewId:', previewId);
            const result = await this.uploadFileToServer(file, previewId);
            console.log('Upload result:', result);
            uploadResults.push({ file: file.name, success: result !== null });
        }
        
        // Show summary message
        if (uploadResults.length > 0) {
            const successCount = uploadResults.filter(r => r.success).length;
            const failCount = uploadResults.length - successCount;
            
            if (failCount === 0) {
                this.showMessage('success', `Successfully uploaded ${successCount} image(s)`);
            } else if (successCount === 0) {
                this.showMessage('error', `Failed to upload ${failCount} image(s)`);
            } else {
                this.showMessage('warning', `Uploaded ${successCount} image(s), failed ${failCount}`);
            }
        }
        
        // Clear pending files after upload
        this.pendingFiles = [];
        console.log('Cleared pending files');
    }

    async removeImage(previewId) {
        const preview = document.getElementById(previewId);
        if (!preview) return;

        const img = preview.querySelector('img');
        
        // Remove from pending files if not yet uploaded (local files)
        if (this.pendingFiles) {
            const pendingItem = this.pendingFiles.find(item => item.previewId === previewId);
            if (pendingItem) {
                // This is a pending file, safe to remove immediately
                this.pendingFiles = this.pendingFiles.filter(item => item.previewId !== previewId);
                preview.remove();
                return;
            }
        }
        
        // For uploaded images (http URLs), need to remove from server first
        if (img && img.src.startsWith('http')) {
            const imageUrl = img.src;
            
            // Call API to remove from server if this is an existing blog
            if (this.currentBlogId) {
                // Show loading state
                const removeBtn = preview.querySelector('.image-remove-btn');
                if (removeBtn) {
                    removeBtn.innerHTML = '<i class="fa fa-spinner fa-spin"></i>';
                    removeBtn.disabled = true;
                }
                
                const success = await this.removeImageFromServer(imageUrl);
                
                if (success) {
                    // Only remove from UI and array if server removal succeeded
                    this.uploadedImages = this.uploadedImages.filter(url => url !== imageUrl);
                    preview.remove();
                } else {
                    // Server removal failed, restore button and don't remove from UI
                    if (removeBtn) {
                        removeBtn.innerHTML = '×';
                        removeBtn.disabled = false;
                    }
                    console.log('Server removal failed, keeping image in UI');
                }
            } else {
                // No blog ID, just remove from UI (shouldn't happen normally)
                this.uploadedImages = this.uploadedImages.filter(url => url !== imageUrl);
                preview.remove();
            }
        } else {
            // Local image (blob URL), safe to remove immediately
            preview.remove();
        }
    }

    async removeImageFromServer(imageUrl) {
        try {
            console.log('Removing image from server:', {
                blogId: this.currentBlogId,
                imageUrl: imageUrl
            });

            const response = await this.makeAuthenticatedRequest(
                `${this.apiBaseUrl}/blog/${this.currentBlogId}/images`,
                {
                    method: 'DELETE',
                    body: JSON.stringify({ imageUrl: imageUrl })
                }
            );

            console.log('Remove image response status:', response?.status);

            if (response && response.ok) {
                const result = await response.json();
                console.log('Remove image result:', result);
                
                if (result.succeeded) {
                    this.showMessage('success', 'Image removed successfully');
                    return true;
                } else {
                    console.error('Remove image failed:', result.message);
                    this.showMessage('error', result.message || 'Failed to remove image');
                    return false;
                }
            } else {
                // Get error details
                let errorText = 'Unknown error';
                try {
                    const errorResponse = await response.text();
                    console.error('Remove image failed with response:', errorResponse);
                    errorText = errorResponse;
                } catch (e) {
                    console.error('Could not read error response');
                }
                
                this.showMessage('error', `Failed to remove image: ${errorText}`);
                return false;
            }
        } catch (error) {
            console.error('Error removing image from server:', error);
            this.showMessage('error', 'Failed to remove image');
            return false;
        }
    }

    // Handle Save as Draft with confirmation for published blogs
    async handleSaveDraft() {
        // If this is a published blog, confirm the action
        if (this.currentBlogId) {
            try {
                const response = await this.makeAuthenticatedRequest(`${this.apiBaseUrl}/blog/${this.currentBlogId}`);
                if (response && response.ok) {
                    const result = await response.json();
                    if (result.succeeded && result.data && result.data.status === this.BlogStatus.Published) {
                        const confirmed = confirm(
                            'This blog is currently published. Saving as draft will:\n' +
                            '• Make it private and no longer visible to public\n' +
                            '• Change its status to Draft\n' +
                            '• Allow you to make further edits\n\n' +
                            'Are you sure you want to continue?'
                        );
                        
                        if (!confirmed) {
                            return;
                        }
                    }
                }
            } catch (error) {
                console.error('Error checking blog status:', error);
            }
        }
        
        // Proceed with saving as draft
        this.saveBlog('draft');
    }

    // Setup Form Event Listeners
    setupFormEventListeners() {
        const saveBtn = document.getElementById('save-btn');
        const saveDraftBtn = document.getElementById('save-draft-btn');
        const submitApprovalBtn = document.getElementById('submit-approval-btn');
        const publishBtn = document.getElementById('publish-btn');

        if (saveBtn) {
            saveBtn.addEventListener('click', () => this.saveBlog('save'));
        }

        if (saveDraftBtn) {
            saveDraftBtn.addEventListener('click', () => this.handleSaveDraft());
        }

        if (submitApprovalBtn) {
            submitApprovalBtn.addEventListener('click', () => this.submitForApproval());
        }

        if (publishBtn) {
            publishBtn.addEventListener('click', () => this.saveBlog('publish'));
        }

        // Comment form
        const commentForm = document.getElementById('comment-form');
        if (commentForm) {
            commentForm.addEventListener('submit', (e) => {
                e.preventDefault();
                this.addComment();
            });
        }
    }

    // Load Blog for Detail (read-only view)
    async loadBlogForDetail(blogId) {
        try {
            this.showLoading(true);
            
            const response = await this.makeAuthenticatedRequest(`${this.apiBaseUrl}/blog/${blogId}`);
            
            if (response && response.ok) {
                const result = await response.json();
                
                if (result.succeeded && result.data) {
                    this.populateBlogDetail(result.data);
                    this.loadComments(blogId);
                } else {
                    this.showMessage('error', result.message || 'Failed to load blog');
                }
            } else {
                this.showMessage('error', 'Failed to load blog');
            }
        } catch (error) {
            console.error('Error loading blog:', error);
            this.showMessage('error', 'An error occurred while loading the blog');
        } finally {
            this.showLoading(false);
        }
    }

    // Load Blog for Edit
    async loadBlogForEdit(blogId) {
        try {
            this.showLoading(true);
            
            const response = await this.makeAuthenticatedRequest(`${this.apiBaseUrl}/blog/${blogId}`);
            
            if (response && response.ok) {
                const result = await response.json();
                
                if (result.succeeded && result.data) {
                    this.populateBlogForm(result.data);
                    this.loadComments(blogId);
                } else {
                    this.showMessage('error', result.message || 'Failed to load blog');
                }
            } else {
                this.showMessage('error', 'Failed to load blog');
            }
        } catch (error) {
            console.error('Error loading blog:', error);
            this.showMessage('error', 'An error occurred while loading the blog');
        } finally {
            this.showLoading(false);
        }
    }

    populateBlogDetail(blog) {
        // Store blog owner ID for permission checks
        this.blogOwnerId = blog.userId;
        
        // Populate detail view elements
        document.getElementById('detail-title').textContent = blog.title;
        document.getElementById('detail-category').textContent = blog.categoryName;
        document.getElementById('detail-author').textContent = blog.userFullName || blog.userName;
        
        const createdDate = new Date(blog.createdAt).toLocaleDateString();
        document.getElementById('detail-date').textContent = createdDate;
        
        // Tags
        const tagsContainer = document.getElementById('detail-tags');
        if (blog.tags) {
            const tags = blog.tags.split(',');
            tagsContainer.innerHTML = tags.map(tag => 
                `<span class="blog-tag">${tag.trim()}</span>`
            ).join('');
        } else {
            tagsContainer.innerHTML = '';
        }
        
        // Content
        document.getElementById('detail-content').innerHTML = blog.content;
        
        // Images
        this.displayDetailImages(blog.images);
        
        // Edit button (if user has permission)
        this.showEditButtonIfAllowed(blog);
        
        // Show status info
        this.displayBlogStatus(blog);
        
        // Show comments section
        document.getElementById('comments-section').style.display = 'block';
    }

    displayDetailImages(images) {
        const container = document.getElementById('detail-images');
        
        if (images && images.length > 0) {
            container.innerHTML = images.map(imageUrl => {
                const fullImageUrl = this.getFullImageUrl(imageUrl);
                return `
                    <div class="detail-image-item mb-2">
                        <img src="${fullImageUrl}" alt="Blog image" class="img-fluid rounded" style="max-width: 100%; height: auto;">
                    </div>
                `;
            }).join('');
        } else {
            container.innerHTML = '';
        }
    }

    showEditButtonIfAllowed(blog) {
        const currentUser = window.authManager?.getCurrentUser();
        const isCurrentUser = currentUser?.id === blog.userId;
        const isAdmin = currentUser?.roleName === 'Admin';
        const editButtonContainer = document.getElementById('detail-edit-button-container');
        
        // Debug logging
        console.log('Permission check:', {
            currentUserId: currentUser?.id,
            blogUserId: blog.userId,
            currentUserRole: currentUser?.roleName,
            isCurrentUser: isCurrentUser,
            isAdmin: isAdmin,
            canEdit: isCurrentUser || isAdmin
        });
        
        if (isCurrentUser || isAdmin) {
            editButtonContainer.innerHTML = `
                <a href="/html/blog/blog-detail.html?id=${blog.id}&mode=edit" class="btn btn-primary">
                    <i class="fa fa-edit"></i> Edit Blog
                </a>
            `;
        } else {
            // Không hiển thị gì cả khi không có quyền edit
            editButtonContainer.innerHTML = '';
        }
    }

    populateBlogForm(blog) {
        // Store blog owner ID for permission checks
        this.blogOwnerId = blog.userId;
        
        // Update page title and breadcrumb
        document.getElementById('page-title').textContent = 'Edit Blog';
        document.getElementById('breadcrumb-current').textContent = 'Edit Blog';

        // Populate form fields
        document.getElementById('blog-id').value = blog.id;
        document.getElementById('blog-title').value = blog.title;
        document.getElementById('blog-category').value = blog.categoryId;
        document.getElementById('blog-tags').value = blog.tags;

        // Set content in Quill editor
        if (this.quillEditor) {
            this.quillEditor.root.innerHTML = blog.content;
            document.getElementById('blog-content').value = blog.content;
        }

        // Show existing images
        this.displayExistingImages(blog.images);

        // Show status info
        this.displayBlogStatus(blog);

        // Show comments section
        document.getElementById('comments-section').style.display = 'block';
    }

    displayExistingImages(images) {
        const container = document.getElementById('image-preview-container');
        
        if (images && images.length > 0) {
            images.forEach((imageUrl, index) => {
                const previewId = 'existing-' + index;
                const preview = document.createElement('div');
                preview.className = 'image-preview';
                preview.id = previewId;

                const img = document.createElement('img');
                // Convert relative path to full URL if needed
                const fullImageUrl = this.getFullImageUrl(imageUrl);
                img.src = fullImageUrl;
                
                const removeBtn = document.createElement('button');
                removeBtn.className = 'image-remove-btn';
                removeBtn.innerHTML = '×';
                removeBtn.onclick = () => this.removeImage(previewId);

                preview.appendChild(img);
                preview.appendChild(removeBtn);
                container.appendChild(preview);
            });

            // Store the full URLs for later use
            this.uploadedImages = images.map(url => this.getFullImageUrl(url));
        }
    }

    getFullImageUrl(imageUrl) {
        // If already a full URL, return as is
        if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) {
            return imageUrl;
        }
        
        // If relative path, prepend base URL
        const baseUrl = this.apiBaseUrl.replace('/api', ''); // Remove /api from base URL
        return `${baseUrl}${imageUrl}`; // imageUrl already starts with /uploads/...
    }

    displayBlogStatus(blog) {
        const statusInfo = document.getElementById('blog-status-info');
        const currentStatus = document.getElementById('current-status');
        const blogMeta = document.getElementById('blog-meta');
        const blogActions = document.getElementById('blog-actions');
        const rejectionContainer = document.getElementById('rejection-reason-container');
        const rejectionText = document.getElementById('rejection-reason-text');

        // Show status info
        statusInfo.style.display = 'block';
        currentStatus.textContent = this.getStatusText(blog.status);
        currentStatus.className = `status-badge status-${this.getStatusClass(blog.status)}`;

        // Show meta info
        const createdDate = new Date(blog.createdAt).toLocaleDateString();
        const updatedDate = blog.updatedAt ? new Date(blog.updatedAt).toLocaleDateString() : null;
        blogMeta.textContent = `Created: ${createdDate}${updatedDate ? ` | Updated: ${updatedDate}` : ''}`;

        // Show rejection reason if applicable
        if (blog.status === this.BlogStatus.Rejected && blog.rejectionReason) {
            rejectionContainer.style.display = 'block';
            rejectionText.textContent = blog.rejectionReason;
        }

        // Configure action buttons based on status and user role
        this.configureActionButtons(blog);
    }

    configureActionButtons(blog) {
        const isCurrentUser = window.authManager?.getCurrentUser()?.id === blog.userId;
        const isAdmin = window.authManager?.getCurrentUser()?.roleName === 'Admin';
        const saveBtn = document.getElementById('save-btn');
        const saveDraftBtn = document.getElementById('save-draft-btn');
        const submitApprovalBtn = document.getElementById('submit-approval-btn');
        const publishBtn = document.getElementById('publish-btn');
        const blogActions = document.getElementById('blog-actions');

        // Hide all buttons initially and reset text
        [saveBtn, saveDraftBtn, submitApprovalBtn, publishBtn].forEach(btn => {
            if (btn) btn.style.display = 'none';
        });
        
        // Reset button text for save draft
        if (saveDraftBtn) {
            saveDraftBtn.innerHTML = '<i class="fa fa-edit"></i> Save as Draft';
            saveDraftBtn.title = 'Save as draft';
        }

        // Clear admin actions
        blogActions.innerHTML = '';

        if (isCurrentUser || isAdmin) {
            // Show appropriate buttons based on status
            switch (blog.status) {
                case this.BlogStatus.Draft:
                case this.BlogStatus.Rejected:
                    if (saveBtn) saveBtn.style.display = 'inline-block';
                    if (saveDraftBtn) saveDraftBtn.style.display = 'inline-block';
                    if (submitApprovalBtn) submitApprovalBtn.style.display = 'inline-block';
                    if (publishBtn) publishBtn.style.display = 'inline-block';
                    break;
                    
                case this.BlogStatus.Pending:
                    // Author can convert back to draft
                    if (isCurrentUser) {
                        if (saveBtn) saveBtn.style.display = 'inline-block';
                        if (saveDraftBtn) saveDraftBtn.style.display = 'inline-block';
                    }
                    
                    if (isAdmin) {
                        blogActions.innerHTML = `
                            <button onclick="blogManager.approveBlog(${blog.id})" class="btn btn-success btn-sm">
                                <i class="fa fa-check"></i> Approve
                            </button>
                            <button onclick="blogManager.rejectBlog(${blog.id})" class="btn btn-warning btn-sm">
                                <i class="fa fa-times"></i> Reject
                            </button>
                        `;
                    }
                    break;
                    
                case this.BlogStatus.Published:
                    if (isCurrentUser) {
                        if (saveBtn) saveBtn.style.display = 'inline-block';
                        if (saveDraftBtn) {
                            saveDraftBtn.style.display = 'inline-block';
                            saveDraftBtn.innerHTML = '<i class="fa fa-edit"></i> Save as Draft';
                            saveDraftBtn.title = 'Save changes and convert to draft (will unpublish)';
                        }
                    }
                    
                    if (isCurrentUser || isAdmin) {
                        blogActions.innerHTML = `
                            <button onclick="blogManager.convertToDraft(${blog.id})" class="btn btn-warning btn-sm">
                                <i class="fa fa-archive"></i> Convert to Draft
                            </button>
                            <button onclick="blogManager.unpublishBlog(${blog.id})" class="btn btn-secondary btn-sm">
                                <i class="fa fa-eye-slash"></i> Unpublish
                            </button>
                        `;
                    }
                    break;
            }
        } else {
            // User without permission - hide edit buttons completely
            const viewOnlyMessage = document.getElementById('blog-actions');
            if (viewOnlyMessage) {
                viewOnlyMessage.innerHTML = ''; // Không hiển thị gì cả
            }
        }
    }

    // Save Blog
    async saveBlog(action = 'draft') {
        const form = document.getElementById('blog-form');
        const formData = new FormData(form);
        
        // Check authentication
        if (!window.authManager?.isAuthenticated()) {
            this.showMessage('error', 'Please login to create/edit blogs');
            window.location.href = '/html/auth/login-register.html';
            return;
        }
        
        // Validate form
        if (!this.validateBlogForm()) {
            return;
        }

        // Prepare blog data
        const blogData = {
            title: formData.get('title'),
            content: document.getElementById('blog-content').value,
            tags: formData.get('tags') || '',
            categoryId: parseInt(formData.get('categoryId'))
        };

        try {
            this.showLoading(true);
            
            // Debug logging
            console.log('Blog data being sent:', blogData);
            
            let response;
            if (this.currentBlogId) {
                // Update existing blog
                response = await this.makeAuthenticatedRequest(
                    `${this.apiBaseUrl}/blog/${this.currentBlogId}`,
                    {
                        method: 'PUT',
                        body: JSON.stringify(blogData)
                    }
                );
            } else {
                // Create new blog
                console.log('Creating new blog with data:', JSON.stringify(blogData));
                response = await this.makeAuthenticatedRequest(
                    `${this.apiBaseUrl}/blog`,
                    {
                        method: 'POST',
                        body: JSON.stringify(blogData)
                    }
                );
            }
            
            console.log('Response status:', response?.status);
            console.log('Response headers:', response?.headers);

            if (response && response.ok) {
                const result = await response.json();
                
                if (result.succeeded && result.data) {
                    this.currentBlogId = result.data.id;
                    
                    // Upload pending files if this was a new blog
                    if (!window.location.search.includes('id=')) {
                        console.log('Uploading pending files:', this.pendingFiles);
                        await this.uploadPendingFiles();
                    }
                    
                    // Handle different actions
                    if (action === 'publish') {
                        await this.publishBlog(this.currentBlogId);
                    } else {
                        this.showMessage('success', 'Blog saved successfully');
                        
                        // Redirect to edit mode if this was a new blog
                        if (!window.location.search.includes('id=')) {
                            window.location.href = `/html/blog/blog-detail.html?id=${this.currentBlogId}`;
                        } else {
                            // Reload blog data to show updated status
                            this.loadBlogForEdit(this.currentBlogId);
                        }
                    }
                } else {
                    this.showMessage('error', result.message || 'Failed to save blog');
                }
            } else {
                // Get error details
                let errorMessage = 'Failed to save blog';
                try {
                    const errorResult = await response.text();
                    console.error('Error response:', errorResult);
                    errorMessage += ` (${response.status}: ${response.statusText})`;
                } catch (e) {
                    console.error('Could not read error response');
                }
                this.showMessage('error', errorMessage);
            }
        } catch (error) {
            console.error('Error saving blog:', error);
            this.showMessage('error', 'An error occurred while saving the blog');
        } finally {
            this.showLoading(false);
        }
    }

    validateBlogForm() {
        const title = document.getElementById('blog-title').value.trim();
        const content = document.getElementById('blog-content').value.trim();
        const categoryId = document.getElementById('blog-category').value;

        if (!title) {
            this.showMessage('error', 'Title is required');
            return false;
        }

        if (title.length > 250) {
            this.showMessage('error', 'Title must be less than 250 characters');
            return false;
        }

        if (!content) {
            this.showMessage('error', 'Content is required');
            return false;
        }

        if (!categoryId) {
            this.showMessage('error', 'Category is required');
            return false;
        }

        return true;
    }

    // Submit for Approval
    async submitForApproval() {
        if (!this.currentBlogId) {
            this.showMessage('error', 'Please save the blog first');
            return;
        }

        try {
            this.showLoading(true);
            
            const response = await this.makeAuthenticatedRequest(
                `${this.apiBaseUrl}/blog/${this.currentBlogId}/submit-for-approval`,
                { method: 'POST' }
            );

            if (response && response.ok) {
                const result = await response.json();
                
                if (result.succeeded) {
                    this.showMessage('success', 'Blog submitted for approval successfully');
                    this.loadBlogForEdit(this.currentBlogId); // Reload to update status
                } else {
                    this.showMessage('error', result.message || 'Failed to submit for approval');
                }
            } else {
                this.showMessage('error', 'Failed to submit for approval');
            }
        } catch (error) {
            console.error('Error submitting for approval:', error);
            this.showMessage('error', 'An error occurred while submitting for approval');
        } finally {
            this.showLoading(false);
        }
    }

    // Publish Blog
    async publishBlog(blogId) {
        try {
            const response = await this.makeAuthenticatedRequest(
                `${this.apiBaseUrl}/blog/${blogId}/publish`,
                { method: 'POST' }
            );

            if (response && response.ok) {
                const result = await response.json();
                
                if (result.succeeded) {
                    this.showMessage('success', 'Blog published successfully');
                    this.loadBlogForEdit(blogId); // Reload to update status
                } else {
                    this.showMessage('error', result.message || 'Failed to publish blog');
                }
            } else {
                this.showMessage('error', 'Failed to publish blog');
            }
        } catch (error) {
            console.error('Error publishing blog:', error);
            this.showMessage('error', 'An error occurred while publishing the blog');
        }
    }

    // Convert Blog to Draft
    async convertToDraft(blogId) {
        if (!confirm('Are you sure you want to convert this blog to draft? This will make it private and editable again.')) {
            return;
        }

        try {
            this.showLoading(true);
            
            const response = await this.makeAuthenticatedRequest(
                `${this.apiBaseUrl}/blog/${blogId}/unpublish`,
                { method: 'POST' }
            );

            if (response && response.ok) {
                const result = await response.json();
                
                if (result.succeeded) {
                    this.showMessage('success', 'Blog converted to draft successfully');
                    if (this.currentBlogId === blogId) {
                        this.loadBlogForEdit(blogId); // Reload to update status
                    } else {
                        this.loadBlogs(); // Reload list
                    }
                } else {
                    this.showMessage('error', result.message || 'Failed to convert blog to draft');
                }
            } else {
                this.showMessage('error', 'Failed to convert blog to draft');
            }
        } catch (error) {
            console.error('Error converting blog to draft:', error);
            this.showMessage('error', 'An error occurred while converting the blog to draft');
        } finally {
            this.showLoading(false);
        }
    }

    // Unpublish Blog
    async unpublishBlog(blogId) {
        if (!confirm('Are you sure you want to unpublish this blog?')) {
            return;
        }

        try {
            this.showLoading(true);
            
            const response = await this.makeAuthenticatedRequest(
                `${this.apiBaseUrl}/blog/${blogId}/unpublish`,
                { method: 'POST' }
            );

            if (response && response.ok) {
                const result = await response.json();
                
                if (result.succeeded) {
                    this.showMessage('success', 'Blog unpublished successfully');
                    if (this.currentBlogId === blogId) {
                        this.loadBlogForEdit(blogId); // Reload to update status
                    } else {
                        this.loadBlogs(); // Reload list
                    }
                } else {
                    this.showMessage('error', result.message || 'Failed to unpublish blog');
                }
            } else {
                this.showMessage('error', 'Failed to unpublish blog');
            }
        } catch (error) {
            console.error('Error unpublishing blog:', error);
            this.showMessage('error', 'An error occurred while unpublishing the blog');
        } finally {
            this.showLoading(false);
        }
    }

    // Approve Blog (Admin only)
    async approveBlog(blogId) {
        if (!confirm('Are you sure you want to approve this blog?')) {
            return;
        }

        try {
            this.showLoading(true);
            
            const response = await this.makeAuthenticatedRequest(
                `${this.apiBaseUrl}/blog/${blogId}/approve`,
                { method: 'POST' }
            );

            if (response && response.ok) {
                const result = await response.json();
                
                if (result.succeeded) {
                    this.showMessage('success', 'Blog approved successfully');
                    if (this.currentBlogId === blogId) {
                        this.loadBlogForEdit(blogId); // Reload to update status
                    } else {
                        this.loadBlogs(); // Reload list
                    }
                } else {
                    this.showMessage('error', result.message || 'Failed to approve blog');
                }
            } else {
                this.showMessage('error', 'Failed to approve blog');
            }
        } catch (error) {
            console.error('Error approving blog:', error);
            this.showMessage('error', 'An error occurred while approving the blog');
        } finally {
            this.showLoading(false);
        }
    }

    // Reject Blog (Admin only)
    async rejectBlog(blogId) {
        const reason = prompt('Please provide a reason for rejection:');
        
        if (!reason || !reason.trim()) {
            this.showMessage('error', 'Rejection reason is required');
            return;
        }

        try {
            this.showLoading(true);
            
            const response = await this.makeAuthenticatedRequest(
                `${this.apiBaseUrl}/blog/${blogId}/reject`,
                {
                    method: 'POST',
                    body: JSON.stringify({ rejectionReason: reason.trim() })
                }
            );

            if (response && response.ok) {
                const result = await response.json();
                
                if (result.succeeded) {
                    this.showMessage('success', 'Blog rejected successfully');
                    if (this.currentBlogId === blogId) {
                        this.loadBlogForEdit(blogId); // Reload to update status
                    } else {
                        this.loadBlogs(); // Reload list
                    }
                } else {
                    this.showMessage('error', result.message || 'Failed to reject blog');
                }
            } else {
                this.showMessage('error', 'Failed to reject blog');
            }
        } catch (error) {
            console.error('Error rejecting blog:', error);
            this.showMessage('error', 'An error occurred while rejecting the blog');
        } finally {
            this.showLoading(false);
        }
    }

    // Delete Blog
    async deleteBlog(blogId) {
        if (!confirm('Are you sure you want to delete this blog? This action cannot be undone.')) {
            return;
        }

        try {
            this.showLoading(true);
            
            const response = await this.makeAuthenticatedRequest(
                `${this.apiBaseUrl}/blog/${blogId}`,
                { method: 'DELETE' }
            );

            if (response && response.ok) {
                const result = await response.json();
                
                if (result.succeeded) {
                    this.showMessage('success', 'Blog deleted successfully');
                    
                    // If we're on the detail page of the deleted blog, redirect to list
                    if (this.currentBlogId === blogId) {
                        window.location.href = '/html/blog/blog-list.html';
                    } else {
                        this.loadBlogs(); // Reload list
                    }
                } else {
                    this.showMessage('error', result.message || 'Failed to delete blog');
                }
            } else {
                this.showMessage('error', 'Failed to delete blog');
            }
        } catch (error) {
            console.error('Error deleting blog:', error);
            this.showMessage('error', 'An error occurred while deleting the blog');
        } finally {
            this.showLoading(false);
        }
    }

    // Load Comments
    async loadComments(blogId) {
        try {
            const response = await this.makeAuthenticatedRequest(`${this.apiBaseUrl}/comment/blog/${blogId}`);
            
            if (response && response.ok) {
                const result = await response.json();
                
                if (result.succeeded && result.data) {
                    this.renderComments(result.data);
                }
            }
        } catch (error) {
            console.error('Error loading comments:', error);
        }
    }

    renderComments(comments) {
        const container = document.getElementById('comments-container');
        
        if (!comments || comments.length === 0) {
            container.innerHTML = '<p class="text-muted">No comments yet.</p>';
            return;
        }

        const commentsHtml = comments.map(comment => this.createCommentHtml(comment)).join('');
        container.innerHTML = commentsHtml;
    }

    createCommentHtml(comment) {
        const createdDate = new Date(comment.createdAt).toLocaleDateString();
        const currentUser = window.authManager?.getCurrentUser();
        const isCurrentUser = currentUser?.id === comment.userId;
        const isAdmin = currentUser?.roleName === 'Admin';
        const isBlogOwner = currentUser?.id === this.blogOwnerId; // Blog owner ID
        const canSeeHidden = isAdmin || isBlogOwner;
        
        // Skip hidden comments for regular users
        if (comment.isHide && !canSeeHidden) {
            return '';
        }

        return `
            <div class="comment-item ${comment.isHide ? 'opacity-50' : ''}">
                <div class="comment-meta">
                    <strong>${comment.userFullName || comment.userName}</strong> • ${createdDate}
                    ${comment.isHide ? '<span class="badge badge-warning">Hidden</span>' : ''}
                </div>
                <div class="comment-content">${comment.content}</div>
                ${comment.children && comment.children.length > 0 ? `
                    <div class="ml-4 mt-3">
                        ${comment.children.map(child => this.createCommentHtml(child)).join('')}
                    </div>
                ` : ''}
                <div class="comment-actions">
                    ${isCurrentUser ? `
                        <button onclick="blogManager.editComment(${comment.id})" class="btn btn-sm btn-secondary">
                            <i class="fa fa-edit"></i> Edit
                        </button>
                        <button onclick="blogManager.deleteComment(${comment.id})" class="btn btn-sm btn-danger">
                            <i class="fa fa-trash"></i> Delete
                        </button>
                    ` : ''}
                    ${canSeeHidden && !comment.isHide ? `
                        <button onclick="blogManager.hideComment(${comment.id})" class="btn btn-sm btn-warning">
                            <i class="fa fa-eye-slash"></i> Hide
                        </button>
                    ` : ''}
                    ${canSeeHidden && comment.isHide ? `
                        <button onclick="blogManager.showComment(${comment.id})" class="btn btn-sm btn-info">
                            <i class="fa fa-eye"></i> Show
                        </button>
                    ` : ''}
                    <button onclick="blogManager.replyToComment(${comment.id})" class="btn btn-sm btn-primary">
                        <i class="fa fa-reply"></i> Reply
                    </button>
                </div>
            </div>
        `;
    }

    // Reply to a comment
    replyToComment(commentId) {
        // Store the comment ID we're replying to
        this.replyingToCommentId = commentId;
        
        // Scroll to comment form
        const commentForm = document.getElementById('comment-form');
        if (commentForm) {
            commentForm.scrollIntoView({ behavior: 'smooth' });
            
            // Focus on the comment textarea
            const commentTextarea = document.getElementById('comment-content');
            if (commentTextarea) {
                commentTextarea.focus();
                
                // Show reply indicator
                const replyIndicator = document.getElementById('reply-indicator') || this.createReplyIndicator();
                if (replyIndicator) {
                    // Find the comment we're replying to
                    const allComments = document.querySelectorAll('.comment-item');
                    let commentAuthor = 'this comment';
                    
                    allComments.forEach(item => {
                        if (item.querySelector(`button[onclick*="replyToComment(${commentId})"]`)) {
                            const authorElement = item.querySelector('.comment-meta strong');
                            if (authorElement) {
                                commentAuthor = authorElement.textContent;
                            }
                        }
                    });
                    
                    replyIndicator.innerHTML = `
                        <div class="alert alert-info">
                            <div class="d-flex justify-content-between align-items-center">
                                <span>Replying to ${commentAuthor}</span>
                                <button type="button" class="btn btn-sm btn-light" onclick="blogManager.cancelReply()">
                                    <i class="fa fa-times"></i> Cancel
                                </button>
                            </div>
                        </div>
                    `;
                    replyIndicator.style.display = 'block';
                }
            }
        }
    }
    
    // Create reply indicator element if it doesn't exist
    createReplyIndicator() {
        const commentForm = document.getElementById('comment-form');
        if (commentForm) {
            const replyIndicator = document.createElement('div');
            replyIndicator.id = 'reply-indicator';
            replyIndicator.style.marginBottom = '15px';
            
            // Insert before the textarea or as the first child
            const textarea = commentForm.querySelector('textarea');
            if (textarea) {
                textarea.parentNode.insertBefore(replyIndicator, textarea);
            } else {
                commentForm.insertBefore(replyIndicator, commentForm.firstChild);
            }
            
            return replyIndicator;
        }
        return null;
    }
    
    // Cancel reply
    cancelReply() {
        this.replyingToCommentId = null;
        
        // Hide reply indicator
        const replyIndicator = document.getElementById('reply-indicator');
        if (replyIndicator) {
            replyIndicator.style.display = 'none';
        }
    }
    
    // Edit comment
    async editComment(commentId) {
        try {
            // Get the comment from API
            const response = await this.makeAuthenticatedRequest(`${this.apiBaseUrl}/comment/${commentId}`);
            
            if (response && response.ok) {
                const result = await response.json();
                
                if (result.succeeded && result.data) {
                    const comment = result.data;
                    
                    // Find the comment element
                    const commentElement = document.querySelector(`.comment-item button[onclick*="editComment(${commentId})"]`)?.closest('.comment-item');
                    if (!commentElement) {
                        this.showMessage('error', 'Could not find comment element');
                        return;
                    }
                    
                    // Create edit form
                    const contentElement = commentElement.querySelector('.comment-content');
                    const originalContent = contentElement.innerHTML;
                    
                    // Replace content with edit form
                    contentElement.innerHTML = `
                        <div class="edit-comment-form">
                            <textarea class="form-control" rows="3">${comment.content}</textarea>
                            <div class="mt-2">
                                <button class="btn btn-sm btn-primary save-edit-btn">
                                    <i class="fa fa-save"></i> Save
                                </button>
                                <button class="btn btn-sm btn-secondary cancel-edit-btn">
                                    <i class="fa fa-times"></i> Cancel
                                </button>
                            </div>
                        </div>
                    `;
                    
                    // Add event listeners
                    const saveBtn = contentElement.querySelector('.save-edit-btn');
                    const cancelBtn = contentElement.querySelector('.cancel-edit-btn');
                    const textarea = contentElement.querySelector('textarea');
                    
                    // Focus on textarea
                    textarea.focus();
                    
                    // Save button
                    saveBtn.addEventListener('click', async () => {
                        const newContent = textarea.value.trim();
                        if (!newContent) {
                            this.showMessage('error', 'Comment cannot be empty');
                            return;
                        }
                        
                        await this.saveCommentEdit(commentId, newContent, contentElement, originalContent);
                    });
                    
                    // Cancel button
                    cancelBtn.addEventListener('click', () => {
                        contentElement.innerHTML = originalContent;
                    });
                    
                    // Hide action buttons while editing
                    const actionButtons = commentElement.querySelector('.comment-actions');
                    if (actionButtons) {
                        actionButtons.style.display = 'none';
                    }
                } else {
                    this.showMessage('error', result.message || 'Failed to get comment');
                }
            } else {
                this.showMessage('error', 'Failed to get comment');
            }
        } catch (error) {
            console.error('Error editing comment:', error);
            this.showMessage('error', 'An error occurred while editing comment');
        }
    }
    
    // Save comment edit
    async saveCommentEdit(commentId, content, contentElement, originalContent) {
        try {
            const response = await this.makeAuthenticatedRequest(
                `${this.apiBaseUrl}/comment/${commentId}`,
                {
                    method: 'PUT',
                    body: JSON.stringify({
                        content: content
                    })
                }
            );
            
            if (response && response.ok) {
                const result = await response.json();
                
                if (result.succeeded) {
                    // Update content
                    contentElement.innerHTML = content;
                    
                    // Show action buttons again
                    const commentElement = contentElement.closest('.comment-item');
                    const actionButtons = commentElement.querySelector('.comment-actions');
                    if (actionButtons) {
                        actionButtons.style.display = '';
                    }
                    
                    this.showMessage('success', 'Comment updated successfully');
                } else {
                    // Restore original content on error
                    contentElement.innerHTML = originalContent;
                    this.showMessage('error', result.message || 'Failed to update comment');
                }
            } else {
                // Restore original content on error
                contentElement.innerHTML = originalContent;
                this.showMessage('error', 'Failed to update comment');
            }
        } catch (error) {
            console.error('Error updating comment:', error);
            // Restore original content on error
            contentElement.innerHTML = originalContent;
            this.showMessage('error', 'An error occurred while updating comment');
        }
    }

    // Add Comment
    async addComment() {
        const content = document.getElementById('comment-content').value.trim();
        
        if (!content) {
            this.showMessage('error', 'Comment content is required');
            return;
        }

        if (!this.currentBlogId) {
            this.showMessage('error', 'Blog ID not found');
            return;
        }

        try {
            const response = await this.makeAuthenticatedRequest(
                `${this.apiBaseUrl}/comment`,
                {
                    method: 'POST',
                    body: JSON.stringify({
                        content: content,
                        blogId: this.currentBlogId,
                        parentId: this.replyingToCommentId || null
                    })
                }
            );

            if (response && response.ok) {
                const result = await response.json();
                
                if (result.succeeded) {
                    this.showMessage('success', 'Comment added successfully');
                    document.getElementById('comment-content').value = '';
                    
                    // Reset reply state
                    this.cancelReply();
                    
                    this.loadComments(this.currentBlogId); // Reload comments
                } else {
                    this.showMessage('error', result.message || 'Failed to add comment');
                }
            } else {
                this.showMessage('error', 'Failed to add comment');
            }
        } catch (error) {
            console.error('Error adding comment:', error);
            this.showMessage('error', 'An error occurred while adding the comment');
        }
    }

    // Hide Comment
    async hideComment(commentId) {
        try {
            const response = await this.makeAuthenticatedRequest(
                `${this.apiBaseUrl}/comment/${commentId}/hide`,
                { method: 'POST' }
            );

            if (response && response.ok) {
                const result = await response.json();
                
                if (result.succeeded) {
                    this.showMessage('success', 'Comment hidden successfully');
                    this.loadComments(this.currentBlogId);
                } else {
                    this.showMessage('error', result.message || 'Failed to hide comment');
                }
            }
        } catch (error) {
            console.error('Error hiding comment:', error);
            this.showMessage('error', 'An error occurred while hiding the comment');
        }
    }

    // Show Comment
    async showComment(commentId) {
        try {
            const response = await this.makeAuthenticatedRequest(
                `${this.apiBaseUrl}/comment/${commentId}/show`,
                { method: 'POST' }
            );

            if (response && response.ok) {
                const result = await response.json();
                
                if (result.succeeded) {
                    this.showMessage('success', 'Comment shown successfully');
                    this.loadComments(this.currentBlogId);
                } else {
                    this.showMessage('error', result.message || 'Failed to show comment');
                }
            }
        } catch (error) {
            console.error('Error showing comment:', error);
            this.showMessage('error', 'An error occurred while showing the comment');
        }
    }

    // Delete Comment
    async deleteComment(commentId) {
        if (!confirm('Are you sure you want to delete this comment?')) {
            return;
        }

        try {
            const response = await this.makeAuthenticatedRequest(
                `${this.apiBaseUrl}/comment/${commentId}`,
                { method: 'DELETE' }
            );

            if (response && response.ok) {
                const result = await response.json();
                
                if (result.succeeded) {
                    this.showMessage('success', 'Comment deleted successfully');
                    this.loadComments(this.currentBlogId);
                } else {
                    this.showMessage('error', result.message || 'Failed to delete comment');
                }
            }
        } catch (error) {
            console.error('Error deleting comment:', error);
            this.showMessage('error', 'An error occurred while deleting the comment');
        }
    }

    // Utility Methods
    showLoading(show) {
        const overlay = document.getElementById('loading-overlay');
        if (overlay) {
            overlay.style.display = show ? 'flex' : 'none';
        }
    }

    showMessage(type, message) {
        const container = document.getElementById('form-response');
        if (!container) return;

        let alertClass = 'alert-info'; // default
        switch(type) {
            case 'success':
                alertClass = 'alert-success';
                break;
            case 'error':
                alertClass = 'alert-danger';
                break;
            case 'warning':
                alertClass = 'alert-warning';
                break;
            case 'info':
                alertClass = 'alert-info';
                break;
        }
        
        container.innerHTML = `<div class="alert ${alertClass}">${message}</div>`;
        container.style.display = 'block';

        // Auto hide after 5 seconds
        setTimeout(() => {
            container.style.display = 'none';
        }, 5000);

        // Scroll to message
        container.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }

    showError(container, message) {
        container.innerHTML = `
            <div class="empty-state">
                <i class="fa fa-exclamation-triangle fa-3x text-danger"></i>
                <h4>Error</h4>
                <p>${message}</p>
                <button onclick="location.reload()" class="btn btn-primary">Retry</button>
            </div>
        `;
    }
}

// Initialize blog manager when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    window.blogManager = new BlogManager();
});

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = BlogManager;
}
