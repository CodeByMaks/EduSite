// Общие переменные для поиска и фильтрации
window.appState = {
    currentItems: [],
    searchTimeout: null,
    currentFilters: {
        query: '',
        category: '',
        priceRange: '',
        sortBy: ''
    }
};

async function searchItems(containerType) {
    try {
        const filters = window.appState.currentFilters;
        const searchUrl = containerType === '.courses-filters' ? '/Course/SearchWithFilters' : '/Book/Search';
        
        // Формируем URL с параметрами
        const params = new URLSearchParams({
            query: filters.query || '',
            category: filters.category || '',
            priceRange: filters.priceRange || '',
            sortBy: filters.sortBy || ''
        });

        const response = await fetch(`${searchUrl}?${params.toString()}`);
        if (!response.ok) throw new Error('Network response was not ok');
        
        const data = await response.json();
        window.appState.currentItems = data;
        
        // Обновляем отображение
        const container = document.querySelector(
            containerType === '.courses-filters' ? '#coursesContainer' : '#booksContainer'
        );
        
        if (container) {
            const renderFunction = containerType === '.courses-filters' ? window.renderCourses : window.renderBooks;
            renderFunction(data, container);
            
            // Обновляем счетчик
            const counter = document.querySelector(`${containerType} .results-counter`);
            if (counter) {
                counter.textContent = `Найдено: ${data.length}`;
            }
            
            // Показываем сообщение, если ничего не найдено
            const noResultsMessage = document.querySelector('.no-results-message');
            if (noResultsMessage) {
                if (data.length === 0) {
                    noResultsMessage.style.display = 'block';
                    noResultsMessage.innerHTML = `
                        <div class="text-center my-5">
                            <i class="fas fa-search fa-3x text-muted mb-3"></i>
                            <h3>По вашему запросу ничего не найдено</h3>
                            <p class="text-muted">Попробуйте изменить параметры поиска</p>
                            <button class="btn btn-outline-primary mt-3" onclick="resetFilters('${containerType}')">
                                <i class="fas fa-redo"></i> Сбросить фильтры
                            </button>
                        </div>
                    `;
                } else {
                    noResultsMessage.style.display = 'none';
                }
            }
        }
    } catch (error) {
        console.error('Error searching items:', error);
        showErrorMessage(document.querySelector(containerType), 'Произошла ошибка при поиске. Пожалуйста, попробуйте позже.');
    }
}

function initializeSearch(searchInputId, resultsContainerId, containerType) {
    const searchInput = document.getElementById(searchInputId);
    if (!searchInput) {
        console.error('Search input not found:', searchInputId);
        return;
    }

    searchInput.addEventListener('input', function(e) {
        clearTimeout(window.appState.searchTimeout);
        window.appState.currentFilters.query = e.target.value;
        
        window.appState.searchTimeout = setTimeout(() => {
            searchItems(containerType);
        }, 300);
    });
}

function showErrorMessage(container, message) {
    if (!container) return;
    const errorDiv = document.createElement('div');
    errorDiv.className = 'alert alert-danger';
    errorDiv.role = 'alert';
    errorDiv.innerHTML = `<i class="fas fa-exclamation-circle"></i> ${message}`;
    container.innerHTML = '';
    container.appendChild(errorDiv);
}

function resetFilters(containerType) {
    // Сбрасываем значения фильтров
    window.appState.currentFilters = {
        query: '',
        category: '',
        priceRange: '',
        sortBy: ''
    };
    
    // Сбрасываем значения в элементах формы
    const container = document.querySelector(containerType);
    if (container) {
        const searchInput = container.querySelector('input[type="search"]');
        const selects = container.querySelectorAll('select');
        
        if (searchInput) searchInput.value = '';
        selects.forEach(select => select.value = '');
        
        // Обновляем результаты
        searchItems(containerType);
    }
}

