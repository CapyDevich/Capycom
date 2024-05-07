'use strict'
const datetimePicker = document.getElementById('datetimePicker');
const postButton = document.getElementById('postButton');

datetimePicker.addEventListener('input', function () {
    if (datetimePicker.value) {
        postButton.textContent = "Запланировать публикацию";
    }
    else {
        postButton.textContent = "Опубликовать";
    }
});

const commentTextarea = document.getElementById('commentText');
commentTextarea.addEventListener('input', function () {

    if (this.scrollHeight > 200)
        this.style.overflow = 'scroll';
    else {
        this.style.overflow = 'hidden';
        this.style.height = 'auto';
        this.style.height = (this.scrollHeight) + 'px';
    }
});