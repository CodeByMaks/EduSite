// Общие переменные для поиска
let searchTimeout = null;

async function searchItems(query = '') {
    try {
        const searchUrl = '/Course/SearchWithFilters';
        const params = new URLSearchParams({ query });
        const url = `${searchUrl}?${params.toString()}`;
        
        const container = document.querySelector('#coursesContainer');
        if (container) {
            container.innerHTML = '<div class="text-center"><div class="spinner-border" role="status"></div></div>';
        }

        const response = await fetch(url);
        if (!response.ok) {
            throw new Error('Ошибка поиска');
        }
        
        const result = await response.json();
        if (!result.success) {
            throw new Error(result.message || 'Ошибка поиска');
        }
        
        renderCourses(result.data, container);
    } catch (error) {
        console.error('Search error:', error);
        showError('Произошла ошибка при поиске курсов');
    }
}

function initializeSearch(searchInputId) {
    const searchInput = document.getElementById(searchInputId);
    if (!searchInput) return;

    searchInput.addEventListener('input', (event) => {
        if (searchTimeout) {
            clearTimeout(searchTimeout);
        }
        
        searchTimeout = setTimeout(() => {
            searchItems(event.target.value);
        }, 300);
    });
}

function showError(message) {
    const container = document.querySelector('#coursesContainer');
    if (!container) return;
    
    container.innerHTML = `
        <div class="alert alert-danger" role="alert">
            ${escapeHtml(message)}
        </div>
    `;
}

function renderCourses(courses, container) {
    if (!container) return;
    
    if (!courses || courses.length === 0) {
        container.innerHTML = '<div class="alert alert-info">Курсы не найдены</div>';
        return;
    }

    const coursesHtml = courses.map(course => `
        <div class="col-md-6 col-lg-4 mb-4">
            <div class="card h-100">
                <img src="${escapeHtml(course.imageUrl || '/images/courses-hero.svg')}" 
                     class="card-img-top" 
                     alt="${escapeHtml(course.title)}"
                     onerror="this.src='/images/courses-hero.svg'">
                <div class="card-body">
                    <h5 class="card-title">${escapeHtml(course.title)}</h5>
                    <p class="card-text">${escapeHtml(course.description)}</p>
                    <div class="d-flex justify-content-between align-items-center">
                        <small class="text-muted">Автор: ${escapeHtml(course.author)}</small>
                        <small class="text-muted">Категория: ${escapeHtml(course.category)}</small>
                    </div>
                    <div class="mt-2">
                        <span class="badge bg-primary">${course.difficulty}</span>
                        <span class="badge bg-info">${course.duration}</span>
                        <span class="badge bg-success">${course.isFree ? 'Бесплатно' : `${course.price} ₽`}</span>
                    </div>
                    <div class="mt-2">
                        <span class="badge bg-secondary">Учащихся: ${course.enrolledCount}</span>
                    </div>
                </div>
                <div class="card-footer">
                    <a href="/Course/Details/${course.id}" class="btn btn-primary w-100">Подробнее</a>
                </div>
            </div>
        </div>
    `).join('');

    container.innerHTML = `<div class="row">${coursesHtml}</div>`;
}

function escapeHtml(unsafe) {
    if (unsafe == null) return '';
    return unsafe
        .toString()
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

window.initializeSearch = initializeSearch;
