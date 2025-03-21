function purchaseCourse(courseId) {
    if (!confirm('Вы уверены, что хотите купить этот курс?')) {
        return;
    }

    fetch(`/Course/Purchase/${courseId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        }
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            alert(data.message);
            // После успешной покупки показываем кнопку скачивания
            document.getElementById(`download-btn-${courseId}`).style.display = 'block';
            document.getElementById(`purchase-btn-${courseId}`).style.display = 'none';
        } else {
            alert('Произошла ошибка при покупке курса');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        alert('Произошла ошибка при покупке курса');
    });
}

function downloadCourse(courseId) {
    window.location.href = `/Course/Download/${courseId}`;
}
