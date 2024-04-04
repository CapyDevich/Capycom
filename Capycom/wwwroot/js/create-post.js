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