// Функция рендеринга курсов
function renderCourses(courses, container) {
    if (!courses || !container) {
        console.error('Invalid parameters for renderCourses');
        return;
    }
    
    container.innerHTML = courses.length === 0 ? '' : courses.map(course => `
        <div class="col-md-6 col-lg-4 course-item">
            <div class="course-card h-100">
                <div class="course-image">
                    <img src="${course.imageUrl || '/images/default-course.jpg'}" 
                         alt="${escapeHtml(course.title)}" 
                         class="img-fluid"
                         onerror="this.src='/images/default-course.jpg'">
                    <div class="course-overlay">
                        <a href="/Course/Details/${course.id}" class="btn btn-light">
                            <i class="fas fa-info-circle"></i> Подробнее
                        </a>
                    </div>
                    <div class="course-category">
                        <i class="fas fa-bookmark"></i> <span class="category-text">${escapeHtml(course.category || 'Без категории')}</span>
                    </div>
                </div>
                <div class="course-content">
                    <h3 class="course-title">${escapeHtml(course.title)}</h3>
                    <p class="course-description">${escapeHtml(course.description?.length > 100 ? course.description.substring(0, 97) + '...' : course.description || '')}</p>
                    <div class="course-info">
                        <span><i class="fas fa-clock"></i> ${escapeHtml(course.duration || 'Не указано')}</span>
                        <span><i class="fas fa-signal"></i> ${escapeHtml(course.level || 'Любой уровень')}</span>
                        <span class="price"><i class="fas fa-tag"></i> ${course.isFree ? 'Бесплатно' : `${course.price} ₽`}</span>
                    </div>
                    <div class="course-footer">
                        <a href="/Course/Details/${course.id}" class="btn btn-success btn-sm">
                            <i class="fas fa-play-circle"></i> Начать обучение
                        </a>
                    </div>
                </div>
            </div>
        </div>
    `).join('');
}

// Функция рендеринга книг
function renderBooks(books, container) {
    if (!books || !container) {
        console.error('Invalid parameters for renderBooks');
        return;
    }
    
    container.innerHTML = books.length === 0 ? '' : books.map(book => `
        <div class="col-md-6 col-lg-4 book-item">
            <div class="book-card h-100">
                <div class="book-image">
                    <img src="${book.coverImageUrl || '/images/default-book.jpg'}" 
                         alt="${escapeHtml(book.title)}" 
                         class="img-fluid"
                         onerror="this.src='/images/default-book.jpg'">
                    <div class="book-overlay">
                        <a href="/Book/Details/${book.id}" class="btn btn-light">
                            <i class="fas fa-info-circle"></i> Подробнее
                        </a>
                    </div>
                    <div class="book-category">
                        <i class="fas fa-bookmark"></i> <span class="category-text">${escapeHtml(book.category || 'Без категории')}</span>
                    </div>
                </div>
                <div class="book-content">
                    <h3 class="book-title">${escapeHtml(book.title)}</h3>
                    <div class="book-author">
                        <i class="fas fa-user"></i> ${escapeHtml(book.author)}
                    </div>
                    <p class="book-description">${escapeHtml(book.description?.length > 100 ? book.description.substring(0, 97) + '...' : book.description || '')}</p>
                    <div class="book-info">
                        <span><i class="fas fa-calendar-alt"></i> ${book.publicationYear} г.</span>
                        <span><i class="fas fa-file-pdf"></i> ${escapeHtml(book.format || 'PDF')}</span>
                        <span class="price"><i class="fas fa-tag"></i> ${book.isFree ? 'Бесплатно' : `${book.price} ₽`}</span>
                    </div>
                    <div class="book-footer">
                        <a href="/Book/Download/${book.id}" class="btn btn-success btn-sm">
                            <i class="fas fa-download"></i> Скачать
                        </a>
                        <a href="/Book/Details/${book.id}" class="btn btn-outline-success btn-sm">
                            <i class="fas fa-eye"></i> Читать онлайн
                        </a>
                    </div>
                </div>
            </div>
        </div>
    `).join('');
}

// Вспомогательная функция для экранирования HTML
function escapeHtml(unsafe) {
    if (!unsafe) return '';
    return unsafe
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

// Экспортируем функции
window.initializeSearch = initializeSearch;
window.searchItems = searchItems;
window.resetFilters = resetFilters;
window.renderCourses = renderCourses;
window.renderBooks = renderBooks;
