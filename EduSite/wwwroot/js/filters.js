// Функция применения фильтров
function applyFilters(containerSelector) {
    const container = document.querySelector(containerSelector);
    if (!container) {
        console.error('Filter container not found:', containerSelector);
        return;
    }

    try {
        const categorySelect = container.querySelector('.category-filter');
        const priceSelect = container.querySelector('.price-filter');
        const sortSelect = container.querySelector('.sort-filter');
        
        const selectedCategory = categorySelect?.value || '';
        const selectedPrice = priceSelect?.value || '';
        const selectedSort = sortSelect?.value || '';

        // Фильтрация по категории
        let filteredItems = [...window.appState.currentItems];
        if (selectedCategory && selectedCategory !== 'all') {
            const categoryLower = selectedCategory.toLowerCase();
            filteredItems = filteredItems.filter(item => 
                (item.category || '').toLowerCase().includes(categoryLower)
            );
        }

        // Фильтрация по цене
        if (selectedPrice) {
            filteredItems = filteredItems.filter(item => {
                const price = parseFloat(item.price) || 0;
                switch (selectedPrice) {
                    case 'free':
                        return item.isFree === true;
                    case 'paid':
                        return item.isFree === false;
                    case 'under1000':
                        return item.isFree === false && price < 1000;
                    case 'under5000':
                        return item.isFree === false && price < 5000;
                    default:
                        return true;
                }
            });
        }

        // Сортировка
        if (selectedSort) {
            filteredItems.sort((a, b) => {
                switch (selectedSort) {
                    case 'title':
                        return (a.title || '').localeCompare(b.title || '');
                    case 'price-asc':
                        return (parseFloat(a.price) || 0) - (parseFloat(b.price) || 0);
                    case 'price-desc':
                        return (parseFloat(b.price) || 0) - (parseFloat(a.price) || 0);
                    default:
                        return 0;
                }
            });
        }

        // Обновляем отображение
        const itemsContainer = document.querySelector(
            containerSelector === '.courses-filters' ? '#coursesContainer' : '#booksContainer'
        );
        
        if (itemsContainer) {
            const renderFunction = containerSelector === '.courses-filters' ? window.renderCourses : window.renderBooks;
            renderFunction(filteredItems, itemsContainer);

            // Обновляем счетчик результатов
            const counter = container.querySelector('.results-counter');
            if (counter) {
                counter.textContent = `Найдено: ${filteredItems.length}`;
            }

            // Показываем сообщение, если ничего не найдено
            const noResultsMessage = document.querySelector('.no-results-message');
            if (noResultsMessage) {
                if (filteredItems.length === 0) {
                    noResultsMessage.style.display = 'block';
                    noResultsMessage.innerHTML = `
                        <i class="fas fa-search fa-3x text-muted mb-3"></i>
                        <h3>По вашему запросу ничего не найдено</h3>
                        <p class="text-muted">Попробуйте изменить параметры поиска</p>
                    `;
                } else {
                    noResultsMessage.style.display = 'none';
                }
            }
        } else {
            console.error('Items container not found');
        }
    } catch (error) {
        console.error('Error applying filters:', error);
    }
}

function initializeFilters(containerSelector) {
    const container = document.querySelector(containerSelector);
    if (!container) {
        console.error('Container not found:', containerSelector);
        return;
    }

    try {
        // Инициализируем фильтры
        const filters = container.querySelectorAll('select');
        filters.forEach(filter => {
            const newFilter = filter.cloneNode(true);
            filter.parentNode.replaceChild(newFilter, filter);
            
            newFilter.addEventListener('change', () => {
                // Обновляем состояние фильтров
                if (newFilter.classList.contains('category-filter')) {
                    window.appState.currentFilters.category = newFilter.value;
                } else if (newFilter.classList.contains('price-filter')) {
                    window.appState.currentFilters.priceRange = newFilter.value;
                } else if (newFilter.classList.contains('sort-filter')) {
                    window.appState.currentFilters.sortBy = newFilter.value;
                }
                
                // Запускаем поиск с новыми фильтрами
                window.searchItems(containerSelector);
            });
        });

        // Загружаем начальные данные
        window.searchItems(containerSelector);
    } catch (error) {
        console.error('Error initializing filters:', error);
        showErrorMessage(container, 'Произошла ошибка при инициализации фильтров. Пожалуйста, обновите страницу.');
    }
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

// Экспортируем функции
window.applyFilters = applyFilters;
window.initializeFilters = initializeFilters;
